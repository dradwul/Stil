using Microsoft.AspNetCore.Components;
using Stil.Blazor.Components;
using Stil.Charts.Internal;

namespace Stil.Charts;

/// <summary>
/// Shared parameters and behaviour of the Stil charts: items, a category per item, series as
/// child components, a title, a legend and a data table.
/// </summary>
public abstract partial class StilChartBase<TItem> : StilComponentBase
{
    /// <summary>The number of chart color tokens (--stil-chart-1 to --stil-chart-6).</summary>
    public const int PaletteSize = 6;

    private readonly List<StilChartSeriesBase<TItem>> _series = [];
    private readonly string _id = ChartIds.Next();
    private bool _rendered;

    /// <summary>The data, one item per category (a month, a product, …).</summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// The category of an item, for example <c>s =&gt; s.Month</c>. Shown on the category axis,
    /// in tooltips and in the data table. The default is the item's position (1, 2, 3, …).
    /// </summary>
    [Parameter]
    public Func<TItem, object?>? Category { get; set; }

    /// <summary>A format string for categories that are dates or numbers, for example "MMM".</summary>
    [Parameter]
    public string? CategoryFormat { get; set; }

    /// <summary>The chart title. It is also the chart's accessible name, so set it.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>A short line under the title, for example the period or the unit.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// What screen readers hear about the data. The default is generated from the data, for
    /// example the range of each series.
    /// </summary>
    [Parameter]
    public string? Summary { get; set; }

    /// <summary>The height of the drawing in pixels, including the axis labels.</summary>
    [Parameter]
    public int? Height { get; set; }

    /// <summary>
    /// A format string for values, for example "N0", "C0" or "0.0 'kg'". Values are formatted
    /// with the current culture. The default shows thousands separators and up to two decimals.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>Shows the legend. The default shows it when there are two or more series.</summary>
    [Parameter]
    public bool? ShowLegend { get; set; }

    /// <summary>How the data table is offered. The default is a "Show data" toggle.</summary>
    [Parameter]
    public StilChartDataTable DataTable { get; set; }

    /// <summary>The series: <c>StilChartSeries</c> components.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>The text of the data table toggle.</summary>
    [Parameter]
    public string? ShowDataText { get; set; }

    /// <summary>Shown instead of the chart when there is nothing to draw.</summary>
    [Parameter]
    public string? EmptyText { get; set; }

    /// <summary>The header of the category column in the data table.</summary>
    [Parameter]
    public string? CategoryText { get; set; }

    /// <summary>Shown in tooltips and the data table for a missing value.</summary>
    [Parameter]
    public string MissingText { get; set; } = "–";

    internal IReadOnlyList<StilChartSeriesBase<TItem>> RegisteredSeries => _series;

    internal ChartData Data { get; private set; } = ChartData.Empty;

    /// <summary>A class for the chart type, for example stil-chart-line.</summary>
    private protected abstract string KindClass { get; }

    /// <summary>The chart type in words, used in the generated summary, for example "Line chart".</summary>
    private protected abstract string KindName { get; }

    /// <summary>The drawing.</summary>
    private protected abstract RenderFragment RenderPlot { get; }

    /// <summary>Legend keys are short lines (line charts) instead of squares.</summary>
    private protected virtual bool LegendUsesLines => false;

    private protected virtual int DefaultHeight => 300;

    private protected int EffectiveHeight => Math.Max(80, Height ?? DefaultHeight);

    private protected string TitleId => $"{_id}-title";

    private protected string DescriptionId => $"{_id}-desc";

    private protected bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    /// <summary>aria-labelledby for the drawing: the visible title.</summary>
    private protected string? SvgLabelledBy => HasTitle ? TitleId : null;

    /// <summary>aria-label for the drawing when there is no title.</summary>
    private protected string? SvgLabel => HasTitle ? null : KindName;

    private protected bool LegendVisible => ShowLegend ?? Data.Series.Count >= 2;

    private protected string SummaryText => string.IsNullOrWhiteSpace(Summary) ? BuildSummary() : Summary;

    /// <summary>A localized chart text (<c>Chart.{key}</c>), formatted with the current culture.</summary>
    private protected string Phrase(string key, params object[] arguments) =>
        string.Format(System.Globalization.CultureInfo.CurrentCulture, Localize(null, "Chart." + key), arguments);

