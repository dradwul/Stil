using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilFieldTests
{
    [Fact]
    public void WrapsCustomControlWithLabelAndHint()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilField>(parameters => parameters
            .Add(component => component.Label, "Color")
            .Add(component => component.InputId, "color")
            .Add(component => component.Hint, "Pick any color.")
            .AddChildContent("<input id=\"color\" type=\"color\" aria-describedby=\"color-hint\" />"));

        var field = cut.Find("div");
        Assert.Equal(["stil-field"], field.ClassList);
        Assert.False(field.HasAttribute("style"));
        Assert.Equal(["label", "input", "div"], field.Children.Select(child => child.LocalName));

        var label = cut.Find("label");
        Assert.Equal(["stil-label"], label.ClassList);
        Assert.Equal("color", label.GetAttribute("for"));
        Assert.Equal("Color", label.TextContent);

        var hint = cut.Find(".stil-field-hint");
        Assert.Equal("color-hint", hint.Id);
        Assert.Equal("Pick any color.", hint.TextContent);
        Assert.Empty(cut.FindAll(".stil-field-error"));
    }

    [Fact]
    public void RendersEachValidationMessage()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilField>(parameters => parameters
            .Add(component => component.InputId, "code")
            .Add(component => component.Messages, new[] { "Too short.", "Must be numeric." }));

        var error = cut.Find(".stil-field-error");
        Assert.Equal("code-error", error.Id);
        Assert.Equal(["Too short.", "Must be numeric."], error.Children.Select(child => child.TextContent));
        Assert.Empty(cut.FindAll("label"));
    }

    [Fact]
    public void EmptyMessagesRenderNoErrorElement()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilField>(parameters => parameters
            .Add(component => component.InputId, "code")
            .Add(component => component.Messages, Array.Empty<string>()));

        Assert.Empty(cut.FindAll(".stil-field-error"));
    }

    [Fact]
    public void ClassAndStyleParametersGoOnWrapper()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilField>(parameters => parameters
            .Add(component => component.Class, "stil-col-6 custom")
            .Add(component => component.Style, "grid-row: 1"));

        var field = cut.Find("div");
        Assert.Equal(["stil-field", "stil-col-6", "custom"], field.ClassList);
        Assert.Equal("grid-row: 1", field.GetAttribute("style"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WhitespaceClassIsIgnored(string? cssClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilField>(parameters => parameters.Add(component => component.Class, cssClass));

        Assert.Equal("stil-field", cut.Find("div").GetAttribute("class"));
    }
}
