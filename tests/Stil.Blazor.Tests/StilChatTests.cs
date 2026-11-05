using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilChatTests
{
    private static readonly DateTime Now = new(2026, 11, 5, 14, 0, 0);

    private sealed record Message(int Id, DateTime Time, string Author, string Text, bool Mine = false);

    private static List<Message> Conversation() =>
    [
        new(1, Now.AddDays(-1).AddHours(-3), "Maja Lind", "Is the room free tomorrow?"),
        new(2, Now.AddMinutes(-10), "You", "Yes, from nine.", Mine: true),
        new(3, Now.AddMinutes(-8), "Maja Lind", "Great, I'll book it."),
        new(4, Now.AddMinutes(-7), "Maja Lind", "Thanks!"),
    ];

    private static IRenderedComponent<TChat> Render<TChat>(BunitContext context, List<Message> messages,
        Action<ComponentParameterCollectionBuilder<TChat>>? more = null) where TChat : StilChat<Message> =>
        context.Render<TChat>(parameters =>
        {
            parameters
                .Add(component => component.Items, messages)
                .Add(component => component.Time, m => m.Time)
                .Add(component => component.Title, m => m.Text)
                .Add(component => component.Actor, m => m.Author)
                .Add(component => component.Mine, m => m.Mine)
                .Add(component => component.ItemKey, m => m.Id)
                .Add(component => component.Now, Now);
            more?.Invoke(parameters);
        });

    [Fact]
    public void MessagesReadOldestFirstWithSidesAndRuns()
    {
        using var context = new BunitContext();

        var cut = Render<StilChat<Message>>(context, Conversation());

        Assert.Equal(["Yesterday", "Today"], cut.FindAll(".stil-chat-day").Select(d => d.TextContent));
        var messages = cut.FindAll(".stil-chat-message");
        Assert.Equal("Is the room free tomorrow?", messages[0].QuerySelector(".stil-chat-text")!.TextContent);
        Assert.Contains("stil-chat-message-mine", messages[1].ClassName);
        Assert.Null(messages[1].QuerySelector(".stil-chat-author"));
        Assert.Equal("Maja Lind", messages[2].QuerySelector(".stil-chat-author")!.TextContent);
        // Maja's second message within five minutes joins her first.
        Assert.Contains("stil-chat-message-continued", messages[3].ClassName);
        Assert.Null(messages[3].QuerySelector(".stil-chat-head"));
        Assert.Equal("28rem", cut.Find(".stil-chat-scroll").GetAttribute("style")!["height: ".Length..]);
    }

    [Fact]
    public void SendingRaisesOnSendAndEmptiesTheComposer()
    {
        using var context = new BunitContext();
        var sent = new List<StilChatMessage>();

        var cut = Render<StilChat<Message>>(context, Conversation(), p => p.Add(c => c.OnSend, (StilChatMessage m) => sent.Add(m)));

        Assert.Equal("enter", cut.Find("textarea").GetAttribute("data-stil-send"));
        cut.Find("form").Submit();
        Assert.Empty(sent);

        cut.Find("textarea").Input("  See you at nine \n");
        cut.Find("form").Submit();

        Assert.Equal("See you at nine", sent.Single().Text);
        Assert.Empty(sent.Single().Files);
        Assert.Equal("", cut.Find("textarea").GetAttribute("value"));
    }

    [Fact]
    public void OthersTypingAndNewMessagesAreShownAndAnnounced()
    {
        using var context = new BunitContext();
        var messages = Conversation();

        var cut = Render<StilChat<Message>>(context, messages, p => p.Add(c => c.Typing, ["Maja Lind"]));
        Assert.Equal("Maja Lind is typing…", cut.Find(".stil-chat-typing").TextContent.Trim());

        messages.Add(new Message(5, Now, "You", "Booked.", Mine: true));
        cut.Render(parameters => parameters.Add(component => component.Items, messages).Add(component => component.Typing, null));
        Assert.Empty(cut.Find("[role=status]").TextContent);
        Assert.Empty(cut.FindAll(".stil-chat-typing"));

        messages.Add(new Message(6, Now, "Erik Holm", "Can I join?"));
        cut.Render(parameters => parameters.Add(component => component.Items, messages));
        Assert.Equal("New message from Erik Holm: Can I join?", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void CommentsAreAThreadThatSendsOnCtrlEnter()
    {
        using var context = new BunitContext();

        var cut = Render<StilComments<Message>>(context, Conversation());

        Assert.Contains("stil-comments", cut.Find(".stil-chat").ClassName);
        Assert.Empty(cut.FindAll(".stil-chat-message-mine"));
        Assert.Equal("ctrl-enter", cut.Find("textarea").GetAttribute("data-stil-send"));
        Assert.Null(cut.Find(".stil-chat-scroll").GetAttribute("style"));
        Assert.Equal("Comment", cut.Find("button[type=submit]").TextContent.Trim());
    }
}
