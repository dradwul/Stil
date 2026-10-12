using Bunit;
using Stil.Blazor.Tests.TestComponents;

namespace Stil.Blazor.Tests;

/// <summary>Stil fields inside an EditForm with DataAnnotationsValidator (see TestComponents/SignUpForm.razor).</summary>
public class FormValidationTests
{
    public static TheoryData<string, string, string> ControlFields => new()
    {
        // input id, control selector, expected message
        { "name", "input#name", "Name is required." },
        { "email", "input#email", "Email is required." },
        { "bio", "textarea#bio", "Bio is required." },
        { "age", "input#age", "Age is required." },
        { "start", "input#start", "Start date is required." },
        { "color", "select#color", "Color is required." },
        { "terms", "input#terms", "You must accept the terms." },
        { "notifications", "input#notifications", "Notifications must be on." },
    };

    [Fact]
    public void FieldsShowNoErrorsBeforeSubmit()
    {
        using var context = new BunitContext();

        var cut = context.Render<SignUpForm>();

        Assert.Empty(cut.FindAll(".stil-field-error"));
        Assert.Empty(cut.FindAll("[aria-invalid]"));
        Assert.Equal("email-hint", cut.Find("#email").GetAttribute("aria-describedby"));
        Assert.False(cut.Find("#name").HasAttribute("aria-describedby"));
    }

    [Theory]
    [MemberData(nameof(ControlFields))]
    public void SubmitShowsErrorLinkedToInvalidControl(string id, string selector, string message)
    {
        using var context = new BunitContext();
        var cut = context.Render<SignUpForm>();

        cut.Find("form").Submit();

        var error = cut.Find($"#{id}-error");
        Assert.Equal(["stil-field-error"], error.ClassList);
        Assert.Equal(message, error.TextContent.Trim());
        Assert.Equal("stil-field", error.ParentElement?.ClassName);

        var control = cut.Find(selector);
        Assert.Equal("true", control.GetAttribute("aria-invalid"));
        Assert.Contains($"{id}-error", control.GetAttribute("aria-describedby")!.Split(' '));
        Assert.Equal(0, cut.Instance.ValidSubmits);
    }

    [Fact]
    public void ErrorIdFollowsHintIdInAriaDescribedBy()
    {
        using var context = new BunitContext();
        var cut = context.Render<SignUpForm>();

        cut.Find("form").Submit();

        Assert.Equal("email-hint email-error", cut.Find("#email").GetAttribute("aria-describedby"));
        Assert.Equal("name-error", cut.Find("#name").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void SubmitShowsRadioGroupErrorLinkedToFieldset()
    {
        using var context = new BunitContext();
        var cut = context.Render<SignUpForm>();

        cut.Find("form").Submit();

        var fieldset = cut.Find("fieldset#plan");
        var error = fieldset.QuerySelector(".stil-field-error");
        Assert.NotNull(error);
        Assert.Equal("plan-error", error.Id);
        Assert.Equal("Plan is required.", error.TextContent.Trim());
        Assert.Equal("plan-error", fieldset.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void SubmitMarksRadioGroupInvalid()
    {
        using var context = new BunitContext();
        var cut = context.Render<SignUpForm>();

        cut.Find("form").Submit();

        Assert.All(cut.FindAll("fieldset#plan input[type=radio]"), radio => Assert.Equal("true", radio.GetAttribute("aria-invalid")));
    }

    [Fact]
    public void InvalidEmailShowsFormatMessageWhenChanged()
    {
        using var context = new BunitContext();
        var cut = context.Render<SignUpForm>();

        cut.Find("#email").Change("not-an-email");

        Assert.Equal("Email is not valid.", cut.Find("#email-error").TextContent.Trim());
        Assert.Equal("true", cut.Find("#email").GetAttribute("aria-invalid"));
        // Only the changed field is validated.
        Assert.Empty(cut.FindAll("#name-error"));
    }

    [Fact]
    public void FixingFieldsRemovesErrorsAndAllowsValidSubmit()
    {
        using var context = new BunitContext();
        var model = new SignUpModel();
        var cut = context.Render<SignUpForm>(parameters => parameters.Add(component => component.Model, model));
        cut.Find("form").Submit();
        Assert.NotEmpty(cut.FindAll(".stil-field-error"));

        cut.Find("#name").Change("Ada");
        cut.Find("#email").Change("ada@example.com");
        cut.Find("#bio").Change("Mathematician");
        cut.Find("#age").Change("36");
        cut.Find("#start").Change("2025-01-15");
        cut.Find("#color").Change("Green");
        cut.Find("#terms").Change(true);
        cut.Find("#notifications").Change(true);
        var pro = cut.FindAll("fieldset#plan input[type=radio]")[1];
        pro.Change(pro.GetAttribute("value"));

        Assert.Equal("Ada", model.Name);
        Assert.Equal("ada@example.com", model.Email);
        Assert.Equal("Mathematician", model.Bio);
        Assert.Equal(36, model.Age);
        Assert.Equal(new DateOnly(2025, 1, 15), model.StartDate);
        Assert.Equal(Color.Green, model.Color);
        Assert.True(model.AcceptTerms);
        Assert.True(model.Notifications);
        Assert.Equal(Plan.Pro, model.Plan);

        Assert.Empty(cut.FindAll(".stil-field-error"));
        Assert.Empty(cut.FindAll("[aria-invalid]"));
        Assert.Equal("email-hint", cut.Find("#email").GetAttribute("aria-describedby"));
        Assert.False(cut.Find("#name").HasAttribute("aria-describedby"));
        Assert.False(cut.Find("fieldset#plan").HasAttribute("aria-describedby"));

        cut.Find("form").Submit();

        Assert.Equal(1, cut.Instance.ValidSubmits);
    }

    [Fact]
    public void InputsKeepStilClassesNextToBlazorValidationClasses()
    {
        using var context = new BunitContext();
        var cut = context.Render<SignUpForm>();

        cut.Find("form").Submit();

        Assert.Contains("stil-input", cut.Find("#name").ClassList);
        Assert.Contains("invalid", cut.Find("#name").ClassList);
        Assert.Contains("stil-select", cut.Find("#color").ClassList);
        Assert.Contains("stil-check-input", cut.Find("#terms").ClassList);
        Assert.Contains("stil-switch", cut.Find("#notifications").ClassList);
    }
}
