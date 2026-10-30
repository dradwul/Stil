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
  - `stil.js`, an optional 14 kB module for dropdowns, dismissing, dialogs,
    toggles, calendar dragging and keyboard handling.
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
  - XML documentation for IntelliSense, Source Link and symbol packages.
- **Stil.Charts**: SVG line, area, bar (grouped, stacked or horizontal), donut
  and sparkline charts that follow the theme and dark mode and render in static
  SSR. Each chart has a generated summary and a data table for screen readers.
- **Stil.Templates**: `dotnet new stil-app`.
- A documentation site with live examples, API tables and a theme builder.
- A guide for migrating from Bootstrap.

[Unreleased]: https://github.com/dradwul/StilCSS/commits/main
