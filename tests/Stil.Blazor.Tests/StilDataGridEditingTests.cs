using System.Globalization;
using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilDataGridEditingTests
{
    private static List<EditOrder> Orders() =>
    [
        new() { Id = 1001, Customer = "Fjällbacka Café", Status = GridOrderStatus.New, Paid = true, Amount = 100m, Date = new DateOnly(2026, 11, 2), Email = "a@example.com" },
        new() { Id = 1002, Customer = "Berglund Outdoor", Status = GridOrderStatus.Shipped, Amount = 250m, Date = new DateOnly(2026, 11, 3), Email = "b@example.com" },
    ];

    private static IRenderedComponent<EditGridHost> Render(BunitContext context,
        Action<ComponentParameterCollectionBuilder<EditGridHost>>? configure = null)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context.Render<EditGridHost>(parameters =>
        {
            parameters.Add(host => host.Items, Orders());
            configure?.Invoke(parameters);
        });
    }

    private static AngleSharp.Dom.IElement EditRow(IRenderedComponent<EditGridHost> cut) => cut.Find("tr.stil-data-grid-editing");

    private static List<string> Cells(IRenderedComponent<EditGridHost> cut, int row) =>
        cut.FindAll("tbody tr")[row].QuerySelectorAll("td").Select(td => td.TextContent.Trim()).ToList();

    [Fact]
    public void TheEditButtonTurnsTheRowIntoFields()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        cut.FindAll("button[aria-label=Edit]")[1].Click();

        var row = EditRow(cut);
        Assert.Equal("1002", row.QuerySelectorAll("td")[0].TextContent); // Order cannot be edited.
        Assert.Equal("Berglund Outdoor", row.QuerySelector("input[aria-label=Customer]")!.GetAttribute("value"));
        Assert.Equal("Shipped", row.QuerySelector("select[aria-label=Status]")!.GetAttribute("value"));
        Assert.Equal("checkbox", row.QuerySelector("input[aria-label=Paid]")!.GetAttribute("type"));
        Assert.Equal("number", row.QuerySelector("input[aria-label=Amount]")!.GetAttribute("type"));
        Assert.Equal("date", row.QuerySelector("input[aria-label=Date]")!.GetAttribute("type"));
        Assert.NotNull(row.QuerySelector("button[aria-label=Save]"));
        Assert.NotEmpty(context.JSInterop.Invocations["focusEditor"]);
    }

    [Fact]
    public void SaveValidatesTheEditedFieldsAndCallsOnRowSave()
    {
        using var context = new BunitContext();
        var cut = Render(context);
        cut.FindAll("button[aria-label=Edit]")[0].Click();

        EditRow(cut).QuerySelector("input[aria-label=Customer]")!.Change("");
        EditRow(cut).QuerySelector("input[aria-label=Amount]")!.Change("0");
        cut.Find("button[aria-label=Save]").Click();

        Assert.Empty(cut.Instance.Saved);
        Assert.Equal(["Enter a customer.", "Enter an amount from 1 to 10,000."], EditRow(cut).QuerySelectorAll(".stil-field-error").Select(e => e.TextContent.Trim()));

        EditRow(cut).QuerySelector("input[aria-label=Customer]")!.Change("Norrsken AB");
        EditRow(cut).QuerySelector("input[aria-label=Amount]")!.Change("150");
        EditRow(cut).QuerySelector("select[aria-label=Status]")!.Change("Cancelled");
        cut.Find("button[aria-label=Save]").Click();

        var saved = Assert.Single(cut.Instance.Saved);
        Assert.False(saved.IsNew);
        Assert.Equal("Norrsken AB", saved.Item.Customer);
        Assert.Empty(cut.FindAll("tr.stil-data-grid-editing"));
        Assert.Equal(["1001", "Norrsken AB", "Cancelled", "Yes", "150.00", "2026-11-02"], Cells(cut, 0).Take(6));
    }

    [Fact]
    public void EscapeCancelsAndPutsTheValuesBack()
    {
        using var context = new BunitContext();
        var cut = Render(context);
        cut.FindAll("button[aria-label=Edit]")[0].Click();

        EditRow(cut).QuerySelector("input[aria-label=Customer]")!.Change("Someone else");
        EditRow(cut).KeyDown("Escape");

        Assert.Empty(cut.FindAll("tr.stil-data-grid-editing"));
        Assert.Equal("Fjällbacka Café", Cells(cut, 0)[1]);
        Assert.Equal("Fjällbacka Café", Assert.Single(cut.Instance.Cancelled).Item.Customer);
    }

    [Fact]
    public void OnRowSaveReturningFalseKeepsEditing()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.Accept, false));
        cut.FindAll("button[aria-label=Edit]")[0].Click();

        cut.Find("button[aria-label=Save]").Click();

        Assert.Single(cut.Instance.Saved);
        Assert.NotNull(EditRow(cut));
    }

    [Fact]
    public void EnterF2AndDoubleClickStartEditing()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        cut.FindAll("tbody tr")[0].KeyDown("Enter");
        Assert.Equal("1001", EditRow(cut).QuerySelectorAll("td")[0].TextContent);

        // Moving to another row saves the first.
        cut.FindAll("tbody tr")[1].KeyDown("F2");
        Assert.Single(cut.Instance.Saved);
        Assert.Equal("1002", EditRow(cut).QuerySelectorAll("td")[0].TextContent);

        EditRow(cut).KeyDown("Escape");
        cut.FindAll("tbody tr")[0].DoubleClick();
        Assert.Equal("1001", EditRow(cut).QuerySelectorAll("td")[0].TextContent);
    }

    [Fact]
    public async Task AddRowPutsANewRowOnTop()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        await cut.InvokeAsync(() => cut.Instance.Grid.AddRowAsync(new EditOrder { Id = 1003, Date = new DateOnly(2026, 11, 4) }));
        Assert.Equal(3, cut.FindAll("tbody tr").Count);
        Assert.Contains("stil-data-grid-editing", cut.FindAll("tbody tr")[0].ClassList);

        EditRow(cut).KeyDown("Escape");
        Assert.Equal(2, cut.FindAll("tbody tr").Count);

        await cut.InvokeAsync(() => cut.Instance.Grid.AddRowAsync(new EditOrder { Id = 1003, Date = new DateOnly(2026, 11, 4) }));
        EditRow(cut).QuerySelector("input[aria-label=Customer]")!.Change("Solvik Bageri");
        EditRow(cut).QuerySelector("input[aria-label=Amount]")!.Change("75");
        cut.Find("button[aria-label=Save]").Click();

        Assert.True(Assert.Single(cut.Instance.Saved).IsNew);
        Assert.Equal(["1003", "Solvik Bageri"], Cells(cut, 0).Take(2));
        Assert.Equal(3, cut.FindAll("tbody tr").Count);
    }

    [Fact]
    public async Task SavingRendersTheAppAgainSoTheGridGetsItsNewItems()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.ReplaceItems, true));

        await cut.InvokeAsync(() => cut.Instance.Grid.AddRowAsync(new EditOrder { Id = 1003, Customer = "Lappi Tours", Amount = 42m }));
        cut.Find("button[aria-label=Save]").Click();

        Assert.Equal(["1003", "1001", "1002"], cut.FindAll("tbody tr").Select(row => row.QuerySelector("td")!.TextContent));
    }

    [Fact]
    public void CellEditingEditsTheClickedCellAndSavesOnEnter()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.EditMode, StilDataGridEditMode.Cell));

        Assert.Null(cut.FindAll("tbody tr")[0].QuerySelectorAll("td")[0].GetAttribute("data-stil-editable"));
        cut.FindAll("tbody tr")[0].QuerySelectorAll("td")[4].Click();

        var editors = EditRow(cut).QuerySelectorAll("td[data-stil-editor]");
        Assert.Equal("Amount", Assert.Single(editors).QuerySelector("input")!.GetAttribute("aria-label"));

        editors[0].QuerySelector("input")!.Change("120");
        EditRow(cut).KeyDown("Enter");

        Assert.Equal("Amount", Assert.Single(cut.Instance.Saved).ColumnKey);
        Assert.Equal("120.00", Cells(cut, 0)[4]);

        // Leaving a cell saves it too; Escape cancels.
        cut.FindAll("tbody tr")[1].QuerySelectorAll("td")[1].Click();
        EditRow(cut).QuerySelector("input")!.Change("Dala Trä");
        cut.Find("td[data-stil-editor]").TriggerEvent("onfocusout", new Microsoft.AspNetCore.Components.Web.FocusEventArgs());
        Assert.Equal("Dala Trä", Cells(cut, 1)[1]);
    }
}
