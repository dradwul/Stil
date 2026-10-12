using System.ComponentModel.DataAnnotations;

namespace Stil.Blazor.Tests.TestComponents;

public enum Plan
{
    Free,
    Pro,
    Enterprise
}

public enum Color
{
    Red,
    Green,
    Blue
}

/// <summary>A model with one validated property per Stil form field.</summary>
public sealed class SignUpModel
{
    [Required(ErrorMessage = "Name is required.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Bio is required.")]
    public string? Bio { get; set; }

    [Required(ErrorMessage = "Age is required.")]
    public int? Age { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateOnly? StartDate { get; set; }

    [Required(ErrorMessage = "Color is required.")]
    public Color? Color { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms.")]
    public bool AcceptTerms { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Notifications must be on.")]
    public bool Notifications { get; set; }

    [Required(ErrorMessage = "Plan is required.")]
    public Plan? Plan { get; set; }
}
