using System.Globalization;
using System.Net;
using System.Text;

namespace Stil.Blazor;

/// <summary>How much of a QR code can be damaged and still read. More correction makes a denser code.</summary>
public enum StilQrErrorCorrection
{
    /// <summary>About 7% of the code can be restored.</summary>
    Low,

    /// <summary>About 15%; the default.</summary>
    Medium,

    /// <summary>About 25%.</summary>
    Quartile,

    /// <summary>About 30%, for codes that get dirty or carry a logo.</summary>
    High,
}

/// <summary>A one-dimensional barcode format.</summary>
public enum StilBarcodeFormat
{
    /// <summary>Code 128: any printable ASCII text, compact for digits. Common on shipping labels and badges.</summary>
    Code128,

    /// <summary>EAN-13: 12 digits and a check digit, on retail products.</summary>
    Ean13,

    /// <summary>EAN-8: 7 digits and a check digit, on small packages.</summary>
    Ean8,
}

/// <summary>
/// Generates QR codes and barcodes in C#, with no dependency. <c>StilQrCode</c> and
/// <c>StilBarcode</c> use it; call it yourself for codes on a PDF, an e-mail or a label printer.
/// The methods throw <see cref="ArgumentException"/> for a value the format cannot hold.
/// </summary>
public static class StilCodes
{
    /// <summary>The modules of a QR code as [row, column], true for dark, without the quiet zone.</summary>
    public static bool[,] QrModules(string value, StilQrErrorCorrection errorCorrection = StilQrErrorCorrection.Medium) =>
        QrEncoder.Encode(value, errorCorrection);

    /// <summary>
    /// A QR code as an SVG document, one unit per module, with a quiet zone of four modules (the
    /// standard's minimum). Pass <c>currentColor</c> and no background to follow the page's colors.
    /// </summary>
    public static string QrSvg(string value, StilQrErrorCorrection errorCorrection = StilQrErrorCorrection.Medium,
        string color = "#000", string? background = "#fff")
    {
        var modules = QrModules(value, errorCorrection);
        var size = modules.GetLength(0);
        const int quiet = 4;
        var path = new StringBuilder();
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                if (!modules[y, x])
                {
                    continue;
                }

                // One rectangle per run of dark modules in a row.
                var run = 1;
                while (x + run < size && modules[y, x + run])
                {
                    run++;
                }

                path.Append(CultureInfo.InvariantCulture, $"M{x + quiet} {y + quiet}h{run}v1h-{run}z");
                x += run - 1;
            }
        }

        var total = size + 2 * quiet;
        return Svg(total, total, background, $"<path fill=\"{Color(color)}\" d=\"{path}\"/>");
    }

    /// <summary>The modules of a barcode, true for a bar, without the quiet zone.</summary>
    public static bool[] BarcodeModules(string value, StilBarcodeFormat format = StilBarcodeFormat.Code128) =>
        BarcodeEncoder.Encode(value, format).Modules;

    /// <summary>The EAN check digit for 12 (EAN-13) or 7 (EAN-8) digits.</summary>
    public static int EanCheckDigit(string digits)
    {
        ArgumentNullException.ThrowIfNull(digits);
        if (digits.Length is not (7 or 12) || !digits.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("Pass the 12 digits of an EAN-13 or the 7 digits of an EAN-8.", nameof(digits));
        }

        return BarcodeEncoder.EanCheckDigit(digits);
    }

    /// <summary>
    /// A barcode as an SVG document, one unit per module, 50 units high, with a quiet zone of ten
    /// modules and, when <paramref name="showText"/> is true, the value under the bars (for EAN with
    /// the check digit).
    /// </summary>
    public static string BarcodeSvg(string value, StilBarcodeFormat format = StilBarcodeFormat.Code128, bool showText = true,
        string color = "#000", string? background = "#fff")
    {
        var (modules, text) = BarcodeEncoder.Encode(value, format);
        const int quiet = 10;
        const int barHeight = 50;
        var path = new StringBuilder();
        for (var x = 0; x < modules.Length; x++)
        {
            if (!modules[x])
            {
                continue;
            }

            var run = 1;
            while (x + run < modules.Length && modules[x + run])
            {
                run++;
            }

            path.Append(CultureInfo.InvariantCulture, $"M{x + quiet} 0h{run}v{barHeight}h-{run}z");
            x += run - 1;
        }

        var width = modules.Length + 2 * quiet;
        var content = $"<path fill=\"{Color(color)}\" d=\"{path}\"/>";
        if (!showText)
        {
            return Svg(width, barHeight, background, content);
        }

        content += string.Create(CultureInfo.InvariantCulture, $"<text x=\"{width / 2.0:0.#}\" y=\"{barHeight + 10}\" text-anchor=\"middle\" font-size=\"9\" font-family=\"inherit\" fill=\"{Color(color)}\">{WebUtility.HtmlEncode(text)}</text>");
        return Svg(width, barHeight + 13, background, content);
    }

    private static string Svg(int width, int height, string? background, string content)
    {
        var rect = background is null ? "" : $"<rect width=\"{width}\" height=\"{height}\" fill=\"{Color(background)}\"/>";
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {width} {height}\" shape-rendering=\"crispEdges\">{rect}{content}</svg>";
    }

    private static string Color(string color) =>
        color.IndexOfAny(['"', '<', '>', '&']) >= 0
            ? throw new ArgumentException($"'{color}' is not a color.", nameof(color))
            : color;
}
