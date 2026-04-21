using BlazorBlueprint.Primitives.Table;

namespace BlazorBlueprint.Primitives.DataView;

/// <summary>
/// Delegate for asynchronous server-side data fetching in a DataView.
/// Invoked whenever pagination, sort, or search state changes.
/// </summary>
/// <typeparam name="TItem">The type of data items.</typeparam>
/// <param name="request">The request containing sort, pagination, search, and cancellation information.</param>
/// <returns>A result containing the items for the current page and the total count.</returns>
public delegate ValueTask<DataViewResult<TItem>> DataViewItemsProvider<TItem>(
    DataViewRequest request) where TItem : class;

/// <summary>
/// Describes the data request from BbDataView to the items provider.
/// </summary>
public class DataViewRequest
{
    /// <summary>
    /// Gets the zero-based index of the first item to return.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Gets the maximum number of items to return. Null means return all.
    /// </summary>
    public int? Count { get; init; }

    /// <summary>
    /// Gets the ID of the column to sort by, or <c>null</c> if no sort is active.
    /// </summary>
    public string? SortField { get; init; }

    /// <summary>
    /// Gets the sort direction. <see cref="SortDirection.None"/> when no sort is active.
    /// </summary>
    public SortDirection SortDirection { get; init; }

    /// <summary>
    /// Gets the global search text, or <c>null</c> if no search is active.
    /// </summary>
    public string? SearchText { get; init; }

    /// <summary>
    /// Gets the cancellation token for the request.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// The result returned by a <see cref="DataViewItemsProvider{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The type of data items.</typeparam>
public class DataViewResult<TItem>
{
    /// <summary>
    /// Gets the items for the current page/request.
    /// </summary>
    public required ICollection<TItem> Items { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// Used by the pagination component to calculate total pages.
    /// </summary>
    public int TotalItemCount { get; init; }
}
