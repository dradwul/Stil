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

    /// <summary>When the order came in, for the orders-per-hour heat map.</summary>
    public TimeOnly OrderTime { get; set; } = new(12, 0);

    /// <summary>What the order cost us, for the margin. Not edited in the demo.</summary>
    public decimal Cost { get; set; }

    /// <summary>Internal notes, written in Markdown.</summary>
    [MaxLength(2000, ErrorMessage = "Keep the notes under 2,000 characters.")]
    public string? Notes { get; set; }

    /// <summary>The product description, written in the rich text editor and stored as sanitized HTML.</summary>
    [MaxLength(4000, ErrorMessage = "Keep the description under 4,000 characters.")]
    public string? Description { get; set; }

    /// <summary>The margin in percent of the amount.</summary>
    public double Margin => Amount <= 0 ? 0 : (double)((Amount - Cost) / Amount) * 100;

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
        // Times, costs and texts come from a second generator, so the orders above stay the same.
        var extra = new Random(2026);
        var statuses = Enum.GetValues<OrderStatus>();
        var orders = Enumerable.Range(0, 64).Select(i =>
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

        foreach (var order in orders)
        {
            // Most orders come in around lunch and in the evening.
            var hour = extra.Next(3) switch { 0 => 11 + extra.Next(3), 1 => 18 + extra.Next(4), _ => 8 + extra.Next(14) };
            order.OrderTime = new TimeOnly(hour, extra.Next(60));
            order.Cost = Math.Round(order.Amount * (decimal)(0.58 + extra.NextDouble() * 0.2), 2);
        }

        var newest = orders[^1];
        newest.Notes = """
            Customer asked for delivery **before Friday**.

            - Call before delivery: 070-123 45 67
            - Use the back entrance
            """;
        newest.Description = "<h2>Oak desk</h2><p>A <strong>solid oak</strong> desk with a <em>cable tray</em>.</p><ul><li>160 × 80 cm</li><li>Oiled finish</li></ul>";
        orders[^2].Notes = "Invoice to the *head office*, not the store.";
        return orders;
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
                if (order.Cost == 0)
                {
                    order.Cost = Math.Round(order.Amount * 0.7m, 2);
                }

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
