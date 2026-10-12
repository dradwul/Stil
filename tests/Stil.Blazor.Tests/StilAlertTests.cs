using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilAlertTests
{
    [Theory]
    [InlineData(StilVariant.Primary, "stil-alert-primary")]
    [InlineData(StilVariant.Secondary, "stil-alert-secondary")]
    [InlineData(StilVariant.Success, "stil-alert-success")]
    [InlineData(StilVariant.Danger, "stil-alert-danger")]
    [InlineData(StilVariant.Warning, "stil-alert-warning")]
    [InlineData(StilVariant.Info, "stil-alert-info")]
    public void RendersVariantClassAndPassesAttributesThrough(
        StilVariant variant,
        string variantClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Variant, variant)
            .AddChildContent("Alert content")
            .AddUnmatched("aria-live", "polite")
            .AddUnmatched("class", "custom-alert"));

        var alert = cut.Find("[role=alert]");
        Assert.Equal(["stil-alert", variantClass, "custom-alert"], alert.ClassList);
        Assert.Equal("polite", alert.GetAttribute("aria-live"));
        Assert.Equal("Alert content", alert.TextContent);
    }

    [Fact]
    public void DefaultsToInfoVariant()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>();

        Assert.Contains("stil-alert-info", cut.Find("[role=alert]").ClassList);
    }
}
