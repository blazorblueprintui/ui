namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the data type of a filter field, which determines available operators and value input.
/// </summary>
public enum FilterFieldType
{
    /// <summary>
    /// Text field. Supports operators like Contains, StartsWith, EndsWith.
    /// </summary>
    String,

    /// <summary>
    /// Numeric field. Supports comparison operators like GreaterThan, LessThan, Between.
    /// </summary>
    Number,

    /// <summary>
    /// Date-only field. Supports date comparison and range operators.
    /// </summary>
    Date,

    /// <summary>
    /// Date and time field. Supports date/time comparison and range operators.
    /// </summary>
    DateTime,

    /// <summary>
    /// Boolean field. Supports IsTrue and IsFalse operators.
    /// </summary>
    Boolean,

    /// <summary>
    /// Enumeration field with predefined options. Supports Equals, In, and NotIn operators.
    /// </summary>
    Enum
}
