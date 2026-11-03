using System.Globalization;
using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilDataGridTotalsTests
{
    private static readonly string[] Customers = ["Fjällbacka Café", "Berglund Outdoor", "Norrsken AB", "Ek; Lind \"AB\"", "=Solvik"];

    /// <summary>25 orders, 1001–1025, with amounts 100–2500. Every other order is paid.</summary>
    private static List<GridOrder> Orders() =>
        Enumerable.Range(1, 25)
            .Select(i => new GridOrder(1000 + i, Customers[i % Customers.Length], (GridOrderStatus)(i % 3), i % 2 == 0, i * 100m))
            .ToList();

    private static IRenderedComponent<TotalsGridHost> Render(BunitContext context, string culture = "en-US",
        Action<ComponentParameterCollectionBuilder<TotalsGridHost>>? configure = null, bool withProvider = false)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        return context.Render<TotalsGridHost>(parameters =>
        {
            if (!withProvider)
            {
                parameters.Add(host => host.Items, Orders());
            }

            configure?.Invoke(parameters);
        });
    }

    private static IReadOnlyList<string> Totals(IRenderedComponent<TotalsGridHost> cut) =>
        cut.FindAll("tfoot tr.stil-data-grid-totals > *").Select(cell => cell.TextContent).ToList();

    [Fact]
    public void FooterTotalsEveryFilteredRowNotOnlyThePage()
    {
        using var context = new BunitContext();

        var cut = Render(context);

        // Count of 25, the sum 100 + … + 2500 = 32 500, and the own aggregate (12 of 25 paid).
        cut.WaitForAssertion(() => Assert.Equal(["25", "", "", "32,500.00", "48%", ""], Totals(cut)));
        Assert.Equal(10, cut.FindAll("tbody tr").Count);
    }

    [Theory]
    [InlineData(StilAggregate.Average, "1,300.00")]
    [InlineData(StilAggregate.Min, "100.00")]
    [InlineData(StilAggregate.Max, "2,500.00")]
    [InlineData(StilAggregate.Count, "25")]
    public void EachAggregateIsComputed(StilAggregate aggregate, string expected)
    {
        using var context = new BunitContext();

        var cut = Render(context, configure: p => p.Add(host => host.AmountAggregate, aggregate));

        cut.WaitForAssertion(() => Assert.Equal(expected, Totals(cut)[3]));
    }

    [Fact]
    public void TotalsFollowTheFilter()
    {
        using var context = new BunitContext();
        var cut = Render(context);
        cut.WaitForAssertion(() => Assert.Equal("25", Totals(cut)[0]));

        cut.FindAll("thead select")[0].Change("true");

        Assert.Equal("12", Totals(cut)[0]);
        Assert.Equal("100%", Totals(cut)[4]);
    }

    [Fact]
    public void TheFirstColumnIsLabelledWhenItHasNoTotal()
    {
        using var context = new BunitContext();

        var cut = Render(context, configure: p => p.Add(host => host.SelectionMode, StilSelectionMode.Multiple));

        // The checkbox column comes first and stays empty; Order has a total, so there is no label.
        Assert.Equal(7, cut.FindAll("tfoot tr > *").Count);
        Assert.Equal("", cut.FindAll("tfoot tr > *")[0].TextContent);
    }

    [Fact]
    public async Task ItemsProviderGetsTheAggregatesToCompute()
    {
        using var context = new BunitContext();
        StilDataGridRequest<GridOrder>? seen = null;

        var cut = Render(context, configure: p => p
            .Add(host => host.ItemsProvider, request =>
            {
                seen = request;
                var query = request.ApplyFiltersAndSorting(Orders().AsQueryable());
                return Task.FromResult(new StilDataGridResult<GridOrder>(
                    request.ApplyPaging(query).ToList(), query.Count(), request.ComputeAggregates(query)));
            }), withProvider: true);

        await cut.InvokeAsync(() => cut.Instance.Grid.RefreshAsync());

        Assert.Equal(["Id", "Amount"], seen!.Aggregates);
        cut.WaitForAssertion(() => Assert.Equal("32,500.00", Totals(cut)[3]));
    }

    [Fact]
    public async Task ExportHasEveryFilteredRowWithCellTexts()
    {
        using var context = new BunitContext();
        var cut = Render(context);

        var csv = await cut.InvokeAsync(() => cut.Instance.Grid.ExportCsvAsync());

        var lines = csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(26, lines.Length);
        Assert.Equal("Order,Customer,Paid,Amount,Share paid", lines[0]);
        Assert.Equal("1001,Berglund Outdoor,No,100,0", lines[1]);
        // A separator and quotes are quoted, and text that starts like a formula is defused.
        Assert.Equal("1003,Ek; Lind \"\"AB\"\",No,300,0".Replace("Ek; Lind \"\"AB\"\"", "\"Ek; Lind \"\"AB\"\"\""), lines[3]);
        Assert.Equal("1004,'=Solvik,Yes,400,1", lines[4]);
    }

    [Fact]
    public async Task ExportUsesTheCulturesSeparatorAndDecimals()
    {
        using var context = new BunitContext();
        var cut = Render(context, culture: "sv-SE");

        var csv = await cut.InvokeAsync(() => cut.Instance.Grid.ExportCsvAsync());

        var lines = csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("Order;Customer;Paid;Amount;Share paid", lines[0]);
        Assert.Equal("1003;\"Ek; Lind \"\"AB\"\"\";No;300;0", lines[3]);
        cut.WaitForAssertion(() => Assert.Contains("32 500,00", Totals(cut)[3].Replace(' ', ' ')));
    }

    [Fact]
    public async Task ExportPagesThroughTheItemsProvider()
    {
        using var context = new BunitContext();
        var many = Enumerable.Range(1, 1200).Select(i => new GridOrder(i, "C", GridOrderStatus.New, false, i)).ToList();
        var requests = new List<(int Start, int? Count)>();

        var cut = Render(context, configure: p => p
            .Add(host => host.ItemsProvider, request =>
            {
                requests.Add((request.StartIndex, request.Count));
                var query = request.ApplyFiltersAndSorting(many.AsQueryable());
                return Task.FromResult(new StilDataGridResult<GridOrder>(request.ApplyPaging(query).ToList(), query.Count()));
            }), withProvider: true);
        // The first load, and the reload once the columns with totals are registered.
        cut.WaitForState(() => requests.Count >= 2);
        requests.Clear();

        var csv = await cut.InvokeAsync(() => cut.Instance.Grid.ExportCsvAsync());

        Assert.Equal(1201, csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.Equal([(0, 500), (500, 500), (1000, 500)], requests.Select(r => (r.Start, r.Count)));
    }

    [Fact]
    public void ExportButtonDownloadsTheCsvWithAByteOrderMark()
    {
        using var context = new BunitContext();
        var module = context.JSInterop.SetupModule("./_content/Stil/stil.js");
        module.SetupVoid("downloadFile", _ => true);

        var cut = Render(context, configure: p => p.Add(host => host.ShowExport, true));
        cut.Find(".stil-data-grid-export").Click();

        cut.WaitForState(() => module.Invocations["downloadFile"].Count == 1);
        var call = module.Invocations["downloadFile"].Single();
        Assert.Equal("orders.csv", call.Arguments[0]);
        Assert.StartsWith("﻿Order,Customer", (string)call.Arguments[2]!);
        Assert.Contains("Export CSV", cut.Find(".stil-data-grid-export").TextContent);
    }
}
