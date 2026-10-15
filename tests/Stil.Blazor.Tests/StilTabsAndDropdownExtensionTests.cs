using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilTabsExtensionTests
{
    private static RenderFragment Tab(string title, string id, string content) => builder =>
    {
        builder.OpenComponent<StilTab>(0);
        builder.AddComponentParameter(1, nameof(StilTab.Title), title);
        builder.AddComponentParameter(2, nameof(StilTab.Id), id);
        builder.AddComponentParameter(3, nameof(StilTab.ChildContent), (RenderFragment)(b => b.AddMarkupContent(0, $"<p>{content}</p>")));
        builder.CloseComponent();
    };

    private static RenderFragment ThreeTabs => builder =>
    {
        builder.AddContent(0, Tab("Profile", "profile", "Profile panel"));
        builder.AddContent(1, Tab("Security", "security", "Security panel"));
        builder.AddContent(2, Tab("Billing", "billing", "Billing panel"));
    };

    [Fact]
    public void ActiveIdSelectsTheTabWithThatId()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTabs>(parameters => parameters
            .Add(component => component.ActiveId, "security")
            .Add(component => component.ChildContent, ThreeTabs));

        Assert.Equal("Security", cut.Find("[role=tab][aria-selected=true]").TextContent.Trim());
        Assert.Equal("Security panel", cut.Find("[role=tabpanel]:not([hidden]) p").TextContent);
    }

    [Fact]
    public void SelectingRaisesActiveIdAndActiveIndexChanged()
    {
        using var context = new BunitContext();
        string? activeId = "profile";
        var activeIndex = 0;

        var cut = context.Render<StilTabs>(parameters => parameters
            .Add(component => component.ActiveId, activeId)
            .Add(component => component.ActiveIdChanged, (string? value) => activeId = value)
            .Add(component => component.ActiveIndexChanged, (int value) => activeIndex = value)
            .Add(component => component.ChildContent, ThreeTabs));

        cut.FindAll("[role=tab]")[2].Click();

        Assert.Equal("billing", activeId);
        Assert.Equal(2, activeIndex);
    }

    [Fact]
    public void UnknownActiveIdFallsBackToActiveIndex()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTabs>(parameters => parameters
            .Add(component => component.ActiveId, "missing")
            .Add(component => component.ActiveIndex, 1)
            .Add(component => component.ChildContent, ThreeTabs));

        Assert.Equal("Security", cut.Find("[role=tab][aria-selected=true]").TextContent.Trim());
    }

    [Fact]
    public void PillsAppearanceAddsTheClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTabs>(parameters => parameters
            .Add(component => component.Appearance, StilTabsAppearance.Pills)
            .Add(component => component.ChildContent, ThreeTabs));

        Assert.Equal(["stil-tabs", "stil-tabs-pills"], cut.Find("[role=tablist]").ClassList);
    }

    [Fact]
    public void KeepAliveRendersHiddenPanelsToo()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTabs>(parameters => parameters
            .Add(component => component.KeepAlive, true)
            .Add(component => component.ChildContent, ThreeTabs));

        var panels = cut.FindAll("[role=tabpanel]");
        Assert.Equal(3, panels.Count);
        Assert.All(panels, panel => Assert.NotNull(panel.QuerySelector("p")));
        Assert.Equal(2, panels.Count(panel => panel.HasAttribute("hidden")));
    }

    [Fact]
    public void WithoutKeepAliveOnlyTheSelectedPanelHasContent()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTabs>(parameters => parameters.Add(component => component.ChildContent, ThreeTabs));

        Assert.Single(cut.FindAll("[role=tabpanel] p"));
    }
}

public class StilDropdownExtensionTests
{
    [Fact]
    public void OpenRendersTheOpenAttribute()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Menu")
            .Add(component => component.Open, true));

        Assert.True(cut.Find("details").HasAttribute("open"));
    }

    [Fact]
    public void ToggleEventUpdatesTheBinding()
    {
        using var context = new BunitContext();
        var open = false;

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Menu")
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, (bool value) => open = value));

        cut.Find("details").TriggerEvent("onstiltoggle", new StilToggleEventArgs { Open = true });
        Assert.True(open);

        cut.Find("details").TriggerEvent("onstiltoggle", new StilToggleEventArgs { Open = false });
        Assert.False(open);
    }

    [Fact]
    public void DisabledKeepsTheMenuClosedAndTheToggleUnfocusable()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Menu")
            .Add(component => component.Open, true)
            .Add(component => component.Disabled, true));

        Assert.False(cut.Find("details").HasAttribute("open"));
        var summary = cut.Find("summary");
        Assert.Equal("true", summary.GetAttribute("aria-disabled"));
        Assert.Equal("-1", summary.GetAttribute("tabindex"));
    }

    [Fact]
    public void DropUpAddsTheMenuClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Menu")
            .Add(component => component.DropUp, true)
            .Add(component => component.AlignEnd, true));

        Assert.Equal(["stil-dropdown-menu", "stil-dropdown-menu-end", "stil-dropdown-menu-up"], cut.Find(".stil-dropdown-menu").ClassList);
    }

    [Fact]
    public void ItemsRenderHeadersDividersLinksAndActions()
    {
        using var context = new BunitContext();
        var edited = 0;

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Actions")
            .Add(component => component.Items,
            [
                StilMenuItem.Header("Order #1043"),
                StilMenuItem.Action("Edit", () => edited++),
                StilMenuItem.Link("Open", "orders/1043"),
                StilMenuItem.Divider,
                StilMenuItem.Action("Archive", () => { }, disabled: true),
            ]));

        Assert.Equal("Order #1043", cut.Find(".stil-dropdown-header").TextContent);
        Assert.Single(cut.FindAll("hr.stil-dropdown-divider"));
        Assert.Equal("orders/1043", cut.Find("a.stil-dropdown-item").GetAttribute("href"));
        Assert.True(cut.FindAll("button.stil-dropdown-item")[1].HasAttribute("disabled"));

        cut.FindAll("button.stil-dropdown-item")[0].Click();
        Assert.Equal(1, edited);
    }

    [Fact]
    public async Task AsyncActionItemsAreAwaited()
    {
        using var context = new BunitContext();
        var done = false;

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Actions")
            .Add(component => component.Items, [StilMenuItem.Action("Save", async () => { await Task.Yield(); done = true; })]));

        await cut.Find("button.stil-dropdown-item").ClickAsync(new());

        Assert.True(done);
    }
}
