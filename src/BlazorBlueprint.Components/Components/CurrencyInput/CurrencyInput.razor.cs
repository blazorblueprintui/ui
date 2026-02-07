using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace BlazorBlueprint.Components.CurrencyInput;

/// <summary>
/// A currency input component with locale-aware formatting.
/// </summary>
public partial class CurrencyInput : ComponentBase
{
    private ElementReference _inputRef;
    private string _editingValue = string.Empty;
    private bool _isEditing;
    private CurrencyDefinition? _currency;
    private CultureInfo? _cultureInfo;

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    [Parameter]
    public decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<decimal> ValueChanged { get; set; }

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
    /// Gets or sets whether to use thousand separators in display mode.
    /// </summary>
    [Parameter]
    public bool UseThousandSeparator { get; set; } = true;

    private CurrencyDefinition Currency => _currency ??= CurrencyCatalog.GetCurrency(CurrencyCode);

    private CultureInfo CultureInfo
    {
        get
        {
            if (_cultureInfo == null || _cultureInfo.Name != Currency.CultureName)
            {
                try
                {
                    _cultureInfo = new CultureInfo(Currency.CultureName);
                }
                catch
                {
                    _cultureInfo = CultureInfo.InvariantCulture;
                }
            }
            return _cultureInfo;
        }
    }

    protected override void OnParametersSet()
    {
        // Reset currency cache if currency code changed
        if (_currency != null && !string.Equals(_currency.Code, CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            _currency = null;
            _cultureInfo = null;
        }
    }

    private string DisplayValue
    {
        get
        {
            if (_isEditing)
            {
                return _editingValue;
            }

            return FormatCurrency(Value);
        }
    }

    private string FormatCurrency(decimal value)
    {
        var format = UseThousandSeparator ? "N" : "F";
        return value.ToString($"{format}{Currency.DecimalPlaces}", CultureInfo);
    }

    private string ContainerClass => ClassNames.cn(
        "flex items-center rounded-md overflow-hidden",
        "focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2 focus-within:ring-offset-background",
        Disabled ? "opacity-50" : null
    );

    private string CssClass => ClassNames.cn(
        "flex h-10 w-full border border-input bg-background px-3 py-2 text-base",
        "placeholder:text-muted-foreground",
        "focus-visible:outline-none",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "aria-[invalid=true]:border-destructive",
        "transition-colors",
        "md:text-sm",
        "text-right tabular-nums",
        ShowSymbol && Currency.SymbolBefore ? "border-l-0" : "rounded-l-md",
        ShowSymbol && !Currency.SymbolBefore ? "border-r-0" : "rounded-r-md",
        Class
    );

    private string SymbolClass => ClassNames.cn(
        "flex h-10 items-center justify-center px-3 border border-input bg-muted text-muted-foreground text-sm",
        Currency.SymbolBefore ? "border-r-0" : "border-l-0",
        Disabled ? "opacity-50" : null
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
            if (clampedValue != Value)
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
            if (clampedValue != Value)
            {
                Value = clampedValue;
                ValueChanged.InvokeAsync(clampedValue);
            }
        }

        StateHasChanged();
    }

    private void HandleFocus(FocusEventArgs args)
    {
        // Show raw number without formatting for easier editing
        _editingValue = Value.ToString($"F{Currency.DecimalPlaces}", CultureInfo.InvariantCulture);
        _isEditing = true;
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Disabled)
        {
            return;
        }

        var step = (decimal)Math.Pow(10, -Currency.DecimalPlaces);

        switch (e.Key)
        {
            case "ArrowUp":
                await SetValue(Value + step);
                break;
            case "ArrowDown":
                await SetValue(Value - step);
                break;
        }
    }

    private async Task SetValue(decimal value)
    {
        var clampedValue = ClampValue(value);

        if (clampedValue != Value)
        {
            Value = clampedValue;
            _editingValue = clampedValue.ToString($"F{Currency.DecimalPlaces}", CultureInfo.InvariantCulture);
            await ValueChanged.InvokeAsync(clampedValue);
        }
    }

    private decimal ClampValue(decimal value)
    {
        // Round to currency's decimal places
        value = Math.Round(value, Currency.DecimalPlaces);

        if (!AllowNegative && value < 0)
        {
            value = 0;
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

    private bool TryParseValue(string? input, out decimal result)
    {
        result = 0;

        if (string.IsNullOrWhiteSpace(input))
        {
            return true;
        }

        // Remove currency symbols, thousand separators, and whitespace
        input = input
            .Replace(Currency.Symbol, "")
            .Replace(" ", "")
            .Trim();

        // Handle locale-specific decimal separators
        var decimalSeparator = CultureInfo.NumberFormat.NumberDecimalSeparator;
        var groupSeparator = CultureInfo.NumberFormat.NumberGroupSeparator;

        // Remove thousand separators
        input = input.Replace(groupSeparator, "");

        // Normalize decimal separator to invariant culture
        if (decimalSeparator != ".")
        {
            input = input.Replace(decimalSeparator, ".");
        }

        // Handle negative sign
        if (!AllowNegative && input.StartsWith('-'))
        {
            return false;
        }

        return decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }
}
