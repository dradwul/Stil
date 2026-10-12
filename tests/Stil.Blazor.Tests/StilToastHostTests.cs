using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilToastHostTests
{
    private static BunitContext CreateContext(TimeSpan? defaultDuration = null)
    {
        var context = new BunitContext();
        context.Services.AddStil(options =>
        {
            if (defaultDuration is { } duration)
            {
                options.DefaultToastDuration = duration;
            }
        });
        return context;
    }

    private static IStilToastService Service(BunitContext context) => context.Services.GetRequiredService<IStilToastService>();

    [Fact]
    public void ContainerIsAlwaysPresentAsLabelledRegion()
    {
        using var context = CreateContext();

        var cut = context.Render<StilToastHost>();

        var container = cut.Find(".stil-toast-container");
        Assert.Equal("region", container.GetAttribute("role"));
        Assert.Equal("Notifications", container.GetAttribute("aria-label"));
        Assert.Equal(0, container.ChildElementCount);
    }

    [Fact]
    public void LabelParameterSetsRegionName()
    {
        using var context = CreateContext();

        var cut = context.Render<StilToastHost>(parameters => parameters.Add(component => component.Label, "Aviseringar"));

        Assert.Equal("Aviseringar", cut.Find(".stil-toast-container").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task ShowAddsInfoToastWithTitleAndMessage()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();
        var service = Service(context);

        await cut.InvokeAsync(() => service.Show("File uploaded", "Upload"));

        var toast = cut.Find(".stil-toast");
        Assert.Equal(["stil-toast", "stil-toast-info"], toast.ClassList);
        Assert.Equal("status", toast.GetAttribute("role"));
        Assert.Equal("Upload", toast.QuerySelector(".stil-toast-body .stil-toast-title")?.TextContent);
        Assert.Contains("File uploaded", toast.QuerySelector(".stil-toast-body")?.TextContent);

        var close = toast.QuerySelector("button.stil-close");
        Assert.NotNull(close);
        Assert.Equal("button", close.GetAttribute("type"));
        Assert.Equal("Close", close.GetAttribute("aria-label"));
    }

    [Fact]
    public async Task ToastWithoutTitleHasNoTitleElement()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();

        await cut.InvokeAsync(() => Service(context).Show("Just a message"));

        Assert.Empty(cut.FindAll(".stil-toast-title"));
        Assert.Equal("Just a message", cut.Find(".stil-toast-body").TextContent.Trim());
    }

    [Theory]
    [InlineData(StilVariant.Primary, "stil-toast-primary", "status")]
    [InlineData(StilVariant.Secondary, "stil-toast-secondary", "status")]
    [InlineData(StilVariant.Success, "stil-toast-success", "status")]
    [InlineData(StilVariant.Info, "stil-toast-info", "status")]
    [InlineData(StilVariant.Warning, "stil-toast-warning", "alert")]
    [InlineData(StilVariant.Danger, "stil-toast-danger", "alert")]
    public async Task ShowUsesVariantClassAndRole(StilVariant variant, string expectedClass, string expectedRole)
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();

        await cut.InvokeAsync(() => Service(context).Show("Message", variant: variant));

        var toast = cut.Find(".stil-toast");
        Assert.Contains(expectedClass, toast.ClassList);
        Assert.Equal(expectedRole, toast.GetAttribute("role"));
    }

    [Fact]
    public async Task ShortcutMethodsUseTheRightVariantAndRole()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();
        var service = Service(context);

        await cut.InvokeAsync(() =>
        {
            service.Success("Saved", "Done");
            service.Error("Failed");
            service.Warning("Careful");
            service.Info("FYI");
        });

        var toasts = cut.FindAll(".stil-toast");
        Assert.Equal(
            ["stil-toast-success", "stil-toast-danger", "stil-toast-warning", "stil-toast-info"],
            toasts.Select(toast => toast.ClassList.Single(c => c != "stil-toast")));
        Assert.Equal(["status", "alert", "alert", "status"], toasts.Select(toast => toast.GetAttribute("role")));
        Assert.Equal("Done", toasts[0].QuerySelector(".stil-toast-title")?.TextContent);
        Assert.Equal(4, service.Toasts.Count);
    }

    [Fact]
    public void ShortcutMethodsUseDefaultDuration()
    {
        using var context = CreateContext(TimeSpan.FromSeconds(7));
        var service = Service(context);

        var toast = service.Success("Saved");

        Assert.Equal(StilVariant.Success, toast.Variant);
        Assert.Equal(TimeSpan.FromSeconds(7), toast.Duration);
        Assert.StartsWith("stil-toast-", toast.Id);
    }

    [Fact]
    public async Task CloseButtonRemovesOnlyThatToast()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();
        var service = Service(context);

        await cut.InvokeAsync(() =>
        {
            service.Show("First", duration: TimeSpan.Zero);
            service.Show("Second", duration: TimeSpan.Zero);
        });

        cut.FindAll(".stil-toast")[0].QuerySelector(".stil-close")!.Click();

        var remaining = cut.FindAll(".stil-toast");
        Assert.Single(remaining);
        Assert.Contains("Second", remaining[0].TextContent);
        Assert.Single(service.Toasts);
    }

    [Fact]
    public async Task CloseLabelParameterIsUsed()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>(parameters => parameters.Add(component => component.CloseLabel, "Stäng"));

        await cut.InvokeAsync(() => Service(context).Show("Hi", duration: TimeSpan.Zero));

        Assert.Equal("Stäng", cut.Find(".stil-close").GetAttribute("aria-label"));
    }

    [Fact]
    public async Task ServiceCloseRemovesToast()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();
        var service = Service(context);
        StilToast toast = null!;

        await cut.InvokeAsync(() => { toast = service.Show("Bye", duration: TimeSpan.Zero); });
        await cut.InvokeAsync(() => service.Close(toast));

        Assert.Empty(cut.FindAll(".stil-toast"));
    }

    [Fact]
    public async Task ToastWithShortDurationDisappears()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();
        var service = Service(context);

        await cut.InvokeAsync(() =>
        {
            service.Show("Quick", duration: TimeSpan.FromMilliseconds(50));
            service.Show("Sticky", duration: TimeSpan.Zero);
        });
        Assert.Equal(2, cut.FindAll(".stil-toast").Count);

        cut.WaitForAssertion(() =>
        {
            var toast = Assert.Single(cut.FindAll(".stil-toast"));
            Assert.Contains("Sticky", toast.TextContent);
        }, TimeSpan.FromSeconds(5));
        Assert.Single(service.Toasts);
    }

    [Fact]
    public async Task DefaultDurationFromOptionsClosesToast()
    {
        using var context = CreateContext(TimeSpan.FromMilliseconds(50));
        var cut = context.Render<StilToastHost>();

        await cut.InvokeAsync(() => Service(context).Info("Quick"));

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".stil-toast")), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ZeroDurationStaysOpen()
    {
        using var context = CreateContext(TimeSpan.FromMilliseconds(20));
        var cut = context.Render<StilToastHost>();

        await cut.InvokeAsync(() => Service(context).Show("Sticky", duration: TimeSpan.Zero));
        await Task.Delay(300);

        Assert.Single(cut.FindAll(".stil-toast"));
    }

    [Fact]
    public async Task ManuallyClosedToastDoesNotFailWhenTimerFires()
    {
        using var context = CreateContext();
        var cut = context.Render<StilToastHost>();
        var service = Service(context);

        await cut.InvokeAsync(() => service.Show("Quick", duration: TimeSpan.FromMilliseconds(100)));
        cut.Find(".stil-close").Click();
        await cut.InvokeAsync(() => service.Show("Later", duration: TimeSpan.Zero));
        await Task.Delay(300);

        var toast = Assert.Single(cut.FindAll(".stil-toast"));
        Assert.Contains("Later", toast.TextContent);
    }

    [Fact]
    public void ToastsShownBeforeTheHostRendersAreShown()
    {
        using var context = CreateContext();
        Service(context).Show("Early", duration: TimeSpan.Zero);

        var cut = context.Render<StilToastHost>();

        Assert.Contains("Early", cut.Find(".stil-toast").TextContent);
    }

    [Fact]
    public void ToastShownBeforeTheHostRendersStillClosesAutomatically()
    {
        using var context = CreateContext();
        Service(context).Show("Early", duration: TimeSpan.FromMilliseconds(50));

        var cut = context.Render<StilToastHost>();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".stil-toast")), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ServiceRaisesToastsChanged()
    {
        using var context = CreateContext();
        var service = Service(context);
        var changes = 0;
        service.ToastsChanged += () => changes++;

        var toast = service.Show("Hi", duration: TimeSpan.Zero);
        service.Close(toast);
        service.Close(toast);

        Assert.Equal(2, changes);
        Assert.Empty(service.Toasts);
        await Task.CompletedTask;
    }
}
