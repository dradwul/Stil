# Stil roadmap

Goal: a themeable CSS class library that can replace Bootstrap or Tailwind,
plus a complete Blazor component library built on it. Each phase ends with
something usable.

## Overview

| Phase | Content | Status |
| --- | --- | --- |
| 0 | Decisions and groundwork | ✅ Done |
| 1 | Tokens and theming engine | ✅ Done |
| 2 | Stil CSS 1.0 (Bootstrap replacement) | ✅ Done |
| 3 | Stil for Blazor core (forms, dialogs, toasts) | ✅ Done |
| 4 | Stil for Blazor: a component for every Stil CSS component | ✅ Done |
| 5 | Data components | ✅ Done |
| 6 | 1.0 release and ecosystem | ✅ Done |
| 7 | Time and planning: calendar, week planner, agenda | ✅ Done |
| 8 | Language and localization | Planned |

A phase is done when every item in it is checked. Items that need the
repository owner, and polish with no date yet, are listed separately at the
end, so they never keep a phase open.

## Where we are (0.1.0, Phases 0–6 done)

- **Stil CSS:** an npm package (`stil-css`, not yet published). It is built
  with Lightning CSS from partials into `dist/stil.css` and `stil.min.css`
  (about 16 kB gzipped), plus the optional 14 kB `stil.js`. It has:
  - cascade layers;
  - a full `--stil-*` token set;
  - light and dark mode;
  - derived colors that stay WCAG AA for any brand color;
  - a scoped reset and base;
  - a 12-column grid;
  - Bootstrap-equivalent components;
  - about 900 utility classes;
  - a [Bootstrap migration guide](src/Stil.Css/MIGRATING-FROM-BOOTSTRAP.md).
- **Stil for Blazor:**
  - About 60 components, with a Blazor counterpart for every Stil CSS
    component:
    - Buttons, icons and badges.
    - Lists, tables, cards, alerts and accordions.
    - Form fields with EditForm validation and add-ons.
    - Navigation: navbar, nav, nav groups, breadcrumbs, pagination, tabs and
      dropdowns.
    - Layout, dialogs, drawers and toasts.
    - Data: a data grid, autocomplete, multi-select, date range, file upload,
      tree view and KPI cards.
  - `IStilDialogService` and `IStilToastService`.
  - Theming with `AddStil()`, `StilTheme`, `StilHead` and
    `StilThemeProvider`.
  - 594 bUnit tests.
- **Stil.Charts:** SVG line, area, bar, donut and sparkline charts that
  follow the theme, with 67 bUnit tests.
- **Stil.Templates:** `dotnet new stil-app`.
- **Docs:** `docs/Stil.Docs`, a documentation site with live examples, API
  tables and a theme builder.
- **Samples:**
  - `samples/Stil.Html/index.html` shows every CSS class.
  - `samples/Stil.Html/admin.html` is a dashboard built only with Stil CSS.
  - `samples/Stil.Demo` is an interactive Blazor app with a component
    gallery and an orders CRUD page built on `StilDataGrid`.
- **Groundwork:** [CONTRIBUTING.md](CONTRIBUTING.md) holds the conventions.
  CI checks the CSS build and contrast, then builds, tests and packs. It also
  tests the template and runs the axe checks. Playwright screenshot tests run
  locally. [CHANGELOG.md](CHANGELOG.md) lists the changes in every package.

### Known issues found in the first review

All ten are fixed.

