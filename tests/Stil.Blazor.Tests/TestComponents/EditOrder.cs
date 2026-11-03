using System.ComponentModel.DataAnnotations;

namespace Stil.Blazor.Tests.TestComponents;

/// <summary>An order that a data grid edits in place.</summary>
public sealed class EditOrder
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter a customer.")]
    public string? Customer { get; set; }

    public GridOrderStatus Status { get; set; }

    public bool Paid { get; set; }

    [Range(1, 10_000, ErrorMessage = "Enter an amount from 1 to 10,000.")]
    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    // Not shown in the grid, so not validated there.
    [Required]
    public string? Email { get; set; }
}
