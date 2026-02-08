namespace BlazorBlueprint.Components.InputField;

/// <summary>
/// Identifies the kind of error that occurred in an <see cref="InputField{TValue}"/> component.
/// </summary>
public enum InputFieldErrorKind
{
    /// <summary>
    /// The raw input string could not be parsed/converted to the target type.
    /// </summary>
    Parse,

    /// <summary>
    /// The raw input string did not match the <see cref="InputField{TValue}.ValidationPattern"/> regex.
    /// </summary>
    PatternValidation,

    /// <summary>
    /// The parsed value was rejected by the <see cref="InputField{TValue}.Validation"/> function.
    /// </summary>
    ValueValidation
}
