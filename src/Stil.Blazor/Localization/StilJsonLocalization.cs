using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Stil.Blazor;

/// <summary>Options for <c>AddStilLocalization()</c>.</summary>
public sealed class StilLocalizationOptions
{
    /// <summary>The folder the JSON files are in, relative to the project. The default is <c>Locales</c>.</summary>
    public string ResourcesPath { get; set; } = "Locales";

    /// <summary>
    /// The language to use when a text is missing in the current culture, and the language of
    /// files without a culture in their name. The default is <c>en</c>.
    /// </summary>
    public string DefaultCulture { get; set; } = "en";

    /// <summary>The assemblies the JSON files are embedded in. The default is the app's entry assembly.</summary>
    public IList<Assembly> Assemblies { get; } = [];
}

/// <summary>
/// The app's JSON text files, embedded in its assemblies. <c>Locales/sv.json</c> holds shared
/// texts; <c>Locales/Pages/Home.sv.json</c> (or <c>Locales/Pages.Home.sv.json</c>) holds the
/// texts of one type. A file without a culture in its name is in the default culture.
/// </summary>
internal sealed partial class StilJsonTextStore
{
    // Base name ("" for shared files) → culture → key → text.
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _files = new(StringComparer.OrdinalIgnoreCase);

    public StilJsonTextStore(StilLocalizationOptions options)
    {
        DefaultCulture = options.DefaultCulture;
        var folder = options.ResourcesPath.Replace('\\', '/').Trim('/');
        IEnumerable<Assembly> assemblies = options.Assemblies.Count > 0
            ? options.Assemblies
            : Assembly.GetEntryAssembly() is { } entry ? [entry] : [];

        foreach (var assembly in assemblies)
        {
            foreach (var resource in assembly.GetManifestResourceNames())
            {
                if (RelativeName(resource, folder) is not { } relative)
                {
                    continue;
                }

                var (baseName, culture) = Parse(relative);
                using var stream = assembly.GetManifestResourceStream(resource)!;
                Dictionary<string, string> texts;
                try
                {
                    texts = StilJsonText.Read(stream);
                }
                catch (Exception exception) when (exception is System.Text.Json.JsonException or FormatException)
                {
                    throw new InvalidOperationException($"The localization file '{resource}' in {assembly.GetName().Name} is not valid: {exception.Message}", exception);
                }

                if (!_files.TryGetValue(baseName, out var cultures))
                {
                    _files[baseName] = cultures = new(StringComparer.OrdinalIgnoreCase);
                }

                if (!cultures.TryGetValue(culture, out var target))
                {
                    cultures[culture] = target = new(StringComparer.Ordinal);
                }

                foreach (var (key, text) in texts)
                {
                    target[key] = text;
                }
            }
        }
    }

    internal string DefaultCulture { get; }

    /// <summary>Base names that have files (<c>""</c> for the shared files).</summary>
    internal IReadOnlyCollection<string> BaseNames => _files.Keys;

    /// <summary>Missing keys already logged, so each is reported once.</summary>
    internal ConcurrentDictionary<string, bool> ReportedMissing { get; } = new(StringComparer.Ordinal);

    internal string? Find(string baseName, string culture, string key) =>
        _files.TryGetValue(baseName, out var cultures)
        && cultures.TryGetValue(culture, out var texts)
        && texts.TryGetValue(key, out var text) ? text : null;

    internal IReadOnlyDictionary<string, string>? Texts(string baseName, string culture) =>
        _files.TryGetValue(baseName, out var cultures) && cultures.TryGetValue(culture, out var texts) ? texts : null;

    /// <summary>The cultures to look in, most specific first, ending with the default culture.</summary>
    internal IEnumerable<string> Cultures(CultureInfo culture, bool includeParents = true)
    {
        var chain = StilLocalizer.CultureChain(culture);
        foreach (var name in includeParents ? chain : chain.Take(1))
        {
            yield return name;
        }

        if (includeParents && !StilLocalizer.CultureChain(culture).Contains(DefaultCulture, StringComparer.OrdinalIgnoreCase))
        {
            yield return DefaultCulture;
        }
    }

