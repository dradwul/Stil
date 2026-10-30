namespace Stil.Blazor;

/// <summary>Options for <c>AddStil()</c>.</summary>
public sealed class StilOptions
{
    /// <summary>Global token overrides, written to <c>:root</c> by <c>&lt;StilHead /&gt;</c>.</summary>
    public StilTheme Theme { get; set; } = new();

    /// <summary>
    /// Supplies icons from another icon set (for example Material Symbols) to StilIcon. Names it
    /// does not know fall back to Stil's built-in icons.
    /// </summary>
    public IStilIconProvider? IconProvider { get; set; }

    /// <summary>How long a toast stays visible when no duration is given. The default is 5 seconds.</summary>
    public TimeSpan DefaultToastDuration { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Overrides for the built-in texts of the components, or texts for another language:
    /// <c>o.Texts.Add("de", "Common.Close", "Schließen")</c>.
    /// </summary>
    public StilTextOptions Texts { get; } = new();
}
