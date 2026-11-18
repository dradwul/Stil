using System.Text;

namespace Stil.Blazor;

/// <summary>
/// Builds a robots.txt: which paths each crawler may visit, and where the sitemaps are. Map it to an
/// endpoint, such as <c>app.MapGet("/robots.txt", () =&gt; robots.ToString())</c>. Without rules, every
/// crawler may visit everything.
/// </summary>
public sealed class StilRobots
{
    private readonly List<(string UserAgent, List<string> Rules)> _groups = [];
    private readonly List<string> _sitemaps = [];

    /// <summary>Lets the crawler visit the path, such as within a disallowed folder. Returns the robots.</summary>
    public StilRobots Allow(string path, string userAgent = "*") => Rule(userAgent, "Allow", path);

    /// <summary>Asks the crawler to stay out of the path, such as <c>/admin/</c>. Returns the robots.</summary>
    public StilRobots Disallow(string path, string userAgent = "*") => Rule(userAgent, "Disallow", path);

    /// <summary>Names a sitemap by its absolute address. Returns the robots.</summary>
    public StilRobots Sitemap(string url)
    {
        if (!Uri.TryCreate(OneLine(url, nameof(url)), UriKind.Absolute, out _))
        {
            throw new ArgumentException($"A sitemap in robots.txt needs an absolute address, not '{url}'.", nameof(url));
        }

        _sitemaps.Add(url);
        return this;
    }

    /// <summary>The robots.txt text.</summary>
    public override string ToString()
    {
        var text = new StringBuilder();
        var groups = _groups.Count > 0 ? _groups : [("*", ["Disallow:"])];
        foreach (var (userAgent, rules) in groups)
        {
            if (text.Length > 0)
            {
                text.Append('\n');
            }

            text.Append("User-agent: ").Append(userAgent).Append('\n');
            foreach (var rule in rules)
            {
                text.Append(rule).Append('\n');
            }
        }

        if (_sitemaps.Count > 0)
        {
            text.Append('\n');
            foreach (var sitemap in _sitemaps)
            {
                text.Append("Sitemap: ").Append(sitemap).Append('\n');
            }
        }

        return text.ToString();
    }

    private StilRobots Rule(string userAgent, string kind, string path)
    {
        OneLine(userAgent, nameof(userAgent));
        OneLine(path, nameof(path));
        if (_groups.FirstOrDefault(group => group.UserAgent == userAgent).Rules is not { } rules)
        {
            rules = [];
            _groups.Add((userAgent, rules));
        }

        rules.Add($"{kind}: {path}");
        return this;
    }

    // A value on a line of its own: a line break would add a rule.
    private static string OneLine(string value, string name) =>
        value.IndexOfAny(['\r', '\n']) < 0 ? value : throw new ArgumentException("The value must be on one line.", name);
}
