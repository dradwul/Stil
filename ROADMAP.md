# Stil roadmap

Goal: a themeable CSS class library that can replace Bootstrap or Tailwind,
plus a Blazor component library built on it that can replace Radzen or
Blazorise. Each phase ends with something usable. Phases 2 and 3 can overlap
once Phase 1 is done.

## Where we are (0.1.0, Phases 0 and 1 done)

- **CSS:** a single stylesheet in cascade layers. It has a full `--stil-*`
  token set on `:root` (colors, neutrals, typography, spacing, shape, focus,
  z-index, motion), light and dark mode through `data-stil-theme`, and
  derived colors that stay WCAG AA readable for any brand color. It has
  `stil-*` classes for container, button, form, input, card, alert and
  table, plus `stil-bg-*`. Base element styles (font, text color, headings,
  links) apply only inside `.stil-scope`.
- **Blazor:** `StilContainer`, `StilCard` and `StilAlert`, plus the shared
  `StilVariant` enum. Extra HTML attributes are passed through and `class`
  values are merged. The library has 12 bUnit tests.
- **Samples:** a plain-HTML showcase and a Blazor demo that shows the same
  sections.
- **Groundwork:** naming rules, scope model and browser support are recorded
  in [CONTRIBUTING.md](CONTRIBUTING.md). CI checks contrast, then builds,
  tests and packs on every push and pull request.
- **Samples:** both showcases have a theme picker (light, dark or auto, and
  four palettes).

### Known issues found in review

| # | Issue | Status |
| --- | --- | --- |
| 1 | Every component set its own `.stil-scope`, which declared the tokens again, so a theme on a wrapper element never reached the components. | ✅ Fixed in Phase 0: tokens are on `:root` and components no longer add `.stil-scope`. |
| 2 | Token names had no prefix (`--primary-color`). | ✅ Fixed in Phase 0: all tokens are `--stil-*`. |
| 3 | White text had 1.5–2.1:1 contrast on every default color except danger. | ✅ Fixed in Phase 1: text on colors is picked automatically (at least 4.58:1), and CI checks every token pair. |
| 4 | The variant names were mixed (`error`/`alert`/`warning`/"Danger"). | ✅ Fixed in Phase 0: `primary`, `secondary`, `success`, `danger`, `warning` and `info` everywhere. |
| 5 | Neutral colors were hard-coded (`#ccc`, `#fff`, `#e3e3e3`, `white`), so dark mode was impossible. | ✅ Fixed in Phase 1: all colors are tokens, and neutrals use `light-dark()`. |
| 6 | `a` inside `.stil-scope` gets padding, so inline links in text shift. | Phase 2 (`form` became the `stil-form` class in Phase 0) |
| 7 | There were no cascade layers, so app CSS had to win on specificity. | ✅ Fixed in Phase 1: every rule is in a `stil.*` layer, so unlayered app CSS always wins. |
| 8 | `StilCard` always renders an `<h2>`. `StilAlert` always has `role="alert"`, which screen readers interrupt for even when the message is static. | Phase 3 |
| 9 | `samples/Stil.Demo/Components/Pages/Home.razor` copies the HTML showcase by hand, so the two will drift apart. | Phase 5 |
| 10 | There was no CI, and nobody had checked the package names. | ✅ Fixed in Phase 0: CI added. `Stil` is free on NuGet and `stil-css` is free on npm. |

## Phase 0: Decisions and groundwork ✅

- [x] Naming rules: `stil-` class prefix (`bttn` became `stil-btn`, and so
  on), `--stil-` token prefix, a single variant vocabulary, and Blazor
  components named `Stil{Component}`, which render `stil-{component}`.
- [x] Scope model: `stil-*` classes work anywhere. Base element styles are
  opt-in with `.stil-scope` (on `<body>` for a whole app).
- [x] Browser support: the latest two versions of Chrome, Edge, Firefox and
  Safari.
- [x] GitHub Actions: build, test, pack, and verify the package contents.
- [x] Package names checked: NuGet `Stil` and npm `stil-css` are free.
- [ ] Rename the GitHub repository from `StilCSS` to `Stil` (needs the repo
  owner), then update the links in `src/Stil.Blazor/README.md`.

## Phase 1: Tokens and theming engine ✅

- [x] Full token set on `:root`:
  - Semantic colors, plus optional `--stil-color-{variant}-contrast` and
    `--stil-color-accent` overrides.
  - Neutrals (body background, surface, muted surface, text, muted text,
    border, strong border).
  - Font sizes, spacing scale, radius, shadow, focus ring, z-index and
    motion. Motion is turned off under `prefers-reduced-motion`.
- [x] Derived colors are computed where they are used, so a theme on any
  wrapper reaches them: automatic text color on colored backgrounds, an
  accent for links and focus rings, and hover shades. This replaces the
  planned fixed color scale per brand color. A full 50–900 scale can be
  added in Phase 2 if the utilities need it.
- [x] Every rule is in `@layer stil.tokens, stil.reset, stil.base,
  stil.components, stil.utilities`.
- [x] Light and dark mode through `data-stil-theme="light|dark|auto"` and
  `light-dark()`. Stil defaults to light, so it never switches a host page
  to dark on its own.
- [x] `tools/check-contrast.mjs` checks all token pairs and the derived
  colors for WCAG AA in CI.
- [x] No hard-coded colors left in components or in the showcase styles.
- [x] The `stil-bg-{variant}` utilities were brought forward from Phase 2.

**Verified:** switching a palette (two tokens) and the mode re-themes both
showcases. Tested in the browser with teal, purple and sunset palettes in
light and dark mode. In the Blazor demo, the host footer outside the themed
`<main>` stays unchanged.

## Phase 2: Stil CSS 1.0 (Bootstrap replacement)

- [ ] Split the source into partials (tokens, reset, base, layout, components,
  utilities). Build `dist/stil.css` and `dist/stil.min.css` with Lightning CSS
  or PostCSS.
- [ ] Fill the empty `stil.reset` layer: box-sizing, margins, and font
  inheritance for form elements.
- [ ] Layout: 12-column grid, `stack`/`cluster` helpers, and
  responsive breakpoints (`sm`, `md`, `lg`, `xl`).
- [ ] Utility classes (`stil-mt-2`, `stil-d-flex`, …): spacing, display,
  flex and grid, gap, text, color, sizing, borders and visibility, with
  responsive variants. `stil-bg-*` already exists.
- [ ] Components:
  - Buttons: solid, outline and ghost styles; sizes; disabled and loading
    states.
  - Forms: input, select, checkbox, radio, switch, input group, validation
    states.
  - Content: card, alert, badge, table (striped, hover, compact).
  - Navigation: nav, navbar, breadcrumb, pagination, tabs.
  - Overlays: dialog (`<dialog>`), dropdown and tooltip (Popover API).
  - Feedback: progress, spinner, toast.
- [ ] Publish to npm as `stil-css` and to jsDelivr, and keep shipping the stylesheet in the
  NuGet package.
- [ ] A Bootstrap migration guide with a class mapping table.

**Done when:** a typical Bootstrap admin page can be rebuilt using only Stil
CSS.

## Phase 3: Stil for Blazor core (forms and basics)

- [ ] `services.AddStil(options => …)` and an optional `StilTheme` C# object
  that renders token overrides.
- [ ] `StilButton`, `StilBadge`, `StilIcon`, and a shared `StilSize` enum (an icon-set strategy is still to
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
