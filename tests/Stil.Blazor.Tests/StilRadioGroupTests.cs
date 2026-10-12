using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilRadioGroupTests
{
    [Fact]
    public void RendersFieldsetWithLegendAndOneRadioPerEnumValue()
    {
        using var context = new BunitContext();
        Plan value = Plan.Pro;

        var cut = context.Render<StilRadioGroup<Plan>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Plan"));

        var fieldset = cut.Find("fieldset");
        Assert.Equal(["stil-field"], fieldset.ClassList);
        Assert.StartsWith("stil-field-", fieldset.Id);
        Assert.False(fieldset.HasAttribute("aria-describedby"));

        var legend = fieldset.QuerySelector("legend");
        Assert.NotNull(legend);
        Assert.Equal(["stil-label"], legend.ClassList);
        Assert.Equal("Plan", legend.TextContent);

        var labels = cut.FindAll("label.stil-check");
        Assert.Equal(["Free", "Pro", "Enterprise"], labels.Select(label => label.TextContent.Trim()));

        var radios = cut.FindAll("input[type=radio]");
        Assert.Equal(3, radios.Count);
        Assert.All(radios, radio =>
        {
            Assert.Equal(["stil-check-input"], radio.ClassList);
            Assert.Equal(fieldset.Id, radio.GetAttribute("name"));
            Assert.False(radio.HasAttribute("disabled"));
        });
        Assert.Equal([false, true, false], radios.Select(radio => radio.HasAttribute("checked")));
    }

    [Fact]
    public void OptionsAreStackedByDefaultAndInRowWhenInline()
    {
        using var context = new BunitContext();
        Plan value = Plan.Free;

        var stacked = context.Render<StilRadioGroup<Plan>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));
        var inline = context.Render<StilRadioGroup<Plan>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Inline, true));

        Assert.Equal(["stil-stack", "stil-gap-2"], stacked.Find("fieldset > div").ClassList);
        Assert.Equal(["stil-cluster", "stil-gap-4"], inline.Find("fieldset > div").ClassList);
    }

    [Fact]
    public void OmitsLegendWhenLabelIsEmpty()
    {
        using var context = new BunitContext();
        Plan value = Plan.Free;

        var cut = context.Render<StilRadioGroup<Plan>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        Assert.Empty(cut.FindAll("legend"));
    }

    [Fact]
    public void ItemsAndItemTextRenderCustomOptions()
    {
        using var context = new BunitContext();
        string? value = "m";

        var cut = context.Render<StilRadioGroup<string?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Items, new string?[] { "s", "m", "l" })
            .Add(component => component.ItemText, (Func<string?, string>)(size => $"Size {size?.ToUpperInvariant()}")));

        Assert.Equal(["Size S", "Size M", "Size L"], cut.FindAll("label.stil-check").Select(label => label.TextContent.Trim()));
        Assert.Equal(["s", "m", "l"], cut.FindAll("input[type=radio]").Select(radio => radio.GetAttribute("value")));
        Assert.True(cut.FindAll("input[type=radio]")[1].HasAttribute("checked"));
    }

    [Fact]
    public void NullableEnumGetsEnumOptions()
    {
        using var context = new BunitContext();
        Plan? value = null;

        var cut = context.Render<StilRadioGroup<Plan?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        var radios = cut.FindAll("input[type=radio]");
        Assert.Equal(3, radios.Count);
        Assert.DoesNotContain(radios, radio => radio.HasAttribute("checked"));
    }

    [Fact]
    public void TwoWayBindingUpdatesValueOnChange()
    {
        using var context = new BunitContext();
        Plan? value = null;

        var cut = context.Render<StilRadioGroup<Plan?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        var enterprise = cut.FindAll("input[type=radio]")[2];
        enterprise.Change(enterprise.GetAttribute("value"));

        Assert.Equal(Plan.Enterprise, value);
    }

    [Fact]
    public void CustomIdHintAndAriaDescribedBy()
    {
        using var context = new BunitContext();
        Plan value = Plan.Free;

        var cut = context.Render<StilRadioGroup<Plan>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "plan")
            .Add(component => component.Hint, "You can change it later."));

        var fieldset = cut.Find("fieldset");
        Assert.Equal("plan", fieldset.Id);
        Assert.All(cut.FindAll("input[type=radio]"), radio => Assert.Equal("plan", radio.GetAttribute("name")));
        var hint = fieldset.QuerySelector(".stil-field-hint");
        Assert.NotNull(hint);
        Assert.Equal("plan-hint", hint.Id);
        Assert.Equal("plan-hint", fieldset.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void DisabledDisablesEveryRadio()
    {
        using var context = new BunitContext();
        Plan value = Plan.Free;

        var cut = context.Render<StilRadioGroup<Plan>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Disabled, true));

        Assert.All(cut.FindAll("input[type=radio]"), radio => Assert.True(radio.HasAttribute("disabled")));
    }

    [Fact]
    public void ClassAndAttributesGoOnFieldsetAndInputClassOnEveryRadio()
    {
        using var context = new BunitContext();
        Plan value = Plan.Free;

        var cut = context.Render<StilRadioGroup<Plan>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.InputClass, "big")
            .AddUnmatched("class", "stil-col-6")
            .AddUnmatched("style", "order: 2")
            .AddUnmatched("data-testid", "plan"));

        var fieldset = cut.Find("fieldset");
        Assert.Equal(["stil-field", "stil-col-6"], fieldset.ClassList);
        Assert.Equal("order: 2", fieldset.GetAttribute("style"));
        Assert.Equal("plan", fieldset.GetAttribute("data-testid"));
        Assert.All(cut.FindAll("input[type=radio]"), radio => Assert.Equal(["stil-check-input", "big"], radio.ClassList));
    }
}
