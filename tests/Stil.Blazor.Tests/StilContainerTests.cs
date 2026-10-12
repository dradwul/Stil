using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilContainerTests
{
    [Fact]
    public void RendersChildContentInsideContainer()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilContainer>(parameters => parameters
            .AddChildContent("<p>Container content</p>")
            .AddUnmatched("data-testid", "container")
            .AddUnmatched("class", "custom-container"));

        var container = cut.Find("div.stil-container");
        Assert.Equal(["stil-container", "custom-container"], container.ClassList);
        Assert.Equal("container", container.GetAttribute("data-testid"));
        Assert.Equal("Container content", container.QuerySelector("p")?.TextContent);
    }
}
