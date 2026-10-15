using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Stil.Blazor.Components;

/// <summary>Renders an <c>h1</c>–<c>h6</c> element chosen by <see cref="Level"/>.</summary>
public sealed class StilHeading : StilComponentBase
{
    /// <summary>The heading level, 1 to 6. The default is 2.</summary>
    [Parameter]
    public int Level { get; set; } = 2;

    /// <summary>The heading text.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, $"h{Math.Clamp(Level, 1, 6)}");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}
