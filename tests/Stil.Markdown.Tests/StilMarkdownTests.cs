using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace Stil.Markdown.Tests;

public class StilMarkdownTests
{
    [Fact]
    public void RendersCommonMarkWithTablesAndTaskLists()
    {
        var html = StilMarkdownRenderer.ToHtml("""
            ## Notes

            **Bold**, _italic_ and ~~gone~~.

            - [x] Packed
            - [ ] Shipped

            | Item | Qty |
            | --- | --- |
            | Chairs | 4 |
            """);

        Assert.Contains("<h2>Notes</h2>", html);
        Assert.Contains("<strong>Bold</strong>", html);
        Assert.Contains("<em>italic</em>", html);
        Assert.Contains("<del>gone</del>", html);
        Assert.Contains("type=\"checkbox\"", html);
        Assert.Contains("<table class=\"stil-table\">", html);
    }

    [Fact]
    public void TaskCheckboxesAreNamedByTheirText()
    {
        var html = StilMarkdownRenderer.ToHtml("""
            - [x] Invoices **sent** for `#1042`
            - [ ] Call "Kallio" & Co
            """);

        Assert.Contains("aria-label=\"Invoices sent for #1042\"", html);
        Assert.Contains("aria-label=\"Call &quot;Kallio&quot; &amp; Co\"", html);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>", "&lt;script&gt;")]
    [InlineData("<img src=x onerror=alert(1)>", "&lt;img")]
    [InlineData("[click](javascript:alert(1))", "href=\"#\"")]
    [InlineData("![x](data:image/svg+xml;base64,AAAA)", "src=\"#\"")]
    [InlineData("[mail](mailto:info@example.com)", "href=\"mailto:info@example.com\"")]
    [InlineData("[page](/orders/42)", "href=\"/orders/42\"")]
    [InlineData("[site](https://example.com)", "rel=\"noopener noreferrer\"")]
    public void KeepsHtmlAndUnsafeLinksOut(string markdown, string expected)
    {
        var html = StilMarkdownRenderer.ToHtml(markdown);

        Assert.Contains(expected, html);
        Assert.DoesNotContain("<script", html);
        Assert.DoesNotContain("javascript:", html);
    }

    [Fact]
    public void ViewRendersInProse()
    {
        using var context = new BunitContext();
        context.Services.AddStil();

        var cut = context.Render<StilMarkdown>(parameters => parameters.Add(c => c.Value, "Hello *there*"));

        Assert.Equal(["stil-prose", "stil-markdown"], cut.Find("div").ClassList);
        Assert.Equal("there", cut.Find("em").TextContent);
    }

    [Fact]
    public void EditorBindsAndPreviewsInATab()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        string? value = "# Title";

        var cut = context.Render<StilMarkdownEditor>(parameters => parameters
            .Add(c => c.Label, "Notes")
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        var textarea = cut.Find("textarea");
        Assert.True(textarea.HasAttribute("data-stil-markdown"));
        Assert.Equal(textarea.Id, cut.Find("[role=toolbar]").GetAttribute("aria-controls"));
        Assert.Equal(8, cut.FindAll("[data-stil-markdown-command]").Count);
        Assert.Equal("Bold (Ctrl+B)", cut.Find("[data-stil-markdown-command=bold]").GetAttribute("title"));

        textarea.Input("**Shipped**");
        Assert.Equal("**Shipped**", value);

        cut.FindAll("button").Single(b => b.TextContent == "Preview").Click();
        Assert.True(cut.Find("textarea").HasAttribute("hidden"));
        Assert.Equal("Shipped", cut.Find(".stil-markdown-preview strong").TextContent);
        Assert.True(cut.Find("[data-stil-markdown-command=bold]").HasAttribute("disabled"));
    }

    [Fact]
    public void EditorShowsValidation()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        var model = new Order();

        var cut = context.Render<EditForm>(parameters => parameters
            .Add(form => form.Model, model)
            .Add<RenderFragment<EditContext>?>(form => form.ChildContent, _ => builder =>
            {
                builder.OpenComponent<DataAnnotationsValidator>(0);
                builder.CloseComponent();
                builder.OpenComponent<StilMarkdownEditor>(1);
                builder.AddAttribute(2, nameof(StilMarkdownEditor.Value), model.Notes);
                builder.AddAttribute(3, nameof(StilMarkdownEditor.ValueChanged), EventCallback.Factory.Create<string?>(this, v => model.Notes = v));
                builder.AddAttribute(4, nameof(StilMarkdownEditor.ValueExpression), (System.Linq.Expressions.Expression<Func<string?>>)(() => model.Notes));
                builder.AddAttribute(5, nameof(StilMarkdownEditor.Preview), StilMarkdownPreview.Side);
                builder.CloseComponent();
            }));

        cut.Find("form").Submit();

        Assert.Equal("Write a note.", cut.Find(".stil-field-error").TextContent);
        Assert.Equal("true", cut.Find("textarea").GetAttribute("aria-invalid"));
        Assert.Contains("stil-markdown-editor-side", cut.Find(".stil-markdown-editor").ClassName);
        Assert.Equal("Nothing to preview", cut.Find(".stil-markdown-preview").TextContent.Trim());
    }

    private sealed class Order
    {
        [Required(ErrorMessage = "Write a note.")]
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
