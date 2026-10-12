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
    private readonly List<Order> _orders =
    [
        new() { Id = 1042, Customer = "Fjällbacka Café", Email = "hej@fjallbacka.example", Amount = 1240m, Status = OrderStatus.Delivered, OrderDate = new(2026, 10, 2) },
        new() { Id = 1043, Customer = "Berglund Outdoor", Email = "order@berglund.example", Amount = 2078.90m, Status = OrderStatus.Shipped, OrderDate = new(2026, 10, 7), Express = true },
        new() { Id = 1044, Customer = "Kallio Design Oy", Email = "studio@kallio.example", Amount = 455.20m, Status = OrderStatus.Pending, OrderDate = new(2026, 10, 8) },
        new() { Id = 1045, Customer = "Østerby Marine", Email = "info@osterby.example", Amount = 9120.75m, Status = OrderStatus.Cancelled, OrderDate = new(2026, 10, 9) },
    ];

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

    public void Delete(int id)
    {
        lock (_lock)
        {
            _orders.RemoveAll(o => o.Id == id);
        }
    }
}
