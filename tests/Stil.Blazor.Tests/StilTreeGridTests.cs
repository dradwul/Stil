using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilTreeGridTests
{
    // Sales 350 (Nordic 100, Export 250); Operations 600 (Facilities 400, IT 200 (Servers 120, Laptops 80)).
    private static List<BudgetLine> Budget() =>
    [
        new() { Name = "Sales", Children = [new() { Name = "Nordic", Amount = 100m }, new() { Name = "Export", Amount = 250m }] },
        new()
        {
            Name = "Operations",
            Children =
            [
                new() { Name = "Facilities", Amount = 400m },
                new() { Name = "IT", Amount = 999m, Children = [new() { Name = "Servers", Amount = 120m }, new() { Name = "Laptops", Amount = 80m }] },
            ],
        },
    ];

    private static IRenderedComponent<TreeGridHost> Render(BunitContext context,
        Action<ComponentParameterCollectionBuilder<TreeGridHost>>? configure = null, List<BudgetLine>? items = null)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context.Render<TreeGridHost>(parameters =>
        {
            parameters.Add(host => host.Items, items ?? Budget());
            configure?.Invoke(parameters);
        });
    }

    private static AngleSharp.Dom.IElement RowOf(IRenderedComponent<TreeGridHost> cut, string name) =>
        cut.FindAll("tbody tr").Single(tr => tr.QuerySelector("td")!.TextContent.Trim().EndsWith(name, StringComparison.Ordinal)); // After a loading text.

    private static List<string> Rows(IRenderedComponent<TreeGridHost> cut) =>
        cut.FindAll("tbody tr").Select(tr => string.Join(" ", tr.QuerySelectorAll("td").Select(td => td.TextContent.Trim()))).ToList();

    private static void Press(AngleSharp.Dom.IElement row, string key) => row.KeyDown(new KeyboardEventArgs { Key = key });

    [Fact]
    public void TopLevelRowsStartCollapsedInATreegrid()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        Assert.Equal("treegrid", cut.Find("table").GetAttribute("role"));
        Assert.Contains("stil-tree-grid", cut.Find(".stil-data-grid").ClassList);
        Assert.Equal(["Sales 350", "Operations 600"], Rows(cut));
        var sales = RowOf(cut, "Sales");
        Assert.Equal("1", sales.GetAttribute("aria-level"));
        Assert.Equal("false", sales.GetAttribute("aria-expanded"));
        Assert.Equal("2", sales.GetAttribute("aria-setsize"));
        Assert.Equal("1", sales.GetAttribute("aria-posinset"));
        Assert.Equal("0", sales.GetAttribute("tabindex"));
        Assert.Equal("-1", RowOf(cut, "Operations").GetAttribute("tabindex"));
        Assert.Empty(cut.FindAll(".stil-pagination"));
    }

    [Fact]
    public void TheToggleExpandsARowBelowItsParent()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        RowOf(cut, "Operations").QuerySelector(".stil-tree-grid-toggle")!.Click();

        Assert.Equal(["Sales 350", "Operations 600", "Facilities 400", "IT 200"], Rows(cut));
        Assert.Equal("true", RowOf(cut, "Operations").GetAttribute("aria-expanded"));
        var it = RowOf(cut, "IT");
        Assert.Equal("2", it.GetAttribute("aria-level"));
        Assert.Equal("--_level: 1", it.GetAttribute("style"));
        Assert.Null(RowOf(cut, "Facilities").GetAttribute("aria-expanded"));

        RowOf(cut, "Operations").QuerySelector(".stil-tree-grid-toggle")!.Click();

        Assert.Equal(["Sales 350", "Operations 600"], Rows(cut));
    }

    [Fact]
    public void ParentsShowTheTotalsOfTheRowsBelowThem()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.ExpandedLevels, 2));

        // IT's own amount (999) is replaced by its children's; the footer adds up the leaves.
        Assert.Equal(["Sales 350", "Nordic 100", "Export 250", "Operations 600", "Facilities 400", "IT 200", "Servers 120", "Laptops 80"], Rows(cut));
        Assert.Equal("950", cut.FindAll("tfoot td")[^1].TextContent.Trim());
    }

    [Fact]
    public void SortingSortsTheRowsUnderEachParent()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.ExpandedLevels, 1));

        cut.FindAll("th button.stil-data-grid-sort")[0].Click();

        Assert.Equal(["Operations 600", "Facilities 400", "IT 200", "Sales 350", "Export 250", "Nordic 100"], Rows(cut));
    }

    [Fact]
    public async Task FiltersKeepMatchesWithTheRowsAboveAndBelowThem()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.Filterable, true));

        cut.Find("input[type=search]").Input("serv");
        cut.WaitForAssertion(() => Assert.Equal(["Operations 120", "IT 120", "Servers 120"], Rows(cut)), TimeSpan.FromSeconds(2));
        Assert.Equal("120", cut.FindAll("tfoot td")[^1].TextContent.Trim());

        // A matching parent keeps all its rows.
        cut.Find("input[type=search]").Input("sales");
        cut.WaitForAssertion(() => Assert.Equal(["Sales 350"], Rows(cut)), TimeSpan.FromSeconds(2));
        await cut.InvokeAsync(() => RowOf(cut, "Sales").QuerySelector(".stil-tree-grid-toggle")!.Click());
        Assert.Equal(["Sales 350", "Nordic 100", "Export 250"], Rows(cut));
    }

    [Fact]
    public void ArrowKeysMoveBetweenRowsAndExpandThem()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        Press(RowOf(cut, "Sales"), "ArrowDown");
        Assert.Equal("0", RowOf(cut, "Operations").GetAttribute("tabindex"));
        Assert.Equal("-1", RowOf(cut, "Sales").GetAttribute("tabindex"));
        Assert.NotEmpty(context.JSInterop.Invocations["focusRow"]);

        Press(RowOf(cut, "Operations"), "ArrowRight");
        Assert.Equal("true", RowOf(cut, "Operations").GetAttribute("aria-expanded"));

        Press(RowOf(cut, "Operations"), "ArrowRight");
        Assert.Equal("0", RowOf(cut, "Facilities").GetAttribute("tabindex"));

        Press(RowOf(cut, "Facilities"), "End");
        Assert.Equal("0", RowOf(cut, "IT").GetAttribute("tabindex"));

        Press(RowOf(cut, "IT"), "ArrowLeft");
        Assert.Equal("0", RowOf(cut, "Operations").GetAttribute("tabindex"));

        Press(RowOf(cut, "Operations"), "ArrowLeft");
        Assert.Equal(["Sales 350", "Operations 600"], Rows(cut));

        Press(RowOf(cut, "Operations"), "Home");
        Assert.Equal("0", RowOf(cut, "Sales").GetAttribute("tabindex"));
    }

    [Fact]
    public async Task ChildrenLoadOnDemandWithALoadingState()
    {
        using var context = new BunitContext();
        var loading = new TaskCompletionSource<IEnumerable<BudgetLine>>();
        var calls = new List<string>();
        var cut = Render(context, p => p.Add(host => host.LoadChildren, line =>
            {
                calls.Add(line.Name);
                return line.Name == "Sales" ? loading.Task : Task.FromResult<IEnumerable<BudgetLine>>([]);
            }),
            items: [new BudgetLine { Name = "Sales" }, new BudgetLine { Name = "Legal" }]);

        Assert.Equal("false", RowOf(cut, "Sales").GetAttribute("aria-expanded"));
        RowOf(cut, "Sales").QuerySelector(".stil-tree-grid-toggle")!.Click();

        Assert.Equal("true", RowOf(cut, "Sales").GetAttribute("aria-busy"));
        Assert.NotNull(RowOf(cut, "Sales").QuerySelector(".stil-spinner[role=status]"));

        await cut.InvokeAsync(() => loading.SetResult([new BudgetLine { Name = "Nordic", Amount = 100m }]));

        cut.WaitForAssertion(() => Assert.Equal(["Sales 100", "Nordic 100", "Legal 0"], Rows(cut)));
        Assert.Null(RowOf(cut, "Sales").GetAttribute("aria-busy"));

        // Loaded once; a row whose loaded children are none becomes a leaf.
        RowOf(cut, "Sales").QuerySelector(".stil-tree-grid-toggle")!.Click();
        RowOf(cut, "Sales").QuerySelector(".stil-tree-grid-toggle")!.Click();
        RowOf(cut, "Legal").QuerySelector(".stil-tree-grid-toggle")!.Click();
        Assert.Equal(["Sales", "Legal"], calls);
        Assert.Null(RowOf(cut, "Legal").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task ExpandAllAndCollapseAll()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        await cut.InvokeAsync(() => cut.Instance.Grid.ExpandAllAsync());
        Assert.Equal(8, Rows(cut).Count);

        await cut.InvokeAsync(() => cut.Instance.Grid.CollapseAllAsync());
        Assert.Equal(["Sales 350", "Operations 600"], Rows(cut));

        await cut.InvokeAsync(() => cut.Instance.Grid.ExpandAsync(cut.Instance.Items[0]));
        Assert.Equal(["Sales 350", "Nordic 100", "Export 250", "Operations 600"], Rows(cut));
    }

    [Fact]
    public async Task TheExportHasEveryRowInTreeOrder()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        var csv = await cut.InvokeAsync(() => cut.Instance.Grid.ExportCsvAsync());

        var names = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')[0]).ToList();
        Assert.Equal(["Sales", "Nordic", "Export", "Operations", "Facilities", "IT", "Servers", "Laptops"], names);
    }

    [Fact]
    public void AnItemsProviderIsNotSupported()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var error = Assert.Throws<InvalidOperationException>(() => context.Render<StilTreeGrid<BudgetLine>>(p => p
            .Add(g => g.ItemsProvider, _ => Task.FromResult(new StilDataGridResult<BudgetLine>([], 0)))));

        Assert.Contains("Items", error.Message);
    }
}
