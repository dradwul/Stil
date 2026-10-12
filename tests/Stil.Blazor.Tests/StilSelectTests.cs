using System.Globalization;
using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilSelectTests
{
    [Fact]
    public void GeneratesOneOptionPerEnumValue()
    {
        using var context = new BunitContext();
        Color value = Color.Green;

        var cut = context.Render<StilSelect<Color>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Label, "Color"));

        var select = cut.Find("select");
        Assert.Equal(["stil-select"], select.ClassList);
        Assert.Equal(select.Id, cut.Find("label.stil-label").GetAttribute("for"));
        var options = cut.FindAll("option");
        Assert.Equal(["Red", "Green", "Blue"], options.Select(option => option.GetAttribute("value")));
        Assert.Equal(["Red", "Green", "Blue"], options.Select(option => option.TextContent));
        Assert.Equal("Green", select.GetAttribute("value"));
    }

    [Fact]
    public void NullableEnumWithPlaceholderStartsWithEmptyOption()
    {
        using var context = new BunitContext();
        Color? value = null;

        var cut = context.Render<StilSelect<Color?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Placeholder, "Choose…"));

        var options = cut.FindAll("option");
        Assert.Equal(4, options.Count);
        Assert.Equal("", options[0].GetAttribute("value"));
        Assert.Equal("Choose…", options[0].TextContent);
        Assert.Equal(["Red", "Green", "Blue"], options.Skip(1).Select(option => option.GetAttribute("value")));
    }

    [Fact]
    public void NoPlaceholderOptionByDefault()
    {
        using var context = new BunitContext();
        Color value = Color.Red;

        var cut = context.Render<StilSelect<Color>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        Assert.DoesNotContain(cut.FindAll("option"), option => option.GetAttribute("value") == "");
    }

    [Fact]
    public void TwoWayBindingParsesEnumOnChange()
    {
        using var context = new BunitContext();
        Color? value = null;

        var cut = context.Render<StilSelect<Color?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Placeholder, "Choose…"));

        cut.Find("select").Change("Blue");
        Assert.Equal(Color.Blue, value);

        cut.Find("select").Change("");
        Assert.Null(value);
    }

    [Fact]
    public void ItemsWithItemTextRenderCustomOptions()
    {
        using var context = new BunitContext();
        int value = 2;

        var cut = context.Render<StilSelect<int>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Items, new[] { 1, 2, 3 })
            .Add(component => component.ItemText, (Func<int, string>)(i => $"{i} seats")));

        var options = cut.FindAll("option");
        Assert.Equal(["1", "2", "3"], options.Select(option => option.GetAttribute("value")));
        Assert.Equal(["1 seats", "2 seats", "3 seats"], options.Select(option => option.TextContent));
        Assert.Equal("2", cut.Find("select").GetAttribute("value"));

        cut.Find("select").Change("3");
        Assert.Equal(3, value);
    }

    [Fact]
    public void ItemsReplaceEnumValues()
    {
        using var context = new BunitContext();
        Color value = Color.Blue;

        var cut = context.Render<StilSelect<Color>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Items, new[] { Color.Blue, Color.Red }));

        Assert.Equal(["Blue", "Red"], cut.FindAll("option").Select(option => option.TextContent));
    }

    [Fact]
    public void ItemsOfStringsUseToStringByDefault()
    {
        using var context = new BunitContext();
        string? value = "b";

        var cut = context.Render<StilSelect<string?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Items, new string?[] { "a", "b" }));

        Assert.Equal(["a", "b"], cut.FindAll("option").Select(option => option.TextContent));
        cut.Find("select").Change("a");
        Assert.Equal("a", value);
    }

    [Fact]
    public void ChildContentReplacesGeneratedOptionsButKeepsPlaceholder()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilSelect<string?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Placeholder, "Pick one")
            .Add(component => component.Items, new string?[] { "ignored" })
            .AddChildContent("<option value=\"x\">Custom X</option><option value=\"y\">Custom Y</option>"));

        Assert.Equal(["Pick one", "Custom X", "Custom Y"], cut.FindAll("option").Select(option => option.TextContent));
    }

    [Fact]
    public void NonEnumWithoutItemsRendersNoOptions()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilSelect<string?>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value));

        Assert.Empty(cut.FindAll("option"));
    }

    [Fact]
    public void CustomIdHintAndSize()
    {
        using var context = new BunitContext();
        Color value = Color.Red;

        var cut = context.Render<StilSelect<Color>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Id, "color")
            .Add(component => component.Label, "Color")
            .Add(component => component.Hint, "Your favourite.")
            .Add(component => component.Size, StilSize.Small));

        var select = cut.Find("select");
        Assert.Equal("color", select.Id);
        Assert.Equal("color", cut.Find("label").GetAttribute("for"));
        Assert.Equal("color-hint", cut.Find(".stil-field-hint").Id);
        Assert.Equal("color-hint", select.GetAttribute("aria-describedby"));
        Assert.Equal(["stil-select", "stil-select-sm"], select.ClassList);
    }

    [Fact]
    public void ClassAndStyleGoOnWrapperInputClassAndAttributesOnSelect()
    {
        using var context = new BunitContext();
        Color value = Color.Red;

        var cut = context.Render<StilSelect<Color>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.InputClass, "w-auto")
            .Add(component => component.Disabled, true)
            .AddUnmatched("class", "stil-col-6")
            .AddUnmatched("style", "order: 1")
            .AddUnmatched("aria-label", "Color"));

        Assert.Equal(["stil-field", "stil-col-6"], cut.Find("div.stil-field").ClassList);
        Assert.Equal("order: 1", cut.Find("div.stil-field").GetAttribute("style"));
        var select = cut.Find("select");
        Assert.Equal(["stil-select", "w-auto"], select.ClassList);
        Assert.False(select.HasAttribute("style"));
        Assert.Equal("Color", select.GetAttribute("aria-label"));
        Assert.True(select.HasAttribute("disabled"));
    }

    [Fact]
    public void BoolItemsMatchTheBoundValue()
    {
        using var context = new BunitContext();
        bool value = true;

        var cut = context.Render<StilSelect<bool>>(parameters => parameters
            .Bind(component => component.Value, value, v => value = v, () => value)
            .Add(component => component.Items, new[] { true, false }));

        var selected = cut.Find("select").GetAttribute("value");
        Assert.Contains(cut.FindAll("option"), option => option.GetAttribute("value") == selected);
    }

    [Fact]
    public void DecimalItemsMatchTheBoundValueInSwedishCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("sv-SE");
        try
        {
            using var context = new BunitContext();
            decimal value = 1.5m;

            var cut = context.Render<StilSelect<decimal>>(parameters => parameters
                .Bind(component => component.Value, value, v => value = v, () => value)
                .Add(component => component.Items, new[] { 1.5m, 2.5m }));

            var selected = cut.Find("select").GetAttribute("value");
            Assert.Contains(cut.FindAll("option"), option => option.GetAttribute("value") == selected);

            cut.Find("select").Change(cut.FindAll("option")[1].GetAttribute("value"));
            Assert.Equal(2.5m, value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
