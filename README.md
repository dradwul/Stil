# Stil

Stil is a framework-independent CSS class library with an optional Blazor
component layer. The `Stil` NuGet package contains both the Blazor components
and the matching stylesheet as a static web asset. The current package targets
.NET 10 and is not yet published to NuGet.

## Repository files

- `Stil.sln` builds the library and its demo app.
- `src/StilCSS.Blazor` contains the Razor Class Library and the package
  stylesheet.
- `samples/StilCSS.Demo` demonstrates the Blazor components.
- `Stil.html` and `Stil.css` are a standalone CSS showcase. The root stylesheet
  imports the package stylesheet and adds showcase-only presentation.
- `stil-logo.svg` is the project logo.
- `Stil-plan.md` records project status and future work.
- `.gitignore`, `.gitattributes`, and `LICENSE` provide repository
  configuration and licensing.

Build the solution with `dotnet build Stil.sln`. Create a local package with:

```shell
dotnet pack src/StilCSS.Blazor/Stil.Blazor.csproj
```

## Use in a Blazor app

After the package is published, install it with:

```shell
dotnet add package Stil
```

Include its stylesheet in the app's `Components/App.razor` `<head>`:

```razor
<link rel="stylesheet" href="@Assets["_content/Stil/stil.css"]" />
```

Then use the components from the `Stil.Blazor.Components` namespace:

```razor
<StilContainer>
    <StilCard HeaderText="Header">
        Body text here
    </StilCard>

    <StilAlert Type="StilAlertType.Error">
        An error message
    </StilAlert>
</StilContainer>
```

The components scope Stil's theme and typography to their rendered content. To
use CSS classes without the components, wrap your markup in `class="stil-scope"`:

```html
<div class="stil-scope">
    <button class="bttn bttn-primary">Save</button>
</div>
```

`Stil.html` is a standalone showcase of the CSS classes.
