<p align="center">
  <img src="assets/stil-logo.svg" alt="Stil" width="220" />
</p>

# Stil

Stil is a themeable UI toolkit in two parts:

| Part | What it is | Docs |
| --- | --- | --- |
| **Stil CSS** | A framework-independent CSS class library: layout, components and utilities, about 23 kB gzipped. Works with plain HTML or any framework. | [`src/Stil.Css`](src/Stil.Css/README.md) |
| **Stil for Blazor** | A Blazor component library built on Stil CSS: forms with validation, a data grid, pickers, tree view, tabs, dropdowns, dialogs and toasts. Shipped as the `Stil` NuGet package. | [`src/Stil.Blazor`](src/Stil.Blazor/README.md) |

Around them: `Stil.Charts`, themeable SVG charts for Blazor
([`src/Stil.Charts`](src/Stil.Charts/README.md)); `Stil.Markdown`, a
Markdown view and editor ([`src/Stil.Markdown`](src/Stil.Markdown/README.md));
`Stil.RichText`, a rich text editor and view
([`src/Stil.RichText`](src/Stil.RichText/README.md));
`dotnet new stil-app`, a
project template that can add features such as charts, editors and planners
(`--Features`) ([`templates/Stil.Templates`](templates/Stil.Templates/README.md));
and a documentation site with live examples, a theme builder and a project wizard
([`docs/Stil.Docs`](docs/Stil.Docs)).

The Blazor components render ordinary HTML with Stil CSS classes, so a page
can mix components and hand-written markup. Every class starts with `stil-`
and every token with `--stil-`, so Stil can sit next to Bootstrap or your own
CSS without clashing. You change the look by overriding a small set of CSS
custom properties (design tokens). You do not need to rebuild or fork the
library.

