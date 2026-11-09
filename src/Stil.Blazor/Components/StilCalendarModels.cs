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
public sealed record StilCalendarItemChange<TItem>(TItem Item, DateTime Start, DateTime End)
{
    /// <summary>
    /// For one occurrence of a recurring item, the start it had before the change; null for a
    /// one-off item. Start and End are then that occurrence's new times: add the occurrence's day
    /// to the rule's Exceptions and add a one-off item at the new time.
    /// </summary>
    public DateTime? Occurrence { get; init; }

    /// <summary>In StilResourcePlanner, the key of the resource the item was dropped on.</summary>
    public string? Resource { get; init; }
}

/// <summary>A row of StilResourcePlanner: a room, a person or anything else that is booked.</summary>
/// <param name="Key">The value of the planner's Resource selector for the items in this row.</param>
/// <param name="Name">The row's name.</param>
/// <param name="Details">A second line, for example the number of seats.</param>
public sealed record StilResource(string Key, string Name, string? Details = null);

/// <summary>A span of time dragged on a resource's row in StilResourcePlanner.</summary>
public readonly record struct StilResourceSlot(string Resource, DateTime Start, DateTime End);

/// <summary>
/// The part of an item that falls on one day. StartMinute and EndMinute are minutes from that
/// day's midnight (0–1440); an item over several days has one occurrence per day.
/// </summary>
internal sealed record StilCalendarOccurrence<TItem>(TItem Item, int Index, DateOnly Date, int StartMinute, int EndMinute)
{
    /// <summary>The start of this occurrence of a recurring item; null for a one-off item.</summary>
    public DateTime? OccurrenceStart { get; init; }
}

/// <summary>An occurrence placed in a day column: its shown start and end, and its overlap column.</summary>
internal sealed class StilCalendarPlacement<TItem>(StilCalendarOccurrence<TItem> occurrence, int start, int end)
{
    public StilCalendarOccurrence<TItem> Occurrence { get; } = occurrence;

    public int Start { get; } = start;

    public int End { get; } = end;

    public int Column { get; set; }

    public int Columns { get; set; } = 1;
}
