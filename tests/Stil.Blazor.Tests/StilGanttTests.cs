using System.Globalization;
using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilGanttTests
{
    private sealed record Step(string Id, string Title, DateOnly Start, DateOnly End, double Done = 0, string[]? After = null, bool Milestone = false);

    private static DateOnly Day(int day) => new(2026, 11, day);

    // Monday 2 November to Friday 20 November 2026.
    private static List<Step> Plan() =>
    [
        new("design", "Design", Day(2), Day(6), 1),
        new("build", "Build", Day(9), Day(13), 0.4, ["design"]),
        new("test", "Test", Day(12), Day(17), 0, ["build"]),
        new("launch", "Launch", Day(20), Day(20), 0, ["test"], Milestone: true),
    ];

    private static IRenderedComponent<StilGantt<Step>> Render(BunitContext context, List<Step> plan,
        Action<ComponentParameterCollectionBuilder<StilGantt<Step>>>? more = null) =>
        context.Render<StilGantt<Step>>(parameters =>
        {
            parameters
                .Add(c => c.Items, plan)
                .Add(c => c.ItemKey, s => s.Id)
                .Add(c => c.Title, s => s.Title)
                .Add(c => c.Start, s => s.Start)
                .Add(c => c.End, s => s.End)
                .Add(c => c.Progress, s => s.Done)
                .Add(c => c.DependsOn, s => s.After)
                .Add(c => c.Milestone, s => s.Milestone)
                .Add(c => c.Now, Day(11))
                .Add(c => c.Culture, CultureInfo.GetCultureInfo("en-GB"));
            more?.Invoke(parameters);
        });

    [Fact]
    public void TasksAreBarsOnTheirDays()
    {
        using var context = new BunitContext();

        var cut = Render(context, Plan());

        // Monday 2 to Sunday 22 November: three weeks.
        Assert.Equal("--stil-gantt-days: 21; --stil-gantt-rows: 4", cut.Find(".stil-gantt").GetAttribute("style"));
        Assert.Equal(["November 2026"], cut.FindAll(".stil-gantt-months span").Select(s => s.TextContent));
        Assert.Equal(["Design", "Build", "Test", "Launch"], cut.FindAll(".stil-gantt-name").Select(n => n.TextContent));
        var bars = cut.FindAll(".stil-gantt-bar");
        Assert.Equal("--_start: 7; --_length: 5; --_row: 1; --_progress: 0.4", bars[1].GetAttribute("style"));
        // The long date pattern differs between Windows and the ICU data on Linux ("Monday, 9
        // November 2026" or "Monday 9 November 2026"), so the expected dates use it too.
        var enGb = CultureInfo.GetCultureInfo("en-GB");
        Assert.Equal($"Build: {Day(9).ToString("D", enGb)} to {Day(13).ToString("D", enGb)}, 40% done, after Design", bars[1].GetAttribute("aria-label"));
        Assert.Contains("stil-gantt-milestone", bars[3].ClassName);
        Assert.StartsWith("Launch: milestone on", bars[3].GetAttribute("aria-label"));
        Assert.Equal("--_start: 9", cut.Find(".stil-gantt-today").GetAttribute("style"));
        Assert.Equal("date", cut.FindAll(".stil-gantt-date")[9].GetAttribute("aria-current"));
    }

    [Fact]
    public void DependenciesAreArrowsAndUnmetOnesAreMarked()
    {
        using var context = new BunitContext();

        var cut = Render(context, Plan());

        var links = cut.FindAll("polyline");
        Assert.Equal(3, links.Count);
        // Design ends after day 5 (Friday); Build starts on day 7: a straight drop.
        Assert.Equal("5,0.5 5.3,0.5 5.3,1.5 7,1.5", links[0].GetAttribute("points"));
        Assert.DoesNotContain("conflict", links[0].ClassName);
        // Test starts on Thursday 12, before Build ends on Friday 13.
        Assert.Contains("stil-gantt-link-conflict", links[1].ClassName);
        Assert.Equal("12,1.5 12.3,1.5 12.3,2 9.7,2 9.7,2.5 10,2.5", links[1].GetAttribute("points"));
        Assert.Equal("--_start: 18.15; --_row: 3", cut.FindAll(".stil-gantt-arrow")[2].GetAttribute("style"));
    }

    [Fact]
    public void BarsAreButtonsWhenClickable()
    {
        using var context = new BunitContext();
        Step? clicked = null;

        var cut = Render(context, Plan(), p => p.Add(c => c.OnItemClick, s => clicked = s));
        cut.FindAll("button.stil-gantt-bar")[2].Click();

        Assert.Equal("Test", clicked?.Title);
        Assert.Empty(cut.FindAll("div.stil-gantt-bar"));
    }

    [Fact]
    public void EmptyPlanShowsTwoWeeksFromToday()
    {
        using var context = new BunitContext();

        var cut = Render(context, []);

        Assert.Equal("--stil-gantt-days: 14; --stil-gantt-rows: 0", cut.Find(".stil-gantt").GetAttribute("style"));
        Assert.Empty(cut.FindAll(".stil-gantt-bar"));
    }
}
