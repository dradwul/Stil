using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilNavTests
{
    [Fact]
    public void RendersNavLandmarkWithModifiers()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNav>(parameters => parameters
            .Add(component => component.Label, "Main")
            .Add(component => component.Vertical, true)
            .Add(component => component.Pills, true)
            .AddUnmatched("class", "stil-mb-4"));

        var nav = cut.Find("nav");
        Assert.Equal("Main", nav.GetAttribute("aria-label"));
        Assert.Equal(["stil-nav", "stil-nav-vertical", "stil-nav-pills", "stil-mb-4"], nav.ClassList);
    }

    [Fact]
    public void ItemsRenderNavLinks()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNav>(parameters => parameters
            .Add(component => component.Items, [new("Orders", "orders"), new("Customers", "customers")]));

        var links = cut.FindAll("a.stil-nav-link");
        Assert.Equal(["Orders", "Customers"], links.Select(link => link.TextContent.Trim()));
        Assert.Equal("orders", links[0].GetAttribute("href"));
    }
}

public class StilNavLinkTests
{
    [Fact]
    public void CurrentRouteGetsAriaCurrent()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<BunitNavigationManager>().NavigateTo("orders");

        var cut = context.Render<StilNavLink>(parameters => parameters
            .Add(component => component.Href, "orders")
            .AddChildContent("Orders"));

        var link = cut.Find("a");
        Assert.Contains("stil-nav-link", link.ClassList);
        Assert.Equal("page", link.GetAttribute("aria-current"));
    }

    [Fact]
    public void OtherRouteIsNotCurrent()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<BunitNavigationManager>().NavigateTo("customers");

        var cut = context.Render<StilNavLink>(parameters => parameters
            .Add(component => component.Href, "orders")
            .AddChildContent("Orders"));

        Assert.Null(cut.Find("a").GetAttribute("aria-current"));
    }

    [Fact]
    public void IconBadgeAndClassMerge()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNavLink>(parameters => parameters
            .Add(component => component.Href, "inbox")
            .Add(component => component.Icon, StilIcons.Info)
            .Add(component => component.Badge, "12")
            .Add(component => component.Match, NavLinkMatch.All)
            .AddUnmatched("class", "custom")
            .AddChildContent("Inbox"));

        var link = cut.Find("a");
        Assert.Contains("custom", link.ClassList);
        Assert.NotNull(link.QuerySelector("svg.stil-icon"));
        Assert.Equal("12", link.QuerySelector(".stil-badge")?.TextContent);
    }
}

public class StilNavbarTests
{
    [Fact]
    public void RendersBrandContentAndEnd()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNavbar>(parameters => parameters
            .Add(component => component.BrandText, "Nordvik")
            .Add(component => component.BrandHref, "/")
            .Add(component => component.End, (RenderFragment)(b => b.AddMarkupContent(0, "<button id=\"user\">User</button>")))
            .AddChildContent("<nav id=\"main\"></nav>"));

