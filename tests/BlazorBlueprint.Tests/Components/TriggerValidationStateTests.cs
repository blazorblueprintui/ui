using System.Reflection;
using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// With <c>IsInvalid</c> unset, BbCombobox and BbMultiSelect mark their trigger <c>aria-invalid</c>
/// from the bound field's messages in the parent EditContext (#574). Validation normally runs after
/// the first render, on submit or when a field changes, so the trigger has to follow it without
/// waiting for its parent to render again.
/// </summary>
public class TriggerValidationStateTests
{
    private static readonly SelectOption<string>[] Options = [new("a", "Alpha")];

    private sealed class Model
    {
        public string? Fruit { get; set; }

        public IEnumerable<string>? Fruits { get; set; }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ComboboxFollowsValidationThatRunsAfterRender(bool initiallyInvalid)
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);
        form.SetError(nameof(Model.Fruit), initiallyInvalid);

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValueExpression, () => model.Fruit));
        AssertInvalid(cut, initiallyInvalid);

        await AssertFollowsValidationAsync(cut, form, nameof(Model.Fruit), !initiallyInvalid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultiSelectFollowsValidationThatRunsAfterRender(bool initiallyInvalid)
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);
        form.SetError(nameof(Model.Fruits), initiallyInvalid);

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValuesExpression, () => model.Fruits));
        AssertInvalid(cut, initiallyInvalid);

        await AssertFollowsValidationAsync(cut, form, nameof(Model.Fruits), !initiallyInvalid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ComboboxIsInvalidWinsOverValidationChanges(bool isInvalid)
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValueExpression, () => model.Fruit)
            .Add(p => p.IsInvalid, isInvalid));

        await AssertOverrideHoldsAsync(cut, form, nameof(Model.Fruit), isInvalid);

        cut.Render(parameters => parameters.Add(p => p.IsInvalid, null));

        await AssertFollowsValidationAsync(cut, form, nameof(Model.Fruit), true);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MultiSelectIsInvalidWinsOverValidationChanges(bool isInvalid)
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValuesExpression, () => model.Fruits)
            .Add(p => p.IsInvalid, isInvalid));

        await AssertOverrideHoldsAsync(cut, form, nameof(Model.Fruits), isInvalid);

        cut.Render(parameters => parameters.Add(p => p.IsInvalid, null));

        await AssertFollowsValidationAsync(cut, form, nameof(Model.Fruits), true);
    }

    [Fact]
    public async Task ComboboxForgetsTheFieldWhenItsBindingIsRemoved()
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);
        form.SetError(nameof(Model.Fruit), true);

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValueExpression, () => model.Fruit));
        AssertInvalid(cut, true);
        Assert.Equal(1, form.ValidationListeners);

        cut.Render(parameters => parameters.Add(p => p.ValueExpression, null));

        AssertInvalid(cut, false);
        Assert.Equal(0, form.ValidationListeners);
    }

    [Fact]
    public async Task MultiSelectForgetsTheFieldWhenItsBindingIsRemoved()
    {
        await using var context = CreateContext();
        var model = new Model();
        var form = new Form(model);
        form.SetError(nameof(Model.Fruits), true);

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .AddCascadingValue(form.EditContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValuesExpression, () => model.Fruits));
        AssertInvalid(cut, true);
        Assert.Equal(1, form.ValidationListeners);

        cut.Render(parameters => parameters.Add(p => p.ValuesExpression, null));

        AssertInvalid(cut, false);
        Assert.Equal(0, form.ValidationListeners);
    }

    [Fact]
    public async Task ComboboxListensOnlyToTheEditContextItIsIn()
    {
        var model = new Model();

        await AssertListensOnlyToCurrentEditContextAsync<BbCombobox<string>>(model, nameof(Model.Fruit), parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.ValueExpression, () => model.Fruit));
    }

    [Fact]
    public async Task MultiSelectListensOnlyToTheEditContextItIsIn()
    {
        var model = new Model();

        await AssertListensOnlyToCurrentEditContextAsync<BbMultiSelect<string>>(model, nameof(Model.Fruits), parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.ValuesExpression, () => model.Fruits));
    }

    // The popover's portal loads its JS module on first render, open or not.
    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static void AssertInvalid<TComponent>(IRenderedComponent<TComponent> cut, bool invalid)
        where TComponent : class, IComponent =>
        Assert.Equal(invalid ? "true" : null, cut.Find("button[role='combobox']").GetAttribute("aria-invalid"));

    // Flips the field's error and back, asserting straight after each notification: no parent render.
    private static async Task AssertFollowsValidationAsync<TComponent>(IRenderedComponent<TComponent> cut, Form form, string field, bool invalid)
        where TComponent : class, IComponent
    {
        await cut.InvokeAsync(() => form.SetError(field, invalid));
        AssertInvalid(cut, invalid);

        await cut.InvokeAsync(() => form.SetError(field, !invalid));
        AssertInvalid(cut, !invalid);
    }

    private static async Task AssertOverrideHoldsAsync<TComponent>(IRenderedComponent<TComponent> cut, Form form, string field, bool isInvalid)
        where TComponent : class, IComponent
    {
        AssertInvalid(cut, isInvalid);

        await cut.InvokeAsync(() => form.SetError(field, true));
        AssertInvalid(cut, isInvalid);

        await cut.InvokeAsync(() => form.SetError(field, false));
        AssertInvalid(cut, isInvalid);
    }

    private static async Task AssertListensOnlyToCurrentEditContextAsync<TComponent>(
        Model model, string field, Action<ComponentParameterCollectionBuilder<TComponent>> bind)
        where TComponent : class, IComponent
    {
        await using var context = CreateContext();

        // Resetting a form swaps in a new EditContext over the same model.
        var first = new Form(model);
        var second = new Form(model);

        var host = context.Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, first.EditContext)
            .AddChildContent(bind));
        var control = host.FindComponent<TComponent>().Instance;
        Assert.Equal(1, first.ValidationListeners);

        host.Render(parameters => parameters
            .Add(p => p.Value, second.EditContext)
            .AddChildContent(bind));

        Assert.Same(control, host.FindComponent<TComponent>().Instance);
        Assert.Equal(0, first.ValidationListeners);
        Assert.Equal(1, second.ValidationListeners);

        await host.InvokeAsync(() => first.SetError(field, true));
        AssertInvalid(host, false);

        await host.InvokeAsync(() => second.SetError(field, true));
        AssertInvalid(host, true);

        // Taken out of the form, the control stops listening.
        host.Render(parameters => parameters
            .Add(p => p.Value, second.EditContext)
            .AddChildContent(string.Empty));

        Assert.Empty(host.FindComponents<TComponent>());
        Assert.Equal(0, second.ValidationListeners);
    }

    private sealed class Form
    {
        private readonly ValidationMessageStore messages;

        public Form(Model model)
        {
            EditContext = new EditContext(model);
            messages = new ValidationMessageStore(EditContext);
        }

        public EditContext EditContext { get; }

        // EditContext doesn't say who listens, so read the event's backing field. A handler left
        // behind keeps a removed control alive and re-rendering on every validation pass.
        public int ValidationListeners =>
            (typeof(EditContext)
                .GetField(nameof(EditContext.OnValidationStateChanged), BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(EditContext) as Delegate)?
                .GetInvocationList().Length ?? 0;

        // Sets or clears the field's error and announces it, the way a validator does.
        public void SetError(string field, bool invalid)
        {
            var identifier = EditContext.Field(field);
            messages.Clear(identifier);
            if (invalid)
            {
                messages.Add(identifier, "Pick a fruit.");
            }

            EditContext.NotifyValidationStateChanged();
        }
    }
}
