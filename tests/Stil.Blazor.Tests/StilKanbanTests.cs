using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilKanbanTests
{
    private sealed record Job(int Id, string Title, string Status);

    private static readonly StilKanbanColumn[] Columns =
    [
        new("todo", "To do"),
        new("doing", "Doing", Limit: 1),
        new("done", "Done"),
    ];

    private static List<Job> Tasks() =>
    [
        new Job(1, "Order badges", "todo"),
        new Job(2, "Book room 4", "todo"),
        new Job(3, "Print signs", "doing"),
        new Job(4, "Clean desk", "doing"),
    ];

    private static IRenderedComponent<StilKanban<Job>> Render(BunitContext context, List<Job> tasks, List<StilKanbanMove<Job>> moves)
    {
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        IRenderedComponent<StilKanban<Job>>? cut = null;
        cut = context.Render<StilKanban<Job>>(parameters => parameters
            .Add(component => component.Items, tasks)
            .Add(component => component.Columns, Columns)
            .Add(component => component.Column, t => t.Status)
            .Add(component => component.ItemKey, t => t.Id)
            .Add(component => component.Title, t => t.Title)
            .Add(component => component.OnCardMove, (StilKanbanMove<Job> move) =>
            {
                moves.Add(move);
                move.ApplyTo(tasks, (t, column) => t with { Status = column });
            }));
        return cut;
    }

    private static List<string> Titles(IRenderedComponent<StilKanban<Job>> cut, string column) =>
        [.. cut.Find($"[data-stil-column={column}]").QuerySelectorAll(".stil-kanban-card").Select(c => c.TextContent.Trim())];

    [Fact]
    public void ColumnsShowTheirCardsCountsAndLimits()
    {
        using var context = new BunitContext();

        var cut = Render(context, Tasks(), []);

        Assert.Equal(["Order badges", "Book room 4"], Titles(cut, "todo"));
        Assert.Equal("2 / 1, over the limit", cut.Find("[data-stil-column=doing] .stil-kanban-count").TextContent.Trim().Replace("\n", "").Replace("  ", ""));
        Assert.Contains("stil-kanban-column-over", cut.Find("[data-stil-column=doing]").ClassName);
        Assert.Equal("No cards", cut.Find("[data-stil-column=done] .stil-kanban-empty").TextContent);
        Assert.Equal("Board", cut.Find(".stil-kanban").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task DroppingACardRaisesTheMoveWithItsNeighbours()
    {
        using var context = new BunitContext();
        var tasks = Tasks();
        var moves = new List<StilKanbanMove<Job>>();

        var cut = Render(context, tasks, moves);
        await cut.Find(".stil-kanban").TriggerEventAsync("onstilcardmove", new StilCardMoveEventArgs { Item = "1", Column = "doing", Index = 1 });

        var move = moves.Single();
        Assert.Equal(("todo", "doing", 1), (move.From, move.To, move.Index));
        Assert.Equal(4, move.Before!.Id);
        Assert.Equal(3, move.After!.Id);
        cut.Render();
        Assert.Equal(["Print signs", "Order badges", "Clean desk"], Titles(cut, "doing"));
    }

    [Fact]
    public void DroppingInTheSamePlaceDoesNothing()
    {
        using var context = new BunitContext();
        var moves = new List<StilKanbanMove<Job>>();

        var cut = Render(context, Tasks(), moves);
        cut.Find(".stil-kanban").TriggerEvent("onstilcardmove", new StilCardMoveEventArgs { Item = "2", Column = "todo", Index = 1 });

        Assert.Empty(moves);
    }

    [Fact]
    public void KeyboardPicksUpMovesDropsAndCancels()
    {
        using var context = new BunitContext();
        var tasks = Tasks();
        var moves = new List<StilKanbanMove<Job>>();
        var cut = Render(context, tasks, moves);

        cut.Find("[data-stil-item='2']").KeyDown(new KeyboardEventArgs { Key = " " });
        Assert.True(cut.Find("[data-stil-item='2']").HasAttribute("data-stil-grabbed"));
        Assert.StartsWith("Book room 4 picked up.", cut.Find("[role=status]").TextContent);

        cut.Find("[data-stil-item='2']").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal(("doing", 1), (moves[^1].To, moves[^1].Index));
        Assert.Equal("Doing, position 2 of 3.", cut.Find("[role=status]").TextContent);
        Assert.Equal(["Print signs", "Book room 4", "Clean desk"], Titles(cut, "doing"));

        cut.Find("[data-stil-item='2']").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Equal(("todo", 1), (moves[^1].To, moves[^1].Index));
        Assert.Equal("Book room 4 put back in To do.", cut.Find("[role=status]").TextContent);
        Assert.False(cut.Find("[data-stil-item='2']").HasAttribute("data-stil-grabbed"));

        cut.Find("[data-stil-item='1']").KeyDown(new KeyboardEventArgs { Key = " " });
        cut.Find("[data-stil-item='1']").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        cut.Find("[data-stil-item='1']").KeyDown(new KeyboardEventArgs { Key = " " });
        Assert.Equal("Order badges dropped in To do, position 2 of 2.", cut.Find("[role=status]").TextContent);
        Assert.Equal(["Book room 4", "Order badges"], Titles(cut, "todo"));
    }

    [Fact]
    public void CollapsedColumnHidesItsCardsAndTakesNoDrops()
    {
        using var context = new BunitContext();
        var moves = new List<StilKanbanMove<Job>>();
        var cut = Render(context, Tasks(), moves);

        cut.Find("[data-stil-column=done] .stil-kanban-toggle").Click();

        Assert.True(cut.Find("[data-stil-column=done]").HasAttribute("data-stil-collapsed"));
        Assert.Equal("Expand Done", cut.Find("[data-stil-column=done] .stil-kanban-toggle").GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("[data-stil-column=done] .stil-kanban-cards"));

        cut.Find(".stil-kanban").TriggerEvent("onstilcardmove", new StilCardMoveEventArgs { Item = "1", Column = "done", Index = 0 });
        Assert.Empty(moves);
    }

    [Fact]
    public void ApplyToKeepsTheBoardOrder()
    {
        var tasks = Tasks();
        var move = new StilKanbanMove<Job>(tasks[3], "doing", "todo", 0, tasks[0], default);

        move.ApplyTo(tasks, (t, column) => t with { Status = column });

        Assert.Equal([4, 1, 2, 3], tasks.Select(t => t.Id));
        Assert.Equal("todo", tasks[0].Status);
    }
}
