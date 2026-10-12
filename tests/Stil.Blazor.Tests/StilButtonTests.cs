using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilButtonTests
{
    [Fact]
    public void RendersPrimarySolidButtonByDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters.AddChildContent("Save"));

        var button = cut.Find("button");
        Assert.Equal(["stil-btn", "stil-btn-primary"], button.ClassList);
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Equal("Save", button.TextContent);
        Assert.False(button.HasAttribute("disabled"));
        Assert.False(button.HasAttribute("aria-busy"));
        Assert.False(button.HasAttribute("aria-pressed"));
    }

    [Theory]
    [InlineData(StilVariant.Primary, "stil-btn-primary")]
    [InlineData(StilVariant.Secondary, "stil-btn-secondary")]
    [InlineData(StilVariant.Success, "stil-btn-success")]
    [InlineData(StilVariant.Danger, "stil-btn-danger")]
    [InlineData(StilVariant.Warning, "stil-btn-warning")]
    [InlineData(StilVariant.Info, "stil-btn-info")]
    public void RendersVariantClass(StilVariant variant, string expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters.Add(component => component.Variant, variant));

        Assert.Equal(["stil-btn", expectedClass], cut.Find("button").ClassList);
    }

    [Theory]
    [InlineData(StilButtonAppearance.Solid, null)]
    [InlineData(StilButtonAppearance.Outline, "stil-btn-outline")]
    [InlineData(StilButtonAppearance.Ghost, "stil-btn-ghost")]
    public void RendersAppearanceClass(StilButtonAppearance appearance, string? expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Variant, StilVariant.Danger)
            .Add(component => component.Appearance, appearance));

        string[] expected = expectedClass is null
            ? ["stil-btn", "stil-btn-danger"]
            : ["stil-btn", "stil-btn-danger", expectedClass];
        Assert.Equal(expected, cut.Find("button").ClassList);
    }

    [Theory]
    [InlineData(StilSize.Medium, null)]
    [InlineData(StilSize.Small, "stil-btn-sm")]
    [InlineData(StilSize.Large, "stil-btn-lg")]
    public void RendersSizeClass(StilSize size, string? expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters.Add(component => component.Size, size));

        var classes = cut.Find("button").ClassList;
        if (expectedClass is null)
        {
            Assert.DoesNotContain(classes, c => c.EndsWith("-sm") || c.EndsWith("-lg"));
        }
        else
        {
            Assert.Contains(expectedClass, classes);
        }
    }

    [Fact]
    public void IconOnlyAddsIconClassAfterSize()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.IconOnly, true)
            .Add(component => component.Size, StilSize.Small)
            .Add(component => component.Appearance, StilButtonAppearance.Ghost)
            .AddUnmatched("aria-label", "Delete"));

        var button = cut.Find("button");
        Assert.Equal(["stil-btn", "stil-btn-primary", "stil-btn-ghost", "stil-btn-sm", "stil-btn-icon"], button.ClassList);
        Assert.Equal("Delete", button.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData("submit")]
    [InlineData("reset")]
    public void RendersGivenType(string type)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters.Add(component => component.Type, type));

        Assert.Equal(type, cut.Find("button").GetAttribute("type"));
    }

    [Fact]
    public void ClickInvokesOnClick()
    {
        using var context = new BunitContext();
        MouseEventArgs? received = null;

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.OnClick, (MouseEventArgs args) => received = args));

        cut.Find("button").Click(new MouseEventArgs { Detail = 2 });

        Assert.NotNull(received);
        Assert.Equal(2, received.Detail);
    }

    [Fact]
    public void DisabledRendersDisabledAttributeAndIgnoresClicks()
    {
        using var context = new BunitContext();
        var clicks = 0;

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Disabled, true)
            .Add(component => component.OnClick, () => clicks++));

        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.False(button.HasAttribute("aria-disabled"));

        // A browser does not dispatch clicks on a disabled button; the component also guards against it.
        button.Click();
        Assert.Equal(0, clicks);
    }

    [Fact]
    public void LoadingRendersAriaBusyAndDoesNotInvokeOnClick()
    {
        using var context = new BunitContext();
        var clicks = 0;

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Loading, true)
            .Add(component => component.OnClick, () => clicks++)
            .AddChildContent("Saving"));

        var button = cut.Find("button");
        Assert.Equal("true", button.GetAttribute("aria-busy"));
        Assert.False(button.HasAttribute("disabled"));
        Assert.Equal("Saving", button.TextContent);

        button.Click();

        Assert.Equal(0, clicks);
    }

    [Fact]
    public void LoadingFalseOmitsAriaBusy()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters.Add(component => component.Loading, false));

        Assert.False(cut.Find("button").HasAttribute("aria-busy"));
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(null, null)]
    public void PressedRendersAriaPressed(bool? pressed, string? expected)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters.Add(component => component.Pressed, pressed));

        Assert.Equal(expected, cut.Find("button").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void HrefRendersLinkStyledAsButton()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Href, "/docs")
            .Add(component => component.Target, "_blank")
            .Add(component => component.Variant, StilVariant.Secondary)
            .AddChildContent("Docs"));

        Assert.Empty(cut.FindAll("button"));
        var link = cut.Find("a");
        Assert.Equal("/docs", link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Equal(["stil-btn", "stil-btn-secondary"], link.ClassList);
        Assert.False(link.HasAttribute("type"));
        Assert.False(link.HasAttribute("role"));
        Assert.False(link.HasAttribute("aria-disabled"));
        Assert.Equal("Docs", link.TextContent);
    }

    [Fact]
    public void DisabledLinkRemovesHrefAndSetsAriaDisabled()
    {
        using var context = new BunitContext();
        var clicks = 0;

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Href, "/docs")
            .Add(component => component.Disabled, true)
            .Add(component => component.OnClick, () => clicks++));

        var link = cut.Find("a");
        Assert.False(link.HasAttribute("href"));
        Assert.Equal("true", link.GetAttribute("aria-disabled"));
        Assert.Equal("link", link.GetAttribute("role"));

        link.Click();

        Assert.Equal(0, clicks);
    }

    [Fact]
    public void LinkSupportsLoadingAndPressed()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Href, "/docs")
            .Add(component => component.Loading, true)
            .Add(component => component.Pressed, true));

        var link = cut.Find("a");
        Assert.Equal("true", link.GetAttribute("aria-busy"));
        Assert.Equal("true", link.GetAttribute("aria-pressed"));
    }

    [Fact]
    public void LinkClickInvokesOnClick()
    {
        using var context = new BunitContext();
        var clicks = 0;

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Href, "/docs")
            .Add(component => component.OnClick, () => clicks++));

        cut.Find("a").Click();

        Assert.Equal(1, clicks);
    }

    [Fact]
    public void MergesClassAndPassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButton>(parameters => parameters
            .Add(component => component.Variant, StilVariant.Success)
            .AddUnmatched("class", "w-100 custom")
            .AddUnmatched("id", "save")
            .AddUnmatched("data-testid", "save-button")
            .AddUnmatched("title", "Save the form"));

        var button = cut.Find("button");
        Assert.Equal(["stil-btn", "stil-btn-success", "w-100", "custom"], button.ClassList);
        Assert.Equal("save", button.Id);
        Assert.Equal("save-button", button.GetAttribute("data-testid"));
        Assert.Equal("Save the form", button.GetAttribute("title"));
    }
}
