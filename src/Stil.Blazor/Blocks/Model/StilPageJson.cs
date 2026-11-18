using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Stil.Blazor.Blocks;

/// <summary>
/// Reads and writes pages (<see cref="StilPageData"/>) as JSON: camelCase names, no empty values,
/// and a version, so a newer format is refused instead of read wrongly. It uses source-generated
/// serialization, so it also works in a trimmed WebAssembly app.
/// </summary>
public static class StilPageJson
{
    /// <summary>The version of the format that this version of Stil writes and reads.</summary>
    public const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions Options = CreateOptions(writeIndented: true);
    private static readonly JsonSerializerOptions CompactOptions = CreateOptions(writeIndented: false);

    /// <summary>The page as JSON; indented by default, for files and diffs.</summary>
    public static string Serialize(StilPageData page, bool indented = true) =>
        JsonSerializer.Serialize(page, (JsonTypeInfo<StilPageData>)(indented ? Options : CompactOptions).GetTypeInfo(typeof(StilPageData)));

    /// <summary>Reads a page. Throws a <see cref="JsonException"/> when the JSON is not a page, or is of a newer version.</summary>
    public static StilPageData Deserialize(string json)
    {
        var page = JsonSerializer.Deserialize(json, (JsonTypeInfo<StilPageData>)Options.GetTypeInfo(typeof(StilPageData)))
            ?? throw new JsonException("The JSON is not a page.");
        if (page.Version > CurrentVersion)
        {
            throw new JsonException($"The page is in version {page.Version} of the format; this version of Stil reads up to version {CurrentVersion}.");
        }

        return page;
    }

    /// <summary>The site, with its theme, font and pages, as JSON; indented by default.</summary>
    public static string SerializeSite(StilSiteData site, bool indented = true) =>
        JsonSerializer.Serialize(site, (JsonTypeInfo<StilSiteData>)(indented ? Options : CompactOptions).GetTypeInfo(typeof(StilSiteData)));

    /// <summary>
    /// Reads a site. Throws a <see cref="JsonException"/> when the JSON is not a site, is of a newer
    /// version, or has a theme or font that could break the page (a value with <c>;</c>, <c>{</c> or <c>&lt;</c>).
    /// </summary>
    public static StilSiteData DeserializeSite(string json)
    {
        var site = JsonSerializer.Deserialize(json, (JsonTypeInfo<StilSiteData>)Options.GetTypeInfo(typeof(StilSiteData)))
            ?? throw new JsonException("The JSON is not a site.");
        if (site.Version > CurrentVersion || site.Pages.FirstOrDefault(page => page.Version > CurrentVersion) is not null)
        {
            throw new JsonException($"The site is in a newer version of the format; this version of Stil reads up to version {CurrentVersion}.");
        }

        try
        {
            // The theme and font end up in the page's CSS; check them now rather than when a page renders.
            site.Theme?.ToCssDeclarations();
            site.Font?.Validate();
        }
        catch (InvalidOperationException error)
        {
            throw new JsonException(error.Message, error);
        }

        return site;
    }

    private static JsonSerializerOptions CreateOptions(bool writeIndented) => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = writeIndented,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        // A block with no items or settings, or a theme with no other tokens, leaves them out.
        TypeInfoResolver = StilPageJsonContext.Default.WithAddedModifier(OmitEmptyLists),
    };

    private static void OmitEmptyLists(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type == typeof(StilTheme))
        {
            foreach (var property in typeInfo.Properties.Where(property => property.Name == "tokens"))
            {
                property.ShouldSerialize = (_, value) => value is IDictionary<string, string> { Count: > 0 };
            }

            return;
        }

        if (typeInfo.Type != typeof(StilBlockData))
        {
            return;
        }

        foreach (var property in typeInfo.Properties)
        {
            if (property.Name == "items")
            {
                property.ShouldSerialize = (_, value) => value is List<StilBlockData> { Count: > 0 };
            }
            else if (property.Name == "settings")
            {
                property.ShouldSerialize = (_, value) => value is System.Text.Json.Nodes.JsonObject { Count: > 0 };
            }
        }
    }
}

[JsonSerializable(typeof(StilPageData))]
[JsonSerializable(typeof(StilSiteData))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class StilPageJsonContext : JsonSerializerContext;
