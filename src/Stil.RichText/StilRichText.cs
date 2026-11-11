using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Stil.Markdown;

namespace Stil.RichText;

/// <summary>The format StilRichTextEditor writes and StilRichTextView reads.</summary>
public enum StilRichTextFormat
{
    /// <summary>Sanitized HTML.</summary>
    Html,

    /// <summary>Markdown, as Stil.Markdown reads it.</summary>
    Markdown,
}

/// <summary>
/// Cleans and converts rich text. <see cref="Sanitize"/> keeps an allow-list of tags (paragraphs,
/// line breaks, bold, italic, underline, strikethrough, headings 2–4, lists, links, quotes, code)
/// and drops every attribute except a safe <c>href</c>; other tags are unwrapped to their content,
/// and scripts, styles and embedded content are removed with what is inside them. The output is
/// written from the parsed document, never copied from the input, so pasted Word and web markup
/// comes out as plain, tidy HTML.
/// </summary>
public static class StilRichText
{
    private static readonly HtmlParser Parser = new();

    // The tags kept, by the name they are written as; tags with the same meaning are renamed.
    private static readonly Dictionary<string, string> Kept = new(StringComparer.OrdinalIgnoreCase)
    {
        ["p"] = "p", ["div"] = "p", ["br"] = "br",
        ["strong"] = "strong", ["b"] = "strong", ["em"] = "em", ["i"] = "em", ["u"] = "u",
        ["s"] = "del", ["strike"] = "del", ["del"] = "del",
        ["h1"] = "h2", ["h2"] = "h2", ["h3"] = "h3", ["h4"] = "h4", ["h5"] = "h4", ["h6"] = "h4",
        ["ul"] = "ul", ["ol"] = "ol", ["li"] = "li",
        ["a"] = "a", ["blockquote"] = "blockquote", ["code"] = "code", ["pre"] = "pre",
    };

    // Removed together with everything inside them.
    private static readonly HashSet<string> Dropped = new(StringComparer.OrdinalIgnoreCase)
    {
        "script", "style", "template", "iframe", "object", "embed", "noscript", "head", "title", "meta", "link",
        "svg", "math", "canvas", "audio", "video", "img", "picture", "form", "input", "button", "select", "textarea",
    };

    private static readonly HashSet<string> BlockTags = ["p", "h2", "h3", "h4", "ul", "ol", "li", "blockquote", "pre"];

