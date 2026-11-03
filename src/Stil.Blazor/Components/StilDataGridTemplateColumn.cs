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

    /// <summary>The cell content of a row being edited (EditMode), for example Stil fields bound to the item.</summary>
    [Parameter]
    public RenderFragment<TItem>? EditTemplate { get; set; }

    internal override RenderFragment RenderCell(TItem item) => ChildContent(item);

    internal override bool CanEdit => EditTemplate is not null;

    internal override RenderFragment RenderEditor(TItem item) => EditTemplate is null ? ChildContent(item) : EditTemplate(item);
}
