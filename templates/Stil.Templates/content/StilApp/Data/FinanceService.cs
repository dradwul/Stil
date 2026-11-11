namespace StilApp.Data;

/// <summary>
/// The results, budgets, transactions and notes on the finance pages. The app registers
/// <see cref="SampleFinanceService"/>; replace it in Program.cs with one that reads your own ledger.
/// The pages do not change.
/// </summary>
public interface IFinanceService
{
    /// <summary>Revenue, costs and budget per month, in thousands; null for the months to come.</summary>
    IReadOnlyList<MonthResult> Months();

    /// <summary>Each department's budget for the year and what it has spent.</summary>
    IReadOnlyList<DepartmentBudget> Budgets();

    /// <summary>The year's transactions.</summary>
    IReadOnlyList<Transaction> Transactions();

    /// <summary>The month's notes, in Markdown.</summary>
    string Notes { get; set; }
}

public sealed record MonthResult(string Month, decimal Budget, decimal? Revenue, decimal? Costs)
{
    public decimal? Result => Revenue - Costs;

    /// <summary>The result as a share of revenue, in percent.</summary>
    public double? Margin => Revenue is > 0 ? (double)(Result! / Revenue * 100) : null;
}

public sealed record DepartmentBudget(string Department, decimal Budget, decimal Spent);

/// <param name="Amount">Positive for income, negative for costs.</param>
public sealed record Transaction(int Id, DateOnly Date, string Account, string Description, string Category, decimal Amount);

/// <summary>Sample figures for a small company.</summary>
public sealed class SampleFinanceService : IFinanceService
{
    private static readonly string[] MonthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    private readonly Lock _lock = new();
    private string _notes = """
        ## This month
        Revenue is **4% over budget**, mostly from the web shop. Costs follow the plan,
        except marketing, which spent its autumn campaign early.

        - [x] Invoices sent for all delivered orders
        - [ ] Follow up the two overdue invoices from Kallio Design
        - [ ] Move 20 000 from marketing to IT for the new laptops
        """;

    public string Notes
    {
        get { lock (_lock) { return _notes; } }
        set { lock (_lock) { _notes = value; } }
    }

    public IReadOnlyList<MonthResult> Months()
    {
        decimal[] budget = [410, 400, 440, 450, 470, 490, 430, 450, 500, 510, 530, 590];
        decimal[] revenue = [402, 418, 452, 449, 491, 503, 441, 466, 527, 531, 549, 612];
        decimal[] costs = [318, 322, 340, 331, 352, 361, 334, 340, 371, 368, 380, 405];
        var current = DateTime.Today.Month;
        return [.. MonthNames.Select((name, i) => new MonthResult(name, budget[i],
            i < current ? revenue[i] : null, i < current ? costs[i] : null))];
    }

    public IReadOnlyList<DepartmentBudget> Budgets() =>
    [
        new("Sales", 1_200_000, 940_000),
        new("Marketing", 600_000, 571_000),
        new("IT", 450_000, 298_000),
        new("Operations", 900_000, 702_000),
        new("Administration", 300_000, 214_000),
    ];

    public IReadOnlyList<Transaction> Transactions()
    {
        (string Account, string Description, string Category, decimal Amount)[] kinds =
        [
            ("3010 Sales", "Web shop payout", "Income", 48_200),
            ("3010 Sales", "Invoice paid, Norrsken AB", "Income", 21_750),
            ("3040 Services", "Installation, Berglund Outdoor", "Income", 8_400),
            ("4010 Goods", "Furniture from Nordvik Supply", "Goods", -26_300),
            ("5010 Rent", "Office and warehouse", "Premises", -18_000),
            ("5410 IT", "Software subscriptions", "IT", -3_150),
            ("5910 Marketing", "Search ads", "Marketing", -6_800),
            ("6110 Office", "Office supplies", "Administration", -940),
            ("7010 Salaries", "Salaries", "Staff", -61_500),
            ("6570 Bank", "Bank fees", "Administration", -320),
        ];

        var random = new Random(11);
        var start = new DateOnly(DateTime.Today.Year, 1, 1);
        var days = DateOnly.FromDateTime(DateTime.Today).DayNumber - start.DayNumber;
        return [.. Enumerable.Range(0, 120).Select(i =>
        {
            var kind = kinds[random.Next(kinds.Length)];
            var amount = Math.Round(kind.Amount * (0.6m + (decimal)random.NextDouble() * 0.8m), 0);
            return new Transaction(1000 + i, start.AddDays(random.Next(0, Math.Max(days, 1))), kind.Account, kind.Description, kind.Category, amount);
        }).OrderByDescending(t => t.Date)];
    }
}
