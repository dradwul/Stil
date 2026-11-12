# Stil.RichText

Rich text for [Stil for Blazor](https://github.com/dradwul/Stil/tree/main/src/Stil.Blazor):
`StilRichTextEditor` writes formatted text without Markdown, and
`StilRichTextView` shows it.

- The output is sanitized HTML, or Markdown if you prefer to store that.
- Sanitizing keeps an allow-list of tags: paragraphs, line breaks, bold,
  italic, underline, strikethrough, headings, lists, links, quotes and code.
  Every attribute except a safe link address is dropped, scripts and embedded
  content are removed, and the HTML is written anew from the parsed
  document, never copied from the input.
- Pastes from Word, Google Docs and web pages come in as plain, tidy text with
  their structure kept.
- Built on [AngleSharp](https://anglesharp.github.io/) (MIT) for parsing, and
  on `Stil.Markdown` for Markdown. It is a package of its own, so the core
  `Stil` package stays free of dependencies.

> **Status:** stable (`1.0`). Requires .NET 10 and the `Stil` package. The
> API changes only in a major version ([Semantic Versioning](https://semver.org/)).

## Get started

1. Install the package next to `Stil`, and add the namespace to
   `_Imports.razor`:

   ```shell
   dotnet add package Stil.RichText
   ```

   ```razor
   @using Stil.RichText
   ```

2. Write rich text. The editor binds like a field and works with EditForm
   validation (`[Required]`, `[MaxLength]`); empty text is null. It needs an
   interactive render mode:

   ```razor
   <StilRichTextEditor @bind-Value="product.Description" Label="Description" />
   ```

3. Show it. The view sanitizes again, so text saved elsewhere is safe too, and
   it works in static SSR:

   ```razor
   <StilRichTextView Value="@product.Description" />
   ```

## Components

| Component | Parameters |
| --- | --- |
| `StilRichTextEditor` | `@bind-Value`, `Format` (`Html`, `Markdown`), `Label`, `Hint`, `Placeholder`, `Disabled`, and the other field parameters |
| `StilRichTextView` | `Value`, `Format` |

The toolbar has bold, italic, heading, bulleted and numbered lists, quote,
code, link and clear formatting; Ctrl+B, Ctrl+I and Ctrl+K work in the text.
`StilRichText.Sanitize`, `ToMarkdown`, `FromMarkdown` and `HasText` do the
same on the server.

## License

[MIT](https://github.com/dradwul/Stil/blob/main/LICENSE). AngleSharp is MIT;
Markdig (through Stil.Markdown) is BSD-2-Clause.
