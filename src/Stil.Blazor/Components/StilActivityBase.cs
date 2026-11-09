using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>
/// What <c>StilActivityLog</c> and <c>StilTimeline</c> share: the items and the selectors that
/// read them, like the calendar's, and how a time is shown.
/// </summary>
public abstract class StilActivityBase<TItem> : StilComponentBase
{
    /// <summary>The entries, in any order.</summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>When an entry happened, in local time.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, DateTime>? Time { get; set; }

    /// <summary>What happened. Without it the entry's ToString() is shown.</summary>
    [Parameter]
    public Func<TItem, string>? Title { get; set; }

    /// <summary>Who did it, if anyone.</summary>
    [Parameter]
    public Func<TItem, string?>? Actor { get; set; }

    /// <summary>The kind of entry, for example "Check-in" or "Comment". The log filters by it.</summary>
    [Parameter]
    public Func<TItem, string?>? Type { get; set; }

    /// <summary>The marker's color. Secondary when not set.</summary>
    [Parameter]
    public Func<TItem, StilVariant?>? Variant { get; set; }

    /// <summary>An icon in the marker, by name (see StilIcons).</summary>
    [Parameter]
    public Func<TItem, string?>? Icon { get; set; }

    /// <summary>Extra text under the title.</summary>
    [Parameter]
    public Func<TItem, string?>? Details { get; set; }

    /// <summary>Extra content under the title, for anything more than text.</summary>
    [Parameter]
    public RenderFragment<TItem>? DetailsTemplate { get; set; }

    /// <summary>The current time, for the relative times. The default is the clock; set it to show a fixed moment.</summary>
    [Parameter]
    public DateTime? Now { get; set; }

    protected DateTime CurrentTime => Now ?? DateTime.Now;

    protected DateTime TimeOf(TItem item) => Time is null ? default : Time(item);

    protected string TitleOf(TItem item) => Title?.Invoke(item) ?? item?.ToString() ?? "";

    protected string MarkerClass(TItem item) => Variant?.Invoke(item) is { } variant
        ? $"stil-log-marker {StilCssClasses.Variant("stil-log-marker", variant)}"
        : "stil-log-marker";

    /// <summary>
    /// "Just now", "5 minutes ago" or "2 hours ago" within six hours on the same day, otherwise
    /// the time of day.
    /// </summary>
    protected string RelativeTime(DateTime time) => StilRelativeTime.Format(time, CurrentTime, key => Localize(null, key), TextCulture);

    /// <summary>"Today", "Yesterday" or the long date.</summary>
    protected string DayName(DateTime day)
    {
        var today = CurrentTime.Date;
        return day == today ? Localize(null, "Calendar.Today")
            : day == today.AddDays(-1) ? Localize(null, "ActivityLog.Yesterday")
            : day.ToString("D", TextCulture);
    }

    /// <summary>The full date and time, for the title of a time element.</summary>
    protected string FullTime(DateTime time) => time.ToString("f", TextCulture);

    protected static string MachineTime(DateTime time) => time.ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
}
