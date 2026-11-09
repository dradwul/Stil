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
| 8 | Language and localization | ✅ Done |
| 9 | Forms 2: popover, time, slider, wizard, rating, color, masks | ✅ Done |
| 10 | Layout and media: skeleton, splitter, carousel, image cropper, PDF viewer | ✅ Done |
| 11 | Data grid 2, in five parts: totals and export ✅, column layout ✅, grouping ✅, inline editing ✅, tree grid ✅ | Done |
| 12 | Capture and records: QR codes, scanner, signature, activity log, chat | Done |
| 13 | Boards and workspace: kanban, command palette, notifications, recurring events, resources, Gantt | In progress |
| 14 | Markdown, rich text and charts 2: Markdown editor and view, rich text editor, scatter, gauge, heat map, combined | Planned |
| V2 | Version 2, Stil Sites: section blocks, page model, a visual page builder, content | Planned (after 1.0) |

A phase is done when every item in it is checked. Items that need the
repository owner, and polish with no date yet, are listed separately at the
end, so they never keep a phase open.

## Where we are (0.1.0-preview.2, Phases 0–10 done)

- **Stil CSS:** an npm package (`stil-css`, not yet published). It is built
  with Lightning CSS from partials into `dist/stil.css` and `stil.min.css`
  (about 19 kB gzipped), plus the optional `stil.js` (10 kB gzipped). It has:
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
  - Built-in texts in English, Swedish, Norwegian, Danish and Finnish,
    `AddStilLocalization()` for the app's own JSON texts, `StilLanguageSelector`
    and right-to-left support.
  - Forms 2: time and date-time pickers, sliders, rating, color picker,
    masked and PIN inputs, popover, stepper and wizard.
  - Layout and media: skeleton, splitter, carousel, image cropper, camera
    and PDF viewer.
  - Capture and records: QR codes and barcodes, scanning with the camera or
    a handheld scanner, signature pad, activity log and timeline, chat and
    comments.
  - 799 bUnit tests.
- **Stil.Charts:** SVG line, area, bar, donut and sparkline charts that
  follow the theme, with 68 bUnit tests.
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
  Publishing came later: a version tag publishes to NuGet (see the release
  flow below and in CONTRIBUTING.md).

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

## Phase 8: Language and localization ✅ Done

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
  Microsoft's small `Microsoft.Extensions.Localization.Abstractions`, which comes along
  transitively.

**Plan:**

- [x] `IStilLocalizer`, with a default implementation backed by embedded
  JSON files for en, sv, nb, da and fi. JSON instead of resx means no
  satellite assemblies, which keeps WebAssembly and trimming simple. The
  text parameters default to `null` and fall back to the localizer. Format
  texts such as `HoursFormat` and `BookedFormat` are included, and so are the
  charts' generated summaries.
  - `AddStil(o => o.Texts.Add(…))` rewords texts or adds a language; an own
    `IStilLocalizer` can read them from anywhere, for example resx files.
  - The keys are listed on the docs page, and a test checks that every
    built-in language has every key with the same placeholders.
- [x] `AddStilLocalization()`, an opt-in JSON-based `IStringLocalizer` for
  the app's own texts:
  - It reads `Locales/**/*.json`, which the package embeds in the app
    (`build/Stil.targets`), so the same files work in Server, static SSR and
    WebAssembly.
  - Keys can be shared (`sv.json`) or per type (`Pages/Home.sv.json` for
    `IStringLocalizer<Home>`). Lookups fall back from the specific culture
    to the parent culture (`sv-FI` to `sv`) and then to the default language.
  - A missing key shows the key itself and logs one warning.
  - Format arguments work as usual (`L["Hello {0}", name]`), and keys under
    `"Stil"` override Stil's own texts.
- [x] `StilLanguageSelector`: a menu of the app's cultures (`Cultures`),
  each shown by its own name ("Svenska", "Suomi"). No flags, because flags
  are countries, not languages.
  - `stil.js` stores the choice in the standard ASP.NET Core culture cookie
    and in local storage, then reloads. The server reads the cookie with
    `UseRequestLocalization()`; WebAssembly apps call
    `StilCulture.ApplyStoredAsync()` before rendering.
  - Without the script, each link shows the page once in its language
    through `?culture=`.
- [x] Culture checks: numbers and dates in the data grid, the calendar and
  the charts' axes, tables and summaries follow the current culture; ARIA
  values and SVG coordinates stay invariant.
