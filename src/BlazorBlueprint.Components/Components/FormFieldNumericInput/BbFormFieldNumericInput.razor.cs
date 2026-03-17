using System.Linq.Expressions;
using System.Numerics;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbNumericInput{TValue}"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
/// <remarks>
/// <para>
/// FormFieldNumericInput provides a higher-level abstraction over <see cref="BbNumericInput{TValue}"/>.
/// It automatically handles label association, helper text display, and error message rendering.
/// </para>
/// <para>
/// For full control over layout and error display, use <see cref="BbNumericInput{TValue}"/> directly
/// with the <see cref="BbField"/> component system.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The numeric type (must implement <see cref="INumber{TSelf}"/>).</typeparam>
/// <example>
/// <code>
/// &lt;BbFormFieldNumericInput TValue="int"
///                          @bind-Value="quantity"
///                          Label="Quantity"
///                          HelperText="Enter a value between 1 and 100"
///                          Min="1"
///                          Max="100"
///                          ShowButtons="true" /&gt;
/// </code>
/// </example>
public partial class BbFormFieldNumericInput<TValue> : FormFieldBase where TValue : struct, INumber<TValue>
{
    // --- NumericInput Pass-Through Parameters ---

    /// <summary>
    /// Gets or sets the current numeric value.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// Automatically provided by <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute. Passed through to the inner NumericInput.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    [Parameter]
    public TValue? Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    [Parameter]
    public TValue? Max { get; set; }

    /// <summary>
    /// Gets or sets the step increment for arrow key/button changes.
    /// </summary>
    [Parameter]
    public TValue? Step { get; set; }

    /// <summary>
    /// Gets or sets the number of decimal places to display.
    /// </summary>
    [Parameter]
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// Gets or sets whether negative values are allowed.
    /// </summary>
    [Parameter]
    public bool AllowNegative { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show increment/decrement buttons.
    /// </summary>
    [Parameter]
    public bool ShowButtons { get; set; }

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
    /// Gets or sets the format string for displaying the value.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets whether debounce is disabled during typing.
    /// </summary>
    [Parameter]
    public bool DisableDebounce { get; set; }

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// </summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 500;

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner NumericInput element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(TValue value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }
}
