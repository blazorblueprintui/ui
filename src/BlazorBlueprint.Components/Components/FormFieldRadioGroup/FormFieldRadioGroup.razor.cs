using System.Linq.Expressions;
using BlazorBlueprint.Components.Field;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormFieldRadioGroup;

/// <summary>
/// A form field wrapper for <see cref="RadioGroup.RadioGroup{TValue}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <typeparam name="TValue">The type of the value associated with radio group items.</typeparam>
public partial class FormFieldRadioGroup<TValue> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets whether the entire radio group is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner RadioGroup element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the radio group.
    /// Should contain RadioGroupItem components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(TValue value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }
}
