namespace Stil.Charts.Internal;

/// <summary>
/// The geometry of a chart with a category axis and a value axis.
/// </summary>
/// <remarks>
/// The drawing is an SVG that is 100% wide and a fixed number of pixels high. Vertical positions
/// are pixels; horizontal positions are percentages of the plot width, so the chart stretches to
/// any width without distorting text, markers or rounded corners. The plot is the SVG's content
/// box: the axis labels on the left are drawn in the wrapper's padding (overflow: visible), whose
/// width (<see cref="Start"/>) is estimated from the longest label.
/// </remarks>
internal sealed class ChartLayout
{
    public const double FontSize = 12;
    public const double TopPad = 10;
    public const double AxisBand = 26;
    public const double AxisTitleBand = 22;
    public const double LabelGap = 8;
    public const double EndPad = 12;

    /// <summary>Container widths for which category labels are thinned (see stil-charts.css).</summary>
    public static readonly (string Class, double Width)[] Tiers = [("stil-chart-hide-s", 320), ("stil-chart-hide-m", 480), ("stil-chart-hide-l", 800)];

    /// <summary>The SVG height in pixels.</summary>
    public double Height { get; init; }

    /// <summary>The top of the plot area in pixels.</summary>
    public double PlotTop { get; init; }

    /// <summary>The bottom of the plot area in pixels.</summary>
    public double PlotBottom { get; init; }

    /// <summary>Padding before the plot in pixels, for the axis labels and title.</summary>
    public double Start { get; init; }

    /// <summary>Padding after the plot in pixels.</summary>
    public double End { get; init; }

    /// <summary>Extra padding after the plot for direct labels, dropped on narrow charts.</summary>
    public double EndLabels { get; set; }

    public double PlotHeight => PlotBottom - PlotTop;

    /// <summary>The vertical pixel position of a value on a vertical value axis.</summary>
    public double Y(ChartScale scale, double value) => PlotBottom - scale.Fraction(value) * PlotHeight;

    /// <summary>The center of category <paramref name="index"/> of <paramref name="count"/>, as a fraction of the width.</summary>
    public static double BandCenter(int index, int count) => (index + 0.5) / Math.Max(1, count);

    /// <summary>
    /// Classes that hide every k-th label when labels of <paramref name="labelWidth"/> pixels do
    /// not all fit, per container width tier. For example 12 month names on a phone show every
    /// second month.
    /// </summary>
    public string? ThinningClass(int index, int count, double labelWidth, bool includesEndLabels = true)
    {
        List<string>? classes = null;
        foreach (var (css, width) in Tiers)
        {
            var plotWidth = width - Start - End - (includesEndLabels && css != "stil-chart-hide-s" ? EndLabels : 0);
            var step = ThinningStep(count, labelWidth, plotWidth);
            if (index % step != 0)
            {
                (classes ??= []).Add(css);
            }
        }

        return classes is null ? null : string.Join(' ', classes);
    }

    /// <summary>Show every n-th of <paramref name="count"/> labels so they fit in <paramref name="plotWidth"/>.</summary>
    public static int ThinningStep(int count, double labelWidth, double plotWidth)
    {
        if (count <= 1 || labelWidth <= 0)
        {
            return 1;
        }

        var fits = Math.Max(1, (int)Math.Floor(Math.Max(1, plotWidth) / labelWidth));
        return Math.Max(1, (int)Math.Ceiling((double)count / fits));
    }
}
