using System.Linq.Expressions;
using BlazorBlueprint.Components.Field;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormFieldSelect;

/// <summary>
/// A form field wrapper for the Select component that provides
/// automatic label, helper text, and error message display with a simplified API.
/// </summary>
/// <remarks>
/// FormFieldSelect auto-generates the SelectTrigger, SelectValue, and SelectContent.
/// Users only need to provide SelectItem children as ChildContent.
/// </remarks>
/// <typeparam name="TValue">The type of the selected value.</typeparam>
public partial class FormFieldSelect<TValue> : FormFieldBase
{
    /// <summary>
    /// Gets or sets the currently selected value.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets whether the select is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed when no value is selected.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets a function to derive display text from the selected value.
    /// </summary>
    [Parameter]
    public Func<TValue, string>? DisplayTextSelector { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner Select element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the select dropdown.
    /// Should contain SelectItem components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(TValue? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }
}
