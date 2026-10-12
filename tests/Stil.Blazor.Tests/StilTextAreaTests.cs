using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilTextAreaTests
{
    [Fact]
    public void RendersTextareaWithLabelAndDefaultRows()
    {
        using var context = new BunitContext();
        string? value = "Hello";

        var cut = context.Render<StilTextArea>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Bio"));

        var textarea = cut.Find("textarea");
        Assert.Equal(textarea.Id, cut.Find("label.stil-label").GetAttribute("for"));
        Assert.Equal(["stil-input"], textarea.ClassList);
        Assert.Equal("3", textarea.GetAttribute("rows"));
        Assert.Equal("Hello", textarea.GetAttribute("value") ?? textarea.TextContent);
    }

    [Fact]
    public void CustomIdRowsAndPlaceholder()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextArea>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "bio")
            .Add(component => component.Label, "Bio")
            .Add(component => component.Rows, 6)
            .Add(component => component.Placeholder, "Tell us about yourself"));

        var textarea = cut.Find("textarea");
        Assert.Equal("bio", textarea.Id);
        Assert.Equal("bio", cut.Find("label").GetAttribute("for"));
        Assert.Equal("6", textarea.GetAttribute("rows"));
        Assert.Equal("Tell us about yourself", textarea.GetAttribute("placeholder"));
    }

    [Fact]
    public void HintIsReferencedByAriaDescribedBy()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextArea>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "bio")
            .Add(component => component.Hint, "Markdown is supported."));

        Assert.Equal("bio-hint", cut.Find(".stil-field-hint").Id);
        Assert.Equal("bio-hint", cut.Find("textarea").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwoWayBindingUpdatesValueOnChange()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextArea>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        cut.Find("textarea").Change("Line one\nLine two");

        Assert.Equal("Line one\nLine two", value);
    }

    [Fact]
    public void ClassAndStyleGoOnWrapperInputClassAndAttributesOnTextarea()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextArea>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.InputClass, "resize-none")
            .Add(component => component.Disabled, true)
            .AddUnmatched("class", "stil-col-12")
            .AddUnmatched("style", "margin: 0")
            .AddUnmatched("maxlength", "500"));

        var field = cut.Find("div.stil-field");
        Assert.Equal(["stil-field", "stil-col-12"], field.ClassList);
        Assert.Equal("margin: 0", field.GetAttribute("style"));

        var textarea = cut.Find("textarea");
        Assert.Equal(["stil-input", "resize-none"], textarea.ClassList);
        Assert.False(textarea.HasAttribute("style"));
        Assert.Equal("500", textarea.GetAttribute("maxlength"));
        Assert.True(textarea.HasAttribute("disabled"));
    }
}
