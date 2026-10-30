using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilCalendarTests
{
    private static IRenderedComponent<StilCalendar<Meeting>> RenderMonth(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<StilCalendar<Meeting>>>? configure = null,
        List<Meeting>? items = null) =>
        context.Render<StilCalendar<Meeting>>(parameters =>
        {
            Meeting.Configure(parameters, items ?? Meeting.Sample());
            configure?.Invoke(parameters);
        });

    private static AngleSharp.Dom.IElement Cell(IRenderedComponent<StilCalendar<Meeting>> cut, string date) =>
        cut.Find($"td[data-date='{date}']");

    [Fact]
    public void RendersMonthAsGridWithWeekNumbersAndMergedClass()
    {
        using var context = new BunitContext();

        var cut = RenderMonth(context, parameters => parameters.AddUnmatched("class", "custom"));

        Assert.Equal(["stil-calendar", "custom"], cut.Find("div").ClassList);
        var grid = cut.Find("table");
        Assert.Equal("grid", grid.GetAttribute("role"));
        var title = cut.Find("h2.stil-calendar-title");
        Assert.Equal("October 2026", title.TextContent.Trim());
        Assert.Equal(title.Id, grid.GetAttribute("aria-labelledby"));

        // October 2026 starts on a Thursday: with Monday first, five weeks from 28 September.
        var cells = cut.FindAll("td[role=gridcell]");
        Assert.Equal(35, cells.Count);
        Assert.Equal("2026-09-28", cells[0].GetAttribute("data-date"));
        Assert.Contains("stil-calendar-outside", cells[0].ClassList);
        Assert.Equal("Mon", cut.FindAll("thead th abbr")[1].TextContent);
        Assert.Equal(["40", "41", "42", "43", "44"], cut.FindAll("td.stil-calendar-week").Select(c => c.TextContent.Replace("Week", "").Trim()));
    }

    [Fact]
    public void TodayIsCurrentTabStopAndTintedByLoad()
    {
        using var context = new BunitContext();

        var cut = RenderMonth(context);

        var today = Cell(cut, "2026-10-14");
        Assert.Equal("date", today.GetAttribute("aria-current"));
        Assert.Equal("0", today.GetAttribute("tabindex"));
        Assert.Single(cut.FindAll("td[role=gridcell][tabindex='0']"));
        Assert.Equal("--stil-load: 0.39", today.GetAttribute("style"));
        Assert.Contains("14 October 2026, 4 events, 39% booked", today.GetAttribute("aria-label"));

        var free = Cell(cut, "2026-10-15");
        Assert.Null(free.GetAttribute("style"));
        Assert.EndsWith(", free", free.GetAttribute("aria-label"));
    }

    [Fact]
    public void ShowsChipsMoreLinkAndDots()
    {
        using var context = new BunitContext();

        var cut = RenderMonth(context);

        var today = Cell(cut, "2026-10-14");
        var chips = today.QuerySelectorAll(".stil-calendar-events .stil-event");
        Assert.Equal(2, chips.Length);
        Assert.Equal("DIV", chips[0].TagName);
        Assert.Contains("stil-event-success", chips[0].ClassList);
        Assert.Equal("08:30Standup", chips[0].TextContent);
        Assert.Equal("+2 more", today.QuerySelector(".stil-calendar-more")!.TextContent.Trim());

        var dots = today.QuerySelectorAll(".stil-calendar-dot");
        Assert.Equal(4, dots.Length);
        Assert.Contains("stil-event-info", dots[2].ClassList);
        Assert.Equal(["stil-calendar-dot"], dots[3].ClassList);
        Assert.Equal("--stil-event-color: #9c36b5", dots[3].GetAttribute("style"));
    }

    [Fact]
    public void EventsAreButtonsWithOnItemClick()
    {
        using var context = new BunitContext();
        Meeting? clicked = null;
        DateOnly? selected = null;

        var cut = RenderMonth(context, parameters => parameters
            .Add(c => c.MaxEventsPerDay, 5)
            .Add(c => c.OnItemClick, m => clicked = m)
            .Add(c => c.SelectedDateChanged, d => selected = d));

        var buttons = Cell(cut, "2026-10-14").QuerySelectorAll("button.stil-event");
        Assert.Equal(4, buttons.Length);
        Assert.Equal("-1", buttons[1].GetAttribute("tabindex"));
        Assert.Equal("Planning, Wednesday 14 October, 10:00–11:30, Room Fjord", buttons[1].GetAttribute("aria-label"));

        buttons[1].Click();

        Assert.Equal("Planning", clicked?.Title);
        Assert.Null(selected);
    }

    [Fact]
    public void ClickSelectsDayAndSecondClickOpensIt()
    {
        using var context = new BunitContext();
        DateOnly? selected = null;
        DateOnly? opened = null;

        var cut = RenderMonth(context, parameters => parameters
            .Add(c => c.SelectedDateChanged, d => selected = d)
            .Add(c => c.OnDateOpen, d => opened = d));

        Cell(cut, "2026-10-20").Click();

        Assert.Equal(new DateOnly(2026, 10, 20), selected);
        Assert.Equal("true", Cell(cut, "2026-10-20").GetAttribute("aria-selected"));
        Assert.Equal("0", Cell(cut, "2026-10-20").GetAttribute("tabindex"));
        Assert.Equal("-1", Cell(cut, "2026-10-14").GetAttribute("tabindex"));
        Assert.Null(opened);

        Cell(cut, "2026-10-20").Click();

        Assert.Equal(new DateOnly(2026, 10, 20), opened);
    }

    [Fact]
    public void NavigationChangesMonth()
    {
        using var context = new BunitContext();
        DateOnly? month = null;

        var cut = RenderMonth(context, parameters => parameters.Add(c => c.MonthChanged, m => month = m));

        cut.Find("button[aria-label='Next month']").Click();

        Assert.Equal(new DateOnly(2026, 11, 1), month);
        Assert.Equal("November 2026", cut.Find(".stil-calendar-title").TextContent.Trim());

        cut.Find("button[aria-label='Previous month']").Click();
        cut.Find("button[aria-label='Previous month']").Click();
        Assert.Equal("September 2026", cut.Find(".stil-calendar-title").TextContent.Trim());

        cut.Find("button.stil-btn-outline").Click();
        Assert.Equal("October 2026", cut.Find(".stil-calendar-title").TextContent.Trim());
        Assert.Equal("true", Cell(cut, "2026-10-14").GetAttribute("aria-selected"));
    }

    [Fact]
    public void KeyboardEdgeShowsNextMonthAndFocusesDay()
    {
        using var context = new BunitContext();
        DateOnly? selected = null;

        var cut = RenderMonth(context, parameters => parameters.Add(c => c.SelectedDateChanged, d => selected = d));

        cut.Find("table").TriggerEvent("onstilcalendaredge", new StilCalendarEdgeEventArgs { Key = "ArrowDown", Date = "2026-11-01" });

        Assert.Equal(new DateOnly(2026, 11, 8), selected);
        Assert.Equal("November 2026", cut.Find(".stil-calendar-title").TextContent.Trim());
        Assert.Equal("0", Cell(cut, "2026-11-08").GetAttribute("tabindex"));
        context.JSInterop.VerifyFocusAsyncInvoke();
    }

    [Fact]
    public void HrefTemplateRendersLinks()
    {
        using var context = new BunitContext();

        var cut = RenderMonth(context, parameters => parameters.Add(c => c.HrefTemplate, "calendar?month={0:yyyy-MM}"));

        Assert.Equal("calendar?month=2026-09", cut.Find("a[aria-label='Previous month']").GetAttribute("href"));
        Assert.Equal("calendar?month=2026-11", cut.Find("a[aria-label='Next month']").GetAttribute("href"));
        Assert.Equal("calendar?month=2026-10", cut.Find("a.stil-btn-outline").GetAttribute("href"));
        Assert.Empty(cut.FindAll(".stil-calendar-header button"));
    }

    [Fact]
    public void MiniMonthShowsDatesOnlyAndMarksRange()
    {
        using var context = new BunitContext();

        var cut = RenderMonth(context, parameters => parameters
            .Add(c => c.Mini, true)
            .Add(c => c.RangeStart, new DateOnly(2026, 10, 12))
            .Add(c => c.RangeEnd, new DateOnly(2026, 10, 18)));

        Assert.Contains("stil-calendar-mini", cut.Find("div").ClassList);
        Assert.Empty(cut.FindAll(".stil-calendar-events, .stil-calendar-dots"));
        Assert.Equal("Wk", cut.Find("th.stil-calendar-week abbr").TextContent);
        Assert.Equal("Mo", cut.FindAll("thead th abbr")[1].TextContent);
        Assert.Equal(7, cut.FindAll("td.stil-calendar-range").Count);
        Assert.Empty(cut.FindAll(".stil-btn-outline"));
    }

    [Fact]
    public void FirstDayOfWeekFollowsCulture()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCalendar<Meeting>>(parameters =>
        {
            Meeting.Configure(parameters, Meeting.Sample(), System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            parameters.Add(c => c.ShowWeekNumbers, false);
        });

        Assert.Equal("Sun", cut.FindAll("thead th abbr")[0].TextContent);
        Assert.Equal("2026-09-27", cut.FindAll("td[role=gridcell]")[0].GetAttribute("data-date"));
        Assert.Empty(cut.FindAll(".stil-calendar-week"));
    }

    [Fact]
    public void ItemsOverMidnightShowOnBothDays()
    {
        using var context = new BunitContext();
        List<Meeting> items =
        [
            new("Night shift", new(2026, 10, 15, 22, 0, 0), new(2026, 10, 16, 2, 0, 0)),
            new("Until midnight", new(2026, 10, 20, 20, 0, 0), new(2026, 10, 21, 0, 0, 0)),
        ];

        var cut = RenderMonth(context, items: items);

        Assert.Single(Cell(cut, "2026-10-15").QuerySelectorAll(".stil-event"));
        Assert.Single(Cell(cut, "2026-10-16").QuerySelectorAll(".stil-event"));
        Assert.Single(Cell(cut, "2026-10-20").QuerySelectorAll(".stil-event"));
        Assert.Empty(Cell(cut, "2026-10-21").QuerySelectorAll(".stil-event"));
    }
}
