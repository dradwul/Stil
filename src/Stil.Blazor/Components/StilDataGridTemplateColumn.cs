using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>
/// A data grid column with custom content, for example row actions. It cannot be sorted or
/// filtered; to sort custom content, use StilDataGridColumn with a Template.
/// </summary>
public sealed class StilDataGridTemplateColumn<TItem> : StilDataGridColumnBase<TItem>
{
    /// <summary>The cell content for a row.</summary>
    [Parameter, EditorRequired]
    public RenderFragment<TItem> ChildContent { get; set; } = default!;

    internal override RenderFragment RenderCell(TItem item) => ChildContent(item);
}
