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
| Utility (Phase 2) | `stil-{property}-{value}` | `stil-mt-2`, `stil-d-flex` |

- Component names are short, lowercase and hyphenated. Common
  abbreviations are fine (`btn`). Spell everything else out.
- States use native attributes and pseudo-classes (`:disabled`,
  `[aria-invalid="true"]`, `[aria-expanded="true"]`), not state classes.

### Variants and sizes

Every component that comes in colors supports all six variants with these
exact names. Do not use `error`, `alert` or `default`.

`primary`, `secondary`, `success`, `danger`, `warning`, `info`

The sizes are `sm` and `lg`. Medium is the default and has no class.

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
  shades. Put the formula in the `stil.tokens` layer, on a selector that
  lists every element that uses it (see `--_fg` and `--_accent`).
- Every derived color gets a public override token as the first choice:
  `var(--stil-color-accent, <formula>)`.

### Private custom properties

Custom properties starting with `--_` (`--_bg`, `--_fg`, `--_contrast`,
`--_accent`) are internal to Stil and can change at any time.

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

### Color modes

Stil renders light unless `data-stil-theme` is set to `light`, `dark` or
`auto` on an element. This keeps Stil from switching a host page to dark by
accident. Components must look right in both modes, so check both in the
showcases (each has a theme picker).

### Blazor components

- A component is named `Stil` plus the full name in PascalCase, and it
  renders the matching CSS block: `StilButton` renders `stil-btn`,
  `StilCard` renders `stil-card`, `StilAlert` renders `stil-alert`.
- Shared parameter names and types:

  | Parameter | Type |
  | --- | --- |
  | `Variant` | `StilVariant` |
  | `Size` | `StilSize` (from Phase 3) |
  | `ChildContent` | `RenderFragment?` |

- Every component captures unmatched attributes and passes them to its
  root element. A consumer's `class` is appended with
  `StilCssClasses.Merge`; it never replaces the component's own classes.
- Public types live in the `Stil.Blazor.Components` namespace.

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
| npm (CSS only, Phase 2) | `stil-css` | Free on npm. `stil` is taken by another package. |

## Workflow

1. Make CSS changes in `src/Stil.Css/stil.css`. The Blazor project serves
   this file directly, so do not copy it anywhere else.
2. Show every CSS change in both `samples/Stil.Html/index.html` and
   `samples/Stil.Demo`.
3. Every Blazor component gets bUnit tests in `tests/Stil.Blazor.Tests`
   and an example in the demo.
4. Run `dotnet build Stil.sln`, `dotnet test Stil.sln` and
   `node tools/check-contrast.mjs` before pushing. CI
   (`.github/workflows/ci.yml`) runs the same checks plus pack on every push
   to `main` and on every pull request. It fails if a token pair drops below
   WCAG AA or if the package is missing the stylesheet.
5. When you change a color formula in `stil.css`, update the mirror of it
   in `tools/check-contrast.mjs`. The script fails if the two drift apart.
6. Commit messages follow
   [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`,
   `fix:`, `docs:`, `test:`, `chore:`).
