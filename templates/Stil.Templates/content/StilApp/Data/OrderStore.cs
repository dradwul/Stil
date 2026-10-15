namespace StilApp.Data;

/// <summary>
/// Sample orders, kept in memory until the app restarts. Replace it with your own data access,
/// for example an EF Core DbContext.
/// </summary>
public sealed class OrderStore
{
    private static readonly string[] Customers =
    [
        "Fjällbacka Café", "Berglund Outdoor", "Kallio Design", "Østerby Marine", "Norrsken AB",
        "Solvik Bageri", "Hvalsø Cykler", "Lappi Tours",
    ];

    private readonly Lock _lock = new();
    private readonly List<Order> _orders = Seed();

    public IReadOnlyList<Order> All()
    {
        lock (_lock)
        {
            return [.. _orders];
        }
    }

    public void Add(Order order)
    {
        lock (_lock)
        {
            order.Id = _orders.Count == 0 ? 1001 : _orders.Max(o => o.Id) + 1;
            _orders.Add(order);
        }
    }

    private static List<Order> Seed()
    {
        var random = new Random(42);
        var statuses = Enum.GetValues<OrderStatus>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        return [.. Enumerable.Range(0, 24).Select(i =>
        {
            var customer = Customers[random.Next(Customers.Length)];
            return new Order
            {
                Id = 1001 + i,
                Customer = customer,
                Email = $"orders@{customer.Split(' ')[0].ToLowerInvariant()}.example",
                Amount = random.Next(50, 5000) + random.Next(0, 100) / 100m,
                Status = statuses[random.Next(statuses.Length)],
                OrderDate = today.AddDays(i - 24),
                Express = random.Next(4) == 0,
            };
        })];
    }
}
