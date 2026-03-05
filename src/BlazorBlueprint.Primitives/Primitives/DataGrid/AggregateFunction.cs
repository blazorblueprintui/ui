namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Defines aggregate functions that can be computed on grouped data.
/// </summary>
public enum AggregateFunction
{
    /// <summary>
    /// No aggregate function.
    /// </summary>
    None = 0,

    /// <summary>
    /// Count of items in the group.
    /// </summary>
    Count,

    /// <summary>
    /// Sum of numeric values in the group.
    /// </summary>
    Sum,

    /// <summary>
    /// Average of numeric values in the group.
    /// </summary>
    Average,

    /// <summary>
    /// Minimum value in the group.
    /// </summary>
    Min,

    /// <summary>
    /// Maximum value in the group.
    /// </summary>
    Max
}

/// <summary>
/// The result of an aggregate computation for a specific column within a group.
/// </summary>
public class AggregateResult
{
    /// <summary>
    /// Gets the aggregate function that was applied.
    /// </summary>
    public required AggregateFunction Function { get; init; }

    /// <summary>
    /// Gets the computed aggregate value.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Gets the column ID this aggregate was computed for.
    /// </summary>
    public required string ColumnId { get; init; }

    /// <summary>
    /// Gets the format string for displaying the aggregate value (e.g., "C0", "N2").
    /// </summary>
    public string? Format { get; init; }
}
