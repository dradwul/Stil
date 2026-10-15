namespace Stil.Blazor.Components;

/// <summary>The direction of a trend in a <see cref="StilStatCard"/>.</summary>
public enum StilTrendDirection
{
    /// <summary>No change, or not a direction worth showing.</summary>
    Neutral,

    /// <summary>The value went up. The default trend color is Success.</summary>
    Up,

    /// <summary>The value went down. The default trend color is Danger.</summary>
    Down
}
