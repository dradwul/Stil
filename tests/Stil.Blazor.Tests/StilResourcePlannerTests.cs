using System.Globalization;
using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilResourcePlannerTests
{
    private static readonly DateTime Now = new(2026, 10, 14, 10, 20, 0);

    private sealed record Booking(string Title, string Room, DateTime Start, DateTime End, StilRecurrence? Rule = null);

    private static readonly StilResource[] Rooms =
    [
        new("fjord", "Room Fjord", "8 seats"),
        new("glacier", "Room Glacier", "4 seats"),
    ];

    private static DateTime At(int hour, int minute = 0) => new(2026, 10, 14, hour, minute, 0);

    private static IRenderedComponent<StilResourcePlanner<Booking>> Render(BunitContext context, List<Booking> bookings,
        Action<ComponentParameterCollectionBuilder<StilResourcePlanner<Booking>>>? more = null) =>
        context.Render<StilResourcePlanner<Booking>>(parameters =>
        {
            parameters
                .Add(c => c.Items, bookings)
                .Add(c => c.Resources, Rooms)
                .Add(c => c.Resource, b => b.Room)
                .Add(c => c.Start, b => b.Start)
                .Add(c => c.End, b => b.End)
                .Add(c => c.Title, b => b.Title)
                .Add(c => c.Recurrence, b => b.Rule)
                .Add(c => c.Now, Now)
                .Add(c => c.Culture, CultureInfo.GetCultureInfo("en-GB"));
            more?.Invoke(parameters);
        });

    private static List<Booking> Bookings() =>
    [
        new("Budget", "fjord", At(9), At(10, 30)),
        new("Interview", "fjord", At(10), At(11)),
        new("Workshop", "glacier", At(13), At(16)),
        new("Standup", "glacier", new DateTime(2026, 10, 12, 8, 45, 0), new DateTime(2026, 10, 12, 9, 0, 0), StilRecurrence.Daily()),
    ];

    [Fact]
    public void RowsShowTheirBookingsAcrossTheDay()
    {
        using var context = new BunitContext();

        var cut = Render(context, Bookings());

        Assert.Equal("Wednesday, 14 October 2026", cut.Find(".stil-calendar-title").TextContent.Trim());
        Assert.Equal(["Room Fjord", "Room Glacier"], cut.FindAll(".stil-resource-name > span").Select(n => n.TextContent));
        Assert.Equal(["8 seats", "4 seats"], cut.FindAll(".stil-resource-name > small").Select(n => n.TextContent));
        var tracks = cut.FindAll(".stil-resource-track");
        Assert.Equal(["Budget", "Interview"], tracks[0].QuerySelectorAll(".stil-event-title").Select(t => t.TextContent));
        Assert.Equal(["Standup", "Workshop"], tracks[1].QuerySelectorAll(".stil-event-title").Select(t => t.TextContent));
        // Budget and Interview overlap and share the row.
        Assert.Contains("--stil-start: 540; --stil-length: 90; --stil-col: 0; --stil-cols: 2", tracks[0].QuerySelector(".stil-event")!.GetAttribute("style"));
        Assert.Equal("--stil-start: 620", cut.Find(".stil-resource-now").GetAttribute("style"));
        Assert.Equal("--stil-resource-first-hour: 7; --stil-resource-hours: 12", cut.Find(".stil-resource").GetAttribute("style"));
    }

    [Fact]
    public void DraggingMovesABookingToAnotherRoomAndTime()
    {
        using var context = new BunitContext();
        StilCalendarItemChange<Booking>? change = null;

        var cut = Render(context, Bookings(), p => p.Add(c => c.Editable, true).Add(c => c.OnItemChange, c => change = c));
        Assert.NotNull(cut.Find(".stil-resource").GetAttribute("data-stil-editable"));

        // Interview (item 1 in row 0) dropped on Room Glacier at 11:00.
        cut.Find(".stil-resource").TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "move", Item = "1-0", Day = 1, Start = 660, End = 720 });

        Assert.Equal(("Interview", "glacier", At(11), At(12)), (change?.Item.Title, change?.Resource, change?.Start, change?.End));

        // Today's standup (one occurrence) made longer.
        cut.Find(".stil-resource").TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "resize", Item = "3-1", Day = 1, Start = 525, End = 570 });

        Assert.Equal(At(8, 45), change?.Occurrence);
        Assert.Equal(At(9, 30), change?.End);
    }

    [Fact]
    public void DraggingOnARowSelectsASlot()
    {
        using var context = new BunitContext();
        StilResourceSlot? slot = null;

        var cut = Render(context, Bookings(), p => p.Add(c => c.Editable, true).Add(c => c.OnSlotSelect, s => slot = s));
        cut.Find(".stil-resource").TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "create", Day = 0, Start = 840, End = 900 });

        Assert.Equal(new StilResourceSlot("fjord", At(14), At(15)), slot);
    }

    [Fact]
    public void NavigationGoesToTheNextDay()
    {
        using var context = new BunitContext();
        DateOnly? date = null;

        var cut = Render(context, Bookings(), p => p.Add(c => c.DateChanged, d => date = d));
        cut.Find("button[aria-label='Next day']").Click();

        Assert.Equal(new DateOnly(2026, 10, 15), date);
        Assert.Equal(["Standup"], cut.FindAll(".stil-event-title").Select(t => t.TextContent));
        Assert.Empty(cut.FindAll(".stil-resource-now"));
    }
}
