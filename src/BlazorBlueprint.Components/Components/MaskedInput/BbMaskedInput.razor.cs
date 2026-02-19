using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components;

/// <summary>
/// A masked input component with preset and custom mask patterns.
/// </summary>
public partial class BbMaskedInput : ComponentBase, IAsyncDisposable
{
    private ElementReference _inputRef;
    private MaskProcessor? _processor;
    private string _displayValue = string.Empty;
    private IJSObjectReference? _jsModule;
    private bool _jsModuleLoaded;
    private string? _generatedId;
    private readonly InputValidationBehavior validation = new();

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    [CascadingParameter(Name = "FieldIsInvalid")]
    private bool? FieldIsInvalid { get; set; }

    /// <summary>
    /// Gets or sets the unmasked value (raw input without formatting).
    /// </summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback invoked when the unmasked value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the mask preset.
    /// </summary>
    [Parameter]
    public MaskPreset Preset { get; set; } = MaskPreset.Custom;

    /// <summary>
    /// Gets or sets the custom mask pattern (when Preset is Custom).
    /// Mask characters: 9 = digit, A = letter, * = alphanumeric.
    /// </summary>
    [Parameter]
    public string? Mask { get; set; }

    /// <summary>
    /// Gets or sets the placeholder character for unfilled positions.
    /// </summary>
    [Parameter]
    public char PlaceholderChar { get; set; } = '_';

    /// <summary>
    /// Gets or sets whether to show the mask with placeholders.
    /// </summary>
    [Parameter]
    public bool ShowMask { get; set; } = true;

    /// <summary>
    /// Gets or sets the placeholder text (shown when empty and not focused).
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
    public Expression<Func<string>>? ValueExpression { get; set; }

    private string? EffectiveAriaInvalid => validation.GetEffectiveAriaInvalid(AriaInvalid, FieldIsInvalid);

    /// <summary>
    /// Gets the effective name attribute, falling back to the FieldIdentifier name when inside an EditForm.
    /// </summary>
    private string? EffectiveName => validation.GetEffectiveName(Name);

    private string EffectiveId => Id ?? (_generatedId ??= $"masked-{Guid.NewGuid().ToString("N")[..8]}");

    /// <summary>
    /// Gets the masked (formatted) value.
    /// </summary>
    public string MaskedValue => _displayValue;

    /// <summary>
    /// Gets the unmasked value.
    /// </summary>
    public string UnmaskedValue => Processor.GetUnmaskedValue(_displayValue);

    private MaskProcessor Processor
    {
        get
        {
            var effectiveMask = GetEffectiveMask();
            if (_processor == null || _processor.Mask != effectiveMask || _processor.PlaceholderChar != PlaceholderChar)
            {
                _processor = new MaskProcessor(effectiveMask, PlaceholderChar);
            }
            return _processor;
        }
    }

    private string GetEffectiveMask()
    {
        if (Preset != MaskPreset.Custom)
        {
            return MaskDefinitions.GetPattern(Preset);
        }

        return Mask ?? string.Empty;
    }

    private string InputMode => MaskDefinitions.GetInputMode(Preset);

    private int MaskLength => GetEffectiveMask().Length;

    private string EffectivePlaceholder
    {
        get
        {
            if (!string.IsNullOrEmpty(Placeholder))
            {
                return Placeholder;
            }

            if (Preset != MaskPreset.Custom)
            {
                return MaskDefinitions.GetPlaceholder(Preset);
            }

            return Processor.GetEmptyMask();
        }
    }

    protected override void OnInitialized() =>
        UpdateDisplayValue();

