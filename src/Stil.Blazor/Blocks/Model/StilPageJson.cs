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

    private static JsonSerializerOptions CreateOptions(bool writeIndented) => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = writeIndented,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        // A block with no items or settings leaves them out.
        TypeInfoResolver = StilPageJsonContext.Default.WithAddedModifier(OmitEmptyLists),
    };

    private static void OmitEmptyLists(JsonTypeInfo typeInfo)
    {
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
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class StilPageJsonContext : JsonSerializerContext;
