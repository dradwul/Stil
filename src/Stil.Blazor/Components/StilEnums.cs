namespace Stil.Blazor.Components;

/// <summary>Component size. <see cref="Medium"/> is the default and adds no class.</summary>
public enum StilSize
{
    Medium,
    Small,
    Large
}

/// <summary>How a button uses its variant color.</summary>
public enum StilButtonAppearance
{
    /// <summary>Filled with the variant color (stil-btn-{variant}).</summary>
    Solid,

    /// <summary>A border and text in the variant color (stil-btn-outline).</summary>
    Outline,

    /// <summary>Text in the variant color only (stil-btn-ghost).</summary>
    Ghost
}

/// <summary>Color mode for <c>data-stil-theme</c>.</summary>
public enum StilThemeMode
{
    Light,
    Dark,

    /// <summary>Follows the operating system setting.</summary>
    Auto
}

/// <summary>Where a tooltip is shown relative to its element.</summary>
public enum StilTooltipPosition
{
    Top,
    Bottom
}
