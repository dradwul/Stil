using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilHeadingTests
{
    [Fact]
    public void RendersH2ByDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilHeading>(parameters => parameters.AddChildContent("Title"));

        Assert.Equal("h2", cut.Find("h2").LocalName);
        Assert.Equal("Title", cut.Find("h2").TextContent);
    }

    [Theory]
    [InlineData(1, "h1")]
    [InlineData(3, "h3")]
    [InlineData(6, "h6")]
    [InlineData(0, "h1")]
    [InlineData(-4, "h1")]
    [InlineData(7, "h6")]
    [InlineData(42, "h6")]
    public void ClampsLevelBetweenOneAndSix(int level, string expectedElement)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilHeading>(parameters => parameters
            .Add(component => component.Level, level)
            .AddChildContent("Title"));

        Assert.Single(cut.FindAll(expectedElement));
    }

    [Fact]
    public void PassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilHeading>(parameters => parameters
            .Add(component => component.Level, 4)
            .AddUnmatched("class", "stil-card-title")
            .AddUnmatched("id", "heading"));

        var heading = cut.Find("h4");
        Assert.Equal(["stil-card-title"], heading.ClassList);
        Assert.Equal("heading", heading.Id);
    }
}
