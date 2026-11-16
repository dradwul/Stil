namespace Stil.Blazor.Blocks;

/// <summary>
/// A page as data: its address, its title and description, and its blocks from top to bottom.
/// <see cref="StilPageJson"/> reads and writes it as JSON, so pages can be stored in a database
/// or files and edited without code; <see cref="StilPage"/> renders it.
/// </summary>
public sealed class StilPageData
{
    /// <summary>The version of the format this page was written in.</summary>
    public int Version { get; set; } = StilPageJson.CurrentVersion;

    /// <summary>The page's address in the site, such as an empty string for the home page or <c>pricing</c>.</summary>
    public string Path { get; set; } = "";

    /// <summary>The page's title, for the browser tab and search results.</summary>
    public string? Title { get; set; }

    /// <summary>A sentence about the page, for search results and shared links.</summary>
    public string? Description { get; set; }

    /// <summary>The page's language, such as <c>en</c>.</summary>
    public string? Language { get; set; }

    /// <summary>The blocks, from the top of the page.</summary>
    public List<StilBlockData> Blocks { get; set; } = [];

    /// <summary>Adds a block and returns the page, to build pages in code.</summary>
    public StilPageData Add(StilBlockData block)
    {
        Blocks.Add(block);
        return this;
    }
}
