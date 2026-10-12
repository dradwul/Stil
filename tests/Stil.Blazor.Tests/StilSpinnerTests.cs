using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilSpinnerTests
{
    [Fact]
    public void RendersStatusRoleWithVisuallyHiddenDefaultLabel()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSpinner>();

        var spinner = cut.Find(".stil-spinner");
        Assert.Equal("span", spinner.LocalName);
        Assert.Equal("status", spinner.GetAttribute("role"));
        Assert.Equal(["stil-spinner"], spinner.ClassList);
        var label = spinner.QuerySelector("span.stil-visually-hidden");
        Assert.NotNull(label);
        Assert.Equal("Loading…", label.TextContent);
    }

    [Fact]
    public void RendersCustomLabel()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSpinner>(parameters => parameters.Add(component => component.Label, "Saving"));

        Assert.Equal("Saving", cut.Find(".stil-visually-hidden").TextContent);
    }

    [Theory]
    [InlineData(StilSize.Small, true)]
    [InlineData(StilSize.Medium, false)]
    [InlineData(StilSize.Large, false)]
    public void OnlySmallAddsSmallClass(StilSize size, bool expectSmall)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSpinner>(parameters => parameters.Add(component => component.Size, size));

        Assert.Equal(expectSmall, cut.Find(".stil-spinner").ClassList.Contains("stil-spinner-sm"));
    }

    [Theory]
    [InlineData(StilVariant.Primary, "stil-text-primary")]
    [InlineData(StilVariant.Secondary, "stil-text-secondary")]
    [InlineData(StilVariant.Success, "stil-text-success")]
    [InlineData(StilVariant.Danger, "stil-text-danger")]
    [InlineData(StilVariant.Warning, "stil-text-warning")]
    [InlineData(StilVariant.Info, "stil-text-info")]
    public void VariantAddsTextColorClass(StilVariant variant, string expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSpinner>(parameters => parameters
            .Add(component => component.Variant, variant)
            .Add(component => component.Size, StilSize.Small));

        Assert.Equal(["stil-spinner", "stil-spinner-sm", expectedClass], cut.Find(".stil-spinner").ClassList);
    }

    [Fact]
    public void MergesClassAndPassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSpinner>(parameters => parameters
            .AddUnmatched("class", "me-2")
            .AddUnmatched("data-testid", "spinner"));

        var spinner = cut.Find(".stil-spinner");
        Assert.Equal(["stil-spinner", "me-2"], spinner.ClassList);
        Assert.Equal("spinner", spinner.GetAttribute("data-testid"));
    }
}
