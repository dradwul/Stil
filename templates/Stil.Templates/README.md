# Stil for Blazor templates

`dotnet new` templates for [Stil for Blazor](https://github.com/dradwul/Stil/tree/main/src/Stil.Blazor),
a set of themeable Blazor components built on Stil CSS.

| Template | Short name | What you get |
| --- | --- | --- |
| Stil for Blazor App | `stil-app` | A Blazor Web App with Stil set up: a navbar, a color-mode switch, a theme, dialogs and toasts, and optional sample pages. |

> **Status:** early preview (`0.1.x`, prereleases on NuGet). Requires the .NET 10 SDK.

## Install

Install the templates from nuget.org (the latest version, a preview until
`1.0`; see [nuget.org](https://www.nuget.org/packages/Stil.Templates)):

```shell
dotnet new install Stil.Templates
```

From a clone of the repository, install the folder instead:

```shell
dotnet new install templates/Stil.Templates/content/StilApp
```

To remove them again, run `dotnet new uninstall Stil.Templates` (or the folder path).

## Create an app

```shell
dotnet new stil-app -n Contoso.Shop
cd Contoso.Shop
dotnet run
```

The app references the `Stil` version the templates were released with
(`--StilVersion` picks another). To try an unreleased build of `Stil`, see
[Try a local build in another app](https://github.com/dradwul/Stil#try-a-local-build-in-another-app).

## Options

| Option | Values | Default | Description |
| --- | --- | --- | --- |
| `--Interactivity` | `Server`, `None` | `Server` | `Server` makes the whole app interactive (Interactive Server). `None` gives static server rendering only. |
| `--IncludeSampleContent` | `true`, `false` | `true` | Adds a dashboard with KPI cards (`StilStatCard`) and a `StilDataGrid`, and a form page with an `EditForm` and validation. With `false` the app has one empty Home page. |
| `--Primary` | a CSS color, for example `#7048e8` | Stil's teal (`#34dbbf`) | The brand color. It is set with `AddStil()` in `Program.cs`, so it applies to the whole app. |
| `--Font` | `lexend`, `br-cobane`, `br-segma`, `br-sonoma`, `br-shape`, `averral`, `sukplena`, `tasa-explorer`, `halcom`, `utara`, `geologica`, `blowreph`, `taster`, `warm-stories`, `system` | `lexend` | The font, from cdnfonts.com (check its license there). It is set with `options.Font` in `Program.cs`. `system` downloads nothing and uses the operating system's font. |
| `--GoogleFont` | a Google font, for example `"Open Sans"` | none | Any Google font by its family name. It replaces `--Font`. |
| `--ColorMode` | `light`, `dark`, `auto` | `auto` | The color mode the app starts in. `auto` follows the operating system. Users can change it with the switch in the navbar. |
| `--Culture` | `en`, `sv`, `nb`, `da`, `fi` | `en` | The default language, for visitors whose browser asks for none of the offered ones. Users can change it with the language selector in the navbar. Stil's components have built-in texts in all five; the app's own texts are in `Locales/*.json` (English and Swedish to start with) and are read with `IStringLocalizer<T>`. |
| `--StilVersion` | a version | `0.1.0` | The version of the `Stil` package to reference. |
| `-f`, `--framework` | `net10.0` | `net10.0` | The target framework. |
| `--no-restore` | | | Skips the restore after creating the app. |

Run `dotnet new stil-app --help` for the full list.

## Examples

```shell
# An app with a purple brand color that starts in dark mode
dotnet new stil-app -n Contoso.Shop --Primary "#7048e8" --ColorMode dark

# Static server rendering, no sample pages
dotnet new stil-app -n Contoso.Site --Interactivity None --IncludeSampleContent false
```

## What the app contains

- `Program.cs` registers Stil with `AddStil()`, the app's translations with
  `AddStilLocalization()`, and the language cookie with `UseRequestLocalization()`.
- `AppCultures.cs` lists the languages and the default one; `Locales/en.json`
  and `Locales/sv.json` hold the app's own texts. Add a file per language.
- `Components/App.razor` adds the stylesheet with `<StilHead />` and sets
  `class="stil-scope"` on `<body>`.
- `Components/Layout/MainLayout.razor` has a `StilThemeProvider` around a
  `StilNavbar` (it collapses behind a menu button on small screens), a
  `StilNav` with `StilNavLink`s, a `StilLanguageSelector` and a color-mode
  switch. In the interactive app it also has `StilDialogHost` and `StilToastHost`, so
  `IStilDialogService` and `IStilToastService` work on every page.
- With `--Interactivity None`, the color-mode switch is a small form that
  keeps the choice in a cookie, and the data grid shows every row.
- `Components/Pages/NotFound.razor` and `Error.razor`.
- With the sample content: `Components/Pages/Home.razor` (the dashboard),
  `Components/Pages/NewOrder.razor` (the form) and `Data/`, an in-memory
  order store to replace with your own data access.

See the [Stil for Blazor README](https://github.com/dradwul/Stil/tree/main/src/Stil.Blazor#readme)
for all components and theming.

## Development

`templates/test-templates.mjs` packs `src/Stil.Blazor` and this package into a
temp folder, installs the template into an isolated template hive, creates
an app for several option combinations and builds each with
`-warnaserror`:

```shell
node templates/test-templates.mjs          # add --keep to keep the generated apps
```

It does not change your installed templates or your NuGet packages folder.
