using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilAlertTests
{
    [Theory]
    [InlineData(StilAlertType.Success, "alert-success")]
    [InlineData(StilAlertType.Error, "alert-error")]
    [InlineData(StilAlertType.Warning, "alert-warning")]
    [InlineData(StilAlertType.Info, "alert-info")]
    public void RendersCorrectVariantAndScopedAlert(
        StilAlertType type,
        string variantClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Type, type)
            .AddChildContent("Alert content")
            .AddUnmatched("aria-live", "polite")
            .AddUnmatched("class", "custom-alert"));

        var alert = cut.Find("[role=alert]");
        Assert.Contains("stil-scope", alert.ClassList);
        Assert.Contains("alert", alert.ClassList);
        Assert.Contains(variantClass, alert.ClassList);
        Assert.Contains("custom-alert", alert.ClassList);
        Assert.Equal("polite", alert.GetAttribute("aria-live"));
        Assert.Equal("Alert content", alert.TextContent);
    }

    [Fact]
    public void DefaultsToInfoVariant()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>();

        Assert.Contains("alert-info", cut.Find("[role=alert]").ClassList);
    }
}
