# Stil project plan

## Vision

Stil is a framework-independent CSS class library with an optional Blazor
component layer. Blazor components render ordinary HTML using the same CSS
classes; the stylesheet does not depend on Blazor.

## Current foundation

- `src/StilCSS.Blazor/wwwroot/stil.css` is the source stylesheet shipped in the
  NuGet package.
- `Stil.css` imports the package stylesheet and contains presentation used only
  by the standalone `Stil.html` showcase.
- `src/StilCSS.Blazor` is a .NET 10 Razor Class Library configured as the
  unpublished `Stil` NuGet package, currently version `0.1.0`.
- The library exposes `StilContainer`, `StilCard`, and `StilAlert` in the
  `Stil.Blazor.Components` namespace. Alert variants are success, error,
  warning, and info.
- `StilCssClasses` merges a consumer-provided `class` attribute with each
  component's required classes, preserving the `.stil-scope` styling.
- `samples/StilCSS.Demo` demonstrates the Blazor components and direct CSS
  classes.
- `tests/Stil.Blazor.Tests` uses xUnit and bUnit to verify rendered component
  content, attributes, scoped CSS classes, card headers, and all alert
  variants.
- The test suite currently contains 10 passing tests. Test coverage includes
  preservation of a consumer-provided `class` attribute, which previously
  replaced the required Stil classes.
- The static stylesheet is served at `_content/Stil/stil.css`.
- Theme and typography styles are opt-in and scoped by `.stil-scope`. The
  Blazor components add the scope to their rendered roots; plain HTML users
  should add it to a wrapper.
- `Stil.html` and `Stil.css` remain a useful standalone browser showcase of the
  framework-independent CSS API; they are separate from the runnable Blazor
  demo.

## Next steps

1. Stabilize the visual foundation: check color contrast, keyboard focus,
   responsive behavior, and consistent spacing.
2. Add reusable components as needed, starting with buttons, inputs,
   navigation, and form controls. Keep their class names aligned with the CSS
   API and support standard HTML attributes where practical.
3. Extend the component tests as new components and parameters are added. Add CI to build,
   test, pack, and verify the package stylesheet.
4. Improve examples and document direct CSS usage separately from the Blazor
   component API.
5. Decide whether to support frameworks before .NET 10, then publish the
   `Stil` package with its supported .NET version documented.
6. Consider a separate framework-free CSS package only if there is a concrete
   need to distribute it independently.

## Practical checks

Build the solution with `dotnet build Stil.sln` and run all tests with
`dotnet test Stil.sln`. Run the live demo with
`dotnet run --project samples/StilCSS.Demo/Stil.Demo.csproj --launch-profile http`
and open `http://localhost:5100`. The demo footer is styled by the host app
outside `.stil-scope`, so it is a manual check that host styles remain intact.
Open `Stil.html` from the repository root to inspect the standalone CSS
showcase without starting the Blazor app.

## Blazor usage

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

Include the package stylesheet in `Components/App.razor`:

```razor
<link rel="stylesheet" href="@Assets["_content/Stil/stil.css"]" />
```
