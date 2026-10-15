using Microsoft.AspNetCore.Components;
using Stil.Charts.Internal;

namespace Stil.Charts;

/// <summary>Shared parameters of charts with a category axis and a value axis (line and bar charts).</summary>
public abstract partial class StilCartesianChartBase<TItem> : StilChartBase<TItem>
{
    /// <summary>A title for the category axis, for example "Month".</summary>
    [Parameter]
    public string? CategoryAxisTitle { get; set; }

    /// <summary>A title for the value axis, for example "Revenue (kr)".</summary>
    [Parameter]
    public string? ValueAxisTitle { get; set; }

    /// <summary>The lower end of the value axis. The default is a round number below the data (and zero for bars).</summary>
    [Parameter]
    public double? Min { get; set; }

    /// <summary>The upper end of the value axis. The default is a round number above the data.</summary>
    [Parameter]
    public double? Max { get; set; }

    /// <summary>Draws a gridline at every value tick. The default is true.</summary>
    [Parameter]
    public bool Gridlines { get; set; } = true;

    /// <summary>A format string for the value axis ticks. The default is Format, or a number with as many decimals as the ticks need.</summary>
    [Parameter]
    public string? TickFormat { get; set; }

    internal ChartScale Scale { get; private set; } = ChartScale.Create(0, 1);

    internal ChartLayout Layout { get; private set; } = new();

    /// <summary>Categories run down the side and values along the bottom.</summary>
    private protected virtual bool Horizontal => false;

    /// <summary>The value axis starts at zero.</summary>
    private protected virtual bool IncludeZero => true;

    /// <summary>The smallest and largest value the axis must show.</summary>
    private protected virtual (double Min, double Max) ValueExtent()
    {
        var values = Data.Series.SelectMany(s => s.Present).ToList();
        return values.Count == 0 ? (0, 0) : (values.Min(), values.Max());
    }

    /// <summary>Extra room after the plot for direct labels, in pixels.</summary>
    private protected virtual double EndLabelWidth => 0;

    /// <summary>Room after the plot for value labels at the bar ends (horizontal bars), in pixels.</summary>
    private protected virtual double EndValueWidth => 0;

    /// <summary>The pixel height of one category band in a horizontal chart without a set Height.</summary>
    private protected virtual double HorizontalBand => 36;

    /// <summary>Extra room above a vertical plot, for value labels on the tallest column, in pixels.</summary>
    private protected virtual double ExtraTop => 0;

    /// <summary>Adds a Total column to the data table.</summary>
    private protected virtual bool TableTotals => false;

    private protected virtual string TotalHeader => "Total";

    private protected string CategoryHeader => string.IsNullOrWhiteSpace(CategoryAxisTitle) ? CategoryText : CategoryAxisTitle;

    private protected string TickText(double value) =>
        ChartText.Value(value, TickFormat ?? Format ?? ChartText.TickFormat(Scale.Decimals));

    /// <summary>The longest category label shown on a horizontal chart's axis, in characters.</summary>
    private protected const int HorizontalLabelChars = 20;

    /// <summary>The longest category label shown on a vertical chart's axis, in characters.</summary>
    private protected const int VerticalLabelChars = 14;

    private protected override void Prepare()
    {
        base.Prepare();
        var (low, high) = ValueExtent();
        var categoryTitle = string.IsNullOrWhiteSpace(CategoryAxisTitle) ? 0 : ChartLayout.AxisTitleBand;
        var valueTitle = string.IsNullOrWhiteSpace(ValueAxisTitle) ? 0 : ChartLayout.AxisTitleBand;

        if (!Horizontal)
        {
            double height = EffectiveHeight;
            var plotBottom = height - ChartLayout.AxisBand - categoryTitle;
            var maxTicks = Math.Clamp((int)((plotBottom - ChartLayout.TopPad - ExtraTop) / 48), 2, 8);
            Scale = ChartScale.Create(low, high, maxTicks, IncludeZero, Min, Max);
            var tickWidth = Scale.Ticks.Select(t => ChartText.Width(TickText(t))).DefaultIfEmpty(0).Max();
            Layout = new ChartLayout
            {
                Height = height,
                PlotTop = ChartLayout.TopPad + ExtraTop,
                PlotBottom = plotBottom,
                Start = Math.Ceiling(Math.Max(16, tickWidth + ChartLayout.LabelGap + valueTitle)),
                End = ChartLayout.EndPad,
                EndLabels = Math.Ceiling(EndLabelWidth)
            };
        }
        else
        {
            var count = Math.Max(1, Data.Categories.Count);
            var band = Height is { } fixedHeight
                ? Math.Max(8, (fixedHeight - ChartLayout.TopPad - ChartLayout.AxisBand - valueTitle) / count)
                : HorizontalBand;
            var plotBottom = ChartLayout.TopPad + band * count;
            Scale = ChartScale.Create(low, high, 5, IncludeZero, Min, Max);
            var labelWidth = Data.Categories.Select(c => ChartText.Width(ChartText.Truncate(c, HorizontalLabelChars))).DefaultIfEmpty(0).Max();
            var lastTick = Scale.Ticks.Count == 0 ? 0 : ChartText.Width(TickText(Scale.Ticks[^1])) / 2;
            Layout = new ChartLayout
            {
                Height = plotBottom + ChartLayout.AxisBand + valueTitle,
                PlotTop = ChartLayout.TopPad,
                PlotBottom = plotBottom,
                Start = Math.Ceiling(Math.Max(16, labelWidth + ChartLayout.LabelGap + categoryTitle)),
                End = Math.Ceiling(Math.Max(ChartLayout.EndPad, Math.Max(lastTick + 4, EndValueWidth))),
                EndLabels = 0
            };
        }
    }

