using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Stil.Blazor.Components;

/// <summary>
/// One event (stil-event) in a calendar view: a button when it can be clicked, otherwise text.
/// The views work out the classes, position and texts; this only renders them.
/// </summary>
internal sealed record StilCalendarEventView
{
    public string Classes { get; init; } = "stil-event";

    public string? Style { get; init; }

    public string Title { get; init; } = "";

    /// <summary>A start time shown before the title.</summary>
    public string? Time { get; init; }

    /// <summary>A second line, for example the time and place.</summary>
    public string? Meta { get; init; }

    /// <summary>The accessible name of the button.</summary>
    public string? Label { get; init; }

    public bool Tabbable { get; init; } = true;

    /// <summary>Identifies the event in a stilweekedit event (data-stil-item).</summary>
    public string? ItemKey { get; init; }

    /// <summary>Adds the stil-event-resize handle.</summary>
    public bool Resizable { get; init; }

    public EventCallback OnClick { get; init; }

    public RenderFragment Render() => builder =>
    {
        var clickable = OnClick.HasDelegate;
        builder.OpenElement(0, clickable ? "button" : "div");
        if (clickable)
        {
            builder.AddAttribute(1, "type", "button");
            builder.AddAttribute(2, "aria-label", Label);
            builder.AddAttribute(3, "tabindex", Tabbable ? null : "-1");
            builder.AddAttribute(4, "onclick", OnClick);
            builder.AddEventStopPropagationAttribute(5, "onclick", true);
        }

        builder.AddAttribute(6, "class", Classes);
        builder.AddAttribute(7, "style", Style);
        builder.AddAttribute(8, "data-stil-item", ItemKey);

        builder.OpenElement(9, "span");
        builder.AddAttribute(10, "class", "stil-event-title");
        if (Time is not null)
        {
            builder.OpenElement(11, "span");
            builder.AddAttribute(12, "class", "stil-event-time");
            builder.AddContent(13, Time);
            builder.CloseElement();
        }

        builder.AddContent(14, Title);
        builder.CloseElement();

        if (!string.IsNullOrEmpty(Meta))
        {
            builder.OpenElement(15, "span");
            builder.AddAttribute(16, "class", "stil-event-meta");
            builder.AddContent(17, Meta);
            builder.CloseElement();
        }

        if (Resizable)
        {
            builder.OpenElement(18, "span");
            builder.AddAttribute(19, "class", "stil-event-resize");
            builder.AddAttribute(20, "aria-hidden", "true");
            builder.CloseElement();
        }

        builder.CloseElement();
    };
}
