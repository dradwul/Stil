# Migrating from 0.1 to 1.0

This guide is for apps built on a `0.1.0-preview` version of Stil (`Stil`,
`Stil.Charts`, `Stil.Markdown`, `Stil.RichText` or `stil-css`). Before `1.0`
the names were reviewed once, and the ones that broke the library's own rules
were changed. This page lists every rename, with the new name next to the
old. Nothing was removed.

From `1.0` on, names change only in a major version: CI compares the public
.NET API with a baseline, and the stylesheet and script with a size budget
(see [CONTRIBUTING.md](CONTRIBUTING.md#the-public-api)).

## Steps

1. Update the packages to `1.0.0` (and `stil-css`, if you use it directly).
2. Search your project for the old names below, and replace them. The search
   is needed: see the next section.
3. Build, and look through the pages that use the renamed components.

The old names, as one search from the root of your project:

```bash
git grep -n -E "AriaLabel|\bTop=|CardTemplate|ChildrenSelector|<StilColumn\b|\bCurrent=|\.Read\b|\bGridlines=|--stil-(backdrop|start|length|col|cols|load)\b"
```

It also finds a few names that are not Stil's (an `AriaLabel` of your own
component, say); check each hit.

## Why the compiler does not find them all

Every Stil component passes the attributes it does not know on to its HTML
element (so `id`, `title` or `data-*` work). An old parameter name is such an
attribute: `<StilDropdown AriaLabel="Actions">` still compiles, and renders
`arialabel="Actions"`, which does nothing, so the menu loses its accessible
name. The same goes for the old CSS variables, which the browser ignores.

A renamed type (`StilColumn`) or member you call from C#
(`notification.Read`) is a compiler error, as usual.

## Stil for Blazor

| Was | Is now |
| --- | --- |
| `StilDropdown.AriaLabel` | `Label` |
| `StilPopover.AriaLabel` | `Label` |
| `StilPopover.Top` | `DropUp`, as on `StilDropdown` |
| `StilKanban.CardTemplate` | `ItemTemplate` |
| `StilTreeView.ChildrenSelector` | `Children` |
| `StilTreeGrid.ChildrenSelector` | `Children` |
| `StilColumn` (the column of `StilTable`) | `StilTableColumn` |
| `StilColumn.Field` | `StilTableColumn.Value` |
| `StilStepper.Current` | `Step`, as on `StilWizard` |
| `StilNotification.Read` | `IsRead` |

```razor
@* 0.1 *@
<StilDropdown Text="Actions" AriaLabel="Order actions" Items="_actions" />
<StilTable Items="_orders">
    <StilColumn TItem="Order" Title="Customer" Field="o => o.Customer" />
</StilTable>
<StilStepper Steps="_steps" Current="_step" />

@* 1.0 *@
<StilDropdown Text="Actions" Label="Order actions" Items="_actions" />
<StilTable Items="_orders">
    <StilTableColumn TItem="Order" Title="Customer" Value="o => o.Customer" />
</StilTable>
<StilStepper Steps="_steps" Step="_step" />
```

## Stil.Charts

| Was | Is now |
| --- | --- |
| `Gridlines` on `StilLineChart`, `StilBarChart` and `StilScatterChart` | `ShowGridlines` |

## Stil CSS

The variables you set yourself, in a theme or on an element:

| Was | Is now |
| --- | --- |
| `--stil-backdrop` (the color behind a dialog or drawer) | `--stil-color-backdrop` |
| `--stil-start`, `--stil-length` (where an event sits in a week or resource grid, in minutes from midnight) | `--stil-event-start`, `--stil-event-length` |
| `--stil-col`, `--stil-cols` (the column of an event that overlaps others) | `--stil-event-col`, `--stil-event-cols` |
| `--stil-load` (how booked a day is, from 0 to 1) | `--stil-day-load` |

```html
<!-- 0.1 -->
<div class="stil-event" style="--stil-start: 540; --stil-length: 30">Stand-up</div>

<!-- 1.0 -->
<div class="stil-event" style="--stil-event-start: 540; --stil-event-length: 30">Stand-up</div>
```

No class names changed. The `stil.js` exports listed in the
[Stil CSS README](src/Stil.Css/README.md) are its public API; the others are
used by Stil for Blazor and may change in any version.

## Changes you may see, without renames

- Dark text on colored backgrounds (buttons, badges, alerts) is a soft gray,
  `#3d3d3d`, instead of black. Where that gray would fall below 4.5:1 it
  darkens, so it still meets WCAG AA.
- With `stil.js`, open dropdown menus and the lists of `StilAutocomplete`,
  `StilMultiSelect` and `StilTimePicker` sit in the top layer, so a parent
  with `overflow` no longer cuts them off. A test that looks for the menu
  inside its parent element has to look in the whole page.
- A progress bar's label is centered on the whole track.
- Members that are public only so `stil.js` can call them (the
  `[JSInvokable]` methods, the `Stil…EventArgs` types and
  `StilConfirmDialogContent`) are hidden from IntelliSense. They still work,
  but they are not part of the API and may change.
- `StilCameraCapture`, `StilScanner`, `StilScanInput` and `StilSignaturePad`
  are *experimental*: their API may still change after `1.0`.

## Apps made with `dotnet new stil-app`

The template changes only new apps. An app made from an older template keeps
its own files (such as `OrderStore`, which new apps call `IOrderService`) and
needs only the renames above.
