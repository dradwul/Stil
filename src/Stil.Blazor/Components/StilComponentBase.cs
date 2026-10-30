using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>
/// Base class for Stil components. Unmatched attributes (id, data-*, aria-*, style, …) are
/// passed to the root element, and a <c>class</c> attribute is appended to the component's own.
/// </summary>
public abstract class StilComponentBase : ComponentBase
{
    private IStilLocalizer? _localizer;

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject]
    private IServiceProvider StilServices { get; set; } = default!;

    /// <summary>The culture of the built-in texts. The default is the current UI culture.</summary>
    protected virtual CultureInfo TextCulture => CultureInfo.CurrentUICulture;

    /// <summary>The component's classes followed by the consumer's <c>class</c> attribute.</summary>
    protected string Classes(params string?[] componentClasses) =>
        StilCssClasses.Merge(AdditionalAttributes, componentClasses);

    /// <summary><paramref name="text"/> when it is set, otherwise the localized built-in text for <paramref name="key"/>.</summary>
    protected internal string Localize(string? text, string key) =>
        text ?? StilLocalizer.Get(_localizer ??= StilLocalizer.From(StilServices), key, TextCulture);
}
