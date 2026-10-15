namespace Stil.Blazor.Tests.TestComponents;

/// <summary>Tree test data. Equality is by reference, so ItemKey tests can use copies.</summary>
public sealed class TreeNode(string id, string name, params TreeNode[] children)
{
    public string Id { get; } = id;

    public string Name { get; } = name;

    public List<TreeNode> Children { get; } = [.. children];

    public override string ToString() => $"Node {Name}";

    /// <summary>
    /// docs (guide (intro, setup), api), src (app), readme.
    /// </summary>
    public static List<TreeNode> Sample() =>
    [
        new("docs", "Docs",
            new TreeNode("guide", "Guide", new TreeNode("intro", "Intro"), new TreeNode("setup", "Setup")),
            new TreeNode("api", "Api")),
        new("src", "Src", new TreeNode("app", "App")),
        new("readme", "Readme"),
    ];
}
