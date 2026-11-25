namespace Stil.Mcp.Tests;

/// <summary>Every tool, called through the SDK's client against the server over stdio.</summary>
[Collection("Server")]
public sealed class StilToolsTests(StilServerFixture server)
{
    [Fact]
    public async Task The_server_says_who_it_is_and_how_to_use_Stil()
    {
        Assert.Equal("Stil", server.Client.ServerInfo.Name);
        Assert.Contains("get_component", server.Client.ServerInstructions);
        Assert.Contains("Bootstrap", server.Client.ServerInstructions);
    }

    [Fact]
    public async Task It_lists_its_tools_as_read_only()
    {
        var tools = await server.Client.ListToolsAsync();

        Assert.Equal(
            ["add_to_existing_app", "get_component", "get_example", "get_type", "list_components", "list_css_classes", "list_tokens", "new_project", "search"],
            tools.Select(t => t.Name).Order(StringComparer.Ordinal));
        Assert.All(tools, tool =>
        {
            Assert.False(string.IsNullOrWhiteSpace(tool.Description));
            Assert.True(tool.ProtocolTool.Annotations?.ReadOnlyHint);
        });
    }

    [Fact]
    public async Task List_components_groups_them_by_docs_page()
    {
        var text = await server.CallAsync("list_components");

        Assert.Contains("## Buttons and badges", text);
        Assert.Contains("`StilButton`", text);
        Assert.Contains("`StilDataGrid<TItem>`", text);
        Assert.Contains("`StilLineChart<TItem>`", text);
        Assert.DoesNotContain("## Other components\n\n", text);
    }

    [Fact]
    public async Task List_components_filters_by_package()
    {
        var text = await server.CallAsync("list_components", new() { ["package"] = "Stil.Charts" });

        Assert.Contains("`StilLineChart<TItem>`", text);
        Assert.DoesNotContain("`StilButton`", text);
        Assert.Contains("The packages are", await server.CallAsync("list_components", new() { ["package"] = "Nope" }));
    }

    [Fact]
    public async Task Get_component_gives_its_parameters_enums_docs_and_examples()
    {
        var text = await server.CallAsync("get_component", new() { ["name"] = "button" });

        Assert.StartsWith("# StilButton", text);
        Assert.Contains("- Package: `Stil`", text);
        Assert.Contains("`@using Stil.Blazor.Components`", text);
        Assert.Contains("| `Variant` | `StilVariant` | `StilVariant.Primary` | The color. The default is Primary. |", text);
        Assert.Contains("| `Appearance` | `StilButtonAppearance` |", text);
        Assert.Contains("- `StilButtonAppearance`: Solid, Outline, Ghost, OnColor", text);
        Assert.Contains("- `StilVariant`: Primary, Secondary, Success, Danger, Warning, Info", text);
        Assert.Contains("https://dradwul.github.io/Stil/components/buttons.md", text);
        Assert.Contains("```razor\n", text);
        Assert.Contains("<StilButton", text);
    }

    [Fact]
    public async Task Get_component_marks_required_parameters_and_type_parameters()
    {
        var text = await server.CallAsync("get_component", new() { ["name"] = "StilDataGrid<TItem>", ["examples"] = 0 });

        Assert.StartsWith("# StilDataGrid<TItem>", text);
        Assert.Contains("Type parameters: `TItem`", text);
        Assert.Contains("| `Items` |", text);
        Assert.DoesNotContain("## Examples", text);
        Assert.Contains("More examples with get_example: ", text);
    }

    [Fact]
    public async Task Get_component_suggests_names_for_an_unknown_one()
    {
        var text = await server.CallAsync("get_component", new() { ["name"] = "StilDate" });

        Assert.StartsWith("No Stil component is called StilDate.", text);
        Assert.Contains("Did you mean", text);
        Assert.Contains("StilDateRangePicker", text);
    }

    [Fact]
    public async Task Get_type_gives_enum_values_and_service_members()
    {
        var variant = await server.CallAsync("get_type", new() { ["name"] = "StilVariant" });
        Assert.Contains("- Kind: enum", variant);
        Assert.Contains("- `Danger`", variant);

        var toasts = await server.CallAsync("get_type", new() { ["name"] = "IStilToastService" });
        Assert.Contains("- Kind: interface", toasts);
        Assert.Contains("Success(", toasts);
    }

