using System.Globalization;
using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilRecurrenceTests
{
    private static readonly DateTime Now = new(2026, 10, 14, 10, 20, 0);
    private static readonly DateTime FirstStandup = new(2026, 10, 5, 9, 0, 0);

    private sealed record Series(string Title, DateTime Start, DateTime End, StilRecurrence? Rule = null);

    private static void Configure<TComponent>(ComponentParameterCollectionBuilder<TComponent> parameters, IEnumerable<Series> items)
        where TComponent : StilCalendarBase<Series> =>
        parameters
            .Add(c => c.Items, items)
            .Add(c => c.Start, s => s.Start)
            .Add(c => c.End, s => s.End)
            .Add(c => c.Title, s => s.Title)
            .Add(c => c.Recurrence, s => s.Rule)
            .Add(c => c.Now, Now)
            .Add(c => c.Culture, CultureInfo.GetCultureInfo("en-GB"));

    [Fact]
    public void WeeklyOnDaysWithCountAndExceptions()
    {
        // Monday 5 October; Mondays and Wednesdays, six times, but not Wednesday 14 October.
        var rule = StilRecurrence.Weekly(DayOfWeek.Monday, DayOfWeek.Wednesday) with { Count = 6 };

        var dates = rule.Except(new DateOnly(2026, 10, 14)).Occurrences(FirstStandup).Select(d => d.Day);

        Assert.Equal([5, 7, 12, 19, 21], dates);
    }

    [Fact]
    public void DailyMonthlyAndUntil()
    {
        Assert.Equal([5, 7, 9], StilRecurrence.Daily(2).Occurrences(FirstStandup).Take(3).Select(d => d.Day));
        Assert.Equal([new DateTime(2026, 1, 31), new DateTime(2026, 3, 31), new DateTime(2026, 5, 31)],
            StilRecurrence.Monthly().Occurrences(new DateTime(2026, 1, 31)).Take(3));
        Assert.Equal(3, (StilRecurrence.Weekly() with { Until = new DateOnly(2026, 10, 19) }).Occurrences(FirstStandup).Count());
        Assert.Equal(new DateTime(2026, 10, 21, 9, 0, 0), StilRecurrence.Weekly(DayOfWeek.Wednesday).OccurrenceOn(FirstStandup, new DateOnly(2026, 10, 21)));
        Assert.Null(StilRecurrence.Weekly().OccurrenceOn(FirstStandup, new DateOnly(2026, 10, 13)));
    }

    [Theory]
    [InlineData("FREQ=WEEKLY;BYDAY=MO,WE;COUNT=10")]
    [InlineData("FREQ=MONTHLY;INTERVAL=2;UNTIL=20271231")]
    [InlineData("FREQ=DAILY")]
    public void RRuleRoundTrips(string rule)
    {
        Assert.Equal(rule, StilRecurrence.Parse("RRULE:" + rule).ToString());
    }

    [Fact]
    public void WeekPlannerShowsEachOccurrenceAndMovesOneOnItsOwn()
    {
        using var context = new BunitContext();
        StilCalendarItemChange<Series>? change = null;
        var items = new List<Series> { new("Standup", FirstStandup, FirstStandup.AddMinutes(15), StilRecurrence.Daily() with { Until = new DateOnly(2026, 10, 16) }) };

        var cut = context.Render<StilWeekPlanner<Series>>(parameters =>
        {
            Configure(parameters, items);
            parameters.Add(c => c.Editable, true).Add(c => c.OnItemChange, c => change = c);
        });

        // Week 42: Monday 12 to Friday 16 October have a standup; the weekend does not.
        Assert.Equal(5, cut.FindAll(".stil-week-col > .stil-event").Count);

        // Wednesday's standup (day 2) dragged to 10:00.
        cut.Find("div.stil-week").TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "move", Item = "0-2", Day = 2, Start = 600, End = 615 });

        Assert.Equal(new DateTime(2026, 10, 14, 9, 0, 0), change?.Occurrence);
        Assert.Equal(new DateTime(2026, 10, 14, 10, 0, 0), change?.Start);
        Assert.Equal(new DateTime(2026, 10, 14, 10, 15, 0), change?.End);
    }

    [Fact]
    public void AgendaAndCalendarShowTheSeriesWithOneOccurrenceMoved()
    {
        using var context = new BunitContext();
        var weekly = StilRecurrence.Weekly(DayOfWeek.Wednesday).Except(new DateOnly(2026, 10, 21));
        var items = new List<Series>
        {
            new("Planning", new DateTime(2026, 10, 7, 10, 0, 0), new DateTime(2026, 10, 7, 11, 0, 0), weekly),
            new("Planning", new DateTime(2026, 10, 22, 13, 0, 0), new DateTime(2026, 10, 22, 14, 0, 0)),
        };

        var agenda = context.Render<StilAgenda<Series>>(parameters =>
        {
            Configure(parameters, items);
            parameters.Add(c => c.Days, 15).Add(c => c.ShowEmptyDays, false);
        });

        // Wednesday 21 October is moved to Thursday 22.
        Assert.Equal(["Today 14 October", "Thursday 22 October", "Wednesday 28 October"],
            agenda.FindAll(".stil-agenda-day-title").Select(t => t.TextContent.Split('·')[0].Trim()));

        var month = context.Render<StilCalendar<Series>>(parameters => Configure(parameters, items));

        // 7, 14, 22 (moved from 21) and 28 October; the month view ends on Sunday 1 November.
        Assert.Equal(["2026-10-07", "2026-10-14", "2026-10-22", "2026-10-28"],
            month.FindAll("td[data-date]").Where(td => td.TextContent.Contains("Planning")).Select(td => td.GetAttribute("data-date")));
    }
}
