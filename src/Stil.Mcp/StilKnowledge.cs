using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Stil.Docs;

namespace Stil.Mcp;

/// <summary>A docs page: its route, title and description, and what it documents.</summary>
internal sealed record DocsPageInfo(string Route, string Title, string? Description, IReadOnlyList<string> ApiTypes, IReadOnlyList<string> Examples, string Source)
{
    public string Url => StilKnowledge.SiteUrl + Route;

    public string MarkdownUrl => StilKnowledge.SiteUrl + (Route.Length == 0 ? "index.html.md" : Route + ".md");
}

/// <summary>A Stil component: its type, package and the docs page with its API table.</summary>
internal sealed record ComponentInfo(Type Type, string Name, string Package, DocsPageInfo? Page)
{
    /// <summary>The name as written in Razor, with its type parameters: <c>StilDataGrid&lt;TItem&gt;</c>.</summary>
    public string DisplayName => Type.IsGenericTypeDefinition
        ? $"{Name}<{string.Join(", ", Type.GetGenericArguments().Select(a => a.Name))}>"
        : Name;
}

/// <summary>A docs example: its path (<c>Buttons/Variants</c>) and its Razor source.</summary>
internal sealed record ExampleInfo(string Name, string Source);

/// <summary>A CSS custom property of Stil and its default value.</summary>
internal sealed record TokenInfo(string Name, string Value);

/// <summary>
/// Everything the server knows about Stil, read once from the referenced libraries, their XML
/// documentation and the docs site's pages, examples and built CSS (embedded in this assembly).
/// </summary>
internal sealed partial class StilKnowledge
{
    public const string SiteUrl = "https://dradwul.github.io/Stil/";

    private static readonly Lazy<StilKnowledge> Shared = new(() => new StilKnowledge());

    public static StilKnowledge Instance => Shared.Value;

    /// <summary>The Stil libraries, and the NuGet package each one is in.</summary>
    public static readonly IReadOnlyList<(Assembly Assembly, string Package)> Libraries =
    [
        (typeof(Stil.Blazor.Components.StilButton).Assembly, "Stil"),
        (typeof(Stil.Charts.StilLineChart<>).Assembly, "Stil.Charts"),
        (typeof(Stil.Markdown.StilMarkdown).Assembly, "Stil.Markdown"),
        (typeof(Stil.RichText.StilRichTextEditor).Assembly, "Stil.RichText"),
    ];

    private StilKnowledge()
    {
        var assembly = typeof(StilKnowledge).Assembly;
        var resources = assembly.GetManifestResourceNames().ToDictionary(name => name.Replace('\\', '/'), name => name, StringComparer.Ordinal);
        string Read(string name)
        {
            using var stream = assembly.GetManifestResourceStream(resources[name])!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Replace("\r\n", "\n");
        }

        Examples = [.. resources.Keys
            .Where(name => name.StartsWith("Examples/", StringComparison.Ordinal))
            .Select(name => new ExampleInfo(name["Examples/".Length..^".razor".Length], Read(name).Trim()))
            .OrderBy(example => example.Name, StringComparer.Ordinal)];

        Pages = [.. resources.Keys
            .Where(name => name.StartsWith("Pages/", StringComparison.Ordinal))
            .Select(name => ReadPage(Read(name)))
            .OfType<DocsPageInfo>()
            .OrderBy(page => DocsOrder(page.Route))];

        Components = [.. Libraries
            .SelectMany(library => library.Assembly.GetExportedTypes().Select(type => (type, library.Package)))
            .Where(item => IsComponent(item.type))
            .Select(item =>
            {
                // Its docs page has its API table, or else an example that uses it.
                var name = BaseName(item.type);
                var tag = UsageOf(name);
                var page = Pages.FirstOrDefault(p => p.ApiTypes.Contains(name))
                    ?? Pages.FirstOrDefault(p => p.Examples.Any(e => tag.IsMatch(Examples.First(x => x.Name == e).Source)));
                return new ComponentInfo(item.type, name, item.Package, page);
            })
            .OrderBy(component => component.Name, StringComparer.Ordinal)];

        Types = [.. Libraries
            .SelectMany(library => library.Assembly.GetExportedTypes())
            .Where(type => !IsComponent(type) && type.Name.StartsWith("Stil", StringComparison.Ordinal) || type.Name.StartsWith("IStil", StringComparison.Ordinal))
            .OrderBy(type => type.Name, StringComparer.Ordinal)];

        var css = new[] { ("stil.css", Read("Css/stil.css")), ("stil-blocks.css", Read("Css/stil-blocks.css")) };
        CssClasses = [.. css
            .SelectMany(file => ClassName().Matches(StripComments(file.Item2)).Select(match => (Name: match.Groups[1].Value.Replace("\\", ""), File: file.Item1)))
            .DistinctBy(item => item.Name)
            .OrderBy(item => item.Name, StringComparer.Ordinal)];
        Tokens = [.. css
            .SelectMany(file => TokenDeclaration().Matches(StripComments(file.Item2)))
            .Select(match => new TokenInfo(match.Groups[1].Value, Whitespace().Replace(match.Groups[2].Value, " ").Trim()))
            .DistinctBy(token => token.Name)
            .OrderBy(token => token.Name, StringComparer.Ordinal)];
    }

    public IReadOnlyList<ComponentInfo> Components { get; }

    /// <summary>The other public types: enums, options, services and models.</summary>
    public IReadOnlyList<Type> Types { get; }

    public IReadOnlyList<DocsPageInfo> Pages { get; }

    public IReadOnlyList<ExampleInfo> Examples { get; }

    public IReadOnlyList<(string Name, string File)> CssClasses { get; }

