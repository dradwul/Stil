using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilWeekPlannerTests
{
    private static IRenderedComponent<StilWeekPlanner<Meeting>> RenderWeek(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<StilWeekPlanner<Meeting>>>? configure = null,
        List<Meeting>? items = null) =>
        context.Render<StilWeekPlanner<Meeting>>(parameters =>
        {
            Meeting.Configure(parameters, items ?? Meeting.Sample());
            configure?.Invoke(parameters);
        });

    private static AngleSharp.Dom.IElement Root(IRenderedComponent<StilWeekPlanner<Meeting>> cut) => cut.Find("div.stil-week");

    private static AngleSharp.Dom.IElement Event(IRenderedComponent<StilWeekPlanner<Meeting>> cut, string title) =>
        cut.FindAll(".stil-week-col > .stil-event").Single(e => e.QuerySelector(".stil-event-title")!.TextContent == title);

    [Fact]
    public void RendersWeekGridWithSettingsAndTitle()
    {
        using var context = new BunitContext();

        var cut = RenderWeek(context, parameters => parameters.AddUnmatched("class", "custom"));

        var root = Root(cut);
        Assert.Equal(["stil-week", "stil-week-auto", "custom"], root.ClassList);
        Assert.Equal("--stil-week-days: 7; --stil-week-first-hour: 7; --stil-week-hours: 12", root.GetAttribute("style"));
        Assert.Null(root.GetAttribute("data-stil-editable"));
        Assert.Equal("Week 42 12 October – 18 October 2026", cut.Find(".stil-calendar-title").TextContent.Trim().Replace("  ", " "));

        var heads = cut.FindAll(".stil-week-day-head");
        Assert.Equal(7, heads.Count);
        Assert.Equal("Mon", heads[0].QuerySelector(".stil-week-day-name")!.TextContent);
        Assert.Equal("date", heads[2].GetAttribute("aria-current"));
        Assert.Equal("--stil-day-load: 0.39", heads[2].QuerySelector(".stil-week-load-bar")!.GetAttribute("style"));
        Assert.Equal("3.5 h", heads[2].QuerySelector(".stil-week-load > span:last-child")!.TextContent);
        Assert.Equal("Free", heads[3].QuerySelector(".stil-week-load > span:last-child")!.TextContent);

        Assert.Equal(12, cut.FindAll(".stil-week-time").Count);
        Assert.Equal("07:00", cut.Find(".stil-week-time").TextContent);
        var columns = cut.FindAll(".stil-week-col");
        Assert.Equal(7, columns.Count);
        Assert.Contains("stil-week-col-muted", columns[5].ClassList);
        Assert.DoesNotContain("stil-week-col-muted", columns[4].ClassList);
    }

    [Fact]
    public void ViewsShowWorkWeekOrOneDay()
    {
        using var context = new BunitContext();

        var cut = RenderWeek(context, parameters => parameters.Add(c => c.View, StilWeekView.WorkWeek));
        Assert.Equal(5, cut.FindAll(".stil-week-col").Count);
        Assert.Equal("12", cut.Find(".stil-week-day-head .stil-calendar-date").TextContent);

        cut.Render(parameters => parameters.Add(c => c.View, StilWeekView.Day));
        Assert.Single(cut.FindAll(".stil-week-col"));
        Assert.Contains("14 October 2026", cut.Find(".stil-calendar-title").TextContent);
        // One day fits any width, so it never turns into an agenda.
        Assert.DoesNotContain("stil-week-auto", Root(cut).ClassList);
        Assert.Empty(cut.FindAll(".stil-agenda"));
    }

    [Fact]
    public void PlacesEventsAndCascadesOverlaps()
    {
        using var context = new BunitContext();

        var cut = RenderWeek(context);

        Assert.Equal("--stil-event-start: 510; --stil-event-length: 30; --stil-event-col: 0; --stil-event-cols: 1", Event(cut, "Standup").GetAttribute("style"));
        Assert.Equal("--stil-event-start: 600; --stil-event-length: 90; --stil-event-col: 0; --stil-event-cols: 2", Event(cut, "Planning").GetAttribute("style"));
        Assert.Equal("--stil-event-start: 660; --stil-event-length: 60; --stil-event-col: 1; --stil-event-cols: 2", Event(cut, "Review").GetAttribute("style"));
        Assert.Equal("--stil-event-color: #9c36b5; --stil-event-start: 780; --stil-event-length: 60; --stil-event-col: 0; --stil-event-cols: 1",
            Event(cut, "Lunch").GetAttribute("style"));

        // A short event only has room for its title.
        Assert.Null(Event(cut, "Standup").QuerySelector(".stil-event-meta"));
        Assert.Equal("10:00–11:30 · Room Fjord", Event(cut, "Planning").QuerySelector(".stil-event-meta")!.TextContent);
        Assert.Equal("--stil-event-start: 620", cut.Find(".stil-week-now").GetAttribute("style"));
    }

    [Fact]
    public void EventsOutsideTheGridAreClippedOrHidden()
    {
        using var context = new BunitContext();
        List<Meeting> items =
        [
            new("Early", new(2026, 10, 15, 5, 0, 0), new(2026, 10, 15, 6, 0, 0)),
            new("Early start", new(2026, 10, 15, 6, 0, 0), new(2026, 10, 15, 8, 0, 0)),
        ];

        var cut = RenderWeek(context, items: items);

        Assert.Single(cut.FindAll(".stil-week-col > .stil-event"));
        Assert.StartsWith("--stil-event-start: 420; --stil-event-length: 60;", Event(cut, "Early start").GetAttribute("style"));
    }

    [Fact]
    public void FreeBlocksAreButtonsWithOnSlotSelect()
    {
        using var context = new BunitContext();

        var cut = RenderWeek(context);
        Assert.Empty(cut.FindAll("button.stil-week-free"));
        Assert.Equal("Free · 1 h", cut.Find("div.stil-week-free").TextContent);

        StilTimeSlot? slot = null;
        cut.Render(parameters => parameters.Add(c => c.OnSlotSelect, s => slot = s));

        // Past days have no free time; today's starts after now.
        var columns = cut.FindAll(".stil-week-col");
        Assert.Empty(columns[1].QuerySelectorAll(".stil-week-free"));
        var free = columns[2].QuerySelectorAll("button.stil-week-free");
        Assert.Equal(2, free.Length);
        Assert.Equal("--stil-event-start: 720; --stil-event-length: 60", free[0].GetAttribute("style"));
        Assert.Equal("Free Wednesday 12:00–13:00, create an event", free[0].GetAttribute("aria-label"));
        Assert.Equal("--stil-event-start: 480; --stil-event-length: 540", columns[3].QuerySelector(".stil-week-free")!.GetAttribute("style"));
        Assert.Empty(columns[5].QuerySelectorAll(".stil-week-free"));

        free[0].Click();

        Assert.Equal(new StilTimeSlot(new(2026, 10, 14, 12, 0, 0), new(2026, 10, 14, 13, 0, 0)), slot);
    }

    [Fact]
    public void EditableRendersHandlesAndItemKeys()
    {
        using var context = new BunitContext();

        var cut = RenderWeek(context, parameters => parameters.Add(c => c.Editable, true));

        Assert.Equal("", Root(cut).GetAttribute("data-stil-editable"));
        Assert.Equal("1-2", Event(cut, "Planning").GetAttribute("data-stil-item"));
        Assert.NotNull(Event(cut, "Planning").QuerySelector(".stil-event-resize[aria-hidden=true]"));
    }

    [Fact]
    public void DraggingAnEventRaisesOnItemChange()
    {
        using var context = new BunitContext();
        StilCalendarItemChange<Meeting>? change = null;

        var cut = RenderWeek(context, parameters => parameters
            .Add(c => c.Editable, true)
            .Add(c => c.OnItemChange, c => change = c));

        // Planning (10:00–11:30 on Wednesday) dropped on Thursday at 11:00.
        Root(cut).TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "move", Item = "1-2", Day = 3, Start = 660, End = 750 });

        Assert.Equal("Planning", change?.Item.Title);
        Assert.Equal(new DateTime(2026, 10, 15, 11, 0, 0), change?.Start);
        Assert.Equal(new DateTime(2026, 10, 15, 12, 30, 0), change?.End);

        Root(cut).TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "resize", Item = "1-2", Day = 2, Start = 600, End = 720 });

        Assert.Equal(new DateTime(2026, 10, 14, 10, 0, 0), change?.Start);
        Assert.Equal(new DateTime(2026, 10, 14, 12, 0, 0), change?.End);
    }

    [Fact]
    public void DraggingOnEmptyTimeRaisesOnSlotSelect()
    {
        using var context = new BunitContext();
        StilTimeSlot? slot = null;
        var changed = false;

        var cut = RenderWeek(context, parameters => parameters
            .Add(c => c.Editable, true)
            .Add(c => c.OnItemChange, _ => changed = true)
            .Add(c => c.OnSlotSelect, s => slot = s));

        Root(cut).TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "create", Day = 3, Start = 540, End = 600 });
        Root(cut).TriggerEvent("onstilweekedit", new StilWeekEditEventArgs { Action = "move", Item = "unknown", Day = 3, Start = 540, End = 600 });

        Assert.Equal(new StilTimeSlot(new(2026, 10, 15, 9, 0, 0), new(2026, 10, 15, 10, 0, 0)), slot);
        Assert.False(changed);
    }

    [Fact]
    public void NavigationMovesByWeekOrDay()
    {
        using var context = new BunitContext();
        DateOnly? date = null;

        var cut = RenderWeek(context, parameters => parameters.Add(c => c.DateChanged, d => date = d));

        cut.Find("button[aria-label='Next week']").Click();
        Assert.Equal(new DateOnly(2026, 10, 21), date);
        Assert.StartsWith("Week 43", cut.Find(".stil-calendar-title").TextContent.Trim());

        cut.Render(parameters => parameters.Add(c => c.View, StilWeekView.Day));
        cut.Find("button[aria-label='Previous day']").Click();
        Assert.Equal(new DateOnly(2026, 10, 20), date);
    }

    [Fact]
    public void NarrowContainerAgendaShowsTheSameDays()
    {
        using var context = new BunitContext();

        var cut = RenderWeek(context);

        var agenda = cut.Find(".stil-week > .stil-agenda");
        Assert.Equal(7, agenda.QuerySelectorAll(".stil-agenda-day").Length);
        Assert.Equal("h3", agenda.QuerySelector(".stil-agenda-day-title")!.TagName.ToLowerInvariant());

        cut.Render(parameters => parameters.Add(c => c.AgendaWhenNarrow, false));
        Assert.Empty(cut.FindAll(".stil-agenda"));
    }
}
