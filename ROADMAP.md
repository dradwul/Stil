# Stil roadmap

Goal: a themeable CSS class library that can replace Bootstrap or Tailwind,
plus a Blazor component library built on it that can replace Radzen or
Blazorise. Each phase ends with something usable.

## Overview

| Phase | Content | Status |
| --- | --- | --- |
| 0 | Decisions and groundwork | ✅ Done |
| 1 | Tokens and theming engine | ✅ Done |
| 2 | Stil CSS 1.0 (Bootstrap replacement) | ✅ Done |
| 3 | Stil for Blazor core (forms, dialogs, toasts) | ✅ Done |
| 4 | Stil for Blazor: a component for every Stil CSS component | ⏭ Next |
| 5 | Data components (Radzen replacement) | ⏸ On hold (since 2026-10-12) |
| 6 | 1.0 release and ecosystem | Planned |

A phase is done when every item in it is checked. Items that need the
repository owner, and polish with no date yet, are listed separately at the
end, so they never keep a phase open.

## Where we are (0.1.0)

- **Stil CSS:** an npm package (`stil-css`, not yet published). It is built
  with Lightning CSS from partials into `dist/stil.css` and `stil.min.css`
  (about 10 kB gzipped), plus the optional 3 kB `stil.js`. It has:
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
  - About 40 components: buttons, badges, lists, cards, alerts, form fields
    with EditForm validation, tabs, dropdowns, tooltips, progress, spinners,
    dialogs and toasts.
  - `IStilDialogService` and `IStilToastService`.
  - Theming with `AddStil()`, `StilTheme`, `StilHead` and
    `StilThemeProvider`.
  - 366 bUnit tests.
- **Samples:**
  - `samples/Stil.Html/index.html` shows every CSS class.
  - `samples/Stil.Html/admin.html` is a dashboard built only with Stil CSS.
  - `samples/Stil.Demo` is an interactive Blazor app with a component
    gallery and an orders CRUD page.
- **Groundwork:** [CONTRIBUTING.md](CONTRIBUTING.md) holds the conventions.
  CI checks the CSS build and contrast, then builds, tests and packs.

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

All of this was tested in the browser, in light and dark mode. No Radzen was
needed.

**Changes from the plan:**

- The form fields wrap Blazor's built-in inputs instead of reimplementing
  `InputBase<T>`.
- `StilIcon` moved to Phase 4, because the icon strategy is still open.

---

## Phase 4: A Blazor component for every Stil CSS component ⏭ Next

Goal: everything Stil CSS can render also has a Blazor component, so Blazor
users rarely need to write `stil-*` classes by hand. The parameter names
below are proposals and are settled when each component is built.

### Navigation

| Component | Proposed parameters | Notes |
| --- | --- | --- |
| `StilBreadcrumbs` (+ `StilBreadcrumbItem`) | `Items` (Text, Href), or `<StilBreadcrumbItem Text Href />` as child content; `Label` (default "Breadcrumb") | The last item gets `aria-current="page"` and is not a link. |
| `StilPagination` | `@bind-Page` (1-based); `PageCount`, or `TotalItems` + `PageSize`; `MaxVisiblePages` (ellipsis); `ShowFirstLast`; `PreviousText`/`NextText`; `Size`; `HrefTemplate` (for example `"orders?page={0}"`); `Label` | With `HrefTemplate` it renders links, which also work in static SSR. |
| `StilNav` + `StilNavLink` | `Vertical`, `Pills`, `Label`; `StilNavLink`: `Href`, `Match`, `Badge` | `StilNavLink` wraps Blazor's `NavLink`, so `aria-current` follows the route. |
| `StilNavbar` | `Brand` or `BrandText`/`BrandHref`, `ChildContent`, `End`, `Sticky`, `Variant`, `CollapseBelow` | Needs a navbar toggle in CSS and `stil.js` for mobile. |
| `StilTabs`: extend | `@bind-ActiveId` (key instead of index), `Appearance` (Underline, Pills), `KeepAlive` (keep inactive panels in the DOM), `StilTab Href` (link tabs for routing) | The basic component exists from Phase 3. |
| `StilDropdown`: extend | `@bind-Open`, `Items` (data-driven: Text, Href/OnClick, Disabled, divider), `Placement`, `Disabled`; arrow-key navigation in the menu (WAI-ARIA menu button) | The basic component exists from Phase 3. |

### Content and layout

