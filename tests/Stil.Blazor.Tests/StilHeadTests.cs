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
}
