using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>
/// A data grid for items with children: the same columns, sorting, filters, formats, editing and
/// totals as StilDataGrid, with the rows below an item shown when it is expanded. It follows the
/// WAI-ARIA treegrid pattern: the grid is one tab stop, and the arrow keys move between rows,
/// expand and collapse them. Items are the top-level rows; it shows every row without a pager by
/// default (PageSize 0) and has no grouping or ItemsProvider.
/// </summary>
[CascadingTypeParameter(nameof(TItem))]
public class StilTreeGrid<TItem> : StilDataGrid<TItem>
{
    public StilTreeGrid()
    {
        IsTree = true;
        PageSize = 0;
    }

    /// <summary>
    /// Returns a row's children, or null or an empty sequence for a row without them. With
    /// LoadChildren, null means the children are not loaded yet.
    /// </summary>
    [Parameter]
    public Func<TItem, IEnumerable<TItem>?>? Children
    {
        get => TreeChildren;
        set => TreeChildren = value;
    }

    /// <summary>
    /// Loads a row's children the first time it is expanded (when Children returns null
    /// for it, or there is no Children). The row shows a spinner meanwhile.
    /// </summary>
    [Parameter]
    public Func<TItem, Task<IEnumerable<TItem>>>? LoadChildren
    {
        get => TreeLoadChildren;
        set => TreeLoadChildren = value;
    }

    /// <summary>
    /// Whether a row whose children are not loaded yet has any, so it gets an expand toggle.
    /// The default is true; a row whose loaded children are none then shows as a leaf.
    /// </summary>
    [Parameter]
    public Func<TItem, bool>? HasChildren
    {
        get => TreeHasChildren;
        set => TreeHasChildren = value;
    }

    /// <summary>How many levels start expanded: 1 expands the top-level rows. The default is 0.</summary>
    [Parameter]
    public int ExpandedLevels
    {
        get => TreeExpandedLevels;
        set => TreeExpandedLevels = value;
    }

    /// <summary>Expands a row, loading its children with LoadChildren when they are not known.</summary>
    public Task ExpandAsync(TItem item) => SetRowExpandedAsync(item, true);

    /// <summary>Collapses a row.</summary>
    public Task CollapseAsync(TItem item) => SetRowExpandedAsync(item, false);

    /// <summary>Expands every row whose children are known. Children still to be loaded are not loaded.</summary>
    public Task ExpandAllAsync() => ExpandAllRowsAsync();

    /// <summary>Collapses every row, so only the top level shows.</summary>
    public Task CollapseAllAsync() => CollapseAllRowsAsync();
}
