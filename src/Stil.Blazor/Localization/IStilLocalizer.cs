using System.Globalization;

namespace Stil.Blazor;

/// <summary>
/// Supplies the built-in texts of Stil components: button labels, empty states and screen reader
/// texts such as "Next page". The default implementation has English, Swedish, Norwegian
/// (Bokmål), Danish and Finnish, follows the UI culture, and reads overrides from
/// <see cref="StilOptions.Texts"/> and from the app's JSON files (<c>AddStilLocalization()</c>).
/// Register your own implementation to read the texts from somewhere else, for example resx files.
/// </summary>
/// <remarks>
/// A text set on a component parameter always wins. When this localizer returns <c>null</c>,
/// the component falls back to the built-in text, and then to English.
/// </remarks>
public interface IStilLocalizer
{
    /// <summary>The text for <paramref name="key"/> (such as <c>Pagination.Next</c>) in <paramref name="culture"/>, or <c>null</c>.</summary>
    string? GetText(string key, CultureInfo culture);
}
