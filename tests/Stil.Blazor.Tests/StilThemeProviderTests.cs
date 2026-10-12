using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilThemeProviderTests
{
    [Fact]
    public void RendersWrapperWithoutModeOrStyleByDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilThemeProvider>(parameters => parameters.AddChildContent("<p>Inside</p>"));

        var wrapper = cut.Find("div");
        Assert.False(wrapper.HasAttribute("data-stil-theme"));
        Assert.False(wrapper.HasAttribute("style"));
        Assert.Equal("Inside", wrapper.QuerySelector("p")?.TextContent);
    }

    [Theory]
    [InlineData(StilThemeMode.Light, "light")]
    [InlineData(StilThemeMode.Dark, "dark")]
    [InlineData(StilThemeMode.Auto, "auto")]
    public void ModeSetsDataStilTheme(StilThemeMode mode, string expected)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilThemeProvider>(parameters => parameters.Add(component => component.Mode, mode));

        Assert.Equal(expected, cut.Find("div").GetAttribute("data-stil-theme"));
    }

    [Fact]
    public void ThemeWritesInlineCustomProperties()
    {
        using var context = new BunitContext();
        var theme = new StilTheme { Primary = "#6741d9", Radius = "0.25rem" };

        var cut = context.Render<StilThemeProvider>(parameters => parameters.Add(component => component.Theme, theme));

        Assert.Equal("--stil-color-primary: #6741d9; --stil-radius-md: 0.25rem;", cut.Find("div").GetAttribute("style"));
    }

    [Fact]
    public void ThemeDeclarationsComeBeforeConsumerStyle()
    {
        using var context = new BunitContext();
        var theme = new StilTheme { Danger = "#c92a2a" };

        var cut = context.Render<StilThemeProvider>(parameters => parameters
            .Add(component => component.Theme, theme)
            .Add(component => component.Mode, StilThemeMode.Dark)
            .AddUnmatched("style", "padding: 1rem")
            .AddUnmatched("class", "panel")
            .AddUnmatched("id", "themed"));

        var wrapper = cut.Find("div");
        Assert.Equal("--stil-color-danger: #c92a2a; padding: 1rem", wrapper.GetAttribute("style"));
        Assert.Equal("dark", wrapper.GetAttribute("data-stil-theme"));
        Assert.Equal("panel", wrapper.GetAttribute("class"));
        Assert.Equal("themed", wrapper.Id);
    }

    [Fact]
    public void ConsumerStyleIsKeptWithoutTheme()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilThemeProvider>(parameters => parameters.AddUnmatched("style", "margin: 0"));

        Assert.Equal("margin: 0", cut.Find("div").GetAttribute("style"));
    }

    [Fact]
    public void EmptyThemeRendersNoStyle()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilThemeProvider>(parameters => parameters.Add(component => component.Theme, new StilTheme()));

        Assert.False(cut.Find("div").HasAttribute("style"));
    }
}
