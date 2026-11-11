using System.ComponentModel.DataAnnotations;

namespace StilApp.Data;

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

    /// <summary>A copy, so a form can change it without changing the stored order.</summary>
    public Order Copy() => (Order)MemberwiseClone();
}
