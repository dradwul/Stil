# Contributing to Stil

This file records the conventions both parts of Stil follow. Do not change a
rule here without also updating the code that depends on it.

## Naming

### CSS classes

Every class starts with `stil-`. This avoids clashes with Bootstrap,
Tailwind and the host app, so Stil can be added to an existing site one page
at a time.

| Kind | Pattern | Example |
| --- | --- | --- |
| Component | `stil-{component}` | `stil-btn`, `stil-card`, `stil-alert` |
| Part of a component | `stil-{component}-{part}` | `stil-card-title`, `stil-card-footer` |
| Variant | `stil-{component}-{variant}` | `stil-btn-danger`, `stil-alert-info` |
| Size | `stil-{component}-{size}` | `stil-btn-sm`, `stil-btn-lg` |
| Utility | Bootstrap's utility name with the prefix | `stil-mt-2`, `stil-d-flex`, `stil-justify-content-between` |
| Responsive | Breakpoint after the name: `stil-{name}-{bp}-{value}` | `stil-d-md-none`, `stil-col-lg-4` |

- Component names are short, lowercase and hyphenated. Common
  abbreviations are fine (`btn`). Spell everything else out.
- Follow Bootstrap's name when Stil has the same concept. That keeps the
  [migration guide](src/Stil.Css/MIGRATING-FROM-BOOTSTRAP.md) short.
- Breakpoints are `sm` 576px, `md` 768px, `lg` 992px and `xl` 1200px (min-width).
- States use native attributes and pseudo-classes (`:disabled`,
  `[aria-invalid="true"]`, `[aria-expanded="true"]`), not state classes.

### Variants and sizes

Every component that comes in colors supports all six variants with these
exact names. Do not use `error`, `alert` or `default`.

`primary`, `secondary`, `success`, `danger`, `warning`, `info`

The sizes are `sm` and `lg`. Medium is the default and has no class.

The variant classes (`stil-btn-danger`, `stil-alert-info`, …) are generated.
To give a new component variants, add it to `variantComponents` in
`src/Stil.Css/build/generate.mjs`. Its base rule sets the defaults; the
generated variant rules come after all components and override them.

### CSS custom properties (tokens)

Every public custom property starts with `--stil-` and follows
`--stil-{category}-{name}[-{variant}]`, for example `--stil-color-primary`,
`--stil-space-4` or `--stil-radius-md`. Tokens are declared on `:root`.
Components read them and never declare them, so a theme set on any ancestor
element reaches everything inside it.

- Neutral colors are written as `light-dark(light, dark)`. Brand and
  semantic colors are the same in both modes.
- Never use a hard-coded color in a component. If no token fits, add one.
- **Derived colors are computed where they are used, never on `:root`.** A
  custom property that refers to another one is resolved on the element that
  declares it, so a value derived on `:root` would ignore a theme set on a
  wrapper. Examples are text on a colored background, the accent and hover
  shades. The shared formulas (`--_fg`, `--_ink`, `--_accent`) live in
  `src/derived.css`, on every element with a `stil-` class. A component just
  sets `--_bg` and reads the result.
- Every derived color gets a public override token as the first choice:
  `var(--stil-color-accent, <formula>)`.

### Private custom properties

Custom properties starting with `--_` are internal to Stil and can change
at any time:

| Property | Meaning |
| --- | --- |
| `--_bg` | The component's color (set by the component and its variants) |
| `--_contrast` | Optional text-color override, from `--stil-color-{variant}-contrast` |
| `--_fg` | Readable text on `--_bg` |
| `--_ink` | `--_bg` adjusted to be readable on the neutral surfaces |
| `--_accent` | The primary color adjusted the same way |
| `--_pad`, `--_pad-x`, `--_pad-y` | Component padding that child parts reuse |

- A component sets defaults for every private property it reads, so it
  never inherits one from a colored ancestor. For example, a plain
  `stil-btn` inside a `stil-alert-danger` must not turn red.
- Variant rules come after the component's base rule, so they override
  those defaults.

### Cascade layers

All rules live in `@layer stil.tokens, stil.reset, stil.base,
stil.components, stil.utilities`. Any unlayered CSS in the host app wins
over Stil, so users can override without fighting specificity. Never use
`!important` in Stil.

The source files do not declare their layer. `build/build.mjs` wraps each
file in its layer, so the order of the file list there is the cascade order.
The reset uses `:where()` (zero specificity) and only targets Stil elements
and content inside `.stil-scope`.

### Color modes

Stil renders light unless `data-stil-theme` is set to `light`, `dark` or
`auto` on an element. This keeps Stil from switching a host page to dark by
accident. Components must look right in both modes, so check both in the
showcases (each has a theme picker).

### Blazor components

- **Naming:** a component is named `Stil` plus the full name in PascalCase,
  and it renders the matching CSS block: `StilButton` renders `stil-btn`,
  `StilListItem` renders `stil-list-item`. Public types live in
  `Stil.Blazor.Components`; services and options live in `Stil.Blazor`.
