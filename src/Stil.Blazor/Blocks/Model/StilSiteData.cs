namespace Stil.Blazor.Blocks;

/// <summary>
/// A website as data: its name and address, its theme and font, its languages, and its pages.
/// <see cref="StilPageJson.SerializeSite"/> reads and writes it, so a site's look is stored with
/// its pages. Give <c>&lt;StilHead /&gt;</c> its <see cref="Theme"/> and <see cref="Font"/> for the
/// whole document, and <c>StilSeo</c> the <see cref="AlternatesOf"/> of each page.
/// </summary>
public sealed class StilSiteData
{
    /// <summary>The version of the format the site was written in.</summary>
    public int Version { get; set; } = StilPageJson.CurrentVersion;

    /// <summary>The site's name, such as for the browser's title.</summary>
    public string Name { get; set; } = "";

    /// <summary>The site's address, such as <c>https://example.com/</c>, for a sitemap and the alternates' addresses.</summary>
    public string? Url { get; set; }

    /// <summary>The language of the site's main pages, such as <c>en</c>; its pages are the x-default of their translations.</summary>
    public string? Language { get; set; }

    /// <summary>The site's colors, corners and other token overrides.</summary>
    public StilTheme? Theme { get; set; }

    /// <summary>The site's web font.</summary>
    public StilFont? Font { get; set; }

    /// <summary>The pages, each with its own path.</summary>
    public List<StilPageData> Pages { get; set; } = [];

    /// <summary>Adds a page and returns the site, to build sites in code.</summary>
    public StilSiteData Add(StilPageData page)
    {
        Pages.Add(page);
        return this;
    }

    /// <summary>The page at the path, ignoring slashes at its ends and case; null when there is none.</summary>
    public StilPageData? Find(string path)
    {
        var wanted = path.Trim('/');
        return Pages.FirstOrDefault(page => string.Equals(page.Path.Trim('/'), wanted, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The page and its translations: the pages with its <see cref="StilPageData.Key"/>, in the order of the site.</summary>
    public IReadOnlyList<StilPageData> TranslationsOf(StilPageData page) =>
        page.Key is { Length: > 0 } key ? [.. Pages.Where(other => other.Key == key)] : [page];

    /// <summary>
    /// The language alternates (hreflang) of a page: each translation by its language and address,
    /// and the one in the site's <see cref="Language"/> also as <c>x-default</c>. Empty when the page has
    /// no translations. Addresses are absolute when the site has a <see cref="Url"/>.
    /// </summary>
    public IReadOnlyList<StilAlternate> AlternatesOf(StilPageData page)
    {
        var translations = TranslationsOf(page).Where(translation => !string.IsNullOrEmpty(translation.Language)).ToList();
        if (translations.Count < 2)
        {
            return [];
        }

        var alternates = translations.Select(translation => new StilAlternate(translation.Language!, Address(translation))).ToList();
        if (translations.FirstOrDefault(translation => string.Equals(translation.Language, Language, StringComparison.OrdinalIgnoreCase)) is { } main)
        {
            alternates.Add(new StilAlternate("x-default", Address(main)));
        }

        return alternates;
    }

    private string Address(StilPageData page) =>
        Url is { Length: > 0 } url && Uri.TryCreate(url.EndsWith('/') ? url : url + "/", UriKind.Absolute, out var site)
            ? new Uri(site, page.Path.TrimStart('/')).AbsoluteUri
            : "/" + page.Path.TrimStart('/');
}
