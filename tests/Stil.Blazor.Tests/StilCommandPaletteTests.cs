using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilCommandPaletteTests
{
    private const string ModulePath = "./_content/Stil/stil.js";

    private static (IRenderedComponent<StilCommandPalette> Cut, BunitJSModuleInterop Module, List<string> Ran) Render(BunitContext context, string? recent = null)
    {
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Mode = JSRuntimeMode.Loose;
        module.Setup<string?>("readStorage", _ => true).SetResult(recent);
        var ran = new List<string>();
        var commands = new List<StilCommand>
        {
            new("Components", "Pages") { Href = "/" },
            new("Orders", "Pages") { Href = "/orders", Keywords = "sales" },
            new("Check-in", "Pages") { Href = "/check-in" },
            new("New order", "Actions") { Action = () => { ran.Add("new order"); return Task.CompletedTask; }, Shortcut = "Ctrl+N" },
            new("Toggle dark mode", "Actions") { Action = () => { ran.Add("dark"); return Task.CompletedTask; } },
        };
        var cut = context.Render<StilCommandPalette>(parameters => parameters.Add(component => component.Commands, commands));
        return (cut, module, ran);
    }

    private static List<string> Options(IRenderedComponent<StilCommandPalette> cut) =>
        [.. cut.FindAll("[role=option] .stil-command-title").Select(o => o.TextContent)];

    [Fact]
    public void OpensWithTheRecentChoicesFirst()
    {
        using var context = new BunitContext();
        var (cut, module, _) = Render(context, "Pages/Orders\nActions/New order");

        Assert.Empty(cut.FindAll("[role=combobox]"));
        cut.Find(".stil-command-button").Click();

        Assert.Single(module.Invocations["showDialog"]);
        Assert.Equal(["Recent", "Pages", "Actions"], cut.FindAll(".stil-command-group").Select(g => g.TextContent));
        Assert.Equal(["Orders", "New order", "Components", "Orders", "Check-in", "New order", "Toggle dark mode"], Options(cut));
        Assert.Equal(cut.FindAll("[role=option]")[0].Id, cut.Find("[role=combobox]").GetAttribute("aria-activedescendant"));
        Assert.Equal("Ctrl+N", cut.Find("[role=option] kbd").TextContent);
    }

    [Fact]
    public void SearchRanksTitleMatchesFirst()
    {
        using var context = new BunitContext();
        var (cut, _, _) = Render(context);
        cut.Find(".stil-command-button").Click();

        cut.Find("[role=combobox]").Input("ord");
        Assert.Equal(["Orders", "New order"], Options(cut));

        cut.Find("[role=combobox]").Input("sales");
        Assert.Equal(["Orders"], Options(cut));

        cut.Find("[role=combobox]").Input("xyz");
        Assert.Equal("No results", cut.Find(".stil-command-empty").TextContent);
    }

    [Fact]
    public void EnterRunsTheActiveCommandAndRemembersIt()
    {
        using var context = new BunitContext();
        var (cut, module, ran) = Render(context, "Pages/Orders");
        cut.Find(".stil-command-button").Click();

        cut.Find("[role=combobox]").Input("new");
        cut.Find("[role=combobox]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal(["new order"], ran);
        Assert.Equal("Actions/New order\nPages/Orders", module.Invocations["writeStorage"].Single().Arguments[1]);
        Assert.Single(module.Invocations["closeDialog"]);
        Assert.Empty(cut.FindAll("[role=combobox]"));
    }

    [Fact]
    public void ArrowKeysMoveAndAPageIsOpened()
    {
        using var context = new BunitContext();
        var (cut, _, _) = Render(context);
        cut.Find(".stil-command-button").Click();

        cut.Find("[role=combobox]").Input("o");
        var options = cut.FindAll("[role=option]");
        cut.Find("[role=combobox]").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Equal(options[1].Id, cut.Find("[role=combobox]").GetAttribute("aria-activedescendant"));
        cut.Find("[role=combobox]").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        cut.Find("[role=combobox]").KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Equal(cut.FindAll("[role=option]")[^1].Id, cut.Find("[role=combobox]").GetAttribute("aria-activedescendant"));

        cut.Find("[role=combobox]").Input("orders");
        cut.Find("[role=combobox]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.EndsWith("/orders", context.Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void EscapeAndCtrlKClose()
    {
        using var context = new BunitContext();
        var (cut, module, _) = Render(context);

        cut.InvokeAsync(() => cut.Instance.OpenFromKey());
        Assert.Single(cut.FindAll("[role=combobox]"));

        cut.Find("[role=combobox]").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Empty(cut.FindAll("[role=combobox]"));
        Assert.Single(module.Invocations["closeDialog"]);
    }
}
