using System.Globalization;
using System.Text;
using System.Xml;
using Stil.Blazor.Blocks;

namespace Stil.Blazor;

/// <summary>How often a page in a sitemap changes: a hint to search engines.</summary>
public enum StilSitemapFrequency
{
    /// <summary>On every visit.</summary>
    Always,

    /// <summary>Every hour.</summary>
    Hourly,

    /// <summary>Every day.</summary>
    Daily,

    /// <summary>Every week.</summary>
    Weekly,

    /// <summary>Every month.</summary>
    Monthly,

    /// <summary>Every year.</summary>
    Yearly,

    /// <summary>Archived; it does not change.</summary>
    Never,
}

/// <summary>A page in a sitemap: its absolute address and the optional hints.</summary>
/// <param name="Url">The page's absolute address.</param>
/// <param name="LastModified">When it last changed.</param>
/// <param name="ChangeFrequency">How often it changes.</param>
/// <param name="Priority">How important it is next to the site's other pages, from 0 to 1.</param>
public sealed record StilSitemapEntry(string Url, DateTimeOffset? LastModified = null, StilSitemapFrequency? ChangeFrequency = null, double? Priority = null);

/// <summary>
/// Builds a sitemap (sitemaps.org XML) of a site's pages. Map it to an endpoint, such as
/// <c>app.MapGet("/sitemap.xml", () =&gt; Results.Text(sitemap.ToXml(), "application/xml"))</c>, and
/// name it in robots.txt (<see cref="StilRobots.Sitemap"/>).
/// </summary>
public sealed class StilSitemap
{
    /// <summary>The most addresses one sitemap may hold.</summary>
    public const int MaxEntries = 50_000;

    private const string Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private readonly Uri _baseUrl;
    private readonly List<StilSitemapEntry> _entries = [];

    /// <summary>A sitemap of the site at the given address, such as <c>https://example.com/</c>; paths are relative to it.</summary>
    public StilSitemap(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException($"The sitemap's base address '{baseUrl}' must be an absolute http or https address.", nameof(baseUrl));
        }

        _baseUrl = url.AbsoluteUri.EndsWith('/') ? url : new Uri(url.AbsoluteUri + "/");
    }

    /// <summary>The pages, in the order they were added.</summary>
    public IReadOnlyList<StilSitemapEntry> Entries => _entries;

    /// <summary>Adds a page by its path (relative to the base address) or its absolute address. Returns the sitemap.</summary>
    public StilSitemap Add(string path, DateTimeOffset? lastModified = null, StilSitemapFrequency? changeFrequency = null, double? priority = null)
    {
        if (priority is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), priority, "A priority is from 0 to 1.");
        }

        if (_entries.Count == MaxEntries)
        {
            throw new InvalidOperationException($"A sitemap holds at most {MaxEntries} addresses; split the site into several sitemaps.");
        }

        var url = new Uri(_baseUrl, path.TrimStart('/'));
        _entries.Add(new StilSitemapEntry(url.AbsoluteUri, lastModified, changeFrequency, priority));
        return this;
    }

    /// <summary>Adds a page of a site as data, by its path. Returns the sitemap.</summary>
    public StilSitemap AddPage(StilPageData page, DateTimeOffset? lastModified = null) => Add(page.Path, lastModified);

    /// <summary>The sitemap as XML, in UTF-8.</summary>
    public string ToXml()
    {
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true }))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", Namespace);
            foreach (var entry in _entries)
            {
                writer.WriteStartElement("url", Namespace);
                writer.WriteElementString("loc", Namespace, entry.Url);
                if (entry.LastModified is { } modified)
                {
                    writer.WriteElementString("lastmod", Namespace, modified.TimeOfDay == TimeSpan.Zero
                        ? modified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        : modified.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture));
                }

                if (entry.ChangeFrequency is { } frequency)
                {
                    writer.WriteElementString("changefreq", Namespace, frequency.ToString().ToLowerInvariant());
                }

                if (entry.Priority is { } priority)
                {
                    writer.WriteElementString("priority", Namespace, priority.ToString("0.0#", CultureInfo.InvariantCulture));
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
