namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a single filter condition with a field, operator, and value(s).
/// </summary>
public class FilterCondition
{
    /// <summary>
    /// Gets or sets the unique identifier for this condition.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Gets or sets the name of the field to filter on. Must match a <see cref="FilterField.Name"/>.
    /// </summary>
    public string Field { get; set; } = "";

    /// <summary>
    /// Gets or sets the comparison operator for this condition.
    /// </summary>
    public FilterOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the primary filter value.
    /// The type depends on the field type (string, double, DateTime, bool, string[] for In/NotIn).
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the secondary filter value, used for range operators like <see cref="FilterOperator.Between"/>.
    /// </summary>
    public object? ValueEnd { get; set; }

    /// <summary>
    /// Creates a deep clone of this condition.
    /// </summary>
    public FilterCondition Clone()
    {
        return new FilterCondition
        {
            Id = Id,
            Field = Field,
            Operator = Operator,
            Value = CloneValue(Value),
            ValueEnd = CloneValue(ValueEnd)
        };
    }

    private static object? CloneValue(object? value)
    {
        return value switch
        {
            null => null,
            string[] arr => (string[])arr.Clone(),
            Array arr => arr.Clone(),
            _ => value
        };
    }
}
