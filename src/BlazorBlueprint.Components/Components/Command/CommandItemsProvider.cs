namespace BlazorBlueprint.Components;

/// <summary>
/// Delegate for asynchronous, server-side data fetching in a <see cref="BbCommandVirtualizedGroup{TItem}"/>.
/// Invoked as the user scrolls (and whenever the search query changes) when
/// <c>EnableLazyLoading</c> is <c>true</c> and an <c>ItemsProvider</c> is supplied — so the caller
/// never has to materialize the full collection up front.
/// </summary>
/// <typeparam name="TItem">The type of data items.</typeparam>
/// <param name="request">The request describing the slice to fetch, the active search text, and a cancellation token.</param>
/// <returns>A result containing the items for the requested slice and the total (filtered) count.</returns>
public delegate ValueTask<CommandItemsProviderResult<TItem>> CommandItemsProvider<TItem>(
    CommandItemsProviderRequest request);

/// <summary>
/// Describes the data request from <see cref="BbCommandVirtualizedGroup{TItem}"/> to the items provider.
/// </summary>
public class CommandItemsProviderRequest
{
    /// <summary>
    /// Gets the zero-based index of the first item to return.
    /// </summary>
    public int StartIndex { get; init; }

    /// <summary>
    /// Gets the maximum number of items to return for this slice.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Gets the active search text the provider should filter by, or <c>null</c> when no search is active.
    /// Filtering is the provider's responsibility — the component does not filter provider results locally.
    /// </summary>
    public string? SearchText { get; init; }

    /// <summary>
    /// Gets the cancellation token for the request. Cancelled when the request is superseded
    /// (e.g. the user keeps scrolling or changes the search) or the component is disposed.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// The result returned by a <see cref="CommandItemsProvider{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">The type of data items.</typeparam>
public class CommandItemsProviderResult<TItem>
{
    /// <summary>
    /// Gets the items for the requested slice.
    /// </summary>
    public required ICollection<TItem> Items { get; init; }

    /// <summary>
    /// Gets the total number of items matching the current search across all slices.
    /// Used to size the scroll area and drive keyboard navigation.
    /// </summary>
    public int TotalItemCount { get; init; }
}
