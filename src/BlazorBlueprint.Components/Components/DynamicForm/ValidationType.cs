namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the type of validation to apply to a dynamic form field.
/// </summary>
public enum ValidationType
{
    /// <summary>Field must have a non-empty value.</summary>
    Required,

    /// <summary>String value must meet a minimum length.</summary>
    MinLength,

    /// <summary>String value must not exceed a maximum length.</summary>
    MaxLength,

    /// <summary>Numeric value must be at or above a minimum.</summary>
    Min,

    /// <summary>Numeric value must be at or below a maximum.</summary>
    Max,

    /// <summary>String value must match a regular expression pattern.</summary>
    Pattern,

    /// <summary>String value must be a valid email address.</summary>
    Email,

    /// <summary>String value must be a valid URL.</summary>
    Url,

    /// <summary>String value must be a valid phone number.</summary>
    Phone,

    /// <summary>Custom validation using a delegate provided via metadata.</summary>
    Custom
}

/// <summary>
/// A validation rule applied to a dynamic form field.
/// </summary>
public class FieldValidation
{
    /// <summary>
    /// Gets or sets the type of validation.
    /// </summary>
    public ValidationType Type { get; set; }

    /// <summary>
    /// Gets or sets the validation parameter value (e.g., max length number, regex pattern string).
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets a custom error message. When null, a default message is generated.
    /// </summary>
    public string? Message { get; set; }
}
