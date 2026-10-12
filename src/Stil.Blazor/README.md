# Stil for Blazor

Stil for Blazor is a set of themeable Blazor components built on
[Stil CSS](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css). Each
component renders plain HTML with Stil CSS classes, so you can mix components
with hand-written Stil markup and restyle both with the same CSS tokens.

> **Status:** early preview (`0.1.0`, not yet on NuGet). Requires .NET 10.
> APIs will change before `1.0`. See the
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

The components work in every render mode: static SSR, Interactive Server and
WebAssembly. No service registration or JavaScript is needed.

## Components

### `StilContainer`

A centered layout box. It renders `<div class="stil-scope container">`.

```razor
<StilContainer>
    Page content
</StilContainer>
```

### `StilCard`

A card with an optional heading. It renders
`<div class="stil-scope card">`.

| Parameter | Type | Description |
| --- | --- | --- |
| `HeaderText` | `string?` | Shown as an `<h2>` heading. Left out when empty. |
| `ChildContent` | `RenderFragment?` | Card body. |

```razor
<StilCard HeaderText="Profile">
    <p>Card body</p>
    <div class="card-footer">
        <button class="bttn bttn-primary">Save</button>
    </div>
</StilCard>
```

### `StilAlert`

A colored message box with `role="alert"`.

| Parameter | Type | Description |
| --- | --- | --- |
| `Type` | `StilAlertType` | `Success`, `Error`, `Warning` or `Info`. The default is `Info`. |
| `ChildContent` | `RenderFragment?` | Message content. |

```razor
<StilAlert Type="StilAlertType.Error">Something went wrong.</StilAlert>
```

### Extra HTML attributes

Every component passes extra HTML attributes (`id`, `data-*`, `aria-*`, and
so on) to its root element. A `class` you supply is appended to the
component's own classes, not used in place of them:

```razor
<StilCard class="profile-card" id="profile" aria-label="Profile card" />
@* renders class="stil-scope card profile-card" *@
```

### Plain CSS classes

Buttons, inputs, tables and forms do not have components yet. Use the Stil
CSS classes inside any Stil component, or inside an element with
`class="stil-scope"`:

```razor
<div class="stil-scope">
    <button class="bttn bttn-primary" @onclick="Save">Save</button>
</div>
```

See the [Stil CSS class reference](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css#classes).

## Theming

Override the tokens after the Stil stylesheet, for example in `wwwroot/app.css`:

```css
.stil-scope {
    --primary-color: #6741d9;
    --error-color: #c92a2a;
    font-family: system-ui, sans-serif;
}
```

See the [full token list](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css#theming).

## Development

The component source is in this folder. The tests are in
`tests/Stil.Blazor.Tests`, and a runnable demo is in `samples/Stil.Demo`.
The stylesheet is linked from `src/Stil.Css/stil.css` at build time. Make
style changes there, not here.
