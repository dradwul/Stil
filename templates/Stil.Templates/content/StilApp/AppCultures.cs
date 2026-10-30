namespace StilApp;

/// <summary>The languages the app offers in its language selector.</summary>
public static class AppCultures
{
    /// <summary>The language for visitors whose browser asks for none of <see cref="All"/>.</summary>
    public const string Default = "STIL_DEFAULT_CULTURE";

    /// <summary>
    /// Stil's components have built-in texts in these languages. The app's own texts are in
    /// Locales/*.json; a language without a file there falls back to English.
    /// </summary>
    public static readonly string[] All = ["en", "sv", "nb", "da", "fi"];
}
