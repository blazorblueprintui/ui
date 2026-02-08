using BlazorBlueprint.Components.Converters;
using BlazorBlueprint.Components.Field;
using BlazorBlueprint.Components.Input;
using BlazorBlueprint.Components.InputField;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormField;

/// <summary>
/// An opinionated form field that composes <see cref="InputField{TValue}"/> with
/// <see cref="Field.Field"/>, <see cref="FieldLabel"/>, <see cref="FieldDescription"/>,
/// and <see cref="FieldError"/> for a complete, ready-to-use form field experience.
/// </summary>
/// <remarks>
/// <para>
/// FormField provides a higher-level abstraction over the primitive <see cref="InputField{TValue}"/>.
/// It automatically handles label association, helper text display, and error message rendering
/// based on the error kind from the underlying InputField.
/// </para>
/// <para>
/// For full control over layout and error display, use <see cref="InputField{TValue}"/> directly
/// with the <see cref="Field.Field"/> component system.
/// </para>
/// <para>
/// Features:
/// - Automatic label with proper for/id association
/// - Helper text that hides during error state
/// - Auto-generated contextual error messages based on error kind
/// - Manual error text override via <see cref="ErrorText"/>
/// - All <see cref="InputField{TValue}"/> parameters passed through
/// - Automatic ARIA attribute wiring (describedby, invalid)
/// </para>
/// </remarks>
/// <typeparam name="TValue">The value type to bind to.</typeparam>
/// <example>
/// <code>
/// &lt;FormField TValue="int"
///            @bind-Value="age"
///            Label="Age"
///            HelperText="Must be 18 or older"
///            Validation="v =&gt; v &gt;= 18" /&gt;
/// </code>
/// </example>
public partial class FormField<TValue> : ComponentBase, IDisposable
{
    private InputField<TValue>? _inputRef;
    private bool _hasError;
    private string? _errorMessage;
    private readonly string _inputId = $"formfield-{Guid.NewGuid():N}";

    // --- Form Field Parameters ---

    /// <summary>
    /// Gets or sets the label text displayed above the input.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the helper text displayed below the input.
    /// </summary>
    /// <remarks>
    /// Hidden when the field is in an error state; the error message takes its place.
    /// </remarks>
    [Parameter]
    public string? HelperText { get; set; }

    /// <summary>
    /// Gets or sets a manual error text override.
    /// </summary>
    /// <remarks>
    /// When set, this text is displayed instead of the auto-generated error message
    /// whenever the field enters an error state. Useful when you need domain-specific
    /// error messages rather than generic ones.
    /// </remarks>
    [Parameter]
    public string? ErrorText { get; set; }

    /// <summary>
    /// Gets or sets the orientation of the field layout.
    /// </summary>
    [Parameter]
    public FieldOrientation Orientation { get; set; } = FieldOrientation.Vertical;

    /// <summary>
    /// Gets or sets additional CSS classes applied to the outer Field container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes applied to the inner InputField element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

    // --- InputField Pass-Through Parameters ---

    /// <summary>
    /// Gets or sets the current typed value.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the typed value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an optional custom converter.
    /// </summary>
    [Parameter]
    public InputConverter<TValue>? Converter { get; set; }

    /// <summary>
    /// Gets or sets the display format string.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the type of input.
    /// </summary>
    [Parameter]
    public InputType Type { get; set; } = InputType.Text;

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
    /// Gets or sets a post-parse validation function.
    /// </summary>
    [Parameter]
    public Func<TValue, bool>? Validation { get; set; }

    /// <summary>
    /// Gets or sets a regex pattern for pre-parse validation.
    /// </summary>
    [Parameter]
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the input.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets when ValueChanged fires.
    /// </summary>
    [Parameter]
    public UpdateTiming UpdateTiming { get; set; } = UpdateTiming.Immediate;

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds.
    /// </summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 500;

    /// <summary>
    /// Gets or sets a callback invoked when a parse or validation error occurs.
    /// </summary>
    [Parameter]
    public EventCallback<InputParseException> OnParseError { get; set; }

    /// <summary>
    /// Gets or sets a callback invoked when the error state clears.
    /// </summary>
    [Parameter]
    public EventCallback OnErrorCleared { get; set; }

    /// <summary>
    /// Gets whether the form field currently has an error.
    /// </summary>
    public bool HasError => _hasError;

    /// <summary>
    /// Gets the underlying InputField component reference.
    /// </summary>
    public InputField<TValue>? InputFieldRef => _inputRef;

    private string _descriptionId => $"{_inputId}-description";
    private string _errorId => $"{_inputId}-error";

    private string? _describedById => _hasError
        ? _errorId
        : !string.IsNullOrEmpty(HelperText) ? _descriptionId : null;

    private async Task HandleValueChanged(TValue? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    private async Task HandleParseError(InputParseException ex)
    {
        _hasError = true;
        _errorMessage = ErrorText ?? GenerateErrorMessage(ex);

        if (OnParseError.HasDelegate)
        {
            await OnParseError.InvokeAsync(ex);
        }
    }

    private async Task HandleErrorCleared()
    {
        _hasError = false;
        _errorMessage = null;

        if (OnErrorCleared.HasDelegate)
        {
            await OnErrorCleared.InvokeAsync();
        }
    }

    private static string GenerateErrorMessage(InputParseException ex) => ex.ErrorKind switch
    {
        InputFieldErrorKind.Parse => $"'{ex.RawInput}' is not a valid {GetFriendlyTypeName(ex.TargetType)}.",
        InputFieldErrorKind.PatternValidation => $"'{ex.RawInput}' does not match the required format.",
        InputFieldErrorKind.ValueValidation => "The entered value is not valid.",
        _ => "Invalid input."
    };

    private static string GetFriendlyTypeName(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(int) || underlying == typeof(long))
            return "whole number";
        if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
            return "number";
        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset) || underlying == typeof(DateOnly))
            return "date";
        if (underlying == typeof(TimeOnly))
            return "time";
        if (underlying == typeof(Guid))
            return "GUID";
        if (underlying == typeof(bool))
            return "true/false value";

        return underlying.Name.ToLowerInvariant();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
