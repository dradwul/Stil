# Stil.Markdown

Markdown for [Stil for Blazor](https://github.com/dradwul/Stil/tree/main/src/Stil.Blazor):
`StilMarkdown` shows Markdown with Stil's typography, and `StilMarkdownEditor`
writes it with a toolbar, keyboard shortcuts and a preview.

- Built on [Markdig](https://github.com/xoofx/markdig) (BSD licence), the
  standard .NET Markdown library. It is a package of its own, so the core
  `Stil` package stays free of dependencies.
- Safe by default: raw HTML in the Markdown is shown as text, and links and
  images only go to http, https, mailto and tel addresses or relative ones.
- CommonMark with tables, task lists, autolinks and strikethrough. The text
  uses `stil-prose` from Stil CSS, so it follows your theme and dark mode.

> **Status:** stable (`1.0`). Requires .NET 10 and the `Stil` package. The
> API changes only in a major version ([Semantic Versioning](https://semver.org/)).

## Get started

1. Install the package next to `Stil`, and add the namespace to
   `_Imports.razor`:

   ```shell
   dotnet add package Stil.Markdown
   ```

   ```razor
   @using Stil.Markdown
   ```

2. Show Markdown. It works in static SSR:

   ```razor
   <StilMarkdown Value="@order.Notes" />
   ```

3. Write it. The editor binds like a text area and works with EditForm
   validation (`[Required]`, `[MaxLength]`):

   ```razor
   <StilMarkdownEditor @bind-Value="order.Notes" Label="Notes" />
   ```

## Components

| Component | Parameters |
| --- | --- |
| `StilMarkdown` | `Value` |
| `StilMarkdownEditor` | `@bind-Value`, `Label`, `Hint`, `Rows` (8), `Placeholder`, `Preview` (`Tab`, `Side`, `None`), `OnImageUpload` (stores a chosen image and returns its URL), `Disabled`, and the other field parameters |

`StilMarkdownRenderer.ToHtml(markdown)` gives the same safe HTML on the
server, for example for an e-mail.

The toolbar has bold, italic, heading, bulleted and numbered lists, quote,
code and link; Ctrl+B, Ctrl+I and Ctrl+K work in the text. Stil's `stil.js`
(loaded automatically) formats the text, so Ctrl+Z undoes a change.

## License

[MIT](https://github.com/dradwul/Stil/blob/main/LICENSE). Markdig is BSD-2-Clause.
