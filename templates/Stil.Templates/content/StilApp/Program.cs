using Stil.Blazor;
using StilApp;
using StilApp.Components;
#if (IncludeSampleContent)
using StilApp.Data;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (UseServer)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
#else
builder.Services.AddRazorComponents();
#endif

// Stil's services, and the font and global theme that <StilHead /> writes to every page.
builder.Services.AddStil(options =>
{
#if (HasPrimary)
    options.Theme = new() { Primary = "STIL_PRIMARY_COLOR" };
#else
    // Your brand color for the whole app, for example:
    // options.Theme = new() { Primary = "#7048e8" };
#endif
#if (HasGoogleFont)
    // Any Google font: StilFont.Google("…"). Or one from cdnfonts.com: StilFonts.BrCobane, StilFont.CdnFonts("…").
    options.Font = StilFont.Google("STIL_GOOGLE_FONT");
#elif (HasCdnFont)
    // Another font: StilFonts.BrCobane, StilFont.CdnFonts("…") from cdnfonts.com, or StilFont.Google("…").
    options.Font = StilFonts.Lexend;
#else
    // A web font, for example StilFonts.BrCobane, StilFont.CdnFonts("…") or StilFont.Google("…"):
    // options.Font = StilFonts.BrCobane;
#endif
});
// The app's own texts from Locales/*.json, through IStringLocalizer<T>.
builder.Services.AddStilLocalization();
#if (IncludeSampleContent)
builder.Services.AddSingleton<OrderStore>();
#endif

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. See https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
// The language the StilLanguageSelector in the navbar stores in the culture cookie.
app.UseRequestLocalization(options => options
    .SetDefaultCulture(AppCultures.Default)
    .AddSupportedCultures(AppCultures.All)
    .AddSupportedUICultures(AppCultures.All));
app.UseAntiforgery();

app.MapStaticAssets();
#if (UseServer)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
#else
app.MapRazorComponents<App>();
#endif

app.Run();
