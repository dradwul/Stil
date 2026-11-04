using System.Text.RegularExpressions;

namespace Stil.Blazor;

/// <summary>
/// A web font: the family name and the stylesheet that loads it. Set it with
/// <c>AddStil(o =&gt; o.Font = …)</c>; <c>&lt;StilHead /&gt;</c> links the stylesheet and sets
/// <c>--stil-font-family</c>. Use a font from <see cref="StilFonts"/>, <see cref="CdnFonts"/>,
/// <see cref="Google"/>, or the constructor for a stylesheet of your own.
/// </summary>
/// <param name="Family">The font-family name, for example <c>BR Cobane</c>.</param>
/// <param name="StylesheetUrl">The stylesheet with the <c>@font-face</c> rules, or null for a font the page already has.</param>
/// <param name="Fallback">The fonts used until it loads, or when it has no glyph. The default is <c>sans-serif</c>.</param>
public sealed partial record StilFont(string Family, string? StylesheetUrl, string Fallback = "sans-serif")
{
    [GeneratedRegex(@"^[\p{L}\p{N} ._-]+$")]
    private static partial Regex FamilyNameRegex();

    /// <summary>The value for <c>--stil-font-family</c>, for example <c>'BR Cobane', sans-serif</c>.</summary>
    public string FontFamily => $"'{Family}', {Fallback}";

    /// <summary>A font from cdnfonts.com. The slug is the font's address there; it defaults to the family name in lowercase with hyphens.</summary>
    /// <example><c>StilFont.CdnFonts("BR Cobane")</c> loads <c>https://fonts.cdnfonts.com/css/br-cobane</c>.</example>
    public static StilFont CdnFonts(string family, string? slug = null) =>
        new(family, $"https://fonts.cdnfonts.com/css/{slug ?? Slug(family)}");

    /// <summary>A font from Google Fonts, in the regular and bold weights Stil uses (400 and 600).</summary>
    /// <param name="family">The family name as Google Fonts shows it, for example <c>Inter</c> or <c>Open Sans</c>.</param>
    /// <param name="weights">The weights to load, separated by semicolons and in ascending order.</param>
    public static StilFont Google(string family, string weights = "400;600") =>
        new(family, $"https://fonts.googleapis.com/css2?family={Uri.EscapeDataString(family).Replace("%20", "+", StringComparison.Ordinal)}:wght@{weights}&display=swap");

    /// <summary>Checks the values before they are written to the page.</summary>
    internal void Validate()
    {
        if (!FamilyNameRegex().IsMatch(Family))
        {
            throw new InvalidOperationException($"Font family '{Family}' may contain only letters, digits, spaces, dots, hyphens and underscores.");
        }

        if (Fallback.IndexOfAny([';', '{', '}', '<', '>']) >= 0)
        {
            throw new InvalidOperationException($"Font fallback '{Fallback}' is not valid.");
        }

        if (StylesheetUrl is not null && !(Uri.TryCreate(StylesheetUrl, UriKind.RelativeOrAbsolute, out var url) && (!url.IsAbsoluteUri || url.Scheme == Uri.UriSchemeHttps)))
        {
            throw new InvalidOperationException($"Font stylesheet '{StylesheetUrl}' must be a relative or https URL.");
        }
    }

    private static string Slug(string family) => family.Trim().ToLowerInvariant().Replace(' ', '-');
}

/// <summary>
/// Ready-made fonts from cdnfonts.com. Check each font's license on cdnfonts.com before you use it in a
/// product; some are free for personal use only. Add another with <see cref="StilFont.CdnFonts"/>.
/// </summary>
public static class StilFonts
{
    /// <summary>Lexend, Stil's default font.</summary>
    public static StilFont Lexend { get; } = StilFont.CdnFonts("Lexend");
    public static StilFont BrCobane { get; } = StilFont.CdnFonts("BR Cobane");
    public static StilFont BrSegma { get; } = StilFont.CdnFonts("BR Segma");
    public static StilFont BrSonoma { get; } = StilFont.CdnFonts("BR Sonoma");
    public static StilFont BrShape { get; } = StilFont.CdnFonts("BR Shape");
    public static StilFont Averral { get; } = StilFont.CdnFonts("Averral");
    public static StilFont Sukplena { get; } = StilFont.CdnFonts("Sukplena");

    /// <summary>A display font, for headings and short texts more than for body text.</summary>
    public static StilFont Blowreph { get; } = StilFont.CdnFonts("BLOWREPH");

    /// <summary>A display font, for headings and short texts more than for body text.</summary>
    public static StilFont Taster { get; } = StilFont.CdnFonts("Taster");

    /// <summary>A display font, for headings and short texts more than for body text.</summary>
    public static StilFont Zagist { get; } = StilFont.CdnFonts("Zagist");

    /// <summary>Every font above, Lexend first.</summary>
    public static IReadOnlyList<StilFont> All { get; } =
        [Lexend, BrCobane, BrSegma, BrSonoma, BrShape, Averral, Sukplena, Blowreph, Taster, Zagist];
}
