# Stil for Blazor

Stil for Blazor is a set of themeable Blazor components built on
[Stil CSS](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css). Each
component renders plain HTML with `stil-*` CSS classes. You can mix
components with hand-written Stil markup and restyle both with the same
`--stil-*` tokens.

> **Status:** early preview (`0.1.0`, not yet on NuGet). Requires .NET 10.
> APIs may still change before `1.0`. See the
> [roadmap](https://github.com/dradwul/StilCSS/blob/main/ROADMAP.md).

## Install

```shell
dotnet add package Stil
```

Add the stylesheet to the `<head>` in `Components/App.razor`:

```razor
<link rel="stylesheet" href="@Assets["_content/Stil/stil.css"]" />
```

Add the namespace to `_Imports.razor`:

```razor
@using Stil.Blazor.Components
```

Optional: put `class="stil-scope"` on `<body>` so that Stil also styles the
font, headings and links on your pages.

The components work in every render mode: static SSR, Interactive Server and
WebAssembly. No service registration or JavaScript is needed.

## Naming

Each component is named `Stil` + the component name and renders the
matching CSS class:

| Component | CSS class |
| --- | --- |
| `StilContainer` | `stil-container` |
| `StilCard` | `stil-card` |
| `StilAlert` | `stil-alert` |

Color variants use the shared `StilVariant` enum: `Primary`, `Secondary`,
`Success`, `Danger`, `Warning`, `Info`.

## Components

### `StilContainer`

A centered layout box.

```razor
<StilContainer>
    Page content
</StilContainer>
```

### `StilCard`

A card with an optional title.

| Parameter | Type | Description |
| --- | --- | --- |
| `HeaderText` | `string?` | Shown as `<h2 class="stil-card-title">`. Left out when empty. |
| `ChildContent` | `RenderFragment?` | Card body. |

```razor
<StilCard HeaderText="Profile">
    <p>Card body</p>
    <div class="stil-card-footer">
        <button class="stil-btn stil-btn-primary">Save</button>
    </div>
</StilCard>
```

### `StilAlert`

A colored message box with `role="alert"`.

| Parameter | Type | Description |
| --- | --- | --- |
| `Variant` | `StilVariant` | The color. The default is `Info`. |
| `ChildContent` | `RenderFragment?` | Message content. |

```razor
<StilAlert Variant="StilVariant.Danger">Something went wrong.</StilAlert>
```

### Extra HTML attributes

Every component passes extra HTML attributes (`id`, `data-*`, `aria-*`, and
so on) to its root element. A `class` you supply is appended to the
component's own classes, not used in place of them:

```razor
<StilCard class="profile-card" id="profile" aria-label="Profile card" />
@* renders class="stil-card profile-card" *@
```

### Plain CSS classes

Buttons, inputs, tables and forms do not have components yet (they are
planned as `StilButton`, `StilTextBox`, …). Until then, use the Stil CSS
classes directly. They work anywhere:

```razor
<button class="stil-btn stil-btn-primary" @onclick="Save">Save</button>
```

See the [Stil CSS class reference](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css#classes).

## Theming

Override the tokens in your own CSS, for example in `wwwroot/app.css`.
Stil's rules live in cascade layers, so your CSS always wins:

```css
:root {
    --stil-color-primary: #6741d9;
    --stil-color-danger: #c92a2a;
    --stil-font-family: system-ui, sans-serif;
}
```

Text on colored components, link colors and focus rings are derived from
these tokens and stay WCAG AA readable. Tokens set on a wrapper element apply
only to the components inside it.

For dark mode, set `data-stil-theme` to `light`, `dark` or `auto`, either on
`<html>` in `App.razor` or on any element:

```razor
<html lang="en" data-stil-theme="auto">
```

See the [theming guide and full token list](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css#theming).

## Development

The component source is in this folder. The tests are in
`tests/Stil.Blazor.Tests`, and a runnable demo is in `samples/Stil.Demo`.
The stylesheet is served directly from `src/Stil.Css/stil.css`. Make style
changes there. Naming rules are in
[CONTRIBUTING.md](https://github.com/dradwul/StilCSS/blob/main/CONTRIBUTING.md).
