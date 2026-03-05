namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Manages row expansion state for a DataGrid.
/// Tracks which rows are expanded using reference equality via a HashSet.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class ExpandedRowState<TData> where TData : class
{
    private HashSet<TData> expandedItems = new();

    /// <summary>
    /// Gets the collection of expanded items.
    /// </summary>
    public IReadOnlyCollection<TData> ExpandedItems => expandedItems;

    /// <summary>
    /// Gets the number of expanded items.
    /// </summary>
    public int ExpandedCount => expandedItems.Count;

    /// <summary>
    /// Gets whether any items are expanded.
    /// </summary>
    public bool HasExpanded => expandedItems.Count > 0;

    /// <summary>
    /// Checks if a specific item is expanded.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is expanded, false otherwise.</returns>
    public bool IsExpanded(TData item)
    {
        if (item == null)
        {
            return false;
        }

        return expandedItems.Contains(item);
    }

    /// <summary>
    /// Expands an item.
    /// </summary>
    /// <param name="item">The item to expand.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void Expand(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);
        expandedItems.Add(item);
    }

    /// <summary>
    /// Collapses an item.
    /// </summary>
    /// <param name="item">The item to collapse.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void Collapse(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);
        expandedItems.Remove(item);
    }

    /// <summary>
    /// Toggles the expansion state of an item.
    /// If expanded, collapses it. If collapsed, expands it.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void Toggle(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (IsExpanded(item))
        {
            Collapse(item);
        }
        else
        {
            Expand(item);
        }
    }

    /// <summary>
    /// Expands all items in the provided collection.
    /// </summary>
    /// <param name="items">The items to expand.</param>
    public void ExpandAll(IEnumerable<TData> items)
    {
        foreach (var item in items)
        {
            if (item != null)
            {
                expandedItems.Add(item);
            }
        }
    }

    /// <summary>
    /// Collapses all items in the provided collection.
    /// </summary>
    /// <param name="items">The items to collapse.</param>
    public void CollapseAll(IEnumerable<TData> items)
    {
        foreach (var item in items)
        {
            expandedItems.Remove(item);
        }
    }

    /// <summary>
    /// Clears all expansion state.
    /// </summary>
    public void Clear() => expandedItems.Clear();

    /// <summary>
    /// Rebuilds the internal HashSet with the given comparer, preserving existing items.
    /// Pass null to revert to default reference equality.
    /// </summary>
    /// <param name="comparer">The equality comparer to use, or null for default.</param>
    public void SetComparer(IEqualityComparer<TData>? comparer) =>
        expandedItems = new HashSet<TData>(expandedItems, comparer);
}
