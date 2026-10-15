using System.Text;

namespace Stil.Charts.Internal;

/// <summary>SVG path data for lines, areas and donut slices.</summary>
internal static class ChartPaths
{
    /// <summary>Splits values into runs of consecutive present values; a missing value ends a run.</summary>
    public static List<List<(int Index, double Value)>> Runs(IReadOnlyList<double?> values)
    {
        var runs = new List<List<(int, double)>>();
        List<(int, double)>? current = null;
        for (var i = 0; i < values.Count; i++)
        {
            if (values[i] is { } value)
            {
                current ??= [];
                current.Add((i, value));
            }
            else if (current is not null)
            {
                runs.Add(current);
                current = null;
            }
        }

        if (current is not null)
        {
            runs.Add(current);
        }

        return runs;
    }

    /// <summary>A polyline through the points: "M x y L x y …".</summary>
    public static string Line(IReadOnlyList<(double X, double Y)> points)
    {
        var path = new StringBuilder();
        for (var i = 0; i < points.Count; i++)
        {
            path.Append(i == 0 ? "M" : " L").Append(ChartText.N(points[i].X)).Append(' ').Append(ChartText.N(points[i].Y));
        }

        return path.ToString();
    }

    /// <summary>The area between a polyline and a horizontal baseline.</summary>
    public static string Area(IReadOnlyList<(double X, double Y)> points, double baseline)
    {
        if (points.Count == 0)
        {
            return "";
        }

        return $"M{ChartText.N(points[0].X)} {ChartText.N(baseline)} L" +
               Line(points)[1..] +
               $" L{ChartText.N(points[^1].X)} {ChartText.N(baseline)} Z";
    }

    /// <summary>
    /// A donut slice from angle <paramref name="start"/> to <paramref name="end"/> (radians,
    /// clockwise from twelve o'clock) between radius <paramref name="inner"/> and <paramref name="outer"/>.
    /// A full turn is drawn as two halves, since an SVG arc cannot start and end at the same point.
    /// </summary>
    public static string DonutSlice(double cx, double cy, double outer, double inner, double start, double end)
    {
        if (end - start >= Math.PI * 2 - 1e-9)
        {
            return DonutSlice(cx, cy, outer, inner, start, start + Math.PI) + " " +
                   DonutSlice(cx, cy, outer, inner, start + Math.PI, start + Math.PI * 2);
        }

        var large = end - start > Math.PI ? 1 : 0;
        var (ox0, oy0) = Point(cx, cy, outer, start);
        var (ox1, oy1) = Point(cx, cy, outer, end);
        var (ix1, iy1) = Point(cx, cy, inner, end);
        var (ix0, iy0) = Point(cx, cy, inner, start);
        return $"M{ChartText.N(ox0)} {ChartText.N(oy0)} " +
               $"A{ChartText.N(outer)} {ChartText.N(outer)} 0 {large} 1 {ChartText.N(ox1)} {ChartText.N(oy1)} " +
               $"L{ChartText.N(ix1)} {ChartText.N(iy1)} " +
               $"A{ChartText.N(inner)} {ChartText.N(inner)} 0 {large} 0 {ChartText.N(ix0)} {ChartText.N(iy0)} Z";
    }

    private static (double X, double Y) Point(double cx, double cy, double radius, double angle) =>
        (cx + radius * Math.Sin(angle), cy - radius * Math.Cos(angle));
}
