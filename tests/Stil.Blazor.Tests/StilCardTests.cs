using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilCardTests
{
    [Fact]
    public void RendersHeaderAndChildContentInsideScopedCard()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters => parameters
            .Add(component => component.HeaderText, "Card heading")
            .AddChildContent("<p>Card content</p>")
            .AddUnmatched("data-testid", "card")
            .AddUnmatched("class", "custom-card"));

        var card = cut.Find("div.card");
        Assert.Contains("stil-scope", card.ClassList);
        Assert.Contains("custom-card", card.ClassList);
        Assert.Equal("card", card.GetAttribute("data-testid"));
        Assert.Equal("Card heading", card.QuerySelector("h2")?.TextContent);
        Assert.Equal("Card content", card.QuerySelector("p")?.TextContent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void OmitsEmptyHeader(string? headerText)
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCard>(parameters =>
            parameters.Add(component => component.HeaderText, headerText));

        Assert.Empty(cut.FindAll("h2"));
    }
}
