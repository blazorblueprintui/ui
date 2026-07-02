using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbMaskedInput"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class BbFormFieldMaskedInput : FormFieldBase
{
    /// <summary>
    /// Gets or sets the unmasked (raw) value.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback invoked when the unmasked value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<string>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the predefined mask preset.
    /// </summary>
    [Parameter]
    public MaskPreset Preset { get; set; } = MaskPreset.Custom;

    /// <summary>
    /// Gets or sets the custom mask pattern (when Preset is Custom).
    /// Characters: 9=digit, A=letter, *=alphanumeric.
    /// </summary>
    [Parameter]
    public string? Mask { get; set; }

    /// <summary>
    /// Gets or sets the placeholder character for unfilled positions.
    /// </summary>
    [Parameter]
    public char PlaceholderChar { get; set; } = '_';

    /// <summary>
    /// Gets or sets whether to show mask with placeholders.
    /// </summary>
    [Parameter]
    public bool ShowMask { get; set; } = true;

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the input is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the input is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner MaskedInput element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(string value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }
}
