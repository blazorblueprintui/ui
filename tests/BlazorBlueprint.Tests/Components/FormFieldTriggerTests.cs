using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// BbFormFieldCombobox and BbFormFieldMultiSelect give their inner control's trigger button the
/// field's generated id and invalid state, so the label, error text and aria-invalid all reach it.
/// </summary>
public class FormFieldTriggerTests
{
    private static readonly SelectOption<string>[] Options = [new("a", "Alpha")];

    private sealed class Model
    {
        public string? Fruit { get; set; }

        public IEnumerable<string>? Fruits { get; set; }
    }

    [Fact]
    public async Task ComboboxLabelPointsAtTheTrigger()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbFormFieldCombobox<string>>(parameters => parameters
            .Add(p => p.Label, "Fruit")
            .Add(p => p.Options, Options));

        AssertLabelPointsAtTrigger(cut);
    }

    [Fact]
    public async Task MultiSelectLabelPointsAtTheTrigger()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbFormFieldMultiSelect<string>>(parameters => parameters
            .Add(p => p.Label, "Fruits")
            .Add(p => p.Options, Options));

        AssertLabelPointsAtTrigger(cut);
    }

    [Theory]
    [InlineData(false, "Pick one you like")]
    [InlineData(true, "Pick a fruit")]
    public async Task ComboboxTriggerFollowsErrorText(bool invalid, string message)
    {
        await using var context = CreateContext();

        var cut = context.Render<BbFormFieldCombobox<string>>(parameters =>
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

    [Theory]
    [InlineData(false, "Pick the ones you like")]
    [InlineData(true, "Pick at least one fruit")]
    public async Task MultiSelectTriggerFollowsErrorText(bool invalid, string message)
    {
        await using var context = CreateContext();

        var cut = context.Render<BbFormFieldMultiSelect<string>>(parameters =>
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

    [Fact]
    public async Task ComboboxTriggerFollowsFormValidation()
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);

        var cut = context.Render<BbFormFieldCombobox<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValueExpression, () => model.Fruit));

        await AssertFollowsValidationAsync(cut, form, nameof(Model.Fruit));
    }

    [Fact]
    public async Task MultiSelectTriggerFollowsFormValidation()
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);

        var cut = context.Render<BbFormFieldMultiSelect<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValuesExpression, () => model.Fruits));

        await AssertFollowsValidationAsync(cut, form, nameof(Model.Fruits));
    }

    // The popover's portal loads its JS module on first render, open or not.
    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static void AssertLabelPointsAtTrigger<TComponent>(IRenderedComponent<TComponent> cut)
        where TComponent : class, IComponent
    {
        var labelFor = cut.Find("label").GetAttribute("for");
        Assert.False(string.IsNullOrEmpty(labelFor));
        Assert.Equal(labelFor, cut.Find("button[role='combobox']").Id);
        Assert.Single(cut.FindAll($"[id='{labelFor}']"));
    }

    private static void AssertInvalid<TComponent>(IRenderedComponent<TComponent> cut, bool invalid)
        where TComponent : class, IComponent =>
        Assert.Equal(invalid ? "true" : null, cut.Find("button[role='combobox']").GetAttribute("aria-invalid"));

    private static void AssertDescribedBy<TComponent>(IRenderedComponent<TComponent> cut, string message)
        where TComponent : class, IComponent
    {
        var describedBy = cut.Find("button[role='combobox']").GetAttribute("aria-describedby");
        Assert.False(string.IsNullOrEmpty(describedBy));
        Assert.Contains(message, cut.Find($"[id='{describedBy}']").TextContent, StringComparison.Ordinal);
    }

    // Validation runs after the first render, the way a submit does; the trigger and its
    // description follow it both ways.
    private static async Task AssertFollowsValidationAsync<TComponent>(IRenderedComponent<TComponent> cut, Form form, string field)
        where TComponent : class, IComponent
    {
        AssertInvalid(cut, false);

        await cut.InvokeAsync(() => form.SetError(field, true));
        AssertInvalid(cut, true);
        AssertDescribedBy(cut, Form.Message);

        await cut.InvokeAsync(() => form.SetError(field, false));
        AssertInvalid(cut, false);
    }

    private sealed class Form
    {
        public const string Message = "Pick a fruit.";

        private readonly ValidationMessageStore messages;

        public Form(Model model)
        {
            EditContext = new EditContext(model);
            messages = new ValidationMessageStore(EditContext);
        }

        public EditContext EditContext { get; }

        // Sets or clears the field's error and announces it, the way a validator does.
        public void SetError(string field, bool invalid)
        {
            var identifier = EditContext.Field(field);
            messages.Clear(identifier);
            if (invalid)
            {
                messages.Add(identifier, Message);
            }

            EditContext.NotifyValidationStateChanged();
        }
    }
}
