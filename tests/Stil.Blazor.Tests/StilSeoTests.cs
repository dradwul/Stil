using System.Globalization;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Blocks;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilSeoTests
{
    // Renders a HeadOutlet with a StilSeo, as App.razor and a page do.
    private static IRenderedComponent<IComponent> RenderHead(BunitContext context, Dictionary<string, object?> parameters)
    {
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context.Render(builder =>
        {
            builder.OpenComponent<HeadOutlet>(0);
            builder.CloseComponent();
            builder.OpenComponent<StilSeo>(1);
            builder.AddMultipleAttributes(2, parameters!);
            builder.CloseComponent();
        });
    }

    private static string? Meta(IRenderedComponent<IComponent> cut, string key) =>
        cut.FindAll($"meta[property='{key}'], meta[name='{key}']").SingleOrDefault()?.GetAttribute("content");

    [Theory]
    [InlineData("Pricing", "My Site", "Pricing · My Site")]
    [InlineData(null, "My Site", "My Site")]
    [InlineData("My Site", "My Site", "My Site")]
    [InlineData("Pricing", null, "Pricing")]
    public void TitleIsFollowedByTheSiteName(string? title, string? siteName, string expected)
    {
        using var context = new BunitContext();

        var cut = RenderHead(context, new() { ["Title"] = title, ["SiteName"] = siteName });

        Assert.Equal(expected, cut.Find("title").TextContent);
        Assert.Equal(title ?? siteName, Meta(cut, "og:title"));
    }

    [Fact]
    public void DescribesThePageForSearchAndLinkPreviews()
    {
        using var context = new BunitContext();

        var cut = RenderHead(context, new()
        {
            ["Title"] = "Pricing",
            ["SiteName"] = "My Site",
            ["Description"] = "Plans for every team.",
            ["Image"] = "images/share.png",
            ["ImageAlt"] = "The planner",
        });

        Assert.Equal("Plans for every team.", Meta(cut, "description"));
        Assert.Equal("Plans for every team.", Meta(cut, "og:description"));
        Assert.Equal("website", Meta(cut, "og:type"));
        Assert.Equal("My Site", Meta(cut, "og:site_name"));
        Assert.Equal("http://localhost/images/share.png", Meta(cut, "og:image"));
        Assert.Equal("The planner", Meta(cut, "og:image:alt"));
        Assert.Equal("summary_large_image", Meta(cut, "twitter:card"));
        Assert.Null(Meta(cut, "robots"));
    }

    [Fact]
    public void CanonicalIsTheAddressWithoutQueryOrFragmentUnlessSet()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("blog?utm_source=mail#top");

        var current = RenderHead(context, new() { ["Title"] = "Blog" });
        Assert.Equal("http://localhost/blog", current.Find("link[rel=canonical]").GetAttribute("href"));
        Assert.Equal("http://localhost/blog", Meta(current, "og:url"));

        using var other = new BunitContext();
        var set = RenderHead(other, new() { ["Title"] = "Blog", ["Canonical"] = "blog?page=2" });
        Assert.Equal("http://localhost/blog?page=2", set.Find("link[rel=canonical]").GetAttribute("href"));
        Assert.Equal("summary", Meta(set, "twitter:card"));
    }

    [Fact]
    public void AnArticleHasItsDatesAndAuthor()
    {
        using var context = new BunitContext();

        var cut = RenderHead(context, new()
        {
            ["Title"] = "Calm Mondays",
            ["Type"] = StilSeoType.Article,
            ["Published"] = (DateTimeOffset?)new DateTimeOffset(2026, 11, 3, 8, 0, 0, TimeSpan.FromHours(1)),
            ["Author"] = "Maja Lind",
        });

        Assert.Equal("article", Meta(cut, "og:type"));
        Assert.Equal("2026-11-03T08:00:00+01:00", Meta(cut, "article:published_time"));
        Assert.Null(Meta(cut, "article:modified_time"));
        Assert.Equal("Maja Lind", Meta(cut, "article:author"));
    }

    [Theory]
    [InlineData(true, false, "noindex")]
    [InlineData(false, true, "nofollow")]
    [InlineData(true, true, "noindex, nofollow")]
    public void RobotsKeepAPageOutOfSearch(bool noIndex, bool noFollow, string expected)
    {
        using var context = new BunitContext();

        var cut = RenderHead(context, new() { ["Title"] = "Not found", ["NoIndex"] = noIndex, ["NoFollow"] = noFollow });

        Assert.Equal(expected, Meta(cut, "robots"));
        Assert.Equal(noIndex, cut.FindAll("link[rel=canonical]").Count == 0);
        Assert.Equal(noIndex, Meta(cut, "og:url") is null);
    }

    [Fact]
    public void LocaleFollowsTheCultureOrIsSet()
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("sv-SE");
        try
        {
            using var context = new BunitContext();
            Assert.Equal("sv_SE", Meta(RenderHead(context, new() { ["Title"] = "Hem" }), "og:locale"));

            using var other = new BunitContext();
            Assert.Equal("nb_NO", Meta(RenderHead(other, new() { ["Title"] = "Hjem", ["Locale"] = "nb-NO" }), "og:locale"));

            using var neutral = new BunitContext();
            Assert.Null(Meta(RenderHead(neutral, new() { ["Title"] = "Home", ["Locale"] = "en" }), "og:locale"));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void StructuredDataIsJsonLdThatCannotCloseItsScript()
    {
        using var context = new BunitContext();
        var faq = StilStructuredData.FaqPage([("Kan jag säga upp?", "Yes.</script><script>alert(1)</script>")]);

        var cut = RenderHead(context, new() { ["Title"] = "FAQ", ["StructuredData"] = new[] { faq } });

        var script = cut.Find("script[type='application/ld+json']");
        Assert.DoesNotContain("<script>alert", cut.Markup);
        Assert.Contains("Kan jag säga upp?", script.TextContent);
        var json = JsonNode.Parse(script.TextContent)!;
        Assert.Equal("FAQPage", json["@type"]!.GetValue<string>());
        Assert.Equal("Yes.</script><script>alert(1)</script>", json["mainEntity"]![0]!["acceptedAnswer"]!["text"]!.GetValue<string>());
    }

    [Fact]
    public void StructuredDataHasTheSchemaOrgShapes()
    {
        var organization = StilStructuredData.Organization("Nordvik", "https://example.com", "https://example.com/logo.png", ["https://github.com/example"]);
        Assert.Equal("https://schema.org", organization["@context"]!.GetValue<string>());
        Assert.Equal("https://github.com/example", organization["sameAs"]![0]!.GetValue<string>());

        var article = StilStructuredData.Article("Calm Mondays", "https://example.com/blog/calm", published: new DateTimeOffset(2026, 11, 3, 0, 0, 0, TimeSpan.Zero), author: "Maja Lind");
        Assert.Equal("Article", article["@type"]!.GetValue<string>());
        Assert.Equal("2026-11-03T00:00:00+00:00", article["datePublished"]!.GetValue<string>());
        Assert.Equal("Person", article["author"]!["@type"]!.GetValue<string>());
        Assert.Null(article["author"]!["@context"]);
        Assert.False(article.ContainsKey("image"));

        var block = StilBlockData.Create("faq")
            .Add(StilBlockData.Create("question").Set("question", "Can I cancel?").Set("answer", "Yes."))
            .Add(StilBlockData.Create("question").Set("question", "Half done"));
        var questions = StilStructuredData.FaqPage(block)["mainEntity"]!.AsArray();
        Assert.Equal("Can I cancel?", Assert.Single(questions)!["name"]!.GetValue<string>());

        Assert.Equal("WebSite", StilStructuredData.WebSite("Nordvik", "https://example.com")["@type"]!.GetValue<string>());
    }

    [Fact]
    public void SitemapListsAbsoluteAddressesWithTheirHints()
    {
        var sitemap = new StilSitemap("https://example.com/sv")
            .Add("/", new DateTimeOffset(2026, 11, 3, 0, 0, 0, TimeSpan.Zero), StilSitemapFrequency.Weekly, 1)
            .Add("pricing?plan=team&period=year")
            .AddPage(new StilPageData { Path = "/blog/calm-mondays" }, new DateTimeOffset(2026, 11, 3, 9, 30, 0, TimeSpan.FromHours(1)));

        var xml = sitemap.ToXml();
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", xml);
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = XDocument.Parse(xml).Root!.Elements(ns + "url").ToList();
        Assert.Equal(["https://example.com/sv/", "https://example.com/sv/pricing?plan=team&period=year", "https://example.com/sv/blog/calm-mondays"],
            urls.Select(url => url.Element(ns + "loc")!.Value));
        Assert.Equal("2026-11-03", urls[0].Element(ns + "lastmod")!.Value);
        Assert.Equal("weekly", urls[0].Element(ns + "changefreq")!.Value);
        Assert.Equal("1.0", urls[0].Element(ns + "priority")!.Value);
        Assert.Null(urls[1].Element(ns + "lastmod"));
        Assert.Equal("2026-11-03T09:30:00+01:00", urls[2].Element(ns + "lastmod")!.Value);
        Assert.Contains("plan=team&amp;period=year", xml);
    }

    [Fact]
    public void SitemapRefusesWhatIsNotValid()
    {
        Assert.Throws<ArgumentException>(() => new StilSitemap("/relative"));
        Assert.Throws<ArgumentException>(() => new StilSitemap("ftp://example.com"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StilSitemap("https://example.com").Add("/", priority: 1.5));
    }

    [Fact]
    public void RobotsAllowsEverythingUntilItHasRules()
    {
        Assert.Equal("User-agent: *\nDisallow:\n", new StilRobots().ToString());

        var robots = new StilRobots()
            .Disallow("/admin/")
            .Allow("/admin/help")
            .Disallow("/", "GPTBot")
            .Sitemap("https://example.com/sitemap.xml");

        Assert.Equal(
            "User-agent: *\nDisallow: /admin/\nAllow: /admin/help\n\nUser-agent: GPTBot\nDisallow: /\n\nSitemap: https://example.com/sitemap.xml\n",
            robots.ToString());
    }

    [Fact]
    public void RobotsRefusesALineBreakAndARelativeSitemap()
    {
        Assert.Throws<ArgumentException>(() => new StilRobots().Disallow("/a\nAllow: /"));
        Assert.Throws<ArgumentException>(() => new StilRobots().Allow("/", "*\r\nDisallow: /"));
        Assert.Throws<ArgumentException>(() => new StilRobots().Sitemap("sitemap.xml"));
    }
}
