using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Stil.Blazor.Components;

/// <summary>
/// The data model shared by StilCalendar, StilWeekPlanner and StilAgenda. Items are any type, read
/// through selectors like the columns of StilDataGrid. Everything else is worked out from them:
/// how booked a day is, the free time in the working day and the layout of overlapping events.
/// </summary>
public abstract class StilCalendarBase<TItem> : StilComponentBase
{
    internal const int MinutesPerDay = 24 * 60;
    internal const int SnapMinutes = 15;

    private static readonly DayOfWeek[] MondayToFriday =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    private Dictionary<DateOnly, List<StilCalendarOccurrence<TItem>>>? _byDay;
    private readonly List<(TItem Item, int Index, StilRecurrence Rule)> _recurring = [];
    private readonly Dictionary<DateOnly, IReadOnlyList<StilCalendarOccurrence<TItem>>> _withRecurring = [];

    /// <summary>The events. They are read again every time the parameters are set.</summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>When an item starts.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, DateTime>? Start { get; set; }

    /// <summary>When an item ends. An item can run over several days.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, DateTime>? End { get; set; }

    /// <summary>The text of an item. Without it the item's ToString() is shown.</summary>
    [Parameter]
    public Func<TItem, string>? Title { get; set; }

    /// <summary>The color of an item as a variant, for example by its calendar. Primary when not set.</summary>
    [Parameter]
    public Func<TItem, StilVariant?>? Variant { get; set; }

    /// <summary>An own CSS color for an item (--stil-event-color), used when it has no variant.</summary>
    [Parameter]
    public Func<TItem, string?>? Color { get; set; }

    /// <summary>Extra text for an item, for example its place or calendar.</summary>
    [Parameter]
    public Func<TItem, string?>? Details { get; set; }

    /// <summary>
    /// The rule an item repeats by, or null for a one-off item. Its Start and End are the first
    /// occurrence; every occurrence is shown. To move or cancel one occurrence, add it to the rule's
    /// Exceptions (and add a one-off item for a moved one).
    /// </summary>
    [Parameter]
    public Func<TItem, StilRecurrence?>? Recurrence { get; set; }

    /// <summary>Marks an item as a draft (stil-event-draft): an event that is not saved yet.</summary>
    [Parameter]
    public Func<TItem, bool>? Draft { get; set; }

    /// <summary>Raised when an event is clicked. Without it events are shown as text, not buttons.</summary>
    [Parameter]
    public EventCallback<TItem> OnItemClick { get; set; }

    /// <summary>The current time, for today, the now line and the free time left today. The default is DateTime.Now.</summary>
    [Parameter]
    public DateTime? Now { get; set; }

    /// <summary>The start of the working day, for the load of a day and its free time. The default is 08:00.</summary>
    [Parameter]
    public TimeOnly WorkdayStart { get; set; } = new(8, 0);

    /// <summary>The end of the working day. The default is 17:00.</summary>
    [Parameter]
    public TimeOnly WorkdayEnd { get; set; } = new(17, 0);

    /// <summary>The days with a working day. The default is Monday to Friday.</summary>
    [Parameter]
    public IReadOnlyCollection<DayOfWeek> WorkingDays { get; set; } = MondayToFriday;

    /// <summary>Tint each day by how booked its working day is.</summary>
    [Parameter]
    public bool ShowLoad { get; set; } = true;

    /// <summary>Show the free blocks in the working day, from now on.</summary>
    [Parameter]
    public bool ShowFreeTime { get; set; } = true;

    /// <summary>The shortest gap, in minutes, that counts as free time. The default is 60.</summary>
    [Parameter]
    public int MinFreeMinutes { get; set; } = 60;

    /// <summary>The culture for names, times and the first day of the week. The default is the current culture.</summary>
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>The first day of the week. The default comes from the culture.</summary>
    [Parameter]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>The level of the title heading, 1 to 6. The default is 2.</summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 2;

    /// <summary>The text of a free block.</summary>
    [Parameter]
    public string? FreeText { get; set; }

