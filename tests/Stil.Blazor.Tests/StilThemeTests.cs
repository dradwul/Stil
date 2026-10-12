namespace Stil.Blazor.Tests;

public class StilThemeTests
{
    [Fact]
    public void EmptyThemeHasNoOverrides()
    {
        var theme = new StilTheme();

        Assert.False(theme.HasOverrides);
        Assert.Equal("", theme.ToCssDeclarations());
    }

    [Fact]
    public void WritesEveryKnownTokenInOrder()
    {
        var theme = new StilTheme
        {
            Primary = "#1",
            Secondary = "#2",
            Success = "#3",
            Danger = "#4",
            Warning = "#5",
            Info = "#6",
            Accent = "#7",
            FontFamily = "system-ui, sans-serif",
            Radius = "4px",
        };

        Assert.True(theme.HasOverrides);
        Assert.Equal(
            "--stil-color-primary: #1; --stil-color-secondary: #2; --stil-color-success: #3; " +
            "--stil-color-danger: #4; --stil-color-warning: #5; --stil-color-info: #6; " +
            "--stil-color-accent: #7; --stil-font-family: system-ui, sans-serif; --stil-radius-md: 4px;",
            theme.ToCssDeclarations());
    }

    [Fact]
    public void WritesCustomTokensAfterKnownTokens()
    {
        var theme = new StilTheme { Primary = "#6741d9" };
        theme.Tokens["--stil-space-4"] = "1.25rem";

        Assert.Equal("--stil-color-primary: #6741d9; --stil-space-4: 1.25rem;", theme.ToCssDeclarations());
    }

    [Fact]
    public void CustomTokenAloneCountsAsOverride()
    {
        var theme = new StilTheme();
        theme.Tokens["--stil-space-4"] = "1rem";

        Assert.True(theme.HasOverrides);
    }

    [Fact]
    public void SkipsEmptyValuesAndTrimsOthers()
    {
        var theme = new StilTheme { Primary = "  #6741d9  ", Secondary = "", Danger = "   " };
        theme.Tokens["--stil-space-2"] = " ";

        Assert.Equal("--stil-color-primary: #6741d9;", theme.ToCssDeclarations());
    }

    [Theory]
    [InlineData("color")]
    [InlineData("--color-primary")]
    [InlineData("-stil-color-primary")]
    [InlineData("--STIL-color")]
    public void RejectsTokenNamesWithoutStilPrefix(string name)
    {
        var theme = new StilTheme();
        theme.Tokens[name] = "red";

        var error = Assert.Throws<InvalidOperationException>(() => theme.ToCssDeclarations());
        Assert.Contains(name, error.Message);
        Assert.Throws<InvalidOperationException>(() => theme.HasOverrides);
    }

    [Theory]
    [InlineData("red; color: blue")]
    [InlineData("red }")]
    [InlineData("{ red")]
    [InlineData("</style><script>alert(1)</script>")]
    [InlineData("a > b")]
    public void RejectsCustomTokenValuesThatCouldEscape(string value)
    {
        var theme = new StilTheme();
        theme.Tokens["--stil-space-4"] = value;

        Assert.Throws<InvalidOperationException>(() => theme.ToCssDeclarations());
    }

    [Theory]
    [InlineData(";")]
    [InlineData("{")]
    [InlineData("}")]
    [InlineData("<")]
    [InlineData(">")]
    public void RejectsKnownTokenValuesThatCouldEscape(string character)
    {
        var theme = new StilTheme { Primary = $"#fff{character}" };

        Assert.Throws<InvalidOperationException>(() => theme.ToCssDeclarations());
    }

    [Theory]
    [InlineData("--stil-x: red; color")]
    [InlineData("--stil-x } body { color")]
    [InlineData("--stil-x</style>")]
    public void RejectsTokenNamesThatCouldEscape(string name)
    {
        var theme = new StilTheme();
        theme.Tokens[name] = "red";

        Assert.Throws<InvalidOperationException>(() => theme.ToCssDeclarations());
    }

    [Fact]
    public void TokenNamesAreCaseSensitive()
    {
        var theme = new StilTheme();
        theme.Tokens["--stil-space-4"] = "1rem";
        theme.Tokens["--stil-SPACE-4"] = "2rem";

        Assert.Equal(2, theme.Tokens.Count);
    }
}
