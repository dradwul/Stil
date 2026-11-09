namespace Stil.Blazor.Components;

/// <summary>
/// A page or an action in a <c>StilCommandPalette</c>. Give it an <see cref="Href"/> to go to a
/// page, or an <see cref="Action"/> to run.
/// </summary>
/// <param name="Title">What the user searches for and sees, for example "Orders" or "New order".</param>
/// <param name="Group">The heading it is listed under, for example "Pages" or "Actions".</param>
public sealed record StilCommand(string Title, string? Group = null)
{
    /// <summary>The page to go to.</summary>
    public string? Href { get; init; }

    /// <summary>The action to run.</summary>
    public Func<Task>? Action { get; init; }

    /// <summary>More words that find it, for example "customers clients" for "People".</summary>
    public string? Keywords { get; init; }

    /// <summary>An icon by name (see StilIcons).</summary>
    public string? Icon { get; init; }

    /// <summary>A keyboard shortcut to show next to it, for example "Ctrl+N". The palette does not bind it.</summary>
    public string? Shortcut { get; init; }

    /// <summary>A key that identifies it among the recent choices. The default is the group and title.</summary>
    public string Id { get; init; } = $"{Group}/{Title}";
}