    /// <summary>The pixel height of one category band in a horizontal chart.</summary>
    private protected double BandHeight => Layout.PlotHeight / Math.Max(1, Data.Categories.Count);

    /// <summary>Snaps a horizontal hairline to the pixel grid so it stays crisp.</summary>
    private protected static string Crisp(double y) => ChartText.N(Math.Round(y) + 0.5);

    private protected override string BuildSummary()
    {
        var categories = Data.Categories;
        var series = Data.Series;
        var span = categories.Count == 1
            ? $"1 category, {categories[0]}"
            : $"{categories.Count} categories, from {categories[0]} to {categories[^1]}";
        var parts = new List<string>
        {
            series.Count == 1
                ? $"{KindName} of {series[0].Name}, {span}."
                : $"{KindName} with {series.Count} series ({ChartText.List(series.Select(s => s.Name).ToList())}), {span}."
        };

        if (series.Count <= 4)
        {
            foreach (var s in series)
            {
                var present = s.Values.Select((v, i) => (Value: v, Index: i)).Where(p => p.Value.HasValue).ToList();
                if (present.Count == 0)
                {
                    parts.Add($"{s.Name}: no values.");
                    continue;
                }

                var low = present.MinBy(p => p.Value!.Value);
                var high = present.MaxBy(p => p.Value!.Value);
                var text = $"lowest {FormatValue(low.Value, s, MissingText)} ({categories[low.Index]}), highest {FormatValue(high.Value, s, MissingText)} ({categories[high.Index]}).";
                parts.Add(series.Count == 1 ? char.ToUpperInvariant(text[0]) + text[1..] : $"{s.Name}: {text}");
            }
        }

        parts.AddRange(ExtraSummary());
        return string.Join(" ", parts);
    }

    private protected virtual IEnumerable<string> ExtraSummary() => [];

    private protected override (IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows) TableData()
    {
        var headers = new List<string> { CategoryHeader };
        headers.AddRange(Data.Series.Select(s => s.Name));
        if (TableTotals)
        {
            headers.Add(TotalHeader);
        }

        var rows = new List<IReadOnlyList<string>>();
        for (var i = 0; i < Data.Categories.Count; i++)
        {
            var row = new List<string> { Data.Categories[i] };
            row.AddRange(Data.Series.Select(s => FormatValue(s.Values[i], s, MissingText)));
            if (TableTotals)
            {
                row.Add(ChartText.Value(CategoryTotal(i), Format));
            }

            rows.Add(row);
        }

        return (headers, rows);
    }

    /// <summary>The sum of the values in a category.</summary>
    private protected double CategoryTotal(int index) => Data.Series.Sum(s => s.Values[index] ?? 0);

    /// <summary>A tooltip listing every series' value in a category.</summary>
    private protected string CategoryTooltip(int index, bool withTotal = false)
    {
        var lines = new List<string> { Data.Categories[index] };
        lines.AddRange(Data.Series.Select(s => $"{s.Name}: {FormatValue(s.Values[index], s, MissingText)}"));
        if (withTotal)
        {
            lines.Add($"{TotalHeader}: {ChartText.Value(CategoryTotal(index), Format)}");
        }

        return string.Join("\n", lines);
    }

    /// <summary>A category label shortened for an axis.</summary>
    private protected static string AxisLabel(string category, int max) => ChartText.Truncate(category, max);
}
