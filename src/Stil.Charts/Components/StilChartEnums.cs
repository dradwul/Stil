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
