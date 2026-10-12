using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilDatePickerTests
{
    [Fact]
    public void RendersDateInputWithLabel()
    {
        using var context = new BunitContext();
        DateOnly value = new(2024, 5, 17);

        var cut = context.Render<StilDatePicker<DateOnly>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Start"));

        var input = cut.Find("input");
        Assert.Equal("date", input.GetAttribute("type"));
        Assert.Equal(["stil-input"], input.ClassList);
        Assert.Equal("2024-05-17", input.GetAttribute("value"));
        Assert.Equal(input.Id, cut.Find("label.stil-label").GetAttribute("for"));
    }

    [Theory]
    [InlineData(InputDateType.DateTimeLocal, "datetime-local")]
    [InlineData(InputDateType.Month, "month")]
    [InlineData(InputDateType.Time, "time")]
    public void TypeChangesInputType(InputDateType type, string expected)
    {
        using var context = new BunitContext();
        DateTime value = new(2024, 5, 17, 13, 45, 0);

        var cut = context.Render<StilDatePicker<DateTime>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Type, type));

        Assert.Equal(expected, cut.Find("input").GetAttribute("type"));
    }

    [Fact]
    public void CustomIdMinMaxAndSize()
    {
        using var context = new BunitContext();
        DateOnly? value = null;

        var cut = context.Render<StilDatePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "start")
            .Add(component => component.Label, "Start")
            .Add(component => component.Min, "2024-01-01")
            .Add(component => component.Max, "2024-12-31")
            .Add(component => component.Size, StilSize.Large));

        var input = cut.Find("input");
        Assert.Equal("start", input.Id);
        Assert.Equal("start", cut.Find("label").GetAttribute("for"));
        Assert.Equal("2024-01-01", input.GetAttribute("min"));
        Assert.Equal("2024-12-31", input.GetAttribute("max"));
        Assert.Equal(["stil-input", "stil-input-lg"], input.ClassList);
    }

    [Fact]
    public void HintIsReferencedByAriaDescribedBy()
    {
        using var context = new BunitContext();
        DateOnly? value = null;

        var cut = context.Render<StilDatePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "start")
            .Add(component => component.Hint, "The first day."));

        Assert.Equal("start-hint", cut.Find(".stil-field-hint").Id);
        Assert.Equal("start-hint", cut.Find("input").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TwoWayBindingParsesDateOnChange()
    {
        using var context = new BunitContext();
        DateOnly? value = null;

        var cut = context.Render<StilDatePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        cut.Find("input").Change("2025-02-03");

        Assert.Equal(new DateOnly(2025, 2, 3), value);
    }

    [Fact]
    public void ClassAndStyleGoOnWrapperInputClassAndAttributesOnInput()
    {
        using var context = new BunitContext();
        DateOnly? value = null;

        var cut = context.Render<StilDatePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.InputClass, "w-auto")
            .Add(component => component.Disabled, true)
            .AddUnmatched("class", "stil-col-4")
            .AddUnmatched("style", "order: 2")
            .AddUnmatched("aria-label", "Start date"));

        Assert.Equal(["stil-field", "stil-col-4"], cut.Find("div.stil-field").ClassList);
        Assert.Equal("order: 2", cut.Find("div.stil-field").GetAttribute("style"));
        var input = cut.Find("input");
        Assert.Equal(["stil-input", "w-auto"], input.ClassList);
        Assert.False(input.HasAttribute("style"));
        Assert.Equal("Start date", input.GetAttribute("aria-label"));
        Assert.True(input.HasAttribute("disabled"));
    }
}
