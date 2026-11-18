namespace Stil.Blazor;

/// <summary>A page in another language: its language (or <c>x-default</c>) and its address, for hreflang.</summary>
/// <param name="Language">The language, such as <c>sv</c> or <c>sv-SE</c>, or <c>x-default</c> for the page shown to everyone else.</param>
/// <param name="Url">The page's address, absolute or relative to the site.</param>
public sealed record StilAlternate(string Language, string Url);
