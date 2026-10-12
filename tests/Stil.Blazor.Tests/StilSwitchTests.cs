using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilSwitchTests
{
    [Fact]
    public void RendersCheckboxWithSwitchRole()
    {
        using var context = new BunitContext();
        bool value = true;

        var cut = context.Render<StilSwitch>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Dark mode"));

        var label = cut.Find("div.stil-field > label.stil-check");
        Assert.Equal("Dark mode", label.TextContent.Trim());

        var input = label.QuerySelector("input");
        Assert.NotNull(input);
        Assert.Equal("checkbox", input.GetAttribute("type"));
        Assert.Equal("switch", input.GetAttribute("role"));
        Assert.Equal(["stil-switch"], input.ClassList);
        Assert.True(input.HasAttribute("checked"));
    }

    [Fact]
    public void CustomIdAndHint()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilSwitch>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "notify")
            .Add(component => component.Hint, "Email me about replies."));

        var input = cut.Find("input");
        Assert.Equal("notify", input.Id);
        Assert.Equal("notify-hint", cut.Find(".stil-field-hint").Id);
        Assert.Equal("notify-hint", input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void ChildContentReplacesLabelText()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilSwitch>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .AddChildContent("<strong>Beta</strong> features"));

        Assert.Equal("Beta", cut.Find("label.stil-check strong").TextContent);
    }

    [Fact]
    public void TwoWayBindingUpdatesValueOnChange()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilSwitch>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        cut.Find("input").Change(true);

        Assert.True(value);
    }

    [Fact]
    public void ClassAndStyleGoOnWrapperInputClassAndAttributesOnInput()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilSwitch>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.InputClass, "lg")
            .Add(component => component.Disabled, true)
            .AddUnmatched("class", "stil-mb-0")
            .AddUnmatched("style", "order: 1")
            .AddUnmatched("aria-label", "Dark mode"));

        Assert.Equal(["stil-field", "stil-mb-0"], cut.Find("div.stil-field").ClassList);
        Assert.Equal("order: 1", cut.Find("div.stil-field").GetAttribute("style"));
        var input = cut.Find("input");
        Assert.Equal(["stil-switch", "lg"], input.ClassList);
        Assert.Equal("switch", input.GetAttribute("role"));
        Assert.False(input.HasAttribute("style"));
        Assert.Equal("Dark mode", input.GetAttribute("aria-label"));
        Assert.True(input.HasAttribute("disabled"));
    }
}
