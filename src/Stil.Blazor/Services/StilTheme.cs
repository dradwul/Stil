using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Stil.Blazor;

/// <summary>
/// Token overrides for a Stil theme. Only the values you set are written; everything else keeps
/// the Stil default. Use it globally (<c>AddStil(o =&gt; o.Theme = …)</c> with <c>&lt;StilHead /&gt;</c>)
/// or for part of a page with <c>&lt;StilThemeProvider Theme="…"&gt;</c>.
/// </summary>
public sealed partial class StilTheme
{
    [GeneratedRegex("^--stil-[a-zA-Z0-9-]+$")]
    private static partial Regex TokenNameRegex();

    private static readonly Regex TokenName = TokenNameRegex();

    public string? Primary { get; set; }
    public string? Secondary { get; set; }
    public string? Success { get; set; }
    public string? Danger { get; set; }
    public string? Warning { get; set; }
    public string? Info { get; set; }

    /// <summary>Overrides the derived link and focus color (<c>--stil-color-accent</c>).</summary>
    public string? Accent { get; set; }

    public string? FontFamily { get; set; }

    /// <summary>The default corner radius (<c>--stil-radius-md</c>).</summary>
    public string? Radius { get; set; }

    /// <summary>Any other token, keyed by its full name, for example <c>--stil-space-4</c>.</summary>
    public IDictionary<string, string> Tokens { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>True when the theme overrides at least one token.</summary>
    public bool HasOverrides => GetDeclarations().Any();

    /// <summary>The overrides as CSS declarations, for example <c>--stil-color-primary: #6741d9;</c>.</summary>
    public string ToCssDeclarations()
    {
        var css = new StringBuilder();
        foreach (var (name, value) in GetDeclarations())
        {
            css.Append(CultureInfo.InvariantCulture, $"{name}: {value}; ");
        }

        return css.ToString().TrimEnd();
    }

    private IEnumerable<(string Name, string Value)> GetDeclarations()
    {
        var known = new (string Name, string? Value)[]
        {
            ("--stil-color-primary", Primary),
            ("--stil-color-secondary", Secondary),
            ("--stil-color-success", Success),
            ("--stil-color-danger", Danger),
            ("--stil-color-warning", Warning),
            ("--stil-color-info", Info),
            ("--stil-color-accent", Accent),
            ("--stil-font-family", FontFamily),
            ("--stil-radius-md", Radius),
        };

        foreach (var (name, value) in known.Concat(Tokens.Select(t => (t.Key, (string?)t.Value))))
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!TokenName.IsMatch(name))
            {
                throw new InvalidOperationException(
                    $"Theme token '{name}' must start with '--stil-' and contain only letters, digits and hyphens.");
            }

            // The value ends up inside a style attribute or a <style> element; refuse anything
            // that could end the declaration or the element.
            if (value.IndexOfAny([';', '{', '}', '<', '>']) >= 0)
            {
                throw new InvalidOperationException($"Theme token '{name}' has an invalid value '{value}'.");
            }

            yield return (name, value.Trim());
        }
    }
}
