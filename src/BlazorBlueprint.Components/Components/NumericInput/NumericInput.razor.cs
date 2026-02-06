using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Numerics;

namespace BlazorBlueprint.Components.NumericInput;

/// <summary>
/// A generic numeric input component supporting int, double, decimal, float, long, and short.
/// </summary>
/// <typeparam name="TValue">The numeric type (must implement INumber&lt;TValue&gt;).</typeparam>
public partial class NumericInput<TValue> : ComponentBase where TValue : struct, INumber<TValue>
{
    private ElementReference _inputRef;
    private string _editingValue = string.Empty;
    private bool _isEditing;

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

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
    /// Gets or sets the number of decimal places to display (for decimal/double/float types).
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
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ID of the element that describes the input.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the input value is invalid.
    /// </summary>
    [Parameter]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    /// Gets or sets the format string for displaying the value.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    private TValue StepValue => Step ?? TValue.One;

    private bool IsAtMax => Max.HasValue && Value >= Max.Value;
    private bool IsAtMin => Min.HasValue && Value <= Min.Value;

    private string DisplayValue
    {
        get
        {
            if (_isEditing)
            {
                return _editingValue;
            }

            if (Format != null)
            {
                return string.Format(CultureInfo.InvariantCulture, $"{{0:{Format}}}", Value);
            }

            if (DecimalPlaces.HasValue && IsFloatingPoint)
            {
                return string.Format(CultureInfo.InvariantCulture, $"{{0:F{DecimalPlaces.Value}}}", Value);
            }

            return Value.ToString() ?? string.Empty;
        }
    }

    private static bool IsFloatingPoint =>
        typeof(TValue) == typeof(double) ||
        typeof(TValue) == typeof(float) ||
        typeof(TValue) == typeof(decimal);

    private static string InputMode => IsFloatingPoint ? "decimal" : "numeric";

    private string ContainerClass => ClassNames.cn(
        "flex items-center",
        ShowButtons ? "rounded-md overflow-hidden focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2 focus-within:ring-offset-background" : null
    );

    private string CssClass => ClassNames.cn(
        "flex h-10 w-full border border-input bg-background px-3 py-2 text-base",
        "placeholder:text-muted-foreground",
        ShowButtons ? "focus-visible:outline-none" : "rounded-md ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "aria-[invalid=true]:border-destructive",
        "transition-colors",
        "md:text-sm",
        ShowButtons ? "pr-8 border-r-0" : null,
        Class
    );

    private static string ButtonClass => ClassNames.cn(
        "flex items-center justify-center w-8 h-5 border border-input bg-background",
        "hover:bg-accent hover:text-accent-foreground",
        "focus-visible:outline-none",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "first:border-b-0",
        "transition-colors"
    );

    private void HandleInput(ChangeEventArgs args)
    {
        var inputValue = args.Value?.ToString() ?? string.Empty;
        _editingValue = inputValue;
        _isEditing = true;

        // Try to parse and update value in real-time
        if (TryParseValue(inputValue, out var parsedValue))
        {
            var clampedValue = ClampValue(parsedValue);
            if (!clampedValue.Equals(Value))
            {
                Value = clampedValue;
                ValueChanged.InvokeAsync(clampedValue);
            }
        }
    }

    private void HandleBlur(FocusEventArgs args)
    {
        _isEditing = false;

        // On blur, ensure we have a valid value
        if (TryParseValue(_editingValue, out var parsedValue))
        {
            var clampedValue = ClampValue(parsedValue);
            if (!clampedValue.Equals(Value))
            {
                Value = clampedValue;
                ValueChanged.InvokeAsync(clampedValue);
            }
        }

        StateHasChanged();
    }

    private void HandleFocus(FocusEventArgs args)
    {
        _editingValue = Value.ToString() ?? string.Empty;
        _isEditing = true;
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Disabled)
        {
            return;
        }

        switch (e.Key)
        {
            case "ArrowUp":
                await Increment();
                break;
            case "ArrowDown":
                await Decrement();
                break;
            case "PageUp":
                await IncrementBy(TValue.CreateChecked(10) * StepValue);
                break;
            case "PageDown":
                await DecrementBy(TValue.CreateChecked(10) * StepValue);
                break;
            case "Home":
                if (Min.HasValue)
                {
                    await SetValue(Min.Value);
                }
                break;
            case "End":
                if (Max.HasValue)
                {
                    await SetValue(Max.Value);
                }
                break;
        }
    }

    private async Task Increment()
    {
        if (Disabled || IsAtMax)
        {
            return;
        }
        await SetValue(Value + StepValue);
    }

    private async Task Decrement()
    {
        if (Disabled || IsAtMin)
        {
            return;
        }
        await SetValue(Value - StepValue);
    }

    private async Task IncrementBy(TValue amount)
    {
        if (Disabled)
        {
            return;
        }
        await SetValue(Value + amount);
    }

    private async Task DecrementBy(TValue amount)
    {
        if (Disabled)
        {
            return;
        }
        await SetValue(Value - amount);
    }

    private async Task SetValue(TValue value)
    {
        var clampedValue = ClampValue(value);

        if (!clampedValue.Equals(Value))
        {
            Value = clampedValue;
            _editingValue = clampedValue.ToString() ?? string.Empty;
            await ValueChanged.InvokeAsync(clampedValue);
        }
    }

    private TValue ClampValue(TValue value)
    {
        if (!AllowNegative && value < TValue.Zero)
        {
            value = TValue.Zero;
        }

        if (Min.HasValue && value < Min.Value)
        {
            value = Min.Value;
        }

        if (Max.HasValue && value > Max.Value)
        {
            value = Max.Value;
        }

        return value;
    }

    private bool TryParseValue(string? input, out TValue result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(input))
        {
            result = TValue.Zero;
            return true;
        }

        // Remove any thousand separators and normalize decimal separator
        input = input.Replace(",", "").Trim();

        // Handle negative sign
        if (!AllowNegative && input.StartsWith('-'))
        {
            return false;
        }

        return TValue.TryParse(input, CultureInfo.InvariantCulture, out result);
    }
}
