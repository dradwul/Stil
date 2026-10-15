using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilAccordionTests
{
    private static RenderFragment Item(string title, bool open = false) => builder =>
    {
        builder.OpenComponent<StilAccordionItem>(0);
        builder.AddComponentParameter(1, nameof(StilAccordionItem.Title), title);
        builder.AddComponentParameter(2, nameof(StilAccordionItem.Open), open);
        builder.AddComponentParameter(3, nameof(StilAccordionItem.ChildContent), (RenderFragment)(b => b.AddMarkupContent(0, $"<p>{title} body</p>")));
        builder.CloseComponent();
    };

    [Fact]
    public void ItemsShareANameSoOnlyOneOpensAtATime()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAccordion>(parameters => parameters
            .AddChildContent(Item("Shipping"))
            .AddChildContent(Item("Returns", open: true)));

        Assert.Equal(["stil-accordion"], cut.Find("div").ClassList);
        var items = cut.FindAll("details.stil-accordion-item");
        Assert.Equal(2, items.Count);
        Assert.NotNull(items[0].GetAttribute("name"));
        Assert.Equal(items[0].GetAttribute("name"), items[1].GetAttribute("name"));
        Assert.False(items[0].HasAttribute("open"));
        Assert.True(items[1].HasAttribute("open"));
        Assert.Equal("Shipping", items[0].QuerySelector("summary.stil-accordion-header")?.TextContent);
        Assert.Equal("Returns body", items[1].QuerySelector(".stil-accordion-body p")?.TextContent);
    }

    [Fact]
    public void MultipleLeavesTheNameOut()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAccordion>(parameters => parameters
            .Add(component => component.Multiple, true)
            .Add(component => component.Flush, true)
            .AddChildContent(Item("A"))
            .AddChildContent(Item("B")));

        Assert.Contains("stil-accordion-flush", cut.Find("div").ClassList);
        Assert.All(cut.FindAll("details"), details => Assert.False(details.HasAttribute("name")));
    }

    [Fact]
    public void ToggleEventUpdatesTheBinding()
    {
        using var context = new BunitContext();
        var open = false;

        var cut = context.Render<StilAccordionItem>(parameters => parameters
            .Add(component => component.Title, "FAQ")
            .Add(component => component.Open, open)
            .Add(component => component.OpenChanged, (bool value) => open = value));

        cut.Find("details").TriggerEvent("onstiltoggle", new StilToggleEventArgs { Open = true });

        Assert.True(open);
        Assert.True(cut.Find("details").HasAttribute("open"));
    }

    [Fact]
    public void HeaderFragmentReplacesTitle()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAccordionItem>(parameters => parameters
            .Add(component => component.Title, "Plain")
            .Add(component => component.Header, (RenderFragment)(b => b.AddMarkupContent(0, "<strong>Rich</strong>"))));

        Assert.Equal("Rich", cut.Find("summary strong").TextContent);
    }
}

public class StilAvatarTests
{
    [Theory]
    [InlineData("Anna Berg", "AB")]
    [InlineData("anna maria berg", "AB")]
    [InlineData("Cher", "C")]
    [InlineData("  ", "")]
    public void InitialsComeFromTheFirstAndLastWord(string name, string initials)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAvatar>(parameters => parameters.Add(component => component.Name, name));

        Assert.Equal(initials, cut.Find(".stil-avatar span").TextContent);
    }

    [Fact]
    public void NameIsTheAccessibleNameAndSizeAddsAClass()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAvatar>(parameters => parameters
            .Add(component => component.Name, "Anna Berg")
            .Add(component => component.Size, StilSize.Large));

        var avatar = cut.Find(".stil-avatar");
        Assert.Equal("img", avatar.GetAttribute("role"));
        Assert.Equal("Anna Berg", avatar.GetAttribute("aria-label"));
        Assert.Contains("stil-avatar-lg", avatar.ClassList);
        Assert.Equal("true", avatar.QuerySelector("span")?.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void ImageReplacesInitials()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAvatar>(parameters => parameters
            .Add(component => component.Name, "Anna Berg")
            .Add(component => component.ImageSrc, "anna.jpg"));

        var image = cut.Find(".stil-avatar img");
        Assert.Equal("anna.jpg", image.GetAttribute("src"));
        Assert.Equal("", image.GetAttribute("alt"));
        Assert.Empty(cut.FindAll(".stil-avatar span"));
    }
}

public class StilTableTests
{
    private sealed record Order(int Id, string Customer, decimal Amount);

    private static readonly Order[] Orders = [new(1042, "Fjällbacka Café", 1240m), new(1043, "Berglund Outdoor", 2078.9m)];

