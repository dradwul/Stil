using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilFileUploadTests
{
    private static InputFileContent File(string name, int size = 10, string? contentType = null) =>
        InputFileContent.CreateFromBinary(new byte[size], name, contentType: contentType);

    private static void WithInvariantCulture(Action action)
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void RendersFileInputInsideDropZoneWithLabel()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Id, "docs")
            .Add(component => component.Label, "Documents"));

        var input = cut.Find(".stil-dropzone > input[type=file]");
        Assert.Equal("docs", input.Id);
        Assert.Equal(["stil-dropzone-input"], input.ClassList);
        Assert.Equal("docs", cut.Find("label.stil-label").GetAttribute("for"));
        Assert.False(input.HasAttribute("multiple"));
        Assert.False(input.HasAttribute("accept"));
        Assert.False(input.HasAttribute("aria-label"));
        Assert.False(input.HasAttribute("aria-invalid"));
        Assert.Equal("Drag files here or", cut.Find(".stil-dropzone-text").TextContent);
        Assert.Equal("Browse", cut.Find(".stil-dropzone-button").TextContent);
        Assert.Equal("true", cut.Find(".stil-dropzone-button").GetAttribute("aria-hidden"));
        Assert.Empty(cut.FindAll(".stil-file-list"));
        Assert.Equal("status", cut.Find(".stil-visually-hidden").GetAttribute("role"));
    }

    [Fact]
    public void GeneratesIdWhenNotSet()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFileUpload>(parameters => parameters.Add(component => component.Label, "Files"));

        var id = cut.Find("input").Id;
        Assert.StartsWith("stil-file-", id);
        Assert.Equal(id, cut.Find("label").GetAttribute("for"));
    }

    [Fact]
    public void MultipleAcceptDisabledTextsAndIcon()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Multiple, true)
            .Add(component => component.Accept, ".pdf,image/*")
            .Add(component => component.Disabled, true)
            .Add(component => component.Text, "Drop here")
            .Add(component => component.ButtonText, "Choose")
            .Add(component => component.Icon, StilIcons.Plus));

        var input = cut.Find("input");
        Assert.True(input.HasAttribute("multiple"));
        Assert.Equal(".pdf,image/*", input.GetAttribute("accept"));
        Assert.True(input.HasAttribute("disabled"));
        Assert.Equal("Drop here", cut.Find(".stil-dropzone-text").TextContent);
        Assert.Equal("Choose", cut.Find(".stil-dropzone-button").TextContent);
        Assert.Contains("stil-dropzone-icon", cut.Find("svg").ClassList);
        // Without a Label the input is named by the button text.
        Assert.Equal("Choose", input.GetAttribute("aria-label"));
    }

    [Fact]
    public void EmptyTextsAreNotRendered()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Text, null)
            .Add(component => component.ButtonText, null));

        Assert.Empty(cut.FindAll(".stil-dropzone-text, .stil-dropzone-button"));
    }

    [Fact]
    public void HintIsReferencedByAriaDescribedBy()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Id, "docs")
            .Add(component => component.Hint, "PDF only."));

        Assert.Equal("docs-hint", cut.Find(".stil-field-hint").Id);
        Assert.Equal("docs-hint", cut.Find("input").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void ClassAndStyleGoOnWrapperOtherAttributesOnInput()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilFileUpload>(parameters => parameters
            .AddUnmatched("class", "stil-col-6")
            .AddUnmatched("style", "order: 1")
            .AddUnmatched("aria-label", "Attachments")
            .AddUnmatched("capture", "user"));

        var field = cut.Find("div.stil-field");
        Assert.Equal(["stil-field", "stil-col-6"], field.ClassList);
        Assert.Equal("order: 1", field.GetAttribute("style"));
        var input = cut.Find("input");
        Assert.Equal("Attachments", input.GetAttribute("aria-label"));
        Assert.Equal("user", input.GetAttribute("capture"));
        Assert.Equal(["stil-dropzone-input"], input.ClassList);
        Assert.False(input.HasAttribute("style"));
    }

    [Fact]
    public void UploadListsFilesWithFormattedSizeAndRaisesOnChange() => WithInvariantCulture(() =>
    {
        using var context = new BunitContext();
        IReadOnlyList<IBrowserFile>? changed = null;
        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Multiple, true)
            .Add(component => component.OnChange, files => changed = files));

        cut.FindComponent<InputFile>().UploadFiles(File("a.txt", 500), File("b.pdf", 1536), File("c.zip", 3 * 1024 * 1024));

        Assert.NotNull(changed);
        Assert.Equal(["a.txt", "b.pdf", "c.zip"], changed.Select(f => f.Name));
        var items = cut.FindAll(".stil-file-list > li.stil-file-list-item");
        Assert.Equal(3, items.Count);
        Assert.Equal("a.txt", items[0].QuerySelector(".stil-file-list-name")!.TextContent);
        Assert.Equal("500 B", items[0].QuerySelector(".stil-file-list-size")!.TextContent);
        Assert.Equal("1.5 KB", items[1].QuerySelector(".stil-file-list-size")!.TextContent);
        Assert.Equal("3 MB", items[2].QuerySelector(".stil-file-list-size")!.TextContent);
        Assert.Equal("Remove a.txt", items[0].QuerySelector("button.stil-close")!.GetAttribute("aria-label"));
        Assert.Equal("Selected files", cut.Find(".stil-file-list").GetAttribute("aria-label"));
        Assert.Equal("3 file(s) selected.", cut.Find("[role=status]").TextContent);
        Assert.Equal(3, cut.Instance.Files.Count);
    });

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(10_444, "10.2 KB")]
    [InlineData(1024 * 1024, "1 MB")]
    [InlineData(5L * 1024 * 1024 * 1024, "5 GB")]
    public void FormatSizeUsesBinaryUnits(long bytes, string expected) =>
        WithInvariantCulture(() => Assert.Equal(expected, StilFileUpload.FormatSize(bytes)));

    [Fact]
    public void SingleFileModeReplacesTheFile()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>();

        cut.FindComponent<InputFile>().UploadFiles(File("a.txt"));
        cut.FindComponent<InputFile>().UploadFiles(File("b.txt"));

        Assert.Equal(["b.txt"], cut.FindAll(".stil-file-list-name").Select(e => e.TextContent));
    }

    [Fact]
    public void MultipleModeAddsToTheList()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>(parameters => parameters.Add(component => component.Multiple, true));

        cut.FindComponent<InputFile>().UploadFiles(File("a.txt"));
        cut.FindComponent<InputFile>().UploadFiles(File("b.txt"));

        Assert.Equal(["a.txt", "b.txt"], cut.FindAll(".stil-file-list-name").Select(e => e.TextContent));
    }

    [Fact]
    public void RemoveButtonRemovesFileAndRaisesOnChange()
    {
        using var context = new BunitContext();
        IReadOnlyList<IBrowserFile>? changed = null;
        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Multiple, true)
            .Add(component => component.OnChange, files => changed = files));
        cut.FindComponent<InputFile>().UploadFiles(File("a.txt"), File("b.txt"));

        cut.Find("button[aria-label=\"Remove a.txt\"]").Click();

        Assert.Equal(["b.txt"], changed!.Select(f => f.Name));
        Assert.Equal(["b.txt"], cut.FindAll(".stil-file-list-name").Select(e => e.TextContent));
        Assert.Equal("1 file(s) selected.", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void DisabledHidesRemoveButtons()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>();
        cut.FindComponent<InputFile>().UploadFiles(File("a.txt"));

        cut.Render(parameters => parameters.Add(component => component.Disabled, true));

        Assert.Single(cut.FindAll(".stil-file-list-item"));
        Assert.Empty(cut.FindAll("button.stil-close"));
    }

    [Fact]
    public void TooLargeFilesAreRejectedWithLinkedError() => WithInvariantCulture(() =>
    {
        using var context = new BunitContext();
        IReadOnlyList<IBrowserFile>? changed = null;
        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Id, "docs")
            .Add(component => component.Hint, "Up to 1 KB.")
            .Add(component => component.Multiple, true)
            .Add(component => component.MaxFileSize, 1024)
            .Add(component => component.OnChange, files => changed = files));

        cut.FindComponent<InputFile>().UploadFiles(File("small.txt", 100), File("big.txt", 2048));

        Assert.Equal(["small.txt"], changed!.Select(f => f.Name));
        var error = cut.Find(".stil-field-error");
        Assert.Equal("docs-error", error.Id);
        Assert.Equal("big.txt is larger than 1 KB.", error.TextContent.Trim());
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("docs-hint docs-error", input.GetAttribute("aria-describedby"));
        Assert.Contains("big.txt is larger than 1 KB.", cut.Find("[role=status]").TextContent);
    });

    [Fact]
    public void FilesOverMaxFilesAreRejected()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Multiple, true)
            .Add(component => component.MaxFiles, 2));

        cut.FindComponent<InputFile>().UploadFiles(File("a.txt"), File("b.txt"), File("c.txt"));

        Assert.Equal(["a.txt", "b.txt"], cut.FindAll(".stil-file-list-name").Select(e => e.TextContent));
        Assert.Equal("c.txt was not added. You can add up to 2 files.", cut.Find(".stil-field-error").TextContent.Trim());
    }

    [Fact]
    public void FilesNotMatchingAcceptAreRejected()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Multiple, true)
            .Add(component => component.Accept, ".pdf, image/*, text/csv"));

        cut.FindComponent<InputFile>().UploadFiles(
            File("report.PDF", contentType: "application/pdf"),
            File("photo.jpg", contentType: "image/jpeg"),
            File("data.csv", contentType: "text/csv"),
            File("notes.txt", contentType: "text/plain"));

        Assert.Equal(["report.PDF", "photo.jpg", "data.csv"], cut.FindAll(".stil-file-list-name").Select(e => e.TextContent));
        Assert.Equal("notes.txt is not an accepted file type.", cut.Find(".stil-field-error").TextContent.Trim());
    }

    [Fact]
    public void RejectionsClearOnNextChange()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>(parameters => parameters.Add(component => component.MaxFileSize, 10));
        cut.FindComponent<InputFile>().UploadFiles(File("big.txt", 20));
        Assert.Single(cut.FindAll(".stil-field-error"));

        cut.FindComponent<InputFile>().UploadFiles(File("ok.txt", 5));

        Assert.Empty(cut.FindAll(".stil-field-error"));
        Assert.False(cut.Find("input").HasAttribute("aria-invalid"));
    }

    [Fact]
    public void CustomMessageTexts()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>(parameters => parameters
            .Add(component => component.Accept, ".pdf")
            .Add(component => component.WrongTypeText, "Fel filtyp: {0}")
            .Add(component => component.RemoveText, "Ta bort {0}")
            .Add(component => component.SelectedText, "{0} valda")
            .Add(component => component.ListLabel, "Valda filer"));

        cut.FindComponent<InputFile>().UploadFiles(File("a.txt"));
        Assert.Equal("Fel filtyp: a.txt", cut.Find(".stil-field-error").TextContent.Trim());

        cut.FindComponent<InputFile>().UploadFiles(File("a.pdf"));
        Assert.Equal("Ta bort a.pdf", cut.Find("button.stil-close").GetAttribute("aria-label"));
        Assert.Equal("Valda filer", cut.Find(".stil-file-list").GetAttribute("aria-label"));
        Assert.Equal("1 valda", cut.Find("[role=status]").TextContent);
    }

    [Fact]
    public void DragEnterAndLeaveToggleDragState()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>();
        Assert.False(cut.Find(".stil-dropzone").HasAttribute("data-dragover"));

        cut.Find("input").DragEnter();
        Assert.Equal("true", cut.Find(".stil-dropzone").GetAttribute("data-dragover"));

        cut.Find("input").DragLeave();
        Assert.False(cut.Find(".stil-dropzone").HasAttribute("data-dragover"));

        cut.Find("input").DragEnter();
        cut.Find("input").Drop();
        Assert.False(cut.Find(".stil-dropzone").HasAttribute("data-dragover"));
    }

    [Fact]
    public void ClearEmptiesTheList()
    {
        using var context = new BunitContext();
        var cut = context.Render<StilFileUpload>();
        cut.FindComponent<InputFile>().UploadFiles(File("a.txt"));

        cut.InvokeAsync(cut.Instance.Clear);

        Assert.Empty(cut.FindAll(".stil-file-list"));
        Assert.Empty(cut.Instance.Files);
    }
}
