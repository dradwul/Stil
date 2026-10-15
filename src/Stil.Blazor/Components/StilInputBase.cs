using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Stil.Blazor.Components;

/// <summary>
/// Base class for Stil form fields. A field renders a label, the control, an optional hint and
/// its validation messages, and wires up id, aria-describedby and aria-invalid.
/// Use it with <c>@bind-Value</c>, inside or outside an <see cref="EditForm"/>.
/// </summary>
public abstract class StilInputBase<TValue> : ComponentBase, IDisposable
{
    private EditContext? _subscribedContext;
    private readonly string _generatedId;

    protected StilInputBase()
    {
        _generatedId = StilIds.Next("field");
    }

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    [Parameter]
    public string? Label { get; set; }

    /// <summary>Help text under the control, linked with aria-describedby.</summary>
    [Parameter]
    public string? Hint { get; set; }

    /// <summary>The control's id. A unique id is generated when it is not set.</summary>
    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>Extra classes on the control itself. A <c>class</c> attribute goes on the field wrapper.</summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Unmatched attributes. <c>class</c> and <c>style</c> go on the field wrapper (for layout, such as
    /// grid columns); everything else (placeholder, autocomplete, aria-label, …) goes on the control.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>The consumer's <c>class</c> attribute, applied to the field wrapper.</summary>
    protected string? FieldClass => AdditionalAttributes?.TryGetValue("class", out var value) == true ? value?.ToString() : null;

    /// <summary>The consumer's <c>style</c> attribute, applied to the field wrapper.</summary>
    protected string? FieldStyle => AdditionalAttributes?.TryGetValue("style", out var value) == true ? value?.ToString() : null;

    /// <summary>Text joined to the start of the control, for example "https://" or a currency.</summary>
    [Parameter]
    public string? PrependText { get; set; }

    /// <summary>Text joined to the end of the control, for example "kg" or "@example.com".</summary>
    [Parameter]
    public string? AppendText { get; set; }

    /// <summary>Content joined to the start of the control, for example a StilButton.</summary>
    [Parameter]
    public RenderFragment? Prepend { get; set; }

    /// <summary>Content joined to the end of the control, for example a search StilButton.</summary>
    [Parameter]
    public RenderFragment? Append { get; set; }

    protected EditContext? EditContext => CascadedEditContext;

    /// <summary>
    /// The expression passed to Blazor's inner input. Blazor's inputs require one, so without
    /// @bind-Value (for example Value plus ValueChanged) an expression for this component's Value is used.
    /// </summary>
    protected Expression<Func<TValue?>> BoundExpression => ValueExpression ?? (() => Value);

    /// <summary>
    /// Wraps the control in a stil-input-group with the prepend and append content, or returns it
    /// unchanged when there is none. Used by the text-like fields.
    /// </summary>
    protected RenderFragment WithAddons(RenderFragment control) => builder =>
    {
        var hasStart = Prepend is not null || !string.IsNullOrWhiteSpace(PrependText);
        var hasEnd = Append is not null || !string.IsNullOrWhiteSpace(AppendText);
        if (!hasStart && !hasEnd)
        {
            builder.AddContent(0, control);
            return;
        }

        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "class", "stil-input-group");
        AddAddon(builder, 3, Prepend, PrependText);
        builder.AddContent(10, control);
        AddAddon(builder, 11, Append, AppendText);
        builder.CloseElement();
    };

    private static void AddAddon(RenderTreeBuilder builder, int sequence, RenderFragment? content, string? text)
    {
        if (content is not null)
        {
            builder.AddContent(sequence, content);
        }
        else if (!string.IsNullOrWhiteSpace(text))
        {
            builder.OpenElement(sequence + 1, "span");
            builder.AddAttribute(sequence + 2, "class", "stil-input-group-text");
            builder.AddContent(sequence + 3, text);
            builder.CloseElement();
        }
    }

    protected string InputId => Id ?? _generatedId;

    protected string HintId => $"{InputId}-hint";

    protected string ErrorId => $"{InputId}-error";

    protected FieldIdentifier? Field { get; private set; }

    /// <summary>The current validation messages for the bound field.</summary>
    protected IReadOnlyList<string> Messages =>
        EditContext is not null && Field is { } fieldId
            ? EditContext.GetValidationMessages(fieldId).ToList()
            : [];

    protected string? DescribedBy
    {
        get
        {
            var ids = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(Hint))
            {
                ids.Add(HintId);
            }

            if (Messages.Count > 0)
            {
                ids.Add(ErrorId);
            }

            return ids.Count == 0 ? null : string.Join(' ', ids);
        }
    }

    /// <summary>
    /// The attributes for the inner control: the consumer's attributes (except class and style),
    /// plus id, aria-describedby, disabled and the given Stil classes followed by InputClass.
    /// </summary>
    protected IReadOnlyDictionary<string, object> ControlAttributes(params string?[] classes)
    {
        var attributes = AdditionalAttributes is null
            ? new Dictionary<string, object>()
            : AdditionalAttributes.Where(a => a.Key is not ("class" or "style")).ToDictionary();

        attributes["id"] = InputId;
        attributes["class"] = string.Join(' ', classes.Append(InputClass).Where(c => !string.IsNullOrWhiteSpace(c)));

        if (DescribedBy is { } describedBy)
        {
            attributes["aria-describedby"] = describedBy;
        }

        if (Disabled)
        {
            attributes["disabled"] = true;
        }

        return attributes;
    }

    protected override void OnParametersSet()
    {
        Field = ValueExpression is null ? null : FieldIdentifier.Create(ValueExpression);

        if (!ReferenceEquals(_subscribedContext, EditContext))
        {
            Unsubscribe();
            _subscribedContext = EditContext;
            if (_subscribedContext is not null)
            {
                _subscribedContext.OnValidationStateChanged += OnValidationStateChanged;
            }
        }
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) =>
        _ = InvokeAsync(StateHasChanged);

    private void Unsubscribe()
    {
        if (_subscribedContext is not null)
        {
            _subscribedContext.OnValidationStateChanged -= OnValidationStateChanged;
        }
    }

    public void Dispose()
    {
        Unsubscribe();
        GC.SuppressFinalize(this);
    }
}
