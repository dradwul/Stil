using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Stil.Blazor;

/// <summary>
/// Helpers for the culture a <c>StilLanguageSelector</c> picks. The selector stores the choice
/// in the standard ASP.NET Core culture cookie (read by <c>UseRequestLocalization()</c> on the
/// server) and in the browser's local storage (read by <see cref="ApplyStoredAsync"/> in
/// WebAssembly), then reloads the page.
/// </summary>
public static class StilCulture
{
    /// <summary>The cookie the selector writes: the default name of ASP.NET Core's <c>CookieRequestCultureProvider</c>.</summary>
    public const string CookieName = ".AspNetCore.Culture";

    /// <summary>The local storage key the selector writes, for WebAssembly apps.</summary>
    public const string StorageKey = "stil-culture";

    // Each language's own name for itself, so the names do not depend on ICU data.
    private static readonly Dictionary<string, string> NativeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ar"] = "العربية",
        ["da"] = "Dansk",
        ["de"] = "Deutsch",
        ["en"] = "English",
        ["es"] = "Español",
        ["et"] = "Eesti",
        ["fa"] = "فارسی",
        ["fi"] = "Suomi",
        ["fr"] = "Français",
        ["he"] = "עברית",
        ["is"] = "Íslenska",
        ["it"] = "Italiano",
        ["lt"] = "Lietuvių",
        ["lv"] = "Latviešu",
        ["nb"] = "Norsk bokmål",
        ["nl"] = "Nederlands",
        ["nn"] = "Norsk nynorsk",
        ["pl"] = "Polski",
        ["pt"] = "Português",
        ["se"] = "Davvisámegiella",
        ["sv"] = "Svenska",
        ["uk"] = "Українська",
        ["ur"] = "اردو",
    };

    /// <summary>ArrowLeft and ArrowRight as "previous" and "next": swapped in a right-to-left culture.</summary>
    internal static string LogicalArrow(string key, CultureInfo culture) => !IsRightToLeft(culture) ? key : key switch
    {
        "ArrowLeft" => "ArrowRight",
        "ArrowRight" => "ArrowLeft",
        _ => key
    };

    private static readonly HashSet<string> RightToLeft = new(StringComparer.OrdinalIgnoreCase) { "ar", "fa", "he", "ps", "ur", "yi" };

    /// <summary>
    /// The language's name in that language ("Svenska", "Suomi"), for a language picker. A
    /// regional culture gets its region from .NET when the data is available.
    /// </summary>
    public static string DisplayName(string culture)
    {
        ArgumentException.ThrowIfNullOrEmpty(culture);

        if (NativeNames.TryGetValue(culture, out var name))
        {
            return name;
        }

        try
        {
            var info = CultureInfo.GetCultureInfo(culture);
            if (!string.IsNullOrEmpty(info.NativeName) && info.NativeName != culture)
            {
                return char.ToUpper(info.NativeName[0], info) + info.NativeName[1..];
            }
        }
        catch (CultureNotFoundException)
        {
        }

        var language = culture.Split('-')[0];
        return NativeNames.TryGetValue(language, out var languageName) ? languageName : culture;
    }

    /// <summary>The languages Stil has built-in texts for: en, sv, nb, da and fi.</summary>
    public static IReadOnlyCollection<string> BuiltInLanguages => StilLocalizer.BuiltInCultures;

    /// <summary>
    /// The built-in texts of a language (see <see cref="BuiltInLanguages"/>), keyed like
    /// <c>Pagination.Next</c>. Use the English ones as the list of keys to translate.
    /// </summary>
    public static IReadOnlyDictionary<string, string> BuiltInTexts(string language) => StilLocalizer.BuiltInFor(language);

    /// <summary><c>rtl</c> for right-to-left languages such as Arabic and Hebrew, otherwise <c>ltr</c>. Put it on <c>&lt;html dir&gt;</c>.</summary>
    public static string Direction(CultureInfo? culture = null) => IsRightToLeft(culture) ? "rtl" : "ltr";

    /// <summary>Whether <paramref name="culture"/> (by default the current UI culture) is written right to left.</summary>
    public static bool IsRightToLeft(CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentUICulture;
        return culture.TextInfo.IsRightToLeft
            || RightToLeft.Contains(culture.TwoLetterISOLanguageName)
            || RightToLeft.Contains(culture.Name.Split('-')[0]);
    }

    /// <summary>
    /// For WebAssembly apps: applies the culture from a <c>?culture=</c> link, else the one the
    /// selector stored, else <paramref name="defaultCulture"/>, before the app renders. Call it in <c>Program.cs</c>:
    /// <c>var host = builder.Build(); await StilCulture.ApplyStoredAsync(host.Services); await host.RunAsync();</c>
    /// </summary>
    /// <returns>The culture that was applied, or <c>null</c> when none was.</returns>
    public static async Task<CultureInfo?> ApplyStoredAsync(IServiceProvider services, string? defaultCulture = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // A ?culture= link (the selector's fallback when stil.js has not loaded yet) wins.
        string? name = null;
        if (services.GetService(typeof(NavigationManager)) is NavigationManager navigation
            && Uri.TryCreate(navigation.Uri, UriKind.Absolute, out var uri))
        {
            name = CultureFromQuery(uri.Query);
        }

        if (string.IsNullOrEmpty(name) && services.GetService(typeof(IJSRuntime)) is IJSRuntime js)
        {
            try
            {
                name = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            }
            catch (JSException)
            {
                // Storage can be blocked (private mode, site settings); use the default.
            }
        }

        name = string.IsNullOrEmpty(name) ? defaultCulture : name;
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(name, predefinedOnly: true);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        return culture;
    }

    private static string? CultureFromQuery(string query)
    {
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && parts[0] == "culture")
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }
}
