# Changelog

All notable changes to the Stil packages are listed here: `stil-css` (npm),
`Stil`, `Stil.Charts` and `Stil.Templates` (NuGet). They share one version
number.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and versions follow [Semantic Versioning](https://semver.org/). Before `1.0`,
a minor version can contain breaking changes; they are marked **Breaking**.

## [Unreleased]

### Changed

- **Stil CSS and Stil for Blazor**: open dropdown menus and the lists of
  `StilAutocomplete`, `StilMultiSelect` and `StilTimePicker` sit in the
  top layer (stil.js), placed under their toggle or field and following it on
  scroll. A parent with `overflow`, such as a scrolling card, a table or a
  dialog, no longer cuts them off. Without stil.js they open in place, as
  before.

- **Stil CSS**: the default font is BR Shape (`--stil-font-family`), used
  when the page loads it from `https://fonts.cdnfonts.com/css/br-shape`;
  otherwise `sans-serif`. Lexend is still in `StilFonts`.
- **Stil CSS**: dark text on colored backgrounds (buttons, badges, alerts and
  other variants) is a soft gray, `#3d3d3d`, instead of black. On colors
  where that gray would fall below 4.5:1 it darkens just enough, so the text
  still meets WCAG AA.
- **Stil CSS**: a progress bar's value (`stil-progress-label` inside the bar)
  is centered on the whole track, and its text changes color where the bar
  ends, so it reads on both parts. The bar takes its width from
  `--stil-progress-value`; `style="width: …"` still works.
- **Stil for Blazor**: `StilProgress` with `ShowValue` uses that label, also
  at the medium size.

### Added

- **Stil for Blazor**: `StilQrCode` and `StilBarcode` (Code 128, EAN-13,
  EAN-8) draw codes as SVG in C#, with no dependency, so they render in
  static SSR and print sharply. `StilCodes` has the encoders (`QrSvg`,
  `QrModules`, `BarcodeSvg`, `BarcodeModules`, `EanCheckDigit`) for PDFs and
  label printers.
- **Stil for Blazor**: `StilCommandPalette`: Ctrl+K (Cmd+K) opens a search
  over pages and actions (`StilCommand`) in a modal dialog, with groups,
  keywords, shortcuts, the combobox keys and the recent choices first.
- **Stil CSS**: `stil-command-palette`, and in stil.js `bindCommandKey`,
  `readStorage` and `writeStorage`.
- **Demo**: the navbar has the command palette.
- **Stil for Blazor**: `StilKanban<TItem>`, a board of columns whose cards
  move by dragging (also by touch) and with the keyboard (Space picks a card
  up, the arrow keys move it, Space drops it, Escape puts it back), announced
  to screen readers. Columns have WIP limits (`Limit`) and collapse.
  `OnCardMove` gets a `StilKanbanMove` whose `ApplyTo` updates your list.
- **Stil CSS**: `stil-kanban`, and in stil.js the `stilcardmove` event,
  arrow-key focus between cards and `focusCard`.
- **Demo**: a task board (`/tasks`).
- **Demo**: a check-in desk (`/check-in`) that scans a visitor's code, takes
  a signature, writes the visit to the log and chats with the host, live
  between two windows.
- **Stil for Blazor**: `StilChat<TItem>` and `StilComments<TItem>`, a
  conversation or a comment thread on the activity log's selectors: own
  messages on the other side, runs, day separators, a composer (Enter or
  Ctrl+Enter sends), attachments, "is typing", and polite announcements of
  new messages. Stil renders and raises `OnSend`; the transport is the app's.
- **Stil CSS**: `stil-chat` and `stil-comments`, and `data-stil-send` on a
  textarea in stil.js.
- **Stil for Blazor**: `StilActivityLog<TItem>`, a log of what happens from
  your items and selectors (`Time`, `Title`, `Actor`, `Type`, `Variant`,
  `Icon`, `Details`): newest first, grouped by day, with localized relative
  times that keep up to date, search, filters by type and person, and
  virtualization. New entries are highlighted and announced politely.
  `StilTimeline<TItem>` is the compact form, for an order's history.
- **Stil CSS**: `stil-log` and `stil-timeline`, with the marker variants
  `stil-log-marker-{variant}`.
- **Stil for Blazor**: `StilSignaturePad` takes a signature drawn with a
  finger, pen or mouse (pressure, undo, clear) or a typed name, as a
  `StilSignature` that is null until signed, so `[Required]` works. It saves
  as SVG (`ToSvg`, in C#) and PNG (`ToPngAsync`).
- **Stil CSS**: `stil-signature` (pad, canvas, line, placeholder and typed
  name), and in stil.js `initSignaturePad`, `drawSignature` and
  `signaturePng`.
- **Stil for Blazor**: `StilScanner` reads barcodes and QR codes with the
  camera (the browser's `BarcodeDetector`, or a decoder the app registers
  with stil.js's `registerScanDecoder`), with camera switching, a flashlight
  button, a scan sound and a field for typed codes. `StilScanInput` takes
  codes from handheld USB and Bluetooth scanners and tells them from typing.
  Both raise `OnScan` with a `StilScanResult`.
- **Stil CSS**: `stil-scanner` (view, preview, aiming frame and status), and
  in stil.js `startScanner`, `stopScanner`, `setTorch`, `hasTorch`,
  `registerScanDecoder` and `initScanInput`.
- **Stil CSS**: `stil-qr-code`, `stil-barcode` and `stil-code-invalid`, with
  the tokens `--stil-barcode-color` and `--stil-barcode-bg`.

- **Stil for Blazor**: `options.Font` in `AddStil()` gives the whole app a web
  font; `<StilHead />` links its stylesheet and sets `--stil-font-family`.
  `StilFonts` has ready-made fonts from cdnfonts.com (Lexend, BR Cobane,
  BR Segma, BR Sonoma, BR Shape, Averral, Sukplena, TASA Explorer, Halcom,
  Utara, Geologica, BLOWREPH, Taster and WARM STORIES); `StilFont.CdnFonts("…")` takes any other font there and
  `StilFont.Google("…")` any Google font.
- **Stil.Templates**: `--Font` picks one of those fonts (`br-shape` by default,
  or `system` for none) and `--GoogleFont` any Google font. New apps now load
  BR Shape.
- **Stil CSS**: `samples/Stil.Html/fonts.html` compares the fonts.
- **Docs**: the theme builder previews the cdnfonts.com fonts and writes
  `options.Font` in its C#.

## [0.1.0-preview.4] - 2026-11-03

The first release on nuget.org since `0.1.0-preview.2`; it also contains
the changes listed under `0.1.0-preview.3`.

### Added

- **Stil for Blazor**: totals and export in `StilDataGrid`. `Aggregate` on a
  column (sum, average, count, min, max) or `AggregateValue` adds a totals
  row over every filtered row; `ShowExport` and `ExportCsvAsync()` give the
  filtered and sorted rows of every page as CSV that Excel opens as it is.
  With an `ItemsProvider`, `StilDataGridRequest.ComputeAggregates` computes
  the totals in the database and the export pages through the provider.
- **Stil for Blazor**: column layout in `StilDataGrid`: `ResizableColumns`,
  `ReorderableColumns` (dragging headers), `ShowColumnChooser`, frozen start
  and end columns (`Frozen`), and `@bind-Layout` (`StilDataGridLayout`) to
  save and restore the order, widths, hidden columns and sort.
- **Stil for Blazor**: grouping in `StilDataGrid`: `@bind-GroupBy` (one or
  more columns) and `ShowGroupChooser`, with group headers that show the
  value, the row count and the column totals, and collapse and expand with
  the mouse and keyboard (all at once from the toolbar). Paging counts rows
  and virtualization shows the headers in place. With an `ItemsProvider`,
  `StilDataGridRequest.GroupBy` and `ComputeGroups` give the groups.
- **Stil for Blazor**: editing in place in `StilDataGrid`: `EditMode` Row
  (with `StilDataGridEditColumn`, a double click, Enter or F2) or Cell, Stil
  fields for text, number, date, enum and bool columns or an `EditTemplate`,
  validation with data annotations next to the fields, `OnRowSave` (return
  false to keep editing), `OnRowCancel` and `AddRowAsync()` for a new row.
- **Stil for Blazor**: `StilTreeGrid<TItem>`, a data grid for items with
  children (`ChildrenSelector`, or `LoadChildren` on demand with a spinner).
  Columns, sorting (per parent), filters (matches keep the rows above and
  below them), editing and export work as in `StilDataGrid`; rows with
  children show the totals of the rows below them. The WAI-ARIA treegrid
  pattern with the arrow keys, Home and End.
- **Stil CSS**: the `stil-data-grid-totals` row, column resize handles, drag
  markers, frozen columns, group header rows (`stil-data-grid-group`) and rows being edited (`stil-data-grid-editing`), tree grids (`stil-tree-grid`), a `pencil` icon, and `stil.js`'s `downloadFile`, column resizing
  and moving (`stilcolumnresize`, `stilcolumnmove`), `freezeColumns`, `focusEditor` and `focusRow`.
- **Stil CSS**: every GitHub release has `stil.css`, `stil.min.css` and
  `stil.js` attached.

## [0.1.0-preview.3] - 2026-11-03

Tagged but not published: the release failed. These changes ship in
`0.1.0-preview.4`.

### Added

- **Stil CSS**: skeleton, splitter, carousel, image cropper, camera and PDF
  frame styles (`stil-skeleton`, `stil-splitter`, `stil-carousel`,
  `stil-cropper`, `stil-camera`, `stil-pdf`), and the `camera` and `download`
  icons. `stil.js` resizes splitters (`stilsplit`), runs carousels
  (`stilslide`, autoplay) and exports functions for cropping, the camera and
  printing.
- **Stil for Blazor**: `StilSkeleton` (also as the data grid's loading
  state), `StilSplitter`, `StilCarousel`, `StilImageCropper` and
  `StilCameraCapture` (both give a `StilImageData`), and `StilPdfViewer`.

## [0.1.0-preview.2] - 2026-11-03

### Fixed

- **Stil CSS**: `body.stil-scope` has no margin, so an app with a dark theme
  no longer shows a light edge around the page (the browser's default 8px
  body margin). This also fixes apps made with `dotnet new stil-app`, the
  demo and the docs site.
- **Stil CSS**: in a mini month (`stil-calendar-mini`), days of the other
  months are no longer tinted by their load, so their muted date keeps its
  contrast in dark mode.

## [0.1.0-preview.1] - 2026-10-30

### Added

- **Stil CSS** (`stil-css`):
  - Tokens (`--stil-*`) for colors, typography, spacing, shape, depth and
    motion, with light, dark and auto modes through `data-stil-theme`.
    The default font is Lexend, used when the page loads it.
  - Derived colors: text on colored backgrounds, links and focus rings stay
    WCAG AA readable for any brand color. CI runs 163 contrast checks.
  - Cascade layers (`stil.tokens`, `stil.reset`, `stil.base`,
    `stil.components`, `stil.utilities`) and a reset and base styles scoped to
    `.stil-scope`.
  - Layout: container, 12-column grid, stack, cluster and sidebar layout.
  - Components: buttons, forms and input groups, cards, alerts, badges,
    avatars, lists, tables, data grid, combobox and multi-select, chips, date
    range, file drop zone, stat cards, tree, accordion, nav and nav groups,
    navbar, breadcrumbs, pagination, tabs, dropdowns, dialogs, drawers,
    tooltips, progress, spinners and toasts.
  - Popover, slider and range, rating, color picker, one-time code, stepper
    and wizard styles (`stil-popover`, `stil-slider`, `stil-range`,
    `stil-rating`, `stil-color`, `stil-pin`, `stil-stepper`, `stil-wizard`).
  - Calendar components: events with variants or an own color, a month grid
    tinted by how busy each day is, a week time grid with free blocks and a now
    line, and an agenda. `stil.js` adds keyboard navigation in the month grid
    and dragging events in a week grid (`data-stil-editable`).
  - About 900 utility classes and about 30 built-in icons (`stil-icon-*`).
  - Right-to-left support: logical properties throughout, mirrored switches,
    drawers and tree chevrons, and arrow and chevron icons that flip under
    `dir="rtl"` (`stil-icon-directional` for icons of your own).
  - `stil.js`, an optional 20 kB module for dropdowns, dismissing, dialogs,
    toggles, calendar dragging, popover placement, input masks, language
    links (`data-stil-culture`) and keyboard handling.
- **Stil for Blazor** (`Stil`):
  - About 60 components, with one for every Stil CSS component, including
    `StilDataGrid` (sorting, filtering, paging, selection, virtualization and
    server-side data), `StilAutocomplete`, `StilMultiSelect`,
    `StilDateRangePicker`, `StilFileUpload`, `StilTreeView`, `StilNavGroup`
    and `StilStatCard`.
  - More inputs: `StilTimePicker` and `StilDateTimePicker` (typed times and
    steps in the culture's format), `StilSlider` and `StilRangeSlider`,
    `StilRating`, `StilColorPicker` (with a contrast hint), `StilMaskedInput`,
    `StilPinInput` and `StilPopover` (the Popover API).
  - `StilWizard`, `StilWizardStep` and `StilStepper`: a form in steps where
    Next validates only the current step's fields.
  - `StilCalendar`, `StilWeekPlanner` and `StilAgenda`: a month, a week
    planner and an agenda from one list of items, showing how booked each day
    is and where the free time is. The planner can move, resize and create
    events by dragging, and turns into an agenda on small screens.
  - Form fields with EditForm validation, labels, hints, add-ons and
    `aria-describedby`/`aria-invalid`.
  - `IStilDialogService`, `IStilToastService`, `AddStil()`, `StilTheme`,
    `StilHead` and `StilThemeProvider`.
  - `StilIcon` with built-in icons, and `IStilIconProvider` for other icon
    sets.
  - Localization: every built-in text (screen reader labels, empty states,
    chart summaries) in English, Swedish, Norwegian (Bokmål), Danish and
    Finnish, following the UI culture. Text parameters override one
    component, `AddStil(o => o.Texts.Add(…))` rewords a text everywhere or
    adds a language, and `IStilLocalizer` reads texts from elsewhere.
  - `AddStilLocalization()`: the app's own texts from `Locales/*.json`
    through the standard `IStringLocalizer<T>`, embedded by the package so
    the files work in Server, static SSR and WebAssembly.
  - `StilLanguageSelector` and `StilCulture`: a language menu that stores
    the choice in the ASP.NET Core culture cookie and local storage, and
    helpers for WebAssembly startup and `<html dir>`. Tabs and tree views
    follow the reading direction of right-to-left cultures.
  - XML documentation for IntelliSense, Source Link and symbol packages.
- **Stil.Charts**: SVG line, area, bar (grouped, stacked or horizontal), donut
  and sparkline charts that follow the theme and dark mode and render in static
  SSR. Each chart has a generated summary and a data table for screen readers.
- **Stil.Templates**: `dotnet new stil-app`, with a language selector,
  `Locales/*.json` and a `Culture` option for the default language.
- A documentation site with live examples, API tables and a theme builder.
- A guide for migrating from Bootstrap.

[Unreleased]: https://github.com/dradwul/Stil/compare/v0.1.0-preview.4...HEAD
[0.1.0-preview.4]: https://github.com/dradwul/Stil/compare/v0.1.0-preview.3...v0.1.0-preview.4
[0.1.0-preview.3]: https://github.com/dradwul/Stil/compare/v0.1.0-preview.2...v0.1.0-preview.3
[0.1.0-preview.2]: https://github.com/dradwul/Stil/compare/v0.1.0-preview.1...v0.1.0-preview.2
[0.1.0-preview.1]: https://github.com/dradwul/Stil/releases/tag/v0.1.0-preview.1
