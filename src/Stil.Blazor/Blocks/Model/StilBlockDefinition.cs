namespace Stil.Blazor.Blocks;

/// <summary>What kind of value a block setting holds, so an editor can show the right field.</summary>
public enum StilBlockSettingKind
{
    /// <summary>A line of text.</summary>
    Text,

    /// <summary>Text over several lines; blank lines separate paragraphs.</summary>
    LongText,

    /// <summary>Yes or no.</summary>
    Boolean,

    /// <summary>A number.</summary>
    Number,

    /// <summary>One of the setting's <see cref="StilBlockSetting.Choices"/>.</summary>
    Choice,

    /// <summary>The name of a built-in icon (see StilIcons).</summary>
    Icon,

    /// <summary>An image: its address and its alt text.</summary>
    Image,

    /// <summary>An address: a page of the site, a full URL, mailto: or tel:.</summary>
    Url,

    /// <summary>A list of short texts.</summary>
    List,

    /// <summary>A date (stored as yyyy-MM-dd).</summary>
    Date,
}

/// <summary>A setting of a block type: its name in the data, a label and what kind of value it holds.</summary>
/// <param name="Name">The name the value is stored under, in camelCase, such as <c>title</c>.</param>
/// <param name="Label">The label of the setting's field in an editor, such as "Title".</param>
/// <param name="Kind">What kind of value it holds.</param>
public sealed record StilBlockSetting(string Name, string Label, StilBlockSettingKind Kind = StilBlockSettingKind.Text)
{
    /// <summary>A line that helps fill it in.</summary>
    public string? Help { get; init; }

    /// <summary>The block needs it.</summary>
    public bool Required { get; init; }

    /// <summary>For <see cref="StilBlockSettingKind.Choice"/>: the values, in kebab case; the first is the default.</summary>
    public IReadOnlyList<string> Choices { get; init; } = [];

    /// <summary>The block has a value for this setting: text that is not empty, a picture with an address, a list with an item.</summary>
    public bool HasValue(StilBlockData block) => Kind switch
    {
        StilBlockSettingKind.Boolean => true,
        StilBlockSettingKind.Number => block.GetNumber(Name) is not null,
        StilBlockSettingKind.Image => block.GetImage(Name) is not null,
        StilBlockSettingKind.List => block.GetList(Name).Count > 0,
        _ => !string.IsNullOrWhiteSpace(block.GetText(Name)),
    };
}

/// <summary>The items a block type has, such as the features of a feature grid: their type, a title and their settings.</summary>
/// <param name="Type">The items' type name, such as <c>feature</c>.</param>
/// <param name="Title">What one item is called in an editor, such as "Feature".</param>
/// <param name="Settings">The settings of each item.</param>
public sealed record StilBlockItemDefinition(string Type, string Title, IReadOnlyList<StilBlockSetting> Settings)
{
    /// <summary>The most items the block shows, or null for no limit.</summary>
    public int? Max { get; init; }
}

/// <summary>
/// A block type: its name in the data, a title and description for a block library, its settings
/// and its items. The built-in types render with <see cref="StilBlockView"/>; your own set
/// <see cref="Component"/> to a component with a <c>[Parameter] StilBlockData Data</c>.
/// </summary>
/// <param name="Type">The name in the data, such as <c>hero</c>.</param>
/// <param name="Title">The name in a block library, such as "Hero".</param>
/// <param name="Settings">The block's settings.</param>
public sealed record StilBlockDefinition(string Type, string Title, IReadOnlyList<StilBlockSetting> Settings)
{
    /// <summary>A sentence about what the block is for.</summary>
    public string? Description { get; init; }

    /// <summary>The block's items, or null when it has none.</summary>
    public StilBlockItemDefinition? Items { get; init; }

    /// <summary>The component that renders a block of this type: it gets the block as its <c>Data</c> parameter. Null for the built-in types.</summary>
    public Type? Component { get; init; }

    /// <summary>The setting with the given name, or null.</summary>
    public StilBlockSetting? Setting(string name) => Settings.FirstOrDefault(setting => setting.Name == name);

    /// <summary>The block has every required setting, and so has each of its items.</summary>
    public bool IsComplete(StilBlockData block) =>
        Settings.All(setting => !setting.Required || setting.HasValue(block))
        && (Items is not { } items || block.Items.All(item => items.Settings.All(setting => !setting.Required || setting.HasValue(item))));
}
