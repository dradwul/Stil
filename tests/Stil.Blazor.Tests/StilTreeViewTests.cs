using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilTreeViewTests
{
    private static IRenderedComponent<StilTreeView<TreeNode>> RenderTree(
        BunitContext context,
        Action<ComponentParameterCollectionBuilder<StilTreeView<TreeNode>>>? configure = null,
        List<TreeNode>? roots = null) =>
        context.Render<StilTreeView<TreeNode>>(parameters =>
        {
            parameters
                .Add(component => component.Items, roots ?? TreeNode.Sample())
                .Add(component => component.Children, node => node.Children)
                .Add(component => component.ItemText, node => node.Name);
            configure?.Invoke(parameters);
        });

    private static IReadOnlyList<IElement> Items<T>(IRenderedComponent<T> cut) where T : IComponent =>
        cut.FindAll("[role=treeitem]");

    private static IElement Row(IElement item) => item.Children.Single(child => child.ClassList.Contains("stil-tree-row"));

    private static IElement? Group(IElement item) => item.Children.SingleOrDefault(child => child.TagName == "UL");

    private static string Name(IElement item) => Row(item).QuerySelector(".stil-tree-label")!.TextContent.Trim();

    private static IElement Item<T>(IRenderedComponent<T> cut, string name) where T : IComponent =>
        Items(cut).Single(item => Name(item) == name);

    private static string[] VisibleNames<T>(IRenderedComponent<T> cut) where T : IComponent =>
        cut.FindAll(".stil-tree-label").Select(label => label.TextContent.Trim()).ToArray();

    private static string TabStop<T>(IRenderedComponent<T> cut) where T : IComponent =>
        Name(Assert.Single(cut.FindAll("[role=treeitem][tabindex='0']")));

    [Fact]
    public void RendersTreeWithLabelAndMergedClass()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.Label, "Files")
            .AddUnmatched("class", "custom")
            .AddUnmatched("id", "files"));

        var tree = cut.Find("ul");
        Assert.Equal("tree", tree.GetAttribute("role"));
        Assert.Equal("Files", tree.GetAttribute("aria-label"));
        Assert.Equal("files", tree.Id);
        Assert.Equal(["stil-tree", "custom"], tree.ClassList);
    }

    [Fact]
    public void CollapsedByDefaultWithAriaAttributes()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context);

        Assert.Equal(["Docs", "Src", "Readme"], VisibleNames(cut));
        Assert.Empty(cut.FindAll("[role=group]"));

        var docs = Item(cut, "Docs");
        Assert.Equal("false", docs.GetAttribute("aria-expanded"));
        Assert.Equal("false", docs.GetAttribute("aria-selected"));
        Assert.Equal("1", docs.GetAttribute("aria-level"));
        Assert.Contains("stil-tree-item", docs.ClassList);

        // A leaf has no aria-expanded and an empty toggle, so the labels line up.
        var readme = Item(cut, "Readme");
        Assert.False(readme.HasAttribute("aria-expanded"));
        Assert.Empty(readme.QuerySelector(".stil-tree-toggle")!.Children);
        Assert.NotNull(docs.QuerySelector(".stil-tree-toggle svg.stil-icon"));
    }

    [Fact]
    public void ExpandAllRendersNestedGroupsAndLevels()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context, parameters => parameters.Add(component => component.ExpandAll, true));

        Assert.Equal(["Docs", "Guide", "Intro", "Setup", "Api", "Src", "App", "Readme"], VisibleNames(cut));
        Assert.Equal("true", Item(cut, "Docs").GetAttribute("aria-expanded"));
        Assert.Equal("true", Item(cut, "Guide").GetAttribute("aria-expanded"));
        Assert.Equal("2", Item(cut, "Guide").GetAttribute("aria-level"));
        Assert.Equal("3", Item(cut, "Intro").GetAttribute("aria-level"));

        var group = Group(Item(cut, "Docs"))!;
        Assert.Equal("group", group.GetAttribute("role"));
        Assert.Contains("stil-tree-group", group.ClassList);
        Assert.Equal(2, group.Children.Count(child => child.GetAttribute("role") == "treeitem"));
        Assert.Null(Group(Item(cut, "Readme")));
    }

    [Fact]
    public void ExpandedItemsParameterExpandsMatchingItemsByKey()
    {
        using var context = new BunitContext();
        var roots = TreeNode.Sample();

        // A copy with the same id expands the item when ItemKey is set.
        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.ItemKey, node => node.Id)
            .Add(component => component.ExpandedItems, [new TreeNode("src", "Src copy")]), roots);

        Assert.Equal(["Docs", "Src", "App", "Readme"], VisibleNames(cut));
    }

    [Fact]
    public void WithoutItemKeyItemsAreComparedByEquals()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.ExpandedItems, [new TreeNode("src", "Src copy")]));

        Assert.Equal(["Docs", "Src", "Readme"], VisibleNames(cut));
    }

    [Fact]
    public void SelectedItemSetsAriaSelectedAndTabStop()
    {
        using var context = new BunitContext();
        var roots = TreeNode.Sample();

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.ItemKey, node => node.Id)
            .Add(component => component.SelectedItem, new TreeNode("readme", "copy")), roots);

        Assert.Equal("true", Item(cut, "Readme").GetAttribute("aria-selected"));
        Assert.Equal("false", Item(cut, "Docs").GetAttribute("aria-selected"));
        Assert.Equal("Readme", TabStop(cut));
    }

    [Fact]
    public void FirstItemIsTheTabStopWithoutSelection()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context);

        Assert.Equal("Docs", TabStop(cut));
        Assert.All(Items(cut).Skip(1), item => Assert.Equal("-1", item.GetAttribute("tabindex")));
    }

    [Fact]
    public void HiddenSelectedItemFallsBackToFirstTabStop()
    {
        using var context = new BunitContext();
        var roots = TreeNode.Sample();

        var cut = RenderTree(context, parameters => parameters.Add(component => component.SelectedItem, roots[1].Children[0]), roots);

        Assert.Equal("Docs", TabStop(cut));
    }

    [Fact]
    public void ItemTemplateIconAndToStringFallback()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTreeView<TreeNode>>(parameters => parameters
            .Add(component => component.Items, TreeNode.Sample())
            .Add(component => component.ItemIcon, node => node.Children.Count > 0 ? StilIcons.Menu : null));

        Assert.Equal(["Node Docs", "Node Src", "Node Readme"], VisibleNames(cut));
        Assert.Single(Row(Item(cut, "Node Docs")).Children, child => child.ClassList.Contains("stil-icon"));
        Assert.DoesNotContain(Row(Item(cut, "Node Readme")).Children, child => child.ClassList.Contains("stil-icon"));

        // Without Children every item is a leaf.
        Assert.Empty(cut.FindAll("[aria-expanded]"));

        var templated = RenderTree(context, parameters => parameters
            .Add(component => component.ItemTemplate, node => builder => builder.AddMarkupContent(0, $"<b>{node.Id}</b>")));

        Assert.Equal("docs", templated.Find(".stil-tree-label > b").TextContent);
    }

    [Fact]
    public void ClickingTheRowSelects()
    {
        using var context = new BunitContext();
        TreeNode? selected = null;

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.SelectedItemChanged, (TreeNode? node) => selected = node));

        Item(cut, "Src").Children[0].Click();

        Assert.Equal("src", selected?.Id);
        Assert.Equal("true", Item(cut, "Src").GetAttribute("aria-selected"));
        Assert.Equal("Src", TabStop(cut));
        Assert.Empty(cut.FindAll("[role=group]"));
    }

    [Fact]
    public void ClickingTheToggleExpandsAndCollapsesWithoutSelecting()
    {
        using var context = new BunitContext();
        IReadOnlyCollection<TreeNode>? expanded = null;
        var selectedChanged = false;

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.ExpandedItemsChanged, (IReadOnlyCollection<TreeNode> items) => expanded = items)
            .Add(component => component.SelectedItemChanged, (TreeNode? _) => selectedChanged = true));

        Item(cut, "Docs").QuerySelector(".stil-tree-toggle")!.Click();

        Assert.Equal(["docs"], expanded!.Select(node => node.Id));
        Assert.Equal("true", Item(cut, "Docs").GetAttribute("aria-expanded"));
        Assert.Equal(["Docs", "Guide", "Api", "Src", "Readme"], VisibleNames(cut));
        Assert.False(selectedChanged);

        Item(cut, "Docs").QuerySelector(".stil-tree-toggle")!.Click();

        Assert.Empty(expanded!);
        Assert.Equal(["Docs", "Src", "Readme"], VisibleNames(cut));
    }

    [Fact]
    public void TwoWayBindingsUpdateTheHost()
    {
        using var context = new BunitContext();

        var cut = context.Render<TreeViewHost>();

        Item(cut, "Docs").QuerySelector(".stil-tree-toggle")!.Click();
        Item(cut, "Guide").QuerySelector(".stil-tree-toggle")!.Click();
        Item(cut, "Intro").Children[0].Click();

        Assert.Equal(["docs", "guide"], cut.Instance.ExpandedItems!.Select(node => node.Id));
        Assert.Equal("intro", cut.Instance.SelectedItem?.Id);
        Assert.Equal("true", Item(cut, "Intro").GetAttribute("aria-selected"));
        Assert.Equal("Files", cut.Find("[role=tree]").GetAttribute("aria-label"));
    }

    [Fact]
    public void HostCanChangeTheBindings()
    {
        using var context = new BunitContext();
        var roots = TreeNode.Sample();

        var cut = context.Render<TreeViewHost>(parameters => parameters.Add(component => component.Roots, roots));

        cut.Render(parameters => parameters
            .Add(component => component.ExpandedItems, [roots[1]])
            .Add(component => component.SelectedItem, roots[1].Children[0]));

        Assert.Equal(["Docs", "Src", "App", "Readme"], VisibleNames(cut));
        Assert.Equal("true", Item(cut, "App").GetAttribute("aria-selected"));
        Assert.Equal("App", TabStop(cut));
    }

    [Theory]
    [InlineData("Docs", "ArrowDown", "Guide")]
    [InlineData("Guide", "ArrowDown", "Intro")]
    [InlineData("Setup", "ArrowDown", "Api")]
    [InlineData("Readme", "ArrowDown", "Readme")]
    [InlineData("Api", "ArrowUp", "Setup")]
    [InlineData("Docs", "ArrowUp", "Docs")]
    [InlineData("Api", "Home", "Docs")]
    [InlineData("Docs", "End", "Readme")]
    [InlineData("Docs", "ArrowRight", "Guide")]
    [InlineData("Intro", "ArrowLeft", "Guide")]
    [InlineData("Guide", "ArrowLeft", "Guide")]
    [InlineData("Docs", "ArrowLeft", "Docs")]
    [InlineData("Readme", "ArrowRight", "Readme")]
    [InlineData("Readme", "ArrowLeft", "Readme")]
    public void KeysMoveTheTabStop(string start, string key, string expected)
    {
        using var context = new BunitContext();

        var cut = RenderTree(context, parameters => parameters.Add(component => component.ExpandAll, true));
        Item(cut, start).Focus();

        Item(cut, start).KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal(expected, TabStop(cut));
    }

    [Fact]
    public void ArrowKeysMoveFocus()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context);

        Item(cut, "Docs").KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        context.JSInterop.VerifyFocusAsyncInvoke();
        Assert.Equal("Src", TabStop(cut));
    }

    [Fact]
    public void ArrowRightExpandsThenMovesToTheFirstChild()
    {
        using var context = new BunitContext();
        IReadOnlyCollection<TreeNode>? expanded = null;

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.ExpandedItemsChanged, (IReadOnlyCollection<TreeNode> items) => expanded = items));

        Item(cut, "Docs").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal(["docs"], expanded!.Select(node => node.Id));
        Assert.Equal("true", Item(cut, "Docs").GetAttribute("aria-expanded"));
        Assert.Equal("Docs", TabStop(cut));
        context.JSInterop.VerifyNotInvoke("Blazor._internal.domWrapper.focus");

        Item(cut, "Docs").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal("Guide", TabStop(cut));
        context.JSInterop.VerifyFocusAsyncInvoke();
    }

    [Fact]
    public void ArrowLeftCollapsesThenMovesToTheParent()
    {
        using var context = new BunitContext();
        IReadOnlyCollection<TreeNode>? expanded = null;

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.ExpandAll, true)
            .Add(component => component.ExpandedItemsChanged, (IReadOnlyCollection<TreeNode> items) => expanded = items));
        Item(cut, "Guide").Focus();

        Item(cut, "Guide").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        Assert.Equal(["docs", "src"], expanded!.Select(node => node.Id).Order());
        Assert.Equal("false", Item(cut, "Guide").GetAttribute("aria-expanded"));
        Assert.Equal("Guide", TabStop(cut));

        Item(cut, "Guide").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        Assert.Equal("Docs", TabStop(cut));
    }

    [Fact]
    public void ArrowRightCollapsesInRightToLeftCultures() => StilLocalizationTests.InCulture("he-IL", () =>
    {
        using var context = new BunitContext();

        var cut = RenderTree(context, parameters => parameters.Add(component => component.ExpandAll, true));
        Item(cut, "Guide").Focus();

        Item(cut, "Guide").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        Assert.Equal("false", Item(cut, "Guide").GetAttribute("aria-expanded"));
    });

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public void EnterAndSpaceSelect(string key)
    {
        using var context = new BunitContext();
        TreeNode? selected = null;

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.SelectedItemChanged, (TreeNode? node) => selected = node));

        Item(cut, "Src").KeyDown(new KeyboardEventArgs { Key = key });

        Assert.Equal("src", selected?.Id);
        Assert.Equal("true", Item(cut, "Src").GetAttribute("aria-selected"));
        Assert.Equal("Src", TabStop(cut));
    }

    [Fact]
    public void OtherKeysDoNothing()
    {
        using var context = new BunitContext();
        var changed = false;

        var cut = RenderTree(context, parameters => parameters
            .Add(component => component.SelectedItemChanged, (TreeNode? _) => changed = true)
            .Add(component => component.ExpandedItemsChanged, (IReadOnlyCollection<TreeNode> _) => changed = true));

        Item(cut, "Docs").KeyDown(new KeyboardEventArgs { Key = "a" });
        Item(cut, "Docs").KeyDown(new KeyboardEventArgs { Key = "Tab" });

        Assert.False(changed);
        Assert.Equal("Docs", TabStop(cut));
        context.JSInterop.VerifyNotInvoke("Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void CollapsingAnAncestorOfTheFocusedItemMovesTheTabStopToIt()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context, parameters => parameters.Add(component => component.ExpandAll, true));
        Item(cut, "Intro").Focus();
        Assert.Equal("Intro", TabStop(cut));

        Item(cut, "Docs").QuerySelector(".stil-tree-toggle")!.Click();

        Assert.Equal("Docs", TabStop(cut));
    }

    [Fact]
    public void FocusingAnItemMakesItTheTabStop()
    {
        using var context = new BunitContext();

        var cut = RenderTree(context);

        Item(cut, "Readme").Focus();

        Assert.Equal("Readme", TabStop(cut));
    }
}
