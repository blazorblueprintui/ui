namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Represents a group header in the flattened render list.
/// Contains the group key, item count, child items, and aggregate results.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridGroupRow<TData> where TData : class
{
    /// <summary>
    /// Gets the group key value (the value of the grouped column for this group).
    /// </summary>
    public object? Key { get; init; }

    /// <summary>
    /// Gets the ID of the column that was grouped by.
    /// </summary>
    public required string ColumnId { get; init; }

    /// <summary>
    /// Gets the display name of the grouped column.
    /// </summary>
    public string? ColumnTitle { get; init; }

    /// <summary>
    /// Gets the number of data items in this group.
    /// </summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// Gets the data items belonging to this group.
    /// </summary>
    public required IReadOnlyList<TData> Items { get; init; }

    /// <summary>
    /// Gets the aggregate results for this group, keyed by column ID.
    /// </summary>
    public Dictionary<string, AggregateResult> Aggregates { get; init; } = new();
}