| # | Issue | Fixed in |
| --- | --- | --- |
| 1 | Every component set its own `.stil-scope`, so a theme on a wrapper element never reached the components. | Phase 0: tokens are on `:root`, and components no longer add `.stil-scope`. |
| 2 | Token names had no prefix (`--primary-color`). | Phase 0: all tokens are `--stil-*`. |
| 3 | White text had 1.5–2.1:1 contrast on every default color except danger. | Phase 1: text on colors is picked automatically (at least 4.58:1), and CI checks every token pair. |
| 4 | Variant names were mixed (`error`, `alert`, `warning`, "Danger"). | Phase 0: `primary`, `secondary`, `success`, `danger`, `warning` and `info` everywhere. |
| 5 | Neutral colors were hard-coded, so dark mode was impossible. | Phase 1: all colors are tokens, and neutrals use `light-dark()`. |
| 6 | Links inside `.stil-scope` had padding, so inline links shifted. | Phase 2: links are underlined and have no padding. |
| 7 | There were no cascade layers, so app CSS had to win on specificity. | Phase 1: every rule is in a `stil.*` layer. |
| 8 | `StilCard` always rendered an `<h2>`, and `StilAlert` always used `role="alert"`. | Phase 3: `HeadingLevel`, and `role` is set by severity. |
| 9 | The Blazor demo copied the HTML showcase by hand. | Phase 3: the HTML showcase covers the CSS, and the Blazor demo covers the components. |
| 10 | There was no CI, and nobody had checked the package names. | Phase 0: CI was added. `Stil` (NuGet) and `stil-css` (npm) are free. |

---

## Phase 0: Decisions and groundwork ✅ Done

- [x] Naming rules:
  - `stil-` class prefix (`bttn` became `stil-btn`).
  - `--stil-` token prefix.
  - One variant vocabulary.
  - Blazor components named `Stil{Component}`, which render `stil-{component}`.
- [x] Scope model: `stil-*` classes work anywhere. Base element styles are
  opt-in with `.stil-scope` (on `<body>` for a whole app).
- [x] Browser support: the latest two versions of Chrome, Edge, Firefox and
  Safari.
- [x] GitHub Actions: build, test, pack, and verify the package contents.
- [x] Package names checked: NuGet `Stil` and npm `stil-css` are free.

## Phase 1: Tokens and theming engine ✅ Done

- [x] A full token set on `:root`:
  - Semantic colors, with optional `--stil-color-{variant}-contrast` and
    `--stil-color-accent` overrides.
  - Neutrals.
  - Font sizes, spacing, radius, shadow, focus ring, z-index and motion.
    Motion is off under `prefers-reduced-motion`.
- [x] Derived colors are computed where they are used, so a theme on any
  wrapper reaches them: automatic text color on colored backgrounds, an
  accent for links and focus rings, and hover shades.
- [x] Every rule is in `@layer stil.tokens, stil.reset, stil.base,
  stil.components, stil.utilities`.
- [x] Light and dark mode through `data-stil-theme="light|dark|auto"` and
  `light-dark()`. Stil defaults to light.
- [x] `tools/check-contrast.mjs` checks every token pair and the derived
  colors for WCAG AA in CI.
- [x] There are no hard-coded colors in components or showcases.

**Verified:** switching the palette (two tokens) and the mode re-themes both
showcases in light and dark mode.

## Phase 2: Stil CSS 1.0 (Bootstrap replacement) ✅ Done

- [x] The sources are split into partials and built with Lightning CSS into a
  committed `dist/`. CI fails if `dist/` is out of date.
- [x] A scoped reset and a new base (typography, underlined links, code,
  `kbd`, `hr`, `blockquote`, `mark`).
- [x] Layout: container, a 12-column grid with responsive columns, stack,
  cluster and sidebar layout.
- [x] About 900 utilities with Bootstrap names and responsive variants.
- [x] Components:
  - Buttons (solid, outline, ghost, sizes, icon, loading, toggle, group,
    close).
  - Forms (field, input, select, check, radio, switch, input group,
    validation).
  - Card, alert, badge, avatar and table.
  - Nav, navbar, breadcrumb, pagination and tabs.
  - Dialog, dropdown, tooltip, progress, spinner and toast.
- [x] A [Bootstrap migration guide](src/Stil.Css/MIGRATING-FROM-BOOTSTRAP.md).
- [x] The npm package is prepared (`src/Stil.Css/package.json`, name
  `stil-css`). Publishing it is an owner task (see the end of this file).

**Verified:** `samples/Stil.Html/admin.html` is a full dashboard built only
with Stil CSS, with no custom CSS. It was checked in light and dark mode,
narrow and wide.

**Changes from the plan:**

