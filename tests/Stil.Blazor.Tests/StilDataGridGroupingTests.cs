using System.Globalization;
using System.Text.RegularExpressions;
using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public partial class StilDataGridGroupingTests
{
    private static readonly List<GridOrder> Orders =
    [
        new(1001, "Fjällbacka Café", GridOrderStatus.New, true, 100m),
        new(1002, "Berglund Outdoor", GridOrderStatus.Shipped, false, 250m),
        new(1003, "Café Norr", GridOrderStatus.New, false, 50m),
        new(1004, "Dala Trä", GridOrderStatus.Cancelled, true, 75m),
        new(1005, "Ekdal Bygg", GridOrderStatus.New, true, 20m),
    ];

    private static IRenderedComponent<GroupGridHost> Render(BunitContext context,
        Action<ComponentParameterCollectionBuilder<GroupGridHost>>? configure = null, bool withItems = true,
        IReadOnlyList<string>? groupBy = null)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<GroupGridHost>(parameters =>
        {
            if (withItems)
            {
                parameters.Add(host => host.Items, Orders);
            }

            parameters.Add(host => host.GroupBy, groupBy ?? ["Status"]);
            configure?.Invoke(parameters);
        });

        // The grid groups once its columns have registered, after the first render.
        cut.WaitForState(() => cut.Instance.GroupBy is not { Count: > 0 } || cut.FindAll("tr.stil-data-grid-group").Count > 0);
        return cut;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    // The body as lines: "Status: New (3)" for a group header, the order number for a row.
    private static List<string> Lines(IRenderedComponent<GroupGridHost> cut) => cut.FindAll("tbody tr:has(td)")
        .Select(row => row.ClassList.Contains("stil-data-grid-group")
            ? Spaces().Replace(row.QuerySelector(".stil-data-grid-group-toggle")!.TextContent, " ").Trim()
            : row.QuerySelectorAll("td")[0].TextContent)
        .ToList();

    private static AngleSharp.Dom.IElement Toggle(IRenderedComponent<GroupGridHost> cut, string value) =>
        cut.FindAll(".stil-data-grid-group-toggle").Single(t => t.TextContent.Contains($" {value}", StringComparison.Ordinal));

    [Fact]
    public void RowsAreGroupedWithCountsAndTotals()
    {
        using var context = new BunitContext();

        var cut = Render(context);

        Assert.Equal(["Status: New (3)", "1001", "1003", "1005", "Status: Shipped (1)", "1002", "Status: Cancelled (1)", "1004"], Lines(cut));
        var header = cut.FindAll("tr.stil-data-grid-group")[0];
        Assert.Equal("4", header.QuerySelector("td")!.GetAttribute("colspan"));
        Assert.Equal("170.00", header.QuerySelectorAll("td")[1].TextContent);
        Assert.Equal("true", header.QuerySelector("button")!.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void GroupsCollapseAndExpandWithTheMouseAndKeyboard()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        Toggle(cut, "New").Click();
        Assert.Equal(["Status: New (3)", "Status: Shipped (1)", "1002", "Status: Cancelled (1)", "1004"], Lines(cut));
        Assert.Equal("false", Toggle(cut, "New").GetAttribute("aria-expanded"));

        Toggle(cut, "New").KeyDown("ArrowRight");
        Assert.Equal(8, Lines(cut).Count);
        Toggle(cut, "Shipped").KeyDown("ArrowLeft");
        Assert.DoesNotContain("1002", Lines(cut));
    }

    [Fact]
    public void ExpandAndCollapseAllFromTheToolbar()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        cut.FindAll(".stil-data-grid-toolbar button").Single(b => b.TextContent.Trim() == "Collapse all").Click();
        Assert.Equal(["Status: New (3)", "Status: Shipped (1)", "Status: Cancelled (1)"], Lines(cut));

        cut.FindAll(".stil-data-grid-toolbar button").Single(b => b.TextContent.Trim() == "Expand all").Click();
        Assert.Equal(8, Lines(cut).Count);
    }

    [Fact]
    public void PagesCountRowsAndRepeatAContinuedGroup()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.PageSize, 2));

        Assert.Equal(["Status: New (3)", "1001", "1003"], Lines(cut));
        Assert.Equal("1–2 of 5", cut.Find(".stil-data-grid-footer span").TextContent);

        cut.FindAll(".stil-page-link").Single(link => link.TextContent == "2").Click();
        Assert.Equal(["Status: New (3)", "1005", "Status: Shipped (1)", "1002"], Lines(cut));

        // Collapsed rows leave the pages: 2 rows remain, on one page.
        Toggle(cut, "New").Click();
        Assert.Equal(["Status: New (3)", "Status: Shipped (1)", "1002", "Status: Cancelled (1)", "1004"], Lines(cut));
        Assert.Equal("1–2 of 2", cut.Find(".stil-data-grid-footer span").TextContent);
    }

    [Fact]
    public void GroupsNestAndFollowTheSortDirection()
    {
        using var context = new BunitContext();

        var cut = Render(context, groupBy: ["Paid", "Status"]);
        Assert.Equal(["Paid: No (2)", "Status: New (1)", "1003", "Status: Shipped (1)", "1002",
            "Paid: Yes (3)", "Status: New (2)", "1001", "1005", "Status: Cancelled (1)", "1004"], Lines(cut));
        Assert.Equal("1", cut.FindAll("tr.stil-data-grid-group")[1].GetAttribute("data-stil-level"));

        // Sorting by a grouped column orders its groups; other sorts order the rows in the groups.
        cut.Find("th[data-stil-column=Paid] .stil-data-grid-sort").Click();
        cut.Find("th[data-stil-column=Paid] .stil-data-grid-sort").Click();
        cut.Find("th[data-stil-column=Paid] .stil-data-grid-sort").Click();
        cut.Find("th[data-stil-column=Amount] .stil-data-grid-sort").Click();
        Assert.Equal(["Status: New (2)", "1005", "1001"], Lines(cut).Skip(6).Take(3));
        cut.Find("th[data-stil-column=Paid] .stil-data-grid-sort").Click();
        cut.Find("th[data-stil-column=Paid] .stil-data-grid-sort").Click();
        Assert.Equal("Paid: Yes (3)", Lines(cut)[0]);
    }

    [Fact]
    public void TheChooserGroupsAndReportsTheColumns()
    {
        using var context = new BunitContext();
        IReadOnlyList<string>? reported = null;
        var cut = Render(context, p => p.Add(host => host.GroupByChanged, (IReadOnlyList<string> keys) => reported = keys), groupBy: []);

        Assert.Empty(cut.FindAll("tr.stil-data-grid-group"));
        var choices = cut.FindAll(".stil-data-grid-grouping input[type=checkbox]");
        Assert.Equal(4, choices.Count); // Order cannot be grouped.

        choices[1].Change(true); // Status
        Assert.Equal(["Status"], reported);
        Assert.Equal(3, cut.FindAll("tr.stil-data-grid-group").Count);

        cut.FindAll(".stil-data-grid-grouping input[type=checkbox]")[1].Change(false);
        Assert.Empty(reported!);
        Assert.Empty(cut.FindAll("tr.stil-data-grid-group"));
    }

    [Fact]
    public void AnItemsProviderGetsTheGroupingAndComputesTheGroups()
    {
        using var context = new BunitContext();
        var requests = new List<StilDataGridRequest<GridOrder>>();
        Task<StilDataGridResult<GridOrder>> Provide(StilDataGridRequest<GridOrder> request)
        {
            requests.Add(request);
            var query = request.ApplyFiltersAndSorting(Orders.AsQueryable());
            return Task.FromResult(new StilDataGridResult<GridOrder>(
                request.ApplyPaging(query).ToList(), query.Count(), request.ComputeAggregates(query), request.ComputeGroups(query)));
        }

        var cut = Render(context, p => p.Add(host => host.ItemsProvider, Provide).Add(host => host.PageSize, 10), withItems: false);

        Assert.Equal(["Status"], requests[^1].GroupBy);
        Assert.Equal(["Status: New (3)", "1001", "1003", "1005", "Status: Shipped (1)", "1002", "Status: Cancelled (1)", "1004"], Lines(cut));
        Assert.Equal("250.00", cut.FindAll("tr.stil-data-grid-group")[1].QuerySelectorAll("td")[1].TextContent);

        // Collapsing a group needs no new groups, only rows.
        var before = requests.Count;
        Toggle(cut, "Shipped").Click();
        Assert.All(requests.Skip(before), request => Assert.Empty(request.ComputeGroups(Orders.AsQueryable())));
    }

    [Fact]
    public void VirtualizedRowsShowTheGroupHeadersInPlace()
    {
        using var context = new BunitContext();

        var cut = Render(context, p => p.Add(host => host.Virtualize, true));

        Assert.Equal(["Status: New (3)", "1001", "1003", "1005", "Status: Shipped (1)", "1002", "Status: Cancelled (1)", "1004"], Lines(cut));
        Toggle(cut, "New").Click();
        cut.WaitForAssertion(() => Assert.Equal(["Status: New (3)", "Status: Shipped (1)", "1002", "Status: Cancelled (1)", "1004"], Lines(cut)));
    }

    [Fact]
    public void SelectionAddsTheCheckboxColumnToTheHeaderSpan()
    {
        using var context = new BunitContext();

        var cut = Render(context, p => p.Add(host => host.SelectionMode, StilSelectionMode.Multiple));

        Assert.Equal("5", cut.Find("tr.stil-data-grid-group td").GetAttribute("colspan"));
        Assert.Equal(5, cut.FindAll("tbody tr:not(.stil-data-grid-group) input[type=checkbox]").Count);
    }
}
