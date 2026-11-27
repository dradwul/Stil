using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Stil.Docs;

/// <summary>
/// The HTML of a docs example, for people who use Stil CSS without Blazor. It is rendered from the
/// example itself, so it cannot drift from it, then tidied: indented, and with the ids that
/// components make up numbered from 1.
/// </summary>
public static partial class ExampleHtml
{
    // Examples whose point is state, a service or a script of Stil for Blazor, so their HTML alone
    // would show something that does not work: a whole folder, or one example ("Folder/Name").
    private static readonly HashSet<string> BlazorOnly =
    [
        "Calendar", "Charts", "Chat", "Codes", "CommandPalette", "DataGrid", "Dialogs", "Gantt", "Kanban",
        "Localization", "Markdown", "Notifications", "Pickers", "RichText", "Signature", "TreeGrid", "TreeView",
        "ActivityLog/LiveLog", "Inputs/Times", "Inputs/Wizard", "Media/Avatar", "Menus/PillTabs", "Menus/Tabs", "Theming/Provider",
    ];

    private static readonly HashSet<string> Void =
        ["area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "source", "track", "wbr"];

    private static readonly HashSet<string> Inline =
    [
        "a", "abbr", "b", "bdi", "bdo", "br", "button", "cite", "code", "data", "dfn", "em", "i", "img", "input",
        "kbd", "label", "mark", "meter", "output", "progress", "q", "s", "samp", "select", "small", "span",
        "strong", "sub", "sup", "svg", "textarea", "time", "u", "var", "wbr",
    ];

    private static readonly HashSet<string> Raw = ["pre", "textarea", "script", "style"];

    private const int LineLength = 100;

    /// <summary>The example's name, as in its folder: "Buttons/Variants".</summary>
    public static string NameOf(Type example) =>
        example.FullName!["Stil.Docs.Examples.".Length..].Replace('.', '/');

    /// <summary>Whether the example works as HTML and Stil CSS alone.</summary>
    public static bool Has(Type example)
    {
        var name = NameOf(example);
        return !BlazorOnly.Contains(name) && !BlazorOnly.Contains(name[..name.IndexOf('/')]);
    }

    /// <summary>Renders the example to HTML, outside the page, and tidies it.</summary>
    public static async Task<string> RenderAsync(Type example, IServiceProvider services, ILoggerFactory loggerFactory)
    {
        await using var renderer = new HtmlRenderer(services, loggerFactory);
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync(example, ParameterView.Empty)).ToHtmlString());
        return Tidy(html);
    }

    /// <summary>Whether the HTML needs stil.js to behave: dropdowns, tabs, popovers and the like.</summary>
    public static bool NeedsScript(string html) => ScriptMarker().IsMatch(html);

    /// <summary>Numbers the made-up ids from 1 and indents the HTML.</summary>
    public static string Tidy(string html)
    {
        html = Comment().Replace(html, "");
        html = BlazorOnlyAttribute().Replace(html, "");
        // EditForm's field classes, which Stil's CSS does not use.
        html = ClassAttribute().Replace(html, m =>
        {
            var classes = m.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(name => name is not ("valid" or "invalid" or "modified"));
            return $@" class=""{string.Join(' ', classes)}""";
        });
        html = EmptyClass().Replace(html, "");
        html = EmptyData().Replace(html, " $1");
        html = RenumberIds(html);
        var nodes = Parse(html);
        var text = new StringBuilder();
        foreach (var node in nodes)
        {
            Write(node, 0, text);
        }

        return text.ToString().TrimEnd();
    }

    // StilIds makes ids such as stil-tabs-1234 (and stil-tabs-1234-tab-0 from it) that count up
    // across the page; in the HTML they are numbered per example, so an example always shows the
    // same ids.
    private static string RenumberIds(string html)
    {
        var made = IdAttribute().Matches(html)
            .SelectMany(attribute => MadeUpId().Matches(attribute.Groups[1].Value).Select(m => m.Value))
            .Distinct()
            .ToList();
        var counts = new Dictionary<string, int>();
        var names = new Dictionary<string, string>();
        foreach (var id in made)
        {
            var prefix = id[..id.LastIndexOf('-')];
            counts[prefix] = counts.GetValueOrDefault(prefix) + 1;
            names[id] = $"{prefix}-{counts[prefix]}";
        }

        return made.Count == 0
            ? html
            : Regex.Replace(html, $@"(?<![A-Za-z0-9_])({string.Join('|', made.Select(Regex.Escape))})(?!\d)", m => names[m.Value]);
    }

    private abstract record Node;

    private sealed record Text(string Value) : Node;

    private sealed record Element(string Name, string Attributes, bool SelfClosing) : Node
    {
        public List<Node> Children { get; } = [];

        // The element as written, used for <pre> and the like, whose spaces matter.
        public string Source { get; set; } = "";

        public string Open => $"<{Name}{Attributes}{(SelfClosing ? " /" : "")}>";
    }

    private static List<Node> Parse(string html)
    {
        var root = new List<Node>();
        var stack = new Stack<(Element Element, int Start)>();
        List<Node> Current() => stack.Count == 0 ? root : stack.Peek().Element.Children;

        foreach (Match token in Token().Matches(html))
        {
            if (token.Groups["text"].Success)
            {
                Current().Add(new Text(token.Value));
                continue;
            }

            var name = token.Groups["name"].Value.ToLowerInvariant();
            if (token.Groups["close"].Value == "/")
            {
                if (stack.All(open => open.Element.Name != name))
                {
                    continue;
                }

                while (stack.Count > 0)
                {
                    var (element, start) = stack.Pop();
                    element.Source = html[start..(token.Index + token.Length)];
                    if (element.Name == name)
                    {
                        break;
                    }
                }

                continue;
            }

            var attributes = Whitespace().Replace(token.Groups["attributes"].Value, " ").TrimEnd();
            var self = token.Groups["self"].Value == "/";
            var node = new Element(name, attributes, self) { Source = token.Value };
            Current().Add(node);
            if (!self && !Void.Contains(name))
            {
                stack.Push((node, token.Index));
            }
        }

        return root;
    }

    private static void Write(Node node, int indent, StringBuilder text)
    {
        var pad = new string(' ', indent);
        if (node is Text plain)
        {
            var value = Whitespace().Replace(plain.Value, " ").Trim();
            if (value.Length > 0)
            {
                text.Append(pad).Append(value).Append('\n');
            }

            return;
        }

        var element = (Element)node;
        if (Raw.Contains(element.Name))
        {
            text.Append(pad).Append(element.Source).Append('\n');
            return;
        }

        // A line that fits stays one line, and so does an inline element or a block with text in it
        // (a paragraph). A block of elements, such as a row of buttons, gets a line for each.
        var flat = Flat(element).Trim();
        if (indent + flat.Length <= LineLength || Inline.Contains(element.Name) || element.Children.Any(HasText))
        {
            text.Append(pad).Append(flat).Append('\n');
            return;
        }

        text.Append(pad).Append(element.Open).Append('\n');
        foreach (var child in element.Children)
        {
            Write(child, indent + 2, text);
        }

        text.Append(pad).Append("</").Append(element.Name).Append(">\n");
    }

    private static bool HasText(Node node) => node is Text text && !string.IsNullOrWhiteSpace(text.Value);

    private static string Flat(Node node)
    {
        if (node is Text plain)
        {
            return Whitespace().Replace(plain.Value, " ");
        }

        var element = (Element)node;
        if (Raw.Contains(element.Name))
        {
            return element.Source;
        }

        if (element.SelfClosing || Void.Contains(element.Name))
        {
            return element.Open;
        }

        var inner = string.Concat(element.Children.Select(Flat));
        // Space just inside a block, such as a div's newline before its first child, is not shown.
        if (!Inline.Contains(element.Name))
        {
            inner = inner.Trim();
        }

        return $"{element.Open}{inner}</{element.Name}>";
    }

    [GeneratedRegex(@"<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex Comment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"\s(?:id|name)=""([^""]*)""")]
    private static partial Regex IdAttribute();

    [GeneratedRegex(@"stil-[a-z]+(?:-[a-z]+)*-\d+(?!\d)")]
    private static partial Regex MadeUpId();

    [GeneratedRegex(@"\sclass=""([^""]*)""")]
    private static partial Regex ClassAttribute();

    // Blazor's own attributes (event options), the action Blazor gives a form, and a select's value.
    [GeneratedRegex(@"\s__internal_[\w-]+|(?<=<form\b[^>]*)\saction=""[^""]*""|(?<=<select\b[^>]*)\svalue=""[^""]*""")]
    private static partial Regex BlazorOnlyAttribute();

    [GeneratedRegex(@"\sclass=""\s*""")]
    private static partial Regex EmptyClass();

    [GeneratedRegex(@"\s(data-[\w-]+)=""""")]
    private static partial Regex EmptyData();

    [GeneratedRegex(@"<(?<close>/?)(?<name>[a-zA-Z][\w:-]*)(?<attributes>(?:\s+[^\s=>/""']+(?:\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>""']+))?)*)\s*(?<self>/?)>|(?<text>[^<]+)")]
    private static partial Regex Token();

    // What stil.js acts on (see the stil-css README); data-stil-tooltip and data-stil-theme are CSS only.
    [GeneratedRegex(@"data-stil-(?:dismiss|dialog|toggle|inline|placement|editable|send|mask|autoplay|culture|richtext)|stil-dropdown|stil-popover|popovertarget|stil-combobox|stil-splitter|stil-carousel|role=""(?:tab|treeitem)""|<dialog")]
    private static partial Regex ScriptMarker();
}