- The dropdown uses `<details>` instead of the Popover API, because CSS
  anchor positioning is not yet in every target browser.
- The tooltip is pure CSS.

## Phase 3: Stil for Blazor core ✅ Done

- [x] `AddStil()`, a global `StilTheme` written by `<StilHead />`, and
  `StilThemeProvider` for part of a page.
- [x] `StilButton`, `StilBadge`, and the shared `StilVariant`/`StilSize`
  enums.
- [x] Form fields with a label, hint, validation messages,
  `aria-describedby` and `aria-invalid`, inside or outside `EditForm`:
  - `StilTextBox`, `StilTextArea`, `StilNumeric`, `StilDatePicker`.
  - `StilSelect`, `StilCheckbox`, `StilSwitch`, `StilRadioGroup`.
  - `StilField`.
- [x] `StilCard`, `StilAlert`, `StilList` and `StilListItem` (a row with
  title, description and status badge).
- [x] `StilTabs`, `StilDropdown`, `StilTooltip`, `StilProgress`,
  `StilSpinner`.
- [x] `StilDialog` with `IStilDialogService`, and toasts with
  `IStilToastService`.
- [x] `stil.js`, which Blazor loads automatically. It makes dropdowns close
  on an outside click or Escape, also in static SSR.
- [x] 366 bUnit tests covering parameters, ARIA and keyboard behaviour. The
  render-mode support is documented in the Blazor README.

**Verified:** the orders CRUD page in `samples/Stil.Demo`:

- It lists orders with status badges and filters them with tabs.
- It creates and edits orders in a validated dialog form.
- It deletes through a confirm dialog.
- It shows a toast after each action.

All of this was tested in the browser, in light and dark mode.

**Changes from the plan:**

- The form fields wrap Blazor's built-in inputs instead of reimplementing
  `InputBase<T>`.
- `StilIcon` moved to Phase 4, because the icon strategy is still open.

---

## Phase 4: A Blazor component for every Stil CSS component ✅ Done

Goal: everything Stil CSS can render also has a Blazor component, so Blazor
users rarely need to write `stil-*` classes by hand.

- [x] **Icons:**
  - About 25 built-in icons, drawn by Stil (see the icon strategy below), in
    `src/Stil.Css/src/icons.json`. That file generates the `stil-icon-*` CSS
    classes and is embedded in the Blazor assembly.
  - `StilIcon` (`Name`, `Size`, `Label`, or child content) and
    `StilIcons` name constants.
  - `IStilIconProvider` for other icon sets, plus
    `StilLigatureIconProvider` for fonts such as Material Symbols.
- [x] **Navigation:**
  - `StilBreadcrumbs` (`Items` or `StilBreadcrumbItem` children).
  - `StilPagination` (`@bind-Page`, `PageCount` or `TotalItems`/`PageSize`,
    ellipses, `ShowFirstLast`, `HrefTemplate` for links in static SSR).
  - `StilNav`/`StilNavLink` (wraps `NavLink`; `Icon`, `Badge`).
  - `StilNavbar` with a mobile menu (`CollapseBelow`; new
    `stil-navbar-expand-*` CSS and `data-stil-toggle` in stil.js).
- [x] **Tabs, extended:** `@bind-ActiveId` with `StilTab Id`,
  `Appearance=Pills` (new `stil-tabs-pills` CSS), `KeepAlive`.
- [x] **Dropdown, extended:**
  - `@bind-Open`, which stays in sync when stil.js closes the menu, through
    the new `onstiltoggle` event.
  - `Items` with `StilMenuItem`.
  - `Disabled` and `DropUp` (new `stil-dropdown-menu-up` CSS).
  - Arrow keys, Home and End in the menu, from stil.js, so this also works in
    plain HTML.
- [x] **Content and layout:**
  - `StilTable<TItem>` + `StilColumn` (`Field`/`Format` or `Template`, `Align`,
    `Striped`, `Hover`, `Compact`, `Bordered`, `Responsive`, `Caption`,
    `EmptyContent`, `OnRowClick`, `RowClass`).
  - `StilAvatar`.
  - `StilAccordion` + `StilAccordionItem` (new `stil-accordion` CSS, one open
    at a time through `<details name>`, no JavaScript).
  - `StilGrid`/`StilCol`, `StilStack`, `StilCluster`, `StilSidebarLayout`.
