using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// BbFormFieldSelect gives its trigger button the field's generated id and invalid state, so the
/// label, error text and aria-invalid all reach it.
/// </summary>
public class FormFieldSelectTriggerTests
{
    private static readonly SelectOption<string>[] Options = [new("a", "Alpha")];

    private sealed class Model
    {
        public string? Fruit { get; set; }
    }

    [Fact]
    public async Task LabelPointsAtTheTrigger()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbFormFieldSelect<string>>(parameters => parameters
            .Add(p => p.Label, "Fruit")
            .Add(p => p.Options, Options));

        var labelFor = cut.Find("label").GetAttribute("for");
        Assert.False(string.IsNullOrEmpty(labelFor));
        Assert.Equal(labelFor, Trigger(cut).Id);
        Assert.Single(cut.FindAll($"[id='{labelFor}']"));
    }

    [Theory]
    [InlineData(false, "Pick one you like")]
    [InlineData(true, "Pick a fruit")]
    public async Task TriggerFollowsErrorText(bool invalid, string message)
    {
        await using var context = CreateContext();

        var cut = context.Render<BbFormFieldSelect<string>>(parameters =>
        {
            parameters.Add(p => p.Options, Options);
            if (invalid)
            {
                parameters.Add(p => p.ErrorText, message);
            }
            else
            {
                parameters.Add(p => p.HelperText, message);
            }
        });

        AssertInvalid(cut, invalid);
        AssertDescribedBy(cut, message);
    }

    // Validation runs after the first render, the way a submit does; the trigger and its
    // description follow it both ways.
    [Fact]
    public async Task TriggerFollowsFormValidation()
    {
        await using var context = CreateContext();
        var model = new Model();
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);
        var field = editContext.Field(nameof(Model.Fruit));
        const string message = "Pick a fruit.";

        var cut = context.Render<BbFormFieldSelect<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValueExpression, () => model.Fruit));

        AssertInvalid(cut, false);

        await cut.InvokeAsync(() =>
        {
            messages.Add(field, message);
            editContext.NotifyValidationStateChanged();
        });
        AssertInvalid(cut, true);
        AssertDescribedBy(cut, message);

        await cut.InvokeAsync(() =>
        {
            messages.Clear(field);
            editContext.NotifyValidationStateChanged();
        });
        AssertInvalid(cut, false);
    }

    // The content's portal loads its JS module on first render, open or not.
    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static AngleSharp.Dom.IElement Trigger(IRenderedComponent<BbFormFieldSelect<string>> cut) =>
        cut.Find("button[role='combobox']");

    private static void AssertInvalid(IRenderedComponent<BbFormFieldSelect<string>> cut, bool invalid) =>
        Assert.Equal(invalid ? "true" : null, Trigger(cut).GetAttribute("aria-invalid"));

    private static void AssertDescribedBy(IRenderedComponent<BbFormFieldSelect<string>> cut, string message)
    {
        var describedBy = Trigger(cut).GetAttribute("aria-describedby");
        Assert.False(string.IsNullOrEmpty(describedBy));
        Assert.Contains(message, cut.Find($"[id='{describedBy}']").TextContent, StringComparison.Ordinal);
    }
}
