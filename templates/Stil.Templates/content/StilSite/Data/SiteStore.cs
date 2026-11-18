using Microsoft.Data.Sqlite;
using Stil.Blazor.Blocks;

namespace StilWeb.Data;

/// <summary>
/// The site's pages as data: each page is Stil's page JSON (StilPageJson) in a database, so the
/// content can change without a new build. Page.razor renders them; an editor of your own can save
/// them with SavePageAsync.
/// </summary>
public interface ISiteStore
{
    /// <summary>The site, with its theme and all its pages.</summary>
    Task<StilSiteData> GetSiteAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a page, or replaces the one with its path.</summary>
    Task SavePageAsync(StilPageData page, CancellationToken cancellationToken = default);
}

/// <summary>The site in a SQLite file: a row with the site's settings and a row per page.</summary>
public sealed class SqliteSiteStore(string connectionString) : ISiteStore
{
    /// <summary>Creates the tables, and fills them from the seed file when the database is new.</summary>
    public async Task EnsureCreatedAsync(string seedFile, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS site (id INTEGER PRIMARY KEY CHECK (id = 1), json TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS pages (path TEXT PRIMARY KEY, json TEXT NOT NULL);
            """, cancellationToken);

        var exists = connection.CreateCommand();
        exists.CommandText = "SELECT COUNT(*) FROM site";
        if ((long)(await exists.ExecuteScalarAsync(cancellationToken))! > 0)
        {
            return;
        }

        var seed = StilPageJson.DeserializeSite(await File.ReadAllTextAsync(seedFile, cancellationToken));
        var pages = seed.Pages;
        seed.Pages = [];
        var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO site (id, json) VALUES (1, $json)";
        insert.Parameters.AddWithValue("$json", StilPageJson.SerializeSite(seed, indented: false));
        await insert.ExecuteNonQueryAsync(cancellationToken);
        foreach (var page in pages)
        {
            await SavePageAsync(connection, page, cancellationToken);
        }
    }

    public async Task<StilSiteData> GetSiteAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        var site = connection.CreateCommand();
        site.CommandText = "SELECT json FROM site WHERE id = 1";
        var data = await site.ExecuteScalarAsync(cancellationToken) is string json ? StilPageJson.DeserializeSite(json) : new StilSiteData();

        var pages = connection.CreateCommand();
        pages.CommandText = "SELECT json FROM pages ORDER BY rowid";
        await using var reader = await pages.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            data.Pages.Add(StilPageJson.Deserialize(reader.GetString(0)));
        }

        return data;
    }

    public async Task SavePageAsync(StilPageData page, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await SavePageAsync(connection, page, cancellationToken);
    }

    private static async Task SavePageAsync(SqliteConnection connection, StilPageData page, CancellationToken cancellationToken)
    {
        var save = connection.CreateCommand();
        save.CommandText = "INSERT INTO pages (path, json) VALUES ($path, $json) ON CONFLICT (path) DO UPDATE SET json = excluded.json";
        save.Parameters.AddWithValue("$path", page.Path.Trim('/'));
        save.Parameters.AddWithValue("$json", StilPageJson.Serialize(page, indented: false));
        await save.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static async Task ExecuteAsync(SqliteConnection connection, string sql, CancellationToken cancellationToken)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