- [x] **Forms and actions:**
  - `Prepend`/`Append` and `PrependText`/`AppendText` on `StilTextBox`,
    `StilNumeric`, `StilSelect` and `StilDatePicker`.
  - Fields also work without `@bind-Value`.
  - `StilButtonGroup` and `StilCloseButton`.
- [x] **Drawer:** `StilDrawer` (new `stil-drawer` CSS; `Position`, `Width`).
  It shares its modal logic with `StilDialog` through `StilModalBase`.

**Done when:**

- [x] Every component in Stil CSS has a Blazor counterpart. Utility classes
  are the exception: they are meant to be used as `class` on any component.
  The Blazor README has a table mapping each CSS class to its component.
- [x] The demo gallery shows every component, and the HTML showcase shows
  the new CSS (icons, lists, accordion, drawer, collapsing navbar, pill tabs,
  drop-up menus).
- [x] bUnit tests cover the parameters, ARIA and keyboard behaviour: 449
  tests.

**Verified in the browser:**

- The mobile navbar opens and closes, and closes after navigating.
- The drawer is modal, and Escape closes it.
- Pagination updates its binding.
- The dropdown's `@bind-Open` stays in sync when stil.js closes the menu on
  an outside click or after an item is chosen.
- The arrow keys move between the dropdown items and skip disabled ones.
- Only one accordion item is open at a time.
- The table pages and its rows are clickable.
- Input add-ons render next to the control.

**Changes from the plan:**

- Link tabs (`StilTab Href`) were dropped. Tabs switch content within a page;
  to navigate between pages, use `StilNav` (with `Pills`), which gives the
  correct semantics (`aria-current`).
- The dropdown has `AlignEnd` and `DropUp` instead of a `Placement` enum.
- `StilAvatar` has no `Alt`. `Name` is the accessible name, and the image is
  decorative.
- `StilButtonGroup` has no `Size`. Set `Size` on its buttons.
- The icon provider is `StilOptions.IconProvider`.

### Icon strategy (decided 2026-10-15)

- **Built-in set:** about 25 icons drawn by Stil, MIT-licensed like the rest
  of the project.
  - Inline SVG on a 24 × 24 grid with a 2px round stroke in `currentColor`,
    so the icons follow themes and dark mode.
  - No icon font, no external dependency, no third-party license to ship.
  - To add an icon, add it to `src/Stil.Css/src/icons.json` and add a
    constant to `StilIcons`. A test checks that the two match.
- **Plug in any set:** register an `IStilIconProvider`
  (`AddStil(o => o.IconProvider = …)`) or put markup inside `StilIcon`.
  Material Symbols (Apache 2.0), Lucide (ISC), Bootstrap Icons (MIT) and
  Heroicons (MIT) all allow commercial use. The app brings the set it wants,
  along with that set's license.
- **Plain CSS:** the same SVGs are `stil-icon-*` CSS mask classes.

## Phase 5: Data components ✅ Done

- [x] `StilDataGrid`, with `StilDataGridColumn` (bound to a property
  expression) and `StilDataGridTemplateColumn`:
  - [x] Column templates, formats, alignment and widths.
  - [x] Sorting by clicking a header (ascending, descending, none), with
    `aria-sort` and `InitialSort`.
  - [x] Paging with `StilPagination`, a row summary and a page-size choice.
  - [x] A filter row: "contains" for text (debounced), a choice for enums and
    bools.
  - [x] Single and multiple selection, with a select-all checkbox for the page
    and `ItemKey` to keep the selection across reloads.
  - [x] Virtualization with a sticky header.
  - [x] Server-side data through `ItemsProvider`. `StilDataGridRequest`
    can apply the grid's filters and sorting to an `IQueryable` (EF Core).
  - [x] Toolbar, caption, empty and loading states, `OnRowClick`,
    `RowClass` and `RefreshAsync()`.