    private static RenderFragment Column(string title, Func<Order, object?>? field = null, StilTextAlign align = StilTextAlign.Start,
        string? format = null, RenderFragment<Order>? template = null) => builder =>
    {
        builder.OpenComponent<StilColumn<Order>>(0);
        builder.AddComponentParameter(1, nameof(StilColumn<Order>.Title), title);
        builder.AddComponentParameter(2, nameof(StilColumn<Order>.Field), field);
        builder.AddComponentParameter(3, nameof(StilColumn<Order>.Align), align);
        builder.AddComponentParameter(4, nameof(StilColumn<Order>.Format), format);
        builder.AddComponentParameter(5, nameof(StilColumn<Order>.Template), template);
        builder.CloseComponent();
    };

    private static RenderFragment OrderColumns => builder =>
    {
        builder.AddContent(0, Column("Order", o => $"#{o.Id}"));
        builder.AddContent(1, Column("Customer", o => o.Customer));
        builder.AddContent(2, Column("Amount", o => o.Amount, StilTextAlign.End, "0.00"));
    };

    [Fact]
    public void RendersHeadersRowsAndFormattedCells()
    {
        using var context = new BunitContext();
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        var cut = context.Render<StilTable<Order>>(parameters => parameters
            .Add(component => component.Items, Orders)
            .Add(component => component.Columns, OrderColumns)
            .Add(component => component.Caption, "Recent orders"));

        Assert.NotNull(cut.Find(".stil-table-responsive > table.stil-table"));
        Assert.Equal("Recent orders", cut.Find("caption").TextContent);
        Assert.Equal(["Order", "Customer", "Amount"], cut.FindAll("thead th").Select(th => th.TextContent));
        Assert.All(cut.FindAll("thead th"), th => Assert.Equal("col", th.GetAttribute("scope")));

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        Assert.Equal(["#1043", "Berglund Outdoor", "2078.90"], rows[1].QuerySelectorAll("td").Select(td => td.TextContent));
        Assert.Contains("stil-text-end", rows[1].QuerySelectorAll("td")[2].ClassList);
        Assert.Contains("stil-text-end", cut.FindAll("thead th")[2].ClassList);
    }

    [Fact]
    public void StyleFlagsAddClasses()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTable<Order>>(parameters => parameters
            .Add(component => component.Items, Orders)
            .Add(component => component.Columns, OrderColumns)
            .Add(component => component.Striped, true)
            .Add(component => component.Hover, true)
            .Add(component => component.Compact, true)
            .Add(component => component.Bordered, true)
            .Add(component => component.Responsive, false));

        Assert.Empty(cut.FindAll(".stil-table-responsive"));
        Assert.Equal(["stil-table", "stil-table-striped", "stil-table-hover", "stil-table-compact", "stil-table-bordered"],
            cut.Find("table").ClassList);
    }

    [Fact]
    public void EmptyItemsShowEmptyContentAcrossAllColumns()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTable<Order>>(parameters => parameters
            .Add(component => component.Items, Array.Empty<Order>())
            .Add(component => component.Columns, OrderColumns));

        var cell = cut.Find("tbody td");
        Assert.Equal("3", cell.GetAttribute("colspan"));
        Assert.Equal("No items", cell.TextContent.Trim());
    }

    [Fact]
    public void TemplateColumnAndRowClick()
    {
        using var context = new BunitContext();
        Order? clicked = null;

        var cut = context.Render<StilTable<Order>>(parameters => parameters
            .Add(component => component.Items, Orders)
            .Add(component => component.OnRowClick, (Order order) => clicked = order)
            .Add(component => component.RowClass, (Func<Order, string?>)(o => o.Amount > 2000 ? "big" : null))
            .Add(component => component.Columns, (RenderFragment)(builder =>
                builder.AddContent(0, Column("Status", template: order => b => b.AddMarkupContent(0, $"<span class=\"badge\">{order.Id}</span>"))))));

        Assert.Contains("stil-table-hover", cut.Find("table").ClassList);
        Assert.Equal("1042", cut.Find("tbody .badge").TextContent);
        Assert.Contains("big", cut.FindAll("tbody tr")[1].ClassList);

        cut.FindAll("tbody tr")[1].Click();
        Assert.Equal(1043, clicked?.Id);
    }

    [Fact]
    public void ColumnOutsideTableThrows()
    {
        using var context = new BunitContext();

        Assert.Throws<InvalidOperationException>(() => context.Render<StilColumn<Order>>());
    }
}

public class StilLayoutComponentTests
{
    [Fact]
    public void GridAndColumns()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilGrid>(parameters => parameters
            .Add(component => component.Gap, 6)
            .AddChildContent<StilCol>(col => col.Add(c => c.Span, 12).Add(c => c.SpanMd, 6).Add(c => c.SpanXl, 4)));

