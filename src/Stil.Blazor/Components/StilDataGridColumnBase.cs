using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Stil.Blazor.Components;

/// <summary>Shared parameters of data grid columns. Columns render nothing themselves; the grid does.</summary>
public abstract class StilDataGridColumnBase<TItem> : ComponentBase, IDisposable
{
    [CascadingParameter]
    internal StilDataGrid<TItem>? Grid { get; set; }

    /// <summary>The header text.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Identifies the column in a StilDataGridRequest (SortBy and Filters). The default is the
    /// property name, or the title.
    /// </summary>
    [Parameter]
    public string? Key { get; set; }

    /// <summary>Text alignment of the header and cells. End suits numbers.</summary>
    [Parameter]
    public StilTextAlign Align { get; set; }

    /// <summary>A CSS width for the column, for example "8rem" or "20%".</summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>Extra classes on every cell in the column.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Totals the column in a footer row: Sum, Average, Count, Min or Max.</summary>
    [Parameter]
    public StilAggregate Aggregate { get; set; }

    /// <summary>
    /// A total of your own (for example a weighted average), computed from the filtered rows. It
    /// is used instead of Aggregate when the grid has Items; an ItemsProvider returns it itself.
    /// </summary>
    [Parameter]
    public Func<IEnumerable<TItem>, object?>? AggregateValue { get; set; }

    /// <summary>A format string for the total. The default is the column's Format.</summary>
    [Parameter]
    public string? AggregateFormat { get; set; }

    /// <summary>
    /// The value written in a CSV export. Columns bound to a property export their value without
    /// it; a template column is only exported when it is set.
    /// </summary>
    [Parameter]
    public Func<TItem, object?>? ExportValue { get; set; }

    /// <summary>Includes the column in a CSV export. The default is true.</summary>
    [Parameter]
    public bool Exportable { get; set; } = true;

    /// <summary>Keeps the column at the start or end while the grid scrolls sideways.</summary>
    [Parameter]
    public StilFrozen Frozen { get; set; }

    /// <summary>Lets the user resize the column. The default is the grid's ResizableColumns.</summary>
    [Parameter]
    public bool? Resizable { get; set; }

    /// <summary>Lets the user move the column. The default is the grid's ReorderableColumns.</summary>
    [Parameter]
    public bool? Reorderable { get; set; }

    /// <summary>Lets the user hide the column in the column chooser. The default is true.</summary>
    [Parameter]
    public bool Hideable { get; set; } = true;

    /// <summary>The smallest width when resizing, in pixels. The default is 48.</summary>
    [Parameter]
    public int MinWidth { get; set; } = 48;

    /// <summary>The largest width when resizing, in pixels.</summary>
    [Parameter]
    public int? MaxWidth { get; set; }

    internal string? FrozenValue => Frozen switch
    {
        StilFrozen.Start => "start",
        StilFrozen.End => "end",
        _ => null
    };

    internal bool HasAggregate => Aggregate != StilAggregate.None || AggregateValue is not null;

    /// <summary>The format of the column's values (StilDataGridColumn.Format).</summary>
    internal virtual string? ValueFormat => null;

    internal virtual bool CanExport => Exportable && ExportValue is not null;

    /// <summary>Computes Aggregate on a filtered query; columns without a property have none.</summary>
    internal virtual object? ComputeAggregate(IQueryable<TItem> query) => null;

    internal virtual string ExportText(TItem item) => StilDataGridExport.Text(ExportValue?.Invoke(item), ValueFormat, null);

    internal string FormatTotal(object? value) => value switch
    {
        null => "",
        IFormattable formattable when Aggregate != StilAggregate.Count && (AggregateFormat ?? ValueFormat) is { } format =>
            formattable.ToString(format, System.Globalization.CultureInfo.CurrentCulture),
        IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.CurrentCulture),
        _ => value.ToString() ?? ""
    };

    internal virtual string DisplayTitle => Title ?? "";

    internal virtual string ColumnKey => Key ?? Title ?? "";

    internal virtual bool CanSort => false;

    internal virtual bool CanFilter => false;

    /// <summary>Fixed filter choices (for enums and booleans), or null for a text filter.</summary>
    internal virtual IReadOnlyList<(string Value, string Text)>? FilterOptions => null;

    internal virtual StilSortDirection InitialSortDirection => StilSortDirection.None;

    internal virtual bool CanGroup => false;

    /// <summary>The column has an editor for a row being edited (EditMode).</summary>
    internal virtual bool CanEdit => false;

    /// <summary>A column of buttons (StilDataGridEditColumn): its clicks and keys do not reach the row.</summary>
    internal virtual bool IsCommand => false;

    /// <summary>The cell content of a row being edited.</summary>
    internal virtual RenderFragment RenderEditor(TItem item) => RenderCell(item);

    /// <summary>The field the editor changes, which the grid validates before saving.</summary>
    internal virtual FieldIdentifier? EditField(TItem item) => null;

    /// <summary>The column's distinct values in a query, sorted, for grouping.</summary>
    internal virtual IReadOnlyList<object?> GroupValues(IQueryable<TItem> query, bool descending) => [];

    /// <summary>The rows of a query whose value is the given group value.</summary>
    internal virtual IQueryable<TItem> WhereGroup(IQueryable<TItem> query, object? value) => query;

    /// <summary>A group value as text, formatted like the column's cells.</summary>
    internal virtual string FormatGroupValue(object? value) => value?.ToString() ?? "";

    internal string? AlignClass => Align switch
    {
        StilTextAlign.Center => "stil-text-center",
        StilTextAlign.End => "stil-text-end",
        _ => null
    };

    internal string? CellClass =>
        string.Join(' ', new[] { AlignClass, Class }.Where(c => !string.IsNullOrWhiteSpace(c))) is { Length: > 0 } css ? css : null;

    internal string? WidthStyle => Width is null ? null : $"width: {Width}";

    internal abstract RenderFragment RenderCell(TItem item);

    /// <summary>Sorts by the column; thenBy sorts within an earlier sort (grouping) instead of replacing it.</summary>
    internal virtual IQueryable<TItem> ApplySort(IQueryable<TItem> query, bool descending, bool thenBy = false) => query;

    internal virtual IQueryable<TItem> ApplyFilter(IQueryable<TItem> query, string value) => query;

    protected override void OnInitialized()
    {
        if (Grid is null)
        {
            throw new InvalidOperationException("Data grid columns must be placed in the Columns of a StilDataGrid.");
        }

        Grid.AddColumn(this);
    }

    public void Dispose()
    {
        Grid?.RemoveColumn(this);
        GC.SuppressFinalize(this);
    }
}
