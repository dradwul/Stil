# StilCSS

StilCSS is a CSS class library with an optional Blazor component layer.
The `StilCSS.Blazor` NuGet package contains both the Blazor components and
the matching stylesheet as a static web asset.

## Use in a Blazor app

Install the `StilCSS.Blazor` package and include its stylesheet in the app's
`Components/App.razor` `<head>`:

```razor
<link rel="stylesheet" href="@Assets["_content/StilCSS.Blazor/stilcss.css"]" />
```

Then use the components:

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

The CSS classes can also be used directly in HTML without using the Blazor
components.
