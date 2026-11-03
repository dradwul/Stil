namespace Stil.Blazor.Tests.TestComponents;

/// <summary>A cost centre of a budget, with the cost centres below it.</summary>
public sealed class BudgetLine
{
    public string Name { get; set; } = "";

    public decimal Amount { get; set; }

    public List<BudgetLine>? Children { get; set; }
}
