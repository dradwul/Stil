using Stil.Blazor;
using Stil.Docs.Host.Components;

var builder = WebApplication.CreateBuilder(args);

// Every page is rendered here first, then runs in the browser with WebAssembly (Stil.Docs).
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
builder.Services.AddStil();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/not-found", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// The docs are in English. A language picked in the navbar is in the culture cookie, so the
// first render is already in it; the examples and Stil's own texts follow it.
app.UseRequestLocalization(options => options
    .SetDefaultCulture("en")
    .AddSupportedCultures("en", "sv", "nb", "da", "fi")
    .AddSupportedUICultures("en", "sv", "nb", "da", "fi"));
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Stil.Docs._Imports).Assembly);

app.Run();
