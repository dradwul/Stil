using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Blocks;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilSellingBlocksTests
{
    [Fact]
    public void PlansShowTheirPriceFeaturesAndAction()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPricing>(parameters => parameters
            .AddChildContent<StilPlan>(plan => plan
                .Add(c => c.Name, "Team")
                .Add(c => c.Price, "€6")
                .Add(c => c.Period, "a person a month")
                .Add(c => c.Text, "For growing teams.")
                .Add(c => c.Features, ["Unlimited people", "Priority support"])
                .Add(c => c.Featured, true)
                .Add(c => c.Badge, "Most popular")
                .Add(c => c.ActionText, "Start")
                .Add(c => c.ActionHref, "start")));

        Assert.Empty(cut.FindAll("fieldset"));
        var plan = cut.Find("ul.stil-plans > li.stil-plan");
        Assert.Contains("stil-plan-featured", plan.ClassName);
        Assert.Equal("Most popular", plan.QuerySelector(".stil-plan-badge")!.TextContent);
        Assert.Equal("Team", plan.QuerySelector("h3.stil-plan-name")!.TextContent);
        Assert.Equal("€6", plan.QuerySelector(".stil-plan-amount")!.TextContent);
        Assert.Equal("€6 a person a month", plan.QuerySelector(".stil-plan-price")!.TextContent.Trim());
        Assert.Empty(plan.QuerySelectorAll(".stil-plan-yearly"));
        Assert.Equal(["Unlimited people", "Priority support"], plan.QuerySelectorAll(".stil-plan-features > li").Select(li => li.TextContent.Trim()));
        Assert.All(plan.QuerySelectorAll(".stil-plan-features svg"), icon => Assert.Equal("true", icon.GetAttribute("aria-hidden")));
        var action = plan.QuerySelector("a.stil-plan-action")!;
        Assert.Equal("start", action.GetAttribute("href"));
        Assert.DoesNotContain("stil-btn-outline", action.ClassName);
    }

    [Fact]
    public void PeriodSwitchIsANamedRadioGroupOverBothPrices()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPricing>(parameters => parameters
            .Add(c => c.PeriodSwitch, true)
            .Add(c => c.Yearly, true)
            .Add(c => c.YearlyNote, "2 months free")
            .AddChildContent<StilPlan>(plan => plan
                .Add(c => c.Name, "Team")
                .Add(c => c.Price, "€6")
                .Add(c => c.Period, "a month")
                .Add(c => c.YearlyPrice, "€60")
                .Add(c => c.YearlyPeriod, "a year")
                .Add(c => c.ActionText, "Start")));

        Assert.Equal("Billing period", cut.Find("fieldset > legend").TextContent);
        var radios = cut.FindAll("fieldset input[type=radio]");
        Assert.Equal(2, radios.Count);
        Assert.Equal(radios[0].GetAttribute("name"), radios[1].GetAttribute("name"));
        Assert.False(radios[0].HasAttribute("checked"));
        Assert.True(radios[1].HasAttribute("checked"));
        Assert.Contains("stil-pricing-yearly", radios[1].ClassName);
        Assert.Equal(["Monthly", "Yearly 2 months free"], cut.FindAll("fieldset label").Select(l => string.Join(' ', l.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))));
        Assert.Equal("€6 a month", cut.Find(".stil-plan-monthly").TextContent.Trim());
        Assert.Equal("€60 a year", cut.Find(".stil-plan-yearly").TextContent.Trim());
        Assert.Contains("stil-btn-outline", cut.Find("a.stil-plan-action, button.stil-plan-action").ClassName);
    }

    [Fact]
    public void PeriodSwitchIsTranslated()
    {
        using var context = new BunitContext();
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("sv-SE");
        try
        {
            var cut = context.Render<StilPricing>(parameters => parameters.Add(c => c.PeriodSwitch, true));

            Assert.Equal("Betalningsperiod", cut.Find("legend").TextContent);
            Assert.Equal(["Månadsvis", "Årsvis"], cut.FindAll("fieldset label").Select(l => l.TextContent.Trim()));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void TestimonialIsAFigureWithAQuoteAndItsAuthor()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTestimonials>(parameters => parameters
            .AddChildContent<StilTestimonial>(testimonial => testimonial
                .Add(c => c.Quote, "Our Mondays got calm.")
                .Add(c => c.Name, "Maja Lind")
                .Add(c => c.Role, "Head of operations, Nordvik Supply")));

        var figure = cut.Find("div.stil-testimonials > figure.stil-testimonial");
        Assert.Equal("Our Mondays got calm.", figure.QuerySelector("blockquote.stil-testimonial-quote > p")!.TextContent);
        var author = figure.QuerySelector("figcaption.stil-testimonial-author")!;
        Assert.Equal("ML", author.QuerySelector(".stil-avatar")!.TextContent);
        Assert.Equal("true", author.QuerySelector(".stil-avatar")!.GetAttribute("aria-hidden"));
        Assert.Equal("Maja Lind", author.QuerySelector(".stil-testimonial-name")!.TextContent);
        Assert.Equal("Head of operations, Nordvik Supply", author.QuerySelector(".stil-testimonial-role")!.TextContent);
    }

    [Fact]
    public void FeaturedTestimonialHasADecorativePhoto()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTestimonial>(parameters => parameters
            .Add(c => c.Name, "Erik Holm")
            .Add(c => c.Photo, "erik.jpg")
            .Add(c => c.Featured, true)
            .AddChildContent("<p>One.</p><p>Two.</p>"));

        Assert.Equal("stil-testimonial stil-testimonial-featured", cut.Find("figure").ClassName);
        Assert.Equal(2, cut.FindAll("blockquote > p").Count);
        Assert.Equal("", cut.Find("img.stil-avatar").GetAttribute("alt"));
    }

    [Fact]
    public void FaqIsAHeaderAndAnAccordion()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFaq>(parameters => parameters
            .Add(c => c.Title, "Questions and answers")
            .Add(c => c.Lead, "Anything else? Write to us.")
            .AddChildContent<StilAccordionItem>(item => item
                .Add(c => c.Title, "Can I cancel any time?")
                .AddChildContent("Yes, from the settings.")));

        Assert.Equal("Questions and answers", cut.Find(".stil-faq > header.stil-faq-header > h2.stil-section-title").TextContent);
        Assert.Equal("Anything else? Write to us.", cut.Find(".stil-faq-header .stil-lead").TextContent);
        Assert.Equal("Can I cancel any time?", cut.Find(".stil-faq > .stil-accordion details > summary").TextContent);
    }

    [Fact]
    public void TeamMembersHaveANamedHeadingAndLinks()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTeam>(parameters => parameters
            .AddChildContent<StilTeamMember>(member => member
                .Add(c => c.Name, "Sara Berg")
                .Add(c => c.Role, "Founder")
                .Add(c => c.Text, "Plans the weeks.")
                .Add(c => c.Links, [new StilLinkItem("E-mail", "mailto:sara@example.com")])));

        var member = cut.Find("ul.stil-team > li.stil-team-member");
        Assert.Equal("SB", member.QuerySelector("span.stil-team-photo")!.TextContent);
        Assert.Equal("true", member.QuerySelector(".stil-team-photo")!.GetAttribute("aria-hidden"));
        Assert.Equal("Sara Berg", member.QuerySelector("h3.stil-team-name")!.TextContent);
        Assert.Equal("Founder", member.QuerySelector(".stil-team-role")!.TextContent);
        Assert.Equal("Plans the weeks.", member.QuerySelector(".stil-team-bio")!.TextContent);
        Assert.Equal("mailto:sara@example.com", member.QuerySelector(".stil-team-links a")!.GetAttribute("href"));
    }

    [Fact]
    public void TeamMemberPhotoIsDecorative()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTeamMember>(parameters => parameters
            .Add(c => c.Name, "Sara Berg")
            .Add(c => c.Photo, "sara.jpg"));

        Assert.Equal("", cut.Find("img.stil-team-photo").GetAttribute("alt"));
        Assert.Empty(cut.FindAll(".stil-team-links"));
    }
}
