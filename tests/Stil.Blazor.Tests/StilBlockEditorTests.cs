using System.Globalization;
using Bunit;
using Stil.Blazor.Blocks;

namespace Stil.Blazor.Tests;

public class StilBlockEditorTests
{
    private static (BunitContext Context, IRenderedComponent<StilBlockEditor> Editor, List<StilBlockData> Changes) Render(
        StilBlockData block, Action<ComponentParameterCollectionBuilder<StilBlockEditor>>? more = null)
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var changes = new List<StilBlockData>();
        var editor = context.Render<StilBlockEditor>(parameters =>
        {
            parameters.Add(c => c.Block, block).Add(c => c.BlockChanged, changed => changes.Add(changed));
            more?.Invoke(parameters);
        });
        return (context, editor, changes);
    }

    private static string LabelOf(IRenderedComponent<StilBlockEditor> editor, string controlId) =>
        editor.Find($"label[for='{controlId}']").TextContent.Trim();

    [Fact]
    public void GeneratesAFieldForEachSettingOfTheType()
    {
        var (context, editor, _) = Render(StilBlockData.Create("hero"));
        using var _ = context;

        Assert.Equal("Hero", editor.Find(".stil-block-editor-settings > legend").TextContent);
        var title = editor.Find("input[id$='-title']");
        Assert.Equal("Title", LabelOf(editor, title.Id!));
        Assert.True(title.HasAttribute("required"));
        Assert.Equal("textarea", editor.Find("[id$='-lead']").LocalName);
        Assert.Equal(["default", "muted", "brand"], editor.FindAll("select[id$='-tone'] option").Select(option => option.GetAttribute("value")));
        Assert.Equal("Picture", editor.Find(".stil-block-editor-image > legend").TextContent);
        Assert.Equal(2, editor.FindAll(".stil-block-editor-image input").Count);
        var anchor = editor.Find("input[id$='-id']");
        Assert.Contains($"{anchor.Id}-hint", anchor.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void TextIsSetAsYouTypeAndRemovedWhenEmpty()
    {
        var block = StilBlockData.Create("hero");
        var (context, editor, changes) = Render(block);
        using var _ = context;

        editor.Find("input[id$='-title']").Input("Plan the week");
        Assert.Equal("Plan the week", block.GetText("title"));
        Assert.Same(block, Assert.Single(changes));

        editor.Find("input[id$='-title']").Input("");
        Assert.False(block.Settings.ContainsKey("title"));
    }

    [Fact]
    public void ChoiceSwitchAndListSetTheirKinds()
    {
        var block = StilBlockData.Create("pricing").Add(StilBlockData.Create("plan"));
        var (context, editor, _) = Render(block);
        using var _ = context;

        editor.Find("select[id$='-tone']").Change("brand");
        Assert.Equal(StilBlockTone.Brand, block.GetChoice("tone", StilBlockTone.Default));
        editor.Find("select[id$='-tone']").Change("default");
        Assert.False(block.Settings.ContainsKey("tone"));

        var periodSwitch = editor.Find("input[id$='-periodSwitch']");
        Assert.Equal("switch", periodSwitch.GetAttribute("role"));
        periodSwitch.Change(true);
        Assert.True(block.GetBool("periodSwitch"));
        editor.Find("input[id$='-periodSwitch']").Change(false);
        Assert.False(block.Settings.ContainsKey("periodSwitch"));

        var features = editor.Find("textarea[id$='-0-features']");
        Assert.Contains("One per line.", editor.Find($"#{features.Id}-hint").TextContent);
        features.Change(" Plans \n\nReports\n");
        Assert.Equal(["Plans", "Reports"], block.Items[0].GetList("features"));
    }

    [Fact]
    public void NumberDateAndIconHaveTheirOwnControls()
    {
        var definition = new StilBlockDefinition("event", "Event",
        [
            new("seats", "Seats", StilBlockSettingKind.Number),
            new("day", "Day", StilBlockSettingKind.Date),
            new("icon", "Icon", StilBlockSettingKind.Icon),
        ]);
        var block = StilBlockData.Create("event");
        var (context, editor, _) = Render(block, parameters => parameters.Add(c => c.Definition, definition));
        using var _ = context;

        Assert.Equal("number", editor.Find("input[id$='-seats']").GetAttribute("type"));
        editor.Find("input[id$='-seats']").Change("12.5");
        Assert.Equal(12.5, block.GetNumber("seats"));

        Assert.Equal("date", editor.Find("input[id$='-day']").GetAttribute("type"));
        editor.Find("input[id$='-day']").Change("2026-12-01");
        Assert.Equal(new DateTime(2026, 12, 1), block.GetDate("day"));

        var icons = editor.FindAll("select[id$='-icon'] option").Select(option => option.GetAttribute("value")).ToList();
        Assert.Equal("", icons[0]);
        Assert.Contains("mail", icons);
        Assert.Empty(editor.FindAll(".stil-block-editor-icon svg"));
        editor.Find("select[id$='-icon']").Change("mail");
        Assert.Equal("mail", block.GetText("icon"));
        Assert.Single(editor.FindAll(".stil-block-editor-icon svg"));
    }

    [Fact]
    public void PictureKeepsItsAddressAndAltTextTogether()
    {
        var block = StilBlockData.Create("hero");
        var (context, editor, _) = Render(block);
        using var _ = context;

        editor.Find("input[id$='-image-alt']").Input("The planner");
        Assert.Null(block.GetImage("image"));
        editor.Find("input[id$='-image-src']").Input("hero.png");
        Assert.Equal(new StilBlockImage("hero.png", "The planner"), block.GetImage("image"));

        editor.Find("input[id$='-image-src']").Input("");
        editor.Find("input[id$='-image-alt']").Input("");
        Assert.False(block.Settings.ContainsKey("image"));
    }

    [Fact]
    public void ARequiredFieldIsMarkedOnceItHasBeenEmptied()
    {
        var block = StilBlockData.Create("hero").Set("title", "Plan");
        var (context, editor, _) = Render(block);
        using var _ = context;

        Assert.Null(editor.Find("input[id$='-title']").GetAttribute("aria-invalid"));
        editor.Find("input[id$='-title']").Input("");

        var title = editor.Find("input[id$='-title']");
        Assert.Equal("true", title.GetAttribute("aria-invalid"));
        Assert.Equal($"{title.Id}-error", title.GetAttribute("aria-describedby"));
        Assert.Equal("Required.", editor.Find($"#{title.Id}-error").TextContent.Trim());
        Assert.False(StilBlockCatalog.Default.Find("hero")!.IsComplete(block));
    }

    [Fact]
    public void ShowValidationMarksEveryEmptyRequiredField()
    {
        var block = StilBlockData.Create("cta").Add(StilBlockData.Create("action"));
        var (context, editor, _) = Render(block, parameters => parameters.Add(c => c.ShowValidation, true));
        using var _ = context;

        var prefix = editor.Find("input[id$='-title']").Id![..^"title".Length];
        Assert.Equal(["title", "0-text", "0-href"], editor.FindAll("[aria-invalid='true']").Select(field => field.Id![prefix.Length..]));
    }

    [Fact]
    public void ItemsAreAddedMovedAndRemoved()
    {
        var block = StilBlockData.Create("features")
            .Add(StilBlockData.Create("feature").Set("title", "A"))
            .Add(StilBlockData.Create("feature").Set("title", "B"));
        var (context, editor, changes) = Render(block);
        using var _ = context;

        Assert.Equal(["Feature 1", "Feature 2"], editor.FindAll(".stil-block-editor-item > legend").Select(legend => legend.TextContent));
        var first = editor.FindAll(".stil-block-editor-item")[0];
        Assert.True(first.QuerySelectorAll("button")[0].HasAttribute("disabled"));
        Assert.False(first.QuerySelectorAll("button")[1].HasAttribute("disabled"));

        editor.FindAll(".stil-block-editor-item")[0].QuerySelectorAll("button")[1].Click();
        Assert.Equal(["B", "A"], block.Items.Select(item => item.GetText("title")));
        Assert.Equal("Feature 2 moved down.", editor.Find("[role=status]").TextContent);
        Assert.Equal("A", editor.Find("input[id$='-1-title']").GetAttribute("value"));

        editor.FindAll(".stil-block-editor-items > div > button")[0].Click();
        Assert.Equal("feature", block.Items[2].Type);
        Assert.Equal("Feature 3 added.", editor.Find("[role=status]").TextContent);

        editor.FindAll(".stil-block-editor-item")[0].QuerySelectorAll("button")[2].Click();
        Assert.Equal(["A", null], block.Items.Select(item => item.GetText("title")));
        Assert.Equal("Feature 1 removed.", editor.Find("[role=status]").TextContent);
        Assert.Equal(3, changes.Count);
    }

    [Fact]
    public void TheAddButtonGivesWayToANoteAtTheMaximum()
    {
        var block = StilBlockData.Create("hero").Add(StilBlockData.Create("action")).Add(StilBlockData.Create("action"));
        var (context, editor, _) = Render(block);
        using var _ = context;

        Assert.Equal("Add Button", editor.Find(".stil-block-editor-items > div > button").TextContent.Trim());
        editor.Find(".stil-block-editor-items > div > button").Click();

        Assert.Equal(3, block.Items.Count);
        Assert.Empty(editor.FindAll(".stil-block-editor-items > div > button"));
        Assert.Equal("Up to 3.", editor.Find(".stil-block-editor-items > p.stil-field-hint").TextContent);
    }

    [Fact]
    public void TextsFollowTheCulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("sv-SE");
        try
        {
            var (context, editor, _) = Render(StilBlockData.Create("hero").Add(StilBlockData.Create("action")));
            using var _ = context;

            Assert.Equal(["Flytta upp", "Flytta ned", "Ta bort"], editor.FindAll(".stil-block-editor-actions button").Select(button => button.TextContent.Trim()));
            Assert.Equal("Lägg till Button", editor.Find(".stil-block-editor-items > div > button").TextContent.Trim());
            Assert.Equal("Alt-text", LabelOf(editor, editor.Find("input[id$='-image-alt']").Id!));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void AnUnknownTypeRendersNothing()
    {
        var (context, editor, _) = Render(StilBlockData.Create("nope"));
        using var _ = context;

        Assert.Equal("", editor.Markup.Trim());
    }

    [Fact]
    public void IsCompleteChecksTheBlockAndItsItems()
    {
        var cta = StilBlockCatalog.Default.Find("cta")!;
        var block = StilBlockData.Create("cta").Set("title", "Start").Add(StilBlockData.Create("action").Set("text", "Go"));

        Assert.False(cta.IsComplete(block));
        block.Items[0].Set("href", "signup");
        Assert.True(cta.IsComplete(block));
        Assert.False(cta.Setting("title")!.HasValue(StilBlockData.Create("cta").Set("title", "  ")));
    }
}
