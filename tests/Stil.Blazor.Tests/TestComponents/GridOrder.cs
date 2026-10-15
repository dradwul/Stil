namespace Stil.Blazor.Tests.TestComponents;

public enum GridOrderStatus
{
    New,
    Shipped,
    Cancelled
}

public sealed record GridOrder(int Id, string Customer, GridOrderStatus Status, bool Paid, decimal Amount);