    protected override void OnParametersSet()
    {
        UpdateDisplayValue();

        validation.Update(CascadedEditContext, ValueExpression);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/masked-input.js");
                _jsModuleLoaded = true;
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

    private void UpdateDisplayValue() =>
        _displayValue = Processor.ApplyMask(Value);

    private void HandleInput(ChangeEventArgs args)
    {
        var inputValue = args.Value?.ToString() ?? string.Empty;
        var mask = GetEffectiveMask();

        // Count max editable positions in mask
        var maxEditablePositions = 0;
        foreach (var c in mask)
        {
            if (MaskProcessor.IsMaskChar(c))
            {
                maxEditablePositions++;
            }
        }

        // Extract only valid characters from input (letters and digits only)
        var rawInput = new System.Text.StringBuilder();

        foreach (var c in inputValue)
        {
            // Skip placeholder characters
            if (c == PlaceholderChar)
            {
                continue;
            }

            // Only collect alphanumeric characters
            if (char.IsLetterOrDigit(c))
            {
                rawInput.Append(c);
            }

            // Stop if we have enough characters
            if (rawInput.Length >= maxEditablePositions)
            {
                break;
            }
        }

        var rawInputStr = rawInput.ToString();

        // Validate each character against its corresponding mask position
        var validatedInput = new System.Text.StringBuilder();
        var rawIndex = 0;
        var maskIndex = 0;

        while (rawIndex < rawInputStr.Length && maskIndex < mask.Length)
        {
            // Find next editable mask position
            while (maskIndex < mask.Length && !MaskProcessor.IsMaskChar(mask[maskIndex]))
            {
                maskIndex++;
            }

            if (maskIndex >= mask.Length)
            {
                break;
            }

            var maskChar = mask[maskIndex];
            var inputChar = rawInputStr[rawIndex];

            var isValid = maskChar switch
            {
                '9' => char.IsDigit(inputChar),
                'A' => char.IsLetter(inputChar),
                '*' => char.IsLetterOrDigit(inputChar),
                _ => false
            };

            if (isValid)
            {
                validatedInput.Append(inputChar);
                maskIndex++;
            }
            rawIndex++;
        }

        var validatedStr = validatedInput.ToString();

        // Apply mask to get display value
        var newDisplayValue = Processor.ApplyMask(validatedStr);

        // Calculate cursor position
        var cursorPosition = CalculateCursorPosition(validatedStr.Length);

        // Update state
        _displayValue = newDisplayValue;

        // Update the bound Value
        var unmasked = Processor.GetUnmaskedValue(_displayValue);
        if (unmasked != Value)
        {
            Value = unmasked;
            _ = ValueChanged.InvokeAsync(unmasked);

            validation.NotifyFieldChanged();
        }

        // Use JS to set value and cursor atomically to prevent flashing
        _ = SetInputValueAsync(newDisplayValue, cursorPosition);
    }

    private async Task SetInputValueAsync(string value, int cursorPosition)
    {
        if (_jsModuleLoaded && _jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("setInputValue", _inputRef, value, cursorPosition);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
        }
    }

    private int CalculateCursorPosition(int rawInputLength)
    {
        var mask = GetEffectiveMask();
        var position = 0;
        var inputCount = 0;

        for (var i = 0; i < mask.Length && inputCount < rawInputLength; i++)
        {
            position = i + 1;
            if (MaskProcessor.IsMaskChar(mask[i]))
            {
                inputCount++;
            }
        }

        // If we've filled some characters, skip any following literals
        if (rawInputLength > 0 && position < mask.Length)
        {
            while (position < mask.Length && !MaskProcessor.IsMaskChar(mask[position]))
            {
                position++;
            }
        }

        return position;
    }

    private async Task HandleFocus(FocusEventArgs args)
    {
        // Show the mask if empty
        if (string.IsNullOrEmpty(Value) && ShowMask)
        {
            _displayValue = Processor.GetEmptyMask();

            // Use JS to set the value and position cursor at start
            if (_jsModuleLoaded && _jsModule != null)
            {
                try
                {
                    // Find the first editable position
                    var firstEditablePos = Processor.GetNextEditablePosition(0);
                    if (firstEditablePos < 0)
                    {
                        firstEditablePos = 0;
                    }

                    await _jsModule.InvokeVoidAsync("setInputValue", _inputRef, _displayValue, firstEditablePos);
                }
                catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
                {
                    // Expected during circuit disconnect
                }
            }
            else
            {
                StateHasChanged();
            }
        }
    }

    private void HandleBlur(FocusEventArgs args)
    {
        // Clear display if value is empty and not showing mask
        if (string.IsNullOrEmpty(Value) && !ShowMask)
        {
            _displayValue = string.Empty;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (_jsModule != null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
        }
    }

    private string CssClass => ClassNames.cn(
        "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-base",
        "placeholder:text-muted-foreground",
        "focus-visible:outline-none",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "aria-[invalid=true]:border-destructive",
        "transition-colors",
        "md:text-sm",
        "font-mono tracking-wider",
        Class
    );
}