    public IReadOnlyList<TokenInfo> Tokens { get; }

    /// <summary>A component by name: <c>StilButton</c>, <c>stilbutton</c>, <c>StilDataGrid&lt;TItem&gt;</c> or <c>Button</c>.</summary>
    public ComponentInfo? FindComponent(string name)
    {
        var key = Normalize(name);
        return Components.FirstOrDefault(c => string.Equals(c.Name, key, StringComparison.OrdinalIgnoreCase))
            ?? Components.FirstOrDefault(c => string.Equals(c.Name, "Stil" + key, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Another public type by name, such as <c>StilVariant</c> or <c>IStilDialogService</c>.</summary>
    public Type? FindType(string name)
    {
        var key = Normalize(name);
        return Types.FirstOrDefault(t => string.Equals(BaseName(t), key, StringComparison.OrdinalIgnoreCase))
            ?? Types.FirstOrDefault(t => string.Equals(BaseName(t), "Stil" + key, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The examples that use a component, the ones on its own docs page first, the shortest first.</summary>
    public IReadOnlyList<ExampleInfo> ExamplesOf(ComponentInfo component)
    {
        var tag = UsageOf(component.Name);
        var onPage = component.Page?.Examples ?? [];
        return [.. Examples
            .Where(example => tag.IsMatch(example.Source))
            .OrderBy(example => onPage.Contains(example.Name) ? 0 : 1)
            .ThenBy(example => example.Source.Length)];
    }

    /// <summary>The parameters of a component, as the docs' API tables list them.</summary>
    public static IReadOnlyList<ApiParameter> ParametersOf(ComponentInfo component) => ApiDocs.Parameters(component.Type);

    public static string BaseName(Type type) => type.Name.Split('`')[0];

    // A component's tag in Razor: <StilButton>, <StilButton /> or <StilButton Variant=…>.
    private static Regex UsageOf(string component) => new($@"<{Regex.Escape(component)}[\s/>]");

    private static string Normalize(string name)
    {
        var key = name.Trim().TrimStart('<').Split('<', '>', '`')[0].Trim();
        var dot = key.LastIndexOf('.');
        return dot >= 0 ? key[(dot + 1)..] : key;
    }

    private static bool IsComponent(Type type) =>
        type is { IsAbstract: false, IsInterface: false }
        && typeof(IComponent).IsAssignableFrom(type)
        && type.Name.StartsWith("Stil", StringComparison.Ordinal);

    // A docs page's route, its header's title and description, its API tables and its examples.
    private DocsPageInfo? ReadPage(string source)
    {
        var route = PageDirective().Match(source);
        var header = DocsHeader().Match(source);
        if (!route.Success || !header.Success)
        {
            return null;
        }

        var usings = ExamplesUsing().Matches(source).Select(m => m.Groups[1].Value).ToList();
        var examples = DocExample().Matches(source)
            .Select(m => ResolveExample(m.Groups[1].Value, usings))
            .OfType<string>()
            .ToList();
        var apiTypes = ApiTable().Matches(source).Select(m => m.Groups[1].Value).Distinct().ToList();
        return new DocsPageInfo(
            route.Groups[1].Value.Trim('/'),
            Attribute(header.Value, "Title") ?? route.Groups[1].Value,
            Attribute(header.Value, "Description"),
            apiTypes,
            examples,
            source);
    }

    // typeof(Variants) with @using Stil.Docs.Examples.Buttons, or typeof(Examples.Theming.Modes).
    private string? ResolveExample(string typeName, IReadOnlyList<string> usings)
    {
        if (typeName.StartsWith("Examples.", StringComparison.Ordinal))
        {
            var path = typeName["Examples.".Length..].Replace('.', '/');
            return Examples.Any(e => e.Name == path) ? path : null;
        }

        return usings.Select(folder => $"{folder.Replace('.', '/')}/{typeName}").FirstOrDefault(path => Examples.Any(e => e.Name == path))
            ?? Examples.FirstOrDefault(e => e.Name.EndsWith("/" + typeName, StringComparison.Ordinal))?.Name;
    }

    private static string? Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $@"\s{name}=""([^""]*)""");
        // @@ is how Razor writes a plain @ in an attribute.
        return match.Success ? System.Net.WebUtility.HtmlDecode(match.Groups[1].Value).Replace("@@", "@") : null;
    }

    // The order of the docs site's sidebar; pages outside it come last.
    private static int DocsOrder(string route)
    {
        var index = DocsPages.All.Select(page => page.Href).ToList().IndexOf(route);
        return index < 0 ? int.MaxValue : index;
    }

    private static string StripComments(string css) => CssComment().Replace(css, "");

    [GeneratedRegex(@"^@page\s+""([^""]*)""", RegexOptions.Multiline)]
    private static partial Regex PageDirective();

    [GeneratedRegex(@"<DocsHeader\b[^>]*>", RegexOptions.Singleline)]
    private static partial Regex DocsHeader();

    [GeneratedRegex(@"^@using\s+Stil\.Docs\.Examples\.([\w.]+)", RegexOptions.Multiline)]
    private static partial Regex ExamplesUsing();

    [GeneratedRegex(@"<DocExample\b[^>]*\bType=""typeof\(([\w.]+)\)""", RegexOptions.Singleline)]
    private static partial Regex DocExample();

    [GeneratedRegex(@"<ApiTable\b[^>]*\bType=""typeof\((\w+)")]
    private static partial Regex ApiTable();

    [GeneratedRegex(@"\.(stil-(?:[\w-]|\\.)+)")]
    private static partial Regex ClassName();

    [GeneratedRegex(@"(--stil-[\w-]+)\s*:\s*([^;{}]+)")]
    private static partial Regex TokenDeclaration();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
