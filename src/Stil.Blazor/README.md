# Stil for Blazor

Stil for Blazor is a set of themeable Blazor components built on
[Stil CSS](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css). It
includes buttons, form fields with validation, lists, cards, alerts, tabs,
dropdowns, dialogs and toasts. Every component renders plain HTML with
`stil-*` classes. You can mix the components with hand-written Stil markup,
and theme both with the same `--stil-*` tokens.

> **Status:** early preview (`0.1.0`, not yet on NuGet). Requires .NET 10.
> APIs may still change before `1.0`. See the
> [roadmap](https://github.com/dradwul/StilCSS/blob/main/ROADMAP.md).

## Get started

1. Install the package:

   ```shell
   dotnet add package Stil
   ```

2. Register the services in `Program.cs`:

   ```csharp
   builder.Services.AddStil();
   // Optional global theme:
   // builder.Services.AddStil(o => o.Theme.Primary = "#6741d9");
   ```

3. Add the stylesheet (and the global theme) to the `<head>` in `Components/App.razor`:

   ```razor
   <StilHead />
   ```

   `StilHead` links `_content/Stil/stil.min.css`. To link the file yourself,
   use `<link rel="stylesheet" href="@Assets["_content/Stil/stil.css"]" />`.
   Add `class="stil-scope"` to `<body>` so Stil also styles the font,
   headings and links on your pages.

4. Add the namespaces to `_Imports.razor`:

   ```razor
   @using Stil.Blazor
   @using Stil.Blazor.Components
   ```

5. If you use dialogs or toasts from code, add the hosts once to an
   interactive layout:

   ```razor
   <StilDialogHost />
   <StilToastHost />
   ```

The library also ships a small script, `stil.js`, which Blazor loads
automatically. It closes dropdowns on an outside click or Escape and handles
dismissing alerts on statically rendered pages. You do not need to add a
`<script>` tag.

## Render modes

| Works in static SSR | Needs an interactive render mode (Server, WebAssembly or Auto) |
| --- | --- |
| Button (links and submit), Badge, Card, Container, Heading, List, ListItem (as a link), Progress, Spinner, Tooltip, ThemeProvider, Head, form fields inside an enhanced `EditForm`, Dropdown, dismissible Alert | Button `OnClick`, ListItem `OnClick`, Tabs switching, Dialog, DialogService, ToastService |

## Components

The color components share the `StilVariant` enum: `Primary`, `Secondary`,
`Success`, `Danger`, `Warning` and `Info`. Sizes use `StilSize`: `Medium`,
`Small` and `Large`. Every component passes extra HTML attributes to its root
element. A `class` you supply is added to the component's own classes and
never replaces them.

### Buttons and badges

```razor
<StilButton OnClick="Save">Save</StilButton>
<StilButton Variant="StilVariant.Danger" Appearance="StilButtonAppearance.Outline">Delete</StilButton>
<StilButton Loading="_saving" Type="submit">Save</StilButton>
<StilButton Href="/orders" Appearance="StilButtonAppearance.Ghost">All orders</StilButton>
<StilBadge Variant="StilVariant.Success">Delivered</StilBadge>
```

| Component | Parameters |
| --- | --- |
| `StilButton` | `Variant`, `Appearance` (Solid, Outline, Ghost), `Size`, `Type` (default "button"), `Disabled`, `Loading` (spinner, ignores clicks), `Pressed` (toggle, aria-pressed), `IconOnly`, `Href`/`Target` (renders a link), `OnClick` |
| `StilBadge` | `Variant`, `Subtle` |

### Lists

A row with a title, an optional description and content at the start or end.
On its own, a row is a bordered box; inside a `StilList` the rows are joined.
A row with `Href` becomes a link, and one with `OnClick` becomes a button.

```razor
<StilListItem Title="Order #1042" Badge="Delivered" BadgeVariant="StilVariant.Success" />

<StilList>
    @foreach (var order in orders)
    {
        <StilListItem Title="@($"Order #{order.Id}")" Description="@order.Customer"
                      Badge="@order.Status" BadgeVariant="@VariantOf(order.Status)"
                      Href="@($"orders/{order.Id}")" />
    }
</StilList>
```

| Component | Parameters |
| --- | --- |
| `StilList` | `Flush` (no outer border, for use inside a card) |
| `StilListItem` | `Title`, `Description`, `Start`, `End`, `Badge`/`BadgeVariant`/`BadgeSubtle`, `Href`, `OnClick`, `Selected` (aria-current), `Disabled`, `ChildContent` |

### Cards, alerts and layout

```razor
<StilCard Title="Team" Subtitle="4 members" HeadingLevel="3">
    <HeaderActions><StilButton Size="StilSize.Small">Invite</StilButton></HeaderActions>
    <ChildContent>…</ChildContent>
    <Footer><StilButton>Save</StilButton></Footer>
</StilCard>

<StilAlert Variant="StilVariant.Warning" Title="Heads up" Subtle Dismissible>Maintenance on Sunday.</StilAlert>
```

| Component | Parameters |
| --- | --- |
| `StilCard` | `Title`, `Subtitle`, `HeadingLevel` (default 2), `HeaderActions` (moves the title into a header bar), `Footer` |
| `StilAlert` | `Variant` (default Info), `Subtle`, `Title`, `Dismissible`, `OnDismiss`, `Role`. Danger and Warning use `role="alert"`; the others use `"status"`. |
| `StilContainer` | `Fluid` |
| `StilHeading` | `Level` (1–6) |

### Forms

Each field renders a label, the control, an optional hint and its validation
messages. It wires up `id`, `for`, `aria-describedby` and `aria-invalid`.
Bind with `@bind-Value`. Inside an `EditForm`, validation messages appear
automatically; fields also work without one.

```razor
<EditForm Model="order" OnValidSubmit="Save" class="stil-form">
    <DataAnnotationsValidator />
    <StilTextBox @bind-Value="order.Customer" Label="Customer" />
    <StilTextBox @bind-Value="order.Email" Label="Email" Type="email" Hint="We send the receipt here." />
    <div class="stil-grid">
        <StilNumeric @bind-Value="order.Amount" Label="Amount" Step="0.01" class="stil-col-md-6" />
        <StilDatePicker @bind-Value="order.Date" Label="Date" class="stil-col-md-6" />
    </div>
    <StilSelect @bind-Value="order.Status" Label="Status" />              @* enum: options generated *@
    <StilRadioGroup @bind-Value="order.Shipping" Label="Shipping" Inline />
    <StilSwitch @bind-Value="order.Express" Label="Express delivery" />
    <StilCheckbox @bind-Value="order.Gift" Label="This is a gift" />
    <StilButton Type="submit">Save</StilButton>
</EditForm>
```

| Component | Value type | Extra parameters |
| --- | --- | --- |
| `StilTextBox` | `string?` | `Type` (text, email, password, …), `Placeholder`, `Size` |
| `StilTextArea` | `string?` | `Rows`, `Placeholder` |
| `StilNumeric<TValue>` | numbers | `Min`, `Max`, `Step`, `Placeholder`, `Size` |
| `StilDatePicker<TValue>` | `DateTime`, `DateOnly`, `TimeOnly`, … | `Type` (Date, DateTimeLocal, Month, Time), `Min`, `Max`, `Size` |
| `StilSelect<TValue>` | any | `Items`, `ItemText`, `Placeholder`, `Size`, or `<option>` elements as child content. Enums need no items. |
| `StilRadioGroup<TValue>` | any | `Items`, `ItemText`, `Inline`. Enums need no items. |
| `StilCheckbox`, `StilSwitch` | `bool` | Rich label as child content |

All fields share `Label`, `Hint`, `Id` (generated if not set), `Disabled` and
`InputClass` (classes on the control).

**Attributes on fields:** `class` and `style` go on the field wrapper, so
`class="stil-col-md-6"` places the whole field in a grid. Other attributes,
such as `placeholder`, `autocomplete` and `aria-label`, go on the control.

`StilField` is the wrapper the fields use. Use it to give a custom control
the same label, hint and error layout.

### Tabs, dropdowns and tooltips

```razor
<StilTabs @bind-ActiveIndex="tab" Label="Settings">
    <StilTab Title="Profile">…</StilTab>
    <StilTab Title="Security">…</StilTab>
</StilTabs>

<StilDropdown Text="Actions" Appearance="StilButtonAppearance.Outline" AlignEnd>
    <StilDropdownHeader>Order #1043</StilDropdownHeader>
    <StilDropdownItem OnClick="Edit">Edit</StilDropdownItem>
    <StilDropdownDivider />
    <StilDropdownItem OnClick="Delete">Delete</StilDropdownItem>
</StilDropdown>

<StilTooltip Text="Copies the link"><StilButton>Share</StilButton></StilTooltip>
```

| Component | Parameters |
| --- | --- |
| `StilTabs` | `ActiveIndex` (two-way), `Label`. Arrow keys, Home and End move between tabs; disabled tabs are skipped. |
| `StilTab` | `Title`, `Disabled`. Only the selected tab's content is rendered. |
| `StilDropdown` | `Text` or `Toggle`, `AriaLabel`, `Variant`, `Appearance`, `Size`, `IconOnly`, `AlignEnd` |
| `StilDropdownItem` | `Href` or `OnClick`, `Disabled` |
| `StilTooltip` | `Text`, `Position` (Top, Bottom). Screen readers do not read it, so the wrapped element needs its own accessible name. |

### Progress and spinners

```razor
<StilProgress Value="uploaded" Max="total" Label="Upload" />
<StilSpinner Size="StilSize.Small" Label="Loading orders…" />
```

| Component | Parameters |
| --- | --- |
| `StilProgress` | `Value`, `Max` (default 100), `Label`, `ShowValue`, `Variant`, `Size` (Medium, Large) |
| `StilSpinner` | `Size` (Medium, Small), `Variant`, `Label` (for screen readers) |

### Dialogs

A dialog in markup, bound to a field:

```razor
<StilButton OnClick="() => open = true">Invite</StilButton>

<StilDialog @bind-Open="open" Title="Invite a member">
    <ChildContent>…</ChildContent>
    <Footer>
        <StilButton Appearance="StilButtonAppearance.Ghost" OnClick="() => open = false">Cancel</StilButton>
        <StilButton OnClick="Invite">Send</StilButton>
    </Footer>
</StilDialog>
```

It is a native modal `<dialog>`: focus stays inside it, Escape closes it, and
the page behind it is inert. Parameters: `Open`, `Title`, `Size`,
`ShowClose`, `CloseLabel`, `OnClose`, `Footer`.

Dialogs from code, through `IStilDialogService`:

```razor
@inject IStilDialogService Dialogs

var confirmed = await Dialogs.ConfirmAsync("Delete order?", "This cannot be undone.",
    new StilConfirmOptions { ConfirmText = "Delete", ConfirmVariant = StilVariant.Danger });

var result = await Dialogs.OpenAsync<EditCustomer>("Edit customer",
    new Dictionary<string, object?> { ["CustomerId"] = 42 });
```

A component opened with `OpenAsync` receives a cascading
`StilDialogReference`. Call `Close(result)` on it to return a value. For a
destructive confirmation (Danger), focus starts on Cancel.

### Toasts

```razor
@inject IStilToastService Toasts

Toasts.Success("Order #1043 shipped.");
Toasts.Error("The payment provider did not respond.", title: "Payment failed");
Toasts.Show("Saved", duration: TimeSpan.Zero);   // stays until closed
```

Toasts close after `StilOptions.DefaultToastDuration` (5 seconds). Danger and
Warning toasts use `role="alert"`; the others use `"status"`.

## Theming

Use `AddStil(o => o.Theme = …)` with `<StilHead />` for the whole app, or
`StilThemeProvider` for part of a page:

```razor
<StilThemeProvider Mode="StilThemeMode.Auto"
                   Theme="@(new StilTheme { Primary = "#6741d9", Radius = "0.5rem" })">
    …
</StilThemeProvider>
```

`StilTheme` has `Primary`, `Secondary`, `Success`, `Danger`, `Warning`,
`Info`, `Accent`, `FontFamily`, `Radius` and `Tokens` (any `--stil-*` token).
Text on colored components, link colors and focus rings are derived from the
tokens and stay WCAG AA readable. Put `StilDialogHost` and `StilToastHost`
inside the provider so dialogs and toasts follow its theme.

You can also override tokens in CSS. Stil's rules are in cascade layers, so
your CSS always wins. See the
[theming guide and full token list](https://github.com/dradwul/StilCSS/tree/main/src/Stil.Css#theming).

## Development

The component source is in this folder. The tests are in
`tests/Stil.Blazor.Tests`, and a runnable demo, with a component gallery and
a CRUD page, is in `samples/Stil.Demo`. The stylesheet and `stil.js` are
served from `src/Stil.Css/dist/`, which is built from `src/Stil.Css/src/`.
Naming rules are in
[CONTRIBUTING.md](https://github.com/dradwul/StilCSS/blob/main/CONTRIBUTING.md).
