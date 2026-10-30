namespace Stil.Blazor;

/// <summary>
/// App-wide overrides for the built-in texts of Stil components, set through
/// <c>AddStil(o =&gt; o.Texts.Add(…))</c>. Use them to reword a text everywhere, or to add a
/// language Stil has no built-in texts for. The keys are listed on the localization docs page.
/// </summary>
public sealed class StilTextOptions
{
    private readonly Dictionary<string, Dictionary<string, string>> _texts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Adds or replaces texts for a culture, such as <c>"de"</c> or <c>"sv-FI"</c>.</summary>
    public StilTextOptions Add(string culture, IEnumerable<KeyValuePair<string, string>> texts)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(texts);

        if (!_texts.TryGetValue(culture, out var target))
        {
            _texts[culture] = target = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        foreach (var (key, text) in texts)
        {
            target[key] = text;
        }

        return this;
    }

    /// <summary>Adds or replaces one text for a culture, for example <c>Add("sv", "Dialog.Confirm", "Ja")</c>.</summary>
    public StilTextOptions Add(string culture, string key, string text) =>
        Add(culture, [KeyValuePair.Create(key, text)]);

    internal string? Find(string culture, string key) =>
        _texts.TryGetValue(culture, out var texts) && texts.TryGetValue(key, out var text) ? text : null;
}
