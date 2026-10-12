using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilAlertTests
{
    [Theory]
    [InlineData(StilVariant.Primary, "stil-alert-primary")]
    [InlineData(StilVariant.Secondary, "stil-alert-secondary")]
    [InlineData(StilVariant.Success, "stil-alert-success")]
    [InlineData(StilVariant.Danger, "stil-alert-danger")]
    [InlineData(StilVariant.Warning, "stil-alert-warning")]
    [InlineData(StilVariant.Info, "stil-alert-info")]
    public void RendersVariantClassAndPassesAttributesThrough(
        StilVariant variant,
        string variantClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Variant, variant)
            .AddChildContent("Alert content")
            .AddUnmatched("aria-live", "polite")
            .AddUnmatched("class", "custom-alert"));

        var alert = cut.Find(".stil-alert");
        Assert.Equal(["stil-alert", variantClass, "custom-alert"], alert.ClassList);
        Assert.Equal("polite", alert.GetAttribute("aria-live"));
        Assert.Equal(variant is StilVariant.Danger or StilVariant.Warning ? "alert" : "status", alert.GetAttribute("role"));
        Assert.Equal("Alert content", alert.TextContent);
    }

    [Fact]
    public void DefaultsToInfoVariant()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>();

        Assert.Contains("stil-alert-info", cut.Find(".stil-alert").ClassList);
    }

    [Theory]
    [InlineData(StilVariant.Danger, "alert")]
    [InlineData(StilVariant.Warning, "alert")]
    [InlineData(StilVariant.Info, "status")]
    [InlineData(StilVariant.Success, "status")]
    [InlineData(StilVariant.Primary, "status")]
    [InlineData(StilVariant.Secondary, "status")]
    public void DefaultRoleDependsOnVariant(StilVariant variant, string expectedRole)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters.Add(component => component.Variant, variant));

        Assert.Equal(expectedRole, cut.Find(".stil-alert").GetAttribute("role"));
    }

    [Theory]
    [InlineData(StilVariant.Danger, "status")]
    [InlineData(StilVariant.Info, "alert")]
    [InlineData(StilVariant.Success, "note")]
    public void RoleParameterOverridesDefault(StilVariant variant, string role)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Variant, variant)
            .Add(component => component.Role, role));

        Assert.Equal(role, cut.Find(".stil-alert").GetAttribute("role"));
    }

    [Fact]
    public void SubtleAddsSubtleClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Variant, StilVariant.Warning)
            .Add(component => component.Subtle, true));

        Assert.Equal(["stil-alert", "stil-alert-warning", "stil-alert-subtle"], cut.Find(".stil-alert").ClassList);
    }

    [Fact]
    public void TitleRendersBeforeContent()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Title, "Heads up")
            .AddChildContent("<span>Body</span>"));

        var alert = cut.Find(".stil-alert");
        var title = alert.FirstElementChild;
        Assert.NotNull(title);
        Assert.Equal("p", title.LocalName);
        Assert.Equal(["stil-alert-title"], title.ClassList);
        Assert.Equal("Heads up", title.TextContent);
        Assert.Equal("Body", alert.QuerySelector("span")?.TextContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OmitsEmptyTitle(string? title)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>(parameters => parameters.Add(component => component.Title, title));

        Assert.Empty(cut.FindAll(".stil-alert-title"));
    }

    [Fact]
    public void NotDismissibleByDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAlert>();

        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void InteractiveDismissibleAlertHidesAndRaisesOnDismiss()
    {
        using var context = new BunitContext();
        context.SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var dismissed = 0;

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Dismissible, true)
            .Add(component => component.OnDismiss, () => dismissed++)
            .AddChildContent("Saved"));

        var close = cut.Find("button.stil-close");
        Assert.Equal("button", close.GetAttribute("type"));
        Assert.Equal("Close", close.GetAttribute("aria-label"));
        // Interactive: Blazor removes the alert, so stil.js must not handle the click as well.
        Assert.False(close.HasAttribute("data-stil-dismiss"));

        close.Click();

        Assert.Empty(cut.FindAll(".stil-alert"));
        Assert.Equal(1, dismissed);
    }

    [Fact]
    public void StaticDismissibleAlertHasDataStilDismiss()
    {
        using var context = new BunitContext();
        context.SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Dismissible, true)
            .Add(component => component.CloseLabel, "Stäng"));

        var close = cut.Find("button.stil-close");
        Assert.True(close.HasAttribute("data-stil-dismiss"));
        Assert.Equal("", close.GetAttribute("data-stil-dismiss"));
        Assert.Equal("Stäng", close.GetAttribute("aria-label"));
    }
}
