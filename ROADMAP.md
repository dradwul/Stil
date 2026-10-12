# Stil roadmap

Goal: a themeable CSS class library that can replace Bootstrap or Tailwind,
plus a Blazor component library built on it that can replace Radzen or
Blazorise. Each phase ends with something usable. Phases 2 and 3 can overlap
once Phase 1 is done.

## Where we are (0.1.0, Phases 0–2 done)

- **Stil CSS:** an npm package (`stil-css`, not yet published). It is built
  with Lightning CSS from partials into `dist/stil.css` and `stil.min.css`
  (about 10 kB gzipped).
  - Cascade layers.
  - A full `--stil-*` token set.
  - Light and dark mode.
  - Derived colors that stay WCAG AA for any brand color.
  - A scoped reset and base.
  - A 12-column grid and layout helpers.
  - Bootstrap-equivalent components and about 900 utility classes with
    responsive variants.
  - A [Bootstrap migration guide](src/Stil.Css/MIGRATING-FROM-BOOTSTRAP.md).
- **Blazor:** `StilContainer`, `StilCard` and `StilAlert`, plus the shared
  `StilVariant` enum. Extra HTML attributes pass through to the element,
  and a `class` you supply is merged with the component's own. The library
  has 12 bUnit tests.
- **Samples:**
  - `samples/Stil.Html/index.html` shows every class. Each example's
    markup is shown from the live element, so the two cannot drift apart.
  - `admin.html` is a full dashboard built only with Stil CSS.
  - The Blazor demo shows the components.
  - All three have a theme picker.
- **Groundwork:**
  - [CONTRIBUTING.md](CONTRIBUTING.md) records the naming rules, scope
    model, browser support and workflow.
  - On every push and pull request, CI checks that `dist/` is up to date
    and that contrast passes, then builds, tests and packs.

### Known issues found in review

| # | Issue | Status |
| --- | --- | --- |
| 1 | Every component set its own `.stil-scope`, which declared the tokens again, so a theme on a wrapper element never reached the components. | ✅ Fixed in Phase 0: tokens are on `:root` and components no longer add `.stil-scope`. |
| 2 | Token names had no prefix (`--primary-color`). | ✅ Fixed in Phase 0: all tokens are `--stil-*`. |
| 3 | White text had 1.5–2.1:1 contrast on every default color except danger. | ✅ Fixed in Phase 1: text on colors is picked automatically (at least 4.58:1), and CI checks every token pair. |
| 4 | The variant names were mixed (`error`/`alert`/`warning`/"Danger"). | ✅ Fixed in Phase 0: `primary`, `secondary`, `success`, `danger`, `warning` and `info` everywhere. |
| 5 | Neutral colors were hard-coded (`#ccc`, `#fff`, `#e3e3e3`, `white`), so dark mode was impossible. | ✅ Fixed in Phase 1: all colors are tokens, and neutrals use `light-dark()`. |
| 6 | `a` inside `.stil-scope` got padding, so inline links in text shifted. | ✅ Fixed in Phase 2: links are underlined and have no padding. Navigation uses `stil-nav-link`. |
| 7 | There were no cascade layers, so app CSS had to win on specificity. | ✅ Fixed in Phase 1: every rule is in a `stil.*` layer, so unlayered app CSS always wins. |
| 8 | `StilCard` always renders an `<h2>`. `StilAlert` always has `role="alert"`, which screen readers interrupt for even when the message is static. | Phase 3 |
| 9 | `samples/Stil.Demo/Components/Pages/Home.razor` copies the HTML showcase by hand, so the two will drift apart. | Partly fixed in Phase 2: the HTML showcase now shows each example's markup from the live element. The Blazor demo will be rebuilt around the components in Phase 3. |
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

## Phase 2: Stil CSS 1.0 (Bootstrap replacement) ✅

- [x] Sources split into partials in `src/Stil.Css/src/`. `build/build.mjs`
  (Lightning CSS) writes `dist/stil.css` and `dist/stil.min.css`.
  `build/generate.mjs` generates the grid columns, variants and utilities.
  `dist/` is committed, and CI fails if it is out of date.
- [x] A scoped reset (box-sizing, margins, form fonts, images, fieldsets).
- [x] A new base: typography, links (underlined instead of padded), code,
  `kbd`, `hr`, `blockquote`, `mark`.
- [x] Layout: `stil-container` (plus `-fluid`), a 12-column `stil-grid`
  with responsive `stil-col-*`, `stil-stack`, `stil-cluster`,
  `stil-with-sidebar`. Breakpoints are sm/md/lg/xl.
- [x] About 900 utilities with Bootstrap names and responsive variants:
  display, flex, alignment, gap, spacing, sizing, text, color, borders,
  shadow, position and visibility.
- [x] Components:
  - Buttons: solid, outline, ghost, sizes, icon, disabled, loading
    (`aria-busy`), toggle (`aria-pressed`), group, close.
  - Forms: field, label, hint, error, input, select, check, radio, switch,
    input group, `aria-invalid`.
  - Card (header, flush children), alert (subtle, dismissible), badge,
    avatar, table (striped, hover, compact, bordered, responsive).
  - Navigation: nav, pills, vertical, navbar, breadcrumb, pagination,
    tabs.
  - Dialog (`<dialog>`), dropdown (`<details>`, no JavaScript), tooltip
    (CSS only), progress, spinner, toast.
- [x] A [Bootstrap migration guide](src/Stil.Css/MIGRATING-FROM-BOOTSTRAP.md).
- [x] The npm package is prepared (`src/Stil.Css/package.json`, name
  `stil-css`).
- [ ] **Publish** to npm, which also makes it available on jsDelivr. This
  needs the package owner: run `npm publish` in `src/Stil.Css`, or add a
  release workflow with an `NPM_TOKEN` secret.

**Verified:** `samples/Stil.Html/admin.html` is built only with Stil CSS.
It has a navbar, sidebar, stat cards, an orders table with menus and
pagination, a settings form with tabs and validation, a confirm dialog and a
toast. No custom CSS was needed. It was checked in light and dark mode,
narrow and wide, with no horizontal overflow.

**Changes from the plan:**

- The dropdown uses `<details>` instead of the Popover API. CSS anchor
  positioning is not yet available in all target browsers.
- The tooltip is pure CSS.

### Gaps found while building the admin page

| Gap | Planned for |
| --- | --- |
| Icons (no icon set; inline SVG works) | Phase 3 (`StilIcon` and an icon strategy) |
| Mobile navigation: navbar toggle, collapsible or offcanvas sidebar | Phase 3 (needs behaviour), with CSS in Phase 2.x |
| Dropdowns closing on an outside click or Escape | Phase 3 (`StilDropdown`) |
| Accordion, list group, offcanvas, popover, carousel | Phase 2.x (CSS), and Phase 3/4 (components) |
| Fixed width/max-width scale, sticky offsets below a sticky navbar | Phase 2.x |
| Tooltips on elements that already use `::after` (dropdown summary, busy button) | Phase 2.x (move to a popover-based tooltip once anchor positioning is available) |
| Neutral or "on-color" button variant | Phase 2.x. Meanwhile, `stil-btn-ghost stil-text-inherit` works on colored bars. |
| Striped or animated progress bars | Phase 2.x |
| KPI/stat card pattern and charts | Phase 4 |

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
