# Stil CSS

Stil CSS is a small, framework-independent CSS class library. It is a single
stylesheet with no JavaScript and no build step. It works in plain HTML,
Razor, React, Vue, or any other front end.

Every class starts with `stil-` and every token with `--stil-`, so Stil can
run next to Bootstrap, Tailwind or your own CSS without clashing.

> **Status:** early preview. Class and token names may still change before
> `1.0`. See the [roadmap](../../ROADMAP.md).

## Install

Stil CSS is not yet published to npm (planned name: `stil-css`) or a CDN.
For now, use one of these options:

- **Copy the file:** copy [`stil.css`](stil.css) into your project and link it:

  ```html
  <link rel="stylesheet" href="/css/stil.css" />
  ```

- **Blazor/.NET:** install the `Stil` NuGet package. It serves this file as
  `_content/Stil/stil.css`. See [Stil for Blazor](../Stil.Blazor/README.md).

## Components and base styles

| Layer | What | Where it applies |
| --- | --- | --- |
| **Components** | All `stil-*` classes | Anywhere. No wrapper needed. |
| **Base** | Font, text color, headings `h1`–`h6`, links | Only inside `class="stil-scope"`. |

```html
<body class="stil-scope">          <!-- whole app: Stil also styles bare elements -->
    ...
</body>

<section class="stil-scope">       <!-- or just one part of an existing page -->
    ...
</section>
```

Elements outside a `.stil-scope` keep the host page's styles. Stil classes
still work there.

## Classes

| Component | Classes | Notes |
| --- | --- | --- |
| Container | `stil-container` | Centered box, max 1200px, with border and padding. |
| Button | `stil-btn` + `stil-btn-{variant}` | Sizes: `stil-btn-sm`, `stil-btn-lg`. Group with `stil-btn-group`. |
| Form | `stil-form` | Stacks its fields vertically. |
| Input | `stil-input-group` wrapping a `<label>` and `stil-input` | `stil-input` works on `<input>`, `<textarea>` and `<select>`. |
| Card | `stil-card`, `stil-card-title`, `stil-card-footer` | |
| Alert | `stil-alert` + `stil-alert-{variant}` | |
| Table | `stil-table` on `<table>` | Full width with row dividers. |
| Background | `stil-bg-{variant}` | A variant color as background, with readable text on top. |

`{variant}` is one of `primary`, `secondary`, `success`, `danger`, `warning`
or `info`.

```html
<form class="stil-form">
    <div class="stil-input-group">
        <label for="email">Email</label>
        <input id="email" type="email" class="stil-input" />
    </div>
    <button type="submit" class="stil-btn stil-btn-primary">Save</button>
</form>

<div class="stil-alert stil-alert-danger">Something went wrong.</div>
```

Open [`samples/Stil.Html/index.html`](../../samples/Stil.Html/index.html) in a
browser to see every class rendered with its markup.

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

- **Text on colored backgrounds** (buttons, alerts, `stil-bg-*`) is black or
  white, whichever contrasts more with the background. That always meets
  WCAG AA (at least 4.58:1). To force a color, set
  `--stil-color-{variant}-contrast`.
- **Links and focus rings** use `--stil-color-primary`, darkened in light
  mode and lightened in dark mode until they are readable on the neutral
  surfaces. To use a different color, set `--stil-color-accent`.
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
| Optional overrides | `--stil-color-{variant}-contrast` (text on the color), `--stil-color-accent` (links, focus) |
| Neutrals | `--stil-color-body-bg`, `-surface`, `-surface-muted`, `-text`, `-text-muted`, `-border`, `-border-strong` |
| Typography | `--stil-font-family`, `--stil-font-family-mono`, `--stil-font-size-xs` … `-xl`, `--stil-letter-spacing`, `--stil-heading-margin` |
| Spacing | `--stil-space-1` (0.25rem) … `--stil-space-7` (3rem) |
| Shape | `--stil-radius-sm`, `-md`, `-lg`, `-pill`, `--stil-shadow-sm`, `-md`, `-lg` |
| Focus | `--stil-focus-ring-width`, `--stil-focus-ring-offset` |
| Layering | `--stil-z-dropdown`, `-sticky`, `-overlay`, `-modal`, `-popover`, `-toast` |
| Motion | `--stil-duration-fast`, `--stil-duration-md`, `--stil-easing`. The durations are 0 when the user prefers reduced motion. |

The defaults are in the `:root` block at the top of [`stil.css`](stil.css).

Stil uses **Didact Gothic** if your page loads it, for example from Google
Fonts. Otherwise it falls back to `sans-serif`. The stylesheet never
fetches fonts on its own.

### Accessibility check

`node tools/check-contrast.mjs` checks every default token pair in light
and dark mode. It also checks the derived text and accent colors against a
range of brand colors. CI runs it on every pull request.

### Current limitations

These are planned for Phase 2 of the [roadmap](../../ROADMAP.md):

- There is no reset/normalize layer, grid or responsive breakpoints yet.
  The only utilities so far are `stil-bg-*`.
