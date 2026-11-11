namespace Stil.Charts;

/// <summary>How a chart offers its data as a table, the accessible alternative to the drawing.</summary>
public enum StilChartDataTable
{
    /// <summary>A "Show data" toggle under the chart (a native details element, no JavaScript).</summary>
    Toggle,

    /// <summary>A table only screen readers find (visually hidden).</summary>
    Hidden,

    /// <summary>No table. Only use this when the same data is on the page in another form.</summary>
    None
}

/// <summary>The direction bars grow in.</summary>
public enum StilChartOrientation
{
    /// <summary>Columns: categories along the bottom, values up the side.</summary>
    Vertical,

    /// <summary>Bars: categories down the side, values along the bottom. Suits long category names.</summary>
    Horizontal
}

/// <summary>How a gauge is drawn.</summary>
public enum StilGaugeShape
{
    /// <summary>A half circle with the value in the middle.</summary>
    Arc,

    /// <summary>A horizontal bar, for a row of gauges or a narrow space.</summary>
    Bar
}

/// <summary>
/// A stretch of a gauge's scale, for example 0–60 as poor, drawn as a tint behind the value.
/// </summary>
/// <param name="From">Where the stretch starts.</param>
/// <param name="To">Where the stretch ends.</param>
/// <param name="Color">A CSS color, for example "var(--stil-color-success)".</param>
/// <param name="Name">What the stretch means, for example "Good". Screen readers hear it with the value.</param>
public sealed record StilGaugeRange(double From, double To, string Color, string? Name = null);
