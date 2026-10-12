# Stil roadmap

Goal: a themeable CSS class library that can replace Bootstrap or Tailwind,
plus a Blazor component library built on it that can replace Radzen or
Blazorise. Each phase ends with something usable. Phases 2 and 3 can overlap
once Phase 1 is done.

## Where we are (0.1.0, Phase 0 done)

- **CSS:** a single stylesheet with `--stil-*` tokens on `:root` for six
  colors, the font and heading spacing. It has `stil-*` classes for
  container, button, form, input, card, alert and table. Base element styles
  (font, headings, links) apply only inside `.stil-scope`.
- **Blazor:** `StilContainer`, `StilCard` and `StilAlert`, plus the shared
  `StilVariant` enum. Extra HTML attributes are passed through and `class`
  values are merged. The library has 12 bUnit tests.
- **Samples:** a plain-HTML showcase and a Blazor demo that shows the same
  sections.
- **Groundwork:** naming rules, scope model and browser support are recorded
  in [CONTRIBUTING.md](CONTRIBUTING.md). CI builds, tests and packs on every
  push and pull request.

### Known issues found in review

| # | Issue | Status |
| --- | --- | --- |
| 1 | Every component set its own `.stil-scope`, which declared the tokens again, so a theme on a wrapper element never reached the components. | ✅ Fixed in Phase 0: tokens are on `:root` and components no longer add `.stil-scope`. |
| 2 | Token names had no prefix (`--primary-color`). | ✅ Fixed in Phase 0: all tokens are `--stil-*`. |
| 3 | White text has 1.5–2.1:1 contrast on every default color except danger (4.6:1). WCAG AA requires at least 4.5:1. | Phase 1 |
| 4 | The variant names were mixed (`error`/`alert`/`warning`/"Danger"). | ✅ Fixed in Phase 0: `primary`, `secondary`, `success`, `danger`, `warning` and `info` everywhere. |
| 5 | Neutral colors are hard-coded (`#ccc`, `#fff`, `#e3e3e3`, `white`), so dark mode is impossible. | Phase 1 |
| 6 | `a` inside `.stil-scope` gets padding, so inline links in text shift. | Phase 2 (`form` became the `stil-form` class in Phase 0) |
| 7 | No cascade layers, so app CSS has to win on specificity. Component selectors dropped from (0,2,0) to (0,1,0) in Phase 0. | Phase 1 |
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

## Phase 1: Tokens and theming engine

Make theming the library's strongest feature.

- [ ] Expand the token set on `:root`:
  - Color scale per brand color.
  - Semantic colors, each with a matching `--stil-color-{variant}-contrast`
    text color.
  - Neutrals (surface, border, text, muted).
  - Spacing scale, radius, shadow, font sizes, z-index, motion.
- [ ] Wrap all CSS in `@layer stil.reset, stil.base, stil.components, stil.utilities`
  so that any unlayered app CSS wins over Stil.
- [ ] Dark mode via `prefers-color-scheme`, plus `data-stil-theme="light|dark"`
  to force a mode.
- [ ] Default palette meets WCAG AA. Add a contrast check to CI.
- [ ] Replace every hard-coded color with a token (fixes known issues 3, 5 and 7).

**Done when:** changing three or four tokens re-themes the showcase and the
demo completely, in both light and dark mode.

## Phase 2: Stil CSS 1.0 (Bootstrap replacement)

- [ ] Split the source into partials (reset, base, layout, components,
  utilities). Build `dist/stil.css` and `dist/stil.min.css` with Lightning CSS
  or PostCSS.
- [ ] Layout: 12-column grid, `stack`/`cluster` helpers, and
  responsive breakpoints (`sm`, `md`, `lg`, `xl`).
- [ ] Utility classes (`stil-mt-2`, `stil-d-flex`, …): spacing, display,
  flex and grid, gap, text, color, sizing, borders and visibility, with
  responsive variants.
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
