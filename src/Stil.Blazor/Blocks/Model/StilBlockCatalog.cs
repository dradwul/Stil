using static Stil.Blazor.Blocks.StilBlockSettingKind;

namespace Stil.Blazor.Blocks;

/// <summary>
/// The block types a page can use, by name. A new catalog has the built-in types (the page blocks
/// of Stil.Blazor.Blocks); add your own with <see cref="Add"/>. Register one as a singleton service
/// to give <see cref="StilPage"/> and <see cref="StilBlockView"/> your types; without it they use
/// <see cref="Default"/>.
/// </summary>
public sealed class StilBlockCatalog
{
    private readonly Dictionary<string, StilBlockDefinition> _definitions = new(StringComparer.Ordinal);

    /// <summary>A catalog with the built-in block types.</summary>
    public StilBlockCatalog()
    {
        foreach (var definition in BuiltIn)
        {
            _definitions[definition.Type] = definition;
        }
    }

    // Lazy, since the built-in types below are set after this line runs.
    private static readonly Lazy<StilBlockCatalog> LazyDefault = new(() => new StilBlockCatalog());

    /// <summary>The built-in block types, read-only. Make a new catalog to add your own.</summary>
    public static StilBlockCatalog Default => LazyDefault.Value;

    /// <summary>Every block type, in the order they were added: the built-in ones first.</summary>
    public IReadOnlyCollection<StilBlockDefinition> All => _definitions.Values;

    /// <summary>Adds a block type, or replaces the one with the same name. Returns the catalog.</summary>
    public StilBlockCatalog Add(StilBlockDefinition definition)
    {
        if (ReferenceEquals(this, Default))
        {
            throw new InvalidOperationException("StilBlockCatalog.Default holds the built-in types; make a new StilBlockCatalog to add your own.");
        }

        _definitions[definition.Type] = definition;
        return this;
    }

    /// <summary>The block type with the given name, or null.</summary>
    public StilBlockDefinition? Find(string type) => _definitions.GetValueOrDefault(type);

    // Boolean alone would also mean System.Boolean.
    private const StilBlockSettingKind YesNo = StilBlockSettingKind.Boolean;

    // The settings most section blocks share.
    private static readonly StilBlockSetting Anchor = new("id", "Anchor", Text) { Help = "The section's id, for links such as #features." };
    private static readonly StilBlockSetting Tone = new("tone", "Background", Choice) { Choices = ["default", "muted", "brand"] };
    private static readonly StilBlockSetting Eyebrow = new("eyebrow", "Eyebrow", Text) { Help = "A short line above the title." };
    private static readonly StilBlockSetting Title = new("title", "Title", Text);
    private static readonly StilBlockSetting Lead = new("lead", "Lead", LongText) { Help = "A sentence or two under the title." };
    private static readonly StilBlockSetting Centered = new("centered", "Centered header", YesNo);
    private static readonly StilBlockSetting[] SectionHeader = [Anchor, Tone, Eyebrow, Title, Lead, Centered];

    private static readonly StilBlockItemDefinition Actions = new("action", "Button",
    [
        new("text", "Text", Text) { Required = true },
        new("href", "Link", Url) { Required = true },
        new("style", "Style", Choice) { Choices = ["solid", "outline"] },
    ]) { Max = 3 };

