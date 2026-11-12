# Stil.Charts

Themeable SVG charts for [Stil for Blazor](https://github.com/dradwul/Stil/tree/main/src/Stil.Blazor):
line, area, bar (grouped, stacked, vertical or horizontal, with a line over
the bars), donut, scatter, bubble and sparkline charts, heat maps and gauges.

- The charts render plain SVG on the server, so they also work in static
  server rendering. There is no JavaScript and no canvas.
- Colors come from `--stil-chart-*` tokens, so the charts follow your theme
  and dark mode like every other Stil component.
- Every chart has an accessible name, a generated description of the data and
  a data table, so screen reader users get the same information.

> **Status:** stable (`1.0`). Requires .NET 10 and the `Stil` package. The
> API changes only in a major version ([Semantic Versioning](https://semver.org/)).

## Get started

1. Install the package next to `Stil`, and add the namespace to
   `_Imports.razor`:

   ```shell
   dotnet add package Stil.Charts
   ```

   ```razor
   @using Stil.Charts
   ```

2. Link the chart stylesheet after `<StilHead />` in `Components/App.razor`:

   ```razor
   <StilHead />
   <link rel="stylesheet" href="@Assets["_content/Stil.Charts/stil-charts.css"]" />
   ```

3. Draw a chart. `Items` holds one item per category, `Category` names it, and
   each `StilChartSeries` reads one value from every item:

   ```razor
   <StilLineChart Items="sales" Category="s => s.Month" Title="Revenue" Format="C0">
       <StilChartSeries Name="2025" Value="s => s.LastYear" />
       <StilChartSeries Name="2026" Value="s => s.ThisYear" />
   </StilLineChart>
   ```

   A value can be any number type, and nullable. A `null` value leaves a gap
   in a line and no bar.

## Charts

| Component | Use it for |
| --- | --- |
| `StilLineChart` | Changes over time. `Area` fills under the lines; `ShowMarkers` marks every point; `BeginAtZero="false"` zooms in on small changes. |
| `StilBarChart` | Comparing categories. `Orientation="Horizontal"` suits long names; `Stacked` adds the series up; `ShowValues` writes the values at the bar ends. A series with `Line="true"` is drawn as a line over the bars. |
| `StilDonutChart` | The parts of a whole (the first series). The total is in the middle; the legend lists each part's value and share. Parts beyond `MaxSlices` are combined into "Other". |
| `StilScatterChart` | Whether two values go together: one point per item by `X` and `Y`. `Group` colors the points; `Size` makes it a bubble chart, the area following a third value. |
| `StilHeatMap` | A value across two categories, such as orders by weekday and hour. `Row` and `Column` place each item; a cell sums its `Value`s, or counts the items. `Rows` and `Columns` set the order. |
| `StilGauge` | One value against a scale and a target, as a half circle or a bar (`Shape`). `Ranges` tint stretches of the scale, and the value takes the color of its range. |
| `StilSparkline` | A small line without axes, for example in a `StilStatCard`. |

Shared parameters: `Title` (also the accessible name), `Description`,
`Height`, `Format` (a .NET format string such as `N0`, `C0` or `0.0 'kg'`,
in the current culture), `CategoryFormat`, `ShowLegend`, `DataTable`,
`Summary` and the texts (`ShowDataText`, `EmptyText`, `CategoryText`,
`MissingText`) for other languages. Line and bar charts also have
`CategoryAxisTitle`, `ValueAxisTitle`, `Min`, `Max`, `TickFormat` and
`ShowGridlines`, and a second value axis at the right for series with
`SecondaryAxis="true"` (`SecondaryAxisTitle`, `SecondaryMin`, `SecondaryMax`,
`SecondaryTickFormat`). The scatter chart has its own axis parameters (`XAxisTitle`,
`YAxisTitle`, `XMin`, `XMax`, `YMin`, `YMax`, `XFormat`).

## Accessibility

- The drawing is an `<svg role="img">` named by the visible `Title` (or the
  chart type when there is no title) and described by a generated summary,
  for example "Line chart of Revenue, 12 categories, from Jan to Dec. Lowest
  …, highest …". Set `Summary` to say what the chart shows in your own words.
- `DataTable` offers the numbers as a real table: a "Show data" toggle under
  the chart (the default), a table only screen readers find (`Hidden`), or
  none (`None`, only when the same numbers are elsewhere on the page).
- Series are told apart by the legend, direct labels at the line ends and the
  table, not by color alone. Each mark has a native tooltip with its values.
- In Windows high-contrast mode the series keep their colors and the text and
  axes use system colors.
- A `StilGauge` is a `role="meter"`: screen readers hear the value, the name
  of its range, the scale and the target.
- A `StilSparkline` without a `Label` is decorative and hidden from screen
  readers. With a `Label`, it reads the first, last, lowest and highest value.

## Theming

The chart tokens are defined in `stil-charts.css`. Override them like any
`--stil-*` token, on `:root` or on any wrapper:

| Token | Default |
| --- | --- |
| `--stil-chart-1` … `--stil-chart-6` | A categorical palette with separate light and dark steps. |
| `--stil-chart-other` | Series 7 and up, and the donut's "Other" slice. Default: `--stil-color-border-strong`. |
| `--stil-chart-surface` | The gap between touching marks and the ring around markers. Set it to the page background when a chart is not on a card. |
| `--stil-chart-grid` | Gridlines. |
| `--stil-chart-axis` | The zero line and the hover crosshair. |
| `--stil-chart-area-opacity` | The wash under area lines. Default: `0.1`. |
| `--stil-chart-sparkline` | A sparkline's line. Default: the muted text color. |

A series can also have its own color: `Color="var(--stil-color-danger)"`.

## Render modes

Charts render completely in static server rendering. Hover effects and
tooltips are CSS and native SVG titles, and the data table toggle is a
`<details>` element, so none of them need interactivity. Charts redraw when
their items or series change in an interactive render mode.
