<p align="center">
  <img src="assets/stil-logo.svg" alt="Stil" width="220" />
</p>

# Stil

Stil is a themeable UI toolkit in two parts:

| Part | What it is | Replaces | Docs |
| --- | --- | --- | --- |
| **Stil CSS** | A framework-independent CSS class library: layout, components and utilities, about 10 kB gzipped. Works with plain HTML or any framework. | Bootstrap, Tailwind, Bulma | [`src/Stil.Css`](src/Stil.Css/README.md) |
| **Stil for Blazor** | A Blazor component library built on Stil CSS: forms with validation, lists, tabs, dropdowns, dialogs and toasts. Shipped as the `Stil` NuGet package. | Radzen, Blazorise, MudBlazor | [`src/Stil.Blazor`](src/Stil.Blazor/README.md) |

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
<link rel="stylesheet" href="stil.min.css" />

<div class="stil-grid">
    <div class="stil-card stil-col-md-6">
        <h2 class="stil-card-title">Welcome</h2>
        <p>Bootstrap-style classes, all prefixed with <code>stil-</code>.</p>
        <div class="stil-cluster">
            <button class="stil-btn stil-btn-primary">Get started</button>
            <button class="stil-btn stil-btn-outline stil-btn-secondary">Docs</button>
        </div>
    </div>
</div>
```

Blazor:

```razor
<StilListItem Title="Order #1042" Badge="Delivered" BadgeVariant="StilVariant.Success" />

<EditForm Model="order" OnValidSubmit="Save" class="stil-form">
    <DataAnnotationsValidator />
    <StilTextBox @bind-Value="order.Customer" Label="Customer" />
    <StilSelect @bind-Value="order.Status" Label="Status" />
    <StilButton Type="submit">Save</StilButton>
</EditForm>
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

Coming from Bootstrap? See the
[migration guide](src/Stil.Css/MIGRATING-FROM-BOOTSTRAP.md).

## Repository layout

```text
.github/workflows/   CI: check, build, test and pack on every push and pull request
assets/              Logo and other shared brand files
src/Stil.Css/        Stil CSS: sources (src/), build scripts (build/), output (dist/), npm package
src/Stil.Blazor/     Stil for Blazor: Razor class library, packed as NuGet "Stil"
samples/Stil.Html/   Plain-HTML showcase of every CSS class, plus an admin page built with Stil
samples/Stil.Demo/   Blazor web app: component gallery and an orders CRUD page
tests/               bUnit/xUnit tests for the Blazor components
tools/               check-contrast.mjs: WCAG contrast check run by CI
CONTRIBUTING.md      Naming rules, scope model, browser support, workflow
ROADMAP.md           Phase plan
Stil.sln             Solution with library, demo and tests
```

The Blazor project does not keep its own copy of the stylesheet. It serves
the committed `src/Stil.Css/dist/` files directly as `_content/Stil/stil.css`
and `stil.min.css`, so the .NET build does not need Node.js.

## Develop

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download), plus
Node.js 20 or later if you change the CSS.

```shell
dotnet build Stil.sln
dotnet test Stil.sln
```

CSS work happens in `src/Stil.Css`:

```shell
cd src/Stil.Css
npm install
npm run watch    # rebuilds dist/ on every change
npm run check    # dist/ up to date + WCAG contrast checks
```

Run the Blazor demo and open <http://localhost:5100>:

```shell
dotnet run --project samples/Stil.Demo --launch-profile http
```

To see the plain CSS showcase, open `samples/Stil.Html/index.html` in a
browser. It needs no server. `samples/Stil.Html/admin.html` is a complete
admin dashboard built only with Stil CSS.

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
