using System.Globalization;
using Bunit;
using Stil.Blazor.Blocks;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilBlogBlocksTests
{
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-GB");

    private static T InCulture<T>(CultureInfo culture, Func<T> render)
    {
        var (previous, previousUi) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
        try
        {
            return render();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    [Fact]
    public void PostCardIsAnArticleWhoseTitleLinksToThePost()
    {
        using var context = new BunitContext();

        var cut = InCulture(English, () => context.Render<StilPostList>(parameters => parameters
            .AddChildContent<StilPostCard>(post => post
                .Add(c => c.Href, "blog/calm-mondays")
                .Add(c => c.Title, "Five rules for a calm Monday")
                .Add(c => c.Excerpt, "Plan on Friday, not on Monday.")
                .Add(c => c.Image, "monday.jpg")
                .Add(c => c.Tags, [new StilLinkItem("Planning", "blog/tags/planning"), new StilLinkItem("Teams")])
                .Add(c => c.Author, "Maja Lind")
                .Add(c => c.Date, new DateTime(2026, 11, 3))
                .Add(c => c.ReadingTime, "5 min read"))));

        var card = cut.Find("ul.stil-posts > li > article.stil-post-card");
        Assert.Equal("", card.QuerySelector("img.stil-post-card-image")!.GetAttribute("alt"));
        var title = card.QuerySelector("h2.stil-post-card-title > a")!;
        Assert.Equal("blog/calm-mondays", title.GetAttribute("href"));
        Assert.Equal("Five rules for a calm Monday", title.TextContent);
        Assert.Equal("Tags", card.QuerySelector("ul.stil-post-tags")!.GetAttribute("aria-label"));
        Assert.Equal("blog/tags/planning", card.QuerySelector("a.stil-post-tag")!.GetAttribute("href"));
        Assert.Equal("Teams", card.QuerySelector("span.stil-post-tag")!.TextContent);
        Assert.Equal("Plan on Friday, not on Monday.", card.QuerySelector(".stil-post-card-excerpt")!.TextContent);
        var time = card.QuerySelector(".stil-post-meta time")!;
        Assert.Equal("2026-11-03", time.GetAttribute("datetime"));
        Assert.Equal("3 November 2026", time.TextContent);
        Assert.Equal(["Maja Lind", "3 November 2026", "5 min read"], card.QuerySelectorAll(".stil-post-meta > *").Select(e => e.TextContent));
    }

    [Fact]
    public void FeaturedPostCardIsMarked()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPostCard>(parameters => parameters
            .Add(c => c.Href, "post")
            .Add(c => c.Title, "Post")
            .Add(c => c.Featured, true));

        Assert.Equal("stil-post-card stil-post-card-featured", cut.Find("article").ClassName);
        Assert.Empty(cut.FindAll(".stil-post-meta, .stil-post-tags, img"));
    }

    [Fact]
    public void ArticleHasAHeaderPictureContentsAndBody()
    {
        using var context = new BunitContext();

        var cut = InCulture(English, () => context.Render<StilArticle>(parameters => parameters
            .Add(c => c.Eyebrow, "Blog")
            .Add(c => c.EyebrowHref, "blog")
            .Add(c => c.Title, "Five rules for a calm Monday")
            .Add(c => c.Lead, "Plan on Friday.")
            .Add(c => c.Author, "Maja Lind")
            .Add(c => c.Date, new DateTime(2026, 11, 3))
            .Add(c => c.Image, "monday.jpg")
            .Add(c => c.ImageAlt, "A calendar on a desk")
            .Add(c => c.Caption, "Monday, planned.")
            .Add(c => c.Contents, [new StilLinkItem("Why Mondays go wrong", "#why")])
            .Add(c => c.Tags, [new StilLinkItem("Planning", "blog/tags/planning")])
            .AddChildContent("<h2 id=\"why\">Why Mondays go wrong</h2><p>Because…</p>")));

        var article = cut.Find("article.stil-article");
        Assert.Equal("blog", article.QuerySelector(".stil-article-header .stil-eyebrow > a")!.GetAttribute("href"));
        Assert.Equal("Five rules for a calm Monday", article.QuerySelector("h1.stil-article-title")!.TextContent);
        Assert.Equal("Plan on Friday.", article.QuerySelector(".stil-article-header .stil-lead")!.TextContent);
        Assert.Equal("3 November 2026", article.QuerySelector(".stil-article-header time")!.TextContent);
        Assert.Equal("A calendar on a desk", article.QuerySelector("figure.stil-article-image > img")!.GetAttribute("alt"));
        Assert.Equal("Monday, planned.", article.QuerySelector("figcaption")!.TextContent);
        var toc = article.QuerySelector("nav.stil-toc")!;
        Assert.Equal("On this page", toc.QuerySelector(".stil-toc-title")!.TextContent);
        Assert.Equal(toc.QuerySelector(".stil-toc-title")!.Id, toc.GetAttribute("aria-labelledby"));
        Assert.Equal("#why", toc.QuerySelector("ol.stil-toc-list a")!.GetAttribute("href"));
        Assert.NotNull(article.QuerySelector(".stil-article-layout > .stil-article-body.stil-prose > h2#why"));
        Assert.Equal("Planning", article.QuerySelector("footer.stil-article-footer .stil-post-tag")!.TextContent);
    }

    [Fact]
    public void ArticleTextsAndDatesFollowTheCulture()
    {
        using var context = new BunitContext();

        var cut = InCulture(CultureInfo.GetCultureInfo("sv-SE"), () => context.Render<StilArticle>(parameters => parameters
            .Add(c => c.Title, "Fem regler")
            .Add(c => c.Date, new DateTime(2026, 11, 3))
            .Add(c => c.Contents, [new StilLinkItem("Varför", "#varfor")])
            .Add(c => c.Tags, [new StilLinkItem("Planering")])));

        Assert.Equal("3 november 2026", cut.Find("time").TextContent);
        Assert.Equal("På den här sidan", cut.Find(".stil-toc-title").TextContent);
        Assert.Equal("Taggar", cut.Find(".stil-post-tags").GetAttribute("aria-label"));
    }
}
