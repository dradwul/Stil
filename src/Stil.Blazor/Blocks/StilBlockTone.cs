namespace Stil.Blazor.Blocks;

/// <summary>The background of a page block.</summary>
public enum StilBlockTone
{
    /// <summary>The page background. For a call to action: the muted surface.</summary>
    Default,

    /// <summary>The muted surface, to set a section apart from the ones around it.</summary>
    Muted,

    /// <summary>The primary color. Text, links, focus rings and buttons on it take colors that read on it.</summary>
    Brand,
}
