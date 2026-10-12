using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilListTests
{
    [Fact]
    public void RendersListWithItems()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilList>(parameters => parameters
            .AddChildContent<StilListItem>(item => item.Add(component => component.Title, "First"))
            .AddChildContent<StilListItem>(item => item.Add(component => component.Title, "Second"))
            .AddUnmatched("class", "mb-3")
            .AddUnmatched("aria-label", "Items"));

        var list = cut.Find(".stil-list");
        Assert.Equal("div", list.LocalName);
        Assert.Equal(["stil-list", "mb-3"], list.ClassList);
        Assert.Equal("Items", list.GetAttribute("aria-label"));
        Assert.Equal(["First", "Second"], list.QuerySelectorAll(".stil-list-item-title").Select(e => e.TextContent));
    }

    [Fact]
    public void FlushAddsFlushClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilList>(parameters => parameters.Add(component => component.Flush, true));

        Assert.Equal(["stil-list", "stil-list-flush"], cut.Find("div").ClassList);
    }
}

public class StilListItemTests
{
    [Fact]
    public void RendersDivWithTitleAndDescriptionByDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Title, "Ada Lovelace")
            .Add(component => component.Description, "Mathematician"));

        var item = cut.Find(".stil-list-item");
        Assert.Equal("div", item.LocalName);
        Assert.Equal(["stil-list-item"], item.ClassList);
        Assert.False(item.HasAttribute("aria-current"));
        Assert.False(item.HasAttribute("aria-disabled"));

        var content = item.QuerySelector(".stil-list-item-content");
        Assert.NotNull(content);
        Assert.Equal("Ada Lovelace", content.QuerySelector(".stil-list-item-title")?.TextContent);
        Assert.Equal("Mathematician", content.QuerySelector(".stil-list-item-description")?.TextContent);
        Assert.Empty(cut.FindAll(".stil-list-item-start"));
        Assert.Empty(cut.FindAll(".stil-list-item-end"));
    }

    [Fact]
    public void OmitsEmptyTitleAndDescription()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Title, " ")
            .Add(component => component.Description, "")
            .AddChildContent("<em>Extra</em>"));

        Assert.Empty(cut.FindAll(".stil-list-item-title"));
        Assert.Empty(cut.FindAll(".stil-list-item-description"));
        Assert.Equal("Extra", cut.Find(".stil-list-item-content em").TextContent);
    }

    [Fact]
    public void RendersStartContentAndEndSlotsInOrder()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Title, "Ada")
            .Add(component => component.Start, "<span class=\"avatar\">AL</span>")
            .Add(component => component.End, "<button class=\"action\">Edit</button>")
            .AddChildContent("<small>Joined 1843</small>"));

        var item = cut.Find(".stil-list-item");
        Assert.Equal(
            ["stil-list-item-start", "stil-list-item-content", "stil-list-item-end"],
            item.Children.Select(child => child.ClassName));
        Assert.Equal("AL", item.QuerySelector(".stil-list-item-start .avatar")?.TextContent);
        Assert.Equal("Joined 1843", item.QuerySelector(".stil-list-item-content small")?.TextContent);
        Assert.Equal("Edit", item.QuerySelector(".stil-list-item-end .action")?.TextContent);
    }

    [Fact]
    public void BadgeShortcutRendersSecondaryBadgeAtTheEnd()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Title, "Order 42")
            .Add(component => component.Badge, "Pending"));

        var badge = cut.Find(".stil-list-item-end .stil-badge");
        Assert.Equal(["stil-badge", "stil-badge-secondary"], badge.ClassList);
        Assert.Equal("Pending", badge.TextContent);
    }

    [Fact]
    public void BadgeVariantAndSubtleAreApplied()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Badge, "Paid")
            .Add(component => component.BadgeVariant, StilVariant.Success)
            .Add(component => component.BadgeSubtle, true));

        Assert.Equal(["stil-badge", "stil-badge-success", "stil-badge-subtle"], cut.Find(".stil-badge").ClassList);
    }

    [Fact]
    public void BadgeRendersBeforeEndContent()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Badge, "Paid")
            .Add(component => component.End, "<button class=\"action\">Refund</button>"));

        var end = cut.Find(".stil-list-item-end");
        Assert.Equal(2, end.ChildElementCount);
        Assert.Contains("stil-badge", end.Children[0].ClassList);
        Assert.Equal("action", end.Children[1].ClassName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OmitsEndWhenNoBadgeAndNoEnd(string? badge)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters.Add(component => component.Badge, badge));

        Assert.Empty(cut.FindAll(".stil-list-item-end"));
        Assert.Empty(cut.FindAll(".stil-badge"));
    }

    [Fact]
    public void HrefRendersLink()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Title, "Settings")
            .Add(component => component.Href, "/settings"));

        var item = cut.Find(".stil-list-item");
        Assert.Equal("a", item.LocalName);
        Assert.Equal("/settings", item.GetAttribute("href"));
        Assert.False(item.HasAttribute("aria-disabled"));
    }

    [Fact]
    public void DisabledLinkRemovesHrefAndSetsAriaDisabled()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Href, "/settings")
            .Add(component => component.Disabled, true));

        var item = cut.Find("a.stil-list-item");
        Assert.False(item.HasAttribute("href"));
        Assert.Equal("true", item.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void OnClickRendersButtonAndInvokesCallback()
    {
        using var context = new BunitContext();
        var clicks = 0;

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Title, "Open")
            .Add(component => component.OnClick, (MouseEventArgs _) => clicks++));

        var item = cut.Find(".stil-list-item");
        Assert.Equal("button", item.LocalName);
        Assert.Equal("button", item.GetAttribute("type"));
        Assert.False(item.HasAttribute("disabled"));

        item.Click();

        Assert.Equal(1, clicks);
    }

    [Fact]
    public void DisabledButtonRendersDisabledAttribute()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.OnClick, (MouseEventArgs _) => { })
            .Add(component => component.Disabled, true));

        Assert.True(cut.Find("button.stil-list-item").HasAttribute("disabled"));
    }

    [Fact]
    public void HrefTakesPrecedenceOverOnClick()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .Add(component => component.Href, "/a")
            .Add(component => component.OnClick, (MouseEventArgs _) => { }));

        Assert.Equal("a", cut.Find(".stil-list-item").LocalName);
    }

    [Theory]
    [InlineData("div", null, false)]
    [InlineData("a", "/item", false)]
    [InlineData("button", null, true)]
    public void SelectedSetsAriaCurrentOnEveryElementType(string expectedElement, string? href, bool clickable)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters =>
        {
            parameters
                .Add(component => component.Selected, true)
                .Add(component => component.Href, href);
            if (clickable)
            {
                parameters.Add(component => component.OnClick, (MouseEventArgs _) => { });
            }
        });

        var item = cut.Find(".stil-list-item");
        Assert.Equal(expectedElement, item.LocalName);
        Assert.Equal("true", item.GetAttribute("aria-current"));
    }

    [Fact]
    public void MergesClassAndPassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilListItem>(parameters => parameters
            .AddUnmatched("class", "active-row")
            .AddUnmatched("data-id", "7"));

        var item = cut.Find(".stil-list-item");
        Assert.Equal(["stil-list-item", "active-row"], item.ClassList);
        Assert.Equal("7", item.GetAttribute("data-id"));
    }
}
