using Stil.Blazor.Components;

namespace Stil.Docs;

/// <summary>A page in the docs navigation.</summary>
public sealed record DocsPage(string Title, string Href, string? Icon = null);

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
            new("Installation", "get-started"),
            new("Theming", "theming"),
            new("Localization", "localization"),
            new("Theme builder", "theme-builder"),
        ]),
        new("Components", StilIcons.Menu,
        [
            new("Buttons and badges", "components/buttons"),
            new("Icons", "components/icons"),
            new("Layout", "components/layout"),
            new("Cards and alerts", "components/cards"),
            new("Lists", "components/lists"),
            new("Tables", "components/tables"),
            new("Forms", "components/forms"),
            new("Pickers and uploads", "components/pickers"),
            new("More inputs", "components/inputs"),
            new("Navigation", "components/navigation"),
            new("Tabs, dropdowns and tooltips", "components/menus"),
            new("Dialogs, drawers and toasts", "components/dialogs"),
            new("Layout and media", "components/media"),
            new("Progress and spinners", "components/progress"),
        ]),
        new("Data", StilIcons.Folder,
        [
            new("Data grid", "components/data-grid"),
            new("Tree grid", "components/tree-grid"),
            new("Tree view", "components/tree-view"),
            new("Calendar and planner", "components/calendar"),
            new("Stat cards", "components/stat-cards"),
            new("Charts", "components/charts"),
        ]),
        new("Capture", StilIcons.Camera,
        [
            new("QR codes and barcodes", "components/codes"),
            new("Scanning", "components/scanning"),
        ]),
    ];

    public static IEnumerable<DocsPage> All => Sections.SelectMany(s => s.Pages);

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
