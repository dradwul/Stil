# Stil

Stil is a framework-independent CSS class library with an optional Blazor
component layer. The `Stil` NuGet package contains both the Blazor components
and the matching stylesheet as a static web asset. The current package targets
.NET 10 and is not yet published to NuGet.

## Repository map

| File or folder | Purpose |
| --- | --- |
| `.gitattributes` | Normalizes text-file line endings in Git. |
| `.gitignore` | Excludes build output and local IDE files. |
| `LICENSE` | Defines the project's MIT license. |
| `README.md` | Package overview, repository map, and usage instructions; also included in the NuGet package. |
| `Stil-plan.md` | Records current implementation and planned work. |
| `Stil.sln` | Builds the library, demo, and component-test project together. |
| `Stil.css` | Standalone-showcase entry stylesheet: imports package styles and adds showcase-only styles. |
| `Stil.html` | Framework-independent, visual example of the CSS classes. Useful even with Blazor because it shows the CSS library without requiring a .NET app. |
| `stil-logo.svg` | Project logo used by the standalone showcase. |
| `src/StilCSS.Blazor` | Razor Class Library source. The directory retains its original name; the package and assembly are named `Stil` and `Stil.Blazor`. |
| `samples/StilCSS.Demo` | Runnable Blazor sample app. The directory retains its original name; the app is named `Stil.Demo`. |
| `tests/Stil.Blazor.Tests` | bUnit/xUnit rendering tests for the public Blazor components. |

### Source and demo contents

- `src/StilCSS.Blazor/Stil.Blazor.csproj` sets the package ID, assembly name,
  target framework, and package dependencies.
- `src/StilCSS.Blazor/_Imports.razor` makes the component namespace available
  to the library's Razor files.
- `src/StilCSS.Blazor/Components/StilContainer.razor`,
  `StilCard.razor`, and `StilAlert.razor` are the public layout, card, and
  alert components. `StilAlertType.cs` defines the alert variants.
- `src/StilCSS.Blazor/Components/StilCssClasses.cs` preserves the component's
  required CSS classes when a consumer supplies a custom `class` attribute.
- `src/StilCSS.Blazor/wwwroot/stil.css` is the scoped stylesheet shipped in the
  NuGet package.
- `samples/StilCSS.Demo/Stil.Demo.csproj` and `Program.cs` configure and start
  the sample app. `appsettings.json` configures sample logging and allowed
  hosts; `Properties/launchSettings.json` defines local HTTP/HTTPS launch
  profiles.
- `samples/StilCSS.Demo/Components/App.razor` provides the document shell and
  loads Stil and app styles. `Routes.razor` connects routes to the main layout.
  `_Imports.razor` holds shared Razor namespaces.
- `samples/StilCSS.Demo/Components/Layout/MainLayout.razor` supplies the
  layout, the showcase-colored demo canvas, and an app-styled footer outside
  Stil's scope.
- `samples/StilCSS.Demo/Components/Pages/Home.razor` demonstrates Stil
  components and direct CSS usage through the same Layout, Navigation, Colors,
  Buttons, Inputs, Cards, Alerts, Typography, Tables, Links, and Forms sections
  as `Stil.html`. `Error.razor` and `NotFound.razor` handle sample-app error and
  missing-page routes.
- `samples/StilCSS.Demo/wwwroot/app.css` styles the host app, including the
  footer outside Stil's scope.
- `tests/Stil.Blazor.Tests/Stil.Blazor.Tests.csproj` defines the test
  framework, bUnit, and library reference. `StilContainerTests.cs`,
  `StilCardTests.cs`, and `StilAlertTests.cs` exercise the rendered components.

`Stil.html` is not replaced by the Blazor demo. Use the Blazor app to exercise
the component API and `Stil.html` to preview the plain CSS API in a browser.
Both examples use Didact Gothic from Google Fonts. The Blazor library falls
back to sans-serif if an app does not load that optional web font.
`Stil.html` and `Stil.css` must stay beside the `src` directory because the
showcase stylesheet imports the library CSS by relative path.

## Build and test

Build every project in the solution and run its tests with:

```shell
dotnet build Stil.sln
dotnet test Stil.sln
```

Run just the component tests with:

```shell
dotnet test tests/Stil.Blazor.Tests/Stil.Blazor.Tests.csproj
```

Create a local NuGet package with:

```shell
dotnet pack src/StilCSS.Blazor/Stil.Blazor.csproj
```

## Try it manually

1. Run the Blazor sample:

   ```shell
   dotnet run --project samples/StilCSS.Demo/Stil.Demo.csproj --launch-profile http
   ```

   Open `http://localhost:5100`. Use the section navigation to inspect the
   showcase examples and rendered results. Cards and alerts use the Blazor
   components; the other examples show the CSS classes directly. The demo
   loads Didact Gothic and uses the showcase teal as demo-only presentation;
   neither is applied globally by the library. The
   demo-app footer is outside `.stil-scope` and retains its styling from
   `wwwroot/app.css`; use browser developer tools to add a temporary style to
   the footer and confirm Stil does not override it.
2. On Windows, open the standalone CSS showcase from the repository root:

   ```powershell
   Start-Process .\Stil.html
   ```

   Use its section links and inspect the rendered CSS examples. No Blazor server
   is needed for this page.

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
