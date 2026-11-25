using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using ModelContextProtocol.Server;
using Stil.Blazor.Components;
using Stil.Docs;

namespace Stil.Mcp;

/// <summary>The tools an assistant calls. Each answers in Markdown, which assistants read best.</summary>
[McpServerToolType]
internal static partial class StilTools
{
    /// <summary>Sent to the assistant when it connects.</summary>
    public const string Instructions = """
        Stil is a themeable CSS library (stil-css) and a Blazor component library (Stil, Stil.Charts, Stil.Markdown,
        Stil.RichText) built on it. When you write UI code in a project that uses Stil:
        - Use Stil components (StilButton, StilTextBox, StilDataGrid<TItem> and so on) and stil-* CSS classes. Do not
          use Bootstrap classes, another component library or inline styles for what Stil has.
        - Look a component up with get_component before you use it: use only the parameters it lists, and the enum
          values it shows (colors are StilVariant: Primary, Secondary, Success, Danger, Warning, Info).
        - Find a CSS class with list_css_classes and a token with list_tokens; theme with --stil-* tokens.
        - Copy the patterns of the examples (get_example): they are the docs site's own, and they compile.
        - For a new app, new_project gives the dotnet new command; add_to_existing_app shows the setup of an
          existing Blazor app.
        """;