    [Fact]
    public async Task Search_finds_components_pages_classes_and_tokens()
    {
        var toast = await server.CallAsync("search", new() { ["query"] = "toast" });
        Assert.Contains("[Dialogs, drawers and toasts]", toast);
        Assert.Contains("`StilToastHost`", toast);

        var sticky = await server.CallAsync("search", new() { ["query"] = "sticky" });
        Assert.Contains("`stil-sticky-top`", sticky);
        Assert.Contains("`--stil-sticky-offset`", sticky);

        Assert.StartsWith("Nothing in Stil matches", await server.CallAsync("search", new() { ["query"] = "qwertyuiop" }));
    }

    [Fact]
    public async Task List_css_classes_gives_families_or_the_classes_of_a_prefix()
    {
        var families = await server.CallAsync("list_css_classes");
        Assert.Contains("- `stil-btn`: ", families);
        Assert.Contains("- `stil-hero`: ", families);
        Assert.Contains("(stil-blocks.css)", families);

        var buttons = await server.CallAsync("list_css_classes", new() { ["prefix"] = "btn" });
        Assert.Contains("- `stil-btn-primary`", buttons);
        Assert.Contains("- `stil-btn-on-color`", buttons);
        Assert.DoesNotContain("stil-badge", buttons);

        Assert.StartsWith("No Stil class starts with stil-nope", await server.CallAsync("list_css_classes", new() { ["prefix"] = "stil-nope" }));
    }

    [Fact]
    public async Task List_tokens_gives_names_and_defaults()
    {
        var text = await server.CallAsync("list_tokens", new() { ["prefix"] = "radius" });

        Assert.StartsWith("# Tokens that start with --stil-radius", text);
        Assert.Contains("- `--stil-radius-md`: `.25rem`", text);
        Assert.DoesNotContain("--stil-space", text);
    }

    [Fact]
    public async Task Get_example_gives_a_source_or_lists_examples()
    {
        var source = await server.CallAsync("get_example", new() { ["name"] = "Buttons/Variants" });
        Assert.StartsWith("# Buttons/Variants", source);
        Assert.Contains("<StilButton Variant=\"StilVariant.Primary\">", source);

        var grid = await server.CallAsync("get_example", new() { ["name"] = "StilDataGrid" });
        Assert.Contains("- DataGrid/", grid);

        Assert.Contains("- Buttons: ", await server.CallAsync("get_example"));
    }

    [Fact]
    public async Task New_project_gives_the_wizards_command()
    {
        var text = await server.CallAsync("new_project", new()
        {
            ["name"] = "Shop",
            ["starter"] = "admin",
            ["features"] = "charts, kanban",
            ["primary"] = "#7c3aed",
            ["culture"] = "sv",
        });

        Assert.Contains("dotnet new install Stil.Templates", text);
        Assert.Contains("dotnet new stil-app -n Shop --Starter admin --Features charts kanban --Primary \"#7c3aed\" --Culture sv", text);
        Assert.Contains("Stil.Charts", text);
        Assert.DoesNotContain("## Options", text);
    }

    [Fact]
    public async Task New_project_lists_the_options_and_reports_what_it_could_not_use()
    {
        var plain = await server.CallAsync("new_project");
        Assert.Contains("dotnet new stil-app -n MyApp\n", plain);
        Assert.Contains("- `planner`: ", plain);
        Assert.Contains("- `gantt`: ", plain);

        var site = await server.CallAsync("new_project", new() { ["starter"] = "website", ["font"] = "comic" });
        Assert.Contains("dotnet new stil-site -n MyApp", site);
        Assert.Contains("There is no font \"comic\"", site);
    }

    [Fact]
    public async Task Add_to_existing_app_shows_the_installation_page_snippets()
    {
        var text = await server.CallAsync("add_to_existing_app");

        Assert.Contains("dotnet add package Stil", text);
        Assert.Contains("builder.Services.AddStil(", text);
        Assert.Contains("<StilHead", text);
        Assert.DoesNotContain("Snippet", text);
    }
}
