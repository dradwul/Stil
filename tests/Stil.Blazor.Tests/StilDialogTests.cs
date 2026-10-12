using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilDialogTests
{
    private const string ModulePath = "./_content/Stil/stil.js";

    private static BunitJSModuleInterop SetupModule(BunitContext context)
    {
        var module = context.JSInterop.SetupModule(ModulePath);
        module.SetupVoid("showDialog", _ => true).SetVoidResult();
        module.SetupVoid("closeDialog", _ => true).SetVoidResult();
        return module;
    }

    [Fact]
    public void ClosedDialogRendersEmptyDialogElementWithoutJavaScript()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);

        var cut = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Title, "Edit")
            .AddChildContent("<p>Body</p>"));

        var dialog = cut.Find("dialog");
        Assert.Equal(["stil-dialog"], dialog.ClassList);
        Assert.False(dialog.HasAttribute("open"));
        Assert.Equal(0, dialog.ChildElementCount);
        Assert.False(string.IsNullOrEmpty(dialog.GetAttribute("aria-labelledby")));
        Assert.Empty(context.JSInterop.Invocations);
        Assert.Empty(module.Invocations);
    }

    [Fact]
    public void OpenDialogRendersHeaderBodyAndCallsShowDialog()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);

        var cut = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.Title, "Edit profile")
            .AddChildContent("<p>Body</p>"));

        var dialog = cut.Find("dialog");
        var title = dialog.QuerySelector(".stil-dialog-header h2.stil-dialog-title");
        Assert.NotNull(title);
        Assert.Equal("Edit profile", title.TextContent);
        Assert.Equal(title.Id, dialog.GetAttribute("aria-labelledby"));
        Assert.Equal("Body", dialog.QuerySelector(".stil-dialog-body p")?.TextContent);
        Assert.Empty(dialog.QuerySelectorAll(".stil-dialog-footer"));

        var close = dialog.QuerySelector(".stil-dialog-header button.stil-close");
        Assert.NotNull(close);
        Assert.Equal("button", close.GetAttribute("type"));
        Assert.Equal("Close", close.GetAttribute("aria-label"));

        context.JSInterop.VerifyInvoke("import");
        var show = module.VerifyInvoke("showDialog");
        Assert.Equal(2, show.Arguments.Count);
        Assert.IsType<ElementReference>(show.Arguments[0]);
        Assert.IsType<DotNetObjectReference<StilDialog>>(show.Arguments[1]);
        module.VerifyNotInvoke("closeDialog");
    }

    [Fact]
    public void FooterIsRenderedWhenSet()
    {
        using var context = new BunitContext();
        SetupModule(context);

        var cut = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.Footer, "<button class=\"save\">Save</button>"));

        Assert.Equal("Save", cut.Find(".stil-dialog-footer .save").TextContent);
    }

    [Fact]
    public void ShowCloseFalseHidesCloseButtonAndCloseLabelIsUsed()
    {
        using var context = new BunitContext();
        SetupModule(context);

        var hidden = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.ShowClose, false));
        var labelled = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.CloseLabel, "Stäng"));

        Assert.Empty(hidden.FindAll(".stil-close"));
        Assert.Equal("Stäng", labelled.Find(".stil-close").GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData(StilSize.Small, "stil-dialog-sm")]
    [InlineData(StilSize.Large, "stil-dialog-lg")]
    public void SizeAddsSizeClass(StilSize size, string expectedClass)
    {
        using var context = new BunitContext();
        SetupModule(context);

        var cut = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Size, size)
            .AddUnmatched("class", "custom")
            .AddUnmatched("id", "edit-dialog"));

        var dialog = cut.Find("dialog");
        Assert.Equal(["stil-dialog", expectedClass, "custom"], dialog.ClassList);
        Assert.Equal("edit-dialog", dialog.Id);
    }

    [Fact]
    public void OpeningLaterCallsShowDialog()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);

        var cut = context.Render<StilDialog>(parameters => parameters.AddChildContent("<p>Body</p>"));
        module.VerifyNotInvoke("showDialog");

        cut.Render(parameters => parameters.Add(component => component.Open, true));

        module.VerifyInvoke("showDialog");
        Assert.Equal("Body", cut.Find(".stil-dialog-body p").TextContent);
    }

    [Fact]
    public void ClosingFromParentCallsCloseDialog()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);

        var cut = context.Render<StilDialog>(parameters => parameters.Add(component => component.Open, true));
        cut.Render(parameters => parameters.Add(component => component.Open, false));

        module.VerifyInvoke("showDialog");
        module.VerifyInvoke("closeDialog");
        Assert.Equal(0, cut.Find("dialog").ChildElementCount);
    }

    [Fact]
    public void CloseButtonSetsOpenFalseAndRaisesOnClose()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);
        var openChanges = new List<bool>();
        var closed = 0;

        var cut = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.OpenChanged, (bool open) => openChanges.Add(open))
            .Add(component => component.OnClose, () => closed++)
            .AddChildContent("<p>Body</p>"));

        cut.Find(".stil-close").Click();

        Assert.Equal([false], openChanges);
        Assert.Equal(1, closed);
        Assert.False(cut.Instance.Open);
        Assert.Empty(cut.FindAll(".stil-dialog-body"));
        module.VerifyInvoke("closeDialog");
    }

    [Fact]
    public async Task OnDialogClosedFromJavaScriptClosesWithoutCallingCloseDialog()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);
        var openChanges = new List<bool>();
        var closed = 0;

        var cut = context.Render<StilDialog>(parameters => parameters
            .Add(component => component.Open, true)
            .Add(component => component.OpenChanged, (bool open) => openChanges.Add(open))
            .Add(component => component.OnClose, () => closed++)
            .AddChildContent("<p>Body</p>"));

        await cut.InvokeAsync(() => cut.Instance.OnDialogClosed());

        Assert.Equal([false], openChanges);
        Assert.Equal(1, closed);
        Assert.Empty(cut.FindAll(".stil-dialog-body"));
        // The browser already closed the dialog (Escape), so there is nothing to close in JS.
        module.VerifyNotInvoke("closeDialog");
    }

    [Fact]
    public async Task OnDialogClosedIsInvokableFromJavaScript()
    {
        var method = typeof(StilDialog).GetMethod(nameof(StilDialog.OnDialogClosed));

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttributes(typeof(JSInvokableAttribute), inherit: false).SingleOrDefault());
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CloseAsyncOnClosedDialogDoesNothing()
    {
        using var context = new BunitContext();
        SetupModule(context);
        var closed = 0;

        var cut = context.Render<StilDialog>(parameters => parameters.Add(component => component.OnClose, () => closed++));

        await cut.InvokeAsync(() => cut.Instance.CloseAsync());

        Assert.Equal(0, closed);
    }

    [Fact]
    public void ReopeningAfterCloseCallsShowDialogAgain()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);

        var cut = context.Render<StilDialog>(parameters => parameters.Add(component => component.Open, true));
        cut.Find(".stil-close").Click();
        cut.Render(parameters => parameters.Add(component => component.Open, true));

        Assert.Equal(2, module.Invocations["showDialog"].Count);
        Assert.Single(context.JSInterop.Invocations["import"]);
    }

    [Fact]
    public async Task DisposingDisposesTheModule()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        context.Render<StilDialog>(parameters => parameters.Add(component => component.Open, true));

        await context.DisposeAsync();
    }
}
