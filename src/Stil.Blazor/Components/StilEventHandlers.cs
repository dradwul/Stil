using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>Arguments of the <c>@onstiltoggle</c> event: the new open state of a &lt;details&gt; element.</summary>
public sealed class StilToggleEventArgs : EventArgs
{
    public bool Open { get; set; }
}

/// <summary>
/// Custom events registered by Stil.lib.module.js. <c>@onstiltoggle</c> fires on a &lt;details&gt;
/// element's toggle event and reports whether it is now open.
/// </summary>
[EventHandler("onstiltoggle", typeof(StilToggleEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class StilEventHandlers
{
}
