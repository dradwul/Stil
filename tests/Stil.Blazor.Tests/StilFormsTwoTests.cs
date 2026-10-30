using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Stil.Blazor.Components;

namespace Stil.Blazor.Tests;

public class StilFormsTwoTests
{
    private static readonly CultureInfo Swedish = CultureInfo.GetCultureInfo("sv-SE");

    // Popover

    [Fact]
    public void PopoverButtonTargetsItsPanel()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilPopover>(parameters => parameters
            .Add(component => component.Text, "Tips")
            .Add(component => component.Title, "Keyboard")
            .AddChildContent("<p>Press ?</p>"));

        var button = cut.Find("button");
        var panel = cut.Find(".stil-popover");
        Assert.Equal(panel.Id, button.GetAttribute("popovertarget"));
        Assert.Equal("auto", panel.GetAttribute("popover"));
        Assert.Equal("dialog", panel.GetAttribute("role"));
        Assert.Equal(cut.Find(".stil-popover-title").Id, panel.GetAttribute("aria-labelledby"));
        Assert.Contains("stil-btn-outline", button.ClassName);
    }

    [Fact]
    public void ManualPopoverHasACloseButtonAndReportsToggles()
    {
        using var context = new BunitContext();
        var states = new List<bool>();

        var cut = context.Render<StilPopover>(parameters => parameters
            .Add(component => component.Text, "Filter")
            .Add(component => component.Manual, true)
            .Add(component => component.OnToggle, (bool open) => states.Add(open)));

        var panel = cut.Find(".stil-popover");
        Assert.Equal("manual", panel.GetAttribute("popover"));
        Assert.Equal("hide", cut.Find(".stil-popover-close").GetAttribute("popovertargetaction"));

        panel.TriggerEvent("onstiltoggle", new StilToggleEventArgs { Open = true });
        Assert.Equal([true], states);
    }

    // Time and date-time

    [Theory]
    [InlineData("14:30", 14, 30)]
    [InlineData("1430", 14, 30)]
    [InlineData("930", 9, 30)]
    [InlineData("9", 9, 0)]
    [InlineData("08.15", 8, 15)]
    public void TypedTimesAreRead(string text, int hours, int minutes) =>
        Assert.Equal(new TimeOnly(hours, minutes), StilTimePicker<TimeOnly>.Parse(text, Swedish));

    [Theory]
    [InlineData("2460")]
    [InlineData("12345")]
    [InlineData("lunch")]
    public void OtherTextIsNotATime(string text) => Assert.Null(StilTimePicker<TimeOnly>.Parse(text, Swedish));

    [Fact]
    public void TimePickerSuggestsStepsInTheCultureFormat()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTimePicker<TimeOnly?>>(parameters => parameters
            .Add(component => component.Label, "Start")
            .Add(component => component.Step, 15)
            .Add(component => component.Min, new TimeOnly(8, 0))
            .Add(component => component.Max, new TimeOnly(9, 0))
            .Add(component => component.Culture, Swedish));

        cut.Find("input").Click();

        Assert.Equal(["08:00", "08:15", "08:30", "08:45", "09:00"], cut.FindAll("[role=option]").Select(o => o.TextContent));
    }

    [Fact]
    public void TimePickerTakesATypedTimeOnBlur()
    {
        using var context = new BunitContext();
        TimeOnly? value = null;

        var cut = context.Render<StilTimePicker<TimeOnly?>>(parameters => parameters
            .Add(component => component.Culture, Swedish)
            .Add(component => component.ValueChanged, (TimeOnly? time) => value = time));

        var input = cut.Find("input");
        input.Input("1345");
        cut.Find(".stil-combobox").TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal(new TimeOnly(13, 45), value);
        Assert.Equal("13:45", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void TimePickerMarksTextThatIsNotATime()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilTimePicker<TimeOnly?>>(parameters => parameters.Add(component => component.Culture, Swedish));

        cut.Find("input").Input("lunch");
        cut.Find(".stil-combobox").TriggerEvent("onfocusout", new FocusEventArgs());

        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
        Assert.Equal("lunch", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void TimePickerChoosesAnOption()
    {
        using var context = new BunitContext();
        TimeOnly? value = null;

        var cut = context.Render<StilTimePicker<TimeOnly?>>(parameters => parameters
            .Add(component => component.Culture, Swedish)
            .Add(component => component.ValueChanged, (TimeOnly? time) => value = time));

        cut.Find("input").Input("10");
        cut.FindAll("[role=option]")[1].Click();

        Assert.Equal(new TimeOnly(10, 30), value);
    }

    [Fact]
    public void DateTimePickerUsesTheDefaultTimeForANewDate()
    {
        using var context = new BunitContext();
        DateTime? value = null;

        var cut = context.Render<StilDateTimePicker<DateTime?>>(parameters => parameters
            .Add(component => component.Label, "Delivery")
            .Add(component => component.Culture, Swedish)
            .Add(component => component.ValueChanged, (DateTime? v) => value = v));

        cut.Find("input[type=date]").Change("2026-11-03");

        Assert.Equal(new DateTime(2026, 11, 3, 9, 0, 0), value);
        Assert.Equal("Delivery", cut.Find("legend").TextContent);
        Assert.Equal("09:00", cut.Find("[role=combobox]").GetAttribute("value"));
    }

    // Sliders and rating

    [Fact]
    public void SliderSetsTheValueWhileDragging()
    {
        using var context = new BunitContext();
        var value = 0;

        var cut = context.Render<StilSlider<int>>(parameters => parameters
            .Add(component => component.Label, "Duration")
            .Add(component => component.Max, 120)
            .Add(component => component.Step, 15)
            .Add(component => component.Format, "0 'min'")
            .Add(component => component.ValueChanged, (int v) => value = v));

        cut.Find("input[type=range]").Input("45");

        Assert.Equal(45, value);
        cut.Render(parameters => parameters.Add(component => component.Value, 45));
        Assert.Equal("45 min", cut.Find("output").TextContent);
        Assert.Equal("--_fill: 37.5%", cut.Find("input").GetAttribute("style"));
        Assert.Equal("45 min", cut.Find("input").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void RangeSliderThumbsCannotPassEachOther()
    {
        using var context = new BunitContext();
        int? start = 20, end = 60;

        var cut = context.Render<StilRangeSlider<int?>>(parameters => parameters
            .Add(component => component.Label, "Price")
            .Add(component => component.Start, start)
            .Add(component => component.End, end)
            .Add(component => component.StartChanged, (int? v) => start = v)
            .Add(component => component.EndChanged, (int? v) => end = v));

        cut.FindAll("input")[0].Input("80");

        Assert.Equal(60, start);
        Assert.Equal(["Minimum", "Maximum"], cut.FindAll("input").Select(i => i.GetAttribute("aria-label")));
        Assert.Equal("--_from: 60; --_to: 60", cut.Find(".stil-range").GetAttribute("style"));
    }

    [Fact]
    public void RatingIsARadioGroup()
    {
        using var context = new BunitContext();
        int? value = 3;

        var cut = context.Render<StilRating<int?>>(parameters => parameters
            .Add(component => component.Label, "Service")
            .Add(component => component.Value, value)
            .Add(component => component.ValueChanged, (int? v) => value = v));

        var radios = cut.FindAll("input[type=radio]");
        Assert.Equal(5, radios.Count);
        Assert.True(radios[2].HasAttribute("checked"));
        Assert.Equal("4 of 5", radios[3].GetAttribute("aria-label"));

        radios[4].Change("5");
        Assert.Equal(5, value);
    }

    [Fact]
    public void ReadOnlyRatingIsOneImage()
    {
        using var context = new BunitContext();

        var cut = context.Render<StilRating<int>>(parameters => parameters
            .Add(component => component.Value, 4)
            .Add(component => component.ReadOnly, true));

        Assert.Equal("4 of 5", cut.Find("[role=img]").GetAttribute("aria-label"));
        Assert.Equal(4, cut.FindAll(".stil-rating-on").Count);
        Assert.Empty(cut.FindAll("input"));
    }

    // Color

    [Theory]
    [InlineData("#ABC", "#aabbcc")]
    [InlineData("7048E8", "#7048e8")]
    [InlineData("#12345", null)]
    [InlineData("teal", null)]
    public void ColorsAreNormalized(string text, string? expected) => Assert.Equal(expected, StilColorPicker.Normalize(text));

    [Fact]
    public void ContrastFollowsWcag()
    {
        Assert.Equal(21, StilColorPicker.Contrast("#000000", "#ffffff"), 1);
        Assert.Equal(1, StilColorPicker.Contrast("#7048e8", "#7048e8"), 1);
    }

    [Fact]
    public void ColorPickerSwatchesAndHint()
    {
        using var context = new BunitContext();
        string? value = "#ffffff";

        var cut = context.Render<StilColorPicker>(parameters => parameters
            .Add(component => component.Label, "Brand")
            .Add(component => component.Value, value)
            .Add(component => component.Swatches, ["#7048E8", "#ffffff"])
            .Add(component => component.ValueChanged, (string? v) => value = v));

        Assert.Contains("black (21", cut.Find(".stil-field-hint").TextContent);
        Assert.Equal("true", cut.FindAll(".stil-color-swatch")[1].GetAttribute("aria-pressed"));

        cut.FindAll(".stil-color-swatch")[0].Click();
        Assert.Equal("#7048e8", value);
    }

    // Masks and codes

    [Theory]
    [InlineData("000000-0000", "1970010112", "197001-0112")]
    [InlineData("000 00", "11455", "114 55")]
    [InlineData("AAA 000", "abc123", "abc 123")]
    [InlineData("(000) 000", "08-555", "(085) 55")]
    public void MasksFormatText(string mask, string text, string expected) => Assert.Equal(expected, StilMaskedInput.Apply(mask, text));

    [Fact]
    public void MaskedInputFormatsAndFlagsIncompleteValues()
    {
        using var context = new BunitContext();
        string? value = null;

        var cut = context.Render<StilMaskedInput>(parameters => parameters
            .Add(component => component.Label, "Postal code")
            .Add(component => component.Mask, "000 00")
            .Add(component => component.ValueChanged, (string? v) => value = v));

        var input = cut.Find("input");
        Assert.Equal("000 00", input.GetAttribute("data-stil-mask"));
        Assert.Equal("numeric", input.GetAttribute("inputmode"));

        input.Change("1145");
        Assert.Equal("114 5", value);
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
        Assert.True(StilMaskedInput.IsComplete("000 00", "114 55"));
    }

    [Fact]
    public void PinInputKeepsDigitsAndReportsCompletion()
    {
        using var context = new BunitContext();
        string? value = null;
        string? completed = null;

        var cut = context.Render<StilPinInput>(parameters => parameters
            .Add(component => component.Label, "Code")
            .Add(component => component.Length, 4)
            .Add(component => component.ValueChanged, (string? v) => value = v)
            .Add(component => component.OnComplete, (string code) => completed = code));

        var input = cut.Find("input");
        Assert.Equal("one-time-code", input.GetAttribute("autocomplete"));
        Assert.Equal("\\d{4}", input.GetAttribute("pattern"));

        input.Input("12 a3");
        Assert.Equal("123", value);
        Assert.Null(completed);

        cut.Find("input").Input("12345");
        Assert.Equal("1234", completed);
    }

    // Stepper and wizard

    [Fact]
    public void StepperMarksDoneCurrentAndErrorSteps()
    {
        using var context = new BunitContext();
        var clicked = -1;

        var cut = context.Render<StilStepper>(parameters => parameters
            .Add(component => component.Steps, new StilStepItem[] { new("Account"), new("Address", State: StilStepState.Error), new("Confirm") })
            .Add(component => component.Current, 2)
            .Add(component => component.OnStepClick, (int index) => clicked = index));

        var steps = cut.FindAll(".stil-step");
        Assert.Contains("stil-step-done", steps[0].ClassName);
        Assert.Contains("stil-step-error", steps[1].ClassName);
        Assert.Equal("step", steps[2].GetAttribute("aria-current"));
        Assert.Contains("completed", steps[0].TextContent);
        Assert.Equal("Progress", cut.Find("nav").GetAttribute("aria-label"));

        Assert.Single(cut.FindAll(".stil-step-button"));
        cut.Find(".stil-step-button").Click();
        Assert.Equal(0, clicked);
    }

    [Fact]
    public void WizardValidatesOnlyTheCurrentStep()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<TestComponents.WizardHost>();

        // Step 1: the name is required.
        cut.FindAll("button").Single(b => b.TextContent.Contains("Next")).Click();
        Assert.Contains("Name", cut.Find(".stil-wizard-title").TextContent);
        Assert.Contains("The Name field is required.", cut.Markup);
        Assert.Contains("stil-step-error", cut.FindAll(".stil-step")[0].ClassName);

        cut.Find("input").Change("Ada");
        cut.FindAll("button").Single(b => b.TextContent.Contains("Next")).Click();

        // Step 2: the email has not been validated yet.
        Assert.Contains("Email", cut.Find(".stil-wizard-title").TextContent);
        Assert.DoesNotContain("The Email field is required.", cut.Markup);
        Assert.Contains("stil-step-done", cut.FindAll(".stil-step")[0].ClassName);

        cut.FindAll("button").Single(b => b.TextContent.Contains("Finish")).Click();
        Assert.Contains("The Email field is required.", cut.Markup);
        Assert.False(cut.Instance.Finished);

        cut.Find("input").Change("ada@example.com");
        cut.FindAll("button").Single(b => b.TextContent.Contains("Finish")).Click();
        Assert.True(cut.Instance.Finished);

        cut.FindAll("button").Single(b => b.TextContent.Contains("Back")).Click();
        Assert.Contains("Name", cut.Find(".stil-wizard-title").TextContent);
    }

    [Fact]
    public void NewFieldTextsAreLocalized() => StilLocalizationTests.InCulture("sv-SE", () =>
    {
        using var context = new BunitContext();

        var cut = context.Render<StilRating<int>>(parameters => parameters.Add(component => component.Value, 2).Add(component => component.ReadOnly, true));

        Assert.Equal("2 av 5", cut.Find("[role=img]").GetAttribute("aria-label"));
    });
}
