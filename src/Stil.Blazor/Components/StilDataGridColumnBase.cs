using Microsoft.AspNetCore.Components;

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

    internal virtual string DisplayTitle => Title ?? "";

    internal virtual string ColumnKey => Key ?? Title ?? "";

    internal virtual bool CanSort => false;

    internal virtual bool CanFilter => false;

    /// <summary>Fixed filter choices (for enums and booleans), or null for a text filter.</summary>
    internal virtual IReadOnlyList<(string Value, string Text)>? FilterOptions => null;

    internal virtual StilSortDirection InitialSortDirection => StilSortDirection.None;

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

    internal virtual IQueryable<TItem> ApplySort(IQueryable<TItem> query, bool descending) => query;

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
