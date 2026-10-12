# Stil roadmap

Goal: a themeable CSS class library that can replace Bootstrap or Tailwind,
plus a Blazor component library built on it that can replace Radzen or
Blazorise. Each phase ends with something usable. Phases 2 and 3 can overlap
once Phase 1 is done.

## Where we are (0.1.0)

- **CSS:** a single 140-line stylesheet. It has tokens for six colors and
  classes for container, button, input, card, alert, table, link and form.
  All styles are scoped under `.stil-scope`.
- **Blazor:** `StilContainer`, `StilCard` and `StilAlert`. Extra HTML
  attributes are passed through and `class` values are merged. The library
  has 10 bUnit tests.
- **Samples:** a plain-HTML showcase and a Blazor demo that shows the same
  sections.

### Known issues found in review

| # | Issue | Fixed in |
| --- | --- | --- |
| 1 | Every component sets its own `.stil-scope`, which declares the tokens again. A theme set on a wrapper element (`.my-theme { --primary-color: … }`) therefore never reaches the components. | Phase 1 |
| 2 | Token names have no prefix (`--primary-color`), so they can clash with the host app or other libraries. | Phase 1 |
| 3 | White text has 1.5–2.1:1 contrast on every default color except error (4.6:1). WCAG AA requires at least 4.5:1. | Phase 1 |
| 4 | The variant names are mixed: `bttn-error`/`bttn-alert` vs `alert-error`/`alert-warning`, the warning color is stored in `--alert-color`, and the showcase calls the error color "Danger". | Phase 0 |
| 5 | Neutral colors are hard-coded (`#ccc`, `#fff`, `#e3e3e3`, `white`), so dark mode is impossible. | Phase 1 |
| 6 | Bare elements get opinionated styles inside the scope. `a` gets padding, so inline links in text shift. `form` is always a flex column. | Phase 2 |
| 7 | Selectors like `.stil-scope .bttn` have specificity (0,2,0), which makes them hard to override. No cascade layers are used. | Phase 1 |
| 8 | `StilCard` always renders an `<h2>`. `StilAlert` always has `role="alert"`, which screen readers interrupt for even when the message is static. | Phase 3 |
| 9 | `samples/Stil.Demo/Components/Pages/Home.razor` copies the HTML showcase by hand, so the two will drift apart. | Phase 5 |
| 10 | There is no CI, and nobody has checked that the names `Stil` (NuGet) and `stil` (npm) are free. | Phase 0 |

## Phase 0: Decisions and groundwork

Settle the things that are expensive to change later.

- [ ] Naming rules:
  - Class names. Use short Bootstrap-like names (`btn`) to ease migration, or
    prefixed names (`stil-btn`) to avoid clashes? Rename `bttn` either way.
  - Variant vocabulary. Use `primary`, `secondary`, `success`, `danger`,
    `warning` and `info` everywhere.
  - Token prefix `--stil-*`.
- [ ] Scoping model. Recommended: global by default, as Bootstrap is, with
  `.stil-scope` as an opt-in variant for gradual migration.
- [ ] Browser support target. Recommended: evergreen browsers only, which
  allows `@layer`, `:where()`, `color-mix()`, native `<dialog>` and the
  Popover API.
- [ ] GitHub Actions: build, test and pack on every PR.
- [ ] Rename the GitHub repository from `StilCSS` to `Stil`. Check that
  `Stil` is free on NuGet, and pick an npm name such as `@stil/css` or
  `stil-css`.

**Done when:** the decisions are written into `CONTRIBUTING.md` and CI is
green.

## Phase 1: Tokens and theming engine

Make theming the library's strongest feature.

- [ ] Token set on `:root`, all prefixed `--stil-`:
  - Color scale per brand color.
  - Semantic colors, each with a matching `*-contrast` text color.
  - Neutrals (surface, border, text, muted).
  - Spacing scale, radius, shadow, font family and sizes, z-index, motion.
