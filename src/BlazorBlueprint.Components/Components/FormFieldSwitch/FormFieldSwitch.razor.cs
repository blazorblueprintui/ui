using System.Linq.Expressions;
using BlazorBlueprint.Components.Field;
using BlazorBlueprint.Components.Switch;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormFieldSwitch;

/// <summary>
/// A form field wrapper for <see cref="Switch.Switch"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class FormFieldSwitch : FormFieldBase
{
    /// <summary>
    /// Gets or sets whether the switch is checked (on).
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
    /// Gets or sets the size variant of the switch.
    /// </summary>
    [Parameter]
    public SwitchSize Size { get; set; } = SwitchSize.Medium;

    /// <summary>
    /// Gets or sets whether the switch is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner Switch element.
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
