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
#elif (HasFeatures)
// The app renders on the server without interactivity, except the feature pages and the parts of
// the layout that need it: they set @rendermode InteractiveServer.
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
    // Any Google font: StilFont.Google("…"). Or one from cdnfonts.com: StilFonts.Geologica, StilFont.CdnFonts("…").
    options.Font = StilFont.Google("STIL_GOOGLE_FONT");
#elif (HasWebFont)
    // Another font: StilFonts.Lexend (the default, free under the SIL Open Font License), StilFont.Google("…"),
    // or StilFont.CdnFonts("…") from cdnfonts.com (check the license there).
    options.Font = StilFonts.Lexend;
#else
    // A web font, for example StilFonts.Lexend (free), StilFont.Google("…") or StilFont.CdnFonts("…"):
    // options.Font = StilFonts.Geologica;
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
#if (UseServer || HasFeatures)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
#else
app.MapRazorComponents<App>();
#endif

app.Run();
