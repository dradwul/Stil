using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilActivityLogTests
{
    private static readonly DateTime Now = new(2026, 11, 5, 14, 0, 0);

    private sealed record Entry(int Id, DateTime Time, string Title, string? Actor = null, string? Type = null, StilVariant? Variant = null);

    private static readonly List<Entry> Entries =
    [
        new(1, Now.AddSeconds(-20), "Checked in", "Maja Lind", "Visit", StilVariant.Success),
        new(2, Now.AddMinutes(-1), "Badge printed", "Reception", "System"),
        new(3, Now.AddMinutes(-25), "Delivery signed", "Erik Holm", "Delivery"),
        new(4, Now.AddHours(-3), "Checked out", "Sara Berg", "Visit"),
        new(5, Now.AddDays(-1).AddHours(-2), "Checked in", "Sara Berg", "Visit"),
        new(6, new DateTime(2026, 10, 30, 9, 15, 0), "Inspection done", "Jonas Ek", "Inspection"),
    ];

    private static IRenderedComponent<StilActivityLog<Entry>> Render(BunitContext context, IEnumerable<Entry> items,
        Action<ComponentParameterCollectionBuilder<StilActivityLog<Entry>>>? more = null) =>
        context.Render<StilActivityLog<Entry>>(parameters =>
        {
            parameters
                .Add(component => component.Items, items)
                .Add(component => component.Time, e => e.Time)
                .Add(component => component.Title, e => e.Title)
                .Add(component => component.Actor, e => e.Actor)
                .Add(component => component.Type, e => e.Type)
                .Add(component => component.Variant, e => e.Variant)
                .Add(component => component.ItemKey, e => e.Id)
                .Add(component => component.Now, Now);
            more?.Invoke(parameters);
        });

    [Fact]
    public void GroupsByDayNewestFirstWithRelativeTimes()
    {
        using var context = new BunitContext();

        var cut = Render(context, Entries);

        Assert.Equal(["Today", "Yesterday", "Friday, October 30, 2026"], cut.FindAll(".stil-log-day").Select(d => d.TextContent));
        Assert.Equal(["Just now", "1 minute ago", "25 minutes ago", "3 hours ago"],
            cut.FindAll(".stil-log-group")[0].QuerySelectorAll(".stil-log-time").Select(t => t.TextContent));
        Assert.Equal("2026-11-05T13:59:40", cut.Find(".stil-log-time").GetAttribute("datetime"));
        Assert.Equal("Maja Lind · Visit", cut.Find(".stil-log-meta").TextContent);
        Assert.Contains("stil-log-marker-success", cut.Find(".stil-log-marker").ClassName);
    }

    [Fact]
    public void SearchAndFiltersNarrowTheLog()
    {
        using var context = new BunitContext();

        var cut = Render(context, Entries, p => p.Add(c => c.Searchable, true).Add(c => c.Filterable, true));

        var selects = cut.FindAll("select");
        Assert.Equal(["All types", "Delivery", "Inspection", "System", "Visit"], selects[0].QuerySelectorAll("option").Select(o => o.TextContent));
        selects[0].Change("Visit");
        Assert.Equal(3, cut.FindAll(".stil-log-entry").Count);

        cut.FindAll("select")[1].Change("Sara Berg");
        Assert.Equal(2, cut.FindAll(".stil-log-entry").Count);

        cut.Find("input[type=search]").Input("out");
        Assert.Equal("Checked out", cut.Find(".stil-log-title").TextContent);

        cut.Find("input[type=search]").Input("nothing like this");
        Assert.Equal("No entries match.", cut.Find(".stil-log-empty").TextContent);
    }

    [Fact]
    public void NewEntriesAreHighlightedAndAnnounced()
    {
        using var context = new BunitContext();
        var items = Entries.Take(3).ToList();

        var cut = Render(context, items);
        Assert.Empty(cut.Find("[role=status]").TextContent);

        items.Add(new Entry(7, Now, "Visitor waiting", "Reception"));
        cut.Render(parameters => parameters.Add(component => component.Items, items));

        Assert.Equal("New: Visitor waiting", cut.Find("[role=status]").TextContent);
        Assert.Single(cut.FindAll(".stil-log-entry-new"));
        Assert.Contains("Visitor waiting", cut.Find(".stil-log-entry-new").TextContent);

        items.Add(new Entry(8, Now, "Parcel arrived"));
        items.Add(new Entry(9, Now, "Parcel signed"));
        cut.Render(parameters => parameters.Add(component => component.Items, items));

        Assert.Equal("2 new entries", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void EmptyLogSaysSo()
    {
        using var context = new BunitContext();

        var cut = Render(context, []);

        Assert.Equal("Nothing has happened yet.", cut.Find(".stil-log-empty").TextContent);
    }

    [Fact]
    public void ScrollingLogIsAFocusableRegion()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render(context, Entries, p => p.Add(c => c.Height, "20rem").Add(c => c.Virtualize, true));

        var scroll = cut.Find(".stil-log-scroll");
        Assert.Equal("max-height: 20rem", scroll.GetAttribute("style"));
        Assert.Equal("0", scroll.GetAttribute("tabindex"));
        Assert.Equal("Activity log", scroll.GetAttribute("aria-label"));
        Assert.NotNull(cut.Find("[role=list]"));
    }

    [Fact]
    public void TimelineListsTheOldestFirst()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTimeline<Entry>>(parameters => parameters
            .Add(component => component.Items, Entries)
            .Add(component => component.Time, e => e.Time)
            .Add(component => component.Title, e => e.Title)
            .Add(component => component.Actor, e => e.Actor)
            .Add(component => component.Label, "Visit history")
            .Add(component => component.Now, Now));

        Assert.Equal("Visit history", cut.Find("ol").GetAttribute("aria-label"));
        var times = cut.FindAll("time").Select(t => t.TextContent).ToList();
        Assert.Equal("October 30, 9:15 AM", times[0]);
        Assert.Equal("Just now", times[^1]);
        Assert.Equal("Inspection done", cut.Find(".stil-log-title").TextContent);
        Assert.Contains("· Jonas Ek", cut.Find(".stil-log-meta").TextContent);
    }
}
