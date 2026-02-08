using System.Text.RegularExpressions;
using BlazorBlueprint.Components.Converters;
using BlazorBlueprint.Components.Input;
using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Components.InputField;

/// <summary>
/// A generic typed input component that supports two-way binding with automatic type conversion.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="Input.Input"/> which binds only to <c>string</c>, <c>InputField&lt;TValue&gt;</c>
/// provides typed two-way binding via <see cref="InputConverter{TValue}"/>. It supports formatting,
/// parsing, validation, and error handling for any type with a registered converter.
/// </para>
/// <para>
/// Features:
/// - Typed two-way binding (int, decimal, DateTime, Guid, custom types, etc.)
/// - Converter system with global, instance, and built-in default resolution
/// - Display format support via <see cref="Format"/> parameter
/// - Editing/display toggle pattern (raw value while focused, formatted while blurred)
/// - Parse error reporting via <see cref="OnParseError"/> and <see cref="HasParseError"/>
/// - Error kind discrimination via <see cref="CurrentErrorKind"/> and <see cref="InputFieldErrorKind"/>
/// - Error cleared notification via <see cref="OnErrorCleared"/>
/// - Invalid text preservation on blur (user sees what they typed wrong)
/// - Pre-parse regex validation via <see cref="ValidationPattern"/>
/// - Post-parse value validation via <see cref="Validation"/>
/// - Full ARIA attribute support with automatic aria-invalid on parse errors
/// - Same visual appearance as <see cref="Input.Input"/>
/// </para>
/// </remarks>
/// <typeparam name="TValue">The value type to bind to. No constraint — works with value types, reference types, and nullables.</typeparam>
/// <example>
/// <code>
/// &lt;InputField TValue="int" @bind-Value="age" Placeholder="Enter age" /&gt;
///
/// &lt;InputField TValue="DateTime?" @bind-Value="birthDate" Format="yyyy-MM-dd" Type="InputType.Date" /&gt;
///
/// &lt;InputField TValue="decimal" @bind-Value="price" OnParseError="HandleError" /&gt;
/// </code>
/// </example>
public partial class InputField<TValue> : ComponentBase, IDisposable
{
    private string _editingValue = string.Empty;
    private bool _isEditing;
    private bool _hasParseError;
    private InputFieldErrorKind? _currentErrorKind;
    private CancellationTokenSource? _debounceCts;

    /// <summary>
    /// Gets or sets the current typed value.
    /// </summary>
    /// <remarks>
    /// Supports two-way binding via @bind-Value syntax.
    /// The value is converted to/from string using the converter resolution chain.
    /// </remarks>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the typed value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an optional custom converter for this component instance.
    /// </summary>
    /// <remarks>
    /// When provided, the converter's <see cref="InputConverter{TValue}.GetFunc"/> and
    /// <see cref="InputConverter{TValue}.SetFunc"/> take highest priority in the resolution chain.
    /// </remarks>
    [Parameter]
    public InputConverter<TValue>? Converter { get; set; }

