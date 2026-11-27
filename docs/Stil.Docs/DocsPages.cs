using Stil.Blazor.Components;

namespace Stil.Docs;

/// <summary>Who a component page is for; a guide has none.</summary>
public enum DocsAudience
{
    /// <summary>Works with Stil CSS alone (with stil.js where it says so), and with Stil for Blazor.</summary>
    CssAndBlazor,

    /// <summary>Needs Stil for Blazor: its state or behaviour is in .NET.</summary>
    BlazorOnly,
}

/// <summary>A page in the docs navigation.</summary>
public sealed record DocsPage(string Title, string Href, string? Icon = null, DocsAudience? Audience = null);

/// <summary>A titled group of pages in the docs navigation.</summary>
public sealed record DocsSection(string Title, string Icon, IReadOnlyList<DocsPage> Pages);

/// <summary>Every docs page, in reading order. The sidebar and the previous/next links use it.</summary>
public static class DocsPages
{
    public static readonly IReadOnlyList<DocsSection> Sections =
    [
        new("Get started", StilIcons.Home,
        [
            new("Introduction", ""),
            new("Install CSS", "get-started/css"),
            new("Install for Blazor", "get-started"),
            new("New app", "new-app"),
            new("Theming", "theming"),
            new("Localization", "localization"),
            new("Theme builder", "theme-builder"),
            new("AI assistants", "ai-assistants"),
        ]),
        new("Stil CSS", StilIcons.File,
        [
            new("Utilities", "css/utilities"),
            new("stil.js", "css/javascript"),
            new("From Bootstrap", "css/from-bootstrap"),
        ]),
        new("Components", StilIcons.Menu,
        [
            new("Buttons and badges", "components/buttons", Audience: DocsAudience.CssAndBlazor),
            new("Icons", "components/icons", Audience: DocsAudience.CssAndBlazor),
            new("Layout", "components/layout", Audience: DocsAudience.CssAndBlazor),
            new("Cards and alerts", "components/cards", Audience: DocsAudience.CssAndBlazor),
            new("Lists", "components/lists", Audience: DocsAudience.CssAndBlazor),
            new("Tables", "components/tables", Audience: DocsAudience.CssAndBlazor),
            new("Forms", "components/forms", Audience: DocsAudience.CssAndBlazor),
            new("Pickers and uploads", "components/pickers", Audience: DocsAudience.BlazorOnly),
            new("More inputs", "components/inputs", Audience: DocsAudience.CssAndBlazor),
            new("Navigation", "components/navigation", Audience: DocsAudience.CssAndBlazor),
            new("Tabs, dropdowns and tooltips", "components/menus", Audience: DocsAudience.CssAndBlazor),
            new("Dialogs, drawers and toasts", "components/dialogs", Audience: DocsAudience.CssAndBlazor),
            new("Layout and media", "components/media", Audience: DocsAudience.CssAndBlazor),
            new("Progress and spinners", "components/progress", Audience: DocsAudience.CssAndBlazor),
        ]),
        new("Data", StilIcons.Folder,
        [
            new("Data grid", "components/data-grid", Audience: DocsAudience.BlazorOnly),
            new("Tree grid", "components/tree-grid", Audience: DocsAudience.BlazorOnly),
            new("Tree view", "components/tree-view", Audience: DocsAudience.BlazorOnly),
            new("Calendar and planner", "components/calendar", Audience: DocsAudience.BlazorOnly),
            new("Stat cards", "components/stat-cards", Audience: DocsAudience.CssAndBlazor),
            new("Charts", "components/charts", Audience: DocsAudience.BlazorOnly),
        ]),
        new("Capture", StilIcons.Camera,
        [
            new("QR codes and barcodes", "components/codes", Audience: DocsAudience.BlazorOnly),
            new("Scanning", "components/scanning", Audience: DocsAudience.BlazorOnly),
            new("Signature", "components/signature", Audience: DocsAudience.BlazorOnly),
            new("Activity log and timeline", "components/activity-log", Audience: DocsAudience.CssAndBlazor),
            new("Chat and comments", "components/chat", Audience: DocsAudience.BlazorOnly),
        ]),
        new("Writing", StilIcons.Pencil,
        [
            new("Markdown", "components/markdown", Audience: DocsAudience.BlazorOnly),
            new("Rich text", "components/rich-text", Audience: DocsAudience.BlazorOnly),
        ]),
        new("Website", StilIcons.Globe,
        [
            new("Sections and heroes", "blocks/sections", Audience: DocsAudience.CssAndBlazor),
            new("Pricing, testimonials and FAQ", "blocks/selling", Audience: DocsAudience.CssAndBlazor),
            new("Header, footer and contact", "blocks/frame", Audience: DocsAudience.CssAndBlazor),
            new("Blog", "blocks/blog", Audience: DocsAudience.CssAndBlazor),
            new("Pages as data", "blocks/pages-as-data", Audience: DocsAudience.BlazorOnly),
            new("Block editor", "blocks/block-editor", Audience: DocsAudience.BlazorOnly),
            new("SEO and sharing", "blocks/seo", Audience: DocsAudience.BlazorOnly),
            new("Themes and languages", "blocks/themes-and-languages", Audience: DocsAudience.BlazorOnly),
        ]),
        new("Boards", StilIcons.Menu,
        [
            new("Kanban board", "components/kanban", Audience: DocsAudience.BlazorOnly),
            new("Command palette", "components/command-palette", Audience: DocsAudience.BlazorOnly),
            new("Notification center", "components/notifications", Audience: DocsAudience.BlazorOnly),
            new("Gantt chart", "components/gantt", Audience: DocsAudience.BlazorOnly),
        ]),
    ];

    public static IEnumerable<DocsPage> All => Sections.SelectMany(s => s.Pages);

    /// <summary>The page at an address relative to the site, such as "components/buttons".</summary>
    public static DocsPage? Find(string href) => All.FirstOrDefault(p => p.Href == href);

    /// <summary>The pages before and after the given one, for the links at the end of a page.</summary>
    public static (DocsPage? Previous, DocsPage? Next) Around(string href)
    {
        var pages = All.ToList();
        var index = pages.FindIndex(p => p.Href == href);
        return index < 0
            ? (null, null)
            : (index > 0 ? pages[index - 1] : null, index < pages.Count - 1 ? pages[index + 1] : null);
    }
}
