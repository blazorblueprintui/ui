namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Result type for paged child loading, containing a page of items and the total child count.
/// </summary>
/// <typeparam name="TItem">The type of items.</typeparam>
public class ChildPageResult<TItem>
{
    /// <summary>
    /// Gets the items for the current page.
    /// </summary>
    public required IReadOnlyList<TItem> Items { get; init; }

    /// <summary>
    /// Gets the total number of children (across all pages).
    /// </summary>
    public required int TotalCount { get; init; }
}