        Assert.Equal(["stil-grid", "stil-gap-6"], cut.Find(".stil-grid").ClassList);
        Assert.Equal(["stil-col-12", "stil-col-md-6", "stil-col-xl-4"], cut.Find(".stil-grid > div").ClassList);
    }

    [Fact]
    public void SpansAndGapsAreClamped()
    {
        using var context = new BunitContext();

        var col = context.Render<StilCol>(parameters => parameters.Add(c => c.Span, 20).Add(c => c.SpanSm, 0));
        Assert.Equal(["stil-col-12", "stil-col-sm-1"], col.Find("div").ClassList);

        var grid = context.Render<StilGrid>(parameters => parameters.Add(c => c.Gap, 99));
        Assert.Contains("stil-gap-7", grid.Find("div").ClassList);
    }

    [Fact]
    public void StackAndCluster()
    {
        using var context = new BunitContext();

        var stack = context.Render<StilStack>(parameters => parameters.Add(c => c.Gap, 2).Add(c => c.Align, StilAlign.Start));
        Assert.Equal(["stil-stack", "stil-gap-2", "stil-align-items-start"], stack.Find("div").ClassList);

        var cluster = context.Render<StilCluster>(parameters => parameters
            .Add(c => c.Justify, StilJustify.Between)
            .Add(c => c.Align, StilAlign.Baseline));
        Assert.Equal(["stil-cluster", "stil-justify-content-between", "stil-align-items-baseline"], cluster.Find("div").ClassList);
    }

    [Fact]
    public void SidebarLayout()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSidebarLayout>(parameters => parameters
            .Add(c => c.SidebarWidth, "18rem")
            .Add(c => c.SidebarLabel, "Sections")
            .Add(c => c.Sidebar, (RenderFragment)(b => b.AddMarkupContent(0, "<nav id=\"side\"></nav>")))
            .AddChildContent("<main id=\"content\"></main>"));

        var layout = cut.Find(".stil-with-sidebar");
        Assert.Equal("--stil-sidebar-width: 18rem", layout.GetAttribute("style"));
        Assert.Equal("Sections", cut.Find("aside").GetAttribute("aria-label"));
        Assert.NotNull(cut.Find("aside > #side"));
        Assert.NotNull(cut.Find(".stil-with-sidebar > div > #content"));
    }

    [Fact]
    public void ButtonGroupIsALabelledGroup()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilButtonGroup>(parameters => parameters
            .Add(c => c.Label, "Alignment")
            .AddChildContent<StilButton>(b => b.AddChildContent("Left")));

        var group = cut.Find(".stil-btn-group");
        Assert.Equal("group", group.GetAttribute("role"));
        Assert.Equal("Alignment", group.GetAttribute("aria-label"));
        Assert.NotNull(group.QuerySelector("button.stil-btn"));
    }

    [Fact]
    public void CloseButton()
    {
        using var context = new BunitContext();
        var clicks = 0;

        var cut = context.Render<StilCloseButton>(parameters => parameters
            .Add(c => c.Label, "Stäng")
            .Add(c => c.OnClick, () => clicks++));

        var button = cut.Find("button.stil-close");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Equal("Stäng", button.GetAttribute("aria-label"));
        button.Click();
        Assert.Equal(1, clicks);
    }
}

public class StilInputAddonTests
{
    [Fact]
    public void TextAddonsWrapTheControlInAnInputGroup()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilTextBox>(parameters => parameters
            .Add(c => c.Value, value)
            .Add(c => c.Label, "Website")
            .Add(c => c.PrependText, "https://")
            .Add(c => c.AppendText, ".se"));

        var group = cut.Find(".stil-field > .stil-input-group");
        var children = group.Children;
        Assert.Equal(3, children.Length);
        Assert.Equal("https://", children[0].TextContent);
        Assert.Contains("stil-input-group-text", children[0].ClassList);
        Assert.Equal("INPUT", children[1].TagName);
        Assert.Equal(".se", children[2].TextContent);
        Assert.NotNull(cut.Find("label.stil-label"));
    }

    [Fact]
    public void FragmentAddonsRenderAsIs()
    {
        using var context = new BunitContext();
        var amount = 0m;

        var cut = context.Render<StilNumeric<decimal>>(parameters => parameters
            .Add(c => c.Value, amount)
            .Add(c => c.Append, (RenderFragment)(b => b.AddMarkupContent(0, "<button id=\"max\">Max</button>"))));

        Assert.Equal("max", cut.Find(".stil-input-group > :last-child").Id);
    }

    [Fact]
    public void WithoutAddonsThereIsNoGroup()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSelect<string>>(parameters => parameters
            .Add(c => c.Items, ["a", "b"])
            .Add(c => c.Value, "a"));

        Assert.Empty(cut.FindAll(".stil-input-group"));
        Assert.NotNull(cut.Find(".stil-field > select"));
    }

    [Fact]
    public void SelectAndDatePickerSupportAddons()
    {
        using var context = new BunitContext();

        var select = context.Render<StilSelect<string>>(parameters => parameters
            .Add(c => c.Items, ["SEK", "EUR"])
            .Add(c => c.Value, "SEK")
            .Add(c => c.PrependText, "Currency"));
        Assert.NotNull(select.Find(".stil-input-group > select"));

        var date = context.Render<StilDatePicker<DateOnly>>(parameters => parameters
            .Add(c => c.Value, new DateOnly(2026, 10, 15))
            .Add(c => c.AppendText, "UTC"));
        Assert.NotNull(date.Find(".stil-input-group > input[type=date]"));
    }
}