    [McpServerTool(Name = "list_components", ReadOnly = true, Idempotent = true)]
    [Description("Lists every Stil Blazor component, grouped by the docs page that documents it, with the page's summary. Call it to find the component for a task; then call get_component for its parameters.")]
    public static string ListComponents(
        [Description("Only the components of this package: Stil, Stil.Charts, Stil.Markdown or Stil.RichText. Leave empty for all.")] string? package = null)
    {
        var knowledge = StilKnowledge.Instance;
        var components = knowledge.Components
            .Where(c => string.IsNullOrWhiteSpace(package) || string.Equals(c.Package, package.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (components.Count == 0)
        {
            return $"No package is called {package}. The packages are {string.Join(", ", StilKnowledge.Libraries.Select(l => l.Package))}.";
        }

        var text = new StringBuilder($"# Stil components ({components.Count})\n\n");
        foreach (var group in components.GroupBy(c => c.Page).OrderBy(g => g.Key is null ? 1 : 0).ThenBy(g => knowledge.Pages.ToList().IndexOf(g.Key!)))
        {
            text.Append("## ").Append(group.Key?.Title ?? "Other components").Append('\n');
            if (group.Key?.Description is { } description)
            {
                text.Append(description).Append('\n');
            }

            text.Append(string.Join(", ", group.Select(c => $"`{c.DisplayName}`"))).Append("\n\n");
        }

        return text.ToString().TrimEnd() + "\n";
    }

    [McpServerTool(Name = "get_component", ReadOnly = true, Idempotent = true)]
    [Description("Gets a Stil component: its package and namespace, its parameters (type, default, whether required, description), the values of the enums it takes, its docs page and examples of its use. Call it before using a component, and use only the parameters it lists.")]
    public static string GetComponent(
        [Description("The component's name, such as StilButton, StilDataGrid or StilTextBox. The Stil prefix may be left out.")] string name,
        [Description("How many examples to include (0 to 5).")] int examples = 2)
    {
        var knowledge = StilKnowledge.Instance;
        var component = knowledge.FindComponent(name);
        if (component is null)
        {
            return knowledge.FindType(name) is { } type
                ? GetType(name)
                : $"No Stil component is called {name}.{Suggest(name)} list_components lists them all.";
        }

        var text = new StringBuilder($"# {component.DisplayName}\n\n");
        text.Append("- Package: `").Append(component.Package).Append("`\n");
        text.Append("- Namespace: `").Append(component.Type.Namespace).Append("` (`@using ").Append(component.Type.Namespace).Append("`)\n");
        if (component.Page is { } page)
        {
            text.Append("- Docs: [").Append(page.Title).Append("](").Append(page.Url).Append("), as Markdown: ").Append(page.MarkdownUrl).Append('\n');
            if (page.Description is not null)
            {
                text.Append("- About: ").Append(page.Description).Append('\n');
            }
        }

        if (component.Type.IsGenericTypeDefinition)
        {
            text.Append("- Type parameters: ").Append(string.Join(", ", component.Type.GetGenericArguments().Select(a => $"`{a.Name}`")))
                .Append(" (Razor usually infers them from a bound value or Items; set them as attributes, such as `TItem=\"Order\"`, when it cannot)\n");
        }

        text.Append("\n## Parameters\n\n");
        var parameters = StilKnowledge.ParametersOf(component);
        if (parameters.Count == 0)
        {
            text.Append("None.\n");
        }
        else
        {
            text.Append("| Parameter | Type | Default | Description |\n| --- | --- | --- | --- |\n");
            foreach (var parameter in parameters)
            {
                text.Append("| `").Append(parameter.Name).Append('`').Append(parameter.Required ? " (required)" : "")
                    .Append(" | `").Append(parameter.Type).Append("` | ")
                    .Append(parameter.Default is null ? "" : $"`{parameter.Default}`").Append(" | ")
                    .Append(Cell(parameter.Description)).Append(" |\n");
            }
        }

        text.Append("\nOther attributes, such as `class`, `id`, `style` and `aria-*`, go on the root element.\n");

        var enums = EnumsOf(component.Type).ToList();
        if (enums.Count > 0)
        {
            text.Append("\n## Enum values\n\n");
            foreach (var type in enums)
            {
                text.Append("- `").Append(type.Name).Append("`: ").Append(string.Join(", ", Enum.GetNames(type))).Append('\n');
            }
        }

        var found = knowledge.ExamplesOf(component);
        var shown = found.Take(Math.Clamp(examples, 0, 5)).ToList();
        if (shown.Count > 0)
        {
            text.Append("\n## Examples\n");
            foreach (var example in shown)
            {
                text.Append("\n### ").Append(example.Name).Append("\n\n```razor\n").Append(example.Source).Append("\n```\n");
            }
        }

        if (found.Count > shown.Count)
        {
            text.Append("\nMore examples with get_example: ").Append(string.Join(", ", found.Skip(shown.Count).Select(e => e.Name))).Append('\n');
        }

        return text.ToString();
    }

    [McpServerTool(Name = "get_type", ReadOnly = true, Idempotent = true)]
    [Description("Gets a Stil type that is not a component: an enum's values (StilVariant, StilSize), or the public members of an option, model or service (StilTheme, StilOptions, IStilDialogService, IStilToastService, StilPageJson).")]
    public static string GetType(
        [Description("The type's name, such as StilVariant or IStilDialogService.")] string name)
    {
        var knowledge = StilKnowledge.Instance;
        var type = knowledge.FindType(name);
        if (type is null)
        {
            return knowledge.FindComponent(name) is not null
                ? GetComponent(name)
                : $"No Stil type is called {name}.{Suggest(name)}";
        }

        var text = new StringBuilder($"# {Friendly(type)}\n\n");
        text.Append("- Kind: ").Append(type.IsEnum ? "enum" : type.IsInterface ? "interface" : type.IsValueType ? "struct" : type.IsAbstract && type.IsSealed ? "static class" : "class").Append('\n');
        text.Append("- Namespace: `").Append(type.Namespace).Append("`\n");
        text.Append("- Package: `").Append(StilKnowledge.Libraries.First(l => l.Assembly == type.Assembly).Package).Append("`\n");
        if (ApiDocs.TypeSummary(type) is { } summary)
        {
            text.Append("\n").Append(summary).Append('\n');
        }

        if (type.IsEnum)
        {
            text.Append("\n## Values\n\n");
            foreach (var value in Enum.GetNames(type))
            {
                text.Append("- `").Append(value).Append('`');
                if (ApiDocs.MemberSummary($"F:{type.FullName}.{value}") is { } doc)
                {
                    text.Append(": ").Append(doc);
                }

                text.Append('\n');
            }

            return text.ToString();
        }

        const BindingFlags Public = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        var properties = type.GetProperties(Public).ToList();
        if (properties.Count > 0)
        {
            text.Append("\n## Properties\n\n");
            foreach (var property in properties)
            {
                text.Append("- `").Append(ApiDocs.FriendlyName(property.PropertyType, property)).Append(' ').Append(property.Name).Append('`');
                if (ApiDocs.MemberSummary($"P:{DocId(type)}.{property.Name}") is { } doc)
                {
                    text.Append(": ").Append(doc);
                }

                text.Append('\n');
            }
        }

        var methods = type.GetMethods(Public).Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object)
            && m.Name is not ("Equals" or "GetHashCode" or "ToString" or "Deconstruct" or "<Clone>$" or "PrintMembers")).ToList();
        if (methods.Count > 0)
        {
            text.Append("\n## Methods\n\n");
            foreach (var method in methods)
            {
                var signature = $"{(method.IsStatic ? "static " : "")}{ApiDocs.FriendlyName(method.ReturnType)} {method.Name}({string.Join(", ", method.GetParameters().Select(p => $"{ApiDocs.FriendlyName(p.ParameterType)} {p.Name}{(p.HasDefaultValue ? $" = {DefaultOf(p)}" : "")}"))})";
                text.Append("- `").Append(signature).Append('`');
                if (ApiDocs.MemberSummary(MethodId(method)) is { } doc)
                {
                    text.Append(": ").Append(doc);
                }

                text.Append('\n');
            }
        }

        return text.ToString();
    }