    /// <summary>
    /// The whole chart. The series register through the cascading value, and the figure renders
    /// after them (in ChartDefer), so the first render, also in static SSR, has every series.
    /// </summary>
    private protected RenderFragment RenderChart => builder =>
    {
        builder.OpenComponent<CascadingValue<StilChartBase<TItem>>>(0);
        builder.AddComponentParameter(1, nameof(CascadingValue<StilChartBase<TItem>>.Value), this);
        builder.AddComponentParameter(2, nameof(CascadingValue<StilChartBase<TItem>>.IsFixed), true);
        builder.AddComponentParameter(3, nameof(CascadingValue<StilChartBase<TItem>>.ChildContent), (RenderFragment)(content =>
        {
            content.AddContent(0, ChildContent);
            content.OpenComponent<ChartDefer>(1);
            content.AddComponentParameter(2, nameof(ChartDefer.ChildContent), RenderFigure);
            content.CloseComponent();
        }));
        builder.CloseComponent();
    };

    internal void AddSeries(StilChartSeriesBase<TItem> series)
    {
        if (_series.Contains(series))
        {
            return;
        }

        _series.Add(series);
        if (_rendered)
        {
            StateHasChanged();
        }
    }

    internal void RemoveSeries(StilChartSeriesBase<TItem> series)
    {
        if (_series.Remove(series))
        {
            StateHasChanged();
        }
    }

    /// <summary>The CSS color of the series at <paramref name="index"/>: its own color or the next chart token.</summary>
    internal static string SeriesColor(int index, string? color) =>
        !string.IsNullOrWhiteSpace(color)
            ? color
            : index < PaletteSize
                ? $"var(--stil-chart-{index + 1})"
                : "var(--stil-chart-other, var(--stil-color-border-strong))";

    /// <summary>Reads the categories and series values. Runs once per render, after the series registered.</summary>
    private protected virtual void Prepare()
    {
        _rendered = true;
        var items = Items?.ToList() ?? [];
        var categories = items
            .Select((item, index) => Category is null
                ? (index + 1).ToString(System.Globalization.CultureInfo.CurrentCulture)
                : ChartText.Category(Category(item), CategoryFormat))
            .ToList();
        var series = _series
            .Select((s, index) => new ChartSeriesData(
                index,
                string.IsNullOrWhiteSpace(s.Name) ? $"Series {index + 1}" : s.Name,
                SeriesColor(index, s.Color),
                s.Format ?? Format,
                items.Select(s.GetValue).ToList()))
            .ToList();
        Data = new ChartData(categories, series);
    }

    /// <summary>The generated summary for screen readers.</summary>
    private protected abstract string BuildSummary();

    /// <summary>The data table: column headers and rows (the first cell of a row is its header).</summary>
    private protected abstract (IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows) TableData();

    private protected static string FormatValue(double? value, ChartSeriesData series, string missing) =>
        ChartText.Value(value, series.Format, missing);

    /// <summary>
    /// An SVG text element. Written in C# because Razor reads a &lt;text&gt; tag in a code block
    /// as its own text transition, not as SVG. <paramref name="fullText"/> becomes a native tooltip
    /// when the label was shortened.
    /// </summary>
    private protected static RenderFragment SvgText(string text, string x, string y, string? cssClass = null,
        string? anchor = null, string? dy = null, string? fullText = null, string? transform = null, string? dx = null) => builder =>
    {
        builder.OpenElement(0, "text");
        builder.AddAttribute(1, "class", cssClass);
        builder.AddAttribute(2, "x", x);
        builder.AddAttribute(3, "y", y);
        builder.AddAttribute(4, "dx", dx);
        builder.AddAttribute(5, "dy", dy);
        builder.AddAttribute(6, "text-anchor", anchor);
        builder.AddAttribute(7, "transform", transform);
        if (!string.IsNullOrEmpty(fullText) && fullText != text)
        {
            builder.OpenElement(8, "title");
            builder.AddContent(9, fullText);
            builder.CloseElement();
        }

        builder.AddContent(10, text);
        builder.CloseElement();
    };
}
