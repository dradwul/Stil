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

## Two layers: components and base

| Layer | What | Where it applies |
| --- | --- | --- |
| **Components** | All `stil-*` classes | Anywhere. No wrapper needed. |
| **Base** | Font, headings `h1`–`h6`, links | Only inside `class="stil-scope"`. |

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

Every color comes from a CSS custom property declared on `:root`. To make
your own theme, load your overrides **after** `stil.css`:

```css
/* my-theme.css */
:root {
    --stil-color-primary: #6741d9;
    --stil-color-secondary: #d0bfff;
    --stil-color-success: #2b8a3e;
    --stil-color-danger: #c92a2a;
    --stil-color-warning: #e67700;
    --stil-color-info: #1971c2;
    --stil-font-family: system-ui, sans-serif;
}
```

Tokens set on any element apply to everything inside it, so you can theme
just one part of a page:

```css
.admin-panel {
    --stil-color-primary: #c92a2a;
}
```

| Token | Default | Used by |
| --- | --- | --- |
| `--stil-color-primary` | `#34dbbf` | `-primary` variants, links, input focus |
| `--stil-color-secondary` | `#a6dbd2` | `-secondary` variants |
| `--stil-color-success` | `#34db58` | `-success` variants |
| `--stil-color-danger` | `#db3434` | `-danger` variants |
| `--stil-color-warning` | `#ebc444` | `-warning` variants |
| `--stil-color-info` | `#34c2db` | `-info` variants |
| `--stil-font-family` | `'Didact Gothic', sans-serif` | `.stil-scope` |
| `--stil-heading-margin` | `0.5rem 0` | `h1`–`h6` inside `.stil-scope` |

Stil uses **Didact Gothic** if your page loads it, for example from Google
Fonts. Otherwise it falls back to `sans-serif`. The stylesheet never fetches
fonts on its own.

### Current limitations

These are planned for Phases 1 and 2 of the [roadmap](../../ROADMAP.md):

- Borders, backgrounds and text colors such as `#ccc` and `white` are
  hard-coded, so there is no dark mode yet.
- The default palette uses white text on light colors. Only the danger color
  meets WCAG AA contrast.
- There are no utility classes, grid or responsive breakpoints yet.
