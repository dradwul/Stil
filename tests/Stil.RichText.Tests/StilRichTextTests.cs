using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace Stil.RichText.Tests;

public class StilRichTextTests
{
    [Theory]
    [InlineData("<p>Hi <script>alert(1)</script>there</p>", "<p>Hi there</p>")]
    [InlineData("<p onclick=\"steal()\" style=\"color:red\" class=\"x\">Text</p>", "<p>Text</p>")]
    [InlineData("<a href=\"javascript:alert(1)\">x</a>", "<a>x</a>")]
    [InlineData("<a href=\"https://example.com\" target=\"_blank\">x</a>", "<a href=\"https://example.com\" rel=\"noopener noreferrer\">x</a>")]
    [InlineData("<a href=\"/orders/1\">x</a>", "<a href=\"/orders/1\">x</a>")]
    [InlineData("<img src=x onerror=alert(1)><iframe src=\"https://evil\"></iframe><p>ok</p>", "<p>ok</p>")]
    [InlineData("<p>a &lt;b&gt; &amp; c</p>", "<p>a &lt;b&gt; &amp; c</p>")]
    [InlineData("<b>bold</b> <i>it</i> <strike>gone</strike> <h1>Big</h1>", "<strong>bold</strong> <em>it</em> <del>gone</del><h2>Big</h2>")]
    [InlineData("<svg><a href=\"https://x\">in svg</a></svg><p>after</p>", "<p>after</p>")]
    [InlineData("<p><span style=\"font-size:20px\"><font color=red>Plain</font></span></p>", "<p>Plain</p>")]
    [InlineData("<p>One</p><p></p><p><br></p><h2> </h2><ul><li></li></ul><p>Two</p>", "<p>One</p><p>Two</p>")]
    public void SanitizeKeepsOnlyAllowedTags(string html, string expected)
    {
        Assert.Equal(expected, StilRichText.Sanitize(html).Replace("\n", ""));
    }

    [Fact]
    public void SanitizingAgainChangesNothing()
    {
        const string html = "<h2>Desk</h2>\n\n<ul>\n  <li>160 × 80 cm, åtta</li>\n</ul>\n<p>A <strong>b</strong> <em>c</em></p>";

        var once = StilRichText.Sanitize(html);

        Assert.Equal("<h2>Desk</h2>\n<ul><li>160 × 80 cm, åtta</li>\n</ul>\n<p>A <strong>b</strong> <em>c</em></p>", once);
        Assert.Equal(once, StilRichText.Sanitize(once));
    }

    [Fact]
    public void WordAndGoogleDocsPastesComeOutPlain()
    {
        const string word = """
            <html xmlns:o="urn:schemas-microsoft-com:office:office"><head><style>p.MsoNormal{margin:0}</style></head>
            <body><!--StartFragment--><p class=MsoNormal><b><span lang=SV style='font-family:"Calibri"'>Delivery</span></b><o:p></o:p></p>
            <p class=MsoListParagraph style='mso-list:l0 level1 lfo1'><span>Chairs</span></p><!--EndFragment--></body></html>
            """;
        const string docs = "<b style=\"font-weight:normal;\" id=\"docs-internal-guid-1\"><p dir=\"ltr\"><span style=\"font-weight:700\">Title</span></p></b>";

        Assert.Equal("<p><strong>Delivery</strong></p><p>Chairs</p>", StilRichText.Sanitize(word).Replace("\n", ""));
        Assert.Equal("<p>Title</p>", StilRichText.Sanitize(docs).Replace("\n", ""));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<p><br></p>")]
    [InlineData("<script>x</script>")]
    public void EmptyContentIsEmpty(string? html)
    {
        Assert.Equal("", StilRichText.Sanitize(html));
        Assert.False(StilRichText.HasText(html));
    }

    [Fact]
    public void HtmlTurnsIntoMarkdown()
    {
        var markdown = StilRichText.ToMarkdown("""
            <h2>Order *1042*</h2>
            <p><strong>Rush</strong> and <em>fragile</em>, see <a href="https://example.com/a">the note</a>.<br>Second line</p>
            <ul><li>Chairs<ul><li>Black</li></ul></li><li>Desks</li></ul>
            <ol><li>Pack</li><li>Ship</li></ol>
            <blockquote><p>Handle with care</p></blockquote>
            <pre><code>SKU-1042</code></pre>
            """);

        Assert.Equal("""
            ## Order \*1042\*

            **Rush** and _fragile_, see [the note](https://example.com/a).\
            Second line

            - Chairs
              - Black
            - Desks

            1. Pack
            2. Ship

            > Handle with care

            ```
            SKU-1042
            ```

            """.Replace("\r\n", "\n"), markdown);
    }

    [Fact]
    public void MarkdownTurnsIntoSanitizedHtml()
    {
        Assert.Equal("<p><strong>Hi</strong> &lt;b&gt;</p>", StilRichText.FromMarkdown("**Hi** <b>").Trim());
    }

    [Fact]
    public void EditorSanitizesWhatItGetsAndKeepsTheCursor()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        string? value = "<p>Start</p>";

        var cut = context.Render<StilRichTextEditor>(parameters => parameters
            .Add(c => c.Label, "Description")
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        var editor = cut.Find("[data-stil-richtext]");
        Assert.Equal("true", editor.GetAttribute("contenteditable"));
        Assert.Equal("Description", editor.GetAttribute("aria-label"));
        Assert.Equal(editor.Id, cut.Find("[role=toolbar]").GetAttribute("aria-controls"));
        Assert.Equal(9, cut.FindAll("[data-stil-richtext-command]").Count);
        Assert.Equal("Start", editor.TextContent);

        cut.InvokeAsync(() => cut.Instance.OnHtml("<p>New <b>text</b><script>x()</script></p>"));
        Assert.Equal("<p>New <strong>text</strong></p>", value);
        // The editor's own change does not re-render its content.
        Assert.Equal("Start", cut.Find("[data-stil-richtext]").TextContent);

        cut.InvokeAsync(() => cut.Instance.OnHtml("<p><br></p>"));
        Assert.Null(value);
    }

    [Fact]
    public void EditorCanWriteMarkdown()
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        string? value = "**Bold** start";

        var cut = context.Render<StilRichTextEditor>(parameters => parameters
            .Add(c => c.Format, StilRichTextFormat.Markdown)
            .Add(c => c.Value, value)
            .Add(c => c.ValueChanged, v => value = v));

        Assert.Equal("Bold", cut.Find("[data-stil-richtext] strong").TextContent);

        cut.InvokeAsync(() => cut.Instance.OnHtml("<p>Now <em>italic</em></p>"));
        Assert.Equal("Now _italic_\n", value);
    }

    [Fact]
    public void ViewShowsSanitizedHtmlOrMarkdown()
    {
        using var context = new BunitContext();
        context.Services.AddStil();

        var html = context.Render<StilRichTextView>(parameters => parameters.Add(c => c.Value, "<p onclick=x()>Hi <script>bad()</script></p>"));
        var markdown = context.Render<StilRichTextView>(parameters => parameters.Add(c => c.Value, "# Title").Add(c => c.Format, StilRichTextFormat.Markdown));

        Assert.Equal(["stil-prose", "stil-richtext-view"], html.Find("div").ClassList);
        Assert.Equal("<p>Hi </p>", html.Find("div").InnerHtml.Trim());
        Assert.Equal("Title", markdown.Find("h2").TextContent);
    }
}
