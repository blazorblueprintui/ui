using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Globalization;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components;

/// <summary>
/// A currency input component with locale-aware formatting.
/// </summary>
public partial class BbCurrencyInput : ComponentBase
{
    private ElementReference inputRef;
    private IJSObjectReference? jsModule;
    private DotNetObjectReference<BbCurrencyInput>? dotNetRef;
    private string instanceId = Guid.NewGuid().ToString("N");
    private string? generatedId;
    private bool jsInitialized;
    private bool disposed;
    private string editingValue = string.Empty;
    private bool isEditing;
    private CurrencyDefinition? currency;
    private CultureInfo? cultureInfo;
    private readonly InputValidationBehavior validation = new();

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    [CascadingParameter(Name = "FieldIsInvalid")]
    private bool? FieldIsInvalid { get; set; }

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
    public Expression<Func<decimal>>? ValueExpression { get; set; }

    private string? EffectiveAriaInvalid => validation.GetEffectiveAriaInvalid(AriaInvalid, FieldIsInvalid);

    /// <summary>
    /// Gets the effective name attribute, falling back to the FieldIdentifier name when inside an EditForm.
    /// </summary>
    private string? EffectiveName => validation.GetEffectiveName(Name);

    private string EffectiveId => Id ?? (generatedId ??= $"currency-{Guid.NewGuid().ToString("N")[..8]}");

    /// <summary>
    /// Gets or sets whether to use thousand separators in display mode.
    /// </summary>
    [Parameter]
    public bool UseThousandSeparator { get; set; } = true;

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

    private CurrencyDefinition Currency => currency ??= CurrencyCatalog.GetCurrency(CurrencyCode);

    private CultureInfo CultureInfo
    {
        get
        {
            if (cultureInfo == null || cultureInfo.Name != Currency.CultureName)
            {
                try
                {
                    cultureInfo = new CultureInfo(Currency.CultureName);
                }
                catch
                {
                    cultureInfo = CultureInfo.InvariantCulture;
                }
            }
            return cultureInfo;
        }
    }

    protected override void OnParametersSet()
    {
        // Reset currency cache if currency code changed
        if (currency != null && !string.Equals(currency.Code, CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            currency = null;
            cultureInfo = null;
        }

        validation.Update(CascadedEditContext, ValueExpression);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/numeric-input.js");
                dotNetRef = DotNetObjectReference.Create(this);
                await jsModule.InvokeVoidAsync("initialize", inputRef, dotNetRef, instanceId, GetJsConfig());
                jsInitialized = true;
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
            }
        }
    }

    /// <summary>
    /// Builds the JS configuration object from current parameters.
    /// </summary>
    private object GetJsConfig() => new
    {
        disableDebounce = DisableDebounce,
        debounceMs = DebounceInterval,
        stepKeys = new[] { "ArrowUp", "ArrowDown" }
    };

    private void NotifyFieldChanged() => validation.NotifyFieldChanged();

    private string DisplayValue
    {
        get
        {
            if (isEditing)
            {
                return editingValue;
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

    /// <summary>
    /// Called from JavaScript during typing. JS has already handled debounce if enabled.
    /// </summary>
    [JSInvokable]
    public async Task JsOnInput(string? value)
    {
        if (disposed) { return; }

        var inputValue = value ?? string.Empty;
        editingValue = inputValue;
        isEditing = true;

        if (TryParseValue(inputValue, out var parsedValue))
        {
            var clampedValue = ClampValue(parsedValue);
            if (clampedValue != Value)
            {
                Value = clampedValue;
                await ValueChanged.InvokeAsync(clampedValue);
                NotifyFieldChanged();
            }
        }

        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript on blur. Commits the current value.
    /// </summary>
    [JSInvokable]
    public async Task JsOnBlur(string? value)
    {
        if (disposed) { return; }

        var inputValue = value ?? string.Empty;
        isEditing = false;

        if (TryParseValue(inputValue, out var parsedValue))
        {
            var clampedValue = ClampValue(parsedValue);
            if (clampedValue != Value)
            {
                Value = clampedValue;
            }
            // Always fire on blur to ensure parent is in sync
            await ValueChanged.InvokeAsync(clampedValue);
            NotifyFieldChanged();
        }

        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript on focus.
    /// </summary>
    [JSInvokable]
    public void JsOnFocus()
    {
        if (disposed) { return; }

        // Show raw number without formatting for easier editing
        editingValue = Value.ToString($"F{Currency.DecimalPlaces}", CultureInfo.InvariantCulture);
        isEditing = true;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript when a step key is pressed (ArrowUp/Down).
    /// JS has already called preventDefault().
    /// </summary>
    [JSInvokable]
    public async Task JsOnKeyDown(string key)
    {
        if (disposed || Disabled) { return; }

        var step = (decimal)Math.Pow(10, -Currency.DecimalPlaces);

        switch (key)
        {
            case "ArrowUp":
                await SetValue(Value + step);
                break;
            case "ArrowDown":
                await SetValue(Value - step);
                break;
        }

        StateHasChanged();
    }

    private async Task SetValue(decimal value)
    {
        var clampedValue = ClampValue(value);

        if (clampedValue != Value)
        {
            Value = clampedValue;
            editingValue = clampedValue.ToString($"F{Currency.DecimalPlaces}", CultureInfo.InvariantCulture);
            await ValueChanged.InvokeAsync(clampedValue);
            NotifyFieldChanged();
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

    public async ValueTask DisposeAsync()
    {
        disposed = true;

        if (jsModule != null && jsInitialized)
        {
            try
            {
                await jsModule.InvokeVoidAsync("dispose", instanceId);
                await jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }
        }

        dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }
}
