namespace BlazorBlueprint.Components.InputField;

/// <summary>
/// Represents an error that occurred while parsing user input to a target type
/// in an <see cref="InputField{TValue}"/> component.
/// </summary>
/// <remarks>
/// Raised via the <see cref="InputField{TValue}.OnParseError"/> callback when the user
/// blurs the input with a value that cannot be converted to the target type.
/// Contains the raw input string and target type for error reporting.
/// </remarks>
/// <example>
/// <code>
/// &lt;InputField TValue="int" @bind-Value="age" OnParseError="HandleError" /&gt;
///
/// @code {
///     private void HandleError(InputParseException ex)
///     {
///         Console.WriteLine($"Could not parse '{ex.RawInput}' as {ex.TargetType.Name}");
///     }
/// }
/// </code>
/// </example>
public class InputParseException : Exception
{
    /// <summary>
    /// Gets the raw string input that failed to parse.
    /// </summary>
    public string RawInput { get; }

    /// <summary>
    /// Gets the target type that the input could not be converted to.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputParseException"/> class.
    /// </summary>
    /// <param name="rawInput">The raw string input that failed to parse.</param>
    /// <param name="targetType">The target type that the input could not be converted to.</param>
    /// <param name="innerException">The exception that caused the parse failure.</param>
    public InputParseException(string rawInput, Type targetType, Exception innerException)
        : base($"Failed to parse '{rawInput}' as {targetType.Name}.", innerException)
    {
        RawInput = rawInput;
        TargetType = targetType;
    }
}
