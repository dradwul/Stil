using System.ComponentModel.DataAnnotations;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilSignaturePadTests
{
    private const string ModulePath = "./_content/Stil/stil.js";

    private static BunitJSModuleInterop SetupModule(BunitContext context)
    {
        var module = context.JSInterop.SetupModule(ModulePath);
        module.SetupVoid("initSignaturePad", _ => true).SetVoidResult();
        module.SetupVoid("drawSignature", _ => true).SetVoidResult();
        return module;
    }

    [Fact]
    public async Task DrawnStrokesBecomeTheValueAndUndoRemovesThem()
    {
        using var context = new BunitContext();
        SetupModule(context);
        StilSignature? value = null;

        var cut = context.Render<StilSignaturePad>(parameters => parameters
            .Add(component => component.Label, "Signature")
            .Add(component => component.ValueChanged, (StilSignature? v) => value = v));

        Assert.Contains("empty", cut.Find("canvas").GetAttribute("aria-label"));
        Assert.Equal("Sign here", cut.Find(".stil-signature-placeholder").TextContent);

        await cut.InvokeAsync(() => cut.Instance.OnStroke([10, 20, 0.5, 30, 40, 1], 300, 120));

        Assert.NotNull(value);
        Assert.Equal((300d, 120d), (value.Width, value.Height));
        Assert.Equal([new StilSignaturePoint(10, 20, 0.5), new StilSignaturePoint(30, 40, 1)], value.Strokes.Single());
        Assert.Equal("Signature area, signed", cut.Find("canvas").GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll(".stil-signature-placeholder"));

        cut.FindAll("button").Single(b => b.TextContent == "Undo").Click();

        Assert.Null(value);
    }

    [Fact]
    public void TypedNameIsTheKeyboardAlternative()
    {
        using var context = new BunitContext();
        SetupModule(context);
        StilSignature? value = null;

        var cut = context.Render<StilSignaturePad>(parameters => parameters
            .Add(component => component.ValueChanged, (StilSignature? v) => value = v));

        cut.FindAll("button").Single(b => b.TextContent == "Type").Click();
        cut.Find("input").Input("  Maja Lind ");

        Assert.Equal("Maja Lind", value?.TypedName);
        Assert.Equal("  Maja Lind ", cut.Find(".stil-signature-typed").TextContent);
        Assert.Equal("true", cut.FindAll("button").Single(b => b.TextContent == "Type").GetAttribute("aria-pressed"));

        cut.FindAll("button").Single(b => b.TextContent == "Draw").Click();

        Assert.Null(value);
        Assert.Single(cut.FindAll("canvas"));
    }

    [Fact]
    public void RequiredSignatureIsValidated()
    {
        using var context = new BunitContext();
        SetupModule(context);
        var model = new Visit();

        var cut = context.Render<EditForm>(parameters => parameters
            .Add(form => form.Model, model)
            .Add<RenderFragment<EditContext>>(form => form.ChildContent, _ => builder =>
            {
                builder.OpenComponent<DataAnnotationsValidator>(0);
                builder.CloseComponent();
                builder.OpenComponent<StilSignaturePad>(1);
                builder.AddAttribute(2, nameof(StilSignaturePad.Value), model.Signature);
                builder.AddAttribute(3, nameof(StilSignaturePad.ValueChanged), EventCallback.Factory.Create<StilSignature?>(this, v => model.Signature = v));
                builder.AddAttribute(4, nameof(StilSignaturePad.ValueExpression), (System.Linq.Expressions.Expression<Func<StilSignature?>>)(() => model.Signature));
                builder.CloseComponent();
            }));

        cut.Find("form").Submit();

        Assert.Equal("Sign the visit.", cut.Find(".stil-field-error").TextContent);
        Assert.True(cut.Find(".stil-signature").HasAttribute("data-stil-invalid"));
    }

    [Fact]
    public void SvgDrawsStrokesAndTypedNames()
    {
        var drawn = new StilSignature([[new(10, 20, 0.5), new(30, 40, 0.5)], [new(50, 60, 1)]], null, 300, 120);

        var svg = drawn.ToSvg("#123");

        Assert.StartsWith("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 300 120\">", svg);
        Assert.Contains("<path d=\"M10 20L30 40\" fill=\"none\" stroke=\"#123\" stroke-width=\"2.5\"", svg);
        Assert.Contains("<path d=\"M50 60h0.1\"", svg);
        Assert.Contains("&lt;Ida &amp; Per&gt;", StilSignature.Typed("<Ida & Per>").ToSvg());
        Assert.False(new StilSignature([], " ", 1, 1).HasContent);
    }

    [Fact]
    public async Task PngComesFromStilJs()
    {
        using var context = new BunitContext();
        var module = SetupModule(context);
        module.Setup<string>("signaturePng", _ => true).SetResult("data:image/png;base64,AAAA");

        var cut = context.Render<StilSignaturePad>(parameters => parameters
            .Add(component => component.Value, new StilSignature([[new(1, 2, 0.5)]], null, 300, 120)));

        Assert.Equal("data:image/png;base64,AAAA", await cut.InvokeAsync(() => cut.Instance.ToPngAsync()));
        var arguments = module.Invocations["signaturePng"].Single().Arguments;
        Assert.Equal([1d, 2d, 0.5], ((double[][])arguments[0]!)[0]);
        Assert.Equal(300d, arguments[2]);
    }

    private sealed class Visit
    {
        [Required(ErrorMessage = "Sign the visit.")]
        public StilSignature? Signature { get; set; }
    }
}
