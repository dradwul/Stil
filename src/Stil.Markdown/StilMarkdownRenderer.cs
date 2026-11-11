using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Stil.Markdown;

/// <summary>
/// Turns Markdown into safe HTML, as <c>StilMarkdown</c> shows it: CommonMark with tables, task
/// lists, autolinks and strikethrough. Raw HTML in the Markdown is shown as text, and links and
/// images only go to http, https, mailto and tel addresses or relative ones. Use it on the server
/// too, for example for an e-mail.
/// </summary>
public static class StilMarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseTaskLists()
        .UseAutoLinks()
        .UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough)
        .DisableHtml()
        .Build();

    private static readonly string[] LinkSchemes = ["http", "https", "mailto", "tel"];
    private static readonly string[] ImageSchemes = ["http", "https"];

    /// <summary>The Markdown as HTML. Tables get the stil-table class.</summary>
    public static string ToHtml(string? markdown)
    {
        var document = Markdig.Markdown.Parse(markdown ?? "", Pipeline);
        foreach (var link in document.Descendants<LinkInline>())
        {
            if (!IsSafe(link.Url, link.IsImage ? ImageSchemes : LinkSchemes))
            {
                link.Url = "#";
            }
            else if (IsExternal(link.Url) && !link.IsImage)
            {
                link.GetAttributes().AddPropertyIfNotExist("rel", "noopener noreferrer");
            }
        }

        foreach (var link in document.Descendants<AutolinkInline>())
        {
            if (!IsSafe(link.Url, LinkSchemes))
            {
                link.Url = "#";
            }
        }

        foreach (var table in document.Descendants<Table>())
        {
            table.GetAttributes().AddClass("stil-table");
        }

        // A task's checkbox gets the task's text as its name, so a screen reader says what is done.
        foreach (var task in document.Descendants<TaskList>())
        {
            if (task.Parent is { } item && TextOf(item) is { Length: > 0 } text)
            {
                task.GetAttributes().AddPropertyIfNotExist("aria-label", text);
            }
        }

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        Pipeline.Setup(renderer);
        renderer.Render(document);
        return writer.ToString();
    }

    /// <summary>
    /// True for a relative address, an anchor, or an http, https, mailto or tel address (for an
    /// image, only http and https): the addresses the rendered links and images may use.
    /// </summary>
    public static bool IsSafeUrl(string? url, bool image = false) => IsSafe(url, image ? ImageSchemes : LinkSchemes);

    // A relative address, an anchor, or one of the allowed schemes.
    internal static bool IsSafe(string? url, string[] schemes)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        var trimmed = url.Trim();
        var colon = trimmed.IndexOf(':');
        var firstOther = trimmed.IndexOfAny(['/', '?', '#']);
        if (colon < 0 || firstOther >= 0 && firstOther < colon)
        {
            return true;
        }

        var scheme = trimmed[..colon];
        return schemes.Any(s => s.Equals(scheme, StringComparison.OrdinalIgnoreCase));
    }

    // The plain text of a line of inlines: its words and code, without the markup.
    private static string TextOf(ContainerInline inlines) =>
        string.Join(" ", string.Concat(inlines.Descendants().Select(inline => inline switch
        {
            LiteralInline literal => literal.Content.ToString(),
            CodeInline code => code.Content,
            LineBreakInline => " ",
            _ => "",
        })).Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static bool IsExternal(string? url) =>
        url is not null && (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
}
