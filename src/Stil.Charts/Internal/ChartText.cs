using System.Globalization;

namespace Stil.Charts.Internal;

/// <summary>Number formatting for SVG attributes and for the text people read.</summary>
internal static class ChartText
{
    /// <summary>The value format used when the app sets none: thousands separators, up to two decimals.</summary>
    public const string DefaultFormat = "#,0.##";

    /// <summary>The average width of a character of the 0.75rem axis text, in pixels.</summary>
    public const double CharWidth = 7;

    /// <summary>A number for an SVG attribute: invariant culture, at most three decimals.</summary>
    public static string N(double value) =>
        (Math.Abs(value) < 0.0005 ? 0 : value).ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>A percentage for an SVG attribute, for example "37.5%".</summary>
    public static string Pct(double fraction) => N(fraction * 100) + "%";

    /// <summary>Formats a value for people, in the current culture.</summary>
    public static string Value(double value, string? format) =>
        value.ToString(string.IsNullOrWhiteSpace(format) ? DefaultFormat : format, CultureInfo.CurrentCulture);

    /// <summary>Formats a value or returns <paramref name="missing"/> when there is none.</summary>
    public static string Value(double? value, string? format, string missing) =>
        value is { } v ? Value(v, format) : missing;

    /// <summary>The default tick format: as many decimals as the step needs.</summary>
    public static string TickFormat(int decimals) => "N" + Math.Clamp(decimals, 0, 10).ToString(CultureInfo.InvariantCulture);

    /// <summary>Formats a category for display.</summary>
    public static string Category(object? category, string? format) => category switch
    {
        null => "",
        IFormattable formattable when !string.IsNullOrWhiteSpace(format) => formattable.ToString(format, CultureInfo.CurrentCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
        _ => category.ToString() ?? ""
    };

    /// <summary>A rough width estimate for axis text, in pixels.</summary>
    public static double Width(string text, double charWidth = CharWidth) => (text?.Length ?? 0) * charWidth;

    /// <summary>Shortens text to <paramref name="max"/> characters with an ellipsis.</summary>
    public static string Truncate(string text, int max) =>
        text.Length <= max ? text : string.Concat(text.AsSpan(0, Math.Max(1, max - 1)).TrimEnd(), "…");

    /// <summary>A share of a whole, for example "42%" (one decimal below 10%).</summary>
    public static string Percent(double fraction) =>
        (fraction * 100).ToString(Math.Abs(fraction) < 0.1 && fraction != 0 ? "0.#" : "0", CultureInfo.CurrentCulture) + "%";

    /// <summary>Joins names as "A, B and C".</summary>
    public static string List(IReadOnlyList<string> items, string and = "and") => items.Count switch
    {
        0 => "",
        1 => items[0],
        _ => string.Join(", ", items.Take(items.Count - 1)) + $" {and} " + items[^1]
    };

    /// <summary>Converts a series value to a double, or null when it is missing or not finite.</summary>
    public static double? ToDouble(object? value)
    {
        double? result = value switch
        {
            null => null,
            double d => d,
            float f => f,
            decimal m => (double)m,
            int i => i,
            long l => l,
            short s => s,
            byte b => b,
            uint ui => ui,
            ulong ul => ul,
            IConvertible convertible => convertible.ToDouble(CultureInfo.InvariantCulture),
            _ => throw new InvalidCastException($"A chart value must be a number, not {value.GetType().Name}.")
        };

        return result is { } r && double.IsFinite(r) ? r : null;
    }
}
