# Changelog

All notable changes to the Stil packages are listed here: `stil-css` (npm),
`Stil`, `Stil.Charts` and `Stil.Templates` (NuGet). They share one version
number.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and versions follow [Semantic Versioning](https://semver.org/). Before `1.0`,
a minor version can contain breaking changes; they are marked **Breaking**.

## [Unreleased]

Nothing has been published.

### Added

- **Stil CSS** (`stil-css`):
  - Tokens (`--stil-*`) for colors, typography, spacing, shape, depth and
    motion, with light, dark and auto modes through `data-stil-theme`.
    The default font is Lexend, used when the page loads it.
  - Derived colors: text on colored backgrounds, links and focus rings stay
    WCAG AA readable for any brand color. CI runs 163 contrast checks.
  - Cascade layers (`stil.tokens`, `stil.reset`, `stil.base`,
    `stil.components`, `stil.utilities`) and a reset and base styles scoped to
    `.stil-scope`.
  - Layout: container, 12-column grid, stack, cluster and sidebar layout.
  - Components: buttons, forms and input groups, cards, alerts, badges,
    avatars, lists, tables, data grid, combobox and multi-select, chips, date
    range, file drop zone, stat cards, tree, accordion, nav and nav groups,
    navbar, breadcrumbs, pagination, tabs, dropdowns, dialogs, drawers,
    tooltips, progress, spinners and toasts.
  - Calendar components: events with variants or an own color, a month grid
    tinted by how busy each day is, a week time grid with free blocks and a now
    line, and an agenda. `stil.js` adds keyboard navigation in the month grid
    and dragging events in a week grid (`data-stil-editable`).
  - About 900 utility classes and about 30 built-in icons (`stil-icon-*`).
  - Right-to-left support: logical properties throughout, mirrored switches,
    drawers and tree chevrons, and arrow and chevron icons that flip under
    `dir="rtl"` (`stil-icon-directional` for icons of your own).
  - `stil.js`, an optional 16 kB module for dropdowns, dismissing, dialogs,
    toggles, calendar dragging, language links (`data-stil-culture`) and
    keyboard handling.
- **Stil for Blazor** (`Stil`):
  - About 60 components, with one for every Stil CSS component, including
    `StilDataGrid` (sorting, filtering, paging, selection, virtualization and
    server-side data), `StilAutocomplete`, `StilMultiSelect`,
    `StilDateRangePicker`, `StilFileUpload`, `StilTreeView`, `StilNavGroup`
    and `StilStatCard`.
  - `StilCalendar`, `StilWeekPlanner` and `StilAgenda`: a month, a week
    planner and an agenda from one list of items, showing how booked each day
    is and where the free time is. The planner can move, resize and create
    events by dragging, and turns into an agenda on small screens.
  - Form fields with EditForm validation, labels, hints, add-ons and
    `aria-describedby`/`aria-invalid`.
  - `IStilDialogService`, `IStilToastService`, `AddStil()`, `StilTheme`,
    `StilHead` and `StilThemeProvider`.
  - `StilIcon` with built-in icons, and `IStilIconProvider` for other icon
    sets.
  - Localization: every built-in text (screen reader labels, empty states,
    chart summaries) in English, Swedish, Norwegian (Bokmål), Danish and
    Finnish, following the UI culture. Text parameters override one
    component, `AddStil(o => o.Texts.Add(…))` rewords a text everywhere or
    adds a language, and `IStilLocalizer` reads texts from elsewhere.
  - `AddStilLocalization()`: the app's own texts from `Locales/*.json`
    through the standard `IStringLocalizer<T>`, embedded by the package so
    the files work in Server, static SSR and WebAssembly.
  - `StilLanguageSelector` and `StilCulture`: a language menu that stores
    the choice in the ASP.NET Core culture cookie and local storage, and
    helpers for WebAssembly startup and `<html dir>`. Tabs and tree views
    follow the reading direction of right-to-left cultures.
  - XML documentation for IntelliSense, Source Link and symbol packages.
- **Stil.Charts**: SVG line, area, bar (grouped, stacked or horizontal), donut
  and sparkline charts that follow the theme and dark mode and render in static
  SSR. Each chart has a generated summary and a data table for screen readers.
- **Stil.Templates**: `dotnet new stil-app`, with a language selector,
  `Locales/*.json` and a `Culture` option for the default language.
- A documentation site with live examples, API tables and a theme builder.
- A guide for migrating from Bootstrap.

[Unreleased]: https://github.com/dradwul/StilCSS/commits/main