- [x] Right-to-left: everything works under `dir="rtl"`.
  - The switch knob uses a logical inset; drawers slide in from the right
    side, and tree and nav group chevrons turn the other way.
  - Tabs and tree views swap the left and right arrow keys in right-to-left
    cultures, and `stil.js` does the same in the calendar grid.
  - Arrow and chevron icons flip (`stil-icon-directional`).
  - An RTL section in the HTML showcase and the demo, with Playwright
    baselines.
- [x] The language selector in the docs site and the demo, translated demo
  texts through `AddStilLocalization()`, a docs page on localization, and a
  `Culture` option in `dotnet new stil-app` that sets up the localizer and
  a `Locales` folder.
- [x] bUnit tests for the fallback order (parameter, options, app texts,
  built-in language, English), the selector, the WebAssembly helper and the
  right-to-left keys. Unit tests for the JSON localizer: shared and per-type
  files, culture fallback, missing keys, format arguments and invalid JSON.

**Verified in the browser:** in the demo, choosing Svenska sets the cookie
and reloads with Swedish component texts, Swedish app texts and a Swedish
calendar; العربية turns the whole page right to left. In the docs site
(WebAssembly) the choice is kept in local storage and applied at startup.

**Changes from the plan:**

- No `MapStilCulture()` endpoint. It would need the ASP.NET Core shared
  framework, which a WebAssembly app cannot reference. `stil.js` writes the
  same cookie in the browser instead, which works in every render mode.
- The app's JSON files are embedded instead of read from disk or `wwwroot`,
  so there is one way that works everywhere.
- In Blazor components the arrow keys follow the UI culture's direction, not
  the `dir` attribute, since the server cannot see the DOM. Set `dir` from
  the same culture (`StilCulture.Direction()`).
- The tooltip keeps `left: 50%`: it centers the bubble, which is the same in
  both directions.
- A stacked bar chart's summary says "Totals range from …" in every language
  instead of building the word from `TotalText`.
- To hide a file upload text, set it to `""`; `null` now means the
  localized default.

**Not in the first version:**

- Languages beyond the five built-in ones (apps add them through
  `AddStil`).
- Plural rules: use separate keys or format strings.
- Reloading JSON files while the app runs.
- Translating the docs site itself.

## Detour: Lexend as the default font ✅ Done

A small change outside the phase plan: `--stil-font-family` is now
`'Lexend', sans-serif` instead of Didact Gothic.

- [x] The token in `tokens.css`, and the font link in the HTML samples, the
  demo and the docs site: `https://fonts.cdnfonts.com/css/lexend`. As before,
  the stylesheet never fetches fonts itself.
- [x] The docs (theming page, theme builder, CSS README).
- [x] Before and after screenshots, approved by the owner.

The logo keeps Didact Gothic: its text is drawn as outlines. The Playwright
baselines are unchanged, because the tests block external fonts and render
with the fallback font.

## Detour: Choosing a font ✅ Done

A small change outside the phase plan: pick the app's font when you create it
or in `AddStil()`, from a short list or any font on cdnfonts.com or Google
Fonts.

- [x] `StilFont` (a family and its stylesheet), with `StilFont.CdnFonts("…")`
  and `StilFont.Google("…")`, and `StilFonts`: Lexend, BR Cobane, BR Segma,
  BR Sonoma, BR Shape, Averral, Sukplena, TASA Explorer, Halcom, Utara,
  Geologica, and the display fonts BLOWREPH, Taster and WARM STORIES. A new ready-made font is one line in `StilFonts`; the
  theme builder lists it by itself.
- [x] `options.Font`: `<StilHead />` links the stylesheet and sets
  `--stil-font-family`. A theme's `FontFamily` still wins.
- [x] `dotnet new stil-app --Font …` (BR Shape by default, `system` for none)
  and `--GoogleFont "…"`.
- [x] The theme builder previews the fonts; `samples/Stil.Html/fonts.html`
  shows them side by side.
- [x] The docs: the theming page, the Blazor, CSS and template READMEs.

The stylesheet still never fetches fonts itself. Each font's license is on
cdnfonts.com; some are free for personal use only, so the docs say to check
it.

## Detour: Popups in the top layer ✅ Done

A fix outside the phase plan: an open list or menu was cut off by any parent
with `overflow`, such as a docs example, a scrolling card or a dialog.

