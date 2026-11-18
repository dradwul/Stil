using System.Globalization;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Blocks;

namespace Stil.Blazor.Tests;

public class StilPageModelTests
{
    private static StilBlockData Action(string text, string href, string? style = null) =>
        StilBlockData.Create("action").Set("text", text).Set("href", href).Set("style", style);

    // One block of each built-in type.
    private static StilPageData FullPage() => new StilPageData { Path = "/", Title = "Home", Language = "en" }
        .Add(StilBlockData.Create("hero").Set("title", "Plan the week").Set("lead", "In ten minutes.")
            .Set("image", new StilBlockImage("hero.png", "The planner"))
            .Add(Action("Start free", "signup")).Add(Action("See prices", "pricing")))
        .Add(StilBlockData.Create("features").Set("id", "features").Set("title", "Features").Set("cards", true)
            .Add(StilBlockData.Create("feature").Set("icon", "calendar").Set("title", "Plans").Set("text", "Drag to plan.")))
        .Add(StilBlockData.Create("logos").Set("title", "Trusted by").Set("muted", true)
            .Add(StilBlockData.Create("logo").Set("name", "Nordvik"))
            .Add(StilBlockData.Create("logo").Set("name", "Kallio").Set("image", new StilBlockImage("kallio.svg"))))
        .Add(StilBlockData.Create("stats").Set("title", "In numbers")
            .Add(StilBlockData.Create("stat").Set("value", "12,000").Set("label", "Teams")))
        .Add(StilBlockData.Create("testimonials").Set("title", "What they say")
            .Add(StilBlockData.Create("testimonial").Set("quote", "Calm Mondays.").Set("name", "Maja Lind").Set("featured", true))
            .Add(StilBlockData.Create("testimonial").Set("quote", "Less meetings.").Set("name", "Ola Berg")))
        .Add(StilBlockData.Create("pricing").Set("title", "Prices").Set("periodSwitch", true)
            .Add(StilBlockData.Create("plan").Set("name", "Team").Set("price", "€8").Set("yearlyPrice", "€80")
                .Set("features", new[] { "Plans", "Reports" }).Set("featured", true).Set("actionText", "Start").Set("actionHref", "signup")))
        .Add(StilBlockData.Create("faq").Set("title", "Questions")
            .Add(StilBlockData.Create("question").Set("question", "Can I cancel?").Set("answer", "Yes.\n\nAny time.")))
        .Add(StilBlockData.Create("team").Set("title", "The team")
            .Add(StilBlockData.Create("member").Set("name", "Maja Lind").Set("role", "CEO")))
        .Add(StilBlockData.Create("cta").Set("title", "Start today").Set("brand", true).Add(Action("Start free", "signup")))
        .Add(StilBlockData.Create("text").Set("title", "About us").Set("text", "We plan.\r\n\r\nWe ship."))
        .Add(StilBlockData.Create("contact").Set("title", "Talk to us").Set("email", "hello@example.com"))
        .Add(StilBlockData.Create("posts").Set("title", "Blog")
            .Add(StilBlockData.Create("post").Set("title", "Calm Mondays").Set("href", "blog/calm-mondays")
                .Set("date", new DateOnly(2026, 11, 3)).Set("tags", new[] { "Planning" })));

