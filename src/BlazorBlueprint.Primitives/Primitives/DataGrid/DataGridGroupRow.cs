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
    /// Gets the ordered keys identifying this group, outermost first.
    /// </summary>
    /// <remarks>
    /// This is what collapsed state is keyed by. A raw key is ambiguous once grouping nests: a
    /// group keyed <c>Active</c> exists under every department, and collapsing one would collapse
    /// them all. Defaults to a single-segment path built from <see cref="Key"/>.
    /// </remarks>
    public GroupPath Path { get; init; } = GroupPath.Root;

    /// <summary>
    /// Gets how deep this group sits, zero-based. The outermost level is 0.
    /// </summary>
    public int Depth { get; init; }

    /// <summary>
    /// Gets the groups nested directly inside this one, outermost first, or an empty list for the
    /// innermost level.
    /// </summary>
    public IReadOnlyList<DataGridGroupRow<TData>> Children { get; init; } =
        Array.Empty<DataGridGroupRow<TData>>();

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
    /// Gets the data items belonging to this group, including those in every nested group beneath
    /// it, so aggregates roll up rather than counting only the leaf level.
    /// </summary>
    public required IReadOnlyList<TData> Items { get; init; }

    /// <summary>
    /// Gets the aggregate results for this group, keyed by column ID.
    /// </summary>
    public Dictionary<string, AggregateResult> Aggregates { get; init; } = new();
}
