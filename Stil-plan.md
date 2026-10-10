# Stil — project plan

## Vision

Stil starts as a small, framework-independent CSS class library and adds an
optional Blazor component layer. The Blazor components should render ordinary
HTML using the same CSS classes, not make the CSS depend on Blazor.

The funeral-agency dashboard is a separate consumer of the library. It can
guide which general-purpose components are useful, but agency-specific
workflows and business rules belong in the dashboard rather than Stil.

## Current foundation

- `src/StilCSS.Blazor/wwwroot/stil.css` is the single source of truth for the
  styles. `Stil.css` imports it for the standalone HTML showcase.
- `src/StilCSS.Blazor` is a Razor Class Library targeting .NET 10 and is
  configured as the `Stil` NuGet package, initially at version `0.1.0`.
- `Stil.Blazor` currently contains `StilContainer`, `StilCard`, and
  `StilAlert`, with `StilAlertType` values for success, error, warning, and info.
- `samples/StilCSS.Demo` is a small Blazor app that references the library and
  demonstrates components alongside direct CSS-class usage.
- `Stil.html` remains the standalone HTML/CSS showcase.

## Suggested next steps

1. **Stabilize the visual foundation**
   - Define consistent spacing, typography, color, focus, and responsive tokens.
   - Check contrast and keyboard focus, especially for alert and button colors.
   - Decide which existing global styles should remain global and which should
     be scoped to the demo.
2. **Build the core component set**
   - Add components only as needed, starting with buttons, inputs, navigation,
     and form controls.
   - Keep public APIs predictable: `ChildContent` for nested content, typed
     enums for variants, and standard HTML attributes where practical.
   - Keep each component's rendered class names documented and consistent with
     the plain CSS API.
3. **Add quality checks**
   - Add focused component tests for rendering, parameters, and alert variants.
   - Build and pack the RCL in CI; inspect the `.nupkg` to confirm the stylesheet
     is present at `_content/Stil/stil.css`.
   - Add accessibility and responsive checks for the demo.
4. **Improve examples and docs**
   - Show a rendered preview and matching HTML or Razor snippet for each
     component.
   - Document package installation and the stylesheet inclusion requirement.
   - Document direct CSS usage separately from the Blazor component API.
5. **Try it in the dashboard**
   - Consume Stil from the separate dashboard project.
   - Promote only reusable patterns to Stil; keep funeral-agency domain
     concepts and data handling in the dashboard.
6. **Publish when the API is ready**
   - Choose and document supported .NET versions before promising compatibility.
   - Start with one package containing the components and static CSS asset.
   - Split a framework-free CSS package out only if there is a concrete need to
     distribute it independently.

## Current usage

```razor
<StilContainer>
    <StilCard HeaderText="Header">
        Bodytext här
    </StilCard>

    <StilAlert Type="StilAlertType.Error">
        Errormeddelande blabla
    </StilAlert>
</StilContainer>
```

The demo app includes the static asset from `Components/App.razor`:

```razor
<link rel="stylesheet" href="@Assets["_content/Stil/stil.css"]" />
```
