using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stil.Blazor;

// The docs in the browser: Stil.Docs.Host renders each page first, then it runs here.
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddStil();
builder.Services.AddScoped<Stil.Docs.DocsCodeChoice>();

var host = builder.Build();
// The language picked in the navbar's StilLanguageSelector.
await StilCulture.ApplyStoredAsync(host.Services, defaultCulture: "en");
await host.RunAsync();
