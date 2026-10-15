namespace Stil.Blazor.Components;

/// <summary>A text with an optional link, used as data for breadcrumbs and navigation.</summary>
/// <param name="Text">The visible text.</param>
/// <param name="Href">The link target. Null renders plain text.</param>
public sealed record StilLinkItem(string Text, string? Href = null);
