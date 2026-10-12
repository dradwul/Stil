# Stil CSS

Stil CSS is a small, framework-independent CSS class library. It is a single
stylesheet with no JavaScript and no build step. It works in plain HTML,
Razor, React, Vue, or any other front end.

> **Status:** early preview. Expect class names and tokens to change before
> `1.0`. See the [roadmap](../../ROADMAP.md).

## Install

Stil CSS is not yet published to npm or a CDN. For now, use one of these
options:

- **Copy the file:** copy [`stil.css`](stil.css) into your project and link it:

  ```html
  <link rel="stylesheet" href="/css/stil.css" />
  ```

- **Blazor/.NET:** install the `Stil` NuGet package. It serves this file as
  `_content/Stil/stil.css`. See [Stil for Blazor](../Stil.Blazor/README.md).

## Scope

All Stil styles live under the `.stil-scope` class. Nothing outside it is
touched, so Stil can be added to an existing site page by page:

```html
<body>
    <div class="stil-scope">
        <!-- Stil styles apply here -->
    </div>
    <footer>Untouched by Stil</footer>
</body>
```

For a whole Stil app, put `class="stil-scope"` on `<body>`.

## Classes

| Area | Classes and elements | Notes |
| --- | --- | --- |
| Layout | `.container` | Centered box, max 1200px, with border and padding. |
| Buttons | `.bttn` + `.bttn-primary` / `-secondary` / `-success` / `-error` / `-alert` / `-info` | Sizes: `.bttn-sm`, `.bttn-lg`. Group with `.bttn-group`. |
| Inputs | `.input-group` wrapping a `<label>` and `.input-field` | `.input-field` works on `<input>`, `<textarea>` and `<select>`. |
| Cards | `.card`, `.card-footer` | |
| Alerts | `.alert` + `.alert-success` / `-error` / `-warning` / `-info` | |
| Typography | `h1` to `h6` | Sizes and spacing are set inside the scope. |
| Tables | `table`, `th`, `td` | Full width with row dividers. |
| Links | `a` | Primary color, with a focus outline. |
| Forms | `form` | Stacks its fields vertically. |

Open [`samples/Stil.Html/index.html`](../../samples/Stil.Html/index.html) in a
browser to see every class rendered with its markup.

## Theming

Every color comes from a CSS custom property. To make your own theme, load
your overrides **after** `stil.css`:

```css
/* my-theme.css */
.stil-scope {
    --primary-color: #6741d9;
    --secondary-color: #d0bfff;
    --success-color: #2b8a3e;
    --error-color: #c92a2a;
    --alert-color: #e67700;
    --info-color: #1971c2;
    --heading-margin: 0.75rem 0;
    font-family: system-ui, sans-serif;
}
```

| Token | Default | Used by |
| --- | --- | --- |
| `--primary-color` | `#34dbbf` | `.bttn-primary`, links, input focus |
| `--secondary-color` | `#a6dbd2` | `.bttn-secondary` |
| `--success-color` | `#34db58` | `.bttn-success`, `.alert-success` |
| `--error-color` | `#db3434` | `.bttn-error`, `.alert-error` |
| `--alert-color` | `#ebc444` | `.bttn-alert`, `.alert-warning` |
| `--info-color` | `#34c2db` | `.bttn-info`, `.alert-info` |
| `--heading-margin` | `0.5rem 0` | `h1` to `h6` |

Stil uses the **Didact Gothic** font if your page loads it, for example from
Google Fonts. Otherwise it falls back to `sans-serif`. The stylesheet never
fetches fonts on its own.

### Current limitations

These are planned for Phase 1 of the [roadmap](../../ROADMAP.md):

- Override tokens on `.stil-scope` itself. Tokens set on another ancestor
  element do not reach Blazor components, because each component sets its
  own `.stil-scope`.
- Borders, backgrounds and text colors such as `#ccc` and `white` are
  hard-coded, so there is no dark mode yet.
- The default palette uses white text on light colors. Only the error color
  meets WCAG AA contrast.
- There are no utility classes, grid or responsive breakpoints yet.
