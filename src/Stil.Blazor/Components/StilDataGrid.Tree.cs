using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Stil.Blazor.Components;

// The rows of a StilTreeGrid: items with children, shown below their parent when it is expanded.
public partial class StilDataGrid<TItem>
{
    // Set by StilTreeGrid.
    private protected bool IsTree;
    private protected Func<TItem, IEnumerable<TItem>?>? TreeChildren;
    private protected Func<TItem, Task<IEnumerable<TItem>>>? TreeLoadChildren;
    private protected Func<TItem, bool>? TreeHasChildren;
    private protected int TreeExpandedLevels;

    private readonly HashSet<object> _expandedRows = [];
    private readonly HashSet<object> _collapsedRows = [];
    private readonly HashSet<object> _loadingRows = [];
    private readonly Dictionary<object, List<TItem>> _loadedChildren = [];
    private List<TreeNode> _treeNodes = [];
    private List<TreeRow> _treeRows = [];
    private List<TItem> _treeLeaves = [];
    private readonly Dictionary<string, object?> _treeTotals = [];
    private bool _treeStale = true;
    private object? _focusedRow;
    private object? _tabStop;
    private bool _focusRowPending;

    // An item of the filtered and sorted tree. Children is null while they are not loaded.
    private sealed class TreeNode(TItem item, object key, List<TreeNode>? children, bool viaDescendant)
    {
        public TItem Item { get; } = item;

        public object Key { get; } = key;

        public List<TreeNode>? Children { get; } = children;

        /// <summary>Shown only because a row below it matches the filters.</summary>
        public bool ViaDescendant { get; } = viaDescendant;

        /// <summary>The rows without known children below this one (or itself), which its totals cover.</summary>
        public List<TItem> Leaves { get; } = children is { Count: > 0 } ? children.SelectMany(c => c.Leaves).ToList() : [item];
    }

    /// <summary>
    /// A shown row of the tree: its level (1 for the top), parent and place among its siblings.
    /// Leaves are the rows its totals cover, for a row with children.
    /// </summary>
    private sealed record TreeRow(TItem Item, object Key, int Level, TreeRow? Parent, bool HasChildren, bool Expanded, int SetSize, int Position, List<TItem>? Leaves)
    {
        /// <summary>The totals computed so far, by column key.</summary>
        public Dictionary<string, object?> Totals { get; } = [];
    }

    // The children the grid knows, or null when they are still to be loaded.
    private IEnumerable<TItem>? KnownChildren(TItem item)
    {
        if (TreeChildren?.Invoke(item) is { } children)
        {
            return children;
        }

        if (TreeLoadChildren is null)
        {
            return [];
        }

        return _loadedChildren.TryGetValue(KeyOf(item), out var loaded) ? loaded : null;
    }

    private List<TItem> SortSiblings(IEnumerable<TItem>? items)
    {
        var list = items?.ToList() ?? [];
        return _sortColumn is not null && _sortDirection != StilSortDirection.None
            ? _sortColumn.ApplySort(list.AsQueryable(), _sortDirection == StilSortDirection.Descending).ToList()
            : list;
    }

