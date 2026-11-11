namespace StilApp.Data;

/// <summary>
/// The orders the pages show and change. The app registers <see cref="InMemoryOrderService"/>, with
/// sample orders; replace it with your own implementation, for example on an EF Core DbContext, in
/// Program.cs. The pages do not change.
/// </summary>
public interface IOrderService
{
    /// <summary>Every order, newest last.</summary>
    IReadOnlyList<Order> All();

    /// <summary>Adds an order and gives it the next id.</summary>
    void Add(Order order);
#if (IsAdmin)

    /// <summary>Replaces the order with the same id.</summary>
    void Update(Order order);

    /// <summary>Deletes the order with the id.</summary>
    void Delete(int id);

    /// <summary>What happened to the orders, newest first.</summary>
    IReadOnlyList<OrderEvent> History();
#endif
}
#if (IsAdmin)

/// <summary>A change to an order, for the history page.</summary>
public sealed record OrderEvent(int Id, DateTime Time, int OrderId, string Customer, string Action, string? Details = null);
#endif

/// <summary>Sample orders, kept in memory until the app restarts.</summary>
public sealed class InMemoryOrderService : IOrderService
{
    private static readonly string[] Customers =
    [
        "Fjällbacka Café", "Berglund Outdoor", "Kallio Design", "Østerby Marine", "Norrsken AB",
        "Solvik Bageri", "Hvalsø Cykler", "Lappi Tours",
    ];

    private readonly Lock _lock = new();
    private readonly List<Order> _orders = Seed();
#if (IsAdmin)
    private readonly List<OrderEvent> _history = [];

    public InMemoryOrderService()
    {
        foreach (var order in _orders.TakeLast(6))
        {
            Record(order, "Created", $"{order.Amount:N2}", order.OrderDate.ToDateTime(new TimeOnly(9, 30)));
        }
    }
#endif

    public IReadOnlyList<Order> All()
    {
        lock (_lock)
        {
            return [.. _orders.Select(o => o.Copy())];
        }
    }

    public void Add(Order order)
    {
        lock (_lock)
        {
            order.Id = _orders.Count == 0 ? 1001 : _orders.Max(o => o.Id) + 1;
            _orders.Add(order.Copy());
#if (IsAdmin)
            Record(order, "Created", $"{order.Amount:N2}");
#endif
        }
    }
#if (IsAdmin)

    public void Update(Order order)
    {
        lock (_lock)
        {
            var index = _orders.FindIndex(o => o.Id == order.Id);
            if (index < 0)
            {
                return;
            }

            var before = _orders[index];
            _orders[index] = order.Copy();
            Record(order, "Updated", before.Status == order.Status ? null : $"{before.Status} → {order.Status}");
        }
    }

    public void Delete(int id)
    {
        lock (_lock)
        {
            var order = _orders.Find(o => o.Id == id);
            if (order is not null)
            {
                _orders.Remove(order);
                Record(order, "Deleted");
            }
        }
    }

    public IReadOnlyList<OrderEvent> History()
    {
        lock (_lock)
        {
            return [.. _history];
        }
    }

    private void Record(Order order, string action, string? details = null, DateTime? time = null) =>
        _history.Insert(0, new(_history.Count + 1, time ?? DateTime.Now, order.Id, order.Customer ?? "", action, details));
#endif

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
