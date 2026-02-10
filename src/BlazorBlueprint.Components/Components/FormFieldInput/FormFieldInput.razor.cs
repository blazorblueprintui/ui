using System.Linq.Expressions;
using BlazorBlueprint.Components.Converters;
using BlazorBlueprint.Components.Field;
using BlazorBlueprint.Components.Input;
using BlazorBlueprint.Components.InputField;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.FormFieldInput;

/// <summary>
/// An opinionated form field that composes <see cref="InputField{TValue}"/> with
/// <see cref="Field.Field"/>, <see cref="Field.FieldLabel"/>, <see cref="Field.FieldDescription"/>,
/// and <see cref="Field.FieldError"/> for a complete, ready-to-use form field experience.
/// </summary>
/// <remarks>
/// <para>
/// FormFieldInput provides a higher-level abstraction over the primitive <see cref="InputField{TValue}"/>.
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
/// - Manual error text override via <see cref="FormFieldBase.ErrorText"/>
/// - All <see cref="InputField{TValue}"/> parameters passed through
/// - Automatic ARIA attribute wiring (describedby, invalid)
/// </para>
/// </remarks>
/// <typeparam name="TValue">The value type to bind to.</typeparam>
/// <example>
/// <code>
/// &lt;FormFieldInput TValue="int"
///            @bind-Value="age"
///            Label="Age"
///            HelperText="Must be 18 or older"
///            Validation="v =&gt; v &gt;= 18" /&gt;
/// </code>
/// </example>
public partial class FormFieldInput<TValue> : FormFieldBase
{
    private InputField<TValue>? _inputRef;
    private bool _hasError;
    private string? _errorMessage;

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
    /// Gets or sets additional CSS classes applied to the inner InputField element.
    /// </summary>
    [Parameter]
    public string? InputClass { get; set; }

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
    /// Gets or sets the expression identifying the bound value for EditForm integration.
    /// Automatically provided by <c>@bind-Value</c>. Passed through to the inner InputField.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the HTML name attribute. Passed through to the inner InputField.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets whether the form field currently has an error.
    /// </summary>
    public bool HasError => _hasError;

    /// <summary>
    /// Gets the underlying InputField component reference.
    /// </summary>
    public InputField<TValue>? InputFieldRef => _inputRef;

    /// <inheritdoc />
    protected override bool IsInvalid => _hasError || HasEditContextErrors;

    /// <inheritdoc />
    protected override string? DescribedById => _hasError || HasEditContextErrors
        ? ErrorId
        : !string.IsNullOrEmpty(HelperText) ? DescriptionId : null;

    /// <inheritdoc />
    protected override LambdaExpression? GetFieldExpression() => ValueExpression;

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

        if (underlying == typeof(string))
        {
            return "text value";
        }
        if (underlying == typeof(int) || underlying == typeof(long))
        {
            return "whole number";
        }
        if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
        {
            return "number";
        }
        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset) || underlying == typeof(DateOnly))
        {
            return "date";
        }
        if (underlying == typeof(TimeOnly))
        {
            return "time";
        }
        if (underlying == typeof(Guid))
        {
            return "GUID";
        }
        if (underlying == typeof(bool))
        {
            return "true/false value";
        }

        return underlying.Name.ToLowerInvariant();
    }
}