    [McpServerTool(Name = "search", ReadOnly = true, Idempotent = true)]
    [Description("Searches Stil for a word or two: components, other types, docs pages, CSS classes and tokens whose names or descriptions match. Use it when you do not know what Stil calls something (\"date range\", \"toast\", \"sticky\", \"spacing\").")]
    public static string Search(
        [Description("One or more words, such as \"file upload\" or \"margin\".")] string query)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => w.ToLowerInvariant()).ToList();
        if (words.Count == 0)
        {
            return "Give a word to search for.";
        }

        bool Matches(params string?[] texts)
        {
            var all = string.Join(' ', texts).ToLowerInvariant();
            var compact = all.Replace("-", "").Replace(" ", "");
            return words.All(w => all.Contains(w) || compact.Contains(w.Replace("-", "")));
        }

        var knowledge = StilKnowledge.Instance;
        // Components whose name matches; only when none does, the components of the matching pages.
        var named = knowledge.Components.Where(c => Matches(c.Name)).ToList();
        var pages = knowledge.Pages
            .Where(p => Matches(p.Title, p.Description) || named.Any(c => c.Page == p))
            .ToList();
        var components = named.Count > 0 ? named : [.. knowledge.Components.Where(c => c.Page is not null && pages.Contains(c.Page))];
        var types = knowledge.Types.Where(t => Matches(t.Name, ApiDocs.TypeSummary(t))).Take(15).ToList();
        var classes = knowledge.CssClasses.Where(c => Matches(c.Name)).ToList();
        var tokens = knowledge.Tokens.Where(t => Matches(t.Name)).ToList();

        var text = new StringBuilder($"# Results for \"{query}\"\n");
        if (pages.Count > 0)
        {
            text.Append("\n## Docs pages\n\n");
            foreach (var page in pages)
            {
                text.Append("- [").Append(page.Title).Append("](").Append(page.MarkdownUrl).Append(')').Append(page.Description is null ? "" : $": {page.Description}").Append('\n');
            }
        }

        if (components.Count > 0)
        {
            text.Append("\n## Components\n\n").Append(string.Join(", ", components.Select(c => $"`{c.DisplayName}`"))).Append('\n');
        }

        if (types.Count > 0)
        {
            text.Append("\n## Other types\n\n").Append(string.Join(", ", types.Select(t => $"`{Friendly(t)}`"))).Append('\n');
        }

        if (classes.Count > 0)
        {
            text.Append("\n## CSS classes\n\n").Append(string.Join(", ", classes.Take(60).Select(c => $"`{c.Name}`")));
            text.Append(classes.Count > 60 ? $", and {classes.Count - 60} more (list_css_classes with a prefix)\n" : "\n");
        }

        if (tokens.Count > 0)
        {
            text.Append("\n## Tokens\n\n");
            foreach (var token in tokens.Take(40))
            {
                text.Append("- `").Append(token.Name).Append("`: `").Append(token.Value).Append("`\n");
            }
        }

        return pages.Count + components.Count + types.Count + classes.Count + tokens.Count == 0
            ? $"Nothing in Stil matches \"{query}\". Try another word, or list_components."
            : text.ToString();
    }

    [McpServerTool(Name = "list_css_classes", ReadOnly = true, Idempotent = true)]
    [Description("Lists Stil CSS classes. Without a prefix it gives the families (stil-btn, stil-mt, stil-col …) and how many classes each has; with a prefix, every class that starts with it. Use only classes this lists.")]
    public static string ListCssClasses(
        [Description("The start of the class names, such as stil-btn, stil-mt- or stil-col-md. Leave empty for the families.")] string? prefix = null)
    {
        var classes = StilKnowledge.Instance.CssClasses;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            var families = classes.GroupBy(c => Family(c.Name)).OrderBy(g => g.Key, StringComparer.Ordinal);
            var text = new StringBuilder($"# Stil CSS classes ({classes.Count})\n\nEvery class starts with `stil-`. ")
                .Append("Classes from `stil-blocks.css` (the website blocks, an extra stylesheet) are marked. ")
                .Append("Call again with a prefix for the names.\n\n");
            foreach (var family in families)
            {
                text.Append("- `").Append(family.Key).Append("`: ").Append(family.Count())
                    .Append(family.All(c => c.File == "stil-blocks.css") ? " (stil-blocks.css)" : "")
                    .Append(" (").Append(string.Join(", ", family.Take(4).Select(c => c.Name))).Append(family.Count() > 4 ? ", …" : "").Append(")\n");
            }

            return text.ToString();
        }

        var start = prefix.Trim().TrimStart('.');
        if (!start.StartsWith("stil-", StringComparison.Ordinal))
        {
            start = "stil-" + start;
        }

        var matching = classes.Where(c => c.Name.StartsWith(start, StringComparison.Ordinal)).ToList();
        return matching.Count == 0
            ? $"No Stil class starts with {start}. Call list_css_classes without a prefix for the families."
            : $"# Classes that start with {start} ({matching.Count})\n\n" + string.Join("\n", matching.Select(c => $"- `{c.Name}`{(c.File == "stil-blocks.css" ? " (stil-blocks.css)" : "")}")) + "\n";
    }

    [McpServerTool(Name = "list_tokens", ReadOnly = true, Idempotent = true)]
    [Description("Lists Stil's design tokens: the --stil-* CSS custom properties and their default values. A theme overrides a few of them (in CSS, or with StilTheme in C#), and every component follows.")]
    public static string ListTokens(
        [Description("The start of the token names, such as --stil-color, --stil-space or --stil-radius. Leave empty for all.")] string? prefix = null)
    {
        var start = string.IsNullOrWhiteSpace(prefix) ? "--stil-" : prefix.Trim();
        if (!start.StartsWith("--", StringComparison.Ordinal))
        {
            start = "--" + (start.StartsWith("stil-", StringComparison.Ordinal) ? start : "stil-" + start);
        }

        var tokens = StilKnowledge.Instance.Tokens.Where(t => t.Name.StartsWith(start, StringComparison.Ordinal)).ToList();
        return tokens.Count == 0
            ? $"No token starts with {start}. Call list_tokens without a prefix for all of them."
            : $"# Tokens that start with {start} ({tokens.Count})\n\n" + string.Join("\n", tokens.Select(t => $"- `{t.Name}`: `{t.Value}`")) + "\n";
    }

    [McpServerTool(Name = "get_example", ReadOnly = true, Idempotent = true)]
    [Description("Gets the Razor source of a docs example, by its name (Buttons/Variants), or lists the examples of a component (StilDataGrid) or a folder (DataGrid). Without a name it lists every example.")]
    public static string GetExample(
        [Description("An example's name such as Buttons/Variants, a component such as StilDataGrid, or a folder such as Forms. Leave empty for the list.")] string? name = null)
    {
        var knowledge = StilKnowledge.Instance;
        if (string.IsNullOrWhiteSpace(name))
        {
            return $"# Examples ({knowledge.Examples.Count})\n\n" + string.Join("\n", knowledge.Examples
                .GroupBy(e => e.Name.Split('/')[0])
                .Select(g => $"- {g.Key}: {string.Join(", ", g.Select(e => e.Name[(g.Key.Length + 1)..]))}")) + "\n";
        }

        var key = name.Trim().Replace('\\', '/').Replace('.', '/');
        if (knowledge.Examples.FirstOrDefault(e => string.Equals(e.Name, key, StringComparison.OrdinalIgnoreCase)) is { } example)
        {
            return $"# {example.Name}\n\n```razor\n{example.Source}\n```\n";
        }

        var list = knowledge.FindComponent(name) is { } component
            ? knowledge.ExamplesOf(component)
            : knowledge.Examples.Where(e => e.Name.StartsWith(key + "/", StringComparison.OrdinalIgnoreCase)).ToList();
        return list.Count == 0
            ? $"No example is called {name}, and no component or folder has examples by that name. get_example without a name lists them."
            : $"# Examples for {name} ({list.Count})\n\n" + string.Join("\n", list.Select(e => $"- {e.Name}")) + "\n\nCall get_example with a name for its source.\n";
    }

    [McpServerTool(Name = "new_project", ReadOnly = true, Idempotent = true)]
    [Description("Gives the dotnet new command for a new Stil app (stil-app) or website (stil-site), the same as the project wizard on the docs site. It does not run it. Without arguments it also lists the starters, features, fonts and other options.")]
    public static string NewProject(
        [Description("The project's name, a C# identifier such as MyApp.")] string name = ProjectSetup.DefaultName,
        [Description("The starter: blank, admin, dashboard, monitoring, finance, planner, or website (a site from page blocks, dotnet new stil-site).")] string starter = "blank",
        [Description("Features to add, separated by spaces or commas, such as \"charts datagrid kanban\". Ignored for a website.")] string? features = null,
        [Description("The primary brand color as #rrggbb.")] string? primary = null,
        [Description("The font id, such as geologica (the default), lexend or system.")] string? font = null,
        [Description("The corners: sharp, small, default, large or round.")] string? radius = null,
        [Description("The color mode: auto (the default), light or dark.")] string? colorMode = null,
        [Description("False for an app with no interactivity (static server rendering).")] bool interactive = true,
        [Description("The app's language: en, sv, nb, da or fi.")] string culture = "en",
        [Description("Without sample pages (the blank starter only).")] bool empty = false)
    {
        var problems = new List<string>();
        var setup = new ProjectSetup { Name = name.Trim(), Interactive = interactive, SampleContent = !empty };
        if (!setup.IsValidName)
        {
            problems.Add($"\"{name}\" is not a valid project name, so the command uses {ProjectSetup.DefaultName}.");
        }

        Choose(starter, ProjectSetup.Starters.Select(s => s.Id), "starter", value => setup.Starter = value);
        foreach (var feature in (features ?? "").Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.Equals(feature, "all", StringComparison.OrdinalIgnoreCase))
            {
                setup.Features.UnionWith(ProjectSetup.AllFeatures.Select(f => f.Id));
            }
            else
            {
                Choose(feature, ProjectSetup.AllFeatures.Select(f => f.Id), "feature", value => setup.Features.Add(value));
            }
        }

        if (primary is not null)
        {
            setup.Primary = primary.Trim();
            if (!setup.IsValidPrimary)
            {
                problems.Add($"\"{primary}\" is not a #rrggbb color, so the default is kept.");
            }
        }

        Choose(font, ProjectSetup.Fonts.Select(f => f.Id), "font", value => setup.Font = value);
        Choose(radius, ProjectSetup.Radii.Select(r => r.Id), "radius", value => setup.Radius = value);
        Choose(colorMode, ["auto", "light", "dark"], "color mode", value => setup.ColorMode = Enum.Parse<StilThemeMode>(value, ignoreCase: true));
        Choose(culture, ProjectSetup.Cultures.Select(c => c.Id), "culture", value => setup.Culture = value);

        var text = new StringBuilder("# A new Stil project\n\n```shell\ndotnet new install Stil.Templates\n")
            .Append(setup.Command).Append("\n```\n\n")
            .Append("Run it where the project folder should be created, then `dotnet run` in it. ");
        text.Append(setup.IsWebsite
            ? "The website's pages are JSON in a SQLite database (seeded from Data/site.json), rendered from page blocks.\n"
            : $"Starter: {setup.StarterChoice.Title}, pages: {setup.StarterChoice.Pages}.{(setup.Packages.Count > 0 ? $" It also references {string.Join(", ", setup.Packages)}." : "")}\n");
        if (problems.Count > 0)
        {
            text.Append("\n## Not used\n\n").Append(string.Join("\n", problems.Select(p => $"- {p}"))).Append('\n');
        }

        if (string.IsNullOrWhiteSpace(features) && starter == "blank" && primary is null && font is null && radius is null)
        {
            text.Append("\n## Options\n\n### Starters\n\n");
            foreach (var choice in ProjectSetup.Starters)
            {
                text.Append("- `").Append(choice.Id).Append("`: ").Append(choice.Text).Append(" Pages: ").Append(choice.Pages).Append('\n');
            }

            text.Append("\n### Features\n\n");
            foreach (var feature in ProjectSetup.AllFeatures)
            {
                text.Append("- `").Append(feature.Id).Append("`: ").Append(feature.Text).Append(feature.Package is null ? "" : $" ({feature.Package})").Append('\n');
            }

            text.Append("\n### Fonts\n\n").Append(string.Join(", ", ProjectSetup.Fonts.Select(f => $"`{f.Id}`"))).Append('\n');
            text.Append("\n### Corners\n\n").Append(string.Join(", ", ProjectSetup.Radii.Select(r => $"`{r.Id}` ({r.Name})"))).Append('\n');
        }

        return text.ToString();

        void Choose(string? value, IEnumerable<string> allowed, string what, Action<string> apply)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var match = allowed.FirstOrDefault(id => string.Equals(id, value.Trim(), StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                problems.Add($"There is no {what} \"{value}\". The {what}s are {string.Join(", ", allowed)}.");
            }
            else
            {
                apply(match);
            }
        }
    }

    [McpServerTool(Name = "add_to_existing_app", ReadOnly = true, Idempotent = true)]
    [Description("Shows how to add Stil to an existing Blazor app: the package, Program.cs, App.razor and the layout, as on the docs site's Installation page. It changes nothing itself.")]
    public static string AddToExistingApp()
    {
        var page = StilKnowledge.Instance.Pages.First(p => p.Route == "get-started");
        var source = page.Source;
        string Snippet(string name) => Dedent(Regex.Match(source, $"{name}Snippet = \"\"\"\\n(.*?)\\n\\s*\"\"\";", RegexOptions.Singleline).Groups[1].Value);

        return $"""
            # Add Stil to an existing Blazor app

            1. Add the package:

               ```shell
               dotnet add package Stil
               ```

            2. Register the services in `Program.cs` (the theme is optional):

            ```csharp
            {Snippet("Program")}
            ```

            3. Add the stylesheet and the global theme in `App.razor`, and put `stil-scope` on the body so the base
               styles apply:

            ```razor
            {Snippet("App")}
            ```

            4. Add the hosts for dialogs and toasts to the layout, and use the components:

            ```razor
            {Snippet("Layout")}
            ```

            5. In `_Imports.razor`:

            ```razor
            @using Stil.Blazor
            @using Stil.Blazor.Components
            ```

            Charts, Markdown and rich text are the packages `Stil.Charts`, `Stil.Markdown` and `Stil.RichText`, with
            `@using Stil.Charts`, `@using Stil.Markdown` and `@using Stil.RichText`. Interactive components (dialogs,
            toasts, the data grid) need an interactive render mode. The whole page: {page.MarkdownUrl}
            """;
    }

    private static IEnumerable<Type> EnumsOf(Type component)
    {
        var instance = component.IsGenericTypeDefinition ? null : component;
        return (instance ?? component).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ParameterAttribute>() is not null)
            .Select(p => Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType)
            .Where(t => t.IsEnum)
            .Distinct()
            .OrderBy(t => t.Name, StringComparer.Ordinal);
    }

    private static string Family(string className)
    {
        // stil-btn-primary -> stil-btn, stil-mt-4 -> stil-mt, stil-col-md-6 -> stil-col.
        var parts = className.Split('-');
        return parts.Length > 2 ? $"{parts[0]}-{parts[1]}" : className;
    }

    private static string Suggest(string name)
    {
        var key = name.Replace("Stil", "", StringComparison.OrdinalIgnoreCase).Trim().ToLowerInvariant();
        if (key.Length < 3)
        {
            return "";
        }

        var knowledge = StilKnowledge.Instance;
        var close = knowledge.Components.Select(c => c.Name)
            .Concat(knowledge.Types.Select(StilKnowledge.BaseName))
            .Where(n => n.Contains(key, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .Take(8)
            .ToList();
        return close.Count == 0 ? "" : $" Did you mean {string.Join(", ", close)}?";
    }

    private static string Friendly(Type type) => type.IsGenericTypeDefinition
        ? $"{StilKnowledge.BaseName(type)}<{string.Join(", ", type.GetGenericArguments().Select(a => a.Name))}>"
        : type.Name;

    // A type as XML documentation ids write it: Stil.Blazor.StilTheme, System.Nullable{System.TimeSpan}.
    private static string DocId(Type type)
    {
        if (type.IsByRef)
        {
            return DocId(type.GetElementType()!) + "@";
        }

        if (type.IsArray)
        {
            return DocId(type.GetElementType()!) + "[]";
        }

        if (type.IsGenericParameter)
        {
            return (type.DeclaringMethod is null ? "`" : "``") + type.GenericParameterPosition;
        }

        var name = $"{(type.DeclaringType is { } outer ? DocId(outer) : type.Namespace)}.{type.Name.Split('`')[0]}";
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            return $"{name}{{{string.Join(",", type.GetGenericArguments().Select(DocId))}}}";
        }

        return type.IsGenericTypeDefinition ? $"{name}`{type.GetGenericArguments().Length - (type.DeclaringType?.GetGenericArguments().Length ?? 0)}" : name;
    }

    private static string MethodId(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var generic = method.IsGenericMethodDefinition ? $"``{method.GetGenericArguments().Length}" : "";
        var id = $"M:{DocId(method.DeclaringType!)}.{method.Name}{generic}";
        return parameters.Length == 0 ? id : $"{id}({string.Join(",", parameters.Select(p => DocId(p.ParameterType)))})";
    }

    // A parameter's default value as C# writes it.
    private static string DefaultOf(ParameterInfo parameter) => parameter.DefaultValue switch
    {
        null => "null",
        bool b => b ? "true" : "false",
        string s => $"\"{s}\"",
        Enum e => $"{e.GetType().Name}.{e}",
        IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
        _ => "default",
    };

    private static string Cell(string? text) => (text ?? "").Replace("|", "\\|").Replace("\n", " ");

    private static string Dedent(string text)
    {
        var lines = text.Split('\n');
        var indent = lines.Where(l => l.Trim().Length > 0).Select(l => l.Length - l.TrimStart().Length).DefaultIfEmpty(0).Min();
        return string.Join("\n", lines.Select(l => l.Length >= indent ? l[indent..] : l.TrimStart()));
    }
}
