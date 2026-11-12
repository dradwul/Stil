using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Stil.Blazor;
using Stil.Blazor.Components;

namespace Stil.Docs;

/// <summary>A starter of <c>dotnet new stil-app --Starter</c>, or the website (<c>dotnet new stil-site</c>).</summary>
public sealed record StarterChoice(string Id, string Title, string Text, string Pages, IReadOnlyList<string> Includes);

/// <summary>A feature of <c>dotnet new stil-app --Features</c>.</summary>
public sealed record FeatureChoice(string Id, string Title, string Text, string? Package = null);

/// <summary>A group of features, as the wizard shows them.</summary>
public sealed record FeatureGroup(string Title, IReadOnlyList<FeatureChoice> Features);

/// <summary>
/// The choices of the project wizard, and the <c>dotnet new</c> command and the link they give. It
/// knows the options of <c>templates/Stil.Templates</c>; keep the two in step.
/// </summary>
public sealed partial class ProjectSetup
{
    public static readonly IReadOnlyList<StarterChoice> Starters =
    [
        new("blank", "Blank", "A dashboard with KPI cards and a data grid, and an order form. Or, without the sample pages, one empty page.",
            "Dashboard, New order", []),
        new("admin", "Admin", "Orders in a data grid with filters, totals and CSV export, edited in dialogs, and a history of every change.",
            "Orders, New order, History", ["commands"]),
        new("dashboard", "Dashboard", "KPI cards with sparklines, a line chart, stacked bars and a donut, and products in a grid.",
            "Overview, Products", []),
        new("monitoring", "Monitoring", "Gauges, a card per server and a heat map, refreshed every five seconds, with alerts under the bell and an event log.",
            "Overview, Events", []),
        new("finance", "Finance", "Revenue against the budget with a margin line, budget used per department, notes in Markdown and transactions with totals.",
            "Overview, Transactions", []),
        new("planner", "Planner", "A week planner, a resource schedule, a kanban board and a Gantt plan.",
            "Overview, Planner, Bookings, Board, Project plan", ["calendar", "schedule", "kanban", "gantt", "commands"]),
        new("website", "Website", "A product site built from the page blocks, rendered without interactivity: a mega menu, pricing, a blog and a contact form.",
            "Home, Pricing, Blog, Article, Contact", []),
    ];

    public static readonly IReadOnlyList<FeatureGroup> FeatureGroups =
    [
        new("Data", [
            new("charts", "Charts", "Line, bar and donut charts.", "Stil.Charts"),
            new("datagrid", "Data grid", "Filters, grouping, totals and CSV export."),
        ]),
        new("Content", [
            new("markdown", "Markdown", "Notes in Markdown, with an editor and a view.", "Stil.Markdown"),
            new("richtext", "Rich text", "An editor with a toolbar that stores safe HTML.", "Stil.RichText"),
        ]),
        new("Planning", [
            new("calendar", "Week planner", "Drag to create, move and resize events."),
            new("schedule", "Resource schedule", "Rooms or vehicles as rows, the day across."),
            new("kanban", "Kanban board", "Drag and drop, with a limit per column."),
            new("gantt", "Gantt chart", "Progress, dependencies and milestones."),
        ]),
        new("People", [
            new("chat", "Chat and comments", "A chat and a comment thread."),
            new("activity", "Activity log", "A log with search and filters."),
            new("notifications", "Notifications", "A bell in the navbar, and toasts that stay under it."),
            new("commands", "Command palette", "Ctrl+K finds every page."),
        ]),
        new("Devices", [
            new("scanner", "Scanner", "Barcodes and QR codes, with the camera or a handheld scanner."),
            new("signature", "Signature", "A signature pad in a form."),
        ]),
    ];

    public static readonly IReadOnlyList<FeatureChoice> AllFeatures = [.. FeatureGroups.SelectMany(g => g.Features)];

    /// <summary>The fonts of <c>--Font</c>: the template's id and the font, or null for the system font.</summary>
    public static readonly IReadOnlyList<(string Id, string Name, StilFont? Font)> Fonts =
    [
        .. StilFonts.All.Select(f => (FontId(f.Family), f.Family, (StilFont?)f)),
        ("system", "The system font", null),
    ];