    [Fact]
    public void PageRoundTripsThroughJson()
    {
        var json = StilPageJson.Serialize(FullPage());
        var again = StilPageJson.Serialize(StilPageJson.Deserialize(json));

        Assert.Equal(json, again);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("version").GetInt32());
        Assert.Equal("Home", root.GetProperty("title").GetString());
        Assert.False(root.TryGetProperty("description", out _));
        var hero = root.GetProperty("blocks")[0];
        Assert.Equal("hero", hero.GetProperty("type").GetString());
        Assert.Equal("The planner", hero.GetProperty("settings").GetProperty("image").GetProperty("alt").GetString());
        Assert.Equal(2, hero.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public void EmptySettingsAndItemsAreLeftOut()
    {
        var json = StilPageJson.Serialize(new StilPageData().Add(StilBlockData.Create("text")), indented: false);

        Assert.Equal("""{"version":1,"path":"","blocks":[{"type":"text"}]}""", json);
    }

    [Fact]
    public void ReadsCommentsAndTrailingCommas()
    {
        var page = StilPageJson.Deserialize("""
            {
              // The home page
              "version": 1,
              "blocks": [ { "type": "text", "settings": { "text": "Hi" }, }, ],
            }
            """);

        Assert.Equal("Hi", Assert.Single(page.Blocks).GetText("text"));
    }

    [Fact]
    public void RefusesANewerVersionAndNull()
    {
        var newer = Assert.Throws<JsonException>(() => StilPageJson.Deserialize("""{ "version": 2, "blocks": [] }"""));
        Assert.Contains("version 2", newer.Message);
        Assert.Throws<JsonException>(() => StilPageJson.Deserialize("null"));
    }

    [Fact]
    public void SettingsReadBackAsTheirKind()
    {
        var block = StilBlockData.Create("x")
            .Set("text", "Hi").Set("yes", true).Set("count", 3).Set("price", 9.5m)
            .Set("date", new DateOnly(2026, 11, 3)).Set("tone", StilBlockTone.Muted)
            .Set("tags", new List<string> { "a", "b" }).Set("image", new StilBlockImage("a.png", "A"));

        Assert.Equal("Hi", block.GetText("text"));
        Assert.True(block.GetBool("yes"));
        Assert.False(block.GetBool("missing"));
        Assert.Equal(3, block.GetNumber("count"));
        Assert.Equal(9.5, block.GetNumber("price"));
        Assert.Equal("2026-11-03", block.GetText("date"));
        Assert.Equal(new DateTime(2026, 11, 3), block.GetDate("date"));
        Assert.Equal("muted", block.GetText("tone"));
        Assert.Equal(StilBlockTone.Muted, block.GetChoice("tone", StilBlockTone.Default));
        Assert.Equal(StilBlockTone.Brand, block.Set("tone", "nonsense").GetChoice("tone", StilBlockTone.Brand));
        Assert.Equal(["a", "b"], block.GetList("tags"));
        Assert.Equal(new StilBlockImage("a.png", "A"), block.GetImage("image"));

        block.Set("text", null);
        Assert.Null(block.GetText("text"));
        Assert.False(block.Settings.ContainsKey("text"));
    }

    [Fact]
    public void CatalogHasTheBuiltInTypesAndTakesYourOwn()
    {
        Assert.Equal(
            ["hero", "features", "logos", "stats", "testimonials", "pricing", "faq", "team", "cta", "text", "contact", "posts"],
            StilBlockCatalog.Default.All.Select(definition => definition.Type));
        var hero = StilBlockCatalog.Default.Find("hero")!;
        Assert.True(hero.Setting("title")!.Required);
        Assert.Equal(3, hero.Items!.Max);
        Assert.Equal(["default", "muted", "brand"], StilBlockCatalog.Default.Find("features")!.Setting("tone")!.Choices);
        Assert.Null(StilBlockCatalog.Default.Find("nope"));

        var custom = new StilBlockDefinition("quote", "Quote", [new("text", "Text", StilBlockSettingKind.LongText)]);
        Assert.Throws<InvalidOperationException>(() => StilBlockCatalog.Default.Add(custom));
        var catalog = new StilBlockCatalog().Add(custom);
        Assert.Same(custom, catalog.Find("quote"));
        Assert.Null(StilBlockCatalog.Default.Find("quote"));
    }

    [Fact]
    public void PageRendersEveryBuiltInBlockInOrder()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<StilPage>(parameters => parameters.Add(c => c.Page, FullPage()));

        string[] roots = ["stil-hero", "stil-features", "stil-logos", "stil-stats", "stil-testimonials", "stil-pricing",
            "stil-faq", "stil-team", "stil-cta", "stil-prose", "stil-contact", "stil-posts"];
        var sections = cut.FindAll("section.stil-section");
        Assert.Equal(roots.Length, sections.Count);
        for (var i = 0; i < roots.Length; i++)
        {
            Assert.True(sections[i].ClassList.Contains(roots[i]) || sections[i].QuerySelector($".{roots[i]}") is not null, roots[i]);
        }

        Assert.Equal("features", sections[1].Id);
        Assert.Single(cut.FindAll("h1"));
    }

