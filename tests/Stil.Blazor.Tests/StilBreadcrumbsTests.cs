using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilBreadcrumbsTests
{
    [Fact]
    public void RendersItemsWithLastAsCurrentPage()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBreadcrumbs>(parameters => parameters
            .Add(component => component.Items, [new("Home", "/"), new("Orders", "/orders"), new("Order #1042", "/orders/1042")])
            .AddUnmatched("class", "stil-mb-0"));

        var nav = cut.Find("nav");
        Assert.Equal("Breadcrumb", nav.GetAttribute("aria-label"));
        Assert.Equal(["stil-mb-0"], nav.ClassList);

        var items = cut.FindAll("ol.stil-breadcrumb > li");
        Assert.Equal(3, items.Count);
        Assert.Equal("/", items[0].QuerySelector("a")?.GetAttribute("href"));
        Assert.Equal("/orders", items[1].QuerySelector("a")?.GetAttribute("href"));

        Assert.Null(items[2].QuerySelector("a"));
        var current = items[2].QuerySelector("span");
        Assert.Equal("page", current?.GetAttribute("aria-current"));
        Assert.Equal("Order #1042", current?.TextContent);
    }

    [Fact]
    public void ItemWithoutHrefRendersText()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBreadcrumbs>(parameters => parameters
            .Add(component => component.Items, [new("Archive"), new("2026")]));

        var first = cut.FindAll("li")[0];
        Assert.Null(first.QuerySelector("a"));
        Assert.Null(first.QuerySelector("span")?.GetAttribute("aria-current"));
    }

    [Fact]
    public void ChildItemsAndCustomLabel()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBreadcrumbs>(parameters => parameters
            .Add(component => component.Label, "Du är här")
            .AddChildContent<StilBreadcrumbItem>(item => item.Add(i => i.Text, "Start").Add(i => i.Href, "/"))
            .AddChildContent<StilBreadcrumbItem>(item => item.Add(i => i.Text, "Inställningar").Add(i => i.Current, true)));

        Assert.Equal("Du är här", cut.Find("nav").GetAttribute("aria-label"));
        Assert.Equal("/", cut.Find("li a").GetAttribute("href"));
        Assert.Equal("page", cut.FindAll("li")[1].QuerySelector("span")?.GetAttribute("aria-current"));
    }

    [Fact]
    public void CurrentItemIsNeverALink()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBreadcrumbItem>(parameters => parameters
            .Add(component => component.Text, "Here")
            .Add(component => component.Href, "/here")
            .Add(component => component.Current, true));

        Assert.Empty(cut.FindAll("a"));
        Assert.Equal("page", cut.Find("span").GetAttribute("aria-current"));
    }
}
