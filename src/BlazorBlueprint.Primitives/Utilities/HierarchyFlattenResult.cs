namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Represents a single item in the flattened output of a hierarchical tree.
/// Each result carries metadata about its position, expansion state, and filter status.
/// </summary>
/// <typeparam name="TItem">The type of items in the hierarchy.</typeparam>
public readonly struct HierarchyFlattenResult<TItem>
{
    /// <summary>
    /// Gets the data item. Default when <see cref="IsChildPagerRow"/> is true.
    /// </summary>
    public TItem Item { get; init; }

    /// <summary>
    /// Gets the depth level of this item in the hierarchy (0 = root).
    /// </summary>
    public int Depth { get; init; }

    /// <summary>
    /// Gets whether this item has children (either loaded or indicated by predicate).
    /// </summary>
    public bool HasChildren { get; init; }

    /// <summary>
    /// Gets whether this item is currently expanded.
    /// </summary>
    public bool IsExpanded { get; init; }

    /// <summary>
    /// Gets whether this item is the last child among its siblings (for future tree-line rendering).
    /// </summary>
    public bool IsLastChild { get; init; }

    /// <summary>
    /// Gets whether this item directly matches the active filter.
    /// False when the item is shown only as an ancestor providing structural context.
    /// </summary>
    public bool MatchesFilter { get; init; }

    /// <summary>
    /// Gets whether this row is a child pagination control row rather than a data row.
    /// </summary>
    public bool IsChildPagerRow { get; init; }

    /// <summary>
    /// Gets the parent value for child pager rows, identifying which parent's children to page.
    /// </summary>
    public string? ParentValue { get; init; }

    /// <summary>
    /// Gets the current page index for this parent's children (0-based).
    /// </summary>
    public int ChildPageIndex { get; init; }

    /// <summary>
    /// Gets the total child count for this parent (for "Showing X of Y" display).
    /// </summary>
    public int TotalChildren { get; init; }
}
