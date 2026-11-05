# Stil CSS

Stil CSS is a themeable, framework-independent CSS class library. It is a
Bootstrap replacement with dark mode and WCAG AA colors built in. It is a
single stylesheet, about 16 kB minified and gzipped, with no JavaScript. It
works with plain HTML, Razor, React, Vue, or any other front end.

Every class starts with `stil-` and every token with `--stil-`, so Stil can
run next to Bootstrap, Tailwind or your own CSS without clashing. If you are
coming from Bootstrap, see the [migration guide](MIGRATING-FROM-BOOTSTRAP.md).

> **Status:** early preview (`0.1.0`). Class and token names may still
> change before `1.0`. See the [roadmap](../../ROADMAP.md).

## Install

Use one of these options:

- **Download:** every [GitHub release](https://github.com/dradwul/Stil/releases)
  has `stil.css`, `stil.min.css` and `stil.js` attached. You can also copy
  [`dist/stil.css`](dist/stil.css) or [`dist/stil.min.css`](dist/stil.min.css)
  from the repository.
- **CDN:** jsDelivr serves the files from a release tag:

  ```html
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/gh/dradwul/Stil@v0.1.0-preview.4/src/Stil.Css/dist/stil.min.css" />
  ```

- **npm:** the package is prepared as `stil-css` but not published yet.
- **Blazor/.NET:** the `Stil` NuGet package serves the files as
  `_content/Stil/stil.css`, `stil.min.css` and `stil.js`. See
  [Stil for Blazor](../Stil.Blazor/README.md).

## Quick start

```html
<!doctype html>
<html lang="en" data-stil-theme="auto">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="stylesheet" href="stil.min.css" />
</head>
<body class="stil-scope">
    <main class="stil-container stil-py-5">
        <h1>Hello, Stil</h1>
        <div class="stil-alert stil-alert-subtle stil-alert-success">It works.</div>
        <button class="stil-btn stil-btn-primary">Get started</button>
    </main>
</body>
</html>
```

## Components and base styles

| Part | What | Where it applies |
| --- | --- | --- |
| **Components and utilities** | All `stil-*` classes | Anywhere. No wrapper needed. |
| **Base** | Font, text color, headings, links, `code`, `hr`, `blockquote` and a light reset | Only inside `class="stil-scope"`. |

Put `class="stil-scope"` on `<body>` for a whole Stil app. To adopt Stil in
one part of an existing page, put it on a `<section>` instead. Elements
outside the scope keep the host page's styles.

All rules live in the cascade layers `stil.tokens`, `stil.reset`,
`stil.base`, `stil.components` and `stil.utilities`. Any CSS you write
outside a layer wins over Stil, whatever its specificity. That includes
generic rules such as `button { … }` from another reset, so keep those out
of pages that use Stil components.

## Layout

| Class | What it does |
| --- | --- |
| `stil-container` | Centered column, `--stil-container-max` wide (1200px). Add `stil-container-fluid` for full width. |
| `stil-grid` | 12-column CSS grid with a gap. Children span the full row by default. |
| `stil-col-{1-12}`, `stil-col-{bp}-{1-12}` | Column span. The `{bp}` form applies from that breakpoint up. |
| `stil-stack` | Children in a column with even gaps. |
| `stil-cluster` | Children in a wrapping row, vertically centered. |
| `stil-with-sidebar` | Sidebar (`--stil-sidebar-width`) beside the content from `lg`, stacked below it. |

The breakpoints `{bp}` are `sm` (576px), `md` (768px), `lg` (992px) and
`xl` (1200px). They are mobile first: each applies from that width up.

```html
<div class="stil-grid">
    <div class="stil-col-md-8">Main</div>
    <div class="stil-col-md-4">Aside</div>
</div>
```

## Components

In the class names below, `{variant}` is one of `primary`, `secondary`,
`success`, `danger`, `warning` or `info`. States use attributes, not
classes: `aria-current="page"`, `aria-selected="true"`,
`aria-invalid="true"`, `aria-busy="true"` and `disabled`.

| Component | Classes |
| --- | --- |
| Button | `stil-btn` + `stil-btn-{variant}`. Styles: `stil-btn-outline`, `stil-btn-ghost`. Sizes: `stil-btn-sm`, `stil-btn-lg`, `stil-btn-icon`. `aria-busy="true"` shows a spinner, and `aria-pressed="true"` shows a toggle as on. Group buttons with `stil-btn-group`. Close button: `stil-close`. |
| Form | `stil-form` > `stil-field` > `stil-label` + control + `stil-field-hint` / `stil-field-error` |
| Text input, select | `stil-input` (input, textarea), `stil-select`. Sizes: `stil-input-sm`, `stil-input-lg`, `stil-select-sm`, `stil-select-lg`. |
| Checkbox, radio, switch | `<label class="stil-check"><input class="stil-check-input">`. For a switch, use `stil-switch` on a checkbox with `role="switch"`. |
| Input group | `stil-input-group` (`stil-input-group-sm`) with `stil-input-group-text`, inputs and buttons |
| Card | `stil-card` with `stil-card-header`, `stil-card-title`, `stil-card-subtitle`, `stil-card-footer`. A `stil-card-flush` child (for example a table) runs edge to edge. |
| Alert | `stil-alert` + `stil-alert-{variant}`, `stil-alert-subtle`, `stil-alert-title`. A `stil-close` inside makes it dismissible. |
| Badge | `stil-badge` + `stil-badge-{variant}`, `stil-badge-subtle` |
| Avatar | `stil-avatar` (`-sm`, `-lg`) holding initials or an `<img>` |
| List | `stil-list` (`stil-list-flush`) > `stil-list-item` with `stil-list-item-start`, `stil-list-item-content` (`stil-list-item-title`, `stil-list-item-description`) and `stil-list-item-end`. A `stil-list-item` on its own is a single bordered row; `<a>` and `<button>` rows are interactive. |
| Table | `stil-table` with `stil-table-striped`, `stil-table-hover`, `stil-table-compact`, `stil-table-bordered`. Wrap it in `stil-table-responsive` to scroll sideways. |
| Data grid | `stil-data-grid` around a table, with `stil-data-grid-toolbar`, `stil-data-grid-footer`, sort buttons `stil-data-grid-sort` in headers with `aria-sort`, a filter row `stil-data-grid-filters`, a checkbox column `stil-data-grid-select` and `stil-data-grid-message`. Selected rows have `aria-selected="true"`. Add `stil-data-grid-scroll` and a `max-height` to the `stil-table-responsive` wrapper for a sticky header. |
| Combobox, multi-select | `stil-combobox` > `stil-input` (`role="combobox"`) + `stil-combobox-popup` > `stil-combobox-list` (`role="listbox"`) > `stil-combobox-option` (`stil-combobox-option-active`), and `stil-combobox-message`. A multi-select adds `stil-multi-select stil-input` to the combobox, `stil-chip` elements (`stil-chip-text`, `stil-chip-remove`) and a `stil-multi-select-input`. Opening the list and moving between options needs a script. |
| Chip | `stil-chip` with `stil-chip-text` and an optional `stil-chip-remove` button, for tags |
| Slider, range | `<input type="range" class="stil-slider">` with `--_fill` (0–100%); `stil-slider-header` (label and `stil-slider-value`) and `stil-slider-marks`. Two sliders in a `stil-range` (`--_from`, `--_to`, 0–100) pick a span. |
| Rating | `stil-rating` (`stil-rating-sm`) with a radio and a `stil-icon-star` in each `label`; `stil-rating-readonly` with `stil-rating-on` for a shown score |
| Color | `stil-color` > `stil-color-input` (`type="color"`) + a text input, and `stil-color-swatches` > `stil-color-swatch` (`--_swatch`, `aria-pressed`) |
| One-time code | `<input class="stil-pin">` drawn as cells; set `--stil-pin-length` |
| Popover | `<div class="stil-popover" popover>` opened by a `popovertarget` button; `stil-popover-title`. stil.js places it next to the button (`data-stil-placement="top"` for above). |
| Stepper | `<ol class="stil-stepper">` > `stil-step` (`stil-step-done`, `stil-step-error`, `aria-current="step"`) > `stil-step-marker` + `stil-step-text` (`stil-step-title`, `stil-step-description`); `stil-wizard` and `stil-wizard-actions` around a form in steps |
| Skeleton | `stil-skeleton` (a block), `stil-skeleton-text` (a line) and `stil-skeleton-circle` (`--stil-skeleton-size`), in an element with `aria-busy="true"`. The shimmer stops under reduced motion. |
| Splitter | `stil-splitter` (`stil-splitter-vertical`, `--stil-splitter-size` in percent) > `stil-splitter-pane`, `stil-splitter-handle` (`role="separator"`), `stil-splitter-pane` |
| Carousel | `stil-carousel` > `stil-carousel-track` (scroll snap) > `stil-carousel-slide`, and `stil-carousel-controls` with `stil-carousel-prev`, `stil-carousel-dots`, `stil-carousel-next` and an optional `stil-carousel-pause` |
| Image cropper, camera | `stil-cropper` (`stil-cropper-round`, `--stil-cropper-ratio`) > `stil-cropper-stage` > `stil-cropper-image` + `stil-cropper-frame`, and `stil-cropper-tools`; `stil-camera` > `stil-camera-preview` |
| PDF | `stil-pdf` > `stil-pdf-toolbar` (`stil-pdf-title`) + `stil-pdf-frame` (an iframe, `--stil-pdf-height`) or `stil-pdf-fallback` |
| Date range | `<fieldset class="stil-date-range">` with a legend and `stil-date-range-inputs` around two fields |
| File upload | `stil-dropzone` (`data-dragover="true"` while dragging) > `stil-dropzone-input` (the file input, laid over the zone), `stil-dropzone-icon`, `stil-dropzone-text`, `stil-dropzone-button`; `stil-file-list` > `stil-file-list-item` with `stil-file-list-name` and `stil-file-list-size` |
| Stat card | `stil-card stil-stat` with `stil-stat-header` (`stil-stat-label`, `stil-stat-icon`), `stil-stat-value`, and `stil-stat-footer` with a `stil-stat-trend` badge (`data-trend="up"` or `"down"`) and `stil-stat-description` |
| Tree | `<ul class="stil-tree" role="tree">` > `stil-tree-item` (`role="treeitem"`, `aria-expanded`, `aria-selected`) > `stil-tree-row` (`stil-tree-toggle`, `stil-tree-label`) + `<ul class="stil-tree-group">`. Keyboard navigation needs a script. |
| Nav | `stil-nav` > `stil-nav-link`, with `stil-nav-vertical` and `stil-nav-pills`. In a vertical nav, `<details class="stil-nav-group">` > `<summary class="stil-nav-group-header">` (`stil-nav-group-title`, `stil-nav-group-chevron`) + `stil-nav-group-body` groups links. |
| Navbar | `stil-navbar` with `stil-navbar-brand`, a `stil-nav` and `stil-navbar-end`. To collapse on small screens, add `stil-navbar-expand-{sm,md,lg,xl}`, wrap the nav and end content in `stil-navbar-collapse`, and add a `stil-navbar-toggle` button (`aria-expanded`, `aria-controls`, `data-stil-toggle`). |
| Breadcrumb | `<ol class="stil-breadcrumb">` |
| Pagination | `stil-pagination` (`stil-pagination-sm`) > `li` > `stil-page-link`. Use `<span class="stil-page-link">` for an ellipsis. |
| Tabs | `stil-tabs` (`role="tablist"`, plus `stil-tabs-pills` for pills) > `stil-tab` (`role="tab"`, `aria-selected`). Switching panels needs a script. |
| Dialog | `<dialog class="stil-dialog">` with `stil-dialog-header`, `stil-dialog-title`, `stil-dialog-body`, `stil-dialog-footer`. Sizes: `stil-dialog-sm`, `stil-dialog-lg`. |
| Drawer | `<dialog class="stil-drawer">` (`stil-drawer-end` to slide in from the end) with the dialog parts. Width: `--stil-drawer-width`. |
| Dropdown | `<details class="stil-dropdown">` > `<summary class="stil-btn">` + `stil-dropdown-menu` (`-end`, `-up`) > `stil-dropdown-item`, `stil-dropdown-header`, `stil-dropdown-divider`. There is no arrow on a `stil-btn-icon` summary. |
| Accordion | `stil-accordion` (`-flush`) > `<details class="stil-accordion-item">` > `<summary class="stil-accordion-header">` + `stil-accordion-body`. Items that share a `name` attribute open one at a time. |
| Icons | `<span class="stil-icon stil-icon-{name}" aria-hidden="true"></span>`: about 30 built-in icons (chevrons, arrows, close, menu, check, plus, minus, search, more, info, success, warning, danger, external, filter, home, sun, moon, file, folder, upload). They use the current text color and are 1em in size. The set is in [`src/icons.json`](src/icons.json). |
| Tooltip | `data-stil-tooltip="Text"` on any element. Add `stil-tooltip-bottom` to show it below. |
| Progress | `stil-progress` (`stil-progress-lg`) > `stil-progress-bar` with a width |
| QR code, barcode | `stil-qr-code` and `stil-barcode` around an `<svg>` of the code (`role="img"` and `aria-label` on the wrapper). Dark on light in both modes: `--stil-barcode-color`, `--stil-barcode-bg`. `stil-code-invalid` for a value that does not fit. |
| Spinner | `stil-spinner`, `stil-spinner-sm`. It uses the current text color. |
| Toast | `stil-toast-container` > `stil-toast` + `stil-toast-{variant}` > `stil-toast-body` (`stil-toast-title`) + `stil-close` |
| Event | `stil-event` + `stil-event-{variant}`, or `--stil-event-color` for your own color, with `stil-event-title` (`stil-event-time`) and `stil-event-meta`. `stil-event-draft` marks an unsaved event. |
| Calendar (month) | `stil-calendar` (`stil-calendar-mini` for a small month) > `stil-calendar-header` (`stil-calendar-title`) + `<table class="stil-calendar-grid" role="grid">`. Days are `<td role="gridcell" class="stil-calendar-day">` with `--stil-load` (0–1, tints the day), `aria-current="date"`, `aria-selected`, `stil-calendar-outside` and `stil-calendar-range`. Inside: `stil-calendar-date`, `stil-calendar-events` (event chips), `stil-calendar-more` and `stil-calendar-dots` (`stil-calendar-dot`, shown instead of chips on a narrow calendar). An optional week number column: `stil-calendar-week`. Key: `stil-calendar-legend` with `stil-calendar-legend-scale`. Filters: `stil-calendar-filter` with a variant class. |
| Week | `stil-week` (`--stil-week-days`, `--stil-week-first-hour`, `--stil-week-hours`) > `stil-week-scroll` > `stil-week-grid` > `stil-week-corner`, `stil-week-day-head` (`stil-week-day-name`, `stil-calendar-date`, `stil-week-load` with `stil-week-load-bar`), `stil-week-times` (`stil-week-time`) and a `stil-week-col` per day (`stil-week-col-muted` for weekends). In a column, `stil-event`, `stil-week-free` and `stil-week-now` are placed by `--stil-start` and `--stil-length` (minutes from midnight); overlapping events get `--stil-col` and `--stil-cols`. With `data-stil-editable`, stil.js lets the user drag events (an optional `stil-event-resize` handle at the bottom of an event resizes it). `stil-week-auto` shows a `stil-agenda` child instead of the grid when the week is narrower than 36rem. |
| Agenda | `stil-agenda` > `stil-agenda-day` (`stil-agenda-day-title`) > `stil-agenda-list` > `stil-agenda-item` (`stil-agenda-time` + a `stil-event`). A free gap: `stil-agenda-item stil-agenda-free` with `stil-agenda-free-line`. |

To see every class with its markup, open
[`samples/Stil.Html/index.html`](../../samples/Stil.Html/index.html). For a
full page built only with Stil, open
[`samples/Stil.Html/admin.html`](../../samples/Stil.Html/admin.html).

## Optional JavaScript

Stil works without JavaScript. The optional `dist/stil.js` module, 47 kB (13 kB gzipped) with
no dependencies, adds a few behaviours:

```html
<script type="module" src="stil.js"></script>
```

- Dropdowns close on an outside click, on Escape and after you choose an
  item. Opening one closes the others.
- `data-stil-dismiss` on a button removes the closest alert or toast, or the
  element that matches the attribute's value (a CSS selector).
- `data-stil-dialog-open="id"` opens that `<dialog>` (dialog or drawer) as a
  modal. `data-stil-dialog-close` closes the dialog the button is in.
- `data-stil-toggle` on a button flips its `aria-expanded`, for example to
  open a collapsed navbar. Escape collapses it again.
- The arrow keys, Home and End move between dropdown items. ArrowDown on
  the toggle opens the menu.
- In a calendar month (`table.stil-calendar-grid` with `role="grid"`), the
  arrow keys move between the days, Home and End go to the start and end of
  the week, and Enter or Space clicks the day. Moving past the first or last
  day fires a bubbling `stilcalendaredge` event (`detail.key`, `detail.cell`),
  so your script can show the previous or next month. In a right-to-left page
  the left and right arrows follow the reading direction, and `detail.key` is
  the logical key (`ArrowRight` means the next day).
- In a week grid with `data-stil-editable` (`div.stil-week`), the user can drag
  an event to another time or day, drag its `stil-event-resize` handle to
  change its end, and drag on empty time to mark a new event. Times snap to
  15 minutes and Escape cancels. On release a bubbling `stilweekedit` event
  fires with `detail.action` (`"move"`, `"resize"` or `"create"`),
  `detail.item` (the event element, or null), `detail.day` (the index of the
  day column) and `detail.start` and `detail.end` (minutes from midnight).
  The grid puts the event back; save the change and redraw.
- Keys that a widget's own script handles do not also scroll the page,
  submit a form or close a dialog: the arrow keys on `role="treeitem"`,
  `role="tab"` and `role="combobox"`, and Enter and Escape in an open
  combobox.
- Popovers (`.stil-popover` with `popover`) are placed next to their
  `popovertarget` button, below it or above it (`data-stil-placement="top"`),
  and follow it on scroll and resize.
- Open dropdown menus and combobox lists (a `stil-combobox-popup` without
  `hidden`) move to the top layer and are placed next to their toggle or field,
  so a parent with `overflow` (a scrolling card, a table, a dialog) never cuts
  them off. Without stil.js they open in place, as before.
- `data-stil-mask="000 00"` on an input formats it while typing: `0` is a digit,
  `A` a letter, `*` any character, and the rest are literals that are filled in.
- A splitter's `stil-splitter-handle` (`role="separator"`) resizes the first
  pane by dragging, with the arrow keys (5% a step), Home, End and Enter
  (collapse and restore), within its `aria-valuemin` and `aria-valuemax`. It
  sets `--stil-splitter-size` and fires a bubbling `stilsplit` event
  (`detail.size`).
- A carousel's `stil-carousel-prev`, `stil-carousel-next` and dot buttons move
  between the slides, and the dot of the shown slide gets `aria-current`; a
  bubbling `stilslide` event (`detail.index`) reports it.
  `data-stil-autoplay="6000"` moves on every 6 seconds, except while the
  pointer or focus is in the carousel, while its `stil-carousel-pause` button is
  pressed and under reduced motion.
- Exports for scripts: `initCropper`, `setCropperZoom`, `rotateCropper` and
  `cropImage` (an image cropper), `startCamera`, `captureCamera` and
  `stopCamera` (a photo from the camera), and `printFrame` (print an iframe).
- A language link, `<a href="?culture=sv" data-stil-culture="sv">`, stores the
  language in the ASP.NET Core culture cookie (`.AspNetCore.Culture`) and in
  local storage (`stil-culture`), then reloads the page.

Stil for Blazor loads it automatically.

## Right to left

Stil uses logical properties (start and end instead of left and right), so
`dir="rtl"` on `<html>` or any element mirrors the layout: spacing, input
add-ons, borders, breadcrumbs, pagination, switches, drawers and tree and nav
group chevrons. The arrow and chevron icons (`stil-icon-arrow-left`,
`stil-icon-chevron-right`, …) flip; add `stil-icon-directional` to an icon of
your own that points along the reading direction.

## Utilities

Utilities use Bootstrap's names with the `stil-` prefix. Responsive versions
put the breakpoint after the property name, as `stil-{name}-{bp}-{value}`.
For example, `stil-d-none stil-d-md-block` hides an element below `md`.

| Group | Classes | Responsive |
| --- | --- | --- |
| Display | `stil-d-{none,block,inline,inline-block,flex,inline-flex,grid}` | yes |
| Flex | `stil-flex-{row,column,row-reverse,column-reverse,wrap,nowrap}`, `stil-flex-{fill,1,none}`, `stil-flex-grow-*`, `stil-flex-shrink-*` | direction, wrap |
| Alignment | `stil-justify-content-{start,end,center,between,around,evenly}`, `stil-align-items-*`, `stil-align-self-*` | justify, align-items |
| Gap | `stil-gap-{0-7}` | yes |
| Spacing | `stil-{m,p}{,t,b,s,e,x,y}-{0-7}` and `stil-m*-auto`. `s` and `e` are the logical start and end. | yes |
| Sizing | `stil-w-{25,50,75,100,auto}`, `stil-h-*`, `stil-mw-100`, `stil-min-vh-100` | |
| Text | `stil-text-{start,center,end}`, `stil-text-{nowrap,wrap,truncate}`, `stil-text-{uppercase,lowercase,capitalize}`, `stil-text-{body,muted,inherit}`, `stil-fs-{xs,sm,md,lg,xl,2xl}`, `stil-fw-{normal,bold}`, `stil-font-mono`, `stil-align-{baseline,top,middle,bottom,text-top,text-bottom}` | text-align |
| Color | `stil-bg-{variant}` (with readable text), `stil-text-{variant}`, `stil-border-{variant}`, `stil-bg-{body,surface,muted,transparent}` | |
| Borders | `stil-border`, `stil-border-{top,bottom,start,end,0}`, `stil-rounded`, `stil-rounded-{0,sm,lg,pill,circle}`, `stil-shadow`, `stil-shadow-{sm,lg,none}` | |
| Position | `stil-position-*`, `stil-{top,bottom,start,end}-{0,50,100}`, `stil-translate-middle`, `stil-sticky-top`, `stil-overflow-*` | |
| Lists | `stil-list-unstyled` | |
| Visibility | `stil-visually-hidden`, `stil-visually-hidden-focusable`, `stil-visible`, `stil-invisible` | |

Children of `stil-stack`, `stil-cluster`, `stil-grid` and `stil-with-sidebar`
get their spacing from the gap, so their own block margins are removed.
A spacing utility on a child still wins.

The spacing scale runs from 0 to 7: 0, 0.25rem, 0.5rem, 0.75rem, 1rem,
1.5rem, 2rem and 3rem. These are the `--stil-space-*` tokens.

## Theming

Stil is themed entirely through CSS custom properties. Override them in your
own stylesheet. All Stil rules live in cascade layers, so your unlayered CSS
wins no matter where it is loaded or how specific it is.

### Change the brand

Most themes only need the brand colors:

```css
:root {
    --stil-color-primary: #6741d9;
    --stil-color-secondary: #d0bfff;
    --stil-font-family: system-ui, sans-serif;
}
```

Everything derived from a color follows it automatically:

- **Text on colored backgrounds** (buttons, alerts, badges, `stil-bg-*`) is black or
  white, whichever contrasts more with the background. That always meets
  WCAG AA (at least 4.58:1). To force a color, set
  `--stil-color-{variant}-contrast`.
- **Links, focus rings and selected states** (checkboxes, pills, current
  page) use `--stil-color-primary`, darkened in light
  mode and lightened in dark mode until they are readable on the neutral
  surfaces. To use a different color, set `--stil-color-accent`.
- **Outline and ghost buttons, `stil-text-*` and error text** use the variant
  color, adjusted the same way.
- **Hover states** move the background toward its text color.

Tokens set on any element apply to everything inside it, so you can theme
part of a page:

```css
.admin-panel {
    --stil-color-primary: #c92a2a;
}
```

### Light and dark mode

Stil renders in light mode unless you opt in. That way it never switches a
host page to dark by accident. Set `data-stil-theme` on `<html>` or on any
element:

| Value | Effect |
| --- | --- |
| `light` | Always light. |
| `dark` | Always dark. |
| `auto` | Follows the operating system (`prefers-color-scheme`). |

```html
<html data-stil-theme="auto">
```

An element with `data-stil-theme` also gets the body background and text
color. The neutral tokens are written as `light-dark(light, dark)`. Use the
same form when you override them, or the override applies to both modes:

```css
:root {
    --stil-color-surface: light-dark(#fffdf8, #1a1b1e);
}
```

### Tokens

| Group | Tokens |
| --- | --- |
| Brand and semantic colors | `--stil-color-primary`, `-secondary`, `-success`, `-danger`, `-warning`, `-info` |
| Optional overrides | `--stil-color-{variant}-contrast` (text on the color), `--stil-color-accent` (links, focus), `--stil-calendar-load` (the busy tint of calendar days) |
| Neutrals | `--stil-color-body-bg`, `-surface`, `-surface-muted`, `-text`, `-text-muted`, `-border`, `-border-strong` |
| Typography | `--stil-font-family`, `--stil-font-family-mono`, `--stil-font-size-xs` … `-xl`, `--stil-font-weight-normal`, `-bold`, `--stil-line-height`, `--stil-letter-spacing`, `--stil-heading-margin` |
| Spacing | `--stil-space-1` (0.25rem) … `--stil-space-7` (3rem) |
| Layout | `--stil-container-max`, `--stil-sidebar-width`, `--stil-calendar-hour` (the height of an hour in `stil-week`) |
| Shape | `--stil-radius-sm`, `-md`, `-lg`, `-pill`, `--stil-shadow-sm`, `-md`, `-lg`, `--stil-backdrop` |
| Focus | `--stil-focus-ring-width`, `--stil-focus-ring-offset` |
| Layering | `--stil-z-dropdown`, `-sticky`, `-overlay`, `-modal`, `-popover`, `-toast` |
| Motion | `--stil-duration-fast`, `--stil-duration-md`, `--stil-easing`. The durations are 0 when the user prefers reduced motion. |

The defaults are in [`src/tokens.css`](src/tokens.css).

Stil uses **BR Shape** if your page loads it, for example with
`<link href="https://fonts.cdnfonts.com/css/br-shape" rel="stylesheet">`.
Otherwise it falls back to `sans-serif`. The stylesheet never
fetches fonts on its own. For another font, load it and set the token:

```html
<link href="https://fonts.cdnfonts.com/css/br-cobane" rel="stylesheet">
<style>:root { --stil-font-family: 'BR Cobane', sans-serif; }</style>
```

[`samples/Stil.Html/fonts.html`](../../samples/Stil.Html/fonts.html) shows the
same card in BR Shape and the other fonts in `StilFonts`, all from cdnfonts.com.

### Accessibility check

`npm run check` runs `tools/check-contrast.mjs`. It checks every default
token pair in light and dark mode, and the derived text, ink and accent colors
against a range of brand colors. CI runs it on every pull request.

## Develop

[Lightning CSS](https://lightningcss.dev/) builds the stylesheet from the
files in [`src/`](src):

```text
src/tokens.css, derived.css      layer stil.tokens: tokens, color modes, derived colors
src/reset.css                    layer stil.reset
src/base.css                     layer stil.base: element styles inside .stil-scope
src/layout.css, components/*.css layer stil.components
src/utilities.css                layer stil.utilities (the hand-written ones)
build/generate.mjs               generates grid columns, variants and utilities
build/build.mjs                  bundles everything into dist/
```

```shell
npm install
npm run build    # writes dist/stil.css and dist/stil.min.css
npm run watch    # rebuilds on every change; the Blazor demo picks it up live
npm run check    # fails if dist/ is stale or a contrast check fails (CI runs this)
```

`dist/` is committed, so neither the .NET build nor anyone who copies the
file needs Node.js. Commit `dist/` together with your source changes.