    /// <summary>How a number of hours is written; {0} is the hours. The default is the localized "{0:0.#} h".</summary>
    [Parameter]
    public string? HoursFormat { get; set; }

    /// <summary>The text of the button (or heading) for today.</summary>
    [Parameter]
    public string? TodayText { get; set; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        _byDay = null;
        _withRecurring.Clear();
        return base.SetParametersAsync(parameters);
    }

    internal CultureInfo CurrentCulture => Culture ?? CultureInfo.CurrentCulture;

    /// <inheritdoc />
    protected override CultureInfo TextCulture => Culture ?? CultureInfo.CurrentUICulture;

    internal DateTime CurrentNow => Now ?? DateTime.Now;

    internal DateOnly Today => DateOnly.FromDateTime(CurrentNow);

    internal DayOfWeek FirstDay => FirstDayOfWeek ?? CurrentCulture.DateTimeFormat.FirstDayOfWeek;

    internal int WorkdayStartMinute => (int)WorkdayStart.ToTimeSpan().TotalMinutes;

    internal int WorkdayEndMinute => Math.Max(WorkdayStartMinute, (int)WorkdayEnd.ToTimeSpan().TotalMinutes);

    internal bool IsWorkingDay(DateOnly date) => WorkingDays.Contains(date.DayOfWeek);

