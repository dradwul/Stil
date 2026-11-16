using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Stil.Blazor.Blocks;

/// <summary>
/// One block of a page, as data: its type (such as <c>hero</c>), its settings and its items (the
/// features of a feature grid, the plans of a pricing table). <see cref="StilBlockView"/> renders
/// it; <see cref="StilBlockDefinition"/> describes what settings and items a type has.
/// </summary>
public sealed class StilBlockData
{
    /// <summary>The block type, as named in the <see cref="StilBlockCatalog"/>, such as <c>hero</c>.</summary>
    public string Type { get; set; } = "";

    /// <summary>The settings, by name. Values are strings, numbers, booleans, lists of strings or images.</summary>
    public JsonObject Settings { get; set; } = [];

    /// <summary>The items of the block, such as the features of a feature grid. Each is a block of its own type.</summary>
    public List<StilBlockData> Items { get; set; } = [];

    /// <summary>A new block of the given type.</summary>
    public static StilBlockData Create(string type) => new() { Type = type };

    /// <summary>Sets a setting and returns the block, to build pages in code. Null removes it.</summary>
    public StilBlockData Set(string name, object? value)
    {
        Settings[name] = value switch
        {
            null => null,
            JsonNode node => node,
            string text => JsonValue.Create(text),
            bool flag => JsonValue.Create(flag),
            int number => JsonValue.Create(number),
            double number => JsonValue.Create(number),
            decimal number => JsonValue.Create(number),
            DateOnly date => JsonValue.Create(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            DateTime date => JsonValue.Create(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            Enum choice => JsonValue.Create(ChoiceName(choice)),
            StilBlockImage image => new JsonObject { ["src"] = image.Src, ["alt"] = image.Alt },
            IEnumerable<string> list => new JsonArray([.. list.Select(item => (JsonNode?)JsonValue.Create(item))]),
            _ => throw new ArgumentException($"A setting cannot hold a {value.GetType().Name}.", nameof(value)),
        };
        if (value is null)
        {
            Settings.Remove(name);
        }

        return this;
    }

    /// <summary>Adds an item and returns the block, to build pages in code.</summary>
    public StilBlockData Add(StilBlockData item)
    {
        Items.Add(item);
        return this;
    }

    /// <summary>A text setting, or null.</summary>
    public string? GetText(string name) =>
        Settings[name] is JsonValue value && value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : null;

    /// <summary>A yes/no setting; false when it is not set.</summary>
    public bool GetBool(string name) =>
        Settings[name] is JsonValue value && value.GetValueKind() == JsonValueKind.True;

    /// <summary>A number setting, or null.</summary>
    public double? GetNumber(string name) =>
        // Read from its JSON, since a value set in code may hold an int or a decimal.
        Settings[name] is JsonValue value && value.GetValueKind() == JsonValueKind.Number
            ? double.Parse(value.ToJsonString(), NumberStyles.Float, CultureInfo.InvariantCulture)
            : null;

    /// <summary>A date setting (stored as yyyy-MM-dd), or null.</summary>
    public DateTime? GetDate(string name) =>
        DateTime.TryParseExact(GetText(name), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

    /// <summary>A list of texts; empty when it is not set.</summary>
    public IReadOnlyList<string> GetList(string name) =>
        Settings[name] is JsonArray array
            ? [.. array.OfType<JsonValue>().Where(item => item.GetValueKind() == JsonValueKind.String).Select(item => item.GetValue<string>())]
            : [];

    /// <summary>An image setting (its address and alt text), or null.</summary>
    public StilBlockImage? GetImage(string name) =>
        Settings[name] is JsonObject image && image["src"] is JsonValue src && src.GetValueKind() == JsonValueKind.String
            ? new StilBlockImage(src.GetValue<string>(), image["alt"] is JsonValue alt && alt.GetValueKind() == JsonValueKind.String ? alt.GetValue<string>() : "")
            : null;

    /// <summary>A choice setting as an enum value, or <paramref name="fallback"/>. Choices are stored in kebab case, such as <c>muted</c>.</summary>
    public TEnum GetChoice<TEnum>(string name, TEnum fallback) where TEnum : struct, Enum =>
        GetText(name) is { } text && Enum.TryParse<TEnum>(text.Replace("-", ""), ignoreCase: true, out var choice) && Enum.IsDefined(choice)
            ? choice
            : fallback;

    /// <summary>The name a choice is stored under: the enum value in kebab case, such as <c>extra-large</c>.</summary>
    public static string ChoiceName(Enum choice)
    {
        var name = choice.ToString();
        var result = new System.Text.StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
            {
                result.Append('-');
            }

            result.Append(char.ToLowerInvariant(name[i]));
        }

        return result.ToString();
    }
}

/// <summary>An image setting of a block: its address and its alt text (empty when it is decorative).</summary>
public sealed record StilBlockImage(string Src, string Alt = "");
