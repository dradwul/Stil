using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilAutocompleteTests
{
    private static readonly string[] Cities = PickerForm.Cities;

    private static IRenderedComponent<StilAutocomplete<string>> RenderAutocomplete(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<StilAutocomplete<string>>>? configure = null,
        bool withItems = true) =>
        context.Render<StilAutocomplete<string>>(parameters =>
        {
            parameters.Add(c => c.Label, "City");
            if (withItems)
            {
                parameters.Add(c => c.Items, Cities);
            }

            configure?.Invoke(parameters);
        });

    [Fact]
    public void RendersAClosedComboboxWithLabelAndListbox()
    {
        using var context = new BunitContext();

        var cut = RenderAutocomplete(context, p => p.Add(c => c.Placeholder, "Search cities"));

        var input = cut.Find("input");
        Assert.Equal("combobox", input.GetAttribute("role"));
        Assert.Equal("list", input.GetAttribute("aria-autocomplete"));
        Assert.Equal("false", input.GetAttribute("aria-expanded"));
        Assert.Equal("off", input.GetAttribute("autocomplete"));
        Assert.Equal("Search cities", input.GetAttribute("placeholder"));
        Assert.Contains("stil-input", input.ClassList);
        Assert.Equal(input.Id, cut.Find("label").GetAttribute("for"));

        var listbox = cut.Find("[role=listbox]");
        Assert.Equal(input.GetAttribute("aria-controls"), listbox.Id);
        Assert.Equal("City", listbox.GetAttribute("aria-label"));
        Assert.True(cut.Find(".stil-combobox-popup").HasAttribute("hidden"));
    }

    [Fact]
    public void TypingFiltersTheOptionsCaseInsensitively()
    {
        using var context = new BunitContext();
        var cut = RenderAutocomplete(context);

        cut.Find("input").Input("ö");

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-expanded"));
        Assert.False(cut.Find(".stil-combobox-popup").HasAttribute("hidden"));
        Assert.Equal(["Göteborg", "Malmö"], cut.FindAll("[role=option]").Select(o => o.TextContent.Trim()));

        // The first match is active and pointed to by aria-activedescendant.
        var first = cut.FindAll("[role=option]")[0];
        Assert.Equal(first.Id, cut.Find("input").GetAttribute("aria-activedescendant"));
        Assert.Equal("true", first.GetAttribute("aria-selected"));
        Assert.Contains("stil-combobox-option-active", first.ClassList);
    }

    [Fact]
    public void NoMatchesShowsTheNoResultsText()
    {
        using var context = new BunitContext();
        var cut = RenderAutocomplete(context, p => p.Add(c => c.NoResultsText, "Nothing found"));

        cut.Find("input").Input("xyz");

        Assert.Empty(cut.FindAll("[role=option]"));
        Assert.Equal("Nothing found", cut.Find(".stil-combobox-message").TextContent);
        Assert.Equal("status", cut.Find(".stil-combobox-message").GetAttribute("role"));
    }

    [Fact]
    public void ArrowKeysMoveTheActiveOptionAndEnterChoosesIt()
    {
        using var context = new BunitContext();
        string? chosen = null;
        var cut = RenderAutocomplete(context, p => p.Add(c => c.ValueChanged, (string? value) => chosen = value));
        var input = cut.Find("input");

        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Equal(5, cut.FindAll("[role=option]").Count);
        Assert.Equal("Oslo", cut.Find(".stil-combobox-option-active").TextContent.Trim());

        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        input.KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        input.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Equal("Stockholm", cut.Find(".stil-combobox-option-active").TextContent.Trim());

        input.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        input.KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Equal("Helsinki", cut.Find(".stil-combobox-option-active").TextContent.Trim());

        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal("Helsinki", chosen);
        Assert.Equal("Helsinki", cut.Find("input").GetAttribute("value"));
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
        Assert.Null(cut.Find("input").GetAttribute("aria-activedescendant"));
    }

    [Fact]
    public void EscapeCloses()
    {
        using var context = new BunitContext();
        var cut = RenderAutocomplete(context);
        cut.Find("input").Input("o");

        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void ClickingAnOptionChoosesIt()
    {
        using var context = new BunitContext();
        string? chosen = null;
        var cut = RenderAutocomplete(context, p => p.Add(c => c.ValueChanged, (string? value) => chosen = value));
        cut.Find("input").Click();

        cut.FindAll("[role=option]")[2].Click();

        Assert.Equal("Göteborg", chosen);
        Assert.Equal("Göteborg", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void TypingAfterChoosingClearsTheValueAndBlurRestoresTheText()
    {
        using var context = new BunitContext();
        var values = new List<string?>();
        var cut = RenderAutocomplete(context, p => p
            .Add(c => c.Value, "Oslo")
            .Add(c => c.ValueChanged, (string? value) => values.Add(value)));
        Assert.Equal("Oslo", cut.Find("input").GetAttribute("value"));

        cut.Find("input").Input("Osl");
        Assert.Equal([null], values);

        cut.Find("input").Input("Osloo");
        Assert.Single(values);

        cut.Find(".stil-combobox").FocusOut();
        Assert.Equal("", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void OpeningWithAChosenValueShowsAllOptions()
    {
        using var context = new BunitContext();
        var cut = RenderAutocomplete(context, p => p.Add(c => c.Value, "Oslo"));

        cut.Find("input").Click();

        Assert.Equal(5, cut.FindAll("[role=option]").Count);
    }

    [Fact]
    public void ItemTextAndItemTemplateShapeTheOptions()
    {
        using var context = new BunitContext();
        var cut = RenderAutocomplete(context, p => p
            .Add(c => c.ItemText, city => city.ToUpperInvariant())
            .Add(c => c.ItemTemplate, city => builder => builder.AddMarkupContent(0, $"<strong>{city}</strong>")));

        cut.Find("input").Input("osl");

        Assert.Equal("<strong>Oslo</strong>", cut.Find("[role=option]").InnerHtml.Trim());
        cut.Find("[role=option]").Click();
        Assert.Equal("OSLO", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void SearchIsDebouncedAndOnlyTheLatestResultIsShown()
    {
        using var context = new BunitContext();
        var queries = new List<string>();
        var cut = RenderAutocomplete(context, p => p
            .Add(c => c.Debounce, 50)
            .Add(c => c.Search, (query, _) =>
            {
                queries.Add(query);
                return Task.FromResult(Cities.Where(city => city.StartsWith(query, StringComparison.OrdinalIgnoreCase)));
            }), withItems: false);

        cut.Find("input").Input("s");
        cut.Find("input").Input("st");
        Assert.Equal("Searching…", cut.Find(".stil-combobox-message").TextContent);

        cut.WaitForAssertion(() => Assert.Equal(["Stockholm"], cut.FindAll("[role=option]").Select(o => o.TextContent.Trim())));
        Assert.Equal(["st"], queries);
    }

    [Fact]
    public void SearchWaitsForMinLength()
    {
        using var context = new BunitContext();
        var calls = 0;
        var cut = RenderAutocomplete(context, p => p
            .Add(c => c.MinLength, 2)
            .Add(c => c.Debounce, 0)
            .Add(c => c.Search, (_, _) =>
            {
                calls++;
                return Task.FromResult<IEnumerable<string>>(Cities);
            }), withItems: false);

        cut.Find("input").Input("o");

        Assert.Equal(0, calls);
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));

        cut.Find("input").Input("os");
        cut.WaitForAssertion(() => Assert.Equal(1, calls));
    }

    [Fact]
    public void MaxResultsLimitsTheList()
    {
        using var context = new BunitContext();
        var cut = RenderAutocomplete(context, p => p.Add(c => c.MaxResults, 2));

        cut.Find("input").Click();

        Assert.Equal(2, cut.FindAll("[role=option]").Count);
    }

    [Fact]
    public void ValidationMessageAndAriaInvalidInAnEditForm()
    {
        using var context = new BunitContext();
        var cut = context.Render<PickerForm>();

        cut.Find("form").Submit();

        Assert.False(cut.Instance.Submitted);
        var input = cut.Find("input[role=combobox]");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Contains("Choose a city.", cut.Find(".stil-field-error").TextContent);

        input.Input("stock");
        cut.Find("[role=option]").Click();

        Assert.Equal("Stockholm", cut.Instance.Model.City);
        Assert.Null(cut.Find("input[role=combobox]").GetAttribute("aria-invalid"));
        cut.Find("form").Submit();
        Assert.True(cut.Instance.Submitted);
    }

    [Fact]
    public void DisabledDisablesTheInput()
    {
        using var context = new BunitContext();

        var cut = RenderAutocomplete(context, p => p.Add(c => c.Disabled, true));

        Assert.True(cut.Find("input").HasAttribute("disabled"));
    }
}

public class StilMultiSelectTests
{
    private static readonly string[] Tags = PickerForm.Tags;

    private static IRenderedComponent<StilMultiSelect<string>> RenderMultiSelect(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<StilMultiSelect<string>>>? configure = null) =>
        context.Render<StilMultiSelect<string>>(parameters =>
        {
            parameters.Add(c => c.Label, "Tags").Add(c => c.Items, Tags);
            configure?.Invoke(parameters);
        });

    [Fact]
    public void RendersChipsForTheChosenOptions()
    {
        using var context = new BunitContext();

        var cut = RenderMultiSelect(context, p => p
            .Add(c => c.Value, ["Bug", "Urgent"])
            .Add(c => c.Placeholder, "Add tags"));

        Assert.NotNull(cut.Find("div.stil-combobox.stil-multi-select.stil-input"));
        Assert.Equal(["Bug", "Urgent"], cut.FindAll(".stil-chip-text").Select(c => c.TextContent));
        Assert.Equal("Remove Bug", cut.Find(".stil-chip-remove").GetAttribute("aria-label"));
        Assert.Contains("stil-multi-select-input", cut.Find("input").ClassList);
        Assert.DoesNotContain("stil-input", cut.Find("input").ClassList);
        Assert.Null(cut.Find("input").GetAttribute("placeholder"));
        Assert.Equal("true", cut.Find("[role=listbox]").GetAttribute("aria-multiselectable"));
    }

    [Fact]
    public void PlaceholderShowsOnlyWhenNothingIsChosen()
    {
        using var context = new BunitContext();

        var cut = RenderMultiSelect(context, p => p.Add(c => c.Placeholder, "Add tags"));

        Assert.Equal("Add tags", cut.Find("input").GetAttribute("placeholder"));
    }

    [Fact]
    public void ChoosingTogglesOptionsAndKeepsTheListOpen()
    {
        using var context = new BunitContext();
        IReadOnlyCollection<string>? value = null;
        var cut = RenderMultiSelect(context, p => p.Add(c => c.ValueChanged, (IReadOnlyCollection<string>? v) => value = v));

        cut.Find("input").Click();
        cut.FindAll("[role=option]")[1].Click();
        cut.FindAll("[role=option]")[2].Click();

        Assert.Equal(["Billing", "Bug"], value);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-expanded"));
        Assert.Equal(["false", "true", "true", "false"], cut.FindAll("[role=option]").Select(o => o.GetAttribute("aria-selected")));

        cut.FindAll("[role=option]")[1].Click();
        Assert.Equal(["Bug"], value);
    }

    [Fact]
    public void EnterChoosesTheActiveOptionAndClearsTheTypedText()
    {
        using var context = new BunitContext();
        IReadOnlyCollection<string>? value = null;
        var cut = RenderMultiSelect(context, p => p.Add(c => c.ValueChanged, (IReadOnlyCollection<string>? v) => value = v));

        cut.Find("input").Input("feat");
        Assert.Equal(["Feature"], cut.FindAll("[role=option]").Select(o => o.TextContent.Trim()));
        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(["Feature"], value);
        Assert.Equal("", cut.Find("input").GetAttribute("value"));
        Assert.Equal(4, cut.FindAll("[role=option]").Count);
    }

    [Fact]
    public void RemoveButtonAndBackspaceRemoveChips()
    {
        using var context = new BunitContext();
        var cut = RenderMultiSelect(context);
        IReadOnlyCollection<string>? value = ["Urgent", "Bug", "Feature"];
        cut.Render(p => p
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, (IReadOnlyCollection<string>? v) => value = v));

        cut.FindAll(".stil-chip-remove")[1].Click();
        Assert.Equal(["Urgent", "Feature"], value);

        cut.Render(p => p.Add(c => c.Value, value));
        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Backspace" });
        Assert.Equal(["Urgent"], value);
    }

    [Fact]
    public void BackspaceWithTextDoesNotRemoveChips()
    {
        using var context = new BunitContext();
        var changes = 0;
        var cut = RenderMultiSelect(context, p => p
            .Add(c => c.Value, ["Bug"])
            .Add(c => c.ValueChanged, (IReadOnlyCollection<string>? _) => changes++));

        cut.Find("input").Input("b");
        cut.Find("input").KeyDown(new KeyboardEventArgs { Key = "Backspace" });

        Assert.Equal(0, changes);
    }

    [Fact]
    public void ItemKeyMatchesChosenOptionsAcrossNewObjects()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilMultiSelect<GridOrder>>(p => p
            .Add(c => c.Items, [new GridOrder(1, "A", GridOrderStatus.New, false, 1), new GridOrder(2, "B", GridOrderStatus.New, false, 2)])
            .Add(c => c.Value, [new GridOrder(2, "B (old)", GridOrderStatus.New, false, 2)])
            .Add(c => c.ItemKey, order => order.Id)
            .Add(c => c.ItemText, order => order.Customer));

        cut.Find("input").Click();

        Assert.Equal(["false", "true"], cut.FindAll("[role=option]").Select(o => o.GetAttribute("aria-selected")));
    }

    [Fact]
    public void BindsInAnEditForm()
    {
        using var context = new BunitContext();
        var cut = context.Render<PickerForm>();

        var input = cut.FindAll("input[role=combobox]")[1];
        input.Click();
        cut.FindAll("[role=option]").First(o => o.TextContent.Trim() == "Bug").Click();

        Assert.Equal(["Bug"], cut.Instance.Model.Tags);
        Assert.Equal(["Bug"], cut.FindAll(".stil-chip-text").Select(c => c.TextContent));
    }

    [Fact]
    public void DisabledHidesRemoveButtons()
    {
        using var context = new BunitContext();

        var cut = RenderMultiSelect(context, p => p.Add(c => c.Value, ["Bug"]).Add(c => c.Disabled, true));

        Assert.Empty(cut.FindAll(".stil-chip-remove"));
        Assert.True(cut.Find("input").HasAttribute("disabled"));
        Assert.Equal("true", cut.Find(".stil-multi-select").GetAttribute("data-disabled"));
    }
}