    /// <summary>The corners of <c>--Radius</c>: the template's id, a name and the three radius tokens.</summary>
    public static readonly IReadOnlyList<(string Id, string Name, string Small, string Medium, string Large)> Radii =
    [
        ("sharp", "Square", "0", "0", "0"),
        ("small", "Slightly rounded", "0.0625rem", "0.125rem", "0.25rem"),
        ("default", "Default", "0.125rem", "0.25rem", "0.5rem"),
        ("large", "Rounder", "0.25rem", "0.5rem", "1rem"),
        ("round", "Very round", "0.5rem", "1rem", "2rem"),
    ];

    public static readonly IReadOnlyList<(string Id, string Name)> Cultures =
        [("en", "English"), ("sv", "Svenska"), ("nb", "Norsk bokmål"), ("da", "Dansk"), ("fi", "Suomi")];

    public const string DefaultPrimary = "#34dbbf";
    public const string DefaultName = "MyApp";

    public string Starter { get; set; } = "blank";

    public bool SampleContent { get; set; } = true;

    public HashSet<string> Features { get; } = new(StringComparer.Ordinal);

    public string Primary { get; set; } = DefaultPrimary;

    public string Font { get; set; } = "lexend";

    public string Radius { get; set; } = "default";

    public StilThemeMode ColorMode { get; set; } = StilThemeMode.Auto;

    public string Name { get; set; } = DefaultName;

    public bool Interactive { get; set; } = true;

    public string Culture { get; set; } = "en";

    public StarterChoice StarterChoice => Starters.FirstOrDefault(s => s.Id == Starter) ?? Starters[0];

    /// <summary>True when the starter already has the feature.</summary>
    public bool Includes(string feature) => StarterChoice.Includes.Contains(feature);

    /// <summary>The website is its own template, stil-site, with the theme options only.</summary>
    public bool IsWebsite => Starter == "website";

    public bool IsValidName => NameRegex().IsMatch(Name);

    public bool IsValidPrimary => HexColor().IsMatch(Primary);

    /// <summary>The features to pass, without the ones the starter has.</summary>
    public IReadOnlyList<string> ChosenFeatures => IsWebsite ? [] : [.. AllFeatures.Select(f => f.Id).Where(id => Features.Contains(id) && !Includes(id))];

    /// <summary>The packages the app gets, besides Stil.</summary>
    public IReadOnlyList<string> Packages
    {
        get
        {
            if (IsWebsite)
            {
                return [];
            }

            var packages = AllFeatures.Where(f => f.Package is not null && Features.Contains(f.Id)).Select(f => f.Package!).ToList();
            if (Starter is "dashboard" or "monitoring" or "finance")
            {
                packages.Add("Stil.Charts");
            }

            if (Starter == "finance")
            {
                packages.Add("Stil.Markdown");
            }

            return [.. packages.Distinct().Order(StringComparer.Ordinal)];
        }
    }

    public StilFont? WebFont => Fonts.FirstOrDefault(f => f.Id == Font).Font;

    /// <summary>The theme the app gets, for the preview.</summary>
    public StilTheme Theme
    {
        get
        {
            var theme = new StilTheme
            {
                Primary = IsValidPrimary && !IsDefaultPrimary ? Primary : null,
                FontFamily = Font == "system" ? "system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif" : WebFont?.FontFamily,
            };
            var radius = Radii.First(r => r.Id == Radius);
            if (Radius != "default")
            {
                theme.Radius = radius.Medium;
                theme.Tokens["--stil-radius-sm"] = radius.Small;
                theme.Tokens["--stil-radius-lg"] = radius.Large;
            }

            return theme;
        }
    }

    private bool IsDefaultPrimary => string.Equals(Primary, DefaultPrimary, StringComparison.OrdinalIgnoreCase);

