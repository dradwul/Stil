using Stil.Blazor;
using StilWeb.Components;
using StilWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// The site renders on the server, without interactivity: fast pages that search engines read as
// they are. Stil's dropdowns, the navbar's menu button and the pricing switch work without it, and
// the contact form posts like any HTML form.
builder.Services.AddRazorComponents();

// Stil's services, and the font and global theme that <StilHead Blocks /> writes to every page.
builder.Services.AddStil(options =>
{
#if (HasPrimary && HasRadius)
    options.Theme = new() { Primary = "STIL_PRIMARY_COLOR", Radius = "STIL_RADIUS_MD" };
#elif (HasPrimary)
    options.Theme = new() { Primary = "STIL_PRIMARY_COLOR" };
#elif (HasRadius)
    options.Theme = new() { Radius = "STIL_RADIUS_MD" };
#else
    // Your brand color for the whole site, for example:
    // options.Theme = new() { Primary = "#7048e8" };
#endif
#if (HasRadius)
    // The corners: Radius above for most things, smaller for small ones, larger for cards.
    options.Theme.Tokens["--stil-radius-sm"] = "STIL_RADIUS_SM";
    options.Theme.Tokens["--stil-radius-lg"] = "STIL_RADIUS_LG";
#endif
#if (HasGoogleFont)
    // Any Google font: StilFont.Google("…").
    options.Font = StilFont.Google("STIL_GOOGLE_FONT");
#elif (HasWebFont)
    // Another font: one of StilFonts, StilFont.Google("…") or StilFont.CdnFonts("…") (check its license).
    options.Font = StilFonts.Geologica;
#else
    // A web font, for example StilFont.Google("Inter"):
    // options.Font = StilFont.Google("Inter");
#endif
});

// The blog's posts. Replace SampleBlogService with your own (a database, files or a CMS); the
// pages only know IBlogService.
builder.Services.AddSingleton<IBlogService, SampleBlogService>();

// The pages as data, in a SQLite file: Data/site.json fills it on the first run. Page.razor renders
// them, so the home page, pricing and any page you add change without a new build.
var sites = new SqliteSiteStore(builder.Configuration.GetConnectionString("Site") ?? "Data Source=site.db");
await sites.EnsureCreatedAsync(Path.Combine(builder.Environment.ContentRootPath, "Data", "site.json"));
builder.Services.AddSingleton<ISiteStore>(sites);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. See https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
// The site is in English, so Stil's built-in texts and the dates are too, whatever the server's
// own culture. For more languages, add their cultures here and translate the pages.
app.UseRequestLocalization(options => options
    .SetDefaultCulture("en")
    .AddSupportedCultures("en")
    .AddSupportedUICultures("en"));
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

// For search engines: the site's addresses, and where they are. The pages with their own route are
// listed by hand; the pages in the database and the posts come from their stores.
app.MapGet("/sitemap.xml", async (HttpRequest request, ISiteStore store, IBlogService posts) =>
{
    var site = await store.GetSiteAsync();
    var sitemap = new StilSitemap(SiteUrl(request));
    foreach (var page in site.Pages)
    {
        sitemap.AddPage(page);
    }

    sitemap.Add("blog").Add("contact");
    foreach (var post in posts.All())
    {
        sitemap.Add($"blog/{post.Slug}", post.Date);
    }

    return Results.Text(sitemap.ToXml(), "application/xml");
});
app.MapGet("/robots.txt", (HttpRequest request) =>
    new StilRobots().Sitemap($"{SiteUrl(request)}sitemap.xml").ToString());

app.Run();

// The site's address, from the request: https://example.com/ in production.
static string SiteUrl(HttpRequest request) => $"{request.Scheme}://{request.Host}{request.PathBase}/";
