using System.Globalization;
using Stil.Demo;
using Stil.Demo.Components;
using Stil.Demo.Components.Layout;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Stil's dialog and toast services. A global theme could be set here, for example:
// builder.Services.AddStil(options => options.Theme.Primary = "#6741d9");
builder.Services.AddStil();
// The app's own texts from Locales/*.json, through IStringLocalizer<T>.
builder.Services.AddStilLocalization();
builder.Services.AddSingleton<OrderStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// The language the StilLanguageSelector stores in the culture cookie. The screenshot tests run
// the demo with invariant globalization, where named cultures do not exist, so it stays English.
if (CultureData.Available(MainLayout.Cultures))
{
    app.UseRequestLocalization(options => options
        .SetDefaultCulture("en")
        .AddSupportedCultures(MainLayout.Cultures)
        .AddSupportedUICultures(MainLayout.Cultures));
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

internal static class CultureData
{
    public static bool Available(IEnumerable<string> cultures)
    {
        try
        {
            foreach (var culture in cultures)
            {
                CultureInfo.GetCultureInfo(culture, predefinedOnly: true);
            }

            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