> **Status:** stable (`1.0`). Class names, token names and component APIs
> change only in a major version ([Semantic Versioning](https://semver.org/));
> CI checks the public .NET API against a baseline. Coming from a `0.1`
> preview? The [migration notes](MIGRATING-TO-1.0.md) list every rename. See
> the [roadmap](ROADMAP.md) for what comes next.

## Install

| You want | Do this |
| --- | --- |
| A new Blazor app with Stil set up | `dotnet new install Stil.Templates`, then `dotnet new stil-app -n MyApp` ([templates](templates/Stil.Templates/README.md)) |
| Stil in an existing Blazor app | `dotnet add package Stil`, then follow [Get started](src/Stil.Blazor/README.md#get-started) |
| Charts | `dotnet add package Stil.Charts` ([charts](src/Stil.Charts/README.md)) |
| Markdown | `dotnet add package Stil.Markdown` ([Markdown](src/Stil.Markdown/README.md)) |
| Rich text | `dotnet add package Stil.RichText` ([rich text](src/Stil.RichText/README.md)) |
| Only the CSS | Download `stil.min.css` from a [release](https://github.com/dradwul/Stil/releases), or link it from jsDelivr ([Stil CSS](src/Stil.Css/README.md#install)) |

All packages require .NET 10. The latest version is on
[nuget.org](https://www.nuget.org/packages/Stil).

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
.github/workflows/   CI: check, build, test, pack, template and axe tests; a docs build
assets/              Logo and other shared brand files
src/Stil.Css/        Stil CSS: sources (src/), build scripts (build/), output (dist/), npm package
src/Stil.Blazor/     Stil for Blazor: Razor class library, packed as NuGet "Stil"
src/Stil.Charts/     SVG charts for Blazor, packed as NuGet "Stil.Charts"
src/Stil.Markdown/   Markdown view and editor (on Markdig), packed as NuGet "Stil.Markdown"
src/Stil.RichText/   Rich text editor and view (on AngleSharp), packed as NuGet "Stil.RichText"
templates/           dotnet new stil-app (Stil.Templates) and its test script
docs/Stil.Docs/      Documentation site (Blazor WebAssembly) with a theme builder
samples/Stil.Html/   Plain-HTML showcase of every CSS class, plus an admin page built with Stil
samples/Stil.Demo/   Blazor web app: gallery, orders with a dashboard, budget, check-in, task board, rooms and a project plan
tests/               bUnit tests (Stil.Blazor.Tests, Stil.Charts.Tests, Stil.Markdown.Tests, Stil.RichText.Tests) and Playwright tests (Stil.Visual)
tools/               CI checks (WCAG contrast, size budget) and update-public-api.mjs
CHANGELOG.md         Changes in every package
CONTRIBUTING.md      Naming rules, scope model, browser support, workflow, the public API
MIGRATING-TO-1.0.md  Every rename from 0.1 to 1.0, with the new name next to the old
ROADMAP.md           Phase plan
Stil.sln             Solution with the libraries, docs, demo, templates and tests
Directory.Build.props  The shared version and package metadata (src/ adds the public API analyzers)
```

The Blazor project does not keep its own copy of the stylesheet. It serves
the committed `src/Stil.Css/dist/` files directly as `_content/Stil/stil.css`
and `stil.min.css`, so the .NET build does not need Node.js.

## Develop

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download), plus
Node.js 20 or later if you change the CSS. The compiled CSS in
`src/Stil.Css/dist/` is committed, so building and testing the .NET projects
needs no Node.js.

```shell
git clone https://github.com/dradwul/Stil.git
cd Stil
dotnet build Stil.sln
dotnet test Stil.sln
```

CSS work happens in `src/Stil.Css`:

```shell
cd src/Stil.Css
npm install
npm run watch    # rebuilds dist/ on every change
npm run check    # dist/ up to date + WCAG contrast checks + size budget
```

Run the Blazor demo and open <http://localhost:5100>:

```shell
dotnet run --project samples/Stil.Demo --launch-profile http
```

To see the plain CSS showcase, open `samples/Stil.Html/index.html` in a
browser. It needs no server. `samples/Stil.Html/admin.html` is a complete
admin dashboard built only with Stil CSS.

Run the documentation site and open <http://localhost:5193>:

```shell
dotnet run --project docs/Stil.Docs --launch-profile http
```

Test the `dotnet new stil-app` template. The script installs it into a
temporary template hive, never into your own template list:

```shell
node templates/test-templates.mjs
```

Run the screenshot and accessibility tests (Playwright with your installed
Edge; see [`tests/Stil.Visual`](tests/Stil.Visual/README.md)):

```shell
cd tests/Stil.Visual
npm ci
npm test
```

Build local NuGet packages (releases are published from a tag; see
[CONTRIBUTING.md](CONTRIBUTING.md#releases)):

```shell
dotnet pack src/Stil.Blazor -o artifacts
dotnet pack src/Stil.Charts -o artifacts
dotnet pack src/Stil.Markdown -o artifacts
dotnet pack src/Stil.RichText -o artifacts
dotnet pack templates/Stil.Templates -o artifacts
```

### Try a local build in another app

Pack with a version of its own, add the folder as a package source once, and
reference that version from your app. Run the first two commands from the
repository root:

```shell
dotnet pack src/Stil.Blazor -o artifacts -p:Version=1.0.1-local.1
dotnet nuget add source "$PWD/artifacts" --name stil-local
```

```shell
dotnet add package Stil --version 1.0.1-local.1
```

NuGet caches a package by its version, so give every new pack a new version
(`1.0.1-local.2`, …). Or clear the cache with
`dotnet nuget locals global-packages --clear`. To stop using the folder, run
`dotnet nuget remove source stil-local`.

To test the stylesheet in any web page without a package, link
`src/Stil.Css/dist/stil.css` directly, or open the plain-HTML showcase.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the naming rules (classes, tokens,
variants, Blazor components), the scope model, browser support and the
workflow.

## License

[MIT](LICENSE)