    // Builds the tree, keeping the rows that match the filters with all rows below them, and the
    // rows above them; then the shown rows, following the expanded state.
    private void BuildTree()
    {
        HashSet<object>? matches = null;
        if (_filters.Any(f => !string.IsNullOrWhiteSpace(f.Value)))
        {
            var all = new List<TItem>();
            Collect(Items, 0);
            matches = ApplyFilters(all.AsQueryable()).Select(KeyOf).ToHashSet();

            void Collect(IEnumerable<TItem>? items, int depth)
            {
                foreach (var item in items ?? [])
                {
                    all.Add(item);
                    // The depth check stops a cycle in the data from recursing forever.
                    if (depth < 100)
                    {
                        Collect(KnownChildren(item), depth + 1);
                    }
                }
            }
        }

        _treeNodes = Build(Items, matches is null, 0);
        var rows = new List<TreeRow>();
        Flatten(_treeNodes, 1, null);

        _treeRows = rows;
        _treeLeaves = _treeNodes.SelectMany(n => n.Leaves).ToList();
        _treeTotals.Clear();
        _total = _rowCount = rows.Count;
        _treeStale = false;

        List<TreeNode> Build(IEnumerable<TItem>? items, bool all, int depth)
        {
            var kept = new List<TreeNode>();
            foreach (var item in SortSiblings(items))
            {
                var key = KeyOf(item);
                var matched = all || matches!.Contains(key);
                var known = depth < 100 ? KnownChildren(item) : [];
                var children = known is null ? null : Build(known, matched, depth + 1);
                if (matched || children is { Count: > 0 })
                {
                    kept.Add(new TreeNode(item, key, children, viaDescendant: !matched));
                }
            }

            return kept;
        }

        void Flatten(List<TreeNode> nodes, int level, TreeRow? parent)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var known = node.Children is { Count: > 0 };
                var hasChildren = known || (node.Children is null && (TreeHasChildren?.Invoke(node.Item) ?? true));
                // Rows still to be loaded show as expanded while they load.
                var expanded = known ? IsRowExpanded(node, level) : hasChildren && _loadingRows.Contains(node.Key);
                var row = new TreeRow(node.Item, node.Key, level, parent, hasChildren, expanded, nodes.Count, i + 1, known ? node.Leaves : null);
                rows.Add(row);
                if (expanded && known)
                {
                    Flatten(node.Children!, level + 1, row);
                }
            }
        }
    }

    // Rows above a filter match open by themselves; a row the user closed stays closed.
    private bool IsRowExpanded(TreeNode node, int level) =>
        !_collapsedRows.Contains(node.Key) && (_expandedRows.Contains(node.Key) || level <= TreeExpandedLevels || node.ViaDescendant);

    // A column's total over a row's leaves, or over all leaves for the footer (row null). Computed
    // when shown, so columns that register after the tree is built get theirs too.
    private object? TreeTotal(StilDataGridColumnBase<TItem> column, TreeRow? row)
    {
        var (totals, leaves) = row is null ? (_treeTotals, _treeLeaves) : (row.Totals, row.Leaves ?? [row.Item]);
        if (!totals.TryGetValue(column.ColumnKey, out var total))
        {
            totals[column.ColumnKey] = total = column.AggregateValue is { } custom ? custom(leaves) : column.ComputeAggregate(leaves.AsQueryable());
        }

        return total;
    }

    // The tree's rows for the current page, or all shown rows with Virtualize.
    private void ShowTree()
    {
        BuildTree();
        var rows = _treeRows;
        if (Paged)
        {
            if (_pageNumber > 1 && (_pageNumber - 1) * _pageSize >= rows.Count)
            {
                _pageNumber = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)_pageSize));
            }

            rows = rows.Skip((_pageNumber - 1) * _pageSize).Take(_pageSize).ToList();
        }

        _entries = rows.Select(r => new Entry(null, r.Item, r)).ToList();
        _page = rows.Select(r => r.Item).ToList();
        _groupRows = [];
        UpdateTabStop();
    }

    private ItemsProviderResult<Entry> ProvideTree(int startIndex, int count)
    {
        if (_treeStale)
        {
            BuildTree();
            UpdateTabStop();
            _ = InvokeAsync(StateHasChanged);
        }

        var rows = _treeRows.Skip(startIndex).Take(count).ToList();
        return new ItemsProviderResult<Entry>(rows.Select(r => new Entry(null, r.Item, r)).ToList(), _treeRows.Count);
    }

    // Every row of the filtered and sorted tree, expanded or not, for the CSV export.
    private List<TItem> TreeExportRows()
    {
        BuildTree();
        var rows = new List<TItem>();
        Add(_treeNodes);
        return rows;

        void Add(List<TreeNode>? nodes)
        {
            foreach (var node in nodes ?? [])
            {
                rows.Add(node.Item);
                Add(node.Children);
            }
        }
    }

    // ----- Expanding -------------------------------------------------------------------------

    /// <summary>Expands or collapses a row; expanding a row whose children are not known loads them with LoadChildren.</summary>
    private protected async Task SetRowExpandedAsync(TItem item, bool expanded)
    {
        var key = KeyOf(item);
        if (!expanded)
        {
            _expandedRows.Remove(key);
            _collapsedRows.Add(key);
            await RefreshAsync();
            return;
        }

        _collapsedRows.Remove(key);
        _expandedRows.Add(key);
        if (KnownChildren(item) is null && TreeLoadChildren is { } load && _loadingRows.Add(key))
        {
            await RefreshAsync();
            try
            {
                _loadedChildren[key] = (await load(item))?.ToList() ?? [];
            }
            finally
            {
                _loadingRows.Remove(key);
            }
        }

        await RefreshAsync();
    }

    /// <summary>Expands every row whose children are known (loaded rows; nothing is loaded).</summary>
    private protected async Task ExpandAllRowsAsync()
    {
        _collapsedRows.Clear();
        _expandedRows.UnionWith(KnownParents());
        await RefreshAsync();
    }

    /// <summary>Collapses every row, so only the top level shows.</summary>
    private protected async Task CollapseAllRowsAsync()
    {
        _expandedRows.Clear();
        _collapsedRows.UnionWith(KnownParents());
        await RefreshAsync();
    }

    private List<object> KnownParents()
    {
        var keys = new List<object>();
        Add(Items, 0);
        return keys;

        void Add(IEnumerable<TItem>? items, int depth)
        {
            foreach (var item in items ?? [])
            {
                var children = KnownChildren(item)?.ToList();
                if (children is { Count: > 0 } && depth < 100)
                {
                    keys.Add(KeyOf(item));
                    Add(children, depth + 1);
                }
            }
        }
    }

    // ----- Keyboard --------------------------------------------------------------------------

    // Only one row is in the tab order: the focused one, else the first.
    private void UpdateTabStop()
    {
        var rows = ShownTreeRows();
        _tabStop = _focusedRow is not null && rows.Exists(r => Equals(r.Key, _focusedRow)) ? _focusedRow : rows.FirstOrDefault()?.Key;
    }

    private List<TreeRow> ShownTreeRows() =>
        Virtualize ? _treeRows : _entries.Select(e => e.Node).OfType<TreeRow>().ToList();

    private int? TreeTabIndex(TreeRow row) => Equals(row.Key, _tabStop) ? 0 : -1;

    private void OnTreeRowFocus(TreeRow row)
    {
        _focusedRow = row.Key;
        _tabStop = row.Key;
    }

    // The WAI-ARIA treegrid keys for rows: Up, Down, Home and End move; Right expands a row or
    // moves to its first child, Left collapses it or moves to its parent. True when handled.
    private async Task<bool> OnTreeKeyDownAsync(KeyboardEventArgs e, TreeRow row)
    {
        var rows = ShownTreeRows();
        var index = rows.FindIndex(r => Equals(r.Key, row.Key));
        if (index < 0 || e.AltKey || e.CtrlKey || e.MetaKey)
        {
            return false;
        }

        switch (StilCulture.LogicalArrow(e.Key, TextCulture))
        {
            case "ArrowDown":
                FocusRow(index + 1 < rows.Count ? rows[index + 1] : null);
                return true;
            case "ArrowUp":
                FocusRow(index > 0 ? rows[index - 1] : null);
                return true;
            case "Home":
                FocusRow(rows[0]);
                return true;
            case "End":
                FocusRow(rows[^1]);
                return true;
            case "ArrowRight" when row.HasChildren && !row.Expanded:
                await SetRowExpandedAsync(row.Item, true);
                return true;
            case "ArrowRight" when row.Expanded:
                FocusRow(index + 1 < rows.Count && rows[index + 1].Level > row.Level ? rows[index + 1] : null);
                return true;
            case "ArrowLeft" when row.Expanded:
                await SetRowExpandedAsync(row.Item, false);
                return true;
            case "ArrowLeft":
                FocusRow(row.Parent is { } parent ? rows.Find(r => Equals(r.Key, parent.Key)) : null);
                return true;
            default:
                return false;
        }
    }

    private void FocusRow(TreeRow? row)
    {
        if (row is null)
        {
            return;
        }

        _focusedRow = row.Key;
        _tabStop = row.Key;
        _focusRowPending = true;
    }
}
