using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;

namespace BlazorBlueprint.Components.NumericInput;

/// <summary>
/// A generic numeric input component supporting int, double, decimal, float, long, and short.
/// </summary>
/// <typeparam name="TValue">The numeric type (must implement INumber&lt;TValue&gt;).</typeparam>
public partial class NumericInput<TValue> : ComponentBase, IDisposable where TValue : struct, INumber<TValue>
{
    private ElementReference _inputRef;
    private string _editingValue = string.Empty;
    private bool _isEditing;
    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;
    private CancellationTokenSource? _debounceCts;

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

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
    /// Gets or sets the HTML name attribute for the input element.
    /// </summary>
    /// <remarks>
    /// When inside an EditForm and not explicitly set, the name is automatically
    /// derived from the ValueExpression (FieldIdentifier) to support SSR form postback.
    /// </remarks>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an expression that identifies the bound value.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets whether the input is in an invalid state based on EditContext validation.
    /// </summary>
    private bool IsInvalid
    {
        get
        {
            if (_editContext != null && ValueExpression != null && _fieldIdentifier.FieldName != null)
            {
                return _editContext.GetValidationMessages(_fieldIdentifier).Any();
            }
            return false;
        }
    }

    /// <summary>
    /// Gets the effective aria-invalid value combining manual AriaInvalid and EditContext validation.
    /// </summary>
    private string? EffectiveAriaInvalid
    {
        get
        {
            if (AriaInvalid == true || IsInvalid)
            {
                return "true";
            }
            return AriaInvalid?.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets the effective name attribute, falling back to the FieldIdentifier name when inside an EditForm.
    /// </summary>
    private string? EffectiveName => Name ?? (_editContext != null && _fieldIdentifier.FieldName != null ? _fieldIdentifier.FieldName : null);

    /// <summary>
    /// Gets or sets the format string for displaying the value.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets whether debounce is disabled. When false (default), <see cref="ValueChanged"/> is debounced during typing.
    /// </summary>
    [Parameter]
    public bool DisableDebounce { get; set; }

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds. Default is 500 ms.
    /// </summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 500;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (CascadedEditContext != null && ValueExpression != null)
        {
            _editContext = CascadedEditContext;
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }
    }

    private void NotifyFieldChanged()
    {
        if (_editContext != null && ValueExpression != null && _fieldIdentifier.FieldName != null)
        {
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }

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
        ShowButtons ? "rounded-md focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2 focus-within:ring-offset-background" : null
    );

    private string CssClass => ClassNames.cn(
        "flex h-10 w-full border border-input bg-background px-3 py-2 text-base",
        "placeholder:text-muted-foreground",
        ShowButtons ? "rounded-l-md focus-visible:outline-none" : "rounded-md ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
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
        "first:rounded-tr-md last:rounded-br-md",
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

                if (DisableDebounce)
                {
                    ValueChanged.InvokeAsync(clampedValue);
                }
                else
                {
                    DebounceValueChanged(clampedValue);
                }

                NotifyFieldChanged();
            }
        }
    }

    private void HandleBlur(FocusEventArgs args)
    {
        _isEditing = false;

        // Cancel pending debounce — blur commits immediately
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;

        // On blur, ensure we have a valid value
        if (TryParseValue(_editingValue, out var parsedValue))
        {
            var clampedValue = ClampValue(parsedValue);
            if (!clampedValue.Equals(Value))
            {
                Value = clampedValue;
            }
            // Always fire on blur to ensure parent is in sync
            ValueChanged.InvokeAsync(clampedValue);
            NotifyFieldChanged();
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
            NotifyFieldChanged();
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

    private async void DebounceValueChanged(TValue value)
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(DebounceInterval, _debounceCts.Token);

            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(value);
            }
        }
        catch (TaskCanceledException)
        {
            // Timer was cancelled — either by a new keystroke or disposal.
        }
    }

    public void Dispose()
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
