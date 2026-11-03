namespace Stil.Blazor.Components;

/// <summary>The sort order of a data grid column.</summary>
public enum StilSortDirection
{
    None,
    Ascending,
    Descending
}

/// <summary>How rows in a data grid can be selected.</summary>
public enum StilSelectionMode
{
    None,

    /// <summary>Clicking a row (or Enter on it) selects it. Bind SelectedItem.</summary>
    Single,

    /// <summary>A checkbox column, with a checkbox in the header for the current page. Bind SelectedItems.</summary>
    Multiple
}

/// <summary>
/// What a data grid asks an ItemsProvider for: a range of rows with the current sorting and
/// filters. For an IQueryable source (for example an EF Core DbSet), apply them with
/// <see cref="ApplyFiltersAndSorting"/> and <see cref="ApplyPaging"/>.
/// </summary>
public sealed class StilDataGridRequest<TItem>
{
    private readonly Func<IQueryable<TItem>, IQueryable<TItem>> _apply;
    private readonly Func<IQueryable<TItem>, IReadOnlyDictionary<string, object?>> _aggregate;

    internal StilDataGridRequest(
        int startIndex,
        int? count,
        string? sortBy,
        StilSortDirection sortDirection,
        IReadOnlyDictionary<string, string> filters,
        Func<IQueryable<TItem>, IQueryable<TItem>> apply,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? aggregates = null,
        Func<IQueryable<TItem>, IReadOnlyDictionary<string, object?>>? aggregate = null)
    {
        StartIndex = startIndex;
        Count = count;
        SortBy = sortBy;
        SortDirection = sortDirection;
        Filters = filters;
        _apply = apply;
        CancellationToken = cancellationToken;
        Aggregates = aggregates ?? [];
        _aggregate = aggregate ?? (_ => new Dictionary<string, object?>());
    }

    /// <summary>The index of the first row to return.</summary>
    public int StartIndex { get; }

    /// <summary>The number of rows to return, or null for all remaining rows.</summary>
    public int? Count { get; }

    /// <summary>The Key of the sorted column, or null when the grid is not sorted.</summary>
    public string? SortBy { get; }

    public StilSortDirection SortDirection { get; }

    /// <summary>The filter text per column Key, for columns that have a filter.</summary>
    public IReadOnlyDictionary<string, string> Filters { get; }

    /// <summary>
    /// The Keys of the columns that show a total (Aggregate). Return their values in
    /// StilDataGridResult.Aggregates, for example with <see cref="ComputeAggregates"/>. Empty when
    /// the grid only needs rows, as during an export.
    /// </summary>
    public IReadOnlyList<string> Aggregates { get; }

    /// <summary>Cancelled when the grid asks again before this request has finished.</summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>Applies the grid's filters and sorting to a query, using the columns' Property expressions.</summary>
    public IQueryable<TItem> ApplyFiltersAndSorting(IQueryable<TItem> query) => _apply(query);

    /// <summary>
    /// Computes the columns' totals (Sum, Average, Count, Min, Max) on a filtered query, using the
    /// columns' Property expressions, so EF Core runs them in the database. Pass the query after
    /// ApplyFiltersAndSorting and before paging.
    /// </summary>
    public IReadOnlyDictionary<string, object?> ComputeAggregates(IQueryable<TItem> query) =>
        Aggregates.Count == 0 ? new Dictionary<string, object?>() : _aggregate(query);

    /// <summary>Applies StartIndex and Count (Skip and Take).</summary>
    public IQueryable<TItem> ApplyPaging(IQueryable<TItem> query)
    {
        query = query.Skip(StartIndex);
        return Count is { } count ? query.Take(count) : query;
    }
}

/// <summary>
/// The rows for one data grid request, the total number of rows after filtering, and the totals
/// of the columns with an Aggregate, by column Key.
/// </summary>
public sealed record StilDataGridResult<TItem>(IReadOnlyList<TItem> Items, int TotalCount, IReadOnlyDictionary<string, object?>? Aggregates = null);

/// <summary>How a data grid column is totalled in the footer row.</summary>
public enum StilAggregate
{
    None,
    Sum,
    Average,
    Count,
    Min,
    Max
}
