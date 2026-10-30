using System.Globalization;
using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests.TestComponents;

/// <summary>Calendar test data: a meeting with a variant or its own color.</summary>
public sealed record Meeting(string Title, DateTime Start, DateTime End, StilVariant? Variant = null, string? Color = null, string? Place = null)
{
    /// <summary>Wednesday 14 October 2026, 10:20. Week 42 runs from Monday 12 to Sunday 18 October.</summary>
    public static readonly DateTime Now = new(2026, 10, 14, 10, 20, 0);

    public static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-GB");

    private static DateTime At(int day, int hour, int minute = 0) => new(2026, 10, day, hour, minute, 0);

    /// <summary>
    /// Four meetings today: 08:30–09:00, 10:00–11:30 and 11:00–12:00 (overlapping) and 13:00–14:00
    /// (own color). 3.5 of the 9 working hours are booked; 12:00–13:00 and 14:00–17:00 are free.
    /// </summary>
    public static List<Meeting> Sample() =>
    [
        new("Standup", At(14, 8, 30), At(14, 9), StilVariant.Success),
        new("Planning", At(14, 10), At(14, 11, 30), StilVariant.Primary, Place: "Room Fjord"),
        new("Review", At(14, 11), At(14, 12), StilVariant.Info),
        new("Lunch", At(14, 13), At(14, 14), Color: "#9c36b5"),
    ];

    /// <summary>Sets the selectors, the fixed time and the culture shared by the calendar tests.</summary>
    public static void Configure<TComponent>(ComponentParameterCollectionBuilder<TComponent> parameters, IEnumerable<Meeting> items, CultureInfo? culture = null)
        where TComponent : StilCalendarBase<Meeting>
    {
        parameters
            .Add(c => c.Items, items)
            .Add(c => c.Start, m => m.Start)
            .Add(c => c.End, m => m.End)
            .Add(c => c.Title, m => m.Title)
            .Add(c => c.Variant, m => m.Variant)
            .Add(c => c.Color, m => m.Color)
            .Add(c => c.Details, m => m.Place)
            .Add(c => c.Now, Now)
            .Add(c => c.Culture, culture ?? Culture);
    }
}
