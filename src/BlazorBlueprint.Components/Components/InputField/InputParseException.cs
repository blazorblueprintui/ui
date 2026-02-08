namespace BlazorBlueprint.Components.InputField;

/// <summary>
/// Represents an error that occurred while parsing or validating user input
/// in an <see cref="InputField{TValue}"/> component.
/// </summary>
/// <remarks>
/// Raised via the <see cref="InputField{TValue}.OnParseError"/> callback when the user
/// blurs the input with a value that cannot be converted or validated.
/// Contains the raw input string, target type, and error kind for contextual error reporting.
/// </remarks>
/// <example>
/// <code>
/// &lt;InputField TValue="int" @bind-Value="age" OnParseError="HandleError" /&gt;
///
/// @code {
///     private void HandleError(InputParseException ex)
///     {
///         var message = ex.ErrorKind switch
///         {
///             InputFieldErrorKind.Parse =&gt; $"'{ex.RawInput}' is not a valid {ex.TargetType.Name}.",
///             InputFieldErrorKind.PatternValidation =&gt; $"'{ex.RawInput}' does not match the required format.",
///             InputFieldErrorKind.ValueValidation =&gt; $"The value is not within the allowed range.",
///             _ =&gt; "Invalid input."
///         };
///     }
/// }
/// </code>
/// </example>
public class InputParseException : Exception
{
    /// <summary>
    /// Gets the raw string input that failed to parse or validate.
    /// </summary>
    public string RawInput { get; }

    /// <summary>
    /// Gets the target type that the input could not be converted to.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Gets the kind of error that occurred.
    /// </summary>
    public InputFieldErrorKind ErrorKind { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputParseException"/> class.
    /// </summary>
    /// <param name="rawInput">The raw string input that failed to parse or validate.</param>
    /// <param name="targetType">The target type that the input could not be converted to.</param>
    /// <param name="errorKind">The kind of error that occurred.</param>
    /// <param name="innerException">The exception that caused the failure.</param>
    public InputParseException(string rawInput, Type targetType, InputFieldErrorKind errorKind, Exception innerException)
        : base(FormatMessage(rawInput, targetType, errorKind), innerException)
    {
        RawInput = rawInput;
        TargetType = targetType;
        ErrorKind = errorKind;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputParseException"/> class
    /// with <see cref="InputFieldErrorKind.Parse"/> as the default error kind.
    /// </summary>
    /// <param name="rawInput">The raw string input that failed to parse.</param>
    /// <param name="targetType">The target type that the input could not be converted to.</param>
    /// <param name="innerException">The exception that caused the parse failure.</param>
    public InputParseException(string rawInput, Type targetType, Exception innerException)
        : this(rawInput, targetType, InputFieldErrorKind.Parse, innerException)
    {
    }

    private static string FormatMessage(string rawInput, Type targetType, InputFieldErrorKind errorKind) => errorKind switch
    {
        InputFieldErrorKind.Parse => $"Failed to parse '{rawInput}' as {targetType.Name}.",
        InputFieldErrorKind.PatternValidation => $"Input '{rawInput}' does not match the required pattern for {targetType.Name}.",
        InputFieldErrorKind.ValueValidation => $"Value parsed from '{rawInput}' failed validation for {targetType.Name}.",
        _ => $"Failed to parse '{rawInput}' as {targetType.Name}."
    };
}
