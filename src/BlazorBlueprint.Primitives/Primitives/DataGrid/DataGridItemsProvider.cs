namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Delegate for asynchronous server-side data fetching.
/// Used when the DataGrid needs to load data from a remote source (REST API, gRPC, etc.).
/// </summary>
/// <typeparam name="TData">The type of data items.</typeparam>
/// <param name="request">The request containing sort, pagination, and cancellation information.</param>
/// <returns>A result containing the items and total count.</returns>
public delegate ValueTask<DataGridResult<TData>> DataGridItemsProvider<TData>(
    DataGridRequest request);

/// <summary>
/// Describes the data request from the DataGrid to the items provider.
/// Contains sort definitions, pagination offsets, and a cancellation token.
/// </summary>
public class DataGridRequest
{
    /// <summary>
    /// Gets the sort definitions to apply, in priority order.
    /// </summary>
    public IReadOnlyList<SortDefinition> SortDefinitions { get; init; } = Array.Empty<SortDefinition>();

    /// <summary>
    /// Gets the zero-based index of the first item to return.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Gets the maximum number of items to return. Null means return all.
    /// </summary>
    public int? Count { get; init; }

    /// <summary>
    /// Gets the cancellation token for the request.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// The result returned by a <see cref="DataGridItemsProvider{TData}"/>.
/// Contains the items for the current page and the total count for pagination.
/// </summary>
/// <typeparam name="TData">The type of data items.</typeparam>
public class DataGridResult<TData>
{
    /// <summary>
    /// Gets the items for the current page/request.
    /// </summary>
    public required ICollection<TData> Items { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// Used by the pagination component to calculate total pages.
    /// </summary>
    public int TotalItemCount { get; init; }
}
