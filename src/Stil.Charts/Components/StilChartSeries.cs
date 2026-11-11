using Microsoft.AspNetCore.Components;
using Stil.Charts.Internal;

namespace Stil.Charts;

/// <summary>Shared parameters of chart series. Series render nothing themselves; the chart does.</summary>
public abstract class StilChartSeriesBase<TItem> : ComponentBase, IDisposable
{
    [CascadingParameter]
    internal StilChartBase<TItem>? Chart { get; set; }

    /// <summary>The series name, shown in the legend, tooltips and the data table.</summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// A CSS color for the series, for example "var(--stil-color-primary)" or "#6741d9". The
    /// default is the next chart token (--stil-chart-1 to --stil-chart-6) in the order the series
    /// are declared.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>A format string for this series' values. The default is the chart's Format.</summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Draws the series as a line over the bars of a vertical bar chart, for example a margin over
    /// revenue. Other charts ignore it.
    /// </summary>
    [Parameter]
    public bool Line { get; set; }

    /// <summary>
    /// Measures the series on a second value axis at the right of a vertical line or bar chart, for
    /// values of another unit or size. In a stacked bar chart only line series can use it.
    /// </summary>
    [Parameter]
    public bool SecondaryAxis { get; set; }

    internal abstract double? GetValue(TItem item);

    protected override void OnInitialized()
    {
        if (Chart is null)
        {
            throw new InvalidOperationException("A chart series must be placed inside a Stil chart, for example StilLineChart.");
        }

        Chart.AddSeries(this);
    }

    public void Dispose()
    {
        Chart?.RemoveSeries(this);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A series in a chart: a name and a value for every item, for example
/// <c>&lt;StilChartSeries Name="2026" Value="s =&gt; s.Revenue" /&gt;</c>. The value can be any
/// number type (int, decimal, double, …) and nullable; null leaves a gap.
/// </summary>
public sealed class StilChartSeries<TItem, TValue> : StilChartSeriesBase<TItem>
{
    /// <summary>The value of an item, for example <c>s =&gt; s.Revenue</c>.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, TValue> Value { get; set; } = default!;

    internal override double? GetValue(TItem item) => Value is null ? null : ChartText.ToDouble(Value(item));
}
