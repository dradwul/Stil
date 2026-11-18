using System.Globalization;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Blocks;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilSiteTests
{
    private static StilSiteData Site()
    {
        var theme = new StilTheme { Primary = "#6741d9", Radius = "0.25rem" };
        theme.Tokens["--stil-space-4"] = "1.5rem";
        return new StilSiteData
        {
            Name = "Nordvik",
            Url = "https://example.com",
            Language = "en",
            Theme = theme,
            Font = StilFonts.Geologica,
        }
        .Add(new StilPageData { Path = "", Key = "home", Language = "en", Title = "Home" })
        .Add(new StilPageData { Path = "sv", Key = "home", Language = "sv", Title = "Hem" })
        .Add(new StilPageData { Path = "pricing", Key = "pricing", Language = "en" })
        .Add(new StilPageData { Path = "sv/priser", Key = "pricing", Language = "sv" })
        .Add(new StilPageData { Path = "about", Language = "en" });
    }

    [Fact]
    public void SiteRoundTripsWithItsThemeFontAndPages()
    {
        var json = StilPageJson.SerializeSite(Site());
        var site = StilPageJson.DeserializeSite(json);

        Assert.Equal(json, StilPageJson.SerializeSite(site));
        Assert.Equal("#6741d9", site.Theme!.Primary);
        Assert.Equal("1.5rem", site.Theme.Tokens["--stil-space-4"]);
        Assert.Equal(StilFonts.Geologica, site.Font);
        Assert.Equal("pricing", site.Pages[3].Key);

        using var document = JsonDocument.Parse(json);
        var theme = document.RootElement.GetProperty("theme");
        Assert.False(theme.TryGetProperty("hasOverrides", out _));
        Assert.False(document.RootElement.GetProperty("font").TryGetProperty("fontFamily", out _));
        Assert.False(StilPageJson.SerializeSite(new StilSiteData { Theme = new StilTheme { Primary = "red" } }).Contains("tokens", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("""{ "version": 1, "theme": { "primary": "red; } body { display: none" } }""")]
    [InlineData("""{ "version": 1, "theme": { "tokens": { "color": "red" } } }""")]
    [InlineData("""{ "version": 1, "font": { "family": "Inter", "stylesheetUrl": "http://example.com/font.css" } }""")]
    [InlineData("""{ "version": 2 }""")]
    [InlineData("""{ "version": 1, "pages": [ { "version": 2 } ] }""")]
    public void RefusesASiteThatCouldBreakThePageOrIsNewer(string json)
    {
        Assert.Throws<JsonException>(() => StilPageJson.DeserializeSite(json));
    }

    [Fact]
    public void FindsAPageByItsPath()
    {
        var site = Site();

        Assert.Equal("Hem", site.Find("/sv/")!.Title);
        Assert.Equal("Home", site.Find("/")!.Title);
        Assert.Equal("sv/priser", site.Find("SV/Priser")!.Path);
        Assert.Null(site.Find("nope"));
    }

    [Fact]
    public void AlternatesAreTheTranslationsAndTheDefault()
    {
        var site = Site();

        Assert.Equal(
            [new StilAlternate("en", "https://example.com/pricing"), new StilAlternate("sv", "https://example.com/sv/priser"), new StilAlternate("x-default", "https://example.com/pricing")],
            site.AlternatesOf(site.Find("sv/priser")!));
        Assert.Empty(site.AlternatesOf(site.Find("about")!));

        // Without the site's address, they are relative to the site.
        var relative = new StilSiteData { Language = "en" }
            .Add(new StilPageData { Path = "", Key = "home", Language = "en" })
            .Add(new StilPageData { Path = "sv", Key = "home", Language = "sv" });
        Assert.Equal(["/", "/sv", "/"], relative.AlternatesOf(relative.Pages[0]).Select(alternate => alternate.Url));
    }

    [Fact]
    public void SeoLinksTheAlternates()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var site = Site();
        var page = site.Find("pricing")!;

        var cut = context.Render(builder =>
        {
            builder.OpenComponent<HeadOutlet>(0);
            builder.CloseComponent();
            builder.OpenComponent<StilSeo>(1);
            builder.AddAttribute(2, nameof(StilSeo.Title), "Pricing");
            builder.AddAttribute(3, nameof(StilSeo.Alternates), site.AlternatesOf(page));
            builder.CloseComponent();
        });

        Assert.Equal(["en https://example.com/pricing", "sv https://example.com/sv/priser", "x-default https://example.com/pricing"],
            cut.FindAll("link[rel=alternate]").Select(link => $"{link.GetAttribute("hreflang")} {link.GetAttribute("href")}"));
    }

    [Fact]
    public void HeadTakesTheSitesThemeAndFont()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options => options.Theme.Primary = "#000000");
        var site = Site();

        var cut = context.Render<StilHead>(parameters => parameters.Add(c => c.Theme, site.Theme).Add(c => c.Font, site.Font));

        Assert.Equal(StilFonts.Geologica.StylesheetUrl, cut.FindAll("link")[1].GetAttribute("href"));
        var style = cut.Find("style").TextContent;
        Assert.Contains("--stil-font-variation-settings: \"SHRP\" 100;", style);
        Assert.True(style.IndexOf("--stil-color-primary: #000000;", StringComparison.Ordinal) < style.IndexOf("--stil-color-primary: #6741d9;", StringComparison.Ordinal));
        Assert.Contains("--stil-space-4: 1.5rem;", style);
    }

    [Fact]
    public void PageIsMarkedWithItsLanguageAndItsTextsFollowIt()
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
        try
        {
            using var context = new BunitContext();
            var page = new StilPageData { Language = "sv" }
                .Add(StilBlockData.Create("contact").Set("title", "Kontakt").Set("formAction", "kontakt"));

            var cut = context.Render<StilPage>(parameters => parameters.Add(c => c.Page, page));

            var wrapper = cut.Find("div.stil-page");
            Assert.Equal("sv", wrapper.GetAttribute("lang"));
            Assert.Null(wrapper.GetAttribute("style"));
            Assert.Equal(["Namn", "E-post", "Meddelande"], cut.FindAll("form label").Select(label => label.TextContent));
            Assert.Equal("Skicka", cut.Find("form button").TextContent);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void PageWithoutALanguageUsesTheCurrentCultureAndCanBeThemed()
    {
        using var context = new BunitContext();
        var page = new StilPageData().Add(StilBlockData.Create("contact").Set("formAction", "contact"));

        var cut = context.Render<StilPage>(parameters => parameters.Add(c => c.Page, page).Add(c => c.Theme, Site().Theme));

        var wrapper = cut.Find("div.stil-page");
        Assert.False(wrapper.HasAttribute("lang"));
        Assert.Equal("--stil-color-primary: #6741d9; --stil-radius-md: 0.25rem; --stil-space-4: 1.5rem;", wrapper.GetAttribute("style"));
        Assert.Equal("Send", cut.Find("form button").TextContent);
    }
}