- [x] stil.js moves open dropdown menus (`details.stil-dropdown`) and combobox
  lists (`.stil-combobox-popup` without `hidden`) to the top layer with
  `popover="manual"`, places them under their toggle or field (above when
  there is no room), and follows it on scroll and resize. Stil still opens
  and closes them; the DOM stays the same, so clicks and keys work as before.
- [x] It covers `StilDropdown`, `StilAutocomplete`, `StilMultiSelect` and
  `StilTimePicker` without changes to the components. Without stil.js the
  popups open in place, as before.
- [x] The docs workaround (examples that let popups overflow) is gone.

The date pickers use the browser's own picker, which is never clipped.

---

## Phase 9: Forms 2 ✅ Done

Goal: the inputs that business forms still lack, built on the same field
model as Phase 3 (label, hint, validation, `@bind-Value`, EditForm), and
following the culture from Phase 8.

**Plan:**

- [x] `StilPopover`: a panel anchored to a button, for pickers and rich
  hints. It uses the Popover API (so it works without JavaScript and in
  static SSR), and `stil.js` places it under or above the button. CSS
  `stil-popover`.
- [x] `StilTimePicker` (`TimeOnly`) and `StilDateTimePicker` (`DateTime`):
  typed input ("1430", "930") with a list of times in steps (`Step`),
  `Min`/`Max`, and the culture's clock.
- [x] `StilSlider` and `StilRangeSlider` (`@bind-Start`/`@bind-End`): steps,
  a formatted value label and min and max marks, as native
  `input type="range"`, so they work with the keyboard and in static SSR.
- [x] `StilStepper` and `StilWizard`: steps with a state (done, current,
  error), and a wizard that validates only the current step's fields before
  moving on.
- [x] `StilRating`: stars or another icon, read-only or editable, as a radio
  group.
- [x] `StilColorPicker`: the native color input, the hex code and swatches
  (Stil's palette by default), with a WCAG contrast hint.
- [x] `StilMaskedInput` (`Mask="00000000-0000"`, formatted while typing by
  `stil.js`) and `StilPinInput` (a one-time code).
- [x] CSS for all of them (`inputs.css`), built-in texts in the five
  languages, 34 bUnit tests, a docs page, a demo section and a showcase
  section.

**Done when:** a booking form (date, time, duration slider, rating) and a
three-step sign-up wizard can be built only with Stil. ✅

**Verified in the browser:** the popover opens under its button and closes
on an outside click; typed times and steps follow the culture; the masked
input and the code field format while typing, and `OnComplete` fires; the
wizard stops on a step with errors and marks it in the stepper; the section
works at phone width.

**Changes from the plan:**

- The PIN input is one field drawn as cells, not separate boxes, so paste,
  autofill and screen readers work as usual.
- The range is its own component, `StilRangeSlider`, so `StilSlider` keeps
  the plain field model (`@bind-Value`, EditForm validation).
- The color picker's contrast hint is computed in C# (black or white text,
  with the ratio) instead of reusing `derived.css`.
- The popover has no `@bind-Open`: opening it from code would need
  JavaScript. `OnToggle` reports its state.

## Phase 10: Layout and media ✅ Done

Goal: the layout pieces and media components that make an app feel
finished. Most are small, and several (skeleton, splitter) are used by the
later phases.

**Plan:**

- [x] `StilSkeleton`: placeholder shapes (text lines, a circle, a block) with
  a calm shimmer that stops under reduced motion. The data grid shows
  skeleton lines while its first rows load.
- [x] `StilSplitter`: two panes with a draggable separator, horizontal or
  vertical (nest them for more), min and max sizes, Enter to collapse and
  restore, keyboard resizing (the WAI-ARIA window splitter pattern), and
  `@bind-Size` to save the layout. The resizing is in `stil.js`, so it also
  works in static SSR.
- [x] `StilCarousel`: slides with previous and next buttons, dots, swipe,
  optional autoplay with a pause button that pauses on hover and focus and
  never runs under reduced motion. CSS scroll snap, so it also works without
  JavaScript.
- [x] `StilImageCropper`: crop and turn an image to a fixed aspect ratio
  (round for avatars), with zoom, keyboard nudging, and the result as a
  `StilImageData` (a data URL and bytes). `StilCameraCapture` takes a photo
  with the device camera into the same flow, or lets the user choose one.
