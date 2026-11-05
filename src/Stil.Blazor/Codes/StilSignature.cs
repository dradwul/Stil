using System.Globalization;
using System.Net;
using System.Text;

namespace Stil.Blazor;

/// <summary>A point of a drawn stroke, in pixels of the signature area, with the pen pressure (0–1; a mouse gives 0.5).</summary>
public readonly record struct StilSignaturePoint(double X, double Y, double Pressure);

/// <summary>
/// A signature from <c>StilSignaturePad</c>: the drawn strokes, or a typed name. It is a picture
/// of a signature to keep with a record, not a qualified electronic signature.
/// </summary>
/// <param name="Strokes">The strokes, each a list of points, in the order they were drawn.</param>
/// <param name="TypedName">The typed name, when the signer typed instead of drawing.</param>
/// <param name="Width">The width of the area the strokes were drawn in, in pixels.</param>
/// <param name="Height">The height of that area, in pixels.</param>
public sealed record StilSignature(IReadOnlyList<IReadOnlyList<StilSignaturePoint>> Strokes, string? TypedName, double Width, double Height)
{
    /// <summary>The font the typed name is shown and saved in.</summary>
    public const string TypedFont = "'Segoe Script', 'Brush Script MT', 'Apple Chancery', cursive";

    /// <summary>A typed signature.</summary>
    public static StilSignature Typed(string name) => new([], name.Trim(), 600, 150);

    /// <summary>True when the signature has a drawn stroke or a typed name.</summary>
    public bool HasContent => Strokes.Any(s => s.Count > 0) || !string.IsNullOrWhiteSpace(TypedName);

    /// <summary>
    /// The signature as an SVG document, in the given ink color on a transparent background. A
    /// stroke is as wide as its average pressure; a typed name uses <see cref="TypedFont"/>.
    /// </summary>
    public string ToSvg(string color = "#000")
    {
        if (color.IndexOfAny(['"', '<', '>', '&']) >= 0)
        {
            throw new ArgumentException($"'{color}' is not a color.", nameof(color));
        }

        var content = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(TypedName))
        {
            content.Append(CultureInfo.InvariantCulture,
                $"<text x=\"{Width / 2:0.#}\" y=\"{Height * 0.62:0.#}\" text-anchor=\"middle\" font-size=\"{Height * 0.36:0.#}\" font-family=\"{TypedFont}\" fill=\"{color}\">{WebUtility.HtmlEncode(TypedName)}</text>");
        }

        foreach (var stroke in Strokes.Where(s => s.Count > 0))
        {
            var path = new StringBuilder();
            path.Append(CultureInfo.InvariantCulture, $"M{stroke[0].X:0.#} {stroke[0].Y:0.#}");
            if (stroke.Count == 1)
            {
                // A dot: a tiny line with round caps.
                path.Append("h0.1");
            }

            foreach (var point in stroke.Skip(1))
            {
                path.Append(CultureInfo.InvariantCulture, $"L{point.X:0.#} {point.Y:0.#}");
            }

            content.Append(CultureInfo.InvariantCulture,
                $"<path d=\"{path}\" fill=\"none\" stroke=\"{color}\" stroke-width=\"{StrokeWidth(stroke):0.##}\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>");
        }

        return string.Create(CultureInfo.InvariantCulture,
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {Width:0.#} {Height:0.#}\">{content}</svg>");
    }

    /// <summary>The line width for a pressure, as stil.js draws it: 1 to 4 pixels.</summary>
    internal static double StrokeWidth(IEnumerable<StilSignaturePoint> stroke) => 1 + 3 * stroke.Average(p => p.Pressure);
}
