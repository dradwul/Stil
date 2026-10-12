using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>
/// Base class for Stil components. Unmatched attributes (id, data-*, aria-*, style, …) are
/// passed to the root element, and a <c>class</c> attribute is appended to the component's own.
/// </summary>
public abstract class StilComponentBase : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>The component's classes followed by the consumer's <c>class</c> attribute.</summary>
    protected string Classes(params string?[] componentClasses) =>
        StilCssClasses.Merge(AdditionalAttributes, componentClasses);
}
