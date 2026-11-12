using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Blocks;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilBlocksTests
{
    [Fact]
    public void SectionIsALandmarkNamedByItsTitle()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSection>(parameters => parameters
            .Add(c => c.Eyebrow, "Features")
            .Add(c => c.Title, "Everything in one place")
            .Add(c => c.Lead, "One stylesheet, every component.")
            .Add(c => c.Centered, true)
            .Add(c => c.Tone, StilBlockTone.Muted)
            .AddChildContent("<p>Body</p>"));

        var section = cut.Find("section");
        Assert.Equal("stil-section stil-section-muted", section.ClassName);
        var title = cut.Find("h2.stil-section-title");
        Assert.Equal("Everything in one place", title.TextContent);
        Assert.Equal(title.Id, section.GetAttribute("aria-labelledby"));
        Assert.Equal("stil-section-header stil-section-header-center", cut.Find("header").ClassName);
        Assert.Equal("Features", cut.Find(".stil-eyebrow").TextContent);
        Assert.Equal("One stylesheet, every component.", cut.Find(".stil-lead").TextContent);
        Assert.Equal("Body", cut.Find(".stil-container > p:last-child").TextContent);
    }

    [Fact]
    public void SectionWithoutAHeaderIsNotNamed()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSection>(parameters => parameters
            .Add(c => c.Tone, StilBlockTone.Brand)
            .AddUnmatched("class", "landing")
            .AddChildContent("<p>Body</p>"));

        var section = cut.Find("section");
        Assert.Equal("stil-section stil-section-brand landing", section.ClassName);
        Assert.False(section.HasAttribute("aria-labelledby"));
        Assert.Empty(cut.FindAll("header"));
    }

    [Fact]
    public void HeroIsCenteredOrSplitBesideItsPicture()
    {
        using var context = new BunitContext();

        var centered = context.Render<StilHero>(parameters => parameters
            .Add(c => c.Title, "Ship your site this week")
            .Add(c => c.Actions, (RenderFragment)(builder => builder.AddMarkupContent(0, "<a class=\"stil-btn\" href=\"start\">Start</a>"))));

        Assert.Equal("stil-section stil-hero", centered.Find("section").ClassName);
        Assert.Equal("Ship your site this week", centered.Find("h1.stil-hero-title").TextContent);
        Assert.Equal("Start", centered.Find(".stil-hero-actions a").TextContent);
        Assert.Empty(centered.FindAll(".stil-hero-media"));

        var split = context.Render<StilHero>(parameters => parameters
            .Add(c => c.Title, "Ship your site this week")
            .Add(c => c.HeadingLevel, 2)
            .Add(c => c.Tone, StilBlockTone.Brand)
            .Add(c => c.Media, (RenderFragment)(builder => builder.AddMarkupContent(0, "<img src=\"shot.png\" alt=\"The editor\">"))));

        Assert.Equal("stil-section stil-hero stil-hero-split stil-section-brand", split.Find("section").ClassName);
        Assert.NotNull(split.Find("h2.stil-hero-title"));
        Assert.Equal("The editor", split.Find(".stil-hero-media img").GetAttribute("alt"));
        Assert.Equal(split.Find("h2").Id, split.Find("section").GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void FeaturesAreAListWithDecorativeIcons()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFeatureGrid>(parameters => parameters
            .Add(c => c.Cards, true)
            .AddChildContent<StilFeature>(feature => feature
                .Add(c => c.Icon, StilIcons.Moon)
                .Add(c => c.Title, "Dark mode")
                .Add(c => c.Text, "Every color has a dark twin.")));

        Assert.Equal("stil-features stil-features-cards", cut.Find("ul").ClassName);
        var item = cut.Find("li.stil-feature");
        Assert.Equal("Dark mode", item.QuerySelector("h3.stil-feature-title")!.TextContent);
        Assert.Equal("Every color has a dark twin.", item.QuerySelector("p")!.TextContent);
        Assert.Equal("true", item.QuerySelector(".stil-feature-icon svg")!.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void CallToActionHasATitleTextAndActions()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCallToAction>(parameters => parameters
            .Add(c => c.Title, "Ready to start?")
            .Add(c => c.Text, "Free for open source.")
            .Add(c => c.Centered, true)
            .Add(c => c.Tone, StilBlockTone.Brand)
            .Add(c => c.Actions, (RenderFragment)(builder => builder.AddMarkupContent(0, "<a class=\"stil-btn\" href=\"start\">Start</a>"))));

        Assert.Equal("stil-cta stil-cta-center stil-cta-brand", cut.Find("div").ClassName);
        Assert.Equal("Ready to start?", cut.Find("h2.stil-cta-title").TextContent);
        Assert.Equal("Free for open source.", cut.Find(".stil-cta-body p").TextContent);
        Assert.Equal("Start", cut.Find(".stil-cta-actions a").TextContent);
    }

    [Fact]
    public void StatsAreADescriptionList()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatsBand>(parameters => parameters
            .AddChildContent<StilStatsItem>(item => item
                .Add(c => c.Label, "Customers")
                .Add(c => c.Value, "12,000")));

        Assert.Equal("stil-stats", cut.Find("dl").ClassName);
        Assert.Equal("Customers", cut.Find("dl > .stil-stats-item > dt.stil-stats-label").TextContent);
        Assert.Equal("12,000", cut.Find("dl > .stil-stats-item > dd.stil-stats-value").TextContent);
    }

    [Fact]
    public void LogosArePicturesWithTheirNames()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilLogoCloud>(parameters => parameters
            .Add(c => c.Title, "Trusted by teams at")
            .Add(c => c.Muted, true)
            .Add(c => c.Height, "2.5rem")
            .AddChildContent<StilLogo>(logo => logo
                .Add(c => c.Src, "northwind.svg")
                .Add(c => c.Alt, "Northwind")
                .Add(c => c.Href, "customers/northwind")));

        var cloud = cut.Find(".stil-logos");
        Assert.Equal("stil-logos stil-logos-muted", cloud.ClassName);
        Assert.Equal("--stil-logos-height: 2.5rem", cloud.GetAttribute("style"));
        Assert.Equal("Trusted by teams at", cut.Find(".stil-logos-title").TextContent);
        Assert.Equal("customers/northwind", cut.Find("ul.stil-logos-list > li > a").GetAttribute("href"));
        Assert.Equal("Northwind", cut.Find("li img").GetAttribute("alt"));
    }

    [Fact]
    public void LogoCanBeInlineContent()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilLogo>(parameters => parameters.AddChildContent("<span>Contoso</span>"));

        Assert.Equal("Contoso", cut.Find("li > span").TextContent);
        Assert.Empty(cut.FindAll("img"));
    }
}