- [x] `StilAutocomplete` (combo box) and `StilMultiSelect`, following the
  WAI-ARIA combobox pattern, with in-memory or debounced async search and
  EditForm validation.
- [x] `StilDateRangePicker`, which reports an end before the start as a
  validation error.
- [x] `StilFileUpload`: a drop zone with type, size and count checks.
- [x] `StilTreeView` (the WAI-ARIA tree pattern) and `StilNavGroup` for
  nested menus in a vertical nav. A group opens by itself when one of its
  links is the current page.
- [x] `StilStatCard`, a KPI card with a trend badge.
- [x] `stil.js` stops the keys that trees, tabs and pickers handle from also
  scrolling the page, submitting a form or closing a dialog.
- [x] Charts: decided to build them as a separate package in Phase 6, so the
  core stays small. Until then, use a chart library next to Stil.
- [x] CSS for every new component (`stil-data-grid`, `stil-combobox`,
  `stil-chip`, `stil-dropzone`, `stil-date-range`, `stil-stat`,
  `stil-tree`, `stil-nav-group`), shown in `samples/Stil.Html`, and the
  `file`, `folder` and `upload` icons.
- [x] Docs: the Blazor and CSS READMEs.

**Done when:** a typical business app screen (the demo's orders page) can be
built only with Stil.

## Phase 6: 1.0 release and ecosystem ✅ Done

- [x] A documentation site, `docs/Stil.Docs` (Blazor WebAssembly):
  - A page for every component group, each with live examples whose code is
    the exact source that runs, and API tables generated from the XML docs.
  - Get started, theming and render-mode guidance.
  - `docs.yml` builds and publishes the site to a folder in CI. It is not
    deployed anywhere.
- [x] An interactive theme builder: edit tokens live, then copy the CSS or the
  C# `StilTheme`.
- [x] Playwright tests in `tests/Stil.Visual`, run with the installed Edge:
  - Screenshots of every showcase section, the admin page, every demo gallery
    section, the orders page and open dialogs, menus and listboxes, in light
    and dark mode, on desktop and phone viewports.
  - axe-core checks (WCAG 2.1 AA) of the samples and the demo in both modes.
    They run in CI; the screenshot baselines are made on Windows.
- [x] A `dotnet new stil-app` template (`Stil.Templates`) with `Primary`,
  `ColorMode`, `Interactivity` (Server or None) and `IncludeSampleContent`.
  `templates/test-templates.mjs` builds five option combinations with
  `-warnaserror` from an isolated template hive, also in CI.
- [x] `Stil.Charts`, a separate package: line, area, bar (grouped, stacked,
  horizontal), donut and sparkline charts as SVG.
  - They render in static SSR and follow the `--stil-chart-*` tokens and dark
    mode.
  - Each chart is a `<figure>` with an `<svg role="img">`, a generated
    summary, a data table and native tooltips.
  - 67 bUnit tests.
- [x] Package metadata (authors, repository, icon, Source Link, symbols, XML
  docs), one shared version in `Directory.Build.props`, and a changelog.
  Publishing is not planned: nothing is pushed to NuGet or npm.

**Verified in the browser,** in light and dark mode:

- The new docs pages: dialogs, drawer, toasts, progress, data grid
  (filters, selection, server-side data, virtualization), tree view keyboard,
  stat cards and charts.
- The charts on a phone: crowded labels are thinned and there is no sideways
  scrolling.
- The demo's new charts section.
- Apps generated with `dotnet new stil-app` were built (five option
  combinations) but not run in a browser.

**Changes from the plan:**

- The docs site is its own WebAssembly app instead of being built from the
  demo. The demo stays the place for the visual tests.
- Bool columns in `StilDataGrid` now show `TrueText` and `FalseText` in their
  cells too, not only in the filter.
- The showcase's code blocks can be focused, so keyboard users can scroll
  them. This was the one axe finding.

## Phase 7: Time and planning ✅ Done

Goal: a calendar, a week planner and an agenda that are easy to understand,
calm to use and look like the rest of Stil. What makes them stand out is
showing at a glance how busy a period is and where the free time is.

**Ideas that set it apart:**

- **Load at a glance:** every day in the month view gets a light tint by how
  full it is (the same tokens as Stil.Charts). Free and fully booked days show
  without a click.
- **Free time visible:** the week view shows a "now" line in the accent color
  and marks free blocks quietly. Clicking a free block creates an event there.
- **Calm density:** overlapping events cascade in the week, so every title
  stays readable. A busy day shows "+N more" in the month, and dots in a
  narrow month.
- **Nordic defaults that follow the culture:** the first day of the week, ISO
  week numbers and the 24-hour clock come from `CultureInfo`.

**Plan:**

- [x] An HTML example, `samples/Stil.Html/calendar.html`: a planner with a
  month, week and agenda view, a mini month, free time and calendar filters.
  On a phone it opens in the agenda; the chosen view is remembered.
- [x] Stil CSS: `stil-event`, `stil-calendar`, `stil-week` and `stil-agenda`.
  - Event colors are variants (`stil-event-{variant}`) or an own color
    (`--stil-event-color`).
  - The week shows any number of days and hours (`--stil-week-days`,
    `--stil-week-first-hour`, `--stil-week-hours`); the working day for load
    and free time is set by the page (08–17 in the example).
  - Tokens: `--stil-calendar-hour`, and `--stil-calendar-load` to override
    the busy tint.
  - `stil.js` adds the WAI-ARIA grid keyboard pattern to the month and fires
    `stilcalendaredge` at the edges. In a week with `data-stil-editable` it
    drags events to move, resize and create them and fires `stilweekedit`.
  - `stil-week-auto` turns a narrow week into an agenda of the same days.
  - A showcase section, Playwright baselines and axe checks of
    `calendar.html`.
- [x] `StilCalendar`: a month view that picks dates and shows events as chips,
  or dots in a narrow container. The keyboard moves past the edges of the
  month; changing months works with links in static SSR (`HrefTemplate`).
  `Mini` and `RangeStart`/`RangeEnd` give a small month next to a planner.
- [x] `StilWeekPlanner`: day, week and work-week views on a time grid, a
  cascade layout for overlapping events, free blocks, a now line, and drag to
  create, move and resize (`Editable`, interactive render modes). Clicking a
  free block also creates an event, so dragging is never required.
- [x] `StilAgenda`: events grouped by day, with the free gaps between them. A
  planner shows its days as an agenda in a narrow container, such as a phone;
  `AgendaWhenNarrow="false"` keeps the time grid.
- [x] One data model for all three, like the data grid: `Items`, plus
  `Start`, `End`, `Title`, `Variant`, `Color`, `Details` and `Draft`
  selectors. No event class of its own.
- [x] bUnit tests, a docs page, a demo section and Playwright baselines.

**Not in the first version:** recurring events (the app expands them into
occurrences) and time zone conversion (the calendar shows the times it gets).

## Phase 8: Language and localization

Goal: one setting makes every Stil component speak the app's language,
the app can translate its own texts with the same package, and users can
switch language themselves. Today about 100 built-in texts
(`PreviousText`, `FreeText`, `EmptyText` and so on, in 42 components) are
English defaults that an app has to override one by one. Dates, times and
week days already follow `CultureInfo`.

**Ideas that set it apart:**

- **Nordic languages built in:** English, Swedish, Norwegian (Bokmål),
  Danish and Finnish ship in the package, so a Nordic app needs no
  translation work.
- **Zero config:** the texts follow `CultureInfo.CurrentUICulture`. An app
  that already sets its culture gets translated components without touching
  Stil.
- **Parameters still win:** a text set on a component always beats the
  translation, so existing apps keep working.
- **App texts without resx:** the app's own texts live in plain JSON files
  and are read through the standard `IStringLocalizer<T>`. There is no
  Stil-specific API to learn, and code written against it keeps working
  with resx if the app ever moves away from Stil.
- **One package:** everything ships in `Stil`. The only new dependency is
  Microsoft's small `Microsoft.Extensions.Localization`, which comes along
  transitively.

**Plan:**

- [ ] `IStilLocalizer`, with a default implementation backed by embedded
  JSON files for en, sv, nb, da and fi. JSON instead of resx means no
  satellite assemblies, which keeps WebAssembly and trimming simple. The
  text parameters default to `null` and fall back to the localizer. Format
  texts such as `HoursFormat` and `BookedFormat` are included.
  - `AddStil(o => …)` can add or replace languages, or plug in the app's own
    `IStringLocalizer` resources.
  - The keys are documented, and a test checks that every built-in language
    has every key (like the icon test).
- [ ] `AddStilLocalization()`, an opt-in JSON-based `IStringLocalizer` for
  the app's own texts:
  - It reads files such as `Locales/sv.json`, from disk on the server and
    from `wwwroot` or embedded resources in WebAssembly (the same loader as
    Stil's own texts).
  - Keys can be shared (`sv.json`) or per type (`Pages.Home.sv.json` for
    `IStringLocalizer<Home>`). Lookups fall back from the specific culture
    to the parent culture (`sv-FI` to `sv`) and then to a default language.
  - A missing key shows the key itself, and logs a warning in development.
  - Format arguments work as usual (`L["Hello {0}", name]`).
  - Apps use `@inject IStringLocalizer<Home> L` and `@L["Save"]` as in the
    ASP.NET Core docs.
- [ ] `StilLanguageSelector`: a menu of the app's cultures (`Cultures`),
  each shown by its own name ("Svenska", "Suomi"). No flags, because flags
  are countries, not languages.
  - Server and static SSR: it posts to `MapStilCulture()`, which sets the
    standard ASP.NET Core culture cookie and redirects back, so it works
    without interactivity.
  - WebAssembly: it stores the choice and reloads, and a startup helper
    applies it before the app renders.
- [ ] Culture checks: numbers, dates and times follow the culture in every
  form field, in the data grid's cells and filters, and in the charts' axes
  and summaries.
- [ ] Right-to-left: everything works under `dir="rtl"`.
  - The CSS uses logical properties; replace the last two physical ones (in
    `form.css` and `overlay.css`).
  - `stil.js` mirrors the left and right arrow keys in tabs, the calendar
    grid and horizontal menus.
  - Directional icons (chevrons, arrows) flip.
  - An RTL section in the HTML showcase and the demo, with Playwright
    baselines.
- [ ] The language selector in the docs site and the demo, translated demo
  texts through `AddStilLocalization()`, a docs page on localization, and a
  `Culture` option in `dotnet new stil-app` that sets up the localizer and
  a `Locales` folder.
- [ ] bUnit tests for the fallback order (parameter, then app texts, then
  built-in language, then English) and for the selector in each render
  mode. Unit tests for the JSON localizer: shared and per-type files,
  culture fallback, missing keys, format arguments and invalid JSON.

**Not in the first version:**

- Languages beyond the five built-in ones (apps add them through
  `AddStil`).
- Plural rules: use separate keys or format strings.
- Reloading JSON files while the app runs.
- Translating the docs site itself.

---

## Owner tasks (not part of a phase)

These need the repository or package owner.

- [ ] Rename the GitHub repository from `StilCSS` to `Stil`, then update the
  links in `src/Stil.Blazor/README.md` and `src/Stil.Css/package.json`.
- [ ] Push and confirm that CI passes on GitHub. It has only been run
  locally so far.

## CSS backlog (no phase yet)

Small CSS improvements found while building the samples. They are picked up
when a phase needs them.

- [ ] A fixed width/max-width scale, and sticky offsets below a sticky navbar.
- [ ] Tooltips on elements that already use `::after` (dropdown summary, busy
  button). Move to a popover-based tooltip once anchor positioning is in
  every target browser.
- [ ] A neutral or "on-color" button variant. Meanwhile,
  `stil-btn-ghost stil-text-inherit` works on colored bars.
- [ ] Striped or animated progress bars.
- [ ] A 50–900 scale per brand color, if the utilities need one.
- [ ] Carousel and popover.
