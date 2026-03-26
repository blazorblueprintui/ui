using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbInputOTP"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class BbFormFieldInputOTP : FormFieldBase
{
    /// <summary>
    /// Gets or sets the current OTP value.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the number of OTP digits.
    /// </summary>
    [Parameter]
    public int Length { get; set; } = 6;

    /// <summary>
    /// Gets or sets the callback invoked when all digits are filled.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnComplete { get; set; }

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
    /// Gets or sets whether to show separators between groups.
    /// </summary>
    [Parameter]
    public bool ShowSeparator { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of digits per group.
    /// </summary>
    [Parameter]
    public int GroupSize { get; set; } = 3;

    /// <summary>
    /// Gets or sets custom separator content.
    /// </summary>
    [Parameter]
    public RenderFragment? Separator { get; set; }

    /// <summary>
    /// Gets or sets the size variant for the input boxes.
    /// </summary>
    [Parameter]
    public InputOTPSize Size { get; set; } = InputOTPSize.Default;

    /// <summary>
    /// Gets or sets whether to mask input values with asterisks.
    /// </summary>
    [Parameter]
    public bool MaskInput { get; set; }

    /// <summary>
    /// Gets or sets the types of characters accepted.
    /// </summary>
    [Parameter]
    public InputOTPInputMode InputMode { get; set; } = InputOTPInputMode.Numbers;

    /// <summary>
    /// Gets or sets additional CSS classes applied to each OTP input box.
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