- **Shared parameters:**

  | Parameter | Type |
  | --- | --- |
  | `Variant` | `StilVariant` |
  | `Size` | `StilSize` (Medium is the default and adds no class) |
  | `Appearance` | `StilButtonAppearance` (buttons and dropdown toggles) |
  | `Subtle` | `bool` (badges and alerts) |
  | `Disabled` | `bool` |
  | `ChildContent` | `RenderFragment?` |

- **Base class:** inherit `StilComponentBase`. It captures unmatched
  attributes for the root element. Build the class with
  `Classes("stil-x", …)`, which appends the consumer's `class` instead of
  replacing it. Put `@attributes` before `class=` in markup.
- **Form fields:** inherit `StilInputBase<TValue>` and wrap Blazor's
  built-in input (`InputText`, `InputNumber`, …) in `StilField`. For fields,
  `class` and `style` go on the field wrapper and every other attribute goes
  on the control. `InputClass` adds classes to the control.
- **Accessibility:**
  - Map states to ARIA: `aria-busy`, `aria-pressed`, `aria-current`,
    `aria-selected`, `aria-invalid`.
  - Generate ids with `StilIds.Next(...)`.
  - Implement keyboard patterns from the
    [WAI-ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/), for
    example the tabs' arrow keys.
- **Render modes:**
  - Components must render correctly with static SSR.
  - Behaviour that needs events or JS interop is documented in the Blazor
    README's render-mode table.
  - Prefer HTML that works without interactivity (`<details>` dropdowns,
    links) and let `stil.js` enhance it.
- **JavaScript:** shared behaviour goes in `src/Stil.Css/src/stil.js`, so
  plain-HTML users get it too. `Stil.lib.module.js` loads it automatically.
  Components import it with `import("./_content/Stil/stil.js")` for interop.
- **<details>-based components** (dropdown, accordion) keep `@bind-Open` in
  sync through the `stiltoggle` event (registered in `Stil.lib.module.js`).
  Razor cannot bind `@onstiltoggle` inside this assembly, so add the handler
  as an attribute: `attributes["onstiltoggle"] = EventCallback.Factory.Create<StilToggleEventArgs>(this, …)`.
  See `StilDropdown`.
- **Icons:** add an icon to `src/Stil.Css/src/icons.json` (inner SVG markup
  on a 24 × 24 grid, 2px round stroke). Add a matching constant to
  `StilIcons`, and rebuild the CSS. A test fails if the constants and the JSON
  differ.
- **Tests:** every public parameter, ARIA attribute and keyboard path gets a
  bUnit test in `tests/Stil.Blazor.Tests`.

## Scope model

Stil is split into two layers:

| Layer | Selector | Applies |
| --- | --- | --- |
| **Components** | `.stil-*` classes | Anywhere. No wrapper needed. |
| **Base** | `.stil-scope h1`, `.stil-scope a`, … | Only inside an element with `class="stil-scope"`. |

- For a whole Stil app, put `class="stil-scope"` on `<body>`, which works
  like Bootstrap's global reboot.
- To migrate one page or widget at a time, put it on a wrapper element. The
  host page's own elements stay untouched.
- Blazor components do **not** add `stil-scope`. Styling bare elements
  (headings, links) belongs to the page, not to a component.

## Browser support

Stil targets the latest two versions of Chrome, Edge, Firefox and Safari,
including iOS Safari. There is no Internet Explorer and no legacy Edge.

Features available in all of these can be used freely, including `@layer`,
`:is()`/`:where()`, `color-mix()`, `light-dark()`, relative color syntax
(`color(from …)`), custom properties, native `<dialog>` and the Popover API. Check anything newer on [Baseline](https://web.dev/baseline)
first.

## Package names

| Package | Name | Status |
| --- | --- | --- |
| NuGet (Blazor + stylesheet) | `Stil` | Free on nuget.org. Not published yet. |
| npm (CSS only) | `stil-css` | Free on npm. Prepared in `src/Stil.Css/package.json`. Not published yet. `stil` is taken by another package. |

## Workflow

1. Make CSS changes in `src/Stil.Css/src/` (or `build/generate.mjs` for
   generated classes). Run `npm run watch` in `src/Stil.Css` while you
   work, and commit the rebuilt `dist/` with your change. The Blazor project
   serves `dist/` directly, so do not copy the file anywhere else.
2. Show every CSS class in `samples/Stil.Html/index.html`. Write each
   example once inside `<div class="example">`; `examples.js` displays its
   markup. Check it in light and dark mode. If a change matters for a typical
   app page, also check `samples/Stil.Html/admin.html`.
3. Every Blazor component gets bUnit tests in `tests/Stil.Blazor.Tests`
   and an example in `samples/Stil.Demo`.
4. Run `npm run check` (in `src/Stil.Css`), `dotnet build Stil.sln` and
   `dotnet test Stil.sln` before pushing. CI (`.github/workflows/ci.yml`)
   runs the same checks plus pack on every push to `main` and on every pull
   request. It fails if `dist/` is stale, if a token pair drops below WCAG
   AA, or if the package is missing the stylesheet.
5. When you change a color formula in `src/derived.css`, update the mirror
   of it in `tools/check-contrast.mjs`. The script fails if the two drift
   apart.
6. Commit messages follow
   [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`,
   `fix:`, `docs:`, `test:`, `chore:`).
