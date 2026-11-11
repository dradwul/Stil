namespace StilApp.Data;

/// <summary>
/// The sales figures on the dashboard. The app registers <see cref="SampleSalesService"/>; replace it
/// in Program.cs with one that reads your own data. The pages do not change.
/// </summary>
public interface ISalesService
{
    /// <summary>Revenue per month, this year and last year, in thousands.</summary>
    IReadOnlyList<MonthlySales> Months();

    /// <summary>Orders per quarter and channel.</summary>
    IReadOnlyList<QuarterlyOrders> Quarters();

    /// <summary>Revenue per region this year.</summary>
    IReadOnlyList<RegionSales> Regions();

    /// <summary>Every product with its sales.</summary>
    IReadOnlyList<ProductSales> Products();
}

/// <param name="ThisYear">Null for the months that have not ended yet.</param>
public sealed record MonthlySales(string Month, decimal LastYear, decimal? ThisYear, int Orders, int Customers);

public sealed record QuarterlyOrders(string Quarter, int WebShop, int Stores, int Wholesale);

public sealed record RegionSales(string Region, decimal Revenue);

/// <param name="Weekly">Units sold per week, the last twelve weeks.</param>
public sealed record ProductSales(string Name, string Category, decimal Price, int Units, decimal Revenue, IReadOnlyList<double> Weekly);

/// <summary>Sample figures for a shop that sells furniture.</summary>
public sealed class SampleSalesService : ISalesService
{
    private static readonly string[] MonthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public IReadOnlyList<MonthlySales> Months()
    {
        decimal[] lastYear = [182, 175, 198, 210, 236, 251, 229, 240, 262, 270, 288, 320];
        decimal[] thisYear = [204, 211, 236, 249, 268, 290, 284, 301, 318, 327, 341, 372];
        var current = DateTime.Today.Month;
        return [.. MonthNames.Select((name, i) => new MonthlySales(name, lastYear[i], i < current ? thisYear[i] : null,
            (int)(thisYear[i] * 6.1m), (int)(thisYear[i] * 2.3m)))];
    }

    public IReadOnlyList<QuarterlyOrders> Quarters() =>
    [
        new("Q1", 1840, 2210, 610), new("Q2", 2310, 2050, 720), new("Q3", 2790, 1980, 690), new("Q4", 3420, 2600, 840),
    ];

    public IReadOnlyList<RegionSales> Regions() =>
    [
        new("Sweden", 1_184_000), new("Norway", 962_000), new("Denmark", 549_000), new("Finland", 413_000),
    ];

    public IReadOnlyList<ProductSales> Products()
    {
        (string Name, string Category, decimal Price)[] products =
        [
            ("Oak desk", "Desks", 6_490), ("Standing desk", "Desks", 8_990), ("Meeting table", "Tables", 12_900),
            ("Task chair", "Chairs", 3_490), ("Lounge chair", "Chairs", 5_990), ("Stool", "Chairs", 890),
            ("Floor lamp", "Lamps", 1_790), ("Desk lamp", "Lamps", 690), ("Bookcase", "Storage", 2_990),
            ("Drawer unit", "Storage", 1_590),
        ];

        var random = new Random(7);
        return [.. products.Select(p =>
        {
            double level = random.Next(8, 60);
            var weekly = Enumerable.Range(0, 12).Select(_ => level = Math.Max(1, level + random.Next(-6, 8))).ToList();
            var units = (int)weekly.Sum();
            return new ProductSales(p.Name, p.Category, p.Price, units, units * p.Price, weekly);
        })];
    }
}
