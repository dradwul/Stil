using Bunit;
using Microsoft.AspNetCore.Components;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilMediaTests
{
    private const string ModulePath = "./_content/Stil/stil.js";

    // A 1x1 PNG.
    private const string Pixel = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";

    // Skeleton

    [Fact]
    public void SkeletonTextHasLinesWithAShortLastOne()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSkeleton>(parameters => parameters
            .Add(component => component.Shape, StilSkeletonShape.Text)
            .Add(component => component.Lines, 3));

        var lines = cut.FindAll(".stil-skeleton-text");
        Assert.Equal(3, lines.Count);
        Assert.Equal("width: 60%", lines[2].GetAttribute("style"));
        Assert.All(lines, line => Assert.Equal("true", line.GetAttribute("aria-hidden")));
        Assert.Empty(cut.FindAll("[role=status]"));
    }

    [Fact]
    public void SkeletonWithALabelAnnouncesLoading()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSkeleton>(parameters => parameters
            .Add(component => component.Shape, StilSkeletonShape.Circle)
            .Add(component => component.Size, "3rem")
            .Add(component => component.Label, ""));

        Assert.Equal("Loading…", cut.Find("[role=status]").TextContent);
        Assert.Equal("true", cut.Find(".stil-skeleton-group").GetAttribute("aria-busy"));
        Assert.Equal("--stil-skeleton-size: 3rem", cut.Find(".stil-skeleton-circle").GetAttribute("style"));
    }

    // Splitter

    [Fact]
    public void SplitterIsAWindowSplitter()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilSplitter>(parameters => parameters
            .Add(component => component.Size, 95)
            .Add(component => component.Min, 20)
            .Add(component => component.Max, 80)
            .Add(component => component.First, "<p>List</p>")
            .Add(component => component.Second, "<p>Message</p>"));

        var handle = cut.Find("[role=separator]");
        var first = cut.FindAll(".stil-splitter-pane")[0];
        Assert.Equal(first.Id, handle.GetAttribute("aria-controls"));
        Assert.Equal("80", handle.GetAttribute("aria-valuenow"));
        Assert.Equal("20", handle.GetAttribute("aria-valuemin"));
        Assert.Equal("vertical", handle.GetAttribute("aria-orientation"));
        Assert.Equal("0", handle.GetAttribute("tabindex"));
        Assert.Equal("Resize panes", handle.GetAttribute("aria-label"));
        Assert.Equal("--stil-splitter-size: 80", cut.Find(".stil-splitter").GetAttribute("style"));
    }

    [Fact]
    public void SplitterReportsTheNewSize()
    {
        using var context = new BunitContext();
        double size = 50;

        var cut = context.Render<StilSplitter>(parameters => parameters
            .Add(component => component.Vertical, true)
            .Add(component => component.SizeChanged, (double value) => size = value));

        cut.Find(".stil-splitter").TriggerEvent("onstilsplit", new StilSplitEventArgs { Size = 33.5 });

        Assert.Equal(33.5, size);
        Assert.Contains("stil-splitter-vertical", cut.Find(".stil-splitter").ClassName);
        Assert.Equal("horizontal", cut.Find("[role=separator]").GetAttribute("aria-orientation"));
        Assert.Equal("33.5", cut.Find("[role=separator]").GetAttribute("aria-valuenow"));
    }

    // Carousel

    [Fact]
    public void CarouselHasNamedSlidesDotsAndButtons()
    {
        using var context = new BunitContext();
        var index = 0;

        var cut = context.Render<StilCarousel<string>>(parameters => parameters
            .Add(component => component.Items, ["Red", "Green", "Blue"])
            .Add(component => component.Label, "Colors")
            .Add(component => component.ItemTemplate, color => builder => builder.AddContent(0, color))
            .Add(component => component.Autoplay, TimeSpan.FromSeconds(6))
            .Add(component => component.IndexChanged, (int value) => index = value));

        var carousel = cut.Find(".stil-carousel");
        Assert.Equal("carousel", carousel.GetAttribute("aria-roledescription"));
        Assert.Equal("6000", carousel.GetAttribute("data-stil-autoplay"));
        Assert.Equal(["1 of 3", "2 of 3", "3 of 3"], cut.FindAll(".stil-carousel-slide").Select(s => s.GetAttribute("aria-label")));
        Assert.Equal("true", cut.FindAll(".stil-carousel-dots > button")[0].GetAttribute("aria-current"));
        Assert.Equal("Next slide", cut.Find(".stil-carousel-next").GetAttribute("aria-label"));
        Assert.Equal("false", cut.Find(".stil-carousel-pause").GetAttribute("aria-pressed"));

        carousel.TriggerEvent("onstilslide", new StilSlideEventArgs { Index = 2 });

        Assert.Equal(2, index);
        Assert.Equal("true", cut.FindAll(".stil-carousel-dots > button")[2].GetAttribute("aria-current"));
    }

    [Fact]
    public void CarouselWithOneSlideHasNoControls()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCarousel<string>>(parameters => parameters
            .Add(component => component.Items, ["Only"])
            .Add(component => component.Label, "One")
            .Add(component => component.ItemTemplate, text => builder => builder.AddContent(0, text)));

        Assert.Empty(cut.FindAll(".stil-carousel-controls"));
    }

    // Image data, cropper and camera

    [Fact]
    public void ImageDataReadsADataUrl()
    {
        var image = new StilImageData(Pixel, 1, 1);

        Assert.Equal("image/png", image.ContentType);
        Assert.Equal(0x89, image.Bytes[0]);
        Assert.Equal("avatar.png", image.FileName("avatar"));
        Assert.Throws<FormatException>(() => new StilImageData("https://example.com/a.png", 1, 1));
    }

    [Fact]
    public async Task CropperInitializesAndCrops()
    {
        using var context = new BunitContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.SetupVoid("initCropper", _ => true);
        module.Setup<string?>("cropImage", _ => true).SetResult(Pixel);

        var cut = context.Render<StilImageCropper>(parameters => parameters
            .Add(component => component.Source, Pixel)
            .Add(component => component.AspectRatio, 1.5)
            .Add(component => component.Round, true)
            .Add(component => component.OutputWidth, 300));

        Assert.Contains("stil-cropper-round", cut.Find(".stil-cropper").ClassName);
        Assert.Equal("--stil-cropper-ratio: 1.5", cut.Find(".stil-cropper").GetAttribute("style"));
        Assert.Single(module.Invocations["initCropper"]);

        var image = await cut.InvokeAsync(() => cut.Instance.CropAsync());

        Assert.NotNull(image);
        Assert.Equal((300, 200), (image.Width, image.Height));
        var crop = Assert.Single(module.Invocations["cropImage"]);
        Assert.Equal([300, 200], crop.Arguments.Skip(1).Take(2).Select(a => (int)a!));
    }

    [Fact]
    public void CameraFallsBackToChoosingAPhoto()
    {
        using var context = new BunitContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<bool>("startCamera", _ => true).SetResult(false);

        var cut = context.Render<StilCameraCapture>();

        cut.WaitForAssertion(() => Assert.Contains("The camera is not available", cut.Markup));
        Assert.Equal("image/*", cut.Find("input[type=file]").GetAttribute("accept"));
        Assert.Empty(cut.FindAll("video"));
    }

    [Fact]
    public void CameraCapturesAPhoto()
    {
        using var context = new BunitContext();
        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<bool>("startCamera", _ => true).SetResult(true);
        module.Setup<string?>("captureCamera", _ => true).SetResult(Pixel);
        module.Setup<int[]>("videoSize", _ => true).SetResult([640, 480]);
        StilImageData? photo = null;

        var cut = context.Render<StilCameraCapture>(parameters => parameters
            .Add(component => component.OnCapture, (StilImageData image) => photo = image));

        cut.WaitForAssertion(() => Assert.False(cut.FindAll("button")[0].HasAttribute("disabled")));
        cut.FindAll("button")[0].Click();

        Assert.NotNull(photo);
        Assert.Equal((640, 480), (photo.Width, photo.Height));
    }

    // PDF viewer

    [Fact]
    public void PdfViewerHasAToolbarAndTheBrowsersViewer()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupModule(ModulePath).Setup<bool>("pdfViewerEnabled", _ => true).SetResult(true);

        var cut = context.Render<StilPdfViewer>(parameters => parameters
            .Add(component => component.Source, "files/invoice.pdf")
            .Add(component => component.Title, "Invoice 1042")
            .Add(component => component.FileName, "invoice-1042.pdf"));

        var links = cut.FindAll(".stil-pdf-toolbar a");
        Assert.Equal("invoice-1042.pdf", links[0].GetAttribute("download"));
        Assert.Equal("_blank", links[1].GetAttribute("target"));
        Assert.Equal("Invoice 1042", cut.Find("iframe").GetAttribute("title"));
        Assert.Contains("Print", cut.Find(".stil-pdf-toolbar").TextContent);
    }

    [Fact]
    public void PdfViewerExplainsWhenTheBrowserCannotShowPdfs()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupModule(ModulePath).Setup<bool>("pdfViewerEnabled", _ => true).SetResult(false);

        var cut = context.Render<StilPdfViewer>(parameters => parameters.Add(component => component.Source, "a.pdf"));

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".stil-pdf-fallback")));
        Assert.Empty(cut.FindAll("iframe"));
        Assert.Equal(2, cut.FindAll(".stil-pdf-toolbar a").Count);
    }

    [Fact]
    public void PdfViewerTakesAnOwnRenderer()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPdfViewer>(parameters => parameters
            .Add(component => component.Source, "a.pdf")
            .AddChildContent("<div class=\"my-renderer\"></div>"));

        Assert.NotNull(cut.Find(".my-renderer"));
        Assert.Empty(cut.FindAll("iframe"));
    }

    [Fact]
    public void MediaTextsAreLocalized() => StilLocalizationTests.InCulture("sv-SE", () =>
    {
        using var context = new BunitContext();

        var cut = context.Render<StilCarousel<int>>(parameters => parameters
            .Add(component => component.Items, [1, 2])
            .Add(component => component.Label, "Bilder")
            .Add(component => component.ItemTemplate, number => builder => builder.AddContent(0, number)));

        Assert.Equal("Nästa bild", cut.Find(".stil-carousel-next").GetAttribute("aria-label"));
        Assert.Equal("1 av 2", cut.Find(".stil-carousel-slide").GetAttribute("aria-label"));
    });
}
