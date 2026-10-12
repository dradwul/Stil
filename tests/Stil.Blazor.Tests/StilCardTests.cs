using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilCardTests
{
    [Fact]
    public void RendersTitleAndChildContentInsideCard()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters
            .Add(component => component.Title, "Card heading")
            .AddChildContent("<p>Card content</p>")
            .AddUnmatched("data-testid", "card")
            .AddUnmatched("class", "custom-card"));

        var card = cut.Find("div.stil-card");
        Assert.Equal(["stil-card", "custom-card"], card.ClassList);
        Assert.Equal("card", card.GetAttribute("data-testid"));
        Assert.Equal("Card heading", card.QuerySelector("h2.stil-card-title")?.TextContent);
        Assert.Equal("Card content", card.QuerySelector("p")?.TextContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OmitsEmptyHeader(string? headerText)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters =>
            parameters.Add(component => component.Title, headerText));

        Assert.Empty(cut.FindAll("h2"));
    }

    [Fact]
    public void RendersSubtitleAfterTitle()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters
            .Add(component => component.Title, "Profile")
            .Add(component => component.Subtitle, "Your public details"));

        var card = cut.Find(".stil-card");
        Assert.Equal(2, card.ChildElementCount);
        Assert.Equal("h2", card.Children[0].LocalName);
        var subtitle = card.Children[1];
        Assert.Equal("p", subtitle.LocalName);
        Assert.Equal(["stil-card-subtitle"], subtitle.ClassList);
        Assert.Equal("Your public details", subtitle.TextContent);
    }

    [Fact]
    public void RendersSubtitleWithoutTitle()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters.Add(component => component.Subtitle, "Only a subtitle"));

        Assert.Empty(cut.FindAll(".stil-card-title"));
        Assert.Equal("Only a subtitle", cut.Find(".stil-card-subtitle").TextContent);
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(3, "h3")]
    [InlineData(6, "h6")]
    [InlineData(9, "h6")]
    [InlineData(0, "h1")]
    public void HeadingLevelChangesTitleElement(int level, string expectedElement)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters
            .Add(component => component.Title, "Card heading")
            .Add(component => component.HeadingLevel, level));

        var heading = cut.Find(".stil-card-title");
        Assert.Equal(expectedElement, heading.LocalName);
        Assert.Equal("Card heading", heading.TextContent);
    }

    [Fact]
    public void HeaderActionsMoveTitleIntoHeaderBar()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters
            .Add(component => component.Title, "Orders")
            .Add(component => component.Subtitle, "Last 30 days")
            .Add(component => component.HeaderActions, "<button>Export</button>")
            .AddChildContent("<p>Body</p>"));

        var card = cut.Find(".stil-card");
        var header = card.FirstElementChild;
        Assert.NotNull(header);
        Assert.Equal(["stil-card-header"], header.ClassList);
        Assert.Equal(2, header.ChildElementCount);

        var titleBlock = header.Children[0];
        Assert.Equal("Orders", titleBlock.QuerySelector("h2.stil-card-title")?.TextContent);
        Assert.Equal("Last 30 days", titleBlock.QuerySelector("p.stil-card-subtitle")?.TextContent);

        var actions = header.Children[1];
        Assert.Equal(["stil-cluster"], actions.ClassList);
        Assert.Equal("Export", actions.QuerySelector("button")?.TextContent);

        Assert.Equal("Body", card.QuerySelector(":scope > p")?.TextContent);
    }

    [Fact]
    public void WithoutHeaderActionsThereIsNoHeaderBar()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters.Add(component => component.Title, "Orders"));

        Assert.Empty(cut.FindAll(".stil-card-header"));
        Assert.Equal(cut.Find(".stil-card"), cut.Find(".stil-card-title").ParentElement);
    }

    [Fact]
    public void FooterRendersAfterContent()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters
            .AddChildContent("<p>Body</p>")
            .Add(component => component.Footer, "<button>Save</button>"));

        var card = cut.Find(".stil-card");
        var footer = card.LastElementChild;
        Assert.NotNull(footer);
        Assert.Equal(["stil-card-footer"], footer.ClassList);
        Assert.Equal("Save", footer.QuerySelector("button")?.TextContent);
    }

    [Fact]
    public void OmitsFooterWhenNotSet()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters.AddChildContent("<p>Body</p>"));

        Assert.Empty(cut.FindAll(".stil-card-footer"));
    }
}
