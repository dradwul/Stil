using System.ComponentModel.DataAnnotations;

namespace Stil.Demo;

public enum OrderStatus
{
    Pending,
    Shipped,
    Delivered,
    Cancelled
}

public sealed class Order
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter the customer's name.")]
    [StringLength(60, ErrorMessage = "Keep the name under 60 characters.")]
    public string? Customer { get; set; }

    [Required(ErrorMessage = "Enter an email address.")]
    [EmailAddress(ErrorMessage = "Enter a complete email address, like name@example.com.")]
    public string? Email { get; set; }

    [Range(1, 1_000_000, ErrorMessage = "The amount must be between 1 and 1,000,000.")]
    public decimal Amount { get; set; }

    public OrderStatus Status { get; set; }

    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public bool Express { get; set; }

    public Order Clone() => (Order)MemberwiseClone();
}

/// <summary>An in-memory order list shared by all users of the demo.</summary>
public sealed class OrderStore
{
    private readonly Lock _lock = new();
    private readonly List<Order> _orders = Seed();

    /// <summary>The customers that orders are generated for, also offered as suggestions.</summary>
    public static readonly string[] Customers =
    [
        "Fjällbacka Café", "Berglund Outdoor", "Kallio Design Oy", "Østerby Marine", "Norrsken AB",
        "Solvik Bageri", "Ek & Lind Advokatbyrå", "Hvalsø Cykler", "Lappi Tours", "Visby Keramik",
    ];

    private static List<Order> Seed()
    {
        var random = new Random(1042);
        var statuses = Enum.GetValues<OrderStatus>();
        return Enumerable.Range(0, 64).Select(i =>
        {
            var customer = Customers[random.Next(Customers.Length)];
            return new Order
            {
                Id = 1001 + i,
                Customer = customer,
                Email = $"order@{customer.Split(' ')[0].ToLowerInvariant()}.example",
                Amount = Math.Round((decimal)(random.NextDouble() * 9000 + 120), 2),
                Status = i > 56 ? OrderStatus.Pending : statuses[random.Next(statuses.Length)],
                OrderDate = new DateOnly(2026, 8, 1).AddDays(i + random.Next(3)),
                Express = random.Next(4) == 0,
            };
        }).ToList();
    }

    public IReadOnlyList<Order> All()
    {
        lock (_lock)
        {
            return _orders.Select(o => o.Clone()).OrderByDescending(o => o.Id).ToList();
        }
    }

    public void Save(Order order)
    {
        lock (_lock)
        {
            var index = _orders.FindIndex(o => o.Id == order.Id);
            if (index >= 0)
            {
                _orders[index] = order.Clone();
            }
            else
            {
                order.Id = _orders.Count == 0 ? 1001 : _orders.Max(o => o.Id) + 1;
                _orders.Add(order.Clone());
            }
        }
    }

    public void Delete(IEnumerable<int> ids)
    {
        var set = ids.ToHashSet();
        lock (_lock)
        {
            _orders.RemoveAll(o => set.Contains(o.Id));
        }
    }

    public void Delete(int id)
    {
        lock (_lock)
        {
            _orders.RemoveAll(o => o.Id == id);
        }
    }
}
