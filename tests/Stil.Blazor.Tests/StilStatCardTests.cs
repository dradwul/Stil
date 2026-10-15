using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilStatCardTests
{
    [Fact]
    public void RendersLabelValueAndCardClasses()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Label, "Revenue")
            .Add(component => component.Value, "$48,200"));

        var root = cut.Find("div.stil-stat");
        Assert.Equal(["stil-card", "stil-stat"], root.ClassList);
        Assert.Equal("Revenue", cut.Find(".stil-stat-header > .stil-stat-label").TextContent);
        Assert.Equal("$48,200", cut.Find(".stil-stat-value").TextContent);
        Assert.Empty(cut.FindAll(".stil-stat-footer"));
        Assert.Empty(cut.FindAll("svg"));
    }

    [Fact]
    public void IconIsDecorative()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Label, "Users")
            .Add(component => component.Icon, StilIcons.Home));

        var icon = cut.Find(".stil-stat-header > svg.stil-stat-icon");
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
    }

    [Theory]
    [InlineData(StilTrendDirection.Up, "+12.4%", "stil-badge-success", "up", "up 12.4%")]
    [InlineData(StilTrendDirection.Down, "-3%", "stil-badge-danger", "down", "down 3%")]
    [InlineData(StilTrendDirection.Down, "−3%", "stil-badge-danger", "down", "down 3%")]
    [InlineData(StilTrendDirection.Neutral, "0%", "stil-badge-secondary", "neutral", "unchanged 0%")]
    public void TrendShowsBadgeArrowAndSpokenText(StilTrendDirection direction, string trend, string variantClass, string data, string spoken)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Value, "42")
            .Add(component => component.Trend, trend)
            .Add(component => component.TrendDirection, direction));

        var badge = cut.Find(".stil-stat-footer > .stil-stat-trend");
        Assert.Equal(["stil-badge", variantClass, "stil-badge-subtle", "stil-stat-trend"], badge.ClassList);
        Assert.Equal(data, badge.GetAttribute("data-trend"));
        var arrow = badge.QuerySelector("svg.stil-icon")!;
        Assert.Equal("true", arrow.GetAttribute("aria-hidden"));
        Assert.Equal("true", badge.QuerySelector("span[aria-hidden]")!.GetAttribute("aria-hidden"));
        Assert.Equal(trend, badge.QuerySelector("span[aria-hidden]")!.TextContent);
        Assert.Equal(spoken, badge.QuerySelector(".stil-visually-hidden")!.TextContent);
    }

    [Fact]
    public void TrendVariantOverridesDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Trend, "+5%")
            .Add(component => component.TrendDirection, StilTrendDirection.Up)
            .Add(component => component.TrendVariant, StilVariant.Danger));

        Assert.Contains("stil-badge-danger", cut.Find(".stil-stat-trend").ClassList);
        Assert.DoesNotContain("stil-badge-success", cut.Find(".stil-stat-trend").ClassList);
    }

    [Fact]
    public void TrendLabelAndDirectionWordsAreCustomizable()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Trend, "+5%")
            .Add(component => component.TrendDirection, StilTrendDirection.Up)
            .Add(component => component.UpText, "upp"));
        Assert.Equal("upp 5%", cut.Find(".stil-visually-hidden").TextContent);

        cut.Render(parameters => parameters.Add(component => component.TrendLabel, "Ökning med 5 procent"));
        Assert.Equal("Ökning med 5 procent", cut.Find(".stil-visually-hidden").TextContent);
    }

    [Fact]
    public void DescriptionFollowsTrend()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Label, "Revenue")
            .Add(component => component.Value, "$48,200")
            .Add(component => component.Trend, "+12.4%")
            .Add(component => component.TrendDirection, StilTrendDirection.Up)
            .Add(component => component.Description, "vs last month"));

        var footer = cut.Find(".stil-stat-footer");
        Assert.Equal("vs last month", footer.QuerySelector(".stil-stat-description")!.TextContent);
        Assert.Equal(["stil-stat-trend", "stil-stat-description"], footer.Children.Select(c => c.ClassList.Last()));

        // What a screen reader reads, in order, without the hidden visual trend.
        var spoken = string.Join(' ', cut.Find(".stil-stat").QuerySelectorAll(".stil-stat-label, .stil-stat-value, .stil-visually-hidden, .stil-stat-description").Select(e => e.TextContent));
        Assert.Equal("Revenue $48,200 up 12.4% vs last month", spoken);
    }

    [Fact]
    public void DescriptionWithoutTrend()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Description, "Today"));

        Assert.Empty(cut.FindAll(".stil-stat-trend"));
        Assert.Equal("Today", cut.Find(".stil-stat-description").TextContent);
    }

    [Fact]
    public void HrefMakesTheCardALink()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Href, "/orders")
            .Add(component => component.Label, "Orders")
            .Add(component => component.Value, "128"));

        var link = cut.Find("a.stil-stat");
        Assert.Equal("/orders", link.GetAttribute("href"));
        Assert.Equal(["stil-card", "stil-stat"], link.ClassList);
        Assert.Empty(cut.FindAll("div.stil-stat"));
    }

    [Fact]
    public void ChildContentAndAttributes()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilStatCard>(parameters => parameters
            .Add(component => component.Value, "1")
            .AddChildContent("<svg class=\"sparkline\"></svg>")
            .AddUnmatched("class", "stil-col-3")
            .AddUnmatched("id", "kpi"));

        var root = cut.Find("#kpi");
        Assert.Equal(["stil-card", "stil-stat", "stil-col-3"], root.ClassList);
        Assert.NotNull(root.QuerySelector(":scope > svg.sparkline"));
    }
}
