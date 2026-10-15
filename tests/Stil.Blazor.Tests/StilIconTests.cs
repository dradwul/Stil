using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilIconTests
{
    [Fact]
    public void RendersBuiltInIconAsDecorativeInlineSvg()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilIcon>(parameters => parameters
            .Add(component => component.Name, StilIcons.Check)
            .AddUnmatched("class", "stil-text-success"));

        var svg = cut.Find("svg");
        Assert.Equal(["stil-icon", "stil-text-success"], svg.ClassList);
        Assert.Equal("0 0 24 24", svg.GetAttribute("viewBox"));
        Assert.Equal("currentColor", svg.GetAttribute("stroke"));
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Null(svg.GetAttribute("role"));
        Assert.NotNull(svg.QuerySelector("path"));
    }

    [Fact]
    public void LabelMakesTheIconAnImageWithAName()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilIcon>(parameters => parameters
            .Add(component => component.Name, StilIcons.Warning)
            .Add(component => component.Label, "Warning"));

        var svg = cut.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Warning", svg.GetAttribute("aria-label"));
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SizeSetsTheFontSize()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilIcon>(parameters => parameters
            .Add(component => component.Name, StilIcons.Menu)
            .Add(component => component.Size, "24px"));

        Assert.Equal("font-size: 24px", cut.Find("svg").GetAttribute("style"));
    }

    [Fact]
    public void UnknownNameRendersNothing()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilIcon>(parameters => parameters.Add(component => component.Name, "no-such-icon"));

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void RegisteredProviderSuppliesTheIcon()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options => options.IconProvider = new StilLigatureIconProvider("material-symbols-outlined"));

        var cut = context.Render<StilIcon>(parameters => parameters.Add(component => component.Name, "shopping_cart"));

        var slot = cut.Find("span.stil-icon-slot");
        Assert.Equal("true", slot.GetAttribute("aria-hidden"));
        var glyph = slot.QuerySelector("span.material-symbols-outlined");
        Assert.NotNull(glyph);
        Assert.Equal("shopping_cart", glyph.TextContent);
    }

    [Fact]
    public void ProviderWithoutTheIconFallsBackToBuiltIn()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options =>
            options.IconProvider = new StilLigatureIconProvider("material-symbols-outlined", name => name == "shopping_cart"));

        var cut = context.Render<StilIcon>(parameters => parameters.Add(component => component.Name, StilIcons.Close));

        Assert.NotNull(cut.Find("svg.stil-icon"));
        Assert.Empty(cut.FindAll(".material-symbols-outlined"));
    }

    [Fact]
    public void ChildContentReplacesNamedIcon()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilIcon>(parameters => parameters
            .Add(component => component.Name, StilIcons.Check)
            .Add(component => component.Label, "Logo")
            .AddChildContent("<svg class=\"brand\"></svg>"));

        var slot = cut.Find("span.stil-icon-slot");
        Assert.Equal("img", slot.GetAttribute("role"));
        Assert.Equal("Logo", slot.GetAttribute("aria-label"));
        Assert.NotNull(slot.QuerySelector("svg.brand"));
        Assert.Empty(cut.FindAll("svg.stil-icon"));
    }

    [Fact]
    public void ConstantsMatchTheEmbeddedIconSet()
    {
        var constants = typeof(StilIcons)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
            .Order()
            .ToList();

        Assert.Equal(StilIcons.BuiltIn.Keys.Order().ToList(), constants);
    }

    [Fact]
    public void EveryBuiltInIconRendersSvgShapes()
    {
        using var context = new BunitContext();

        foreach (var name in StilIcons.BuiltIn.Keys)
        {
            var cut = context.Render<StilIcon>(parameters => parameters.Add(component => component.Name, name));
            Assert.True(cut.Find("svg").ChildElementCount > 0, $"Icon '{name}' has no shapes.");
        }
    }

    [Fact]
    public void LigatureProviderRendersTheNameAsText()
    {
        var fragment = new StilLigatureIconProvider("my-icons").GetIcon("home");
        Assert.NotNull(fragment);

        using var context = new BunitContext();
        var cut = context.Render(fragment);

        Assert.Equal("home", cut.Find("span.my-icons").TextContent);
    }
}