- [x] `StilPdfViewer`: a PDF in the browser's own viewer inside a Stil frame
  (download, open in a new tab, print), with a clear message where the
  browser cannot show PDFs, and `ChildContent` for an own renderer.
- [x] CSS (`media.css`: `stil-skeleton`, `stil-splitter`, `stil-carousel`,
  `stil-cropper`, `stil-camera`, `stil-pdf`), the `camera` and `download`
  icons, built-in texts in the five languages, 14 bUnit tests, a docs page,
  a demo section and a showcase section.

**Done when:** the demo has a mail-like layout (splitter with a list and a
reading pane, skeletons while loading), a carousel, and an avatar that is
cropped and turned into an image to upload. ✅

**Verified in the browser:** the splitter resizes with the keyboard and
collapses with Enter, and the size is bound; the carousel's buttons and dots
work in the demo and in plain HTML; the cropper shows a round frame and
`CropAsync` returns a 256 × 256 PNG; the docs show a PDF in the browser's
viewer.

**Changes from the plan:**

- Cards and charts have no `Loading` parameter yet; wrap `StilSkeleton` in
  them. The data grid has it built in.
- The cropper keeps a fixed frame and moves and zooms the image behind it,
  the usual way for avatars, instead of a free frame. Free cropping can come
  later.
- The result of a round crop is a square image; show it round with CSS.
- The camera was checked in code and tests only: the preview browser has no
  camera. The fallback (choose a photo) is covered by a test.

## Phase 11: Data grid 2 ✅ Done

Goal: the data grid features that larger apps expect, without making the
simple grid harder to use. The phase is split into five parts that each end
with something usable, in this order, because later parts build on earlier
ones (group sums use the aggregates, editing uses the column layout).

**Done when (all parts):** the demo's orders page groups by status, edits a
row in place, exports CSV and remembers its column layout.

### Phase 11.1: Totals and export ✅ Done

- [x] Aggregates per column (`Aggregate="StilAggregate.Sum"`: sum, average,
  count, min, max, or `AggregateValue` for a total of your own) in a footer
  row over every filtered row, formatted like the column's cells.
- [x] `ExportCsvAsync()` and an export button in the toolbar (`ShowExport`):
  the filtered and sorted rows of every page, in the culture's list
  separator, with a byte order mark so Excel opens it right. Numbers have no
  thousands separators, and text that would start a formula is defused.
- [x] Server-side data: `StilDataGridRequest.Aggregates` lists the totals and
  `ComputeAggregates` computes them from the columns' properties (in the
  database with EF Core); the export pages through `ItemsProvider` 500 rows
  at a time.

**Done when:** the orders page shows the total amount under the grid and
exports the filtered orders to a CSV that opens correctly in Excel. ✅ (12
bUnit tests; the CSV was checked in tests, not opened in Excel.)

**Changes from the plan:** numbers are exported without the cell format's
thousands separators, so spreadsheets read them as numbers; dates and other
values keep the cell format.

### Phase 11.2: Column layout ✅ Done

- [x] Column resizing by dragging the header edge, and with the keyboard
  (arrow keys, Shift for bigger steps, Home and End), within `MinWidth` and
  `MaxWidth` (`ResizableColumns`, or `Resizable` per column).
- [x] Reordering by dragging a header (`ReorderableColumns`, or
  `Reorderable` per column); with the keyboard in the column chooser.
- [x] Frozen first and last columns (`Frozen="StilFrozen.Start"` or `End`),
  which stay in view while the grid scrolls sideways; the selection column
  stays with the frozen start columns.
- [x] A column chooser in the toolbar (`ShowColumnChooser`) that shows, hides
  and moves columns, and resets the layout (`Hideable` per column).
- [x] `@bind-Layout` (`StilDataGridLayout`: order, widths, hidden columns,
  sort), which serializes to JSON, so an app can save and restore it.

**Done when:** the orders page keeps its column widths, order and hidden
columns after a reload. ✅ (It saves the layout in local storage; checked in
the browser, with 8 bUnit tests.)

**Changes from the plan:** moving a column with the keyboard is done in the
column chooser (move earlier, move later) instead of on the header, so the
header keeps its keys for sorting.

### Phase 11.3: Grouping ✅ Done

- [x] Grouping by one or more columns (`@bind-GroupBy`, or the group chooser
  in the toolbar, `ShowGroupChooser`), with group headers that show the
  value and the count.
