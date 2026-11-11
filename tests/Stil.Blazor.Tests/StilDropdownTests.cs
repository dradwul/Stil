using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilDropdownTests
{
    [Fact]
    public void RendersDetailsWithSummaryToggleAndMenu()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Actions")
            .AddChildContent("<button class=\"stil-dropdown-item\">Edit</button>"));

        var details = cut.Find("details");
        Assert.Equal(["stil-dropdown"], details.ClassList);
        Assert.False(details.HasAttribute("open"));
        Assert.Equal(2, details.ChildElementCount);

        var summary = details.Children[0];
        Assert.Equal("summary", summary.LocalName);
        Assert.Equal(["stil-btn", "stil-btn-primary"], summary.ClassList);
        Assert.Equal("Actions", summary.TextContent.Trim());
        Assert.False(summary.HasAttribute("aria-label"));

        var menu = details.Children[1];
        Assert.Equal(["stil-dropdown-menu"], menu.ClassList);
        Assert.Equal("Edit", menu.QuerySelector(".stil-dropdown-item")?.TextContent);
    }

    [Fact]
    public void ToggleFragmentReplacesText()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Ignored")
            .Add(component => component.Toggle, "<span class=\"icon\">⋯</span>"));

        var summary = cut.Find("summary");
        Assert.NotNull(summary.QuerySelector("span.icon"));
        Assert.DoesNotContain("Ignored", summary.TextContent);
    }

    [Fact]
    public void ToggleClassesFollowVariantAppearanceSizeAndIconOnly()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Variant, StilVariant.Secondary)
            .Add(component => component.Appearance, StilButtonAppearance.Outline)
            .Add(component => component.Size, StilSize.Small)
            .Add(component => component.IconOnly, true)
            .Add(component => component.Label, "More actions"));

        var summary = cut.Find("summary");
        Assert.Equal(["stil-btn", "stil-btn-secondary", "stil-btn-outline", "stil-btn-sm", "stil-btn-icon"], summary.ClassList);
        Assert.Equal("More actions", summary.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(StilButtonAppearance.Ghost, "stil-btn-ghost")]
    [InlineData(StilButtonAppearance.Outline, "stil-btn-outline")]
    public void AppearanceAddsClass(StilButtonAppearance appearance, string expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters.Add(component => component.Appearance, appearance));

        Assert.Contains(expectedClass, cut.Find("summary").ClassList);
    }

    [Fact]
    public void AlignEndAddsEndClassToMenu()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters.Add(component => component.AlignEnd, true));

        Assert.Equal(["stil-dropdown-menu", "stil-dropdown-menu-end"], cut.Find("details > div").ClassList);
    }

    [Fact]
    public void MergesClassAndPassesAttributesThroughToDetails()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .AddUnmatched("class", "ms-auto")
            .AddUnmatched("id", "user-menu")
            .AddUnmatched("open", true));

        var details = cut.Find("details");
        Assert.Equal(["stil-dropdown", "ms-auto"], details.ClassList);
        Assert.Equal("user-menu", details.Id);
        Assert.True(details.HasAttribute("open"));
    }

    [Fact]
    public void RendersItemsHeadersAndDividers()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdown>(parameters => parameters
            .Add(component => component.Text, "Menu")
            .AddChildContent<StilDropdownHeader>(header => header.AddChildContent("Account"))
            .AddChildContent<StilDropdownItem>(item => item
                .Add(component => component.Href, "/profile")
                .AddChildContent("Profile"))
            .AddChildContent<StilDropdownDivider>(_ => { })
            .AddChildContent<StilDropdownItem>(item => item.AddChildContent("Sign out")));

        var menu = cut.Find(".stil-dropdown-menu");
        Assert.Equal(["div", "a", "hr", "button"], menu.Children.Select(child => child.LocalName));
        Assert.Equal(
            ["stil-dropdown-header", "stil-dropdown-item", "stil-dropdown-divider", "stil-dropdown-item"],
            menu.Children.Select(child => child.ClassName));
    }
}

public class StilDropdownItemTests
{
    [Fact]
    public void RendersButtonThatInvokesOnClick()
    {
        using var context = new BunitContext();
        var clicks = 0;

        var cut = context.Render<StilDropdownItem>(parameters => parameters
            .Add(component => component.OnClick, (MouseEventArgs _) => clicks++)
            .AddChildContent("Delete"));

        var item = cut.Find("button");
        Assert.Equal("button", item.GetAttribute("type"));
        Assert.Equal(["stil-dropdown-item"], item.ClassList);
        Assert.False(item.HasAttribute("disabled"));
        Assert.Equal("Delete", item.TextContent);

        item.Click();

        Assert.Equal(1, clicks);
    }

    [Fact]
    public void DisabledButtonRendersDisabled()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdownItem>(parameters => parameters.Add(component => component.Disabled, true));

        Assert.True(cut.Find("button.stil-dropdown-item").HasAttribute("disabled"));
    }

    [Fact]
    public void HrefRendersLink()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdownItem>(parameters => parameters
            .Add(component => component.Href, "/settings")
            .AddChildContent("Settings"));

        Assert.Empty(cut.FindAll("button"));
        var link = cut.Find("a.stil-dropdown-item");
        Assert.Equal("/settings", link.GetAttribute("href"));
        Assert.False(link.HasAttribute("aria-disabled"));
        Assert.Equal("Settings", link.TextContent);
    }

    [Fact]
    public void DisabledLinkRemovesHrefAndSetsAriaDisabled()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdownItem>(parameters => parameters
            .Add(component => component.Href, "/settings")
            .Add(component => component.Disabled, true));

        var link = cut.Find("a.stil-dropdown-item");
        Assert.False(link.HasAttribute("href"));
        Assert.Equal("true", link.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void MergesClassAndPassesAttributesThrough()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdownItem>(parameters => parameters
            .AddUnmatched("class", "stil-text-danger")
            .AddUnmatched("data-action", "delete"));

        var item = cut.Find("button");
        Assert.Equal(["stil-dropdown-item", "stil-text-danger"], item.ClassList);
        Assert.Equal("delete", item.GetAttribute("data-action"));
    }
}

public class StilDropdownHeaderTests
{
    [Fact]
    public void RendersHeaderWithContentAndMergedClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdownHeader>(parameters => parameters
            .AddChildContent("Signed in as Ada")
            .AddUnmatched("class", "small"));

        var header = cut.Find("div");
        Assert.Equal(["stil-dropdown-header", "small"], header.ClassList);
        Assert.Equal("Signed in as Ada", header.TextContent);
    }
}

public class StilDropdownDividerTests
{
    [Fact]
    public void RendersHorizontalRuleWithMergedClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDropdownDivider>(parameters => parameters
            .AddUnmatched("class", "my-1")
            .AddUnmatched("aria-hidden", "true"));

        var divider = cut.Find("hr");
        Assert.Equal(["stil-dropdown-divider", "my-1"], divider.ClassList);
        Assert.Equal("true", divider.GetAttribute("aria-hidden"));
    }
}
