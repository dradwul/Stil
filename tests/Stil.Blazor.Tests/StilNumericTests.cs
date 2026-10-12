using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilNumericTests
{
    [Fact]
    public void RendersNumberInputWithLabel()
    {
        using var context = new BunitContext();
        int value = 5;

        var cut = context.Render<StilNumeric<int>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Quantity"));

        var input = cut.Find("input");
        Assert.Equal("number", input.GetAttribute("type"));
        Assert.Equal(["stil-input"], input.ClassList);
        Assert.Equal("5", input.GetAttribute("value"));
        Assert.Equal(input.Id, cut.Find("label.stil-label").GetAttribute("for"));
        Assert.False(input.HasAttribute("min"));
        Assert.False(input.HasAttribute("max"));
        Assert.False(input.HasAttribute("placeholder"));
    }

    [Fact]
    public void CustomIdMinMaxStepPlaceholderAndSize()
    {
        using var context = new BunitContext();
        decimal value = 1m;

        var cut = context.Render<StilNumeric<decimal>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "price")
            .Add(component => component.Label, "Price")
            .Add(component => component.Min, "0")
            .Add(component => component.Max, "1000")
            .Add(component => component.Step, "0.01")
            .Add(component => component.Placeholder, "0.00")
            .Add(component => component.Size, StilSize.Small));

        var input = cut.Find("input");
        Assert.Equal("price", input.Id);
        Assert.Equal("price", cut.Find("label").GetAttribute("for"));
        Assert.Equal("0", input.GetAttribute("min"));
        Assert.Equal("1000", input.GetAttribute("max"));
        Assert.Equal("0.01", input.GetAttribute("step"));
        Assert.Equal("0.00", input.GetAttribute("placeholder"));
        Assert.Equal(["stil-input", "stil-input-sm"], input.ClassList);
    }

    [Fact]
    public void DecimalTypesKeepStepAnyByDefault()
    {
        using var context = new BunitContext();
        double value = 1.5;

        var cut = context.Render<StilNumeric<double>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        Assert.Equal("any", cut.Find("input").GetAttribute("step"));
    }

    [Fact]
    public void HintIsReferencedByAriaDescribedBy()
    {
        using var context = new BunitContext();
        int value = 0;

        var cut = context.Render<StilNumeric<int>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "qty")
            .Add(component => component.Hint, "Between 1 and 10."));

        Assert.Equal("qty-hint", cut.Find(".stil-field-hint").Id);
        Assert.Equal("qty-hint", cut.Find("input").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwoWayBindingParsesIntOnChange()
    {
        using var context = new BunitContext();
        int value = 0;

        var cut = context.Render<StilNumeric<int>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        cut.Find("input").Change("42");

        Assert.Equal(42, value);
    }

    [Fact]
    public void TwoWayBindingParsesNullableDecimalOnChange()
    {
        using var context = new BunitContext();
        decimal? value = null;

        var cut = context.Render<StilNumeric<decimal?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        cut.Find("input").Change("12.5");
        Assert.Equal(12.5m, value);

        cut.Find("input").Change("");
        Assert.Null(value);
    }

    [Fact]
    public void ClassAndStyleGoOnWrapperInputClassAndAttributesOnInput()
    {
        using var context = new BunitContext();
        int value = 0;

        var cut = context.Render<StilNumeric<int>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.InputClass, "text-end")
            .Add(component => component.Disabled, true)
            .AddUnmatched("class", "stil-col-3")
            .AddUnmatched("style", "max-width: 10rem")
            .AddUnmatched("inputmode", "numeric"));

        Assert.Equal(["stil-field", "stil-col-3"], cut.Find("div.stil-field").ClassList);
        Assert.Equal("max-width: 10rem", cut.Find("div.stil-field").GetAttribute("style"));
        var input = cut.Find("input");
        Assert.Equal(["stil-input", "text-end"], input.ClassList);
        Assert.False(input.HasAttribute("style"));
        Assert.Equal("numeric", input.GetAttribute("inputmode"));
        Assert.True(input.HasAttribute("disabled"));
    }
}