- [x] Group headers collapse and expand with the mouse and keyboard (Enter,
  Space, the left and right arrow keys); all groups at once from the toolbar.
- [x] Group sums from the column aggregates (11.1) in each group header,
  under their columns.
- [x] Paging counts groups' rows, and virtualization works with groups.
- [x] Server-side data: `StilDataGridRequest` carries the grouping
  (`GroupBy`), and `ItemsProvider` can return groups with their counts and
  sums (`ComputeGroups`).

**Done when:** the orders page groups by status with the count and the total
amount per status. ✅ (Checked in the browser, with 9 bUnit tests.)

**Changes from the plan:** the group menu is in the toolbar
(`ShowGroupChooser`), not in each header, so the headers keep their keys for
sorting and resizing. Paging counts the rows that show: a group that continues
on the next page repeats its header there, and the rows of a collapsed group
leave the pages.

### Phase 11.4: Inline editing ✅ Done

- [x] Row editing: an edit button (`StilDataGridEditColumn`), a double
  click, Enter or F2 turns a row's cells into Stil fields (text, number, date,
  choice, switch, or an `EditTemplate`), with Save and Cancel.
- [x] Validation with an EditContext and data annotations per row, with the
  messages next to the fields; Escape cancels.
- [x] `OnRowSave` (return false to keep editing, for example after a server
  error) and `OnRowCancel`; a new row at the top with `AddRowAsync()`.
- [x] Optional cell editing (`EditMode="Cell"`): click a cell, edit, and
  leave it (or press Enter) to save.

**Done when:** the orders page edits a customer and an amount in place, with
validation, and adds a new order as a row. ✅ (Checked in the browser, with 8
bUnit tests.)

**Changes from the plan:** the row's fields share an EditContext without an
EditForm, since a form cannot wrap table cells. The grid validates only the
fields of the edited columns, so a required property that the grid does not
show cannot block a save. The item is edited in place, and Cancel puts its
public properties back from a copy taken when editing started.

### Phase 11.5: Tree grid ✅ Done

- [x] `StilTreeGrid<TItem>`: the data grid's columns, sorting and formats
  for items with children (`ChildrenSelector`), with expand and collapse in
  the first column.
- [x] The WAI-ARIA treegrid pattern: `aria-level`, `aria-expanded`, and the
  arrow keys to move, expand and collapse.
- [x] Children loaded on demand (`LoadChildren`), with a loading state.
- [x] Aggregates (11.1) that roll up from children to parents.

**Done when:** the demo shows a cost-centre budget as a tree grid where
departments roll up into divisions. ✅ (The Budget page, with 10 bUnit tests.)

**Changes from the plan:** `StilTreeGrid` derives from `StilDataGrid`, so
it also filters (a match keeps the rows above and below it), edits and
exports. A parent's totals cover the rows below it that have no children of
their own, so its own values do not count twice. It has no grouping or
`ItemsProvider`; `LoadChildren` loads from a server instead. Playwright
checks of the treegrid were left out for now.

## Phase 12: Capture and records ✅ Done

Goal: components for apps that register things on the spot: check-ins at a
reception, deliveries in a warehouse, visits, inspections. Few component
libraries cover this, and it fits Stil's focus on calm, practical business
screens. The phase is split into six parts that each end with something
usable; the scanner demo needs codes to scan, and chat builds on the
activity log's data model.

**Done when (all parts):** the check-in demo works on a phone (camera), on a
desktop with a USB scanner, and with only a keyboard.

### Phase 12.1: QR codes and barcodes ✅ Done

- [x] `StilQrCode` and `StilBarcode`: generate QR codes and Code 128 / EAN-13 /
  EAN-8 barcodes as SVG in C#, with no dependency, so they render in static
  SSR, print sharply and follow the theme.
- [x] The encoders are public (`StilCodes`), for codes on a PDF or a label
  printer.
- [x] CSS (`stil-qr-code`, `stil-barcode`), built-in texts, bUnit tests and
  docs.

**Done when:** a QR code and each barcode format scan with a phone and print
sharply. ✅ (Every format and QR level decoded with ZXing, QR up to 2 000
characters with UTF-8 text; 19 bUnit tests.)

**Changes from the plan:** a value the format cannot hold shows a short
localized message instead of throwing, since codes often come from data.

### Phase 12.2: Scanning ✅ Done

