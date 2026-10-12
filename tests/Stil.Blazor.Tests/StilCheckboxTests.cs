using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilCheckboxTests
{
    [Fact]
    public void RendersCheckboxWrappedInItsLabel()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilCheckbox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Remember me"));

        var field = cut.Find("div.stil-field");
        var label = field.QuerySelector(":scope > label.stil-check");
        Assert.NotNull(label);
        Assert.Equal("Remember me", label.TextContent.Trim());
        Assert.False(label.HasAttribute("for"));
        Assert.Empty(cut.FindAll("label.stil-label"));

        var input = label.QuerySelector("input");
        Assert.NotNull(input);
        Assert.Equal("checkbox", input.GetAttribute("type"));
        Assert.Equal(["stil-check-input"], input.ClassList);
        Assert.StartsWith("stil-field-", input.Id);
        Assert.False(input.HasAttribute("checked"));
        Assert.False(input.HasAttribute("role"));
    }

    [Fact]
    public void CustomIdIsUsedByInputAndHint()
    {
        using var context = new BunitContext();
        bool value = true;

        var cut = context.Render<StilCheckbox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "terms")
            .Add(component => component.Hint, "Required to continue."));

        var input = cut.Find("input");
        Assert.Equal("terms", input.Id);
        Assert.True(input.HasAttribute("checked"));
        Assert.Equal("terms-hint", cut.Find(".stil-field-hint").Id);
        Assert.Equal("terms-hint", input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void ChildContentReplacesLabelText()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilCheckbox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Ignored")
            .AddChildContent("I accept the <a href=\"/terms\">terms</a>"));

        var label = cut.Find("label.stil-check");
        Assert.DoesNotContain("Ignored", label.TextContent);
        Assert.Equal("/terms", label.QuerySelector("a")?.GetAttribute("href"));
    }

    [Fact]
    public void TwoWayBindingUpdatesValueOnChange()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilCheckbox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        cut.Find("input").Change(true);
        Assert.True(value);

        cut.Find("input").Change(false);
        Assert.False(value);
    }

    [Fact]
    public void ClassAndStyleGoOnWrapperInputClassAndAttributesOnInput()
    {
        using var context = new BunitContext();
        bool value = false;

        var cut = context.Render<StilCheckbox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.InputClass, "big")
            .Add(component => component.Disabled, true)
            .AddUnmatched("class", "stil-mt-2")
            .AddUnmatched("style", "order: 3")
            .AddUnmatched("data-testid", "terms"));

        Assert.Equal(["stil-field", "stil-mt-2"], cut.Find("div.stil-field").ClassList);
        Assert.Equal("order: 3", cut.Find("div.stil-field").GetAttribute("style"));
        var input = cut.Find("input");
        Assert.Equal(["stil-check-input", "big"], input.ClassList);
        Assert.False(input.HasAttribute("style"));
        Assert.Equal("terms", input.GetAttribute("data-testid"));
        Assert.True(input.HasAttribute("disabled"));
    }
}
