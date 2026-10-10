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
- `samples/StilCSS.Demo` demonstrates the Blazor components and direct CSS
  classes.
- The static stylesheet is served at `_content/Stil/stil.css`.
- Theme and typography styles are opt-in and scoped by `.stil-scope`. The
  Blazor components add the scope to their rendered roots; plain HTML users
  should add it to a wrapper.

## Next steps

1. Stabilize the visual foundation: check color contrast, keyboard focus,
   responsive behavior, and consistent spacing.
2. Add reusable components as needed, starting with buttons, inputs,
   navigation, and form controls. Keep their class names aligned with the CSS
   API and support standard HTML attributes where practical.
3. Add focused component tests for rendering, parameters, and alert variants.
   Build and pack in CI, and verify the stylesheet is included in the package.
4. Improve examples and document direct CSS usage separately from the Blazor
   component API.
5. Decide whether to support frameworks before .NET 10, then publish the
   `Stil` package with its supported .NET version documented.
6. Consider a separate framework-free CSS package only if there is a concrete
   need to distribute it independently.

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
