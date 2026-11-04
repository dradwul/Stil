using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilHeadTests
{
    [Fact]
    public void RendersMinifiedStylesheetByDefault()
    {
        using var context = new BunitContext();
        context.Services.AddStil();

        var cut = context.Render<StilHead>();

        // Without a registered asset, ComponentBase.Assets returns the key unchanged.
        var link = cut.Find("link");
        Assert.Equal("stylesheet", link.GetAttribute("rel"));
        Assert.Equal("_content/Stil/stil.min.css", link.GetAttribute("href"));
        Assert.Empty(cut.FindAll("style"));
    }

    [Fact]
    public void RendersUnminifiedStylesheetWhenMinifiedIsFalse()
    {
        using var context = new BunitContext();
        context.Services.AddStil();

        var cut = context.Render<StilHead>(parameters => parameters.Add(component => component.Minified, false));

        Assert.Equal("_content/Stil/stil.css", cut.Find("link").GetAttribute("href"));
    }

    [Fact]
    public void ResolvesFingerprintedAssetUrl()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        context.AddAsset(url: "_content/Stil/stil.abc123.min.css", label: "_content/Stil/stil.min.css");

        var cut = context.Render<StilHead>();

        Assert.Equal("_content/Stil/stil.abc123.min.css", cut.Find("link").GetAttribute("href"));
    }

    [Fact]
    public void WritesGlobalThemeToRootStyle()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options =>
        {
            options.Theme.Primary = "#6741d9";
            options.Theme.Tokens["--stil-space-4"] = "1.25rem";
        });

        var cut = context.Render<StilHead>();

        var style = cut.Find("style");
        Assert.Equal(":root { --stil-color-primary: #6741d9; --stil-space-4: 1.25rem; }", style.TextContent);
    }

    [Fact]
    public async Task StaticallyRenderedThemeIsNotHtmlEncodedInsideStyle()
    {
        // StilHead belongs in App.razor's <head>, which is always rendered statically. Text inside
        // <style> is raw text for the browser, so entities such as &quot; are not decoded there.
        var services = new ServiceCollection();
        services.AddStil(options => options.Theme.FontFamily = "\"Inter\", sans-serif");
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<StilHead>()).ToHtmlString());

        Assert.Contains("<link rel=\"stylesheet\" href=\"_content/Stil/stil.min.css\" />", html);
        Assert.Contains("<style>:root { --stil-font-family: \"Inter\", sans-serif; }</style>", html);
    }

    [Fact]
    public void LinksTheFontAndSetsTheFontFamily()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options => options.Font = StilFonts.BrCobane);

        var cut = context.Render<StilHead>();

        var links = cut.FindAll("link");
        Assert.Equal("https://fonts.cdnfonts.com/css/br-cobane", links[1].GetAttribute("href"));
        Assert.Equal(":root { --stil-font-family: 'BR Cobane', sans-serif; }", cut.Find("style").TextContent);
    }

    [Fact]
    public void ThemeFontFamilyWinsOverTheFont()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options =>
        {
            options.Font = StilFont.Google("Open Sans");
            options.Theme.FontFamily = "system-ui";
        });

        var cut = context.Render<StilHead>();

        Assert.Equal("https://fonts.googleapis.com/css2?family=Open+Sans:wght@400;600&display=swap", cut.FindAll("link")[1].GetAttribute("href"));
        Assert.Equal(":root { --stil-font-family: 'Open Sans', sans-serif; --stil-font-family: system-ui; }", cut.Find("style").TextContent);
    }

    [Theory]
    [InlineData("Inter'; }", null)]
    [InlineData("Inter", "http://example.com/font.css")]
    public void RejectsAFontThatCouldBreakThePage(string family, string? url)
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddStil(options => options.Font = new StilFont(family, url)));
    }

    [Fact]
    public void CdnFontsSlugDefaultsToTheFamilyName()
    {
        Assert.Equal("https://fonts.cdnfonts.com/css/br-sonoma", StilFont.CdnFonts("BR Sonoma").StylesheetUrl);
        Assert.Equal("https://fonts.cdnfonts.com/css/custom", StilFont.CdnFonts("Some Font", "custom").StylesheetUrl);
    }
}
