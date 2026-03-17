using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A form field wrapper for <see cref="BbCurrencyInput"/> that provides
/// automatic label, helper text, and error message display.
/// </summary>
public partial class BbFormFieldCurrencyInput : FormFieldBase
{
    /// <summary>
    /// Gets or sets the current decimal value.
    /// </summary>
    [Parameter]
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<decimal> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value for EditForm integration.
    /// </summary>
    [Parameter]
    public Expression<Func<decimal>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the ISO 4217 currency code.
    /// </summary>
    [Parameter]
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Gets or sets whether to show the currency symbol.
    /// </summary>
    [Parameter]
    public bool ShowSymbol { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    [Parameter]
    public decimal? Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    [Parameter]
    public decimal? Max { get; set; }

    /// <summary>
    /// Gets or sets whether negative values are allowed.
    /// </summary>
    [Parameter]
    public bool AllowNegative { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use thousand separators in display.
    /// </summary>
    [Parameter]
    public bool UseThousandSeparator { get; set; } = true;

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
    /// Gets or sets additional CSS classes applied to the inner CurrencyInput element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

    private async Task HandleValueChanged(decimal value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyFieldChanged();
    }
}