    // "Locales/Pages/Home.sv.json" (the LogicalName that Stil.targets gives; on Windows the
    // folder separator inside it can be a backslash) or the SDK's
    // default "MyApp.Locales.Pages.Home.sv.json" both become "Pages.Home.sv.json".
    private static string? RelativeName(string resource, string folder)
    {
        if (!resource.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (resource.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase))
        {
            return resource[(folder.Length + 1)..].Replace('/', '.').Replace('\\', '.');
        }

        var dotted = "." + folder.Replace('/', '.') + ".";
        var index = resource.IndexOf(dotted, StringComparison.OrdinalIgnoreCase);
        return index < 0 ? null : resource[(index + dotted.Length)..];
    }

    private (string BaseName, string Culture) Parse(string relative)
    {
        var name = relative[..^".json".Length];
        var dot = name.LastIndexOf('.');
        var last = dot < 0 ? name : name[(dot + 1)..];
        if (CultureName().IsMatch(last))
        {
            return (dot < 0 ? string.Empty : name[..dot], last);
        }

        return (name, DefaultCulture);
    }

    // A language tag such as "sv", "fil" or "sv-FI"; type names start with a capital letter.
    [GeneratedRegex("^[a-z]{2,3}(-[A-Za-z0-9]{2,8})*$")]
    private static partial Regex CultureName();
}

/// <summary>Creates <see cref="IStringLocalizer"/>s that read the app's JSON files.</summary>
internal sealed class StilJsonStringLocalizerFactory(StilJsonTextStore store, ILoggerFactory? loggerFactory) : IStringLocalizerFactory
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger("Stil.Localization");

    public IStringLocalizer Create(Type resourceSource)
    {
        ArgumentNullException.ThrowIfNull(resourceSource);

        // MyApp.Components.Pages.Home in assembly MyApp: "Components.Pages.Home", then "Pages.Home", then "Home".
        var fullName = resourceSource.FullName ?? resourceSource.Name;
        var assemblyName = resourceSource.Assembly.GetName().Name;
        if (assemblyName is not null && fullName.StartsWith(assemblyName + ".", StringComparison.Ordinal))
        {
            fullName = fullName[(assemblyName.Length + 1)..];
        }

        var parts = fullName.Split('.');
        var baseNames = Enumerable.Range(0, parts.Length).Select(i => string.Join('.', parts[i..])).ToArray();
        return new StilJsonStringLocalizer(store, baseNames, _logger);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        ArgumentNullException.ThrowIfNull(baseName);

        if (!string.IsNullOrEmpty(location) && baseName.StartsWith(location + ".", StringComparison.Ordinal))
        {
            baseName = baseName[(location.Length + 1)..];
        }

        return new StilJsonStringLocalizer(store, [baseName], _logger);
    }
}

/// <summary>
/// Looks a key up in the type's own files and then in the shared files, for each culture from
/// the most specific to the default culture. A missing key returns the key itself.
/// </summary>
internal sealed class StilJsonStringLocalizer(StilJsonTextStore store, string[] baseNames, ILogger? logger) : IStringLocalizer
{
    public LocalizedString this[string name]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(name);
            var text = Find(name, CultureInfo.CurrentUICulture);
            return new LocalizedString(name, text ?? name, text is null, store.DefaultCulture);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var text = this[name];
            return new LocalizedString(name, string.Format(CultureInfo.CurrentCulture, text.Value, arguments), text.ResourceNotFound, text.SearchedLocation);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var culture in store.Cultures(CultureInfo.CurrentUICulture, includeParentCultures))
        {
            foreach (var baseName in baseNames.Append(string.Empty))
            {
                foreach (var (key, text) in store.Texts(baseName, culture) ?? new Dictionary<string, string>())
                {
                    if (seen.Add(key))
                    {
                        yield return new LocalizedString(key, text, false, culture);
                    }
                }
            }
        }
    }

    private string? Find(string name, CultureInfo culture)
    {
        foreach (var cultureName in store.Cultures(culture))
        {
            foreach (var baseName in baseNames)
            {
                if (store.Find(baseName, cultureName, name) is { } own)
                {
                    return own;
                }
            }

            if (store.Find(string.Empty, cultureName, name) is { } shared)
            {
                return shared;
            }
        }

        if (logger is not null && store.ReportedMissing.TryAdd($"{culture.Name}|{name}", true))
        {
            logger.LogWarning("No text for '{Key}' in {Culture} or the default culture {DefaultCulture}.", name, culture.Name, store.DefaultCulture);
        }

        return null;
    }
}