    /// <summary>The <c>dotnet new stil-app</c> (or <c>stil-site</c>) command, with every choice that is not the default.</summary>
    public string Command
    {
        get
        {
            var command = new StringBuilder(IsWebsite ? "dotnet new stil-site -n " : "dotnet new stil-app -n ").Append(IsValidName ? Name : DefaultName);
            if (IsWebsite)
            {
                AppendTheme(command);
                return command.ToString();
            }

            if (Starter != "blank")
            {
                command.Append(" --Starter ").Append(Starter);
            }
            else if (!SampleContent)
            {
                command.Append(" --IncludeSampleContent false");
            }

            var features = ChosenFeatures;
            if (features.Count > 0)
            {
                command.Append(" --Features ").Append(Features.Count == AllFeatures.Count ? "all" : string.Join(' ', features));
            }

            AppendTheme(command);

            if (!Interactive)
            {
                command.Append(" --Interactivity None");
            }

            if (Culture != "en")
            {
                command.Append(" --Culture ").Append(Culture);
            }

            return command.ToString();
        }
    }

    // The theme options, the same in both templates.
    private void AppendTheme(StringBuilder command)
    {
        if (IsValidPrimary && !IsDefaultPrimary)
        {
            command.Append(" --Primary \"").Append(Primary.ToLowerInvariant()).Append('"');
        }

        if (Font != "lexend")
        {
            command.Append(" --Font ").Append(Font);
        }

        if (Radius != "default")
        {
            command.Append(" --Radius ").Append(Radius);
        }

        if (ColorMode != StilThemeMode.Auto)
        {
            command.Append(" --ColorMode ").Append(ColorMode.ToString().ToLowerInvariant());
        }
    }

    /// <summary>The choices as query parameters, for a link that opens the wizard with them.</summary>
    public Dictionary<string, object?> ToQuery() => new()
    {
        ["starter"] = Starter == "blank" ? null : Starter,
        ["sample"] = Starter == "blank" && !SampleContent ? "false" : null,
        ["features"] = Features.Count == 0 ? null : string.Join(',', AllFeatures.Select(f => f.Id).Where(Features.Contains)),
        ["primary"] = IsValidPrimary && !IsDefaultPrimary ? Primary.TrimStart('#').ToLowerInvariant() : null,
        ["font"] = Font == "lexend" ? null : Font,
        ["radius"] = Radius == "default" ? null : Radius,
        ["mode"] = ColorMode == StilThemeMode.Auto ? null : ColorMode.ToString().ToLowerInvariant(),
        ["name"] = Name == DefaultName ? null : Name,
        ["interactivity"] = Interactive ? null : "none",
        ["culture"] = Culture == "en" ? null : Culture,
    };

    /// <summary>Reads the choices from a link made by <see cref="ToQuery"/>. Unknown values are ignored.</summary>
    public void ReadQuery(Func<string, string?> query)
    {
        if (query("starter") is { } starter && Starters.Any(s => s.Id == starter))
        {
            Starter = starter;
        }

        SampleContent = query("sample") != "false";
        foreach (var feature in (query("features") ?? "").Split(',').Where(id => AllFeatures.Any(f => f.Id == id)))
        {
            Features.Add(feature);
        }

        if (query("primary") is { } primary && HexColor().IsMatch("#" + primary))
        {
            Primary = "#" + primary.ToLowerInvariant();
        }

        if (query("font") is { } font && Fonts.Any(f => f.Id == font))
        {
            Font = font;
        }

        if (query("radius") is { } radius && Radii.Any(r => r.Id == radius))
        {
            Radius = radius;
        }

        if (Enum.TryParse<StilThemeMode>(query("mode"), ignoreCase: true, out var mode) && Enum.IsDefined(mode))
        {
            ColorMode = mode;
        }

        if (query("name") is { } name && NameRegex().IsMatch(name))
        {
            Name = name;
        }

        Interactive = query("interactivity") != "none";
        if (query("culture") is { } culture && Cultures.Any(c => c.Id == culture))
        {
            Culture = culture;
        }
    }

    // "BR Shape" is br-shape, as in the template's --Font choices.
    private static string FontId(string family) => family.Trim().ToLower(CultureInfo.InvariantCulture).Replace(' ', '-');

    // A project name: letters, digits, dots, underscores and hyphens, starting with a letter.
    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_.\-]{0,63}$")]
    private static partial Regex NameRegex();

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColor();
}
