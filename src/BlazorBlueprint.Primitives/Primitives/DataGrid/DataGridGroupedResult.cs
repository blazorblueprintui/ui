namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Delegate for asynchronous server-side grouped data fetching.
/// Used when the DataGrid needs to load grouped data from a remote source.
/// </summary>
/// <typeparam name="TData">The type of data items.</typeparam>
/// <param name="request">The request containing sort, pagination, grouping, and cancellation information.</param>
/// <returns>A result containing the grouped items and total count.</returns>
public delegate ValueTask<DataGridGroupedResult<TData>> DataGridGroupedItemsProvider<TData>(
    DataGridRequest request) where TData : class;

/// <summary>
/// The result returned by a <see cref="DataGridGroupedItemsProvider{TData}"/>.
/// Contains pre-grouped data with optional aggregate results.
/// </summary>
/// <typeparam name="TData">The type of data items.</typeparam>
public class DataGridGroupedResult<TData> where TData : class
{
    /// <summary>
    /// Gets the groups with their items and optional aggregates.
    /// </summary>
    public required ICollection<DataGridGroupResult<TData>> Groups { get; init; }

    /// <summary>
    /// Gets the total number of items across all groups (for pagination).
    /// </summary>
    public int TotalItemCount { get; init; }
}

/// <summary>
/// Represents a single group within a <see cref="DataGridGroupedResult{TData}"/>.
/// </summary>
/// <typeparam name="TData">The type of data items.</typeparam>
public class DataGridGroupResult<TData> where TData : class
{
    /// <summary>
    /// Gets the group key value.
    /// </summary>
    public object? Key { get; init; }

    /// <summary>
    /// Gets the items in this group.
    /// </summary>
    public required ICollection<TData> Items { get; init; }

    /// <summary>
    /// Gets the total number of items in this group.
    /// </summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// Gets the aggregate results for this group, keyed by column ID.
    /// </summary>
    public Dictionary<string, AggregateResult>? Aggregates { get; init; }
}