| Component | Proposed parameters | Notes |
| --- | --- | --- |
| `StilTable<TItem>` + `StilColumn` | `Items`; `<StilColumn Title Field Template Align>`; `Striped`, `Hover`, `Compact`, `Bordered`, `Responsive`, `Caption`, `EmptyContent` | A lightweight table. Sorting, paging and filtering belong to `StilDataGrid` in Phase 5. |
| `StilAvatar` | `Name` (initials are computed), `ImageSrc`, `Alt`, `Size` | |
| `StilAccordion` + `StilAccordionItem` | `Title`, `@bind-Open`, `Multiple` (allow several open) | Built on `<details>`, so it works in static SSR. Needs `stil-accordion` CSS. |
| `StilGrid` + `StilCol` | `Gap`; `StilCol`: `Span`, `SpanSm`, `SpanMd`, `SpanLg`, `SpanXl` | |
| `StilStack`, `StilCluster` | `Gap`; `StilCluster`: `Justify`, `Align` | |
| `StilSidebarLayout` | `Sidebar`, `ChildContent`, `SidebarWidth` | Renders `stil-with-sidebar`. |

### Forms and actions

| Component | Proposed parameters | Notes |
| --- | --- | --- |
| Input groups | `Prepend` and `Append` (text or `RenderFragment`) on `StilTextBox`, `StilNumeric` and `StilSelect` | Renders `stil-input-group` inside the field, with the label, hint and errors outside it. |
| `StilButtonGroup` | `Label`, `Size` | `role="group"` |
| `StilCloseButton` | `Label`, `OnClick` | |

### Overlays and icons

| Component | Proposed parameters | Notes |
| --- | --- | --- |
| `StilDrawer` (offcanvas) | `@bind-Open`, `Position` (Start, End), `Title`, `Footer` | A native `<dialog>`, like `StilDialog`. Needs `stil-drawer` CSS. It also solves the mobile sidebar. |
| `StilIcon` | `Name`, `Size`, `Label` (otherwise `aria-hidden`) | Icon strategy (decided 2026-10-15): a built-in set below, plus any icon set the app plugs in. |

### Icon strategy (decided 2026-10-15)

- **Built-in set:** about 20 icons drawn by Stil, MIT-licensed like the rest
  of the project.
  - Inline SVG on a 24 × 24 grid with a 2px round stroke in `currentColor`,
    so the icons follow themes and dark mode.
  - Stil's own components use them (chevrons, close, menu, check,
    plus/minus, search, more, info/success/warning/danger, external link).
  - No icon font, no external dependency, no third-party license to ship.
- **Plug in any set:** `StilIcon` also renders icons from other sets, through
  a registered `IStilIconProvider` (`AddStil(o => o.Icons = …)`) or a
  `RenderFragment`. Material Symbols (Apache 2.0), Lucide (ISC), Bootstrap
  Icons (MIT) and Heroicons (MIT) all allow commercial use. The app brings
  the set it wants, along with that set's license.
- **Plain CSS:** the same SVGs are available as `stil-icon-*` CSS mask
  classes, so plain-HTML users get them too.

### Done when

- [ ] Every component in Stil CSS has a Blazor counterpart, or a documented
  reason why not. The Blazor README has a table mapping each CSS class to its
  component.
- [ ] The demo gallery shows every component.
- [ ] bUnit tests cover the parameters, ARIA and keyboard behaviour.

## Phase 5: Data components (Radzen replacement) ⏸ On hold

Paused on 2026-10-12. Resumes after Phase 4.

- [ ] `StilDataGrid`: column templates, sorting, paging, filtering,
  selection, virtualization, and server-side data through an `ItemsProvider`.
  It builds on `StilTable` and `StilPagination` from Phase 4.
- [ ] Autocomplete, combo box, multi-select, date range and file upload.
- [ ] Tree view and menu.
- [ ] A KPI/stat card pattern.
- [ ] Charts, possibly in a separate package.

**Done when:** an existing Radzen-based internal app can be migrated screen
by screen.

## Phase 6: 1.0 release and ecosystem

- [ ] A documentation site built from the Blazor demo.
- [ ] An interactive theme builder: edit tokens live, then copy the CSS or the
  C# `StilTheme`.
- [ ] Playwright visual regression tests for the CSS and the components.
- [ ] A `dotnet new stil-app` template, and migration guides from Radzen and
  Blazorise.
- [ ] Semantic versioning with a changelog. `1.0` is published once the APIs
  have stayed unchanged for one minor release.

---

## Owner tasks (not part of a phase)

These need the repository or package owner.

- [ ] Rename the GitHub repository from `StilCSS` to `Stil`, then update the
  links in `src/Stil.Blazor/README.md` and `src/Stil.Css/package.json`.
- [ ] Push and confirm that CI passes on GitHub. It has only been run
  locally so far.
- [ ] Publish `stil-css` to npm (`npm publish` in `src/Stil.Css`, or a release
  workflow with an `NPM_TOKEN` secret). That also makes it available on
  jsDelivr.
- [ ] Reserve or publish the `Stil` package on NuGet.

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
