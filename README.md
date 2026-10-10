# Stil

Stil is a framework-independent CSS class library with an optional Blazor
component layer. The `Stil` NuGet package contains both the Blazor components
and the matching stylesheet as a static web asset. The current package targets
.NET 10.

## Use in a Blazor app

Install the package:

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

The CSS classes can also be used directly in HTML without using the Blazor
components. `Stil.html` is a standalone showcase of the stylesheet.
