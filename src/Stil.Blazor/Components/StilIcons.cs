using System.Text.Json;

namespace Stil.Blazor.Components;

/// <summary>
/// The names of Stil's built-in icons, for <c>&lt;StilIcon Name="…" /&gt;</c>. The icons come from
/// <c>src/Stil.Css/src/icons.json</c>, which also generates the <c>stil-icon-*</c> CSS classes.
/// </summary>
public static class StilIcons
{
    public const string ArrowLeft = "arrow-left";
    public const string ArrowRight = "arrow-right";
    public const string Bell = "bell";
    public const string Camera = "camera";
    public const string Check = "check";
    public const string ChevronDown = "chevron-down";
    public const string ChevronLeft = "chevron-left";
    public const string ChevronRight = "chevron-right";
    public const string ChevronUp = "chevron-up";
    public const string ChevronsLeft = "chevrons-left";
    public const string ChevronsRight = "chevrons-right";
    public const string Close = "close";
    public const string Danger = "danger";
    public const string Download = "download";
    public const string External = "external";
    public const string File = "file";
    public const string Filter = "filter";
    public const string Folder = "folder";
    public const string Globe = "globe";
    public const string Home = "home";
    public const string Info = "info";
    public const string Menu = "menu";
    public const string Minus = "minus";
    public const string Moon = "moon";
    public const string More = "more";
    public const string Pencil = "pencil";
    public const string Plus = "plus";
    public const string Search = "search";
    public const string Star = "star";
    public const string Success = "success";
    public const string Sun = "sun";
    public const string Upload = "upload";
    public const string Warning = "warning";

    private static readonly Lazy<IReadOnlyDictionary<string, string>> BuiltInIcons = new(Load);

    // Icons that point along the reading direction (also listed in the CSS build).
    private static readonly HashSet<string> Directional = [ArrowLeft, ArrowRight, ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight];

    /// <summary>Whether the icon points left or right, and is mirrored in right-to-left text.</summary>
    public static bool IsDirectional(string name) => Directional.Contains(name);

    /// <summary>The inner SVG markup of every built-in icon, keyed by name.</summary>
    public static IReadOnlyDictionary<string, string> BuiltIn => BuiltInIcons.Value;

    private static IReadOnlyDictionary<string, string> Load()
    {
        using var stream = typeof(StilIcons).Assembly.GetManifestResourceStream("Stil.Blazor.icons.json")
            ?? throw new InvalidOperationException("The embedded icon set (Stil.Blazor.icons.json) is missing.");
        return JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
            ?? throw new InvalidOperationException("The embedded icon set is empty.");
    }
}
