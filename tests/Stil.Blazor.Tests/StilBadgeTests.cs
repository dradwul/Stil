using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilBadgeTests
{
    [Fact]
    public void RendersPrimaryBadgeByDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBadge>(parameters => parameters.AddChildContent("New"));

        var badge = cut.Find("span");
        Assert.Equal(["stil-badge", "stil-badge-primary"], badge.ClassList);
        Assert.Equal("New", badge.TextContent);
    }

    [Theory]
    [InlineData(StilVariant.Primary, "stil-badge-primary")]
    [InlineData(StilVariant.Secondary, "stil-badge-secondary")]
    [InlineData(StilVariant.Success, "stil-badge-success")]
    [InlineData(StilVariant.Danger, "stil-badge-danger")]
    [InlineData(StilVariant.Warning, "stil-badge-warning")]
    [InlineData(StilVariant.Info, "stil-badge-info")]
    public void RendersVariantClass(StilVariant variant, string expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBadge>(parameters => parameters.Add(component => component.Variant, variant));

        Assert.Equal(["stil-badge", expectedClass], cut.Find("span").ClassList);
    }

    [Fact]
    public void SubtleAddsSubtleClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBadge>(parameters => parameters
            .Add(component => component.Variant, StilVariant.Success)
            .Add(component => component.Subtle, true));

        Assert.Equal(["stil-badge", "stil-badge-success", "stil-badge-subtle"], cut.Find("span").ClassList);
    }

    [Fact]
    public void MergesClassAndPassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilBadge>(parameters => parameters
            .AddUnmatched("class", "ms-2")
            .AddUnmatched("aria-label", "3 unread"));

        var badge = cut.Find("span");
        Assert.Equal(["stil-badge", "stil-badge-primary", "ms-2"], badge.ClassList);
        Assert.Equal("3 unread", badge.GetAttribute("aria-label"));
    }
}
