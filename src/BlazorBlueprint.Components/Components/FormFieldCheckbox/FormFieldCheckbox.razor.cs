using System.Linq.Expressions;
using BlazorBlueprint.Components.Field;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormFieldCheckbox;

/// <summary>
/// A form field wrapper for <see cref="Checkbox.Checkbox"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class FormFieldCheckbox : FormFieldBase
{
    /// <summary>
    /// Gets or sets whether the checkbox is checked.
    /// </summary>
    [Parameter]
    public bool Checked { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the checked state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<bool>>? CheckedExpression { get; set; }

    /// <summary>
    /// Gets or sets whether the checkbox is in an indeterminate state.
    /// </summary>
    [Parameter]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the indeterminate state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IndeterminateChanged { get; set; }

    /// <summary>
    /// Gets or sets whether the checkbox is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner Checkbox element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => CheckedExpression;

    private async Task HandleCheckedChanged(bool value)
    {
        Checked = value;
        await CheckedChanged.InvokeAsync(value);
    }
}
