using Stil.Blazor;
using StilApp;
using StilApp.Components;
#if (HasOrders || IsDashboard || IsMonitoring || IsFinance)
using StilApp.Data;
#endif

var builder = WebApplication.CreateBuilder(args);

#if (UseServer)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
#elif (NeedsInteractivity)
// The app renders on the server without interactivity, except the pages and the parts of the
// layout that need it: they set @rendermode InteractiveServer.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
#else
builder.Services.AddRazorComponents();
#endif

// Stil's services, and the font and global theme that <StilHead /> writes to every page.
builder.Services.AddStil(options =>
{
#if (HasPrimary && HasRadius)
    options.Theme = new() { Primary = "STIL_PRIMARY_COLOR", Radius = "STIL_RADIUS_MD" };
#elif (HasPrimary)
    options.Theme = new() { Primary = "STIL_PRIMARY_COLOR" };
#elif (HasRadius)
    options.Theme = new() { Radius = "STIL_RADIUS_MD" };
#else
    // Your brand color for the whole app, for example:
    // options.Theme = new() { Primary = "#7048e8" };
#endif
#if (HasRadius)
    // The corners: Radius above for most things, smaller for small ones, larger for cards and dialogs.
    options.Theme.Tokens["--stil-radius-sm"] = "STIL_RADIUS_SM";
    options.Theme.Tokens["--stil-radius-lg"] = "STIL_RADIUS_LG";
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
// The sample data. Replace an implementation with your own to show real data; the pages only
// know the interfaces.
#if (HasOrders)
builder.Services.AddSingleton<IOrderService, InMemoryOrderService>();
#endif
#if (IsDashboard)
builder.Services.AddSingleton<ISalesService, SampleSalesService>();
#endif
#if (IsMonitoring)
builder.Services.AddSingleton<IMonitoringService, SampleMonitoringService>();
#endif
#if (IsFinance)
builder.Services.AddSingleton<IFinanceService, SampleFinanceService>();
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
#if (UseServer || NeedsInteractivity)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
#else
app.MapRazorComponents<App>();
#endif

app.Run();
