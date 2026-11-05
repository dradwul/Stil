| `StilChat<TItem>` | The activity log's selectors (`Title` is the text) plus `Mine`, `Avatar`, `ItemKey`, `OnSend` (`StilChatMessage`: `Text`, `Files`), `Typing`, `OnTyping`, `AllowAttachments`, `SendOnEnter`, `Height` (28rem), `Disabled`. Oldest first with day separators; stays at the newest message. The transport is the app's. |
| `StilComments<TItem>` | The thread form of `StilChat`: no sides, a taller composer, Ctrl+Enter posts. |
| `StilActivityLog<TItem>` | `Items` with `Time`, `Title`, `Actor`, `Type`, `Variant`, `Icon`, `Details` / `DetailsTemplate`; `ItemKey`, `Searchable`, `Filterable`, `Height`, `Virtualize` (`ItemSize`), `Now`. Newest first, grouped by day, relative times; new entries are highlighted and announced. |
| `StilTimeline<TItem>` | The same selectors, compact and oldest first (`NewestFirst`), `Label`. Works in static SSR. |
# Stil for Blazor

Stil for Blazor is a set of themeable Blazor components built on
[Stil CSS](https://github.com/dradwul/Stil/tree/main/src/Stil.Css). It
includes buttons, icons, form fields with validation, lists, tables, cards,
alerts, navigation, tabs, dropdowns, accordions, dialogs, drawers and toasts. Every component renders plain HTML with
`stil-*` classes. You can mix the components with hand-written Stil markup,
and theme both with the same `--stil-*` tokens.

> **Status:** early preview (`0.1.x`, prereleases on NuGet). Requires .NET 10.
> APIs may still change before `1.0`. See the
> [roadmap](https://github.com/dradwul/Stil/blob/main/ROADMAP.md).

## Get started

For a new app, `dotnet new stil-app` from
[Stil.Templates](https://github.com/dradwul/Stil/tree/main/templates/Stil.Templates)
does all of these steps. To add Stil to an existing Blazor Web App:

1. Install the package:

   ```shell
   dotnet add package Stil --prerelease
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
automatically. It closes dropdowns on an outside click or Escape, handles
dismissing alerts on statically rendered pages, moves between the days of a
calendar month, drags events in an editable week planner, and stops the keys
that trees, tabs and pickers handle from also scrolling the page or submitting
a form. You do not need to add a
`<script>` tag.

## Render modes

| Works in static SSR | Needs an interactive render mode (Server, WebAssembly or Auto) |
| --- | --- |
| Button (links and submit), Badge, Card, StatCard, Container, Heading, Icon, List, ListItem (as a link), Avatar, Table, DataGrid (first page, initial sort), TreeView (initial state), NavGroup, Breadcrumbs, Pagination with `HrefTemplate`, Calendar, WeekPlanner and Agenda (navigation with `HrefTemplate`), Nav, NavLink, Navbar (the menu button works through stil.js), Accordion, the layout components, Progress, Spinner, Tooltip, ThemeProvider, Head, form fields inside an enhanced `EditForm`, Dropdown, dismissible Alert | Button `OnClick`, ListItem `OnClick`, Table `OnRowClick`, Pagination with buttons, Tabs switching, `@bind-Open` on Dropdown and AccordionItem, Dialog, Drawer, DialogService, ToastService, DataGrid sorting, filters, paging and selection, Autocomplete, MultiSelect, TreeView keyboard and selection, FileUpload (the drag highlight and remove buttons), Calendar day selection, event clicks and dragging in WeekPlanner |

## Components

The color components share the `StilVariant` enum: `Primary`, `Secondary`,
`Success`, `Danger`, `Warning` and `Info`. Sizes use `StilSize`: `Medium`,
`Small` and `Large`. Every component passes extra HTML attributes to its root
element. A `class` you supply is added to the component's own classes and
never replaces them.

### Icons

```razor
<StilIcon Name="@StilIcons.Search" />                   @* decorative, aria-hidden *@
<StilIcon Name="@StilIcons.Warning" Label="Warning" Size="24px" />
<StilButton><StilIcon Name="@StilIcons.Plus" /> New order</StilButton>
```

`StilIcon` draws Stil's built-in icons as inline SVG in the current text
color. They are 1em in size, so they follow the text unless `Size` is set.
`StilIcons` lists the names. To use another icon set, register a provider;
names it does not know fall back to the built-in icons:

```csharp
// Material Symbols (Apache 2.0): load its stylesheet in App.razor, then
builder.Services.AddStil(o => o.IconProvider = new StilLigatureIconProvider("material-symbols-outlined"));
// <StilIcon Name="shopping_cart" />
```

Implement `IStilIconProvider` for SVG sprites or other sets, or put any
markup inside `<StilIcon>…</StilIcon>`.

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
| `StilButtonGroup` | `Label`. Joins its buttons into one control (`role="group"`). |
| `StilCloseButton` | `Label` (default "Close"), `OnClick` |

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
| `StilAvatar` | `Name` (accessible name and initials), `ImageSrc`, `Size` |
| `StilAccordion` | `Multiple` (allow several open; by default one at a time, without JavaScript), `Flush` |
| `StilAccordionItem` | `Title` or `Header`, `@bind-Open` |
| `StilGrid` / `StilCol` | `Gap` (0–7) / `Span`, `SpanSm`, `SpanMd`, `SpanLg`, `SpanXl` (1–12) |
| `StilStack`, `StilCluster` | `Gap` (0–7), `Align`; `StilCluster` also `Justify` |
| `StilSidebarLayout` | `Sidebar`, `SidebarWidth`, `SidebarLabel`. The sidebar sits beside the content from the lg breakpoint. |

```razor
<StilGrid>
    <StilCol SpanMd="8">Main</StilCol>
    <StilCol SpanMd="4">Aside</StilCol>
</StilGrid>

<StilAccordion>
    <StilAccordionItem Title="How long does shipping take?" Open>Two to four days.</StilAccordionItem>
    <StilAccordionItem Title="Can I return an order?">Within 30 days.</StilAccordionItem>
</StilAccordion>
```

### Tables

```razor
<StilTable Items="orders" Hover Caption="Recent orders" OnRowClick="Open">
    <Columns>
        <StilColumn Title="Order" Field="@(o => $"#{o.Id}")" />
        <StilColumn Title="Customer" Field="@(o => o.Customer)" />
        <StilColumn Title="Amount" Field="@(o => o.Amount)" Format="N2" Align="StilTextAlign.End" />
        <StilColumn Title="Status">
            <Template><StilBadge Variant="StilVariant.Success">@context.Status</StilBadge></Template>
        </StilColumn>
    </Columns>
</StilTable>
```

| Component | Parameters |
| --- | --- |
| `StilTable<TItem>` | `Items`, `Columns`, `Caption`, `EmptyContent`, `Striped`, `Hover`, `Compact`, `Bordered`, `Responsive` (default true), `OnRowClick`, `RowClass` |
| `StilColumn<TItem>` | `Title`, `Field` (with `Format`) or `Template`, `Align`, `Class` |

The table is a lightweight display component. For sorting, filtering,
paging and selection, use `StilDataGrid`.

### Data grid

`StilDataGrid` is a `stil-table` with sortable headers, a filter row, paging,
row selection and virtualization. Columns bind to a property, so the same
expression shows, sorts and filters the column, also against an `IQueryable`
such as an EF Core `DbSet`.

```razor
<StilDataGrid TItem="Order" Items="orders" Filterable PageSize="10" PageSizeOptions="[10, 25, 50]"
              SelectionMode="StilSelectionMode.Multiple" @bind-SelectedItems="selected" ItemKey="o => o.Id">
    <Toolbar><StilButton OnClick="Export">Export</StilButton></Toolbar>
    <Columns>
        <StilDataGridColumn Property="o => o.Id" Title="Order" InitialSort="StilSortDirection.Descending" />
        <StilDataGridColumn Property="o => o.Customer" />
        <StilDataGridColumn Property="o => o.Status">
            <Template><StilBadge Variant="@VariantOf(context.Status)">@context.Status</StilBadge></Template>
        </StilDataGridColumn>
        <StilDataGridColumn Property="o => o.Amount" Format="N2" Align="StilTextAlign.End" />
        <StilDataGridTemplateColumn Title="Actions">
            <StilButton Size="StilSize.Small" OnClick="() => Edit(context)">Edit</StilButton>
        </StilDataGridTemplateColumn>
    </Columns>
</StilDataGrid>
```

| Component | Parameters |
| --- | --- |
| `StilDataGrid<TItem>` | `Items` or `ItemsProvider`, `Columns`, `Toolbar`, `ResizableColumns`, `ReorderableColumns`, `ShowColumnChooser`, `@bind-Layout` (`StilDataGridLayout`: order, widths, hidden columns, sort; save it to keep the user's layout), `EditMode` (Row or Cell, with `StilDataGridEditColumn`, `OnRowSave` returning false to keep editing, `OnRowCancel`, `AddRowAsync()`; columns have `Editable` and `EditTemplate`), `@bind-GroupBy` and `ShowGroupChooser` (group headers with counts and totals that collapse and expand; `ExpandAllGroupsAsync()`, `CollapseAllGroupsAsync()`), `ShowExport`/`ExportFileName` (a CSV download of the filtered rows; `ExportCsvAsync()` returns the text), `TotalText`, `PageSize` (default 10; 0 shows every row), `PageSizeOptions`, `Filterable`, `SelectionMode` (None, Single, Multiple) with `@bind-SelectedItem` or `@bind-SelectedItems`, `ItemKey`, `OnRowClick`, `RowClass`, `Virtualize` with `Height` and `ItemSize`, `Caption`, `Striped`, `Compact`, `Bordered`, `EmptyContent`, and texts for translation (`EmptyText`, `LoadingText`, `SummaryFormat`, `FilterPlaceholder`, …). `RefreshAsync()` reloads the rows. |
| `StilDataGridColumn<TItem, TValue>` | `Property`, `Title` (default: the property name), `Format`, `Aggregate` (Sum, Average, Count, Min, Max in a totals row), `Frozen` (Start, End), `Resizable`, `Reorderable`, `Hideable`, `MinWidth`, `MaxWidth`, `AggregateValue`, `AggregateFormat`, `ExportValue`, `Exportable`, `Template`, `Sortable` (default true), `Filterable` (default: the grid's), `InitialSort`, `Align`, `Width`, `Class`, `Key`, `TrueText`/`FalseText` |
| `StilDataGridTemplateColumn<TItem>` | Custom content (`context` is the row), `Title`, `Align`, `Width`, `AggregateValue`, `ExportValue` (exported only when set). Not sortable or filterable. |

- **Sorting:** clicking a header sorts ascending, then descending, then not at
  all. The header has `aria-sort`.
- **Filters:** text columns get a case-insensitive "contains" filter that waits
  for typing to pause. Enum and bool columns get a choice. Other types cannot be
  filtered.
- **Selection:** in Single mode a click (or Enter on a focused row) selects the
  row. In Multiple mode a checkbox column appears, and the header checkbox
  selects every row on the page. Set `ItemKey` when rows are new objects on
  every load, so the selection is kept.
- **Server-side data:** `ItemsProvider` receives a `StilDataGridRequest` with
  `StartIndex`, `Count`, `SortBy`, `SortDirection`, `Filters` and a
  `CancellationToken`. For an `IQueryable`, let the grid build the query:

```csharp
private async Task<StilDataGridResult<Order>> LoadOrders(StilDataGridRequest<Order> request)
{
    var query = request.ApplyFiltersAndSorting(db.Orders.AsNoTracking());
    var total = await query.CountAsync(request.CancellationToken);
    var items = await request.ApplyPaging(query).ToListAsync(request.CancellationToken);
    return new(items, total);
}
```

- **Virtualization:** `Virtualize` with a `Height` (for example `"28rem"`)
  renders only the visible rows of a long list instead of pages. The header
  stays visible while scrolling. Rows should all have the same height
  (`ItemSize`, default 41 px).

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

`StilTextBox`, `StilNumeric`, `StilSelect` and `StilDatePicker` can have add-ons
joined to the control: `PrependText`/`AppendText` for text such as
`https://` or `€`, or `Prepend`/`Append` for content such as a button:

```razor
<StilNumeric @bind-Value="price" Label="Price" AppendText="€" />
<StilTextBox @bind-Value="query" Label="Search">
    <Append><StilButton OnClick="Search">Search</StilButton></Append>
</StilTextBox>
```

Fields also work without `@bind-Value`, with `Value` plus `ValueChanged`.

All fields share `Label`, `Hint`, `Id` (generated if not set), `Disabled` and
`InputClass` (classes on the control).

**Attributes on fields:** `class` and `style` go on the field wrapper, so
`class="stil-col-md-6"` places the whole field in a grid. Other attributes,
such as `placeholder`, `autocomplete` and `aria-label`, go on the control.

`StilField` is the wrapper the fields use. Use it to give a custom control
the same label, hint and error layout.

### Pickers, date ranges and file upload

```razor
<StilAutocomplete TItem="Customer" @bind-Value="order.Customer" Search="FindCustomersAsync"
                  ItemText="c => c.Name" ItemKey="c => c.Id" Label="Customer" />
<StilMultiSelect TItem="string" @bind-Value="order.Tags" Items="allTags" Label="Tags" Placeholder="Add tags" />
<StilDateRangePicker TValue="DateOnly?" @bind-Start="from" @bind-End="to" Label="Delivery window" />
<StilFileUpload Label="Attachments" Multiple Accept=".pdf,image/*" MaxFileSize="5_000_000" OnChange="FilesChanged" />
```

| Component | Value | Parameters |
| --- | --- | --- |
| `StilAutocomplete<TItem>` | `TItem` | `Items` (searched in memory) or `Search` (`(text, cancellationToken) => Task<IEnumerable<TItem>>`), `ItemText`, `ItemTemplate`, `ItemKey`, `MinLength` (default 1 with Search, 0 with Items), `Debounce` (ms, default 250), `MaxResults` (default 50), `Placeholder`, `Size`, `NoResultsText`, `SearchingText` |
| `StilMultiSelect<TItem>` | `IReadOnlyCollection<TItem>` | The same as `StilAutocomplete`, plus `RemoveLabelFormat`. The chosen options are chips; Backspace in the empty input removes the last one. |
| `StilDateRangePicker<TValue>` | `@bind-Start`, `@bind-End` (`DateOnly`, `DateTime` or their nullables) | `Label` (a legend), `StartLabel`/`EndLabel`, `Min`/`Max`, `Hint`, `Size`, `RangeErrorText`. An end before the start is a validation error that blocks `OnValidSubmit`. |
| `StilFileUpload` | `OnChange` (`IReadOnlyList<IBrowserFile>`) | `Multiple`, `Accept`, `MaxFileSize`, `MaxFiles`, `Icon`, `Text`, `ButtonText`, texts for errors. A drop zone around `InputFile`; rejected files are listed as errors. `Files` and `Clear()` are public. |

The pickers follow the WAI-ARIA combobox pattern: the input keeps focus, the
arrow keys move the active option, Enter chooses it and Escape closes the
list. They are form fields, so labels, hints, validation and `class` on the
wrapper work as for the other fields.

### More inputs

```razor
<StilDateTimePicker @bind-Value="booking.Start" Label="Start" Step="15" />
<StilSlider @bind-Value="booking.Minutes" Label="Duration" Min="15" Max="180" Step="15" Format="0 'min'" />
<StilRating @bind-Value="review.Score" Label="How was it?" />
<StilMaskedInput @bind-Value="person.Number" Label="Personal identity number" Mask="00000000-0000" />
<StilPinInput @bind-Value="code" Label="Code" OnComplete="VerifyAsync" />
<StilPopover Text="What is this?" Title="Order number">The number on your receipt.</StilPopover>
```

| Component | Value | Parameters |
| --- | --- | --- |
| `StilTimePicker<TValue>` | `TimeOnly` or `TimeOnly?` | `Step` (minutes, default 30), `Min`, `Max`, `Culture`, `Placeholder`, `Size`. Suggests times in steps and takes typed times ("1430", "930"). |
| `StilDateTimePicker<TValue>` | `DateTime` or `DateTime?` | `Step`, `Min`/`Max` (dates), `DefaultTime` (09:00), `Culture`, `DateLabel`/`TimeLabel`, `Size`. |
| `StilSlider<TValue>` | a number | `Min` (0), `Max` (100), `Step` (1), `Format`, `ShowValue`, `ShowMinMax`. A native range input. |
| `StilRangeSlider<TValue>` | `@bind-Start`, `@bind-End` | `Label`, `Hint`, `Min`, `Max`, `Step`, `Format`, `ShowMinMax`, `StartLabel`/`EndLabel`. |
| `StilRating<TValue>` | `int` or `int?` | `Max` (5), `ReadOnly`, `Icon`, `Size`, `OptionFormat`. A radio group; ReadOnly is one image. |
| `StilColorPicker` | `string` (`#rrggbb`) | `Swatches` (Stil's palette by default), `ShowContrast`, `Size`. |
| `StilMaskedInput` | `string` | `Mask` (`0` digit, `A` letter, `*` any), `Placeholder`, `Size`. `StilMaskedInput.IsComplete(mask, text)` for validation. |
| `StilPinInput` | `string` | `Length` (6), `Numeric`, `Secret`, `OnComplete`. One input drawn as cells (`autocomplete="one-time-code"`). |
| `StilPopover` | | `Text` or `Toggle`, `Title`, `ChildContent`, `Top`, `Manual`, `Variant`, `Appearance`, `Size`, `IconOnly`, `AriaLabel`, `OnToggle`. The Popover API, so it works in static SSR. |
| `StilWizard` / `StilWizardStep` | `@bind-Step` | `OnFinish`, `AllowStepClick`, `BackText`/`NextText`/`FinishText`; a step has `Title`, `Description`, `HeadingLevel` and `Validate`. Inside an `EditForm`, Next validates only the current step's fields. |
| `StilStepper` | | `Steps` (`StilStepItem`: Title, Description, State), `Current`, `OnStepClick`, `Label`. |

### Layout and media

```razor
<StilSplitter @bind-Size="listWidth" Min="20" Max="80">
    <First><MessageList /></First>
    <Second>
        @if (loading) { <StilSkeleton Shape="StilSkeletonShape.Text" Lines="4" Label="" /> } else { <Message /> }
    </Second>
</StilSplitter>
<StilCarousel Items="photos" Label="Store photos" Autoplay="TimeSpan.FromSeconds(6)">
    <ItemTemplate Context="photo"><img src="@photo.Url" alt="@photo.Alt" /></ItemTemplate>
</StilCarousel>
<StilImageCropper @ref="cropper" Source="@photoUrl" Round OutputWidth="256" />
<StilCameraCapture OnCapture="image => photoUrl = image.DataUrl" />
<StilPdfViewer Source="files/invoice.pdf" Title="Invoice 1042" FileName="invoice-1042.pdf" />
```

| Component | Parameters |
| --- | --- |
| `StilSkeleton` | `Shape` (Rect, Text, Circle), `Lines`, `Width`, `Height`, `Size`, `Label` (announces loading; `""` for the localized text). The data grid shows skeleton lines while its first rows load. |
| `StilSplitter` | `First`, `Second`, `Vertical`, `@bind-Size` (percent of the first pane), `Min`, `Max`, `Label`. Resized by stil.js with the pointer and keyboard, also in static SSR. |
| `StilCarousel<TItem>` | `Items`, `ItemTemplate`, `Label`, `@bind-Index`, `Autoplay` (with a pause button, off under reduced motion), `ShowDots`, `Height`, texts. |
| `StilImageCropper` | `Source`, `AspectRatio`, `Round`, `AllowRotate`, `OutputWidth`, `OutputType`, `Quality`, `Label`. `CropAsync()` returns `StilImageData` (`DataUrl`, `Bytes`, `ContentType`, `FileName(...)`, `OpenReadStream()`). |
| `StilCameraCapture` | `OnCapture` (`StilImageData`), `BackCamera`, `MaxFileSize`, `TakeText`. Falls back to choosing a photo without a camera. |
| `StilPdfViewer` | `Source`, `Title`, `FileName`, `Height`, `ShowPrint`, `ChildContent` (your own renderer). The browser's viewer with download, open and print; a message where the browser cannot show PDFs. |

The cropper and the camera need an interactive render mode.

### KPI cards

```razor
<StilStatCard Label="Revenue" Value="48,200 €" Trend="+12.4%" TrendDirection="StilTrendDirection.Up"
              Description="vs last month" Icon="@StilIcons.Success" />
```

`StilStatCard`: `Label`, `Value`, `Icon`, `Trend`, `TrendDirection` (Neutral,
Up, Down), `TrendVariant` (default Success for Up, Danger for Down), `TrendLabel`
(what screen readers hear, by default for example "up 12.4%"), `Description`,
`Href` (the whole card is a link), `ChildContent`.

### Calendar and planner

```razor
<StilWeekPlanner TItem="Meeting" Items="meetings" Start="m => m.Start" End="m => m.End" Title="m => m.Title"
                 Variant="m => m.Calendar.Variant" Details="m => m.Room" View="StilWeekView.WorkWeek"
                 Editable OnItemChange="Save" OnSlotSelect="Create" OnItemClick="Edit" />

<StilCalendar TItem="Meeting" Items="meetings" Start="m => m.Start" End="m => m.End" Title="m => m.Title"
              @bind-SelectedDate="day" OnDateOpen="OpenWeek" />

<StilAgenda TItem="Meeting" Items="meetings" Start="m => m.Start" End="m => m.End" Title="m => m.Title" Days="3" />
```

The three views share one data model: any item type with `Start`, `End` and
`Title` selectors, plus `Variant` or `Color`, `Details` and `Draft`. From the
items they work out how booked each working day is (a tint in the month, a bar
in the week), the free blocks in it and the layout of overlapping events. The
working day is 08:00–17:00, Monday to Friday, and names, times, week numbers and
the first day of the week follow the culture.

| Component | Parameters |
| --- | --- |
| Shared | `Items`, `Start`, `End`, `Title`, `Variant`, `Color`, `Details`, `Draft`, `OnItemClick` (events are buttons only when it is set), `Now`, `WorkdayStart`, `WorkdayEnd`, `WorkingDays`, `ShowLoad`, `ShowFreeTime`, `MinFreeMinutes`, `Culture`, `FirstDayOfWeek`, `HeadingLevel`, and the texts |
| `StilCalendar<TItem>` | `@bind-Month`, `@bind-SelectedDate`, `OnDateOpen` (the selected day clicked again, or "+N more"), `Mini`, `ShowWeekNumbers`, `MaxEventsPerDay`, `RangeStart`/`RangeEnd`, `ShowNavigation`, `HrefTemplate`. A WAI-ARIA grid; stil.js moves between the days and past the edges of the month. |
| `StilWeekPlanner<TItem>` | `@bind-Date`, `View` (`Week`, `WorkWeek`, `Day`), `FirstHour`/`LastHour`, `Editable`, `OnItemChange` (moved or resized), `OnSlotSelect` (a free block clicked or a range dragged), `NewEventMinutes`, `AgendaWhenNarrow` (an agenda below 36rem, the default), `ShowNavigation`, `HrefTemplate` |
| `StilAgenda<TItem>` | `Date` (the first day, today by default), `Days`, `ShowEmptyDays`, `EmptyText` |

Dragging never changes your data by itself: save the new times in
`OnItemChange` or create the event in `OnSlotSelect`, and the planner redraws.
Recurring events and time zones are up to the app.

### Navigation

```razor
<StilNavbar Sticky CollapseBelow="StilBreakpoint.Medium" BrandText="Nordvik">
    <ChildContent>
        <StilNav Label="Main">
            <StilNavLink Href="" Match="NavLinkMatch.All" Icon="@StilIcons.Home">Dashboard</StilNavLink>
            <StilNavLink Href="orders" Badge="12">Orders</StilNavLink>
        </StilNav>
    </ChildContent>
    <End><StilButton Size="StilSize.Small">Sign in</StilButton></End>
</StilNavbar>

<StilBreadcrumbs Items="@([new("Home", "/"), new("Orders", "orders"), new("#1043")])" />
<StilPagination @bind-Page="page" TotalItems="480" PageSize="20" />

<StilNav Vertical Label="Sections">
    <StilNavGroup Title="Sales" Icon="@StilIcons.Folder">
        <StilNavLink Href="orders">Orders</StilNavLink>
        <StilNavLink Href="invoices">Invoices</StilNavLink>
    </StilNavGroup>
</StilNav>

<StilTreeView TItem="Folder" Items="roots" ChildrenSelector="f => f.Children" ItemText="f => f.Name"
              @bind-SelectedItem="folder" Label="Folders" />
```

| Component | Parameters |
| --- | --- |
| `StilNavbar` | `Brand` or `BrandText`/`BrandHref`, `ChildContent`, `End`, `CollapseBelow` (a menu button below that breakpoint; it closes after navigating), `Sticky`, `Variant`, `ToggleLabel` |
| `StilNav` | `Items` or `StilNavLink` children, `Vertical`, `Pills`, `Label` |
| `StilNavLink` | `Href`, `Match`, `Icon`, `Badge`/`BadgeVariant`. Wraps Blazor's `NavLink`, so `aria-current` follows the route. |
| `StilBreadcrumbs` | `Items` (`StilLinkItem`: Text, Href) or `StilBreadcrumbItem` children (`Text`, `Href`, `Current`), `Label`. The last item is the current page. |
| `StilLanguageSelector` | `Cultures` (for example `["en", "sv", "fi"]`), `Current`, `IconOnly`, `Label`, `Variant`, `Appearance`, `Size`, `AlignEnd`, `DropUp`. A language menu; see [Localization](#localization). |
| `StilPagination` | `@bind-Page` (1-based), `PageCount` or `TotalItems` + `PageSize`, `MaxVisiblePages` (default 7, with ellipses), `ShowFirstLast`, `HrefTemplate` (links, for example `"orders?page={0}"`), `Size`, `Label`, `PreviousText`/`NextText`/`FirstText`/`LastText` |
| `StilNavGroup` | `Title`, `Icon`, `@bind-Expanded`, `StilNavLink` children. A collapsible group in a vertical `StilNav`; it opens by itself when one of its links is the current page. |
| `StilTreeGrid<TItem>` | The data grid's columns and parameters (no grouping or `ItemsProvider`) plus `ChildrenSelector`, `LoadChildren` (on demand, with a spinner), `HasChildren`, `ExpandedLevels`, `ExpandAsync()`, `CollapseAsync()`, `ExpandAllAsync()`, `CollapseAllAsync()`. Rows with children show the totals of the rows below them. The WAI-ARIA treegrid pattern. |
| `StilTreeView<TItem>` | `Items`, `ChildrenSelector`, `ItemText` or `ItemTemplate`, `ItemIcon`, `@bind-SelectedItem`, `@bind-ExpandedItems`, `ExpandAll`, `ItemKey`, `Label`. The WAI-ARIA tree pattern: arrow keys, Home and End move, Right and Left expand and collapse. |
| `StilDrawer` | Like `StilDialog` (`@bind-Open`, `Title`, `Footer`, …) plus `Position` (Start, End) and `Width`. A side panel for navigation, filters or details. |

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
| `StilTabs` | `@bind-ActiveIndex`, or `@bind-ActiveId` with `Id` on each tab; `Appearance` (Underline, Pills); `KeepAlive` (keep inactive panels rendered); `Label`. Arrow keys, Home and End move between tabs; disabled tabs are skipped. |
| `StilTab` | `Title`, `Id`, `Disabled`. Only the selected tab's content is rendered, unless `KeepAlive` is set. For navigation between pages, use `StilNav` instead of tabs. |
| `StilDropdown` | `Text` or `Toggle`, `AriaLabel`, `Variant`, `Appearance`, `Size`, `IconOnly`, `AlignEnd`, `DropUp`, `Disabled`, `@bind-Open`, `Items` (`StilMenuItem.Action`, `.Link`, `.Header`, `.Divider`). The arrow keys move between items. |
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
| `StilQrCode` | `Value`, `ErrorCorrection` (Low, Medium, Quartile, High), `Size` (default 10rem), `Label`. An SVG drawn in C#, also in static SSR. |
| `StilBarcode` | `Value`, `Format` (Code128, Ean13, Ean8), `ShowText`, `Width` (default 16rem), `Label`. `StilCodes` has the encoders for PDFs and label printers. |
| `StilScanner` | `OnScan` (`StilScanResult`: `Value`, `Format`, `Source`), `Formats`, `Sound`, `Cooldown`, `FrontCamera`, `ManualEntry`. Reads codes with the camera through the browser's `BarcodeDetector`, or a decoder the app registers with stil.js's `registerScanDecoder`; switches cameras and has a flashlight button. |
| `StilScanInput` | A text field for handheld USB and Bluetooth scanners: `OnScan` on Enter, with `Source` Scanner or Keyboard (`MaxKeyInterval`, 50 ms), `ClearOnScan`, `AutoFocus`, and the usual field parameters. Enter never submits the form. |
| `StilSignaturePad` | `@bind-Value` (`StilSignature`: `Strokes`, `TypedName`, `Width`, `Height`, `ToSvg()`; null when empty, so `[Required]` works), `Placeholder`, `NamePlaceholder`, `ToPngAsync()`. Drawn with pointer events and pressure, with undo and clear; typing a name is the keyboard alternative. Not a qualified electronic signature. |
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

## Stil CSS and the matching components

| Stil CSS | Blazor component |
| --- | --- |
| `stil-container`, `stil-grid`/`stil-col-*`, `stil-stack`, `stil-cluster`, `stil-with-sidebar` | `StilContainer`, `StilGrid`/`StilCol`, `StilStack`, `StilCluster`, `StilSidebarLayout` |
| `stil-btn`, `stil-btn-group`, `stil-close` | `StilButton`, `StilButtonGroup`, `StilCloseButton` |
| `stil-icon` | `StilIcon` |
| `stil-form`, `stil-field`, `stil-input`, `stil-select`, `stil-check`, `stil-switch`, `stil-input-group` | An `EditForm` with `StilTextBox`, `StilTextArea`, `StilNumeric`, `StilDatePicker`, `StilSelect`, `StilCheckbox`, `StilSwitch`, `StilRadioGroup`, `StilField`, and `Prepend`/`Append` |
| `stil-card` | `StilCard` |
| `stil-alert` | `StilAlert` |
| `stil-badge`, `stil-avatar` | `StilBadge`, `StilAvatar` |
| `stil-list`, `stil-list-item` | `StilList`, `StilListItem` |
| `stil-table` | `StilTable` with `StilColumn` |
| `stil-data-grid` | `StilDataGrid` with `StilDataGridColumn` and `StilDataGridTemplateColumn`; with `stil-tree-grid`, `StilTreeGrid` |
| `stil-combobox`, `stil-multi-select`, `stil-chip` | `StilAutocomplete`, `StilMultiSelect` |
| `stil-date-range` | `StilDateRangePicker` |
| `stil-slider`, `stil-range` | `StilSlider`, `StilRangeSlider` |
| `stil-rating`, `stil-color`, `stil-pin`, `stil-datetime` | `StilRating`, `StilColorPicker`, `StilPinInput`, `StilDateTimePicker` |
| `stil-popover` | `StilPopover` |
| `stil-stepper`, `stil-step`, `stil-wizard` | `StilStepper`, `StilWizard` |
| `stil-skeleton`, `stil-splitter`, `stil-carousel` | `StilSkeleton`, `StilSplitter`, `StilCarousel` |
| `stil-cropper`, `stil-camera`, `stil-pdf` | `StilImageCropper`, `StilCameraCapture`, `StilPdfViewer` |
| `stil-dropzone`, `stil-file-list` | `StilFileUpload` |
| `stil-stat` | `StilStatCard` |
| `stil-tree` | `StilTreeView` |
| `stil-event`, `stil-calendar`, `stil-week`, `stil-agenda` | `StilCalendar`, `StilWeekPlanner`, `StilAgenda` |
| `stil-accordion` | `StilAccordion` with `StilAccordionItem` |
| `stil-nav`, `stil-navbar`, `stil-nav-group` | `StilNav`/`StilNavLink`, `StilNavbar`, `StilNavGroup` |
| `stil-breadcrumb`, `stil-pagination` | `StilBreadcrumbs`, `StilPagination` |
| `stil-tabs` | `StilTabs` with `StilTab` |
| `stil-dropdown` | `StilDropdown` with `StilDropdownItem`, `StilDropdownHeader` and `StilDropdownDivider`, or `Items` |
| `data-stil-tooltip` | `StilTooltip` |
| `stil-dialog`, `stil-drawer` | `StilDialog` and `IStilDialogService`, `StilDrawer` |
| `stil-toast` | `IStilToastService` with `StilToastHost` |
| `stil-progress`, `stil-spinner` | `StilProgress`, `StilSpinner` |
| `data-stil-theme`, `--stil-*` tokens | `StilThemeProvider`, `StilTheme`, `StilHead` |

Utility classes (`stil-mt-4`, `stil-d-flex`, …) have no components. Use them
as `class` on any component.

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

### Fonts

`options.Font` gives the whole app a web font. `<StilHead />` links its
stylesheet and sets `--stil-font-family` (`using Stil.Blazor;`):

```csharp
builder.Services.AddStil(options => options.Font = StilFonts.BrCobane);
// Any font from cdnfonts.com by its name, or any Google font:
// options.Font = StilFont.CdnFonts("Gilroy");
// options.Font = StilFont.Google("Open Sans");
```

`StilFonts` has BR Shape (the default), Lexend, BR Cobane, BR Segma, BR Sonoma, Averral,
Sukplena, TASA Explorer, Halcom, Utara and Geologica, and the display fonts
BLOWREPH, Taster and WARM STORIES, all from
cdnfonts.com. Check a font's license there before you use it in a product;
some are free for personal use only. For a font you host yourself, use
`new StilFont("My Font", "fonts/my-font.css")`.

You can also override tokens in CSS. Stil's rules are in cascade layers, so
your CSS always wins. See the
[theming guide and full token list](https://github.com/dradwul/Stil/tree/main/src/Stil.Css#theming).

## Localization

Every text a component writes by itself ("Next page" for screen readers, "No
items", chart summaries) is built in for English, Swedish, Norwegian
(Bokmål), Danish and Finnish, and follows `CultureInfo.CurrentUICulture`. An
app that sets its culture gets translated components with no Stil setting;
`sv-FI` gets Swedish, and other languages get English. A text parameter on a
component (`NextText`, `EmptyText`, …) always wins.

```csharp
// Reword a text everywhere, or add a language (the keys are on the docs site).
builder.Services.AddStil(options => options.Texts
    .Add("sv", "Dialog.Confirm", "Ja")
    .Add("de", "Common.Close", "Schließen"));

// The app's own texts from Locales/sv.json, Locales/Pages/Home.sv.json, …
builder.Services.AddStilLocalization();

// Server and static SSR: the language the selector stores in the culture cookie.
app.UseRequestLocalization(o => o.SetDefaultCulture("en")
    .AddSupportedCultures(cultures).AddSupportedUICultures(cultures));
```

```razor
@inject IStringLocalizer<Home> L

<h1>@L["Orders.Title"]</h1>
<StilLanguageSelector Cultures="@(["en", "sv", "fi"])" AlignEnd />
```

- `AddStilLocalization()` reads JSON files through the standard
  `IStringLocalizer<T>`. The package embeds `Locales/**/*.json` in the app, so
  the same files work in Server, static SSR and WebAssembly. A type's own file
  wins over the shared one; lookups fall back from `sv-FI` to `sv` to the
  default language (`en`), and a missing key shows the key. Keys under
  `"Stil"` override Stil's own texts.
- `StilLanguageSelector` lists languages by their own names and stores the
  choice in the ASP.NET Core culture cookie and in local storage (through
  stil.js), then reloads. In WebAssembly, call
  `await StilCulture.ApplyStoredAsync(host.Services)` before `RunAsync()` and
  set `BlazorWebAssemblyLoadAllGlobalizationData`.
- Right to left: put `dir="@StilCulture.Direction()"` on `<html>`. The CSS
  mirrors, arrow and chevron icons flip, and tabs and tree views swap the
  left and right arrow keys in right-to-left cultures.
- Register your own `IStilLocalizer` to read Stil's texts from elsewhere, for
  example resx files; return `null` for the texts it does not have.

## Development

The component source is in this folder. The tests are in
`tests/Stil.Blazor.Tests`, and a runnable demo, with a component gallery and
a CRUD page, is in `samples/Stil.Demo`. The stylesheet and `stil.js` are
served from `src/Stil.Css/dist/`, which is built from `src/Stil.Css/src/`.
Naming rules are in
[CONTRIBUTING.md](https://github.com/dradwul/Stil/blob/main/CONTRIBUTING.md).
