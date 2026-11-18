using System.Globalization;
using System.Text.Json.Nodes;
using Stil.Blazor.Blocks;

namespace Stil.Blazor;

/// <summary>What kind of page <see cref="Components.StilSeo"/> describes, for og:type.</summary>
public enum StilSeoType
{
    /// <summary>A page of a website.</summary>
    Website,

    /// <summary>An article or blog post, with its dates and author.</summary>
    Article,
}

/// <summary>
/// Structured data (schema.org, as JSON-LD) that search engines read: the organization behind a site,
/// the site itself, an article, and the questions of an FAQ. Pass them to <c>StilSeo</c>'s
/// <c>StructuredData</c>. Use absolute addresses.
/// </summary>
public static class StilStructuredData
{
    /// <summary>The organization behind the site: its name, address, logo and its profiles elsewhere.</summary>
    public static JsonObject Organization(string name, string url, string? logo = null, IEnumerable<string>? sameAs = null) =>
        Thing("Organization", new()
        {
            ["name"] = name,
            ["url"] = url,
            ["logo"] = logo,
            ["sameAs"] = sameAs is null ? null : new JsonArray([.. sameAs.Select(profile => (JsonNode?)profile)]),
        });

    /// <summary>The site: its name and address.</summary>
    public static JsonObject WebSite(string name, string url) =>
        Thing("WebSite", new() { ["name"] = name, ["url"] = url });

    /// <summary>An article or blog post.</summary>
    /// <param name="headline">The title.</param>
    /// <param name="url">The post's address.</param>
    /// <param name="published">When it was published.</param>
    /// <param name="modified">When it was last changed.</param>
    /// <param name="author">Who wrote it.</param>
    /// <param name="image">Its picture.</param>
    /// <param name="description">A sentence or two about it.</param>
    /// <param name="publisher">The organization that publishes it, by name.</param>
    public static JsonObject Article(string headline, string url, DateTimeOffset? published = null, DateTimeOffset? modified = null,
        string? author = null, string? image = null, string? description = null, string? publisher = null) =>
        Thing("Article", new()
        {
            ["headline"] = headline,
            ["url"] = url,
            ["mainEntityOfPage"] = url,
            ["datePublished"] = published is { } date ? IsoDate(date) : null,
            ["dateModified"] = modified is { } changed ? IsoDate(changed) : null,
            ["author"] = author is null ? null : Node("Person", new() { ["name"] = author }),
            ["image"] = image,
            ["description"] = description,
            ["publisher"] = publisher is null ? null : Node("Organization", new() { ["name"] = publisher }),
        });

    /// <summary>The questions and answers of an FAQ page.</summary>
    public static JsonObject FaqPage(IEnumerable<(string Question, string Answer)> questions) =>
        Thing("FAQPage", new()
        {
            ["mainEntity"] = new JsonArray([.. questions.Select(qa => (JsonNode?)Node("Question", new()
            {
                ["name"] = qa.Question,
                ["acceptedAnswer"] = Node("Answer", new() { ["text"] = qa.Answer }),
            }))]),
        });

    /// <summary>The questions and answers of an <c>faq</c> block of a page as data.</summary>
    public static JsonObject FaqPage(StilBlockData faq) =>
        FaqPage(faq.Items
            .Select(item => (Question: item.GetText("question") ?? "", Answer: item.GetText("answer") ?? ""))
            .Where(qa => qa.Question.Length > 0 && qa.Answer.Length > 0));

    // A schema.org object to put on a page: its context and type first.
    private static JsonObject Thing(string type, Dictionary<string, JsonNode?> properties)
    {
        var thing = new JsonObject { ["@context"] = "https://schema.org" };
        foreach (var (name, value) in Node(type, properties))
        {
            thing[name] = value?.DeepClone();
        }

        return thing;
    }

    // A schema.org object inside another: its type first; null values are left out.
    private static JsonObject Node(string type, Dictionary<string, JsonNode?> properties)
    {
        var thing = new JsonObject { ["@type"] = type };
        foreach (var (name, value) in properties)
        {
            if (value is not null)
            {
                thing[name] = value;
            }
        }

        return thing;
    }

    private static string IsoDate(DateTimeOffset date) => date.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture);
}
