using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilAgendaTests
{
    private static IRenderedComponent<StilAgenda<Meeting>> RenderAgenda(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<StilAgenda<Meeting>>>? configure = null,
        List<Meeting>? items = null) =>
        context.Render<StilAgenda<Meeting>>(parameters =>
        {
            Meeting.Configure(parameters, items ?? Meeting.Sample());
            configure?.Invoke(parameters);
        });

    [Fact]
    public void GroupsDaysUnderHeadings()
    {
        using var context = new BunitContext();

        var cut = RenderAgenda(context, parameters => parameters.Add(c => c.Days, 3).AddUnmatched("class", "custom"));

        Assert.Equal(["stil-agenda", "custom"], cut.Find("div").ClassList);
        var days = cut.FindAll("section.stil-agenda-day");
        Assert.Equal(3, days.Count);
        var titles = cut.FindAll("h2.stil-agenda-day-title");
        Assert.Equal(["Today", "Tomorrow", "Friday"], titles.Select(t => t.FirstChild!.TextContent.Trim()));
        Assert.Equal("14 October · 3.5 h booked", titles[0].QuerySelector("small")!.TextContent);
        Assert.Equal("15 October", titles[1].QuerySelector("small")!.TextContent);
        Assert.Equal(titles[0].Id, days[0].GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void ListsEventsAndFreeTimeInOrder()
    {
        using var context = new BunitContext();

        var cut = RenderAgenda(context, parameters => parameters.Add(c => c.Days, 1));

        var items = cut.FindAll(".stil-agenda-item");
        Assert.Equal(
            ["08:30–09:00", "10:00–11:30", "11:00–12:00", "12:00–13:00", "13:00–14:00", "14:00–17:00"],
            items.Select(i => i.QuerySelector(".stil-agenda-time")!.TextContent));
        Assert.Contains("stil-agenda-free", items[3].ClassList);
        Assert.Equal("Free · 1 h", items[3].QuerySelector(".stil-agenda-free-line")!.TextContent);
        Assert.Equal("Room Fjord", items[1].QuerySelector(".stil-event-meta")!.TextContent);
        Assert.Equal("DIV", items[1].QuerySelector(".stil-event")!.TagName);
    }

    [Fact]
    public void EventsAreButtonsWithOnItemClick()
    {
        using var context = new BunitContext();
        Meeting? clicked = null;

        var cut = RenderAgenda(context, parameters => parameters
            .Add(c => c.Days, 1)
            .Add(c => c.OnItemClick, m => clicked = m));

        var planning = cut.FindAll("button.stil-event")[1];
        Assert.Equal("Planning, 10:00–11:30, Room Fjord", planning.GetAttribute("aria-label"));
        Assert.Null(planning.GetAttribute("tabindex"));

        planning.Click();

        Assert.Equal("Planning", clicked?.Title);
    }

    [Fact]
    public void EmptyDaysShowTextOrAreSkipped()
    {
        using var context = new BunitContext();

        // Saturday and Sunday: no events and no working day.
        var cut = RenderAgenda(context, parameters => parameters
            .Add(c => c.Date, new DateOnly(2026, 10, 17))
            .Add(c => c.Days, 2)
            .Add(c => c.HeadingLevel, 3));

        Assert.Equal(2, cut.FindAll("h3").Count);
        Assert.Equal(["Nothing planned.", "Nothing planned."], cut.FindAll("p").Select(p => p.TextContent));

        cut.Render(parameters => parameters.Add(c => c.ShowEmptyDays, false));
        Assert.Empty(cut.FindAll("section"));
    }

    [Fact]
    public void FreeTimeFollowsWorkingDayAndSettings()
    {
        using var context = new BunitContext();

        var cut = RenderAgenda(context, parameters => parameters
            .Add(c => c.Date, new DateOnly(2026, 10, 15))
            .Add(c => c.Days, 1)
            .Add(c => c.WorkdayStart, new TimeOnly(9, 0))
            .Add(c => c.WorkdayEnd, new TimeOnly(15, 30)));

        Assert.Equal("09:00–15:30", cut.Find(".stil-agenda-time").TextContent);
        Assert.Equal("Free · 6.5 h", cut.Find(".stil-agenda-free-line").TextContent);

        cut.Render(parameters => parameters.Add(c => c.ShowFreeTime, false));
        Assert.Equal("Nothing planned.", cut.Find("p").TextContent);
    }

    [Fact]
    public void LoadFreeTimeAndLayoutAreWorkedOutFromTheItems()
    {
        using var context = new BunitContext();

        var agenda = RenderAgenda(context).Instance;

        Assert.Equal(210, agenda.BookedMinutes(Meeting.Today));
        Assert.Equal(210 / 540.0, agenda.Load(Meeting.Today), 3);
        Assert.Equal([(720, 780), (840, 1020)], agenda.FreeBlocks(Meeting.Today));
        Assert.Empty(agenda.FreeBlocks(Meeting.Today.AddDays(-1)));

        var placements = agenda.OccurrencesOn(Meeting.Today)
            .Select(o => new StilCalendarPlacement<Meeting>(o, o.StartMinute, o.EndMinute))
            .ToList();
        StilCalendarBase<Meeting>.Layout(placements);
        Assert.Equal([(0, 1), (0, 2), (1, 2), (0, 1)], placements.Select(p => (p.Column, p.Columns)));
    }
}