    /// <summary>
    /// Gets or sets the display format string.
    /// </summary>
    /// <remarks>
    /// When set, uses <see cref="IFormattable.ToString(string, IFormatProvider)"/> for display
    /// formatting (e.g., "yyyy-MM-dd" for dates, "N2" for numbers). Format only affects display;
    /// parsing always uses the converter's <c>Get</c> function.
    /// </remarks>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when a parse or validation error occurs on blur.
    /// </summary>
    /// <remarks>
    /// Fires when the user leaves the input with a value that cannot be converted to
    /// <typeparamref name="TValue"/> or fails validation. During typing, errors are silently ignored.
    /// The <see cref="InputParseException.ErrorKind"/> property indicates the specific failure type.
    /// </remarks>
    [Parameter]
    public EventCallback<InputParseException> OnParseError { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the error state clears.
    /// </summary>
    /// <remarks>
    /// Fires when the input transitions from an error state back to a valid state,
    /// either because the user entered a valid value during typing or cleared the input.
    /// Use this to clear error messages in the parent component.
    /// </remarks>
    [Parameter]
    public EventCallback OnErrorCleared { get; set; }

    /// <summary>
    /// Gets whether the input currently has a parse or validation error.
    /// </summary>
    /// <remarks>
    /// Set to <c>true</c> on blur when parsing or validation fails. Auto-clears when a valid
    /// value is entered. Can be used by consumers to conditionally display error messages.
    /// </remarks>
    public bool HasParseError => _hasParseError;

    /// <summary>
    /// Gets the kind of error currently active, or <c>null</c> if no error.
    /// </summary>
    /// <remarks>
    /// Provides finer-grained error information than <see cref="HasParseError"/>.
    /// Returns <see cref="InputFieldErrorKind.Parse"/> for conversion failures,
    /// <see cref="InputFieldErrorKind.PatternValidation"/> for regex failures,
    /// and <see cref="InputFieldErrorKind.ValueValidation"/> for post-parse validation failures.
    /// </remarks>
    public InputFieldErrorKind? CurrentErrorKind => _currentErrorKind;

    /// <summary>
    /// Gets or sets a post-parse validation function.
    /// </summary>
    /// <remarks>
    /// Called after successful parsing to validate the typed value.
    /// Return <c>true</c> if the value is valid, <c>false</c> to reject it.
    /// </remarks>
    [Parameter]
    public Func<TValue, bool>? Validation { get; set; }

    /// <summary>
    /// Gets or sets a regex pattern for pre-parse validation on the raw string.
    /// </summary>
    /// <remarks>
    /// When set, the raw input string is validated against this pattern before parsing.
    /// Uses <see cref="Regex.IsMatch(string, string)"/>.
    /// </remarks>
    [Parameter]
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Gets or sets the type of input.
    /// </summary>
    /// <remarks>
    /// Determines the HTML input type attribute. Default is <see cref="InputType.Text"/>.
    /// </remarks>
    [Parameter]
    public InputType Type { get; set; } = InputType.Text;

    /// <summary>
    /// Gets or sets the placeholder text displayed when the input is empty.
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
    /// Gets or sets additional CSS classes to apply to the input.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute for the input element.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the input.
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
    /// <remarks>
    /// When true, aria-invalid="true" is set. This is combined with <see cref="HasParseError"/>
    /// so that the destructive border CSS is applied when either is true.
    /// </remarks>
    [Parameter]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    /// Gets or sets when <see cref="ValueChanged"/> fires.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="UpdateTiming.Immediate"/> — every keystroke (default).</item>
    /// <item><see cref="UpdateTiming.OnChange"/> — only on blur.</item>
    /// <item><see cref="UpdateTiming.Debounced"/> — after typing pauses for <see cref="DebounceInterval"/> ms.</item>
    /// </list>
    /// </remarks>
    [Parameter]
    public UpdateTiming UpdateTiming { get; set; } = UpdateTiming.Immediate;

    /// <summary>
    /// Gets or sets the debounce delay in milliseconds when <see cref="UpdateTiming"/> is <see cref="UpdateTiming.Debounced"/>.
    /// </summary>
    [Parameter]
    public int DebounceInterval { get; set; } = 500;

    private InputConverter<TValue> ResolvedConverter => Converter ?? new InputConverter<TValue>();

    private bool? ComputedAriaInvalid => (AriaInvalid == true || _hasParseError) ? true : AriaInvalid;

    private string DisplayValue
    {
        get
        {
            if (_isEditing)
            {
                return _editingValue;
            }

            // Preserve the invalid text so the user can see what they typed wrong
            if (_hasParseError)
            {
                return _editingValue;
            }

            if (Value is null)
            {
                return string.Empty;
            }

            if (Format is not null)
            {
                return ResolvedConverter.SetWithFormat(Value, Format) ?? string.Empty;
            }

            return ResolvedConverter.Set(Value) ?? string.Empty;
        }
    }

    private string CssClass => ClassNames.cn(
        "flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-base",
        "ring-offset-background",
        "file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground",
        "placeholder:text-muted-foreground",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:cursor-not-allowed disabled:opacity-50",
        "aria-[invalid=true]:border-destructive aria-[invalid=true]:ring-destructive",
        "transition-colors",
        "md:text-sm",
        Class
    );

    private string HtmlType => Type switch
    {
        InputType.Text => "text",
        InputType.Email => "email",
        InputType.Password => "password",
        InputType.Number => "number",
        InputType.Tel => "tel",
        InputType.Url => "url",
        InputType.Search => "search",
        InputType.Date => "date",
        InputType.Time => "time",
        InputType.File => "file",
        _ => "text"
    };

    private void HandleInput(ChangeEventArgs args)
    {
        var inputValue = args.Value?.ToString() ?? string.Empty;
        _editingValue = inputValue;
        _isEditing = true;

        // In OnChange mode, only update display — defer parsing and ValueChanged to blur.
        if (UpdateTiming == UpdateTiming.OnChange)
        {
            return;
        }

        // Try to parse in real-time; silently ignore errors during typing
        try
        {
            if (string.IsNullOrEmpty(inputValue))
            {
                var defaultValue = default(TValue);
                if (!EqualityComparer<TValue?>.Default.Equals(Value, defaultValue))
                {
                    Value = defaultValue;

                    if (UpdateTiming == UpdateTiming.Immediate)
                    {
                        ValueChanged.InvokeAsync(defaultValue);
                    }
                    else if (UpdateTiming == UpdateTiming.Debounced)
                    {
                        DebounceValueChanged(defaultValue);
                    }
                }

                ClearErrorState();
                return;
            }

            if (ValidationPattern is not null && !Regex.IsMatch(inputValue, ValidationPattern))
            {
                return;
            }

            var parsed = ResolvedConverter.Get(inputValue);

            if (Validation is not null && !Validation(parsed))
            {
                return;
            }

            if (!EqualityComparer<TValue?>.Default.Equals(Value, parsed))
            {
                Value = parsed;

                if (UpdateTiming == UpdateTiming.Immediate)
                {
                    ValueChanged.InvokeAsync(parsed);
                }
                else if (UpdateTiming == UpdateTiming.Debounced)
                {
                    DebounceValueChanged(parsed);
                }
            }

            ClearErrorState();
        }
        catch
        {
            // Silently ignore parse errors during typing
        }
    }

    private void HandleFocus(FocusEventArgs args)
    {
        if (_hasParseError)
        {
            // Preserve the invalid text so the user can correct it
            _isEditing = true;
            return;
        }

        if (Value is null)
        {
            _editingValue = string.Empty;
        }
        else
        {
            // Show unformatted value while editing
            _editingValue = ResolvedConverter.Set(Value) ?? string.Empty;
        }

        _isEditing = true;
    }

    private async Task HandleBlur(FocusEventArgs args)
    {
        // Cancel any pending debounce — blur takes priority.
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = null;

        _isEditing = false;

        if (string.IsNullOrEmpty(_editingValue))
        {
            var defaultValue = default(TValue);
            if (!EqualityComparer<TValue?>.Default.Equals(Value, defaultValue))
            {
                Value = defaultValue;
                await ValueChanged.InvokeAsync(defaultValue);
            }

            ClearErrorState();
            return;
        }

        try
        {
            if (ValidationPattern is not null && !Regex.IsMatch(_editingValue, ValidationPattern))
            {
                throw new InputFieldValidationException(InputFieldErrorKind.PatternValidation,
                    $"Input '{_editingValue}' does not match validation pattern.");
            }

            var parsed = ResolvedConverter.Get(_editingValue);

            if (Validation is not null && !Validation(parsed))
            {
                throw new InputFieldValidationException(InputFieldErrorKind.ValueValidation,
                    $"Value failed validation.");
            }

            if (!EqualityComparer<TValue?>.Default.Equals(Value, parsed))
            {
                Value = parsed;
                await ValueChanged.InvokeAsync(parsed);
            }

            ClearErrorState();
        }
        catch (InputFieldValidationException ex)
        {
            await SetErrorState(ex.ErrorKind, ex);
        }
        catch (Exception ex)
        {
            await SetErrorState(InputFieldErrorKind.Parse, ex);
        }
    }

    /// <summary>
    /// Clears the error state and fires <see cref="OnErrorCleared"/> if transitioning from error to valid.
    /// </summary>
    private void ClearErrorState()
    {
        if (_hasParseError)
        {
            _hasParseError = false;
            _currentErrorKind = null;

            if (OnErrorCleared.HasDelegate)
            {
                OnErrorCleared.InvokeAsync();
            }
        }
    }

    /// <summary>
    /// Sets the error state and fires <see cref="OnParseError"/>.
    /// </summary>
    private async Task SetErrorState(InputFieldErrorKind errorKind, Exception ex)
    {
        _hasParseError = true;
        _currentErrorKind = errorKind;

        if (OnParseError.HasDelegate)
        {
            var parseException = new InputParseException(_editingValue, typeof(TValue), errorKind, ex);
            await OnParseError.InvokeAsync(parseException);
        }
    }

    /// <summary>
    /// Starts (or restarts) a debounce timer that fires <see cref="ValueChanged"/> after <see cref="DebounceInterval"/> ms.
    /// </summary>
    private async void DebounceValueChanged(TValue? value)
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

    /// <summary>
    /// Internal exception used to distinguish validation failures from parse failures in the catch block.
    /// </summary>
    private sealed class InputFieldValidationException : Exception
    {
        public InputFieldErrorKind ErrorKind { get; }

        public InputFieldValidationException(InputFieldErrorKind errorKind, string message)
            : base(message)
        {
            ErrorKind = errorKind;
        }
    }
}
