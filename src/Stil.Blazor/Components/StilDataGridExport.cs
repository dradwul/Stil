using System.Globalization;
using System.Text;

namespace Stil.Blazor.Components;

/// <summary>Writes data grid rows as CSV that spreadsheets open as they are.</summary>
internal static class StilDataGridExport
{
    /// <summary>
    /// A value as export text: numbers without thousands separators (so a spreadsheet reads them
    /// as numbers) in the current culture, other formattable values with the column's format.
    /// </summary>
    public static string Text(object? value, string? format, Func<bool, string>? boolText) => value switch
    {
        null => "",
        bool flag when boolText is not null => boolText(flag),
        byte or sbyte or short or ushort or int or uint or long or ulong =>
            ((IFormattable)value).ToString(null, CultureInfo.CurrentCulture),
        float or double or decimal => ((IFormattable)value).ToString("0.##########", CultureInfo.CurrentCulture),
        IFormattable formattable when format is not null => formattable.ToString(format, CultureInfo.CurrentCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.CurrentCulture),
        _ => value.ToString() ?? ""
    };

    /// <summary>
    /// The rows as CSV with a header row, separated by the culture's list separator (a semicolon
    /// where the comma is the decimal separator), and CRLF line ends.
    /// </summary>
    public static string Csv<TItem>(IReadOnlyList<StilDataGridColumnBase<TItem>> columns, IEnumerable<TItem> rows, CultureInfo culture)
    {
        var separator = culture.TextInfo.ListSeparator is { Length: > 0 } list ? list : ",";
        var csv = new StringBuilder();
        csv.Append(string.Join(separator, columns.Select(c => Field(c.DisplayTitle, separator)))).Append("\r\n");
        foreach (var row in rows)
        {
            csv.Append(string.Join(separator, columns.Select(c => Field(c.ExportText(row), separator)))).Append("\r\n");
        }

        return csv.ToString();
    }

    // Quotes a field that needs it, and defuses text that a spreadsheet would run as a formula.
    private static string Field(string text, string separator)
    {
        if (text.Length > 0 && "=+@\t\r".Contains(text[0]))
        {
            text = "'" + text;
        }
        else if (text.Length > 1 && text[0] == '-' && !double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out _))
        {
            text = "'" + text;
        }

        var quote = text.Contains(separator, StringComparison.Ordinal) || text.Contains('"') || text.Contains('\n') || text.Contains('\r')
            || text.StartsWith(' ') || text.EndsWith(' ');
        return quote ? $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"" : text;
    }
}
