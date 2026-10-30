using System.Globalization;
using System.Text.RegularExpressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public partial class StilLocalizationTests
{
    private static readonly string[] Languages = ["en", "sv", "nb", "da", "fi"];

    /// <summary>Runs <paramref name="test"/> with <paramref name="culture"/> as the UI culture, then restores it.</summary>
    internal static void InCulture(string culture, Action test)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        try
        {
            test();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void EveryBuiltInLanguageHasEveryKeyWithTheSamePlaceholders()
    {
        Assert.Equal(Languages.Order(), StilLocalizer.BuiltInCultures.Order());
        var english = StilLocalizer.BuiltInFor("en");

        foreach (var language in Languages)
        {
            var texts = StilLocalizer.BuiltInFor(language);
            Assert.Equal(english.Keys.Order(), texts.Keys.Order());
            foreach (var (key, text) in english)
            {
                Assert.True(Placeholders(text).SetEquals(Placeholders(texts[key])), $"{language} {key}: '{texts[key]}' has other placeholders than '{text}'.");
            }
        }
    }

    [GeneratedRegex(@"\{\d+(:[^}]*)?\}")]
    private static partial Regex Placeholder();

    private static HashSet<string> Placeholders(string text) =>
        Placeholder().Matches(text).Select(m => m.Value.Split(':')[0].TrimEnd('}')).ToHashSet();

    [Theory]
    [InlineData("en-US", "Next page")]
    [InlineData("sv-SE", "Nästa sida")]
    [InlineData("nb-NO", "Neste side")]
    [InlineData("da-DK", "Næste side")]
    [InlineData("fi-FI", "Seuraava sivu")]
    [InlineData("sv-FI", "Nästa sida")]
    [InlineData("nn-NO", "Neste side")]
    [InlineData("de-DE", "Next page")]
    public void BuiltInTextsFollowTheUiCulture(string culture, string expected) => InCulture(culture, () =>
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 1)
            .Add(component => component.PageCount, 3));

        Assert.Contains(expected, cut.Markup);
    });

    [Fact]
    public void AParameterWinsOverTheTranslation() => InCulture("sv-SE", () =>
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 1)
            .Add(component => component.PageCount, 3)
            .Add(component => component.NextText, "Vidare"));

        Assert.Contains("Vidare", cut.Markup);
        Assert.DoesNotContain("Nästa sida", cut.Markup);
    });

    [Fact]
    public void OptionsOverrideTextsAndAddLanguages()
    {
        using var context = new BunitContext();
        context.Services.AddStil(options => options.Texts
            .Add("sv", "Pagination.Next", "Framåt")
            .Add("de", new Dictionary<string, string> { ["Pagination.Next"] = "Nächste Seite" }));
        var localizer = context.Services.GetRequiredService<IStilLocalizer>();

        Assert.Equal("Framåt", localizer.GetText("Pagination.Next", CultureInfo.GetCultureInfo("sv-SE")));
        Assert.Equal("Föregående sida", localizer.GetText("Pagination.Previous", CultureInfo.GetCultureInfo("sv-SE")));
        Assert.Equal("Nächste Seite", localizer.GetText("Pagination.Next", CultureInfo.GetCultureInfo("de-AT")));
        Assert.Equal("Previous page", localizer.GetText("Pagination.Previous", CultureInfo.GetCultureInfo("de-AT")));

        InCulture("de-DE", () =>
        {
            var cut = context.Render<StilPagination>(parameters => parameters
                .Add(component => component.Page, 1)
                .Add(component => component.PageCount, 3));
            Assert.Contains("Nächste Seite", cut.Markup);
        });
    }

    private sealed class PartialLocalizer : IStilLocalizer
    {
        public string? GetText(string key, CultureInfo culture) => key == "Common.Close" ? "Shut" : null;
    }

    [Fact]
    public void ACustomLocalizerFallsBackToTheBuiltInTexts()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<IStilLocalizer, PartialLocalizer>();

        Assert.Equal("Shut", context.Render<StilCloseButton>().Find("button").GetAttribute("aria-label"));
        Assert.Equal("Loading…", context.Render<StilSpinner>().Find(".stil-visually-hidden").TextContent);
    }

    [Fact]
    public void TheCalendarCultureAlsoPicksTheLanguage()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilAgenda<Meeting>>(parameters =>
            Meeting.Configure(parameters, [], CultureInfo.GetCultureInfo("sv-SE")));

        Assert.Contains("Inget planerat.", cut.Markup);
        Assert.Contains("I dag", cut.Markup);
    }

    [Fact]
    public void AlertAndToastTextsAreLocalized() => InCulture("fi-FI", () =>
    {
        using var context = new BunitContext();
        context.Services.AddStil();
        context.Renderer.SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        Assert.Equal("Sulje", context.Render<StilAlert>(parameters => parameters
            .Add(component => component.Dismissible, true)).Find("button.stil-close").GetAttribute("aria-label"));
        Assert.Equal("Ilmoitukset", context.Render<StilToastHost>().Find("[role='region']").GetAttribute("aria-label"));
    });

    private static StilLocalizationOptions TestFiles(Action<StilLocalizationOptions>? configure = null)
    {
        var options = new StilLocalizationOptions();
        options.Assemblies.Add(typeof(StilLocalizationTests).Assembly);
        configure?.Invoke(options);
        return options;
    }

    private static IServiceProvider JsonServices(Action<StilLocalizationOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddStilLocalization(options =>
        {
            options.Assemblies.Add(typeof(StilLocalizationTests).Assembly);
            configure?.Invoke(options);
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public void JsonLocalizerReadsSharedAndTypedFiles() => InCulture("sv-SE", () =>
    {
        var services = JsonServices();
        var shared = services.GetRequiredService<IStringLocalizer<StilLocalizationTests>>();
        var meeting = services.GetRequiredService<IStringLocalizer<Meeting>>();

        Assert.Equal("Spara", shared["Save"]);
        Assert.Equal("Beställningar", shared["Orders.Title"]);
        Assert.Equal("Hej Ada", shared["Greeting", "Ada"]);
        Assert.Equal("Spara mötet", meeting["Save"]);
        Assert.Equal("Hej Ada", meeting["Greeting", "Ada"]);
    });

    [Fact]
    public void JsonLocalizerFallsBackToParentAndDefaultCulture() => InCulture("sv-FI", () =>
    {
        var localizer = JsonServices().GetRequiredService<IStringLocalizer<StilLocalizationTests>>();

        Assert.Equal("Spara", localizer["Save"]);
        Assert.Equal("Only in English", localizer["OnlyInEnglish"]);
        Assert.False(localizer["OnlyInEnglish"].ResourceNotFound);
    });

    [Fact]
    public void AMissingKeyReturnsTheKey() => InCulture("sv-SE", () =>
    {
        var text = JsonServices().GetRequiredService<IStringLocalizer<StilLocalizationTests>>()["Nowhere"];

        Assert.Equal("Nowhere", text.Value);
        Assert.True(text.ResourceNotFound);
    });

    [Fact]
    public void GetAllStringsListsEachKeyOnceMostSpecificFirst() => InCulture("sv-SE", () =>
    {
        var localizer = JsonServices().GetRequiredService<IStringLocalizer<Meeting>>();

        var all = localizer.GetAllStrings(includeParentCultures: true).ToDictionary(s => s.Name, s => s.Value);
        Assert.Equal("Spara mötet", all["Save"]);
        Assert.Equal("Only in English", all["OnlyInEnglish"]);
        Assert.DoesNotContain("OnlyInEnglish", localizer.GetAllStrings(includeParentCultures: false).Select(s => s.Name));
    });

    [Fact]
    public void AppJsonFilesOverrideStilTexts() => InCulture("sv-SE", () =>
    {
        using var context = new BunitContext();
        context.Services.AddStilLocalization(options => options.Assemblies.Add(typeof(StilLocalizationTests).Assembly));

        var cut = context.Render<StilPagination>(parameters => parameters
            .Add(component => component.Page, 1)
            .Add(component => component.PageCount, 3));

        Assert.Contains("Framåt", cut.Markup);
        Assert.Contains("Föregående sida", cut.Markup);
    });

    [Fact]
    public void AnInvalidFileNamesTheFile()
    {
        var error = Assert.Throws<InvalidOperationException>(() => new StilJsonTextStore(TestFiles(o => o.ResourcesPath = "BrokenLocales")));

        Assert.Contains("BrokenLocales/sv.json", error.Message);
    }

    [Theory]
    [InlineData("sv", "Svenska")]
    [InlineData("fi", "Suomi")]
    [InlineData("nb", "Norsk bokmål")]
    [InlineData("sv-FI", "Svenska (Finland)")]
    public void DisplayNamesAreInTheLanguageItself(string culture, string expected)
    {
        var name = StilCulture.DisplayName(culture);

        Assert.StartsWith(expected.Split(' ')[0], name);
    }

    [Theory]
    [InlineData("ar-SA", "rtl")]
    [InlineData("he", "rtl")]
    [InlineData("sv-SE", "ltr")]
    [InlineData("en", "ltr")]
    public void DirectionFollowsTheCulture(string culture, string expected) =>
        Assert.Equal(expected, StilCulture.Direction(CultureInfo.GetCultureInfo(culture)));

    [Fact]
    public void LanguageSelectorListsLanguagesAndMarksTheCurrentOne() => InCulture("sv-SE", () =>
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("orders?page=2");

        var cut = context.Render<StilLanguageSelector>(parameters => parameters
            .Add(component => component.Cultures, ["en", "sv", "fi"]));

        var links = cut.FindAll("a.stil-dropdown-item");
        Assert.Equal(["English", "Svenska", "Suomi"], links.Select(link => link.TextContent.Trim()));
        Assert.Equal(["en", "sv", "fi"], links.Select(link => link.GetAttribute("data-stil-culture")));
        Assert.Equal(["en", "sv", "fi"], links.Select(link => link.GetAttribute("lang")));
        Assert.Equal(["en", "sv", "fi"], links.Select(link => link.GetAttribute("hreflang")));
        Assert.Equal("true", links[1].GetAttribute("aria-current"));
        Assert.Null(links[0].GetAttribute("aria-current"));
        Assert.Contains("orders?page=2&culture=fi&ui-culture=fi", links[2].GetAttribute("href"));

        var summary = cut.Find("summary");
        Assert.Contains("Språk:", summary.TextContent);
        Assert.Contains("Svenska", summary.TextContent);
        Assert.NotNull(summary.QuerySelector("svg.stil-icon"));
    });

    [Fact]
    public void IconOnlyLanguageSelectorNamesTheLanguageInItsLabel() => InCulture("fi-FI", () =>
    {
        using var context = new BunitContext();

        var cut = context.Render<StilLanguageSelector>(parameters => parameters
            .Add(component => component.Cultures, ["sv", "fi"])
            .Add(component => component.IconOnly, true));

        Assert.Equal("Kieli: Suomi", cut.Find("summary").GetAttribute("aria-label"));
        Assert.DoesNotContain("Suomi", cut.Find("summary").TextContent);
    });

    [Fact]
    public void LanguageSelectorShowsTheLabelWhenNoLanguageMatches() => InCulture("de-DE", () =>
    {
        using var context = new BunitContext();

        var cut = context.Render<StilLanguageSelector>(parameters => parameters
            .Add(component => component.Cultures, ["sv", "fi"]));

        Assert.Empty(cut.FindAll("[aria-current]"));
        Assert.Contains("Language", cut.Find("summary").TextContent);
    });

    [Fact]
    public void DirectionalIconsAreMarked()
    {
        using var context = new BunitContext();

        Assert.Contains("stil-icon-directional", context.Render<StilIcon>(p => p.Add(c => c.Name, StilIcons.ChevronRight)).Find("svg").ClassName);
        Assert.DoesNotContain("stil-icon-directional", context.Render<StilIcon>(p => p.Add(c => c.Name, StilIcons.ChevronDown)).Find("svg").ClassName);
    }
}

/// <summary>Tests that change the process-wide default culture, so they must not run in parallel with others.</summary>
[CollectionDefinition(nameof(DefaultCultureCollection), DisableParallelization = true)]
public sealed class DefaultCultureCollection;

[Collection(nameof(DefaultCultureCollection))]
public class StilCultureStorageTests
{
    [Theory]
    [InlineData("fi", null, "fi")]
    [InlineData(null, "sv", "sv")]
    [InlineData("not-a-culture-name-at-all", null, null)]
    public async Task ApplyStoredAsyncUsesTheStoredCultureOrTheDefault(string? stored, string? fallback, string? expected)
    {
        var previous = (CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            using var context = new BunitContext();
            context.JSInterop.Setup<string?>("localStorage.getItem", StilCulture.StorageKey).SetResult(stored);

            var applied = await StilCulture.ApplyStoredAsync(context.Services, fallback);

            Assert.Equal(expected, applied?.Name);
            if (expected is not null)
            {
                Assert.Equal(expected, CultureInfo.CurrentUICulture.Name);
                Assert.Equal(expected, CultureInfo.DefaultThreadCurrentUICulture?.Name);
            }
        }
        finally
        {
            (CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture, CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = previous;
        }
    }
}
