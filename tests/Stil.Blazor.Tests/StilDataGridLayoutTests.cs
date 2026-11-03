using System.Globalization;
using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilDataGridLayoutTests
{
    private static readonly List<GridOrder> Orders =
    [
        new(1001, "Fjällbacka Café", GridOrderStatus.New, true, 100m),
        new(1002, "Berglund Outdoor", GridOrderStatus.Shipped, false, 250m),
    ];

    private static IRenderedComponent<LayoutGridHost> Render(BunitContext context,
        Action<ComponentParameterCollectionBuilder<LayoutGridHost>>? configure = null)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context.Render<LayoutGridHost>(parameters =>
        {
            parameters.Add(host => host.Items, Orders);
            configure?.Invoke(parameters);
        });
    }

    private static IReadOnlyList<string?> HeaderKeys(IRenderedComponent<LayoutGridHost> cut) =>
        cut.FindAll("thead tr:first-child th[data-stil-column]").Select(th => th.GetAttribute("data-stil-column")).ToList();

    [Fact]
    public void HeadersCarryTheLayoutHooks()
    {
        using var context = new BunitContext();

        var cut = Render(context);

        var customer = cut.Find("th[data-stil-column=Customer]");
        Assert.NotNull(customer.GetAttribute("data-stil-reorder"));
        Assert.Equal("120", customer.GetAttribute("data-stil-min"));
        Assert.Equal("400", customer.GetAttribute("data-stil-max"));
        Assert.Equal("Resize Customer", customer.QuerySelector(".stil-data-grid-resize")!.GetAttribute("aria-label"));
        Assert.Null(cut.Find("th[data-stil-column=Id]").GetAttribute("data-stil-reorder"));
        Assert.Null(cut.Find("th[data-stil-column=Amount]").QuerySelector(".stil-data-grid-resize"));
    }

    [Fact]
    public void FrozenColumnsAreMarkedAndPlacedByStilJs()
    {
        using var context = new BunitContext();

        var cut = Render(context, p => p.Add(host => host.SelectionMode, StilSelectionMode.Multiple));

        Assert.Equal("start", cut.Find("th[data-stil-column=Id]").GetAttribute("data-stil-frozen"));
        Assert.Equal("start", cut.Find("th.stil-data-grid-select").GetAttribute("data-stil-frozen"));
        Assert.All(cut.FindAll("tbody tr"), row => Assert.Equal("start", row.QuerySelectorAll("td")[1].GetAttribute("data-stil-frozen")));
        Assert.NotEmpty(context.JSInterop.Invocations["freezeColumns"]);
    }

    [Fact]
    public void ResizingSetsTheWidthAndReportsTheLayout()
    {
        using var context = new BunitContext();
        var cut = Render(context);
        cut.Find(".stil-data-grid").TriggerEvent("onstilcolumnresize", new StilColumnResizeEventArgs { Key = "Customer", Width = 182.4 });

        Assert.Equal("182px", cut.Instance.Layout!.Widths["Customer"]);
        Assert.Equal("width: 182px; min-width: 182px", cut.Find("th[data-stil-column=Customer]").GetAttribute("style"));
    }

    [Fact]
    public void MovingAColumnReordersHeadersAndCells()
    {
        using var context = new BunitContext();

        var cut = Render(context);
        cut.Find(".stil-data-grid").TriggerEvent("onstilcolumnmove", new StilColumnMoveEventArgs { Key = "Amount", Before = "Customer" });

        Assert.Equal(["Id", "Amount", "Customer", "Status"], HeaderKeys(cut));
        Assert.Equal("100", cut.FindAll("tbody tr")[0].QuerySelectorAll("td")[1].TextContent);
        Assert.Equal(["Id", "Amount", "Customer", "Status"], cut.Instance.Layout!.Order);
    }

    [Fact]
    public void TheChooserHidesShowsAndMovesColumns()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        var boxes = cut.FindAll(".stil-data-grid-chooser input[type=checkbox]");
        Assert.True(boxes[0].HasAttribute("disabled")); // Order cannot be hidden.
        boxes[2].Change(false); // Status

        Assert.Equal(["Id", "Customer", "Amount"], HeaderKeys(cut));
        Assert.Equal(["Status"], cut.Instance.Layout!.Hidden);
        Assert.Equal(3, cut.FindAll("tbody tr")[0].QuerySelectorAll("td").Length);

        cut.FindAll(".stil-data-grid-chooser button").Single(b => b.GetAttribute("aria-label") == "Move Customer later").Click();
        // The chooser lists hidden columns too, so Customer moves past the hidden Status.
        Assert.Equal(["Id", "Status", "Customer", "Amount"], cut.Instance.Layout!.Order);
        Assert.Equal(["Id", "Customer", "Amount"], HeaderKeys(cut));

        cut.FindAll(".stil-data-grid-chooser input[type=checkbox]")[1].Change(true); // Status, now second
        Assert.Equal(["Id", "Status", "Customer", "Amount"], HeaderKeys(cut));
    }

    [Fact]
    public void ABoundLayoutIsRestored()
    {
        using var context = new BunitContext();

        var cut = Render(context, p => p.Add(host => host.Layout, new StilDataGridLayout(
            ["Id", "Status", "Amount", "Customer"],
            new Dictionary<string, string> { ["Status"] = "9rem" },
            ["Customer"],
            "Amount",
            StilSortDirection.Descending)));

        Assert.Equal(["Id", "Status", "Amount"], HeaderKeys(cut));
        Assert.Equal("width: 9rem; min-width: 9rem", cut.Find("th[data-stil-column=Status]").GetAttribute("style"));
        Assert.Equal("descending", cut.Find("th[data-stil-column=Amount]").GetAttribute("aria-sort"));
        Assert.Equal("1002", cut.FindAll("tbody tr")[0].QuerySelectorAll("td")[0].TextContent);
    }

    [Fact]
    public void SortingIsPartOfTheLayout()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        cut.Find("th[data-stil-column=Customer] .stil-data-grid-sort").Click();

        Assert.Equal("Customer", cut.Instance.Layout!.SortBy);
        Assert.Equal(StilSortDirection.Ascending, cut.Instance.Layout!.SortDirection);
    }

    [Fact]
    public async Task ExportFollowsTheVisibleColumnsAndResetRestores()
    {
        using var context = new BunitContext();
        var cut = Render(context, p => p.Add(host => host.Layout, new StilDataGridLayout(["Amount", "Id"], new Dictionary<string, string>(), ["Status"])));

        var csv = await cut.InvokeAsync(() => cut.Instance.Grid.ExportCsvAsync());
        Assert.StartsWith("Amount,Order,Customer\r\n", csv);

        await cut.InvokeAsync(() => cut.Instance.Grid.ResetLayoutAsync());
        Assert.Equal(["Id", "Customer", "Status", "Amount"], HeaderKeys(cut));
        Assert.Empty(cut.Instance.Layout!.Hidden);
    }
}
