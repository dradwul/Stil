using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Blocks;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilSiteBlocksTests
{
    [Fact]
    public void MegaMenuIsADropdownWithColumnsOfLinks()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilMegaMenu>(parameters => parameters
            .Add(c => c.Text, "Product")
            .Add(c => c.Footer, (RenderFragment)(builder => builder.AddMarkupContent(0, "<p>New: time off.</p>")))
            .AddChildContent<StilMegaMenuColumn>(column => column
                .Add(c => c.Title, "Plan")
                .AddChildContent<StilMegaMenuLink>(link => link
                    .Add(c => c.Href, "week")
                    .Add(c => c.Title, "The week")
                    .Add(c => c.Text, "Everything in one calendar.")
                    .Add(c => c.Icon, StilIcons.Folder))));

        var details = cut.Find("details");
        Assert.Equal("stil-dropdown stil-mega", details.ClassName);
        Assert.Equal("Product", details.QuerySelector("summary.stil-nav-link")!.TextContent);
        Assert.Equal("stil-dropdown-menu stil-mega-menu", details.QuerySelector("summary + div")!.ClassName);
        Assert.Equal("Plan", cut.Find(".stil-mega-columns > .stil-mega-column > .stil-mega-title").TextContent);
        var link = cut.Find("a.stil-dropdown-item.stil-mega-link");
        Assert.Equal("week", link.GetAttribute("href"));
        Assert.Equal("The week Everything in one calendar.", string.Join(' ', link.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)));
        Assert.Equal("true", link.QuerySelector("svg")!.GetAttribute("aria-hidden"));
        Assert.Equal("New: time off.", cut.Find(".stil-mega-footer > p").TextContent);
    }

    [Fact]
    public void MegaMenuCanAlignWithTheEndOfItsToggle()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilMegaMenu>(parameters => parameters.Add(c => c.Text, "More").Add(c => c.AlignEnd, true));

        Assert.Contains("stil-dropdown-menu-end", cut.Find(".stil-mega-menu").ClassName);
    }

    [Fact]
    public void FooterHasANamedNavigationOfColumnsAndABottomRow()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFooter>(parameters => parameters
            .Add(c => c.BrandText, "Tidvy")
            .Add(c => c.Text, "Plan the week.")
            .Add(c => c.Copyright, "© 2026 Tidvy Oy")
            .Add(c => c.Legal, [new StilLinkItem("Privacy", "privacy")])
            .AddChildContent<StilFooterColumn>(column => column
                .Add(c => c.Title, "Product")
                .Add(c => c.Links, [new StilLinkItem("Pricing", "pricing"), new StilLinkItem("Questions", "faq")])));

        Assert.NotNull(cut.Find("footer.stil-footer > .stil-container > .stil-footer-main"));
        var brand = cut.Find(".stil-footer-about > a.stil-footer-brand");
        Assert.Equal("Tidvy", brand.TextContent);
        Assert.Equal("", brand.GetAttribute("href"));
        Assert.Equal("Plan the week.", cut.Find(".stil-footer-about > p").TextContent);
        var nav = cut.Find("nav.stil-footer-columns");
        Assert.Equal("Footer", nav.GetAttribute("aria-label"));
        Assert.Equal("Product", nav.QuerySelector("h2.stil-footer-title")!.TextContent);
        Assert.Equal(["pricing", "faq"], nav.QuerySelectorAll(".stil-footer-links a").Select(a => a.GetAttribute("href")));
        Assert.Equal("© 2026 Tidvy Oy", cut.Find(".stil-footer-bottom > p").TextContent);
        Assert.Equal("privacy", cut.Find(".stil-footer-legal a").GetAttribute("href"));
    }

    [Fact]
    public void FooterWithoutColumnsHasNoNavigation()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFooter>(parameters => parameters.Add(c => c.BrandText, "Tidvy"));

        Assert.Empty(cut.FindAll("nav"));
        Assert.Empty(cut.FindAll(".stil-footer-bottom"));
    }

    [Fact]
    public void ContactListsTheWaysToReachYouBesideTheForm()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilContact>(parameters => parameters
            .Add(c => c.Title, "Talk to us")
            .Add(c => c.Email, "hello@tidvy.example")
            .Add(c => c.Phone, "+358 20 123 4567")
            .Add(c => c.Address, "Tidvy Oy\nLinnankatu 12\n20100 Turku")
            .Add(c => c.MapHref, "https://www.openstreetmap.org/")
            .Add(c => c.Hours, "Weekdays 9–17")
            .AddChildContent("<form><button>Send</button></form>"));

        Assert.Equal("Talk to us", cut.Find(".stil-contact-info > h2.stil-section-title").TextContent);
        var labels = cut.FindAll("dl.stil-contact-details dt.stil-contact-label").Select(dt => dt.TextContent.Trim());
        Assert.Equal(["E-mail", "Phone", "Address", "Opening hours"], labels);
        Assert.All(cut.FindAll("dt svg"), icon => Assert.Equal("true", icon.GetAttribute("aria-hidden")));
        var values = cut.FindAll("dd.stil-contact-value");
        Assert.Equal("mailto:hello@tidvy.example", values[0].QuerySelector("a")!.GetAttribute("href"));
        Assert.Equal("tel:+358201234567", values[1].QuerySelector("a")!.GetAttribute("href"));
        Assert.Equal(2, values[2].QuerySelectorAll("address br").Length);
        Assert.Equal("Show on a map", values[2].QuerySelector("a")!.TextContent);
        Assert.Equal("Weekdays 9–17", values[3].TextContent);
        Assert.NotNull(cut.Find(".stil-contact > .stil-contact-form > form"));
    }

    [Fact]
    public void ContactLabelsAreTranslated()
    {
        using var context = new BunitContext();
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("sv-SE");
        try
        {
            var cut = context.Render<StilContact>(parameters => parameters
                .Add(c => c.Email, "hej@tidvy.example")
                .Add(c => c.Address, "Linnankatu 12")
                .Add(c => c.MapHref, "https://www.openstreetmap.org/"));

            Assert.Equal(["E-post", "Adress"], cut.FindAll("dt").Select(dt => dt.TextContent.Trim()));
            Assert.Equal("Visa på karta", cut.Find("dd address + a").TextContent);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