- [x] `StilScanner`: reads barcodes and QR codes with the camera. It uses the
  browser's `BarcodeDetector` where it exists (Chromium, Android) and an
  optional decoder that the app registers elsewhere, so Stil ships no
  third-party code. Torch and camera switching, a scan sound, and
  `OnScan(code, format)`.
- [x] `StilScanInput`: an input that recognizes handheld barcode scanners
  (fast keystrokes ending in Enter), so a USB or Bluetooth scanner works
  without a camera. It works in every browser, and also as a plain text
  field.

**Done when:** a code from 12.1 scans with the camera on a phone and with a
USB scanner on a desktop. ✅ In a desktop browser: the scanner's fallback, and
fast (scanner) and slow (typed) keys told apart; 6 bUnit tests. The camera
and a USB scanner are checked on real devices by the owner.

**Changes from the plan:** the scanner has a field under the camera for a
typed code or a handheld scanner, so it also works without a camera or
`BarcodeDetector`. The docs show a ZXing decoder for Safari, Firefox and
desktop Chrome on Windows and Linux.

### Phase 12.3: Signature pad ✅ Done

- [x] `StilSignaturePad`: draw a signature with a finger, pen or mouse
  (pointer events, pressure where available). `Clear`, undo, and output as
  SVG, PNG and the points, with EditForm validation ("signature required").
  A typed name is the keyboard alternative. The docs state that it is a
  drawn signature, not a qualified electronic signature.

**Done when:** a form requires a signature, drawn or typed, and saves it.
✅ (The docs example: drawn with the mouse in the browser, saved as SVG and
PNG; 5 bUnit tests.)