    internal DateOnly StartOfWeek(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek - (int)FirstDay + 7) % 7));

    /// <summary>ISO 8601 week numbers when the week starts on Monday, else the culture's week rule.</summary>
    internal int WeekNumber(DateOnly date) => FirstDay == DayOfWeek.Monday
        ? ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue))
        : CurrentCulture.Calendar.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue), CurrentCulture.DateTimeFormat.CalendarWeekRule, FirstDay);

    /// <summary>The items on a day, by start time; longer ones first when they start together.</summary>
    internal IReadOnlyList<StilCalendarOccurrence<TItem>> OccurrencesOn(DateOnly date)
    {
        var single = (_byDay ??= IndexByDay()).TryGetValue(date, out var occurrences) ? occurrences : [];
        if (_recurring.Count == 0)
        {
            return single;
        }

        if (!_withRecurring.TryGetValue(date, out var all))
        {
            var list = new List<StilCalendarOccurrence<TItem>>(single);
            foreach (var (item, index, rule) in _recurring)
            {
                AddRecurring(list, item, index, rule, date);
            }

            list.Sort(ByStart);
            _withRecurring[date] = all = list;
        }

        return all;
    }

    // The occurrences of a recurring item that fall on a date: one that starts that day, or an
    // earlier one that runs into it.
    private void AddRecurring(List<StilCalendarOccurrence<TItem>> list, TItem item, int index, StilRecurrence rule, DateOnly date)
    {
        var first = Start!(item);
        var length = End!(item) - first;
        if (length < TimeSpan.Zero)
        {
            length = TimeSpan.Zero;
        }

        var midnight = date.ToDateTime(TimeOnly.MinValue);
        var spanDays = Math.Min(366, (int)Math.Ceiling(length.TotalDays));
        for (var back = spanDays; back >= 0; back--)
        {
            if (rule.OccurrenceOn(first, date.AddDays(-back)) is not { } start)
            {
                continue;
            }

            var end = start + length;
            if (back > 0 && end <= midnight)
            {
                continue;
            }

            var from = Math.Max(0, (int)(start - midnight).TotalMinutes);
            var to = Math.Min(MinutesPerDay, (int)Math.Ceiling((end - midnight).TotalMinutes));
            list.Add(new(item, index, date, from, Math.Max(from, to)) { OccurrenceStart = start });
        }
    }

    private static int ByStart(StilCalendarOccurrence<TItem> a, StilCalendarOccurrence<TItem> b) =>
        a.StartMinute != b.StartMinute ? a.StartMinute.CompareTo(b.StartMinute) : b.EndMinute.CompareTo(a.EndMinute);

    private Dictionary<DateOnly, List<StilCalendarOccurrence<TItem>>> IndexByDay()
    {
        var byDay = new Dictionary<DateOnly, List<StilCalendarOccurrence<TItem>>>();
        if (Items is null || Start is null || End is null)
        {
            return byDay;
        }

        _recurring.Clear();
        var index = 0;
        foreach (var item in Items)
        {
            if (Recurrence?.Invoke(item) is { } rule)
            {
                _recurring.Add((item, index++, rule));
                continue;
            }

            var start = Start(item);
            var end = End(item);
            if (end < start)
            {
                end = start;
            }

            // An item that ends at midnight belongs to the day before. A year is the longest span shown.
            var first = DateOnly.FromDateTime(start);
            var last = DateOnly.FromDateTime(end > start ? end.AddTicks(-1) : end);
            for (var day = first; day <= last && day.DayNumber - first.DayNumber < 366; day = day.AddDays(1))
            {
                var midnight = day.ToDateTime(TimeOnly.MinValue);
                var from = Math.Max(0, (int)(start - midnight).TotalMinutes);
                var to = Math.Min(MinutesPerDay, (int)Math.Ceiling((end - midnight).TotalMinutes));
                if (!byDay.TryGetValue(day, out var list))
                {
                    byDay[day] = list = [];
                }

                list.Add(new(item, index, day, from, Math.Max(from, to)));
            }

            index++;
        }

        foreach (var list in byDay.Values)
        {
            list.Sort(ByStart);
        }

        return byDay;
    }

    /// <summary>The booked minutes of a day's working day. Overlapping items count once.</summary>
    internal int BookedMinutes(DateOnly date)
    {
        var total = 0;
        var covered = WorkdayStartMinute;
        foreach (var occurrence in OccurrencesOn(date))
        {
            var from = Math.Max(occurrence.StartMinute, covered);
            var to = Math.Min(occurrence.EndMinute, WorkdayEndMinute);
            if (to > from)
            {
                total += to - from;
            }

            covered = Math.Max(covered, to);
        }

        return total;
    }

    /// <summary>How booked a day's working day is, from 0 (free) to 1 (full).</summary>
    internal double Load(DateOnly date)
    {
        var length = WorkdayEndMinute - WorkdayStartMinute;
        return length > 0 ? Math.Min(1, BookedMinutes(date) / (double)length) : 0;
    }

    /// <summary>
    /// The gaps of at least <see cref="MinFreeMinutes"/> in a working day, as minutes from midnight.
    /// Days before today have none; today's start from now, rounded up to 15 minutes.
    /// </summary>
    internal IReadOnlyList<(int Start, int End)> FreeBlocks(DateOnly date)
    {
        if (!ShowFreeTime || !IsWorkingDay(date) || date < Today)
        {
            return [];
        }

        var from = WorkdayStartMinute;
        if (date == Today)
        {
            var now = (int)Math.Ceiling(CurrentNow.TimeOfDay.TotalMinutes / SnapMinutes) * SnapMinutes;
            from = Math.Max(from, now);
        }

        var minimum = Math.Max(1, MinFreeMinutes);
        var blocks = new List<(int Start, int End)>();
        foreach (var occurrence in OccurrencesOn(date))
        {
            var end = Math.Min(occurrence.StartMinute, WorkdayEndMinute);
            if (end - from >= minimum)
            {
                blocks.Add((from, end));
            }

            from = Math.Max(from, occurrence.EndMinute);
        }

        if (WorkdayEndMinute - from >= minimum)
        {
            blocks.Add((from, WorkdayEndMinute));
        }

        return blocks;
    }

    /// <summary>
    /// Gives overlapping placements a column each. A cluster of overlapping items shares one
    /// column count, so the week grid can cascade them.
    /// </summary>
    internal static void Layout(IReadOnlyList<StilCalendarPlacement<TItem>> placements)
    {
        var cluster = new List<StilCalendarPlacement<TItem>>();
        var clusterEnd = int.MinValue;
        foreach (var placement in placements.OrderBy(p => p.Start).ThenByDescending(p => p.End))
        {
            if (placement.Start >= clusterEnd)
            {
                Close(cluster);
                clusterEnd = placement.End;
            }
            else
            {
                clusterEnd = Math.Max(clusterEnd, placement.End);
            }

            var column = 0;
            while (cluster.Exists(p => p.Column == column && p.End > placement.Start))
            {
                column++;
            }

            placement.Column = column;
            cluster.Add(placement);
        }

        Close(cluster);

        static void Close(List<StilCalendarPlacement<TItem>> cluster)
        {
            var columns = cluster.Count == 0 ? 1 : cluster.Max(p => p.Column) + 1;
            cluster.ForEach(p => p.Columns = columns);
            cluster.Clear();
        }
    }

    /// <summary>The click handler of an event, or none when OnItemClick is not set (the event is then text).</summary>
    internal EventCallback ItemClick(TItem item) =>
        OnItemClick.HasDelegate ? EventCallback.Factory.Create(this, () => OnItemClick.InvokeAsync(item)) : default;

    // ---------- Text ----------

    internal string TitleOf(TItem item) => Title is null ? item?.ToString() ?? "" : Title(item);

    internal string FormatTime(int minutes) =>
        new DateTime(2000, 1, 1).AddMinutes(minutes).ToString("t", CurrentCulture);

    internal string FormatRange(int start, int end) => $"{FormatTime(start)}–{FormatTime(end)}";

    internal string FormatHours(int minutes) => string.Format(CurrentCulture, Localize(HoursFormat, "Calendar.HoursFormat"), minutes / 60.0);

    internal string DayName(DateOnly date)
    {
        var name = CurrentCulture.DateTimeFormat.GetDayName(date.DayOfWeek);
        return name.Length == 0 ? name : char.ToUpper(name[0], CurrentCulture) + name[1..];
    }

    /// <summary>The accessible name of an event: title, time, details and (optionally) the day.</summary>
    internal string EventLabel(StilCalendarOccurrence<TItem> occurrence, bool withDay)
    {
        var parts = new List<string> { TitleOf(occurrence.Item) };
        if (withDay)
        {
            parts.Add($"{DayName(occurrence.Date)} {occurrence.Date.ToString("M", CurrentCulture)}");
        }

        parts.Add(FormatRange(occurrence.StartMinute, occurrence.EndMinute));
        if (Details?.Invoke(occurrence.Item) is { Length: > 0 } details)
        {
            parts.Add(details);
        }

        return string.Join(", ", parts);
    }

    // ---------- CSS ----------

    internal static string Css(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>stil-event with the item's variant and draft classes, plus the given ones.</summary>
    internal string EventClasses(TItem item, params string?[] extra)
    {
        var variant = Variant?.Invoke(item);
        var classes = new List<string?> { "stil-event" };
        if (variant is { } value)
        {
            classes.Add(StilCssClasses.Variant("stil-event", value));
        }
        else if (Color is null || string.IsNullOrWhiteSpace(Color(item)))
        {
            classes.Add("stil-event-primary");
        }

        if (Draft?.Invoke(item) == true)
        {
            classes.Add("stil-event-draft");
        }

        classes.AddRange(extra);
        return string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
    }

    /// <summary>The variant class of a dot, or null when the item has its own color.</summary>
    internal string? DotVariantClass(TItem item) =>
        Variant?.Invoke(item) is { } variant ? StilCssClasses.Variant("stil-event", variant)
        : ColorStyle(item) is null ? "stil-event-primary" : null;

    /// <summary>The item's own color as a --stil-event-color declaration, when it has no variant.</summary>
    internal string? ColorStyle(TItem item) =>
        Variant?.Invoke(item) is null && Color?.Invoke(item) is { } color && !string.IsNullOrWhiteSpace(color)
            ? $"--stil-event-color: {color}"
            : null;

    internal static string? JoinStyles(params string?[] styles)
    {
        var present = styles.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        return present.Count == 0 ? null : string.Join("; ", present);
    }
}
