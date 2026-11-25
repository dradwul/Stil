using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Stil.Mcp.Tests;

/// <summary>Starts the server once, as a client does: a process spoken to over stdio.</summary>
public sealed class StilServerFixture : IAsyncLifetime
{
    public McpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var server = Path.Combine(AppContext.BaseDirectory, "Stil.Mcp.dll");
        Client = await McpClient.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "Stil",
            Command = "dotnet",
            Arguments = [server],
        }));
    }

    public async Task DisposeAsync() => await Client.DisposeAsync();

    /// <summary>Calls a tool and returns its text.</summary>
    public async Task<string> CallAsync(string tool, Dictionary<string, object?>? arguments = null)
    {
        var result = await Client.CallToolAsync(tool, arguments ?? []);
        Assert.NotEqual(true, result.IsError);
        return string.Concat(result.Content.OfType<TextContentBlock>().Select(block => block.Text));
    }
}

[CollectionDefinition("Server")]
public sealed class ServerCollection : ICollectionFixture<StilServerFixture>;