**Changes from the plan:** the value is a `StilSignature` (strokes or typed
name, with the area's size) that is null until signed. SVG comes from C#, so
it also works on the server; PNG is drawn by the browser.

### Phase 12.4: Activity log and timeline ✅ Done

- [x] `StilActivityLog<TItem>`: a log of anything that happens (check-ins,
  status changes, comments, system events), from `Items` and selectors like
  the calendar: `Time`, `Title`, `Actor`, `Variant`, `Icon`, `Details`.
  - Grouped by day with relative times ("2 minutes ago", localized), filters
    by type and actor, search, and virtualization for long logs.
  - Live: new entries appear at the top without the list jumping, and are
    announced politely to screen readers.
- [x] `StilTimeline` is the compact form, for an order's history in a side
  panel.

**Done when:** an order's history shows as a timeline, and a live log takes
new entries without jumping. ✅ (The docs: an order's history, and a
reception log that takes new check-ins; 6 bUnit tests.)

**Changes from the plan:** `Time` is a local `DateTime`, like the
calendar's. The log keeps its place with the browser's scroll anchoring
(`overflow-anchor`) rather than script. `Now` fixes the clock.

### Phase 12.5: Chat and comments ✅ Done

- [x] `StilChat` and `StilComments`: a conversation or a comment thread on
  top of the activity log's data model, with avatars, own and others'
  messages, day separators, a composer (Enter sends, Shift+Enter breaks the
  line), attachments through `StilFileUpload`, and "is typing". Transport is
  the app's (SignalR or anything else); Stil only renders and raises
  `OnSend`.

**Done when:** two browser windows chat through the demo's own transport.
✅ In part: the docs chat with a simulated reply (typing, then an answer),
sent with Enter in the browser; 4 bUnit tests. Two windows over a real
transport come with the check-in demo (12.6).

**Changes from the plan:** `Title` is the message text, so the same items
work in the log and the chat. `StilComments` is `StilChat` in thread form,
where Ctrl+Enter posts. Attachments use Blazor's `InputFile` behind an
"Attach" button rather than the full `StilFileUpload` drop zone.

### Phase 12.6: Check-in demo ✅ Done

- [x] A demo page that combines them: a check-in desk that scans a badge or
  a QR code, asks for a signature, and writes the visit to the log.
- [x] CSS (`stil-scanner`, `stil-signature`, `stil-log`, `stil-timeline`),
  built-in texts in the five languages, bUnit tests and docs for every part.

**Done when:** the phase's own "done when" above. ✅ The demo's Check-in page
(`/check-in`, and `/check-in?as=host` for a host): with only a keyboard
(a typed code, a typed signature) in the browser, and in two windows that
share the log and the chat, with "is typing", through the app's own shared
service. The camera on a phone and a USB scanner on a desktop are checked by
the owner on real devices.

**Changes from the plan:** the chat between the reception and a host is part
of the demo, so 12.5's two windows are covered here. The page lists today's
visitors with their QR codes, to scan from the screen with a phone.

## Phase 13: Boards and workspace 🚧 In progress

Goal: the building blocks of planning and internal tools beyond forms and
grids. The phase is split into six parts that each end with something usable;
the resource view and the Gantt view build on the recurring events.

**Done when (all parts):** the demo has a task board, a command palette and a
resource schedule for meeting rooms.

### Phase 13.1: Kanban board ✅ Done

- [x] `StilKanban<TItem>`: columns of cards that move by dragging (stil.js,
  like the week planner) and with the keyboard (Space picks a card up, the
  arrow keys move it, Space drops it, Escape puts it back), announced to
  screen readers.
- [x] WIP limits per column, collapsible columns, and `OnCardMove` with a
  helper that applies the move to the app's list.

**Done when:** the demo has a task board where cards move by mouse, touch and
keyboard. ✅ The demo's Tasks page (`/tasks`): dragged with the mouse and
moved with the keyboard in the browser; 6 bUnit tests. Touch uses the same
pointer events, but is checked on a real device by the owner.

**Changes from the plan:** the board never changes the cards' DOM while
dragging; a copy follows the pointer and a line marks where the card lands,
so Blazor's rendering stays in step. The app applies the move
(`StilKanbanMove.ApplyTo`).

### Phase 13.2: Command palette

- [ ] `StilCommandPalette`: Ctrl+K search over pages and actions, with
  groups, keyboard navigation and recent choices.

**Done when:** the demo opens a palette with Ctrl+K that goes to any page.

### Phase 13.3: Notification center

- [ ] `StilNotificationCenter`: a bell with a list of unread items, built on
  `IStilToastService`, so a toast can also be kept for later.

**Done when:** a toast in the demo can be found again in the bell.

### Phase 13.4: Recurring events

- [ ] Recurring events in the calendar, agenda and planner: RRULE-style rules
  (daily, weekly on given days, monthly, with an end date or a count, and
  exceptions) expanded by Stil.

**Done when:** a weekly meeting shows in every view, with one occurrence
moved.

### Phase 13.5: Resource view

- [ ] A resource view of the planner: rooms or people as rows over time, with
  the same dragging as the week planner.

**Done when:** the demo books meeting rooms in a resource schedule.

### Phase 13.6: Gantt view

- [ ] A simple Gantt view with dependencies between tasks.

**Done when:** a small project plan shows as a Gantt chart with its
dependencies.

## Phase 14: Markdown, rich text and charts 2

Goal: writing formatted text and showing more kinds of data, both in the Stil
look and without large dependencies. Markdown comes first: it is smaller, it
is easier to store, compare and search than HTML, and the rich text editor
reuses its toolbar.

**Plan:**

- [ ] `Stil.Markdown`, a package of its own on top of Markdig (BSD licence,
  the standard .NET Markdown library), so the core `Stil` package stays
  free of dependencies, as `Stil.Charts` does:
  - `StilMarkdown`: renders Markdown safely (raw HTML off, safe links) with
    Stil's typography: headings, lists, tables, code blocks, quotes, task
    lists. It works in static SSR.
  - `StilMarkdownEditor`: a text area with a Stil toolbar (bold, italic,
    heading, lists, link, code, quote) and keyboard shortcuts, and a preview
    beside it or in a tab. `@bind-Value`, EditForm validation (required, max
    length), and images through `StilFileUpload` (the app stores them and
    returns the URL).
- [ ] `StilRichTextEditor`: bold, italic, headings, lists, links, quotes and
  code, with a Stil toolbar and keyboard shortcuts, on top of
  `contenteditable`. The output is sanitized HTML (an allow-list of tags), or
  Markdown. Works in EditForm (`@bind-Value`, required, max length) and pastes
  cleanly from Word and web pages.
- [ ] `StilRichTextView`: renders saved HTML or Markdown safely, with Stil's
  typography.
- [ ] Charts 2 in Stil.Charts, with the same accessibility as today (summary,
  data table, tooltips):
  - `StilScatterChart`, also as a bubble chart.
  - `StilGauge`, for a value against a target, as an arc or a bar.
  - `StilHeatMap`, for example activity by weekday and hour (it can show the
    calendar's load).
  - Combined charts: bars and a line in one chart, with a second value axis.
- [ ] Docs, demo sections and bUnit tests.

**Done when:** the demo's order page has notes written in Markdown, a
product description written in the rich text editor, and its dashboard shows a gauge, a heat map of orders per hour, and revenue as bars
with a margin line.

---

## Version 2: Stil Sites (after 1.0)

Goal: Stil builds whole websites, not only apps and dashboards: marketing
pages, product sites and content pages, made from Stil blocks and edited
visually. Few Blazor libraries offer this. It is a product rather than a
component, so it comes in four steps that are each useful on their own, and it
starts once phases 11–14 are done and `1.0` has a stable API.

Blocks and the page model (V2.1, V2.2) belong in `Stil`. The builder and the
content tools (V2.3, V2.4) are a package of their own (`Stil.Builder`), so the
core stays a component library.

### Phase V2.1: Section blocks

- [ ] Ready-made page sections in CSS and Blazor: hero, feature grid, pricing
  table, testimonials, logo cloud, call to action, FAQ (on the accordion),
  stats band, team, contact, footer, a header with a mega menu, a blog list
  and an article layout.
- [ ] Each block follows the theme and dark mode, works in static SSR, loads
  fast (no JavaScript where none is needed), and meets WCAG AA.
- [ ] A starter template (`dotnet new stil-site`) for a product site.

**Done when:** a product site with a home, pricing, blog and contact page can
be built from the blocks without custom CSS.

### Phase V2.2: Page model

- [ ] Pages as data: a list of blocks with their settings, stored as JSON.
  `StilPage` renders a page from it, and blocks describe their settings so
  editors can be generated.
- [ ] SEO and sharing: title, description, Open Graph and canonical links
  (`StilSeo`), a sitemap and robots endpoint, and structured data for
  articles.
- [ ] A theme per site (`StilTheme` stored with the pages), and pages per
  language with the localization from Phase 8.

**Done when:** the starter site's pages come from JSON in a database and
score well in a Lighthouse SEO check.

### Phase V2.3: The builder

- [ ] A visual editor in `Stil.Builder`: a canvas with the page, blocks dragged
  in from a library and reordered, and a settings panel generated from each
  block's settings (text, Markdown, images, colors, links).
- [ ] Undo and redo, a desktop and phone preview, drafts and publishing, and
  a history of published versions.
- [ ] Storage through `IStilPageStore`, which the app implements (a database,
  files or a headless CMS); Stil ships an in-memory and a file store.
- [ ] Built with Stil's own components: splitter, wizard, popover, image
  cropper and the Markdown editor.

**Done when:** someone who does not code can build and publish the starter
site's home page in the builder.

### Phase V2.4: Content

- [ ] A media library: upload, crop, alt texts and reuse of images.
- [ ] A form builder: contact and sign-up forms from fields, with submissions
  in the activity log (Phase 12) and e-mail through a service the app plugs in.
- [ ] Blog posts and pages written in Markdown, with authors, tags and
  publishing dates.

**Done when:** the starter site has a blog and a contact form that are run
entirely from the builder.

---

## Ideas for later (no phase yet)

- A file manager on top of the tree view and the data grid.
- A map component (it would need an external map service, so it is not
  planned yet).

---

## Detour: release flow ✅ Done

A small change outside the phase plan: `.github/workflows/release.yml`
publishes `Stil`, `Stil.Charts` and `Stil.Templates` to nuget.org when the
owner pushes a version tag (`v0.1.0`, or `v0.1.0-preview.1` for a
prerelease). It uses nuget.org trusted publishing, so no API key is stored,
takes its release notes from CHANGELOG.md and creates a GitHub release.
Pushes to `main` and pull requests still only build and test; nothing goes
to npm, and the docs site is not deployed.

---

## Owner tasks (not part of a phase)

These need the repository or package owner.

- [x] Rename the GitHub repository from `StilCSS` to `Stil`, then update the
  links (package metadata, READMEs, samples and docs now point at
  `dradwul/Stil`).
- [x] Push and confirm that CI passes on GitHub.
- [x] Set up publishing: a trusted publishing policy on nuget.org for
  `release.yml`, the `nuget` environment and the `NUGET_USER` variable on
  GitHub (see Releases in CONTRIBUTING.md).
- [x] First release: `0.1.0-preview.1`, published to nuget.org from the tag
  `v0.1.0-preview.1` on 2026-10-30.

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
- [ ] Carousel and popover: planned in Phases 10 and 9.
