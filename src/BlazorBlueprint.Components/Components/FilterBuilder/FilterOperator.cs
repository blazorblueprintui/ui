namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the comparison operator for a filter condition.
/// Available operators depend on the <see cref="FilterFieldType"/> of the field.
/// </summary>
public enum FilterOperator
{
    // Universal
    Equals,
    NotEquals,
    IsEmpty,
    IsNotEmpty,

    // String
    Contains,
    NotContains,
    StartsWith,
    EndsWith,

    // Number / Date comparison
    GreaterThan,
    LessThan,
    GreaterOrEqual,
    LessOrEqual,
    Between,

    // Date-specific
    InLast,
    InNext,

    // Enum
    In,
    NotIn,

    // Boolean
    IsTrue,
    IsFalse,

    // Date preset
    DateIs,
    DateIsNot
}
