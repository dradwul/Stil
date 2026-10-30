using System.Globalization;
using System.Text.Json;

namespace Stil.Blazor;

/// <summary>
/// The default <see cref="IStilLocalizer"/>. For each culture from the most specific to the
/// neutral one (<c>sv-FI</c>, then <c>sv</c>) it looks in <see cref="StilOptions.Texts"/>, then
/// in the app's JSON files under the <c>Stil.</c> prefix, then in the built-in texts. English is
/// the last resort.
/// </summary>
internal sealed class StilLocalizer(StilOptions? options, StilJsonTextStore? appTexts) : IStilLocalizer
{
    /// <summary>The prefix of Stil's keys in the app's own JSON files: <c>{ "Stil": { "Pagination": { … } } }</c>.</summary>
    internal const string AppKeyPrefix = "Stil.";

    private const string Fallback = "en";

    // Languages that are close enough to share texts when there are no texts of their own.
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["no"] = "nb",
        ["nn"] = "nb",
    };

    private static readonly Lazy<Dictionary<string, Dictionary<string, string>>> BuiltInTexts = new(LoadBuiltIn);

    /// <summary>Only the built-in texts, used when no <see cref="IStilLocalizer"/> is registered.</summary>
    internal static StilLocalizer BuiltIn { get; } = new(null, null);

    /// <summary>The cultures Stil has built-in texts for.</summary>
    internal static IReadOnlyCollection<string> BuiltInCultures => BuiltInTexts.Value.Keys;

    internal static IReadOnlyDictionary<string, string> BuiltInFor(string culture) => BuiltInTexts.Value[culture];

    /// <summary>The localizer for a component: the registered one, or the built-in texts.</summary>
    internal static IStilLocalizer From(IServiceProvider? services) =>
        services?.GetService(typeof(IStilLocalizer)) as IStilLocalizer ?? BuiltIn;

    /// <summary>The registered localizer's text, else the built-in one, else the key itself.</summary>
    internal static string Get(IStilLocalizer localizer, string key, CultureInfo culture) =>
        localizer.GetText(key, culture)
        ?? (ReferenceEquals(localizer, BuiltIn) ? null : BuiltIn.GetText(key, culture))
        ?? key;

    internal static StilLocalizer Create(IServiceProvider services) =>
        new(services.GetService(typeof(StilOptions)) as StilOptions, services.GetService(typeof(StilJsonTextStore)) as StilJsonTextStore);

    public string? GetText(string key, CultureInfo culture)
    {
        foreach (var name in CultureChain(culture))
        {
            var text = options?.Texts.Find(name, key)
                ?? appTexts?.Find(string.Empty, name, AppKeyPrefix + key)
                ?? FindBuiltIn(name, key);
            if (text is not null)
            {
                return text;
            }
        }

        return FindBuiltIn(Fallback, key);
    }

    /// <summary>The culture's name, then its parents' (<c>sv-FI</c>, <c>sv</c>), without the invariant culture.</summary>
    internal static IEnumerable<string> CultureChain(CultureInfo culture)
    {
        for (var current = culture; !string.IsNullOrEmpty(current.Name); current = current.Parent)
        {
            yield return current.Name;
        }
    }

    private static string? FindBuiltIn(string culture, string key)
    {
        var texts = BuiltInTexts.Value;
        if (!texts.ContainsKey(culture) && Aliases.TryGetValue(culture, out var alias))
        {
            culture = alias;
        }

        return texts.TryGetValue(culture, out var found) && found.TryGetValue(key, out var text) ? text : null;
    }

    private static Dictionary<string, Dictionary<string, string>> LoadBuiltIn()
    {
        const string prefix = "Stil.Blazor.Texts.";
        var assembly = typeof(StilLocalizer).Assembly;
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.Ordinal))
            {
                using var stream = assembly.GetManifestResourceStream(name)!;
                result[name[prefix.Length..^".json".Length]] = StilJsonText.Read(stream);
            }
        }

        return result;
    }
}

/// <summary>Reads a JSON text file. Nested objects become dotted keys: <c>{ "Pagination": { "Next": "…" } }</c> is <c>Pagination.Next</c>.</summary>
internal static class StilJsonText
{
    internal static Dictionary<string, string> Read(Stream stream)
    {
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        });

        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        Flatten(document.RootElement, string.Empty, texts);
        return texts;
    }

    private static void Flatten(JsonElement element, string prefix, Dictionary<string, string> texts)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    Flatten(property.Value, prefix.Length == 0 ? property.Name : $"{prefix}.{property.Name}", texts);
                }

                break;
            case JsonValueKind.String:
                texts[prefix] = element.GetString()!;
                break;
            default:
                throw new FormatException($"The text '{prefix}' must be a string or an object, not {element.ValueKind}.");
        }
    }
}
