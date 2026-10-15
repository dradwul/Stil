# Migrating from Bootstrap 5

Stil follows Bootstrap's vocabulary where it can. Most class names map 1:1,
with `stil-` in front.

The main differences:

- **Every class has a `stil-` prefix**, so both libraries can be loaded
  side by side while you migrate one page at a time.
- **States are attributes, not classes.** Use `aria-current="page"` instead
  of `.active`, `aria-invalid="true"` instead of `.is-invalid`, and
  `disabled` or `aria-disabled="true"` instead of `.disabled`. That makes the
  markup accessible by default.
- **No JavaScript bundle.** Dialogs use the native `<dialog>` element,
  dropdowns use `<details>`, and tooltips are pure CSS. The optional 3 kB
  `stil.js` closes dropdowns on an outside click and handles
  `data-stil-dismiss` and `data-stil-dialog-open`. Tabs need a few lines of
  your own script, or the Stil Blazor components.
- **Base styles are opt-in.** Bootstrap's Reboot styles every page. Stil
  only styles headings, links and so on inside `class="stil-scope"`. Put it
  on `<body>` to get the Bootstrap behaviour.
- **Your CSS always wins.** Stil is in cascade layers, so there is no need
  for `!important` or extra specificity to override it.

## Theming

| Bootstrap | Stil |
| --- | --- |
| `--bs-primary` (and other Sass/CSS variables) | `--stil-color-primary`. See the [token list](README.md#tokens). |
| `data-bs-theme="dark"` | `data-stil-theme="dark"`. `auto` follows the OS. |
| `.text-bg-primary` | `stil-bg-primary`. Text contrast is automatic. |

## Layout

| Bootstrap | Stil | Notes |
| --- | --- | --- |
| `container` | `stil-container` | One max width (`--stil-container-max`). |
| `container-fluid` | `stil-container stil-container-fluid` | |
| `row` | `stil-grid` | CSS grid with 12 columns. Use gap utilities instead of gutters. |
| `col-6`, `col-md-4` | `stil-col-6`, `stil-col-md-4` | Children without a column class span the full row. |
| `col` (equal width) | `stil-d-flex` + `stil-flex-1` on children | |
| `vstack`, `hstack` | `stil-stack`, `stil-cluster` | |
| Sidebar layouts | `stil-with-sidebar` | Sidebar beside the content from `lg`; stacked below it. |

Breakpoints are the same as Bootstrap's: `sm` 576px, `md` 768px, `lg` 992px,
`xl` 1200px. There is no `xxl`.

## Components

| Bootstrap | Stil |
| --- | --- |
| `btn btn-primary` | `stil-btn stil-btn-primary` |
| `btn btn-outline-primary` | `stil-btn stil-btn-outline stil-btn-primary` |
| `btn btn-link` | `stil-btn stil-btn-ghost` |
| `btn-sm`, `btn-lg` | `stil-btn-sm`, `stil-btn-lg` |
| `btn-group` | `stil-btn-group` |
| `btn-close` | `stil-close` |
| Spinner inside a button | `aria-busy="true"` on the `stil-btn` |
| `mb-3` wrapper around a label and input | `stil-field` inside a `stil-form`, which spaces the fields |
| `form-label` | `stil-label` |
| `form-control` | `stil-input` (also for `<textarea>`) |
| `form-select` | `stil-select` |
| `form-control-sm`, `form-select-lg` | `stil-input-sm`, `stil-select-lg` |
| `form-text` | `stil-field-hint` |
| `is-invalid` + `invalid-feedback` | `aria-invalid="true"` + `stil-field-error` |
| `form-check` + `form-check-input` + `form-check-label` | `<label class="stil-check"><input class="stil-check-input">Text</label>` |
| `form-switch` | `<input type="checkbox" role="switch" class="stil-switch">` |
| `input-group`, `input-group-text` | `stil-input-group`, `stil-input-group-text` |
| `card`, `card-header`, `card-footer` | `stil-card`, `stil-card-header`, `stil-card-footer` |
| `card-body` | Not needed: `stil-card` has its own padding |
| `card-title`, `card-subtitle` | `stil-card-title`, `stil-card-subtitle` |
| `alert alert-danger` | `stil-alert stil-alert-danger` (solid). Add `stil-alert-subtle` for Bootstrap's light look. |
| `alert-heading` | `stil-alert-title` |
| `alert-dismissible` + `btn-close` | A `stil-close` inside the alert |
| `badge text-bg-success` | `stil-badge stil-badge-success` (pill-shaped by default) |
| `table`, `table-striped`, `table-hover`, `table-bordered` | `stil-table`, `stil-table-striped`, `stil-table-hover`, `stil-table-bordered` |
| `table-sm` | `stil-table-compact` |
| `table-responsive` | `stil-table-responsive` |
| `nav`, `nav-link`, `active` | `stil-nav`, `stil-nav-link`, `aria-current="page"` |
| `nav-pills`, `flex-column` | `stil-nav-pills`, `stil-nav-vertical` |
| `nav-tabs` | `stil-tabs` with `stil-tab` (`role="tab"`, `aria-selected`) |
| `navbar`, `navbar-brand`, `navbar-nav` | `stil-navbar`, `stil-navbar-brand`, `stil-nav` |
| `ms-auto` group in a navbar | `stil-navbar-end` |
| `navbar-expand-md`, `navbar-toggler`, `navbar-collapse` | `stil-navbar-expand-md`, `stil-navbar-toggle` (with `data-stil-toggle`), `stil-navbar-collapse` |
| `offcanvas`, `offcanvas-end` | `<dialog class="stil-drawer">`, `stil-drawer-end` |
| `accordion`, `accordion-item`, `accordion-button`, `accordion-body` | `stil-accordion`, `<details class="stil-accordion-item">`, `<summary class="stil-accordion-header">`, `stil-accordion-body` (`name` attribute instead of `data-bs-parent`) |
| `nav-pills` on tabs | `stil-tabs-pills` |
| `dropup` | `stil-dropdown-menu-up` |
| `breadcrumb`, `breadcrumb-item`, `active` | `stil-breadcrumb`, a plain `<li>`, `aria-current="page"` |
| `pagination`, `page-item`, `page-link` | `stil-pagination`, a plain `<li>`, `stil-page-link` |
| `pagination-sm` | `stil-pagination-sm` |
| `input-group-sm` | `stil-input-group-sm` |
| `active` / `disabled` page | `aria-current="page"` / `aria-disabled="true"` |
| `modal`, `modal-dialog`, `modal-content` | `<dialog class="stil-dialog">`, opened with `showModal()` |
| `modal-header`, `modal-title`, `modal-body`, `modal-footer` | `stil-dialog-header`, `stil-dialog-title`, `stil-dialog-body`, `stil-dialog-footer` |
| `modal-sm`, `modal-lg` | `stil-dialog-sm`, `stil-dialog-lg` |
| `dropdown`, `dropdown-toggle` | `<details class="stil-dropdown"><summary class="stil-btn">` |
| `dropdown-menu`, `dropdown-item`, `dropdown-divider`, `dropdown-header` | `stil-dropdown-menu`, `stil-dropdown-item`, `stil-dropdown-divider`, `stil-dropdown-header` |
| `dropdown-menu-end` | `stil-dropdown-menu-end` |
| Tooltips (JavaScript + Popper) | `data-stil-tooltip="Text"` (CSS only). Add `stil-tooltip-bottom` to show it below. |
| `list-group`, `list-group-item`, `list-group-item-action`, `list-group-flush` | `stil-list`, `stil-list-item` (an `<a>` or `<button>` item is interactive), `stil-list-flush` |
| `progress`, `progress-bar` | `stil-progress`, `stil-progress-bar` |
| `spinner-border`, `spinner-border-sm` | `stil-spinner`, `stil-spinner-sm` |
| `toast-container`, `toast` | `stil-toast-container`, `stil-toast stil-toast-{variant}` |
| `toast-header`, `toast-body` | `stil-toast-title`, `stil-toast-body` |

## Utilities

Utilities keep Bootstrap's names, with responsive infixes in the same place:
`d-md-none` becomes `stil-d-md-none`.

| Bootstrap | Stil | Notes |
| --- | --- | --- |
| `d-*`, `flex-*`, `justify-content-*`, `align-items-*`, `align-self-*`, `gap-*` | Same names with `stil-` | |
| `m-*`, `p-*`, `mt-*`, `px-*` … | Same names with `stil-` | **Different scale**, see below. |
| `text-start`, `text-center`, `text-end`, `text-nowrap`, `text-truncate`, `text-uppercase` | Same names with `stil-` | |
| `text-muted`, `text-body` | `stil-text-muted`, `stil-text-body` | |
| `text-primary` | `stil-text-primary` | Darkened or lightened until readable |
| `fs-1` … `fs-6` | `stil-fs-2xl`, `stil-fs-xl`, `stil-fs-lg`, `stil-fs-md`, `stil-fs-sm`, `stil-fs-xs` | |
| `fw-bold`, `fw-normal` | `stil-fw-bold`, `stil-fw-normal` | |
| `bg-primary`, `text-bg-primary` | `stil-bg-primary` | |
| `bg-body`, `bg-body-secondary` | `stil-bg-body`, `stil-bg-muted` | |
| `border`, `border-top`, `border-0`, `border-primary` | Same names with `stil-` | |
| `rounded`, `rounded-pill`, `rounded-circle`, `shadow`, `shadow-sm` | Same names with `stil-` | |
| `w-50`, `h-100`, `mw-100`, `min-vh-100` | Same names with `stil-` | |
| `position-*`, `top-0`/`-50`/`-100`, `start-*`, `translate-middle`, `overflow-*`, `sticky-top`, `visually-hidden`, `invisible` | Same names with `stil-` | |
| `list-unstyled`, `align-middle` (and other `align-*`) | Same names with `stil-` | |
| `active` on a toggle button | `aria-pressed="true"` | |

**Spacing scale:**

| Bootstrap | Stil |
| --- | --- |
| `1` (0.25rem) | `1` |
| `2` (0.5rem) | `2` |
| *none* (0.75rem) | `3` |
| `3` (1rem) | `4` |
| `4` (1.5rem) | `5` |
| *none* (2rem) | `6` |
| `5` (3rem) | `7` |

## Not in Stil (yet)

Carousel, a generic collapse, placeholders, ratio helpers,
floating labels, popovers and scrollspy. Stil has about 25 built-in icons
(`stil-icon-*`); Bootstrap Icons can be used alongside them. Some of these are planned for the Blazor components. See the
[roadmap](../../ROADMAP.md).
