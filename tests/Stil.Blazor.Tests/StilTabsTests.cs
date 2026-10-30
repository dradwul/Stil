using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilTabsTests
{
    private static IReadOnlyList<IElement> Tabs(IRenderedComponent<TabsHost> cut) => cut.FindAll("[role=tab]");

    private static int SelectedIndex(IRenderedComponent<TabsHost> cut) =>
        Tabs(cut).Select((tab, index) => (tab, index)).Single(t => t.tab.GetAttribute("aria-selected") == "true").index;

    [Fact]
    public void RendersTablistWithOneTabPerStilTab()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();

        var tablist = cut.Find("[role=tablist]");
        Assert.Equal(["stil-tabs"], tablist.ClassList);
        Assert.Equal("Sections", tablist.GetAttribute("aria-label"));

        var tabs = Tabs(cut);
        Assert.Equal(["One", "Two", "Three"], tabs.Select(tab => tab.TextContent.Trim()));
        Assert.All(tabs, tab =>
        {
            Assert.Equal("button", tab.LocalName);
            Assert.Equal("button", tab.GetAttribute("type"));
            Assert.Equal(["stil-tab"], tab.ClassList);
        });
    }

    [Fact]
    public void MarksActiveTabSelectedWithRovingTabindex()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>(parameters => parameters.Add(component => component.ActiveIndex, 2));

        var tabs = Tabs(cut);
        Assert.Equal(["false", "false", "true"], tabs.Select(tab => tab.GetAttribute("aria-selected")));
        Assert.Equal(["-1", "-1", "0"], tabs.Select(tab => tab.GetAttribute("tabindex")));
    }

    [Fact]
    public void TabsAndPanelsReferenceEachOther()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();

        var tabs = Tabs(cut);
        var panels = cut.FindAll("[role=tabpanel]");
        Assert.Equal(tabs.Count, panels.Count);
        Assert.Equal(tabs.Count, tabs.Select(tab => tab.Id).Distinct().Count());

        for (var i = 0; i < tabs.Count; i++)
        {
            Assert.False(string.IsNullOrEmpty(tabs[i].Id));
            Assert.Equal(panels[i].Id, tabs[i].GetAttribute("aria-controls"));
            Assert.Equal(tabs[i].Id, panels[i].GetAttribute("aria-labelledby"));
            Assert.Equal("0", panels[i].GetAttribute("tabindex"));
        }
    }

    [Fact]
    public void TwoTabsComponentsGetDifferentIds()
    {
        using var context = new BunitContext();

        var first = context.Render<TabsHost>();
        var second = context.Render<TabsHost>();

        Assert.NotEqual(Tabs(first)[0].Id, Tabs(second)[0].Id);
    }

    [Fact]
    public void RendersOnlyTheActivePanelContent()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>(parameters => parameters.Add(component => component.ActiveIndex, 1));

        var panels = cut.FindAll("[role=tabpanel]");
        Assert.Equal([true, false, true], panels.Select(panel => panel.HasAttribute("hidden")));
        Assert.Equal(["", "Panel two", ""], panels.Select(panel => panel.TextContent.Trim()));
    }

    [Fact]
    public void ClickingTabSelectsItAndRaisesActiveIndexChanged()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();

        Tabs(cut)[2].Click();

        Assert.Equal(2, SelectedIndex(cut));
        Assert.Equal(2, cut.Instance.ActiveIndex);
        Assert.Equal([2], cut.Instance.Changes);
        Assert.Equal("Panel three", cut.Find("[role=tabpanel]:not([hidden])").TextContent.Trim());
    }

    [Fact]
    public void ClickingSelectedTabDoesNotRaiseActiveIndexChanged()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();

        Tabs(cut)[0].Click();

        Assert.Empty(cut.Instance.Changes);
    }

    [Fact]
    public void ActiveIndexChangedFiresWithoutTwoWayBinding()
    {
        using var context = new BunitContext();
        var changes = new List<int>();

        var cut = context.Render<StilTabs>(parameters => parameters
            .Add(component => component.ActiveIndexChanged, (int index) => changes.Add(index))
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "A").AddChildContent("Panel A"))
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "B").AddChildContent("Panel B")));

        cut.FindAll("[role=tab]")[1].Click();

        Assert.Equal([1], changes);
        Assert.Equal("true", cut.FindAll("[role=tab]")[1].GetAttribute("aria-selected"));
        Assert.Equal("Panel B", cut.Find("[role=tabpanel]:not([hidden])").TextContent.Trim());
    }

    [Fact]
    public void DisabledTabRendersDisabledAndCannotBeSelected()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>(parameters => parameters.Add(component => component.DisableSecond, true));

        Assert.True(Tabs(cut)[1].HasAttribute("disabled"));
        Assert.False(Tabs(cut)[0].HasAttribute("disabled"));
    }

    [Theory]
    [InlineData(0, "ArrowRight", 1)]
    [InlineData(1, "ArrowRight", 2)]
    [InlineData(2, "ArrowRight", 0)]
    [InlineData(0, "ArrowLeft", 2)]
    [InlineData(2, "ArrowLeft", 1)]
    [InlineData(1, "Home", 0)]
    [InlineData(0, "End", 2)]
    public void ArrowHomeAndEndKeysMoveSelection(int start, string key, int expected)
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>(parameters => parameters.Add(component => component.ActiveIndex, start));

        Tabs(cut)[start].KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(expected, SelectedIndex(cut));
        Assert.Equal(expected, cut.Instance.ActiveIndex);
        Assert.Equal("0", Tabs(cut)[expected].GetAttribute("tabindex"));
    }

    [Theory]
    [InlineData(0, "ArrowLeft", 1)]
    [InlineData(1, "ArrowRight", 0)]
    public void ArrowKeysFollowTheReadingDirectionInRightToLeftCultures(int start, string key, int expected) =>
        StilLocalizationTests.InCulture("ar-SA", () =>
        {
            using var context = new BunitContext();

            var cut = context.Render<TabsHost>(parameters => parameters.Add(component => component.ActiveIndex, start));

            Tabs(cut)[start].KeyDown(new KeyboardEventArgs { Key = key });

            Assert.Equal(expected, SelectedIndex(cut));
        });

    [Theory]
    [InlineData(0, "ArrowRight", 2)]
    [InlineData(2, "ArrowLeft", 0)]
    [InlineData(2, "ArrowRight", 0)]
    [InlineData(0, "ArrowLeft", 2)]
    public void ArrowKeysSkipDisabledTabs(int start, string key, int expected)
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>(parameters => parameters
            .Add(component => component.ActiveIndex, start)
            .Add(component => component.DisableSecond, true));

        Tabs(cut)[start].KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(expected, SelectedIndex(cut));
    }

    [Fact]
    public void HomeAndEndSkipDisabledTabs()
    {
        using var context = new BunitContext();

        // Four tabs, the second disabled; Home from the last goes to the first enabled tab.
        var cut = context.Render<StilTabs>(parameters => parameters
            .Add(component => component.ActiveIndex, 2)
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "A").Add(component => component.Disabled, true))
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "B"))
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "C"))
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "D").Add(component => component.Disabled, true)));

        cut.FindAll("[role=tab]")[2].KeyDown(new KeyboardEventArgs { Key = "Home" });
        Assert.Equal("true", cut.FindAll("[role=tab]")[1].GetAttribute("aria-selected"));

        cut.FindAll("[role=tab]")[1].KeyDown(new KeyboardEventArgs { Key = "End" });
        Assert.Equal("true", cut.FindAll("[role=tab]")[2].GetAttribute("aria-selected"));
    }

    [Fact]
    public void KeyboardSelectionMovesFocusToTheNewTab()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();

        Tabs(cut)[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        context.JSInterop.VerifyFocusAsyncInvoke();
    }

    [Fact]
    public void OtherKeysDoNothing()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();

        Tabs(cut)[0].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Tabs(cut)[0].KeyDown(new KeyboardEventArgs { Key = "a" });

        Assert.Equal(0, SelectedIndex(cut));
        Assert.Empty(cut.Instance.Changes);
        context.JSInterop.VerifyNotInvoke("Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void ParentCanChangeActiveIndex()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();
        cut.Render(parameters => parameters.Add(component => component.ActiveIndex, 2));

        Assert.Equal(2, SelectedIndex(cut));
        Assert.Equal("Panel three", cut.Find("[role=tabpanel]:not([hidden])").TextContent.Trim());
    }

    [Fact]
    public void TabAddedAtTheEndAppearsLast()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>();
        cut.Render(parameters => parameters.Add(component => component.ShowFourth, true));

        Assert.Equal(["One", "Two", "Three", "Four"], Tabs(cut).Select(tab => tab.TextContent.Trim()));
    }

    [Fact]
    public void RemovedTabDisappearsFromTablist()
    {
        using var context = new BunitContext();

        var cut = context.Render<TabsHost>(parameters => parameters.Add(component => component.ShowFourth, true));
        cut.Render(parameters => parameters.Add(component => component.ShowFourth, false));

        Assert.Equal(["One", "Two", "Three"], Tabs(cut).Select(tab => tab.TextContent.Trim()));
    }

    [Fact]
    public void TabAddedInTheMiddleKeepsMarkupOrder()
    {
        using var context = new BunitContext();

        var cut = context.Render<ConditionalTabsHost>();
        cut.Render(parameters => parameters.Add(component => component.ShowMiddle, true));

        Assert.Equal(["First", "Middle", "Last"], cut.FindAll("[role=tab]").Select(tab => tab.TextContent.Trim()));
    }

    [Fact]
    public void TabTitleChangeUpdatesTabButton()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTabs>(parameters => parameters
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "Before")));

        cut.Render(parameters => parameters
            .AddChildContent<StilTab>(tab => tab.Add(component => component.Title, "After")));

        Assert.Equal("After", cut.Find("[role=tab]").TextContent.Trim());
    }

    [Fact]
    public void MergesClassAndPassesAttributesThroughToWrapper()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTabs>(parameters => parameters
            .AddUnmatched("class", "settings-tabs")
            .AddUnmatched("id", "settings"));

        var wrapper = cut.Find("#settings");
        Assert.Equal(["settings-tabs"], wrapper.ClassList);
        Assert.NotNull(wrapper.QuerySelector("[role=tablist]"));
    }

    [Fact]
    public void TabOutsideTabsThrows()
    {
        using var context = new BunitContext();

        var error = Assert.Throws<InvalidOperationException>(() =>
            context.Render<StilTab>(parameters => parameters.Add(component => component.Title, "Orphan")));

        Assert.Contains("StilTabs", error.Message);
    }
}
