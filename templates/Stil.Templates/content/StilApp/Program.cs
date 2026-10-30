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

#if (HasPrimary)
// Stil's services and the global theme, which <StilHead /> writes to every page.
builder.Services.AddStil(options => options.Theme = new() { Primary = "STIL_PRIMARY_COLOR" });
#else
// Stil's services. To give the whole app your brand color, use for example:
// builder.Services.AddStil(options => options.Theme = new() { Primary = "#7048e8" });
builder.Services.AddStil();
#endif
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