    /// <summary>The built-in block types.</summary>
    public static IReadOnlyList<StilBlockDefinition> BuiltIn { get; } =
    [
        new("hero", "Hero", [Anchor, Tone, Eyebrow, Title with { Required = true }, Lead,
            new("image", "Picture", Image) { Help = "Shown beside the text on a wide page." }])
        {
            Description = "The first section of a page: the page's title, a lead and its main buttons.",
            Items = Actions,
        },
        new("features", "Feature grid", [.. SectionHeader, new("cards", "On cards", YesNo)])
        {
            Description = "Features with an icon, a title and a sentence, in as many columns as fit.",
            Items = new("feature", "Feature",
            [
                new("icon", "Icon", Icon),
                new("title", "Title", Text) { Required = true },
                new("text", "Text", LongText),
            ]),
        },
        new("logos", "Logo cloud", [Anchor, Tone, new("title", "Title", Text) { Help = "Such as \"Trusted by teams at\"." }, new("muted", "In gray", YesNo)])
        {
            Description = "The logos of customers or partners.",
            Items = new("logo", "Logo",
            [
                new("name", "Company", Text) { Required = true },
                new("image", "Logo", Image) { Help = "Without one, the name is shown." },
                new("href", "Link", Url),
            ]),
        },
        new("stats", "Stats band", [.. SectionHeader])
        {
            Description = "Large numbers with a label.",
            Items = new("stat", "Number",
            [
                new("value", "Value", Text) { Required = true, Help = "Formatted, such as 12,000 or 99.9%." },
                new("label", "Label", Text) { Required = true },
            ]),
        },
        new("testimonials", "Testimonials", [.. SectionHeader])
        {
            Description = "What customers say.",
            Items = new("testimonial", "Testimonial",
            [
                new("quote", "Quote", LongText) { Required = true },
                new("name", "Name", Text) { Required = true },
                new("role", "Role and company", Text),
                new("photo", "Photo", Url),
                new("featured", "One large quote", YesNo),
            ]),
        },
        new("pricing", "Pricing table", [.. SectionHeader, new("periodSwitch", "Monthly/yearly switch", YesNo),
            new("yearlyNote", "Note beside Yearly", Text) { Help = "Such as \"2 months free\"." }])
        {
            Description = "Plans with their price, what is included and a button.",
            Items = new("plan", "Plan",
            [
                new("name", "Name", Text) { Required = true },
                new("price", "Price", Text) { Required = true },
                new("period", "What the price is for", Text) { Help = "Such as \"a person a month\"." },
                new("yearlyPrice", "Yearly price", Text),
                new("yearlyPeriod", "What the yearly price is for", Text),
                new("text", "Text", LongText),
                new("features", "What is included", List),
                new("featured", "Highlighted", YesNo),
                new("badge", "Badge", Text) { Help = "Such as \"Most popular\"." },
                new("actionText", "Button text", Text),
                new("actionHref", "Button link", Url),
            ]),
        },
        new("faq", "FAQ", [Anchor, Tone, Eyebrow, Title, Lead])
        {
            Description = "Questions and answers that open and close.",
            Items = new("question", "Question",
            [
                new("question", "Question", Text) { Required = true },
                new("answer", "Answer", LongText) { Required = true },
            ]),
        },
        new("team", "Team", [.. SectionHeader])
        {
            Description = "People with a photo or their initials, their role and a line about them.",
            Items = new("member", "Person",
            [
                new("name", "Name", Text) { Required = true },
                new("role", "Role", Text),
                new("text", "Text", LongText),
                new("photo", "Photo", Url),
            ]),
        },
        new("cta", "Call to action", [Anchor, Tone, Title with { Required = true }, new("text", "Text", LongText),
            new("brand", "On the brand color", YesNo), Centered with { Label = "Centered" }])
        {
            Description = "A banner that asks for one thing.",
            Items = Actions,
        },
        new("text", "Text", [.. SectionHeader, new("text", "Text", LongText) { Help = "Blank lines separate paragraphs." }])
        {
            Description = "A section of text.",
        },
        new("contact", "Contact", [Anchor, Tone, Eyebrow, Title, Lead,
            new("email", "E-mail", Text), new("phone", "Phone", Text),
            new("address", "Address", LongText) { Help = "One line each." },
            new("mapHref", "Map link", Url), new("hours", "Opening hours", Text),
            new("formAction", "Form address", Url) { Help = "Where the contact form posts its name, email and message. Without it, there is no form." }])
        {
            Description = "How to reach you, beside a contact form.",
        },
        new("posts", "Post list", [.. SectionHeader])
        {
            Description = "Blog posts as cards.",
            Items = new("post", "Post",
            [
                new("title", "Title", Text) { Required = true },
                new("href", "Link", Url) { Required = true },
                new("excerpt", "Excerpt", LongText),
                new("image", "Picture", Image),
                new("author", "Author", Text),
                new("date", "Date", Date),
                new("readingTime", "Reading time", Text),
                new("tags", "Tags", List),
                new("featured", "Featured", YesNo),
            ]),
        },
    ];
}
