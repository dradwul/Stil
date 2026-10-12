<p align="center">
  <img src="assets/stil-logo.svg" alt="Stil" width="220" />
</p>

# Stil

Stil is a themeable UI toolkit in two parts:

| Part | What it is | Replaces | Docs |
| --- | --- | --- | --- |
| **Stil CSS** | A framework-independent CSS class library. Works with plain HTML or any framework. | Bootstrap, Tailwind, Bulma | [`src/Stil.Css`](src/Stil.Css/README.md) |
| **Stil for Blazor** | A Blazor component library built on Stil CSS, shipped as the `Stil` NuGet package. | Radzen, Blazorise, MudBlazor | [`src/Stil.Blazor`](src/Stil.Blazor/README.md) |

The Blazor components render ordinary HTML with Stil CSS classes, so a page
can mix components and hand-written markup. Every class starts with `stil-`
and every token with `--stil-`, so Stil can sit next to Bootstrap or your own
CSS without clashing. You change the look by overriding a small set of CSS
custom properties (design tokens). You do not need to rebuild or fork the
library.

> **Status:** early preview (`0.1.0`, not yet published). Class names, token
> names and component APIs may still change before `1.0`. See the
> [roadmap](ROADMAP.md).

## Quick look

Plain HTML:

```html
<link rel="stylesheet" href="stil.css" />

<div class="stil-card">
    <h2 class="stil-card-title">Welcome</h2>
    <button class="stil-btn stil-btn-primary">Get started</button>
</div>
```

Blazor:

```razor
<StilCard HeaderText="Welcome">
    <StilAlert Variant="StilVariant.Success">Saved!</StilAlert>
</StilCard>
```

Your own theme, with dark mode that follows the OS:

```html
<html data-stil-theme="auto">
```

```css
:root {
    --stil-color-primary: #6741d9;
    --stil-color-secondary: #d0bfff;
}
```

Text on colored surfaces, links and focus rings adapt automatically and stay
WCAG AA readable. Both showcases include a theme picker, so you can try it.

## Repository layout

```text
.github/workflows/   CI: build, test and pack on every push and pull request
assets/              Logo and other shared brand files
src/Stil.Css/        Stil CSS: the stylesheet source (single source of truth)
src/Stil.Blazor/     Stil for Blazor: Razor class library, packed as NuGet "Stil"
samples/Stil.Html/   Plain-HTML showcase of every CSS class (no build needed)
samples/Stil.Demo/   Blazor web app showing the components
tests/               bUnit/xUnit tests for the Blazor components
tools/               check-contrast.mjs: WCAG contrast check run by CI
CONTRIBUTING.md      Naming rules, scope model, browser support, workflow
ROADMAP.md           Phase plan
Stil.sln             Solution with library, demo and tests
```

The Blazor project does not keep its own copy of the stylesheet. It serves
`src/Stil.Css/stil.css` directly as `_content/Stil/stil.css`.

## Develop

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and Node.js (for the
contrast check).

```shell
dotnet build Stil.sln
dotnet test Stil.sln
node tools/check-contrast.mjs
```

Run the Blazor demo and open <http://localhost:5100>:

```shell
dotnet run --project samples/Stil.Demo --launch-profile http
```

To see the plain CSS showcase, open `samples/Stil.Html/index.html` in a
browser. It needs no server.

Build a local NuGet package:

```shell
dotnet pack src/Stil.Blazor -o artifacts
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the naming rules (classes, tokens,
variants, Blazor components), the scope model, browser support and the
workflow.

## License

[MIT](LICENSE)
