using Bunit;
using Stil.Blazor.Components;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

public class StilDateRangePickerTests
{
    [Fact]
    public void RendersFieldsetWithLegendAndTwoLabelledDateInputs()
    {
        using var context = new BunitContext();
        DateOnly? start = null;
        DateOnly? end = null;

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Start, start, v => start = v, () => start)
            .Bind(component => component.End, end, v => end = v, () => end)
            .Add(component => component.Id, "stay")
            .Add(component => component.Label, "Stay"));

        var fieldset = cut.Find("fieldset#stay");
        Assert.Equal(["stil-field", "stil-date-range"], fieldset.ClassList);
        Assert.Equal("Stay", fieldset.QuerySelector("legend.stil-label")!.TextContent);
        Assert.False(fieldset.HasAttribute("aria-describedby"));

        var inputs = cut.FindAll(".stil-date-range-inputs input[type=date]");
        Assert.Equal(["stay-start", "stay-end"], inputs.Select(i => i.Id));
        Assert.All(inputs, input => Assert.Contains("stil-input", input.ClassList));
        Assert.Equal("From", cut.Find("label[for=stay-start]").TextContent);
        Assert.Equal("To", cut.Find("label[for=stay-end]").TextContent);
        // Outside a form the inputs get no Blazor validation classes.
        Assert.All(inputs, input => Assert.DoesNotContain("valid", input.ClassList));
        Assert.Empty(cut.FindAll(".stil-field-error"));
    }

    [Fact]
    public void GeneratesIdsWhenNotSet()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDateRangePicker<DateOnly?>>();

        var id = cut.Find("fieldset").Id;
        Assert.StartsWith("stil-range-", id);
        Assert.Equal([$"{id}-start", $"{id}-end"], cut.FindAll("input").Select(i => i.Id));
    }

    [Fact]
    public void CustomLabelsHintAndAttributes()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Add(component => component.Id, "period")
            .Add(component => component.StartLabel, "Start")
            .Add(component => component.EndLabel, "End")
            .Add(component => component.Hint, "Up to 30 days.")
            .Add(component => component.Size, StilSize.Small)
            .AddUnmatched("class", "stil-col-6")
            .AddUnmatched("data-test", "range"));

        var fieldset = cut.Find("fieldset");
        Assert.Equal(["stil-field", "stil-date-range", "stil-col-6"], fieldset.ClassList);
        Assert.Equal("range", fieldset.GetAttribute("data-test"));
        Assert.Equal("period-hint", fieldset.GetAttribute("aria-describedby"));
        Assert.Equal("Up to 30 days.", cut.Find("#period-hint.stil-field-hint").TextContent);
        Assert.Equal("Start", cut.Find("label[for=period-start]").TextContent);
        Assert.Equal("End", cut.Find("label[for=period-end]").TextContent);
        Assert.All(cut.FindAll("input"), input => Assert.Contains("stil-input-sm", input.ClassList));
    }

    [Fact]
    public void MinAndMaxApplyToBothInputs()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Add(component => component.Id, "r")
            .Add(component => component.Min, "2025-01-01")
            .Add(component => component.Max, "2025-12-31"));

        Assert.Equal("2025-01-01", cut.Find("#r-start").GetAttribute("min"));
        Assert.Equal("2025-12-31", cut.Find("#r-start").GetAttribute("max"));
        Assert.Equal("2025-01-01", cut.Find("#r-end").GetAttribute("min"));
        Assert.Equal("2025-12-31", cut.Find("#r-end").GetAttribute("max"));
    }

    [Fact]
    public void EndMinFollowsStartAndStartMaxFollowsEnd()
    {
        using var context = new BunitContext();
        DateOnly? start = new(2025, 3, 10);
        DateOnly? end = new(2025, 3, 20);

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Start, start, v => start = v, () => start)
            .Bind(component => component.End, end, v => end = v, () => end)
            .Add(component => component.Id, "r")
            .Add(component => component.Min, "2025-01-01")
            .Add(component => component.Max, "2025-12-31"));

        Assert.Equal("2025-01-01", cut.Find("#r-start").GetAttribute("min"));
        Assert.Equal("2025-03-20", cut.Find("#r-start").GetAttribute("max"));
        Assert.Equal("2025-03-10", cut.Find("#r-end").GetAttribute("min"));
        Assert.Equal("2025-12-31", cut.Find("#r-end").GetAttribute("max"));
    }

    [Fact]
    public void BindingUpdatesStartAndEnd()
    {
        using var context = new BunitContext();
        DateOnly? start = null;
        DateOnly? end = null;

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Start, start, v => start = v, () => start)
            .Bind(component => component.End, end, v => end = v, () => end)
            .Add(component => component.Id, "r"));

        cut.Find("#r-start").Change("2025-04-01");
        cut.Find("#r-end").Change("2025-04-05");

        Assert.Equal(new DateOnly(2025, 4, 1), start);
        Assert.Equal(new DateOnly(2025, 4, 5), end);
        Assert.Equal("2025-04-01", cut.Find("#r-end").GetAttribute("min"));
    }

    [Fact]
    public void WorksWithDateTime()
    {
        using var context = new BunitContext();
        var start = new DateTime(2025, 6, 1);
        var end = new DateTime(2025, 6, 7);

        var cut = context.Render<StilDateRangePicker<DateTime>>(parameters => parameters
            .Bind(component => component.Start, start, v => start = v, () => start)
            .Bind(component => component.End, end, v => end = v, () => end)
            .Add(component => component.Id, "r"));

        Assert.Equal("2025-06-01", cut.Find("#r-start").GetAttribute("value"));
        Assert.Equal("2025-06-01", cut.Find("#r-end").GetAttribute("min"));

        cut.Find("#r-end").Change("2025-06-10");

        Assert.Equal(new DateTime(2025, 6, 10), end);
    }

    [Fact]
    public void EndBeforeStartShowsLinkedErrorOnEndField()
    {
        using var context = new BunitContext();
        DateOnly? start = new(2025, 5, 10);
        DateOnly? end = null;

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Start, start, v => start = v, () => start)
            .Bind(component => component.End, end, v => end = v, () => end)
            .Add(component => component.Id, "r"));

        cut.Find("#r-end").Change("2025-05-01");

        var error = cut.Find("#r-end-error");
        Assert.Equal(["stil-field-error"], error.ClassList);
        Assert.Equal("The end date must be on or after the start date.", error.TextContent.Trim());
        var endInput = cut.Find("#r-end");
        Assert.Equal("true", endInput.GetAttribute("aria-invalid"));
        Assert.Equal("r-end-error", endInput.GetAttribute("aria-describedby"));
        Assert.False(cut.Find("#r-start").HasAttribute("aria-invalid"));
        Assert.True(cut.Instance.HasRangeError);

        cut.Find("#r-end").Change("2025-05-12");

        Assert.Empty(cut.FindAll(".stil-field-error"));
        Assert.False(cut.Find("#r-end").HasAttribute("aria-invalid"));
        Assert.False(cut.Instance.HasRangeError);
    }

    [Fact]
    public void ChangingStartAfterEndAlsoShowsError()
    {
        using var context = new BunitContext();
        DateOnly? start = new(2025, 5, 1);
        DateOnly? end = new(2025, 5, 5);

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Bind(component => component.Start, start, v => start = v, () => start)
            .Bind(component => component.End, end, v => end = v, () => end)
            .Add(component => component.Id, "r")
            .Add(component => component.RangeErrorText, "Slutet måste vara efter början."));

        cut.Find("#r-start").Change("2025-05-09");

        Assert.Equal("Slutet måste vara efter början.", cut.Find("#r-end-error").TextContent.Trim());
    }

    [Fact]
    public void InitialInvalidRangeShowsError()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDateRangePicker<DateOnly>>(parameters => parameters
            .Add(component => component.Id, "r")
            .Add(component => component.Start, new DateOnly(2025, 2, 2))
            .Add(component => component.End, new DateOnly(2025, 2, 1)));

        Assert.Single(cut.FindAll("#r-end-error"));
    }

    [Fact]
    public void DisabledDisablesBothInputs()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilDateRangePicker<DateOnly?>>(parameters => parameters
            .Add(component => component.Disabled, true));

        Assert.All(cut.FindAll("input"), input => Assert.True(input.HasAttribute("disabled")));
    }

    [Fact]
    public void InEditFormShowsRequiredMessagesFromExpressions()
    {
        using var context = new BunitContext();
        var cut = context.Render<DateRangeForm>();

        cut.Find("form").Submit();

        Assert.Equal("Check-in is required.", cut.Find("#stay-start-error").TextContent.Trim());
        Assert.Equal("Check-out is required.", cut.Find("#stay-end-error").TextContent.Trim());
        Assert.Equal("true", cut.Find("#stay-start").GetAttribute("aria-invalid"));
        Assert.Equal("true", cut.Find("#stay-end").GetAttribute("aria-invalid"));
        Assert.Equal(0, cut.Instance.ValidSubmits);
    }

    [Fact]
    public void InEditFormRangeErrorBlocksSubmitUntilFixed()
    {
        using var context = new BunitContext();
        var model = new DateRangeForm.DateRangeModel();
        var cut = context.Render<DateRangeForm>(parameters => parameters.Add(component => component.Model, model));

        cut.Find("#stay-start").Change("2025-07-10");
        cut.Find("#stay-end").Change("2025-07-03");

        Assert.Equal(new DateOnly(2025, 7, 10), model.CheckIn);
        Assert.Equal(new DateOnly(2025, 7, 3), model.CheckOut);
        Assert.Equal("The end date must be on or after the start date.", cut.Find("#stay-end-error").TextContent.Trim());
        Assert.Contains("invalid", cut.Find("#stay-end").ClassList);

        cut.Find("form").Submit();
        Assert.Equal(0, cut.Instance.ValidSubmits);
        Assert.Equal("The end date must be on or after the start date.", cut.Find("#stay-end-error").TextContent.Trim());

        cut.Find("#stay-end").Change("2025-07-12");
        Assert.Empty(cut.FindAll(".stil-field-error"));

        cut.Find("form").Submit();
        Assert.Equal(1, cut.Instance.ValidSubmits);
    }
}
