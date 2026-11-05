using Bunit;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilScannerTests
{
    private const string ModulePath = "./_content/Stil/stil.js";

    private static BunitJSModuleInterop SetupModule(BunitContext context, string status, bool torch = false)
    {
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<string>("startScanner", _ => true).SetResult(status);
        module.Setup<bool>("hasTorch", _ => true).SetResult(torch);
        module.SetupVoid("initScanInput", _ => true).SetVoidResult();
        module.SetupVoid("stopScanner", _ => true).SetVoidResult();
        return module;
    }

    [Fact]
    public void WithoutADecoderTheCodeIsTyped()
    {
        using var context = new BunitContext();
        SetupModule(context, "no-decoder");

        var cut = context.Render<StilScanner>();

        cut.WaitForAssertion(() => Assert.Contains("cannot read codes with the camera", cut.Find("[role=status]").TextContent));
        Assert.Empty(cut.FindAll("video"));
        Assert.Equal("Code", cut.Find("label").TextContent);
    }

    [Fact]
    public void ScannedCodeIsRaisedAndAnnounced()
    {
        using var context = new BunitContext();
        var module = SetupModule(context, "scanning", torch: true);
        StilScanResult? scanned = null;

        var cut = context.Render<StilScanner>(parameters => parameters
            .Add(component => component.OnScan, (StilScanResult result) => scanned = result)
            .Add(component => component.Formats, ["qr_code"])
            .Add(component => component.ManualEntry, false));

        cut.WaitForAssertion(() => Assert.Contains("Flashlight", cut.Markup));
        Assert.Single(cut.FindAll("video"));
        Assert.Empty(cut.FindAll("input"));
        var options = module.Invocations["startScanner"].Single().Arguments[2]!;
        Assert.Equal("environment", options.GetType().GetProperty("facingMode")!.GetValue(options));

        cut.InvokeAsync(() => cut.Instance.OnScanned("VIS-1042", "qr_code"));

        Assert.Equal(new StilScanResult("VIS-1042", "qr_code", StilScanSource.Camera), scanned);
        Assert.Equal("Scanned: VIS-1042", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void NoCameraIsExplained()
    {
        using var context = new BunitContext();
        SetupModule(context, "no-camera");

        var cut = context.Render<StilScanner>();

        cut.WaitForAssertion(() => Assert.Equal("The camera is not available. Type the code instead.", cut.Find("[role=status]").TextContent));
    }

    [Theory]
    [InlineData(true, StilScanSource.Scanner)]
    [InlineData(false, StilScanSource.Keyboard)]
    public async Task ScanInputRaisesTheCodeAndClears(bool fromScanner, StilScanSource source)
    {
        using var context = new BunitContext();
        var module = SetupModule(context, "scanning");
        StilScanResult? scanned = null;

        var cut = context.Render<StilScanInput>(parameters => parameters
            .Add(component => component.Label, "Badge")
            .Add(component => component.MaxKeyInterval, 40)
            .Add(component => component.OnScan, (StilScanResult result) => scanned = result));

        cut.WaitForAssertion(() => Assert.Single(module.Invocations["initScanInput"]));
        Assert.Equal(40, module.Invocations["initScanInput"].Single().Arguments[2]);

        var clear = await cut.InvokeAsync(() => cut.Instance.OnEnter(" 7350053850019 ", fromScanner));

        Assert.True(clear);
        Assert.Equal(new StilScanResult("7350053850019", null, source), scanned);
    }

    [Fact]
    public async Task ScanInputCanKeepTheCode()
    {
        using var context = new BunitContext();
        SetupModule(context, "scanning");
        string? value = null;

        var cut = context.Render<StilScanInput>(parameters => parameters
            .Add(component => component.ClearOnScan, false)
            .Add(component => component.ValueChanged, (string? v) => value = v));

        Assert.False(await cut.InvokeAsync(() => cut.Instance.OnEnter("ABC-1", false)));
        Assert.Equal("ABC-1", value);
        Assert.False(await cut.InvokeAsync(() => cut.Instance.OnEnter("   ", false)));
    }
}
