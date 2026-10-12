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
can mix components and hand-written markup. You change the look by
overriding a small set of CSS custom properties (design tokens). You do not
need to rebuild or fork the library.

> **Status:** early preview (`0.1.0`, not yet published). Class names, token
> names and component APIs will change before `1.0`. See the
> [roadmap](ROADMAP.md).

## Quick look

Plain HTML:

```html
<link rel="stylesheet" href="stil.css" />

<div class="stil-scope">
    <div class="card">
        <h2>Welcome</h2>
        <button class="bttn bttn-primary">Get started</button>
    </div>
</div>
```

Blazor:

```razor
<StilCard HeaderText="Welcome">
    <StilAlert Type="StilAlertType.Success">Saved!</StilAlert>
</StilCard>
```

Your own theme:

```css
.stil-scope {
    --primary-color: #6741d9;
    --secondary-color: #d0bfff;
}
```

## Repository layout

```text
assets/              Logo and other shared brand files
src/Stil.Css/        Stil CSS: the stylesheet source (single source of truth)
src/Stil.Blazor/     Stil for Blazor: Razor class library, packed as NuGet "Stil"
samples/Stil.Html/   Plain-HTML showcase of every CSS class (no build needed)
samples/Stil.Demo/   Blazor web app showing the components
tests/               bUnit/xUnit tests for the Blazor components
ROADMAP.md           Phase plan
Stil.sln             Solution with library, demo and tests
```

The Blazor project does not keep its own copy of the stylesheet. It links
`src/Stil.Css/stil.css` and serves it as `_content/Stil/stil.css`.

## Develop

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```shell
dotnet build Stil.sln
dotnet test Stil.sln
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

1. Make CSS changes in `src/Stil.Css/stil.css`. Then check them in both
   `samples/Stil.Html/index.html` and the Blazor demo.
2. A new Blazor component gets a bUnit test in `tests/Stil.Blazor.Tests`
   and an example in `samples/Stil.Demo`.
3. Keep class names identical between the CSS library and the components.
   The components are a convenience layer, not a second design system.

## License

[MIT](LICENSE)
