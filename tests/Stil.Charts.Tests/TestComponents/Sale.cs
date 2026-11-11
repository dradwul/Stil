namespace Stil.Charts.Tests.TestComponents;

public sealed record Sale(string Month, decimal LastYear, decimal? ThisYear);

public sealed record Share(string Name, int Amount);

public static class TestData
{
    public static List<Sale> Sales() =>
    [
        new("Jan", 1200m, 1500m),
        new("Feb", 900m, 1700m),
        new("Mar", 2100m, 2600m),
        new("Apr", 2500m, null),
        new("May", 3000m, 4300m)
    ];
}

public sealed record Store(string Name, string Region, int Visitors, decimal? Revenue, int? Staff);