    [Fact]
    public void HeroHasThePageTitleItsPictureAndItsButtons()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, FullPage().Blocks[0]));

        Assert.Equal("Plan the week", cut.Find("section.stil-hero h1").TextContent);
        Assert.Contains("stil-hero-split", cut.Find("section").ClassList);
        Assert.Equal("The planner", cut.Find("img[src='hero.png']").GetAttribute("alt"));
        var buttons = cut.FindAll("a.stil-btn");
        Assert.Equal(["signup", "pricing"], buttons.Select(button => button.GetAttribute("href")));
        Assert.DoesNotContain("stil-btn-outline", buttons[0].ClassList);
        Assert.Contains("stil-btn-outline", buttons[1].ClassList);
    }

    [Theory]
    [InlineData("pricing")]
    [InlineData("faq")]
    [InlineData("cta")]
    [InlineData("contact")]
    public void ABlockMarkedAsThePageTitleHasTheH1(string type)
    {
        using var context = new BunitContext();

        var plain = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, StilBlockData.Create(type).Set("title", "Prices")));
        var title = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data,
            StilBlockData.Create(type).Set("title", "Prices").Set("pageTitle", true)));

        Assert.Equal("Prices", plain.Find("h2").TextContent);
        Assert.Empty(plain.FindAll("h1"));
        Assert.Equal("Prices", title.Find("h1").TextContent);
        Assert.NotNull(StilBlockCatalog.Default.Find(type)!.Setting("pageTitle"));
    }

    [Fact]
    public void ToneAndAnchorComeFromTheSettings()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data,
            StilBlockData.Create("stats").Set("id", "numbers").Set("tone", StilBlockTone.Brand).Set("title", "In numbers")));

        var section = cut.Find("section");
        Assert.Equal("numbers", section.Id);
        Assert.Contains("stil-section-brand", section.ClassList);
    }

    [Fact]
    public void FeaturedTestimonialIsOutsideTheGrid()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, FullPage().Blocks[4]));

        Assert.Contains("Calm Mondays.", cut.Find(".stil-testimonial-featured").TextContent);
        Assert.Null(cut.Find(".stil-testimonial-featured").Closest(".stil-testimonials"));
        Assert.Contains("Less meetings.", cut.Find(".stil-testimonials .stil-testimonial").TextContent);
    }

    [Fact]
    public void LongTextIsSplitIntoParagraphs()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, FullPage().Blocks[9]));

        Assert.Equal(["We plan.", "We ship."], cut.FindAll(".stil-prose p").Select(p => p.TextContent));
    }

    [Fact]
    public void PlanAndPostReadTheirListsAndDate()
    {
        using var context = new BunitContext();
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
        try
        {
            var pricing = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, FullPage().Blocks[5]));
            Assert.Equal(["Plans", "Reports"], pricing.FindAll(".stil-plan li").Select(li => li.TextContent.Trim()));
            Assert.Contains("stil-plan-featured", pricing.Find(".stil-plan").ClassList);

            var posts = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, FullPage().Blocks[11]));
            Assert.Equal("blog/calm-mondays", posts.Find(".stil-post-card-title a").GetAttribute("href"));
            Assert.Equal("2026-11-03", posts.Find(".stil-post-meta time").GetAttribute("datetime"));
            Assert.Equal("Planning", posts.Find(".stil-post-tag").TextContent);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ContactFormPostsToTheFormAddress()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data,
            StilBlockData.Create("contact").Set("title", "Talk to us").Set("formAction", "contact/send")));

        var form = cut.Find("form");
        Assert.Equal("post", form.GetAttribute("method"));
        Assert.Equal("contact/send", form.GetAttribute("action"));
        Assert.Equal(["name", "email", "message"], form.QuerySelectorAll("input:not([type=hidden]), textarea").Select(field => field.GetAttribute("name")));
        foreach (var label in form.QuerySelectorAll("label"))
        {
            Assert.NotNull(form.QuerySelector($"#{label.GetAttribute("for")}"));
        }

        Assert.Equal(["Name", "E-mail", "Message"], form.QuerySelectorAll("label").Select(label => label.TextContent));
        Assert.Equal("Send", form.QuerySelector("button[type=submit]")!.TextContent);
    }

    [Fact]
    public void ContactWithoutAFormAddressHasNoForm()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, FullPage().Blocks[10]));

        Assert.Empty(cut.FindAll("form"));
        Assert.Equal("mailto:hello@example.com", cut.Find(".stil-contact a[href^='mailto:']").GetAttribute("href"));
    }

    [Fact]
    public void YourOwnTypeRendersWithItsComponent()
    {
        using var context = new BunitContext();
        var catalog = new StilBlockCatalog().Add(new StilBlockDefinition("quote", "Quote", []) { Component = typeof(QuoteBlock) });
        var page = new StilPageData().Add(StilBlockData.Create("quote").Set("text", "Less is more."));

        var direct = context.Render<StilPage>(parameters => parameters.Add(c => c.Page, page).Add(c => c.Catalog, catalog));
        Assert.Equal("Less is more.", direct.Find("blockquote.custom").TextContent);

        using var withService = new BunitContext();
        withService.Services.AddSingleton(catalog);
        var fromServices = withService.Render<StilPage>(parameters => parameters.Add(c => c.Page, page));
        Assert.Equal("Less is more.", fromServices.Find("blockquote.custom").TextContent);
    }

    [Fact]
    public void UnknownTypeRendersNothing()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBlockView>(parameters => parameters.Add(c => c.Data, StilBlockData.Create("quote")));

        Assert.Equal("", cut.Markup.Trim());
    }

    private sealed class QuoteBlock : ComponentBase
    {
        [Parameter]
        public StilBlockData Data { get; set; } = new();

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "blockquote");
            builder.AddAttribute(1, "class", "custom");
            builder.AddContent(2, Data.GetText("text"));
            builder.CloseElement();
        }
    }
}
