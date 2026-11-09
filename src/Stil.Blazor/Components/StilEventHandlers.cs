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

/// <summary>Arguments of the <c>@onstilsplit</c> event: a splitter's first pane is now this size, in percent.</summary>
public sealed class StilSplitEventArgs : EventArgs
{
    public double Size { get; set; }
}

/// <summary>Arguments of the <c>@onstilslide</c> event: the index of the slide a carousel now shows.</summary>
public sealed class StilSlideEventArgs : EventArgs
{
    public int Index { get; set; }
}

/// <summary>Arguments of the <c>@onstilcolumnresize</c> event: a data grid column's new width in pixels.</summary>
public sealed class StilColumnResizeEventArgs : EventArgs
{
    public string Key { get; set; } = "";

    public double Width { get; set; }
}

/// <summary>Arguments of the <c>@onstilcolumnmove</c> event: a column was dragged before another one (or to the end when Before is null).</summary>
public sealed class StilColumnMoveEventArgs : EventArgs
{
    public string Key { get; set; } = "";

    public string? Before { get; set; }
}

/// <summary>Arguments of the <c>@onstilcropzoom</c> event: the zoom of an image cropper after the wheel or a key changed it.</summary>
public sealed class StilCropZoomEventArgs : EventArgs
{
    public double Zoom { get; set; }
}

/// <summary>Arguments of the <c>@onstilcardmove</c> event: a kanban card dropped by dragging.</summary>
public sealed class StilCardMoveEventArgs : EventArgs
{
    /// <summary>The data-stil-item attribute of the card.</summary>
    public string Item { get; set; } = "";

    /// <summary>The data-stil-column attribute of the column it was dropped in.</summary>
    public string Column { get; set; } = "";

    /// <summary>Its place among the column's other cards.</summary>
    public int Index { get; set; }
}

/// <summary>
/// Custom events registered by Stil.lib.module.js. <c>@onstiltoggle</c> fires on a &lt;details&gt;
/// element's toggle event and reports whether it is now open. <c>@onstilcalendaredge</c> and
/// <c>@onstilweekedit</c> are fired by stil.js in calendar grids.
/// </summary>
[EventHandler("onstiltoggle", typeof(StilToggleEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilcalendaredge", typeof(StilCalendarEdgeEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilweekedit", typeof(StilWeekEditEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilsplit", typeof(StilSplitEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilslide", typeof(StilSlideEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilcolumnresize", typeof(StilColumnResizeEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilcolumnmove", typeof(StilColumnMoveEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilcropzoom", typeof(StilCropZoomEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler("onstilcardmove", typeof(StilCardMoveEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class StilEventHandlers
{
}