        var header = cut.Find("header");
        Assert.Equal(["stil-navbar"], header.ClassList);
        Assert.Equal("/", cut.Find("a.stil-navbar-brand").GetAttribute("href"));
        Assert.Equal("Nordvik", cut.Find("a.stil-navbar-brand").TextContent);
        Assert.NotNull(cut.Find(".stil-navbar-collapse > #main"));
        Assert.NotNull(cut.Find(".stil-navbar-collapse > .stil-navbar-end > #user"));
        Assert.Empty(cut.FindAll(".stil-navbar-toggle"));
    }

    [Fact]
    public void CollapseBelowAddsToggleThatControlsTheCollapse()
    {
        using var context = new BunitContext();
        context.SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        var cut = context.Render<StilNavbar>(parameters => parameters
            .Add(component => component.BrandText, "App")
            .Add(component => component.CollapseBelow, StilBreakpoint.Medium));

        Assert.Contains("stil-navbar-expand-md", cut.Find("header").ClassList);
        var toggle = cut.Find(".stil-navbar-toggle");
        var collapse = cut.Find(".stil-navbar-collapse");
        Assert.Equal(collapse.Id, toggle.GetAttribute("aria-controls"));
        Assert.Equal("false", toggle.GetAttribute("aria-expanded"));
        Assert.Equal("Menu", toggle.QuerySelector(".stil-visually-hidden")?.TextContent);

        toggle.Click();
        Assert.Equal("true", cut.Find(".stil-navbar-toggle").GetAttribute("aria-expanded"));

        cut.Find(".stil-navbar-toggle").Click();
        Assert.Equal("false", cut.Find(".stil-navbar-toggle").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void NavigatingClosesTheMenu()
    {
        using var context = new BunitContext();
        context.SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var cut = context.Render<StilNavbar>(parameters => parameters.Add(component => component.CollapseBelow, StilBreakpoint.Large));

        cut.Find(".stil-navbar-toggle").Click();
        context.Services.GetRequiredService<BunitNavigationManager>().NavigateTo("orders");

        cut.WaitForAssertion(() => Assert.Equal("false", cut.Find(".stil-navbar-toggle").GetAttribute("aria-expanded")));
    }

    [Fact]
    public void ToggleUsesStilJsWhenStaticallyRendered()
    {
        using var context = new BunitContext();
        context.SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = context.Render<StilNavbar>(parameters => parameters.Add(component => component.CollapseBelow, StilBreakpoint.Small));

        Assert.True(cut.Find(".stil-navbar-toggle").HasAttribute("data-stil-toggle"));
    }

    [Fact]
    public void ToggleLeavesStilJsOutWhenInteractive()
    {
        using var context = new BunitContext();
        context.SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        var cut = context.Render<StilNavbar>(parameters => parameters.Add(component => component.CollapseBelow, StilBreakpoint.Small));

        Assert.False(cut.Find(".stil-navbar-toggle").HasAttribute("data-stil-toggle"));
    }

    [Fact]
    public void StickyAndVariant()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNavbar>(parameters => parameters
            .Add(component => component.Sticky, true)
            .Add(component => component.Variant, StilVariant.Primary));

        Assert.Equal(["stil-navbar", "stil-sticky-top", "stil-bg-primary"], cut.Find("header").ClassList);
    }
}

public class StilDrawerTests
{
    [Fact]
    public void RendersDrawerDialogAtTheEndWithWidth()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<StilDrawer>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.Title, "Filters")
            .Add(component => component.Position, StilDrawerPosition.End)
            .Add(component => component.Width, "24rem")
            .AddChildContent("<p>Body</p>"));

        var dialog = cut.Find("dialog");
        Assert.Equal(["stil-drawer", "stil-drawer-end"], dialog.ClassList);
        Assert.Equal("--stil-drawer-width: 24rem", dialog.GetAttribute("style"));
        Assert.Equal("Filters", cut.Find(".stil-dialog-title").TextContent);
        Assert.Equal(cut.Find(".stil-dialog-title").Id, dialog.GetAttribute("aria-labelledby"));
        Assert.Equal("Body", cut.Find(".stil-dialog-body p").TextContent);
    }

    [Fact]
    public void OpensThroughStilJsWithItsOwnReference()
    {
        using var context = new BunitContext();
        var module = context.JSInterop.SetupModule("./_content/Stil/stil.js");
        module.SetupVoid("showDialog", _ => true);

        context.Render<StilDrawer>(parameters => parameters.Add(component => component.Open, true));

        var show = module.VerifyInvoke("showDialog");
        Assert.IsType<Microsoft.JSInterop.DotNetObjectReference<StilDrawer>>(show.Arguments[1]);
    }

    [Fact]
    public async Task CloseButtonUpdatesBindingAndRaisesOnClose()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var open = true;
        var closed = 0;

        var cut = context.Render<StilDrawer>(parameters => parameters
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, (bool value) => open = value)
            .Add(component => component.OnClose, () => closed++));

        await cut.Find(".stil-close").ClickAsync(new());

        Assert.False(open);
        Assert.Equal(1, closed);
        Assert.Empty(cut.FindAll(".stil-dialog-body"));
    }

    [Fact]
    public void DefaultsToStartWithoutStyle()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<StilDrawer>();

        var dialog = cut.Find("dialog");
        Assert.Equal(["stil-drawer"], dialog.ClassList);
        Assert.Null(dialog.GetAttribute("style"));
    }
}
