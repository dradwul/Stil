using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>Arguments of the <c>@onstiltoggle</c> event: the new open state of a &lt;details&gt; element.</summary>
public sealed class StilToggleEventArgs : EventArgs
{
    public bool Open { get; set; }
}

/// <summary>
/// Arguments of the <c>@onstilcalendaredge</c> event: the arrow key that moved past the first or
/// last day of a month grid, and that day as yyyy-MM-dd.
/// </summary>
public sealed class StilCalendarEdgeEventArgs : EventArgs
{
    public string Key { get; set; } = "";

    public string? Date { get; set; }
}

/// <summary>
/// Arguments of the <c>@onstilweekedit</c> event: an event was dragged in an editable week grid.
/// </summary>
public sealed class StilWeekEditEventArgs : EventArgs
{
    /// <summary>"move", "resize" or "create".</summary>
    public string Action { get; set; } = "";

    /// <summary>The data-stil-item attribute of the dragged event, or null for "create".</summary>
    public string? Item { get; set; }

    /// <summary>The index of the day column the event was dropped in.</summary>
    public int Day { get; set; }

    /// <summary>The new start, in minutes from midnight.</summary>
    public int Start { get; set; }

    /// <summary>The new end, in minutes from midnight.</summary>
    public int End { get; set; }
}

/// <summary>
/// Custom events registered by Stil.lib.module.js. <c>@onstiltoggle</c> fires on a &lt;details&gt;
/// element's toggle event and reports whether it is now open. <c>@onstilcalendaredge</c> and
/// <c>@onstilweekedit</c> are fired by stil.js in calendar grids.
/// </summary>
[EventHandler("onstiltoggle", typeof(StilToggleEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilcalendaredge", typeof(StilCalendarEdgeEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilweekedit", typeof(StilWeekEditEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class StilEventHandlers
{
}
