using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Floating;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorBlueprint.Tests.Components;

/// <summary>
/// BbCombobox and BbMultiSelect render their trigger button inside a root div, so the id, test
/// attributes and invalid state a form field gives them have to be routed to the button (#574).
/// </summary>
public class TriggerAttributesTests
{
    private static readonly SelectOption<string>[] Options = [new("a", "Alpha"), new("b", "Beta")];

    private sealed class Model
    {
        public string? Fruit { get; set; }

        public IEnumerable<string>? Fruits { get; set; }
    }

    [Fact]
    public async Task ComboboxPutsTriggerIdAttributesAndInvalidStateOnTheButton()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerId, "fruit")
            .Add(p => p.TriggerAttributes, new Dictionary<string, object> { ["data-testid"] = "fruit-trigger" })
            .Add(p => p.IsInvalid, true)
            .AddUnmatched("data-root", "combobox-root"));

        AssertTriggerCarries(cut, "fruit", "fruit-trigger");
    }

    [Fact]
    public async Task MultiSelectPutsTriggerIdAttributesAndInvalidStateOnTheButton()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerId, "fruits")
            .Add(p => p.TriggerAttributes, new Dictionary<string, object> { ["data-testid"] = "fruits-trigger" })
            .Add(p => p.IsInvalid, true)
            .AddUnmatched("data-root", "combobox-root"));

        AssertTriggerCarries(cut, "fruits", "fruits-trigger");
    }

    [Fact]
    public async Task TriggerIdWinsOverAnIdInTriggerAttributes()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerId, "fruit")
            .Add(p => p.TriggerAttributes, new Dictionary<string, object> { ["id"] = "ignored" }));

        Assert.Equal("fruit", cut.Find("button[role='combobox']").Id);
        Assert.Empty(cut.FindAll("#ignored"));
    }

    [Fact]
    public async Task AnIdInTriggerAttributesIsUsedWhenTriggerIdIsUnset()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerAttributes, new Dictionary<string, object> { ["id"] = "fruits" }));

        Assert.Equal("fruits", cut.Find("button[role='combobox']").Id);
        Assert.Equal("fruits", cut.FindComponent<BbFloatingPortal>().Instance.RestoreFocusToId);
    }

    [Fact]
    public async Task WithoutTriggerIdTheButtonKeepsTheGeneratedIdAndNoInvalidState()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbCombobox<string>>(parameters => parameters.Add(p => p.Options, Options));

        var trigger = cut.Find("button[role='combobox']");
        Assert.Matches("^popover-.+-trigger$", trigger.Id);
        Assert.Null(trigger.GetAttribute("aria-invalid"));
        Assert.Equal(trigger.Id, cut.FindComponent<BbFloatingPortal>().Instance.RestoreFocusToId);
    }

    [Fact]
    public async Task ComboboxFollowsTheEditContextWhenIsInvalidIsUnset()
    {
        await using var context = CreateContext();
        var model = new Model();
        var editContext = new EditContext(model);
        new ValidationMessageStore(editContext).Add(editContext.Field(nameof(Model.Fruit)), "Pick a fruit.");

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValueExpression, () => model.Fruit));

        Assert.Equal("true", cut.Find("button[role='combobox']").GetAttribute("aria-invalid"));

        cut.Render(parameters => parameters.Add(p => p.IsInvalid, false));

        Assert.Null(cut.Find("button[role='combobox']").GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task MultiSelectFollowsTheEditContextWhenIsInvalidIsUnset()
    {
        await using var context = CreateContext();
        var model = new Model();
        var editContext = new EditContext(model);
        new ValidationMessageStore(editContext).Add(editContext.Field(nameof(Model.Fruits)), "Pick a fruit.");

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .AddCascadingValue(editContext)
            .Add(p => p.Options, Options)
            .Add(p => p.ValuesExpression, () => model.Fruits));

        Assert.Equal("true", cut.Find("button[role='combobox']").GetAttribute("aria-invalid"));

        cut.Render(parameters => parameters.Add(p => p.IsInvalid, false));

        Assert.Null(cut.Find("button[role='combobox']").GetAttribute("aria-invalid"));
    }

    [Fact]
    public async Task ComboboxReturnsFocusToACustomTriggerIdOnEscape()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbCombobox<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerId, "fruit"));

        await AssertEscapeRestoresFocusToAsync(cut, "fruit");
    }

    [Fact]
    public async Task MultiSelectReturnsFocusToACustomTriggerIdOnEscape()
    {
        await using var context = CreateContext();

        var cut = context.Render<BbMultiSelect<string>>(parameters => parameters
            .Add(p => p.Options, Options)
            .Add(p => p.TriggerId, "fruits"));

        await AssertEscapeRestoresFocusToAsync(cut, "fruits");
    }

    // The popover's portal loads its JS module on first render, open or not.
    private static ComponentBunitContext CreateContext()
    {
        var context = new ComponentBunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static void AssertTriggerCarries<TComponent>(IRenderedComponent<TComponent> cut, string id, string testId)
        where TComponent : class, IComponent
    {
        // A <label for="..."> resolves to the first element with that id: it must be the button.
        var labelled = cut.Find($"[id='{id}']");
        Assert.Equal("BUTTON", labelled.TagName);
        Assert.Equal("combobox", labelled.GetAttribute("role"));
        Assert.Single(cut.FindAll($"[id='{id}']"));

        Assert.Equal(testId, labelled.GetAttribute("data-testid"));
        Assert.Equal("true", labelled.GetAttribute("aria-invalid"));

        // AdditionalAttributes still land on the root, as documented since #486/#490.
        Assert.Equal("combobox-root", cut.Find("[data-root]").GetAttribute("data-root"));
        Assert.Equal("DIV", cut.Find("[data-root]").TagName);
    }

    private static async Task AssertEscapeRestoresFocusToAsync<TComponent>(IRenderedComponent<TComponent> cut, string id)
        where TComponent : class, IComponent
    {
        cut.Find($"#{id}").Click();

        var portal = cut.FindComponent<BbFloatingPortal>();
        Assert.True(portal.Instance.IsOpen);
        Assert.Equal(id, portal.Instance.RestoreFocusToId);
        Assert.Equal(id, portal.Instance.Dismiss?.TriggerId);

        await cut.InvokeAsync(portal.Instance.JsOnDismissEscape);

        Assert.False(portal.Instance.IsOpen);
        Assert.True(portal.Instance.RestoreFocusOnClose);
        Assert.Equal(id, portal.Instance.RestoreFocusToId);
        Assert.Equal("false", cut.Find($"#{id}").GetAttribute("aria-expanded"));
    }
}
