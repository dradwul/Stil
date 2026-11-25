# Stil.Mcp

An [MCP](https://modelcontextprotocol.io/) server for
[Stil](https://github.com/dradwul/Stil). Assistants such as Claude Code, GitHub
Copilot and Cursor can ask it about Stil, so the code they write uses Stil's real
components, parameters and classes instead of guesses or Bootstrap.

- **What it knows:**
  - every component and its parameters (types, defaults, descriptions and the values of the enums they take);
  - the other public types, such as `StilVariant`, `StilTheme` and `IStilDialogService`;
  - every CSS class and `--stil-*` token;
  - the docs site's examples;
  - the `dotnet new` command for a new app.
- **Where it comes from:** the same sources as the
  [docs site](https://dradwul.github.io/Stil/). These are the libraries
  themselves, their XML documentation, the docs pages and examples, and the
  built CSS. Its answers match the version you install.
- **How it runs:** locally, over stdio. It reads nothing from your project, sends
  nothing anywhere and changes no files.

> **Status:** new in `1.3`. Requires the .NET 10 SDK, which runs it with `dnx`.

## Add it to your assistant

The server is started with `dnx Stil.Mcp --yes`, which fetches the package from
nuget.org the first time. Add `@1.3.0` to the package ID to pin a version.

**Claude Code**

```shell
claude mcp add stil -- dnx Stil.Mcp --yes
```

**VS Code (GitHub Copilot):** in `.vscode/mcp.json`:

```json
{
  "servers": {
    "stil": {
      "type": "stdio",
      "command": "dnx",
      "args": ["Stil.Mcp", "--yes"]
    }
  }
}
```

**Visual Studio:** use the same content in `.mcp.json` next to the solution.

**Cursor:** in `.cursor/mcp.json`:

```json
{
  "mcpServers": {
    "stil": {
      "command": "dnx",
      "args": ["Stil.Mcp", "--yes"]
    }
  }
}
```

## Tools

| Tool | What it answers |
| --- | --- |
| `list_components` | Every component, grouped by docs page, optionally for one package. |
| `get_component` | A component's package, namespace, parameters, enum values, docs page and examples. |
| `get_type` | An enum's values, or the members of an option, model or service. |
| `search` | Components, types, docs pages, CSS classes and tokens that match a word. |
| `list_css_classes` | The class families, or every class with a prefix such as `stil-btn`. |
| `list_tokens` | The `--stil-*` tokens and their defaults. |
| `get_example` | A docs example's Razor source, or the examples of a component. |
| `new_project` | The `dotnet new stil-app` or `stil-site` command for a starter and features. |
| `add_to_existing_app` | The package, `Program.cs`, `App.razor` and layout setup of an existing app. |

Every tool is read-only. When an assistant connects, the server also tells it to
use Stil components and `stil-*` classes, and to look a component up before it
uses one.

For assistants without MCP, the docs are also Markdown:
[`llms.txt`](https://dradwul.github.io/Stil/llms.txt) and
[`llms-full.txt`](https://dradwul.github.io/Stil/llms-full.txt).

## Licence

MIT, like the rest of Stil.
