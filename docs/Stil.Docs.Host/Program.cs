using System.Reflection;
using Microsoft.AspNetCore.Components;
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

// For search engines, built with Stil's own StilSitemap and StilRobots: every docs page, and where
// the sitemap is. tools/export-docs.mjs saves both, and reads the sitemap to know which pages to save.
app.MapGet("/sitemap.xml", (HttpRequest request, IConfiguration configuration) =>
{
    var sitemap = new StilSitemap(SiteUrl(request, configuration));
    foreach (var route in DocsRoutes())
    {
        sitemap.Add(route);
    }

    return Results.Text(sitemap.ToXml(), "application/xml");
});
app.MapGet("/robots.txt", (HttpRequest request, IConfiguration configuration) =>
    new StilRobots().Sitemap($"{SiteUrl(request, configuration)}sitemap.xml").ToString());

app.Run();

// The site's public address: Docs:SiteUrl (docs.yml sets the GitHub Pages one), or this server's.
static string SiteUrl(HttpRequest request, IConfiguration configuration) =>
    configuration["Docs:SiteUrl"] is { Length: > 0 } url
        ? url.TrimEnd('/') + "/"
        : $"{request.Scheme}://{request.Host}{request.PathBase}/";

// Every docs page: the routes without parameters, the home page first, and not the not-found page.
static IEnumerable<string> DocsRoutes() => typeof(Stil.Docs._Imports).Assembly.GetTypes()
    .SelectMany(type => type.GetCustomAttributes<RouteAttribute>())
    .Select(route => route.Template)
    .Where(template => !template.Contains('{') && template != "/not-found")
    .Distinct()
    .OrderBy(template => template == "/" ? "" : template, StringComparer.Ordinal);
