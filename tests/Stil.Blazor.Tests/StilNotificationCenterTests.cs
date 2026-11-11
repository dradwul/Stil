using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilNotificationCenterTests
{
    [Fact]
    public void ToastsAreKeptOnlyWhenAsked()
    {
        var services = new ServiceCollection().AddStil().BuildServiceProvider();
        var toasts = services.GetRequiredService<IStilToastService>();
        var notifications = services.GetRequiredService<IStilNotificationService>();

        toasts.Success("Saved");
        toasts.Show("Order 1042 was shipped", "Shipped", StilVariant.Info, keep: true);

        var kept = Assert.Single(notifications.Notifications);
        Assert.Equal(("Order 1042 was shipped", "Shipped", StilVariant.Info, false), (kept.Message, kept.Title, kept.Variant, kept.IsRead));
        Assert.Equal(1, notifications.UnreadCount);
    }

    [Fact]
    public void KeepToastsKeepsEveryToastUpToTheLimit()
    {
        var services = new ServiceCollection().AddStil(o => { o.KeepToasts = true; o.MaxNotifications = 3; }).BuildServiceProvider();
        var toasts = services.GetRequiredService<IStilToastService>();
        var notifications = services.GetRequiredService<IStilNotificationService>();

        for (var i = 1; i <= 5; i++)
        {
            toasts.Info($"Message {i}");
        }

        Assert.Equal(["Message 5", "Message 4", "Message 3"], notifications.Notifications.Select(n => n.Message));
    }

    [Fact]
    public void BellCountsUnreadAndThePanelListsThem()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        var notifications = context.Services.GetRequiredService<IStilNotificationService>();
        notifications.Add("Room 4 is booked", "Booking");

        var cut = context.Render<StilNotificationCenter>();

        Assert.Equal("Notifications, 1 unread", cut.Find(".stil-notification-bell").GetAttribute("aria-label"));
        Assert.Equal("1", cut.Find(".stil-notification-count").TextContent);
        Assert.Equal(cut.Find("[popover]").Id, cut.Find(".stil-notification-bell").GetAttribute("popovertarget"));

        cut.InvokeAsync(() => notifications.Add("Maja Lind checked in", null, StilVariant.Success, href: "/check-in"));

        Assert.Equal("New notification: Maja Lind checked in", cut.Find("[role=status]").TextContent);
        Assert.Equal(["Maja Lind checked in", "Room 4 is booked"], cut.FindAll(".stil-notification-message").Select(m => m.TextContent));
        Assert.Equal(2, cut.FindAll(".stil-notification-unread").Count);
        Assert.Contains("stil-log-marker-success", cut.Find(".stil-notification .stil-log-marker").ClassName);
    }

    [Fact]
    public void ChoosingMarksReadAndOpensThePage()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        var notifications = context.Services.GetRequiredService<IStilNotificationService>();
        notifications.Add("Room 4 is booked");
        notifications.Add("Maja Lind checked in", href: "/check-in");

        var cut = context.Render<StilNotificationCenter>();
        cut.Find(".stil-notification-body").Click();

        Assert.EndsWith("/check-in", context.Services.GetRequiredService<NavigationManager>().Uri);
        Assert.Single(cut.FindAll(".stil-notification-unread"));
        Assert.Equal("Notifications, 1 unread", cut.Find(".stil-notification-bell").GetAttribute("aria-label"));

        cut.FindAll("button").Single(b => b.TextContent == "Mark all as read").Click();
        Assert.Empty(cut.FindAll(".stil-notification-count"));
        Assert.Equal("Notifications", cut.Find(".stil-notification-bell").GetAttribute("aria-label"));
    }

    [Fact]
    public void DismissAndClearRemoveThem()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        var notifications = context.Services.GetRequiredService<IStilNotificationService>();
        notifications.Add("One");
        notifications.Add("Two");

        var cut = context.Render<StilNotificationCenter>();
        cut.Find(".stil-notification button[aria-label=Dismiss]").Click();
        Assert.Equal(["One"], cut.FindAll(".stil-notification-message").Select(m => m.TextContent));

        cut.FindAll("button").Single(b => b.TextContent == "Clear all").Click();
        Assert.Equal("No notifications", cut.Find(".stil-notification-empty").TextContent);
    }
}
