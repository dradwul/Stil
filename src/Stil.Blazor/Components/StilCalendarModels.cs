namespace Stil.Blazor.Components;

/// <summary>How many days StilWeekPlanner shows.</summary>
public enum StilWeekView
{
    /// <summary>Seven days, from the culture's first day of the week.</summary>
    Week,

    /// <summary>Monday to Friday.</summary>
    WorkWeek,

    /// <summary>One day.</summary>
    Day
}

/// <summary>A span of time: a free block that was clicked, or a range dragged in the week grid.</summary>
public readonly record struct StilTimeSlot(DateTime Start, DateTime End)
{
    public TimeSpan Duration => End - Start;
}

/// <summary>An item that was moved or resized in StilWeekPlanner, with its new start and end.</summary>
public sealed record StilCalendarItemChange<TItem>(TItem Item, DateTime Start, DateTime End);

/// <summary>
/// The part of an item that falls on one day. StartMinute and EndMinute are minutes from that
/// day's midnight (0–1440); an item over several days has one occurrence per day.
/// </summary>
internal sealed record StilCalendarOccurrence<TItem>(TItem Item, int Index, DateOnly Date, int StartMinute, int EndMinute);

/// <summary>An occurrence placed in a day column: its shown start and end, and its overlap column.</summary>
internal sealed class StilCalendarPlacement<TItem>(StilCalendarOccurrence<TItem> occurrence, int start, int end)
{
    public StilCalendarOccurrence<TItem> Occurrence { get; } = occurrence;

    public int Start { get; } = start;

    public int End { get; } = end;

    public int Column { get; set; }

    public int Columns { get; set; } = 1;
}
