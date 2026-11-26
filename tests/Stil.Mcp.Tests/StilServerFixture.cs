using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Stil.Mcp.Tests;

/// <summary>
/// Starts the server once, as a client does: a process spoken to over stdio. That happens only on
/// GitHub Actions; elsewhere the tests are skipped and no process is started, since security
/// software on a managed computer may treat a new program spoken to over stdio as a threat.
/// </summary>
public sealed class StilServerFixture : IAsyncLifetime
{
    /// <summary>True on GitHub Actions, which sets GITHUB_ACTIONS to "true".</summary>
    public static bool InCi => Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

    public McpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        if (!InCi)
        {
            return;
        }

        var server = Path.Combine(AppContext.BaseDirectory, "Stil.Mcp.dll");
        Client = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "Stil",
            Command = "dotnet",
            Arguments = [server],
        }));
    }

    public async Task DisposeAsync()
    {
        if (Client is not null)
        {
            await Client.DisposeAsync();
        }
    }

    /// <summary>Calls a tool and returns its text.</summary>
    public async Task<string> CallAsync(string tool, Dictionary<string, object?>? arguments = null)
    {
        var result = await Client.CallToolAsync(tool, arguments ?? []);
        Assert.NotEqual(true, result.IsError);
        return string.Concat(result.Content.OfType<TextContentBlock>().Select(block => block.Text));
    }
}

/// <summary>A test that starts the server: it runs only on GitHub Actions and is skipped elsewhere.</summary>
public sealed class ServerFactAttribute : FactAttribute
{
    public ServerFactAttribute()
    {
        if (!StilServerFixture.InCi)
        {
            Skip = "Starts the MCP server as a process, so it runs only on GitHub Actions (GITHUB_ACTIONS=true).";
        }
    }
}

[CollectionDefinition("Server")]
public sealed class ServerCollection : ICollectionFixture<StilServerFixture>;