- [ ] Wrap all CSS in `@layer stil.reset, stil.base, stil.components, stil.utilities`
  so that any unlayered app CSS wins over Stil.
- [ ] Dark mode via `prefers-color-scheme`, plus `data-stil-theme="light|dark"`
  to force a mode. A custom theme is a block of token overrides on any
  element.
- [ ] Default palette meets WCAG AA. Add a contrast check to CI.
- [ ] Stop declaring tokens on `.stil-scope` (fixes known issues 1, 2, 5 and 7).

**Done when:** changing three or four tokens re-themes the showcase and the
demo completely, in both light and dark mode.

## Phase 2: Stil CSS 1.0 (Bootstrap replacement)

- [ ] Split the source into partials (reset, base, layout, components,
  utilities). Build `dist/stil.css` and `dist/stil.min.css` with Lightning CSS
  or PostCSS.
- [ ] Layout: container, 12-column grid, `stack`/`cluster` helpers, and
  responsive breakpoints (`sm`, `md`, `lg`, `xl`).
- [ ] Utility classes: spacing, display, flex and grid, gap, text, color,
  sizing, borders and visibility, with responsive variants.
- [ ] Components:
  - Buttons: solid, outline and ghost styles; sizes; disabled and loading
    states.
  - Forms: input, select, checkbox, radio, switch, input group, validation
    states.
  - Content: card, alert, badge, table (striped, hover, compact).
  - Navigation: nav, navbar, breadcrumb, pagination, tabs.
  - Overlays: dialog (`<dialog>`), dropdown and tooltip (Popover API).
  - Feedback: progress, spinner, toast.
- [ ] Publish to npm and jsDelivr, and keep shipping the stylesheet in the
  NuGet package.
- [ ] A Bootstrap migration guide with a class mapping table.

**Done when:** a typical Bootstrap admin page can be rebuilt using only Stil
CSS.

## Phase 3: Stil for Blazor core (forms and basics)

- [ ] `services.AddStil(options => …)` and an optional `StilTheme` C# object
  that renders token overrides.
- [ ] `StilButton`, `StilBadge`, `StilIcon` (an icon-set strategy is still to
  be decided).
- [ ] Form inputs built on `InputBase<T>`, so they work with `EditForm` and
  validation: text box, text area, numeric, checkbox, switch, radio group,
  select, date picker.
- [ ] Improved `StilCard` (header, body and footer slots; heading level) and
  `StilAlert` (dismissible; `role` set by severity).
- [ ] `StilTabs`, `StilDropdown`, `StilTooltip`.
- [ ] `StilDialog` with `IStilDialogService`, and `StilToast` with
  `IStilToastService`.
- [ ] bUnit tests for every component, including keyboard and ARIA
  behaviour. Every component must work in static SSR, Server and WebAssembly.

**Done when:** a CRUD form with validation, dialogs and toasts can be built
without Radzen.

## Phase 4: Data components (Radzen replacement)

- [ ] `StilDataGrid`: column templates, sorting, paging, filtering,
  selection, virtualization, and server-side data through an `ItemsProvider`.
- [ ] Autocomplete, combo box, multi-select, date range, file upload.
- [ ] App shell: sidebar layout, menu, tree view.
- [ ] Charts, possibly as a separate package.

**Done when:** an existing Radzen-based internal app can be migrated screen by
screen.

## Phase 5: 1.0 release and ecosystem

- [ ] A documentation site built from the Blazor demo. The CSS and Blazor
  examples come from a single source (fixes known issue 9).
- [ ] An interactive theme builder: edit tokens live, then copy the CSS or C#
  theme.
- [ ] Playwright visual regression tests for the CSS and the components.
- [ ] A `dotnet new stil-app` template. Migration guides from Radzen and
  Blazorise.
- [ ] Semantic versioning with a changelog. Publish `1.0` once the APIs have
  stayed unchanged for one minor release.
