using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stil.Blazor;
using Stil.Docs;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddStil();

var host = builder.Build();
// The language picked in the navbar's StilLanguageSelector.
await StilCulture.ApplyStoredAsync(host.Services, defaultCulture: "en");
await host.RunAsync();
