using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilDialogServiceTests
{
    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddStil();
        return context;
    }

    [Fact]
    public void AddStilRegistersServicesAndOptions()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options => options.DefaultToastDuration = TimeSpan.FromSeconds(9));

        Assert.NotNull(context.Services.GetRequiredService<IStilDialogService>());
        Assert.NotNull(context.Services.GetRequiredService<IStilToastService>());
        Assert.Equal(TimeSpan.FromSeconds(9), context.Services.GetRequiredService<StilOptions>().DefaultToastDuration);
    }

    [Fact]
    public void HostRendersNothingWithoutDialogs()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();

        Assert.Empty(host.FindAll("dialog"));
        Assert.Empty(service.Dialogs);
    }

    [Fact]
    public async Task ConfirmAsyncRendersSmallDialogWithMessageAndButtons()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();

        await host.InvokeAsync(() => { _ = service.ConfirmAsync("Delete file", "This cannot be undone."); });

        var dialog = host.Find("dialog");
        Assert.Equal(["stil-dialog", "stil-dialog-sm"], dialog.ClassList);
        Assert.Equal("Delete file", dialog.QuerySelector(".stil-dialog-title")?.TextContent);
        Assert.Equal("This cannot be undone.", dialog.QuerySelector(".stil-dialog-body p")?.TextContent);

        var buttons = dialog.QuerySelectorAll(".stil-dialog-body button");
        Assert.Equal(["Cancel", "OK"], buttons.Select(button => button.TextContent));
        Assert.Equal(["stil-btn", "stil-btn-secondary", "stil-btn-ghost"], buttons[0].ClassList);
        Assert.Equal(["stil-btn", "stil-btn-primary"], buttons[1].ClassList);
        Assert.Single(service.Dialogs);
    }

    [Fact]
    public async Task ConfirmFocusesConfirmButtonByDefault()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();

        await host.InvokeAsync(() => { _ = service.ConfirmAsync("Save", "Save changes?"); });

        var buttons = host.FindAll(".stil-dialog-body button");
        Assert.False(buttons[0].HasAttribute("autofocus"));
        Assert.True(buttons[1].HasAttribute("autofocus"));
    }

    [Fact]
    public async Task DangerConfirmUsesCustomTextAndFocusesCancel()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        var options = new StilConfirmOptions { ConfirmText = "Delete", CancelText = "Keep", ConfirmVariant = StilVariant.Danger };

        await host.InvokeAsync(() => { _ = service.ConfirmAsync("Delete", "Sure?", options); });

        var buttons = host.FindAll(".stil-dialog-body button");
        Assert.Equal(["Keep", "Delete"], buttons.Select(button => button.TextContent));
        Assert.Contains("stil-btn-danger", buttons[1].ClassList);
        Assert.True(buttons[0].HasAttribute("autofocus"));
        Assert.False(buttons[1].HasAttribute("autofocus"));
    }

    [Fact]
    public async Task ClickingConfirmResolvesTrueAndClosesDialog()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<bool> result = null!;

        await host.InvokeAsync(() => { result = service.ConfirmAsync("Delete", "Sure?"); });
        host.FindAll(".stil-dialog-body button")[1].Click();

        Assert.True(await result.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Empty(host.FindAll("dialog"));
        Assert.Empty(service.Dialogs);
    }

    [Fact]
    public async Task ClickingCancelResolvesFalse()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<bool> result = null!;

        await host.InvokeAsync(() => { result = service.ConfirmAsync("Delete", "Sure?"); });
        host.FindAll(".stil-dialog-body button")[0].Click();

        Assert.False(await result.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Empty(host.FindAll("dialog"));
    }

    [Fact]
    public async Task ClosingWithHeaderButtonResolvesFalse()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<bool> result = null!;

        await host.InvokeAsync(() => { result = service.ConfirmAsync("Delete", "Sure?"); });
        host.Find(".stil-dialog-header .stil-close").Click();

        Assert.False(await result.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Empty(host.FindAll("dialog"));
    }

    [Fact]
    public async Task EscapeInTheBrowserResolvesFalse()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<bool> result = null!;

        await host.InvokeAsync(() => { result = service.ConfirmAsync("Delete", "Sure?"); });
        var dialog = host.FindComponent<StilDialog>();
        await host.InvokeAsync(() => dialog.Instance.OnDialogClosed());

        Assert.False(await result.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Empty(service.Dialogs);
    }

    [Fact]
    public async Task OpenAsyncRendersComponentWithParametersAndOptions()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();

        await host.InvokeAsync(() =>
        {
            _ = service.OpenAsync<GreetingDialogContent>(
                "Greeting",
                new Dictionary<string, object?> { [nameof(GreetingDialogContent.Greeting)] = "hello" },
                new StilDialogOptions { Size = StilSize.Large, ShowClose = false });
        });

        var dialog = host.Find("dialog");
        Assert.Equal(["stil-dialog", "stil-dialog-lg"], dialog.ClassList);
        Assert.Equal("Greeting", dialog.QuerySelector(".stil-dialog-title")?.TextContent);
        Assert.Empty(dialog.QuerySelectorAll(".stil-close"));
        Assert.Equal("hello", dialog.QuerySelector(".stil-dialog-body .greeting")?.TextContent);
    }

    [Fact]
    public async Task ContentCanCloseWithResultThroughCascadingReference()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<object?> result = null!;

        await host.InvokeAsync(() =>
        {
            result = service.OpenAsync<GreetingDialogContent>(
                "Greeting",
                new Dictionary<string, object?> { [nameof(GreetingDialogContent.Greeting)] = "hello" });
        });

        var content = host.FindComponent<GreetingDialogContent>();
        Assert.NotNull(content.Instance.Dialog);
        Assert.Equal("Greeting", content.Instance.Dialog.Title);
        Assert.Same(service.Dialogs.Single(), content.Instance.Dialog);

        host.Find(".pick").Click();

        Assert.Equal("HELLO", await result.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Empty(host.FindAll("dialog"));
    }

    [Fact]
    public async Task DismissedOpenAsyncResolvesNull()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<object?> result = null!;

        await host.InvokeAsync(() => { result = service.OpenAsync<GreetingDialogContent>("Greeting"); });
        host.Find(".stil-close").Click();

        Assert.Null(await result.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task StacksDialogsAndClosesTheRightOne()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<bool> first = null!;
        Task<bool> second = null!;

        await host.InvokeAsync(() =>
        {
            first = service.ConfirmAsync("First", "One");
            second = service.ConfirmAsync("Second", "Two");
        });

        Assert.Equal(["First", "Second"], host.FindAll(".stil-dialog-title").Select(title => title.TextContent));

        await host.InvokeAsync(() => service.Dialogs[1].Close(true));

        Assert.True(await second.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.False(first.IsCompleted);
        Assert.Equal(["First"], host.FindAll(".stil-dialog-title").Select(title => title.TextContent));
    }

    [Fact]
    public async Task DialogOpenedBeforeHostRendersIsShown()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddStil();
        var service = context.Services.GetRequiredService<IStilDialogService>();

        _ = service.ConfirmAsync("Early", "Opened first");
        var host = context.Render<StilDialogHost>();

        Assert.Equal("Early", host.Find(".stil-dialog-title").TextContent);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ClosingTwiceKeepsTheFirstResult()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();
        Task<object?> result = null!;
        var changes = 0;
        service.DialogsChanged += () => changes++;

        await host.InvokeAsync(() => { result = service.OpenAsync<GreetingDialogContent>("Greeting"); });
        var reference = service.Dialogs.Single();
        await host.InvokeAsync(() =>
        {
            reference.Close("first");
            reference.Close("second");
        });

        Assert.Equal("first", await result);
        Assert.Equal(2, changes);
    }

    [Fact]
    public void DialogIdsAreUnique()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        var service = context.Services.GetRequiredService<IStilDialogService>();

        _ = service.ConfirmAsync("A", "a");
        _ = service.ConfirmAsync("B", "b");

        Assert.NotEqual(service.Dialogs[0].Id, service.Dialogs[1].Id);
    }

    [Fact]
    public async Task HostUnsubscribesWhenDisposed()
    {
        using var context = CreateContext();
        var service = context.Services.GetRequiredService<IStilDialogService>();
        var host = context.Render<StilDialogHost>();

        await context.DisposeComponentsAsync();

        // Would throw if the disposed host still tried to render.
        _ = service.ConfirmAsync("After", "dispose");
        Assert.Single(service.Dialogs);
    }
}
