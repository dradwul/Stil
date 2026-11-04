using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilProgressTests
{
    [Fact]
    public void RendersProgressbarWithAriaValues()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters
            .Add(component => component.Value, 40)
            .Add(component => component.Label, "Upload"));

        var progress = cut.Find(".stil-progress");
        Assert.Equal("progressbar", progress.GetAttribute("role"));
        Assert.Equal("0", progress.GetAttribute("aria-valuemin"));
        Assert.Equal("100", progress.GetAttribute("aria-valuemax"));
        Assert.Equal("40", progress.GetAttribute("aria-valuenow"));
        Assert.Equal("Upload", progress.GetAttribute("aria-label"));
        Assert.Equal(["stil-progress"], progress.ClassList);

        var bar = progress.QuerySelector(".stil-progress-bar");
        Assert.NotNull(bar);
        Assert.Equal(["stil-progress-bar"], bar.ClassList);
        Assert.Equal("--stil-progress-value: 40%", bar.GetAttribute("style"));
        Assert.Equal("", bar.TextContent);
    }

    [Fact]
    public void ComputesPercentFromCustomMax()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters
            .Add(component => component.Value, 3)
            .Add(component => component.Max, 8));

        var progress = cut.Find(".stil-progress");
        Assert.Equal("8", progress.GetAttribute("aria-valuemax"));
        Assert.Equal("3", progress.GetAttribute("aria-valuenow"));
        Assert.Equal("--stil-progress-value: 37.5%", cut.Find(".stil-progress-bar").GetAttribute("style"));
    }

    [Theory]
    [InlineData(150, "100", "--stil-progress-value: 100%")]
    [InlineData(-20, "0", "--stil-progress-value: 0%")]
    public void ClampsValueBetweenZeroAndMax(double value, string expectedNow, string expectedStyle)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters.Add(component => component.Value, value));

        Assert.Equal(expectedNow, cut.Find(".stil-progress").GetAttribute("aria-valuenow"));
        Assert.Equal(expectedStyle, cut.Find(".stil-progress-bar").GetAttribute("style"));
    }

    [Fact]
    public void FormatsFractionsWithInvariantCulture()
    {
        using var context = new BunitContext();
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("sv-SE");
        try
        {
            var cut = context.Render<StilProgress>(parameters => parameters
                .Add(component => component.Value, 1.25)
                .Add(component => component.Max, 2.5));

            Assert.Equal("1.25", cut.Find(".stil-progress").GetAttribute("aria-valuenow"));
            Assert.Equal("2.5", cut.Find(".stil-progress").GetAttribute("aria-valuemax"));
            Assert.Equal("--stil-progress-value: 50%", cut.Find(".stil-progress-bar").GetAttribute("style"));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ZeroMaxRendersEmptyBar()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters
            .Add(component => component.Value, 5)
            .Add(component => component.Max, 0));

        Assert.Equal("0", cut.Find(".stil-progress").GetAttribute("aria-valuenow"));
        Assert.Equal("--stil-progress-value: 0%", cut.Find(".stil-progress-bar").GetAttribute("style"));
    }

    [Fact]
    public void NegativeMaxRendersEmptyBarInsteadOfThrowing()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters
            .Add(component => component.Value, 5)
            .Add(component => component.Max, -10));

        Assert.Equal("--stil-progress-value: 0%", cut.Find(".stil-progress-bar").GetAttribute("style"));
    }

    [Theory]
    [InlineData(42, "42%")]
    [InlineData(33.3, "33%")]
    [InlineData(66.7, "67%")]
    public void ShowValueRendersRoundedPercentInsideBar(double value, string expectedText)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.ShowValue, true));

        Assert.Equal(expectedText, cut.Find(".stil-progress-bar").TextContent);
    }

    [Theory]
    [InlineData(StilVariant.Primary, "stil-bg-primary")]
    [InlineData(StilVariant.Secondary, "stil-bg-secondary")]
    [InlineData(StilVariant.Success, "stil-bg-success")]
    [InlineData(StilVariant.Danger, "stil-bg-danger")]
    [InlineData(StilVariant.Warning, "stil-bg-warning")]
    [InlineData(StilVariant.Info, "stil-bg-info")]
    public void VariantAddsBackgroundClassToBar(StilVariant variant, string expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters.Add(component => component.Variant, variant));

        Assert.Equal(["stil-progress-bar", expectedClass], cut.Find(".stil-progress-bar").ClassList);
    }

    [Theory]
    [InlineData(StilSize.Large, true)]
    [InlineData(StilSize.Medium, false)]
    [InlineData(StilSize.Small, false)]
    public void OnlyLargeAddsLargeClass(StilSize size, bool expectLarge)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters.Add(component => component.Size, size));

        Assert.Equal(expectLarge, cut.Find(".stil-progress").ClassList.Contains("stil-progress-lg"));
    }

    [Fact]
    public void MergesClassAndPassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilProgress>(parameters => parameters
            .Add(component => component.Size, StilSize.Large)
            .AddUnmatched("class", "mb-3")
            .AddUnmatched("aria-describedby", "upload-hint"));

        var progress = cut.Find(".stil-progress");
        Assert.Equal(["stil-progress", "stil-progress-lg", "mb-3"], progress.ClassList);
        Assert.Equal("upload-hint", progress.GetAttribute("aria-describedby"));
    }
}
