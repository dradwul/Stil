using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilDataGridTests
{
    private static readonly string[] Customers = ["Fjällbacka Café", "Berglund Outdoor", "Norrsken AB", "Ek & Lind", "Solvik Bageri"];

    /// <summary>25 orders, 1001–1025. Status cycles New, Shipped, Cancelled; every other order is paid.</summary>
    private static List<GridOrder> Orders() =>
        Enumerable.Range(1, 25)
            .Select(i => new GridOrder(1000 + i, Customers[i % Customers.Length], (GridOrderStatus)(i % 3), i % 2 == 0, i * 100m))
            .ToList();

    private static IRenderedComponent<DataGridHost> RenderGrid(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<DataGridHost>>? configure = null,
        IEnumerable<GridOrder>? items = null,
        bool withItems = true)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        return context.Render<DataGridHost>(parameters =>
        {
            if (withItems)
            {
                parameters.Add(host => host.Items, items ?? Orders());
            }

            configure?.Invoke(parameters);
        });
    }

    private static IReadOnlyList<string> Column(IRenderedComponent<DataGridHost> cut, int index) =>
        cut.FindAll("tbody tr").Select(row => row.QuerySelectorAll("td")[index].TextContent).ToList();

    private static IElement Header(IRenderedComponent<DataGridHost> cut, string title) =>
        cut.FindAll("thead th").Single(th => th.TextContent.Trim() == title);

    [Fact]
    public void RendersHeadersTheFirstPageAndASummary()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context);

        Assert.NotNull(cut.Find("div.stil-data-grid > .stil-table-responsive > table.stil-table"));
        Assert.Equal("Orders", cut.Find("caption").TextContent);
        Assert.Equal(["Order", "Customer", "Status", "Paid", "Amount", "Actions"], cut.FindAll("thead th").Select(th => th.TextContent.Trim()));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(10, rows.Count);
        // Bool cells use TrueText and FalseText, like the filter.
        Assert.Equal(["1001", "Berglund Outdoor", "Shipped", "No", "100.00", "Edit 1001"],
            rows[0].QuerySelectorAll("td").Select(td => td.TextContent));
        Assert.Equal("Yes", rows[1].QuerySelectorAll("td")[3].TextContent);
        Assert.Contains("stil-text-end", rows[0].QuerySelectorAll("td")[4].ClassList);
        Assert.Equal("1–10 of 25", cut.Find(".stil-data-grid-footer span").TextContent);
        Assert.Equal(3, cut.FindAll(".stil-pagination .stil-page-link").Count(link => int.TryParse(link.TextContent, out _)));
        Assert.Null(cut.Find(".stil-data-grid").GetAttribute("aria-busy"));
    }

    [Fact]
    public void SortableHeadersAreButtonsWithAriaSortAndTemplateColumnsAreNot()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context);

        Assert.Equal("none", Header(cut, "Customer").GetAttribute("aria-sort"));
        Assert.NotNull(Header(cut, "Customer").QuerySelector("button.stil-data-grid-sort"));
        Assert.Null(Header(cut, "Actions").GetAttribute("aria-sort"));
        Assert.Null(Header(cut, "Actions").QuerySelector("button"));
    }

    [Fact]
    public void ClickingAHeaderCyclesAscendingDescendingAndUnsorted()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context);

        Header(cut, "Amount").QuerySelector("button")!.Click();
        Assert.Equal("ascending", Header(cut, "Amount").GetAttribute("aria-sort"));
        Assert.Equal("100.00", Column(cut, 4)[0]);

        Header(cut, "Amount").QuerySelector("button")!.Click();
        Assert.Equal("descending", Header(cut, "Amount").GetAttribute("aria-sort"));
        Assert.Equal("2500.00", Column(cut, 4)[0]);

        Header(cut, "Amount").QuerySelector("button")!.Click();
        Assert.Equal("none", Header(cut, "Amount").GetAttribute("aria-sort"));
        Assert.Equal("1001", Column(cut, 0)[0]);
    }

    [Fact]
    public void SortingAnotherColumnStartsAscendingAndGoesBackToPageOne()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context);

        cut.FindAll(".stil-page-link").Single(link => link.TextContent == "2").Click();
        Assert.Equal("11–20 of 25", cut.Find(".stil-data-grid-footer span").TextContent);

        Header(cut, "Customer").QuerySelector("button")!.Click();

        Assert.Equal("1–10 of 25", cut.Find(".stil-data-grid-footer span").TextContent);
        Assert.Equal("ascending", Header(cut, "Customer").GetAttribute("aria-sort"));
        Assert.Equal("none", Header(cut, "Amount").GetAttribute("aria-sort"));
        Assert.Equal(Column(cut, 1).Order(StringComparer.Ordinal), Column(cut, 1));
        Assert.Equal("Berglund Outdoor", Column(cut, 1)[0]);
    }

    [Fact]
    public void InitialSortIsAppliedOnFirstLoad()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context, parameters => parameters.Add(host => host.InitialSort, StilSortDirection.Descending));

        cut.WaitForAssertion(() => Assert.Equal("1025", Column(cut, 0)[0]));
        Assert.Equal("descending", Header(cut, "Order").GetAttribute("aria-sort"));
    }

    [Fact]
    public void PagingShowsTheRightRowsAndLastPageSummary()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context);

        cut.FindAll(".stil-page-link").Single(link => link.TextContent == "3").Click();

        Assert.Equal(["1021", "1022", "1023", "1024", "1025"], Column(cut, 0));
        Assert.Equal("21–25 of 25", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void PageSizeOptionsChangeTheNumberOfRows()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters.Add(host => host.PageSizeOptions, [10, 20]));

        var select = cut.Find(".stil-data-grid-footer select");
        Assert.Equal("Rows per page", select.GetAttribute("aria-label"));
        select.Change("20");

        Assert.Equal(20, cut.FindAll("tbody tr").Count);
        Assert.Equal("1–20 of 25", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void PageSizeZeroShowsAllRowsWithoutAFooter()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context, parameters => parameters.Add(host => host.PageSize, 0));

        Assert.Equal(25, cut.FindAll("tbody tr").Count);
        Assert.Empty(cut.FindAll(".stil-data-grid-footer"));
    }

    [Fact]
    public void FilterRowHasTextInputsAndChoicesForEnumsAndBools()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context, parameters => parameters.Add(host => host.Filterable, true));

        var cells = cut.FindAll(".stil-data-grid-filters td");
        Assert.Equal(6, cells.Count);
        Assert.Equal("Filter Customer", cells[1].QuerySelector("input[type=search]")!.GetAttribute("aria-label"));
        Assert.Equal(["All", "New", "Shipped", "Cancelled"], cells[2].QuerySelectorAll("option").Select(o => o.TextContent));
        Assert.Equal(["All", "Yes", "No"], cells[3].QuerySelectorAll("option").Select(o => o.TextContent));

        // Numbers and template columns cannot be filtered.
        Assert.Empty(cells[0].Children);
        Assert.Empty(cells[4].Children);
        Assert.Empty(cells[5].Children);
    }

    [Fact]
    public void NoFilterRowUnlessFilterable()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context);

        Assert.Empty(cut.FindAll(".stil-data-grid-filters"));
    }

    [Fact]
    public void TextFilterIsCaseInsensitiveAndDebounced()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters.Add(host => host.Filterable, true));

        cut.Find(".stil-data-grid-filters input").Input("NORRSKEN");

        // Nothing happens until the typing pauses.
        Assert.Equal(10, cut.FindAll("tbody tr").Count);
        cut.WaitForAssertion(() => Assert.Equal(5, cut.FindAll("tbody tr").Count));
        Assert.All(Column(cut, 1), customer => Assert.Equal("Norrsken AB", customer));
        Assert.Equal("1–5 of 5", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void ChoiceFiltersApplyAtOnceAndCombine()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters.Add(host => host.Filterable, true));

        var selects = cut.FindAll(".stil-data-grid-filters select");
        selects[0].Change("Shipped");
        Assert.Equal(9, cut.Instance.Grid.TotalCount);

        cut.FindAll(".stil-data-grid-filters select")[1].Change("true");

        Assert.Equal(4, cut.Instance.Grid.TotalCount);
        Assert.All(Column(cut, 2), status => Assert.Equal("Shipped", status));
        Assert.All(Column(cut, 3), paid => Assert.Equal("Yes", paid));
    }

    [Fact]
    public void FilteringFromALaterPageGoesBackToPageOne()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters.Add(host => host.Filterable, true));
        cut.FindAll(".stil-page-link").Single(link => link.TextContent == "3").Click();

        cut.FindAll(".stil-data-grid-filters select")[0].Change("New");

        Assert.Equal("1–8 of 8", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void EmptyResultShowsTheEmptyText()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, items: Array.Empty<GridOrder>());

        var message = cut.Find("tbody td.stil-data-grid-message");
        Assert.Equal("No items", message.TextContent);
        Assert.Equal("6", message.GetAttribute("colspan"));
        Assert.Equal("0–0 of 0", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void SingleSelectionSelectsTheClickedRow()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters.Add(host => host.SelectionMode, StilSelectionMode.Single));

        var rows = cut.FindAll("tbody tr");
        Assert.All(rows, row => Assert.Equal("false", row.GetAttribute("aria-selected")));
        Assert.All(rows, row => Assert.Equal("0", row.GetAttribute("tabindex")));
        Assert.Contains("stil-table-hover", cut.Find("table").ClassList);

        rows[2].Click();

        Assert.Equal(1003, cut.Instance.SelectedItem?.Id);
        Assert.Equal("true", cut.FindAll("tbody tr")[2].GetAttribute("aria-selected"));
        Assert.Equal("false", cut.FindAll("tbody tr")[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void EnterOnAFocusedRowActivatesIt()
    {
        using var context = new BunitContext();
        GridOrder? clicked = null;
        var cut = RenderGrid(context, parameters => parameters
            .Add(host => host.OnRowClick, (GridOrder order) => clicked = order));

        cut.FindAll("tbody tr")[1].KeyDown(new KeyboardEventArgs { Key = "Enter" });
        Assert.Equal(1002, clicked?.Id);

        clicked = null;
        cut.FindAll("tbody tr")[1].KeyDown(new KeyboardEventArgs { Key = "a" });
        Assert.Null(clicked);
    }

    [Fact]
    public void RowsAreNotFocusableWithoutSelectionOrRowClick()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context);

        Assert.All(cut.FindAll("tbody tr"), row =>
        {
            Assert.Null(row.GetAttribute("tabindex"));
            Assert.Null(row.GetAttribute("aria-selected"));
        });
        Assert.DoesNotContain("stil-table-hover", cut.Find("table").ClassList);
    }

    [Fact]
    public void ItemKeyMatchesSelectionAcrossNewObjects()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters
            .Add(host => host.SelectionMode, StilSelectionMode.Single)
            .Add(host => host.ItemKey, order => order.Id));
        cut.FindAll("tbody tr")[0].Click();

        // New objects with the same keys, as when a page reloads from a database.
        cut.Render(parameters => parameters.Add(host => host.Items, Orders().Select(o => o with { Customer = o.Customer + "!" }).ToList()));

        Assert.Equal("true", cut.FindAll("tbody tr")[0].GetAttribute("aria-selected"));
    }

    [Fact]
    public void MultipleSelectionUsesCheckboxesAndSelectAllOnThePage()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters.Add(host => host.SelectionMode, StilSelectionMode.Multiple));

        Assert.Equal("Select all rows on this page", cut.Find("thead .stil-data-grid-select input").GetAttribute("aria-label"));
        Assert.Equal(10, cut.FindAll("tbody .stil-data-grid-select input[type=checkbox]").Count);

        cut.FindAll("tbody .stil-data-grid-select input")[1].Change(true);
        cut.FindAll("tbody .stil-data-grid-select input")[3].Change(true);
        Assert.Equal([1002, 1004], cut.Instance.SelectedItems!.Select(o => o.Id));
        Assert.Equal("true", cut.FindAll("tbody tr")[3].GetAttribute("aria-selected"));

        cut.Find("thead .stil-data-grid-select input").Change(true);
        Assert.Equal(10, cut.Instance.SelectedItems!.Count);
        Assert.True(cut.Find("thead .stil-data-grid-select input").HasAttribute("checked"));

        cut.Find("thead .stil-data-grid-select input").Change(false);
        Assert.Empty(cut.Instance.SelectedItems!);

        cut.FindAll("tbody .stil-data-grid-select input")[1].Change(true);
        cut.FindAll("tbody .stil-data-grid-select input")[1].Change(false);
        Assert.Empty(cut.Instance.SelectedItems!);
    }

    [Fact]
    public void SelectAllKeepsSelectionsOnOtherPages()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context, parameters => parameters.Add(host => host.SelectionMode, StilSelectionMode.Multiple));
        cut.FindAll("tbody .stil-data-grid-select input")[0].Change(true);

        cut.FindAll(".stil-page-link").Single(link => link.TextContent == "2").Click();
        cut.Find("thead .stil-data-grid-select input").Change(true);
        Assert.Equal(11, cut.Instance.SelectedItems!.Count);

        cut.Find("thead .stil-data-grid-select input").Change(false);
        Assert.Equal([1001], cut.Instance.SelectedItems!.Select(o => o.Id));
    }

    [Fact]
    public void RowClassAddsClassesToRows()
    {
        using var context = new BunitContext();

        var cut = RenderGrid(context, parameters => parameters
            .Add(host => host.RowClass, order => order.Status == GridOrderStatus.Cancelled ? "stil-text-muted" : null));

        var rows = cut.FindAll("tbody tr");
        Assert.Contains("stil-text-muted", rows[1].ClassList);
        Assert.Empty(rows[0].ClassList);
    }

    [Fact]
    public void ItemsProviderGetsPagingSortingAndFilters()
    {
        using var context = new BunitContext();
        var requests = new List<StilDataGridRequest<GridOrder>>();
        var source = Orders();
        Task<StilDataGridResult<GridOrder>> Provide(StilDataGridRequest<GridOrder> request)
        {
            requests.Add(request);
            var query = request.ApplyFiltersAndSorting(source.AsQueryable());
            return Task.FromResult(new StilDataGridResult<GridOrder>(request.ApplyPaging(query).ToList(), query.Count()));
        }

        var cut = RenderGrid(context, parameters => parameters
            .Add(host => host.ItemsProvider, Provide)
            .Add(host => host.Filterable, true), withItems: false);

        Assert.Equal(0, requests[^1].StartIndex);
        Assert.Equal(10, requests[^1].Count);
        Assert.Null(requests[^1].SortBy);
        Assert.Equal("1–10 of 25", cut.Find(".stil-data-grid-footer span").TextContent);

        Header(cut, "Amount").QuerySelector("button")!.Click();
        Header(cut, "Amount").QuerySelector("button")!.Click();
        Assert.Equal("Amount", requests[^1].SortBy);
        Assert.Equal(StilSortDirection.Descending, requests[^1].SortDirection);

        cut.FindAll(".stil-data-grid-filters select")[0].Change("Cancelled");
        Assert.Equal("Cancelled", requests[^1].Filters["Status"]);
        Assert.Equal(["2300.00", "2000.00"], Column(cut, 4).Take(2));

        Assert.Equal("1–8 of 8", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public async Task SlowItemsProviderShowsLoadingState()
    {
        using var context = new BunitContext();
        var pending = new TaskCompletionSource<StilDataGridResult<GridOrder>>();

        var cut = RenderGrid(context, parameters => parameters
            .Add(host => host.ItemsProvider, _ => pending.Task), withItems: false);

        Assert.Equal("true", cut.Find(".stil-data-grid").GetAttribute("aria-busy"));
        Assert.Equal("Loading…", cut.Find(".stil-data-grid-message").TextContent);

        await cut.InvokeAsync(() => pending.SetResult(new StilDataGridResult<GridOrder>(Orders().Take(3).ToList(), 3)));

        cut.WaitForAssertion(() => Assert.Equal(3, cut.FindAll("tbody tr").Count));
        Assert.Null(cut.Find(".stil-data-grid").GetAttribute("aria-busy"));
    }

    [Fact]
    public async Task RefreshAsyncReloadsChangedData()
    {
        using var context = new BunitContext();
        var source = Orders();
        var cut = RenderGrid(context, items: source);

        source.RemoveRange(0, 20);
        await cut.InvokeAsync(cut.Instance.Grid.RefreshAsync);

        Assert.Equal("1–5 of 5", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void ANewItemsListReloadsAndClampsThePage()
    {
        using var context = new BunitContext();
        var cut = RenderGrid(context);
        cut.FindAll(".stil-page-link").Single(link => link.TextContent == "3").Click();

        cut.Render(parameters => parameters.Add(host => host.Items, Orders().Take(12).ToList()));

        Assert.Equal("11–12 of 12", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void ColumnsOutsideAGridThrow()
    {
        using var context = new BunitContext();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.Render<StilDataGridColumn<GridOrder, int>>(parameters => parameters.Add(c => c.Property, o => o.Id)));

        Assert.Contains("StilDataGrid", exception.Message);
    }

    [Fact]
    public void StringFilterTranslatesToAnExpressionEfCoreUnderstands()
    {
        using var context = new BunitContext();
        StilDataGridRequest<GridOrder>? last = null;
        var cut = RenderGrid(context, parameters => parameters
            .Add(host => host.Filterable, true)
            .Add(host => host.ItemsProvider, request =>
            {
                last = request;
                return Task.FromResult(new StilDataGridResult<GridOrder>([], 0));
            }), withItems: false);

        cut.Find(".stil-data-grid-filters input").Input("café");
        cut.WaitForAssertion(() => Assert.Equal("café", last?.Filters["Customer"]));

        var expression = last!.ApplyFiltersAndSorting(Array.Empty<GridOrder>().AsQueryable()).Expression.ToString();
        Assert.Contains("ToLower()", expression);
        Assert.Contains("Contains(\"café\")", expression);
    }
}
