using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using Stil.Mcp;

// An MCP server over stdio: an assistant starts it (dnx Stil.Mcp) and asks it about Stil.
var builder = Host.CreateApplicationBuilder(args);

// stdout carries the protocol, so every log goes to stderr.
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

var version = typeof(StilTools).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";
builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation { Name = "Stil", Version = version };
        options.ServerInstructions = StilTools.Instructions;
    })
    .WithStdioServerTransport()
    .WithTools([typeof(StilTools)]);

await builder.Build().RunAsync();
