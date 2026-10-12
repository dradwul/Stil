using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilTextBoxTests
{
    [Fact]
    public void RendersFieldWithLabelLinkedToGeneratedId()
    {
        using var context = new BunitContext();
        string? value = "Ada";

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Name"));

        var field = cut.Find("div.stil-field");
        Assert.Equal(["stil-field"], field.ClassList);
        Assert.False(field.HasAttribute("style"));

        var input = cut.Find("input");
        var label = cut.Find("label.stil-label");
        Assert.StartsWith("stil-field-", input.Id);
        Assert.Equal(input.Id, label.GetAttribute("for"));
        Assert.Equal("Name", label.TextContent);
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Equal(["stil-input"], input.ClassList);
        Assert.Equal("Ada", input.GetAttribute("value"));
        Assert.False(input.HasAttribute("aria-describedby"));
        Assert.False(input.HasAttribute("aria-invalid"));
        Assert.False(input.HasAttribute("disabled"));
        Assert.Empty(cut.FindAll(".stil-field-hint"));
        Assert.Empty(cut.FindAll(".stil-field-error"));
    }

    [Fact]
    public void GeneratedIdsAreUniquePerInstance()
    {
        using var context = new BunitContext();
        string? value = null;

        var first = context.Render<StilTextBox>(parameters => parameters.Bind(component => component.Value, value, v => value = v, () => value));
        var second = context.Render<StilTextBox>(parameters => parameters.Bind(component => component.Value, value, v => value = v, () => value));

        Assert.NotEqual(first.Find("input").Id, second.Find("input").Id);
    }

    [Fact]
    public void CustomIdIsUsedByInputLabelAndHint()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "email")
            .Add(component => component.Label, "Email")
            .Add(component => component.Hint, "We never share it."));

        Assert.Equal("email", cut.Find("input").Id);
        Assert.Equal("email", cut.Find("label").GetAttribute("for"));
        Assert.Equal("email-hint", cut.Find(".stil-field-hint").Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OmitsLabelWhenEmpty(string? label)
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, label));

        Assert.Empty(cut.FindAll("label"));
    }

    [Fact]
    public void HintRendersAndIsReferencedByAriaDescribedBy()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Hint, "At least 8 characters."));

        var input = cut.Find("input");
        var hint = cut.Find(".stil-field-hint");
        Assert.Equal("At least 8 characters.", hint.TextContent);
        Assert.Equal($"{input.Id}-hint", hint.Id);
        Assert.Equal(hint.Id, input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwoWayBindingUpdatesValueOnChange()
    {
        using var context = new BunitContext();
        string? value = "Ada";

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        cut.Find("input").Change("Grace");

        Assert.Equal("Grace", value);
        Assert.Equal("Grace", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void TypePlaceholderAndSizeAreApplied()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Type, "email")
            .Add(component => component.Placeholder, "you@example.com")
            .Add(component => component.Size, StilSize.Small));

        var input = cut.Find("input");
        Assert.Equal("email", input.GetAttribute("type"));
        Assert.Equal("you@example.com", input.GetAttribute("placeholder"));
        Assert.Equal(["stil-input", "stil-input-sm"], input.ClassList);
    }

    [Fact]
    public void LargeSizeAddsLargeClass()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Size, StilSize.Large));

        Assert.Equal(["stil-input", "stil-input-lg"], cut.Find("input").ClassList);
    }

    [Fact]
    public void DisabledDisablesInput()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Disabled, true));

        Assert.True(cut.Find("input").HasAttribute("disabled"));
    }

    [Fact]
    public void ClassAndStyleGoOnFieldWrapperAndOtherAttributesOnInput()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .AddUnmatched("class", "stil-col-6")
            .AddUnmatched("style", "grid-row: 2")
            .AddUnmatched("autocomplete", "email")
            .AddUnmatched("aria-label", "Work email")
            .AddUnmatched("data-testid", "email-input"));

        var field = cut.Find("div.stil-field");
        Assert.Equal(["stil-field", "stil-col-6"], field.ClassList);
        Assert.Equal("grid-row: 2", field.GetAttribute("style"));
        Assert.False(field.HasAttribute("autocomplete"));

        var input = cut.Find("input");
        Assert.Equal(["stil-input"], input.ClassList);
        Assert.False(input.HasAttribute("style"));
        Assert.Equal("email", input.GetAttribute("autocomplete"));
        Assert.Equal("Work email", input.GetAttribute("aria-label"));
        Assert.Equal("email-input", input.GetAttribute("data-testid"));
    }

    [Fact]
    public void InputClassIsAddedToTheInput()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Size, StilSize.Large)
            .Add(component => component.InputClass, "stil-font-mono text-end")
            .AddUnmatched("class", "stil-col-6"));

        Assert.Equal(["stil-input", "stil-input-lg", "stil-font-mono", "text-end"], cut.Find("input").ClassList);
        Assert.Equal(["stil-field", "stil-col-6"], cut.Find("div.stil-field").ClassList);
    }

    [Fact]
    public void ConsumerAriaDescribedByIsKeptWithoutHint()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .AddUnmatched("aria-describedby", "external-help"));

        Assert.Equal("external-help", cut.Find("input").GetAttribute("aria-describedby"));
    }
}
