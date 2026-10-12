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

Every custom property starts with `--stil-` and follows
`--stil-{category}-{name}[-{variant}]`, for example `--stil-color-primary`,
`--stil-font-family` or `--stil-heading-margin`. Tokens are declared on
`:root`. Components read them and never declare them, so a theme set on any
ancestor element reaches everything inside it.

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
`:is()`/`:where()`, `color-mix()`, custom properties, native `<dialog>` and
the Popover API. Check anything newer on [Baseline](https://web.dev/baseline)
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
4. Run `dotnet build Stil.sln` and `dotnet test Stil.sln` before pushing. CI
   (`.github/workflows/ci.yml`) runs build, test and pack on every push to
   `main` and on every pull request, and fails if the package is missing the
   stylesheet.
5. Commit messages follow
   [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`,
   `fix:`, `docs:`, `test:`, `chore:`).
