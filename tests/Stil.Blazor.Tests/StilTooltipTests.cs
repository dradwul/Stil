using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilTooltipTests
{
    [Fact]
    public void WrapsContentInInlineBlockWithTooltipText()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTooltip>(parameters => parameters
            .Add(component => component.Text, "Copy to clipboard")
            .AddChildContent("<button aria-label=\"Copy\">⧉</button>"));

        var wrapper = cut.Find("span");
        Assert.Equal("Copy to clipboard", wrapper.GetAttribute("data-stil-tooltip"));
        Assert.Equal(["stil-d-inline-block"], wrapper.ClassList);
        Assert.Equal("Copy", wrapper.QuerySelector("button")?.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(StilTooltipPosition.Top, false)]
    [InlineData(StilTooltipPosition.Bottom, true)]
    public void BottomPositionAddsBottomClass(StilTooltipPosition position, bool expectBottom)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTooltip>(parameters => parameters
            .Add(component => component.Text, "Tip")
            .Add(component => component.Position, position));

        Assert.Equal(expectBottom, cut.Find("span").ClassList.Contains("stil-tooltip-bottom"));
    }

    [Fact]
    public void MergesClassAndPassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTooltip>(parameters => parameters
            .Add(component => component.Text, "Tip")
            .Add(component => component.Position, StilTooltipPosition.Bottom)
            .AddUnmatched("class", "ms-1")
            .AddUnmatched("id", "tip"));

        var wrapper = cut.Find("span");
        Assert.Equal(["stil-d-inline-block", "stil-tooltip-bottom", "ms-1"], wrapper.ClassList);
        Assert.Equal("tip", wrapper.Id);
    }
}
