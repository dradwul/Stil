namespace Stil.Blazor.Components;

/// <summary>A column of a <c>StilKanban</c>.</summary>
/// <param name="Key">The value of the board's <c>Column</c> selector for the cards in it.</param>
/// <param name="Title">The heading.</param>
/// <param name="Limit">The most cards the column should hold (a WIP limit); more marks it as over.</param>
public sealed record StilKanbanColumn(string Key, string Title, int? Limit = null);

/// <summary>
/// A card moved on a <c>StilKanban</c>, by dragging or with the keyboard. The board does not change
/// your data: apply the move, for example with <see cref="ApplyTo"/>.
/// </summary>
/// <param name="Item">The card's item.</param>
/// <param name="From">The column it came from.</param>
/// <param name="To">The column it goes to (the same for a move within a column).</param>
/// <param name="Index">Its place among the other cards of that column.</param>
/// <param name="Before">The card it now comes before, or the default when it is last.</param>
/// <param name="After">The card it now comes after, or the default when it is first.</param>
public sealed record StilKanbanMove<TItem>(TItem Item, string From, string To, int Index, TItem? Before, TItem? After)
{
    /// <summary>
    /// Moves the item in a list that holds the cards in board order: out of its place, into
    /// <see cref="To"/> through <paramref name="withColumn"/>, and next to <see cref="Before"/> or
    /// <see cref="After"/>. <paramref name="withColumn"/> returns the item with its new column: the
    /// same object after setting a property, or a copy of a record (<c>(t, c) =&gt; t with { Status = c }</c>).
    /// </summary>
    public void ApplyTo(IList<TItem> items, Func<TItem, string, TItem> withColumn)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(withColumn);
        items.Remove(Item);
        var moved = withColumn(Item, To);
        var index = Before is not null && items.IndexOf(Before) is >= 0 and var before ? before
            : After is not null && items.IndexOf(After) is >= 0 and var after ? after + 1
            : items.Count;
        items.Insert(index, moved);
    }
}
