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

/// <summary>The side a drawer slides in from.</summary>
public enum StilDrawerPosition
{
    /// <summary>The start edge: left in left-to-right languages.</summary>
    Start,

    /// <summary>The end edge: right in left-to-right languages.</summary>
    End
}

/// <summary>The responsive breakpoints: sm 576px, md 768px, lg 992px, xl 1200px.</summary>
public enum StilBreakpoint
{
    Small,
    Medium,
    Large,
    ExtraLarge
}

/// <summary>How StilTabs draws the selected tab.</summary>
public enum StilTabsAppearance
{
    /// <summary>A line under the selected tab.</summary>
    Underline,

    /// <summary>The selected tab is filled with the accent color.</summary>
    Pills
}

/// <summary>An item in a data-driven StilDropdown: a link, an action, a header or a divider.</summary>
public sealed record StilMenuItem
{
    public string? Text { get; init; }
    public string? Href { get; init; }
    public Func<Task>? OnClick { get; init; }
    public bool Disabled { get; init; }
    public bool IsHeader { get; init; }
    public bool IsDivider { get; init; }

    /// <summary>An item that runs an action.</summary>
    public static StilMenuItem Action(string text, Func<Task> onClick, bool disabled = false) =>
        new() { Text = text, OnClick = onClick, Disabled = disabled };

    /// <summary>An item that runs an action.</summary>
    public static StilMenuItem Action(string text, Action onClick, bool disabled = false) =>
        new() { Text = text, OnClick = () => { onClick(); return Task.CompletedTask; }, Disabled = disabled };

    /// <summary>An item that navigates to a link.</summary>
    public static StilMenuItem Link(string text, string href, bool disabled = false) =>
        new() { Text = text, Href = href, Disabled = disabled };

    /// <summary>A heading above a group of items.</summary>
    public static StilMenuItem Header(string text) => new() { Text = text, IsHeader = true };

    /// <summary>A line between groups of items.</summary>
    public static StilMenuItem Divider { get; } = new() { IsDivider = true };
}

/// <summary>Horizontal text alignment (logical: Start is left in left-to-right languages).</summary>
public enum StilTextAlign
{
    Start,
    Center,
    End
}

/// <summary>How a row distributes its children along the main axis (justify-content).</summary>
public enum StilJustify
{
    Start,
    Center,
    End,
    Between,
    Around,
    Evenly
}

/// <summary>How a row aligns its children across the main axis (align-items).</summary>
public enum StilAlign
{
    Start,
    Center,
    End,
    Baseline,
    Stretch
}

/// <summary>The shape of a <see cref="StilSkeleton"/>.</summary>
public enum StilSkeletonShape
{
    Rect,
    Text,
    Circle
}
