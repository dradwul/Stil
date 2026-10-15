using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilPaginationTests
{
    private static string[] Labels(IRenderedComponent<StilPagination> cut) =>
        cut.FindAll("li > .stil-page-link").Select(link => link.TextContent.Trim()).ToArray();

    [Theory]
    [InlineData(1, 10, "1,2,3,4,5,…,10")]
    [InlineData(5, 10, "1,…,4,5,6,…,10")]
    [InlineData(10, 10, "1,…,6,7,8,9,10")]
    [InlineData(4, 10, "1,2,3,4,5,…,10")]
    [InlineData(2, 5, "1,2,3,4,5")]
    [InlineData(1, 1, "1")]
    public void PageItemsKeepFirstLastAndAWindowAroundTheCurrentPage(int page, int count, string expected)
    {
        var items = StilPagination.GetPageItems(page, count, siblings: 1);

        Assert.Equal(expected, string.Join(',', items.Select(item => item?.ToString() ?? "…")));
    }

    [Fact]
    public void MoreSiblingsWidenTheWindow()
    {
        var items = StilPagination.GetPageItems(10, 20, siblings: 2);

        Assert.Equal("1,…,8,9,10,11,12,…,20", string.Join(',', items.Select(item => item?.ToString() ?? "…")));
    }

    [Fact]
    public void RendersNavigationWithCurrentPage()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 3)
            .Add(component => component.PageCount, 5));

        Assert.Equal("Pagination", cut.Find("nav").GetAttribute("aria-label"));
        Assert.Equal(["stil-pagination"], cut.Find("ul").ClassList);
        Assert.Equal(["Previous page", "1", "2", "3", "4", "5", "Next page"], Labels(cut));
        Assert.Equal("3", cut.Find("[aria-current=page]").TextContent.Trim());
        Assert.Single(cut.FindAll("[aria-current]"));
    }

    [Fact]
    public void PageCountComesFromTotalItemsAndPageSize()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.TotalItems, 48)
            .Add(component => component.PageSize, 10));

        Assert.Equal(["Previous page", "1", "2", "3", "4", "5", "Next page"], Labels(cut));
    }

    [Fact]
    public void PreviousIsDisabledOnFirstPageAndNextOnLast()
    {
        using var context = new BunitContext();

        var first = context.Render<StilPagination>(parameters => parameters.Add(c => c.Page, 1).Add(c => c.PageCount, 3));
        Assert.True(first.Find("button:first-child, li:first-child button").HasAttribute("disabled"));
        Assert.False(first.FindAll("li").Last().QuerySelector("button")!.HasAttribute("disabled"));

        var last = context.Render<StilPagination>(parameters => parameters.Add(c => c.Page, 3).Add(c => c.PageCount, 3));
        Assert.True(last.FindAll("li").Last().QuerySelector("button")!.HasAttribute("disabled"));
    }

    [Fact]
    public void ClickingAPageRaisesPageChanged()
    {
        using var context = new BunitContext();
        var page = 1;

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.PageCount, 5)
            .Add(component => component.PageChanged, (int value) => page = value));

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "4").Click();
        Assert.Equal(4, page);

        cut.FindAll("button").Last().Click();
        Assert.Equal(5, page);
    }

    [Fact]
    public void ClickingTheCurrentPageDoesNothing()
    {
        using var context = new BunitContext();
        var changes = 0;

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 2)
            .Add(component => component.PageCount, 5)
            .Add(component => component.PageChanged, (int _) => changes++));

        cut.Find("[aria-current=page]").Click();

        Assert.Equal(0, changes);
    }

    [Fact]
    public void FirstAndLastButtons()
    {
        using var context = new BunitContext();
        var page = 3;

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, page)
            .Add(component => component.PageCount, 9)
            .Add(component => component.ShowFirstLast, true)
            .Add(component => component.PageChanged, (int value) => page = value));

        var labels = Labels(cut);
        Assert.Equal("First page", labels[0]);
        Assert.Equal("Last page", labels[^1]);

        cut.FindAll("button")[^1].Click();
        Assert.Equal(9, page);
    }

    [Fact]
    public void HrefTemplateRendersLinks()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 1)
            .Add(component => component.PageCount, 3)
            .Add(component => component.HrefTemplate, "orders?page={0}"));

        Assert.Empty(cut.FindAll("button"));
        var links = cut.FindAll("a.stil-page-link");
        Assert.Null(links[0].GetAttribute("href"));
        Assert.Equal("true", links[0].GetAttribute("aria-disabled"));
        Assert.Equal("orders?page=2", links[2].GetAttribute("href"));
        Assert.Equal("orders?page=2", links[^1].GetAttribute("href"));
    }

    [Fact]
    public void EllipsisIsHiddenFromScreenReaders()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 5)
            .Add(component => component.PageCount, 20));

        var ellipses = cut.FindAll("span.stil-page-link");
        Assert.Equal(2, ellipses.Count);
        Assert.All(ellipses, span => Assert.Equal("true", span.GetAttribute("aria-hidden")));
    }

    [Fact]
    public void SmallSizeAndOutOfRangePageIsClamped()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 99)
            .Add(component => component.PageCount, 4)
            .Add(component => component.Size, StilSize.Small));

        Assert.Contains("stil-pagination-sm", cut.Find("ul").ClassList);
        Assert.Equal("4", cut.Find("[aria-current=page]").TextContent.Trim());
    }

    [Fact]
    public void NavigationButtonsUseIconsWithHiddenText()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters.Add(c => c.PageCount, 3));

        var previous = cut.FindAll("button")[0];
        Assert.NotNull(previous.QuerySelector("svg.stil-icon[aria-hidden=true]"));
        Assert.Equal("Previous page", previous.QuerySelector(".stil-visually-hidden")?.TextContent);
    }
}
