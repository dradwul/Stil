namespace Stil.Blazor;

/// <summary>Options for <c>AddStil()</c>.</summary>
public sealed class StilOptions
{
    /// <summary>Global token overrides, written to <c>:root</c> by <c>&lt;StilHead /&gt;</c>.</summary>
    public StilTheme Theme { get; set; } = new();

    /// <summary>How long a toast stays visible when no duration is given. The default is 5 seconds.</summary>
    public TimeSpan DefaultToastDuration { get; set; } = TimeSpan.FromSeconds(5);
}