    /// <summary>The HTML with only the allowed tags and safe links; empty when there is no text.</summary>
    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return "";
        }

        var body = Parse(html);
        var output = new StringBuilder();
        foreach (var node in body.ChildNodes)
        {
            Write(node, output);
        }

        var result = output.ToString().Trim();
        return HasText(result) ? result : "";
    }

    /// <summary>True when the HTML has any visible text (not counting scripts, styles and the like).</summary>
    public static bool HasText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return false;
        }

        var body = Parse(html);
        foreach (var element in body.QuerySelectorAll("*").Where(e => Dropped.Contains(e.LocalName)).ToList())
        {
            element.Remove();
        }

        return body.TextContent.Trim().Length > 0;
    }

    /// <summary>The sanitized HTML as Markdown.</summary>
    public static string ToMarkdown(string? html)
    {
        var clean = Sanitize(html);
        if (clean.Length == 0)
        {
            return "";
        }

        var output = new StringBuilder();
        WriteBlocks(Parse(clean).ChildNodes, output, "");
        return output.ToString().Trim() + "\n";
    }

    /// <summary>Markdown as sanitized HTML, for the editor and the view.</summary>
    public static string FromMarkdown(string? markdown) => Sanitize(StilMarkdownRenderer.ToHtml(markdown));

    private static bool IsBlock(INode? node) =>
        node is IElement element && (Dropped.Contains(element.LocalName)
            || Kept.TryGetValue(element.LocalName, out var tag) && BlockTags.Contains(tag));

    private static bool HasVisibleText(INode node) => node switch
    {
        IText text => !string.IsNullOrWhiteSpace(text.Data),
        IElement element when Dropped.Contains(element.LocalName) => false,
        _ => node.ChildNodes.Any(HasVisibleText),
    };

    // Only what HTML needs escaped, so letters and signs such as å and × stay readable in the saved text.
    private static string Encode(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static IElement Parse(string html) => Parser.ParseDocument($"<!doctype html><body>{html}</body>").Body!;

    private static void Write(INode node, StringBuilder output)
    {
        switch (node)
        {
            case IText text when string.IsNullOrWhiteSpace(text.Data) && (IsBlock(text.PreviousSibling) || IsBlock(text.NextSibling)):
                // Whitespace between blocks shows nothing; each block already ends with a line break.
                return;
            case IText text:
                output.Append(Encode(text.Data));
                return;
            case IElement element when Dropped.Contains(element.LocalName):
                return;
            case IElement element when IsBlock(element) && !HasVisibleText(element):
                // Empty paragraphs, headings and lists that the browser leaves behind.
                return;
            case IElement element when Kept.TryGetValue(element.LocalName, out var tag) && !IsPlainWrapper(element):
                if (tag == "br")
                {
                    output.Append("<br>");
                    return;
                }

                output.Append('<').Append(tag);
                var href = tag == "a" ? element.GetAttribute("href")?.Trim() : null;
                if (href is { Length: > 0 } && StilMarkdownRenderer.IsSafeUrl(href))
                {
                    output.Append(" href=\"").Append(Encode(href)).Append('"');
                    if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        output.Append(" rel=\"noopener noreferrer\"");
                    }
                }

                output.Append('>');
                foreach (var child in element.ChildNodes)
                {
                    Write(child, output);
                }

                output.Append("</").Append(tag).Append('>');
                if (BlockTags.Contains(tag))
                {
                    output.Append('\n');
                }

                return;
            case IElement element:
                // Unknown and presentational tags (span, font, Word's o:p) keep only their content.
                foreach (var child in element.ChildNodes)
                {
                    Write(child, output);
                }

                return;
        }
    }

    // Google Docs wraps a whole paste in <b style="font-weight:normal">, which is not bold.
    private static bool IsPlainWrapper(IElement element) =>
        element.LocalName is "b"
        && (element.GetAttribute("style") ?? "").Replace(" ", "", StringComparison.Ordinal).Contains("font-weight:normal", StringComparison.OrdinalIgnoreCase);

    // Block-level Markdown: each block on its own lines, with a blank line between blocks.
    private static void WriteBlocks(INodeList nodes, StringBuilder output, string indent)
    {
        var inline = new StringBuilder();
        void FlushInline()
        {
            if (inline.ToString().Trim().Length > 0)
            {
                output.Append(indent).Append(inline.ToString().Trim()).Append("\n\n");
            }

            inline.Clear();
        }

        foreach (var node in nodes)
        {
            if (node is not IElement { LocalName: "p" or "h2" or "h3" or "h4" or "ul" or "ol" or "blockquote" or "pre" } block)
            {
                inline.Append(Inline([node]));
                continue;
            }

            FlushInline();
            switch (block.LocalName)
            {
                case "p":
                    output.Append(indent).Append(Inline(block.ChildNodes).Trim()).Append("\n\n");
                    break;
                case "h2" or "h3" or "h4":
                    output.Append(indent).Append('#', block.LocalName[1] - '0').Append(' ').Append(Inline(block.ChildNodes).Trim()).Append("\n\n");
                    break;
                case "ul" or "ol":
                    WriteList(block, output, indent);
                    output.Append('\n');
                    break;
                case "blockquote":
                    var quote = new StringBuilder();
                    WriteBlocks(block.ChildNodes, quote, "");
                    foreach (var line in quote.ToString().Trim().Split('\n'))
                    {
                        output.Append(indent).Append(line.Length == 0 ? ">" : "> " + line).Append('\n');
                    }

                    output.Append('\n');
                    break;
                case "pre":
                    output.Append(indent).Append("```\n").Append(block.TextContent.TrimEnd('\n')).Append("\n```\n\n");
                    break;
            }
        }

        FlushInline();
    }

    private static void WriteList(IElement list, StringBuilder output, string indent)
    {
        var number = 1;
        foreach (var item in list.Children.Where(c => c.LocalName == "li"))
        {
            var marker = list.LocalName == "ol" ? $"{number++}. " : "- ";
            var text = Inline(item.ChildNodes.Where(n => n is not IElement { LocalName: "ul" or "ol" }));
            output.Append(indent).Append(marker).Append(text.Trim()).Append('\n');
            foreach (var nested in item.Children.Where(c => c.LocalName is "ul" or "ol"))
            {
                WriteList(nested, output, indent + new string(' ', marker.Length));
            }
        }
    }

    private static string Inline(IEnumerable<INode> nodes)
    {
        var output = new StringBuilder();
        foreach (var node in nodes)
        {
            switch (node)
            {
                case IText text:
                    output.Append(Escape(text.Data.Replace('\n', ' ')));
                    break;
                case IElement { LocalName: "br" }:
                    // A backslash before the line end is a hard line break, and survives trimming.
                    output.Append("\\\n");
                    break;
                case IElement { LocalName: "strong" } e:
                    output.Append("**").Append(Inline(e.ChildNodes)).Append("**");
                    break;
                case IElement { LocalName: "em" } e:
                    output.Append('_').Append(Inline(e.ChildNodes)).Append('_');
                    break;
                case IElement { LocalName: "del" } e:
                    output.Append("~~").Append(Inline(e.ChildNodes)).Append("~~");
                    break;
                case IElement { LocalName: "code" } e:
                    output.Append('`').Append(e.TextContent).Append('`');
                    break;
                case IElement { LocalName: "a" } e:
                    var href = e.GetAttribute("href");
                    output.Append(href is null ? Inline(e.ChildNodes) : $"[{Inline(e.ChildNodes)}]({href.Replace(")", "%29", StringComparison.Ordinal)})");
                    break;
                case IElement e:
                    // u has no Markdown form; its text is kept.
                    output.Append(Inline(e.ChildNodes));
                    break;
            }
        }

        return output.ToString();
    }

    // Characters that would otherwise start Markdown formatting.
    private static string Escape(string text)
    {
        var output = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (c is '\\' or '*' or '_' or '`' or '[' or ']' or '#' or '<' or '>' or '~' or '|')
            {
                output.Append('\\');
            }

            output.Append(c);
        }

        return output.ToString();
    }
}
