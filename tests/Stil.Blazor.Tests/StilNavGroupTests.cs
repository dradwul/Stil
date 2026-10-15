using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilNavGroupTests
{
    [Fact]
    public void RendersDetailsWithHeaderTitleAndChevron()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNavGroup>(parameters => parameters
            .Add(component => component.Title, "Settings")
            .AddUnmatched("class", "custom")
            .AddUnmatched("id", "settings")
            .AddChildContent("<a class=\"stil-nav-link\" href=\"profile\">Profile</a>"));

        var details = cut.Find("details");
        Assert.Equal(["stil-nav-group", "custom"], details.ClassList);
        Assert.Equal("settings", details.Id);
        Assert.False(details.HasAttribute("open"));

        var summary = cut.Find("details > summary");
        Assert.Equal(["stil-nav-group-header"], summary.ClassList);
        Assert.Equal("Settings", summary.QuerySelector(".stil-nav-group-title")!.TextContent);
        var icons = summary.QuerySelectorAll("svg.stil-icon");
        var chevron = Assert.Single(icons);
        Assert.Contains("stil-nav-group-chevron", chevron.ClassList);
        Assert.Equal("true", chevron.GetAttribute("aria-hidden"));

        Assert.Equal("Profile", cut.Find("details > .stil-nav-group-body > a.stil-nav-link").TextContent);
    }

    [Fact]
    public void IconRendersBeforeTheTitle()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNavGroup>(parameters => parameters
            .Add(component => component.Title, "Settings")
            .Add(component => component.Icon, StilIcons.Home));

        var children = cut.Find("summary").Children;
        Assert.Equal(3, children.Length);
        Assert.Equal(["stil-icon"], children[0].ClassList);
        Assert.Contains("stil-nav-group-title", children[1].ClassList);
        Assert.Contains("stil-nav-group-chevron", children[2].ClassList);
    }

    [Fact]
    public void ExpandedRendersTheOpenAttribute()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilNavGroup>(parameters => parameters
            .Add(component => component.Title, "Settings")
            .Add(component => component.Expanded, true));

        Assert.True(cut.Find("details").HasAttribute("open"));
    }

    [Fact]
    public void ToggleEventUpdatesTheBinding()
    {
        using var context = new BunitContext();
        var expanded = false;
        var calls = 0;

        var cut = context.Render<StilNavGroup>(parameters => parameters
            .Add(component => component.Title, "Settings")
            .Add(component => component.Expanded, expanded)
            .Add(component => component.ExpandedChanged, (bool value) =>
            {
                expanded = value;
                calls++;
            }));

        cut.Find("details").TriggerEvent("onstiltoggle", new StilToggleEventArgs { Open = true });
        Assert.True(expanded);
        Assert.True(cut.Find("details").HasAttribute("open"));

        // The same state again (for example the toggle event after Blazor set open) is ignored.
        cut.Find("details").TriggerEvent("onstiltoggle", new StilToggleEventArgs { Open = true });
        Assert.Equal(1, calls);

        cut.Find("details").TriggerEvent("onstiltoggle", new StilToggleEventArgs { Open = false });
        Assert.False(expanded);
        Assert.False(cut.Find("details").HasAttribute("open"));
    }

    [Fact]
    public void WorksInsideAVerticalNavWithNavLinks()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<BunitNavigationManager>().NavigateTo("settings/profile");

        RenderFragment links = builder =>
        {
            builder.OpenComponent<StilNavLink>(0);
            builder.AddAttribute(1, nameof(StilNavLink.Href), "settings/profile");
            builder.AddAttribute(2, nameof(StilNavLink.ChildContent), (RenderFragment)(b => b.AddContent(0, "Profile")));
            builder.CloseComponent();
        };

        var cut = context.Render<StilNav>(parameters => parameters
            .Add(component => component.Vertical, true)
            .AddChildContent<StilNavGroup>(group => group
                .Add(component => component.Title, "Settings")
                .Add(component => component.ChildContent, links)));

        var link = cut.Find("nav.stil-nav-vertical > details.stil-nav-group > .stil-nav-group-body > a.stil-nav-link");
        Assert.Equal("page", link.GetAttribute("aria-current"));
    }

    private static RenderFragment Link(string href, string text) => builder =>
    {
        builder.OpenComponent<StilNavLink>(0);
        builder.AddComponentParameter(1, nameof(StilNavLink.Href), href);
        builder.AddComponentParameter(2, nameof(StilNavLink.ChildContent), (RenderFragment)(b => b.AddContent(0, text)));
        builder.CloseComponent();
    };

    [Theory]
    [InlineData("orders", true)]
    [InlineData("orders/42", true)]
    [InlineData("orders?page=2", true)]
    [InlineData("customers", false)]
    [InlineData("ordersarchive", false)]
    public void OpensWhenALinkIsTheCurrentPage(string location, bool open)
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo(location);
        var changes = new List<bool>();

        var cut = context.Render<StilNavGroup>(parameters => parameters
            .Add(component => component.Title, "Sales")
            .Add(component => component.ExpandedChanged, (bool value) => changes.Add(value))
            .AddChildContent(Link("orders", "Orders"))
            .AddChildContent(Link("invoices", "Invoices")));

        cut.WaitForAssertion(() => Assert.Equal(open, cut.Find("details").HasAttribute("open")));
        Assert.Equal(open ? [true] : [], changes);
    }

    [Fact]
    public void OpensWhenNavigatingToOneOfItsLinks()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var cut = context.Render<StilNavGroup>(parameters => parameters
            .Add(component => component.Title, "Sales")
            .AddChildContent(Link("invoices", "Invoices")));
        Assert.False(cut.Find("details").HasAttribute("open"));

        cut.InvokeAsync(() => navigation.NavigateTo("invoices"));

        cut.WaitForAssertion(() => Assert.True(cut.Find("details").HasAttribute("open")));
    }
}
