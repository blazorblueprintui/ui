namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Manages row selection state for a table.
/// Supports single and multiple selection modes with reference equality tracking.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public class SelectionState<TData> where TData : class
{
    private HashSet<TData> selectedItems = new();

    /// <summary>
    /// Gets the collection of selected items.
    /// </summary>
    public IReadOnlyCollection<TData> SelectedItems => selectedItems;

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public SelectionMode Mode { get; set; } = SelectionMode.None;

    /// <summary>
    /// Gets or sets the anchor row that a range selection extends from.
    /// </summary>
    /// <remarks>
    /// Shift+Click extends from the last anchor, not from the last selected row. The two differ
    /// after a Ctrl+Click, and using the wrong one is what makes range selection feel wrong rather
    /// than plainly broken. A plain click and a Ctrl+Click both move the anchor; a Shift+Click
    /// leaves it where it is, so repeated Shift+Clicks re-extend from the same origin.
    /// </remarks>
    public TData? Anchor { get; set; }

    /// <summary>
    /// Gets the number of selected items.
    /// </summary>
    public int SelectedCount => selectedItems.Count;

    /// <summary>
    /// Gets whether any items are selected.
    /// </summary>
    public bool HasSelection => selectedItems.Count > 0;

    /// <summary>
    /// Checks if a specific item is selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is selected, false otherwise.</returns>
    public bool IsSelected(TData item)
    {
        if (item == null)
        {
            return false;
        }

        return selectedItems.Contains(item);
    }

    /// <summary>
    /// Selects an item.
    /// In Single mode, clears previous selection first.
    /// In None mode, has no effect.
    /// </summary>
    /// <param name="item">The item to select.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void Select(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Mode == SelectionMode.None)
        {
            return;
        }

        if (Mode == SelectionMode.Single)
        {
            selectedItems.Clear();
        }

        selectedItems.Add(item);
    }

    /// <summary>
    /// Deselects an item.
    /// </summary>
    /// <param name="item">The item to deselect.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void Deselect(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        selectedItems.Remove(item);
    }

    /// <summary>
    /// Toggles the selection state of an item.
    /// If selected, deselects it. If not selected, selects it.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void Toggle(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (IsSelected(item))
        {
            Deselect(item);
        }
        else
        {
            Select(item);
        }
    }

    /// <summary>
    /// Selects all items in the provided collection.
    /// Only works in Multiple mode.
    /// </summary>
    /// <param name="items">The items to select.</param>
    public void SelectAll(IEnumerable<TData> items)
    {
        if (Mode != SelectionMode.Multiple)
        {
            return;
        }

        foreach (var item in items)
        {
            if (item != null)
            {
                selectedItems.Add(item);
            }
        }
    }

    /// <summary>
    /// Deselects all items in the provided collection.
    /// </summary>
    /// <param name="items">The items to deselect.</param>
    public void DeselectAll(IEnumerable<TData> items)
    {
        foreach (var item in items)
        {
            selectedItems.Remove(item);
        }
    }

    /// <summary>
    /// Replaces the whole selection with a single item.
    /// Has no effect in <see cref="SelectionMode.None"/>.
    /// </summary>
    /// <param name="item">The only item that should end up selected.</param>
    /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
    public void ReplaceWith(TData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Mode == SelectionMode.None)
        {
            return;
        }

        selectedItems.Clear();
        selectedItems.Add(item);
    }

    /// <summary>
    /// Replaces the selection with the inclusive range of <paramref name="rows"/> between
    /// <paramref name="from"/> and <paramref name="to"/>, in either order.
    /// Only works in <see cref="SelectionMode.Multiple"/>.
    /// </summary>
    /// <remarks>
    /// If either end is not present in <paramref name="rows"/> — a stale anchor left behind by a
    /// page change or a filter, for instance — the selection falls back to
    /// <paramref name="to"/> alone rather than selecting nothing or throwing.
    /// </remarks>
    /// <param name="rows">The rows in display order.</param>
    /// <param name="from">The anchor end of the range.</param>
    /// <param name="to">The clicked end of the range.</param>
    /// <exception cref="ArgumentNullException">Thrown when rows or to is null.</exception>
    public void SelectRange(IEnumerable<TData> rows, TData? from, TData to)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(to);

        if (Mode != SelectionMode.Multiple)
        {
            ReplaceWith(to);
            return;
        }

        var ordered = rows as IList<TData> ?? rows.ToList();
        var toIndex = IndexOf(ordered, to);
        var fromIndex = from != null ? IndexOf(ordered, from) : -1;

        if (toIndex < 0 || fromIndex < 0)
        {
            ReplaceWith(to);
            return;
        }

        var start = Math.Min(fromIndex, toIndex);
        var end = Math.Max(fromIndex, toIndex);

        selectedItems.Clear();
        for (var i = start; i <= end; i++)
        {
            selectedItems.Add(ordered[i]);
        }
    }

    /// <summary>
    /// Finds a row's position using the same equality the selection uses, so an item key comparer
    /// still matches a row that was re-materialized between renders.
    /// </summary>
    private int IndexOf(IList<TData> rows, TData target)
    {
        var comparer = selectedItems.Comparer;
        for (var i = 0; i < rows.Count; i++)
        {
            if (comparer.Equals(rows[i], target))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    public void Clear()
    {
        selectedItems.Clear();
        Anchor = null;
    }

    /// <summary>
    /// Checks if all items in a collection are selected.
    /// </summary>
    /// <param name="items">The items to check.</param>
    /// <returns>True if all items are selected, false otherwise.</returns>
    public bool AreAllSelected(IEnumerable<TData> items) => items.All(item => IsSelected(item));

    /// <summary>
    /// Checks if some (but not all) items in a collection are selected.
    /// </summary>
    /// <param name="items">The items to check.</param>
    /// <returns>True if some items are selected but not all, false otherwise.</returns>
    public bool AreSomeSelected(IEnumerable<TData> items)
    {
        // Optimize with single-pass enumeration
        var itemsList = items as IList<TData> ?? items.ToList();
        var selectedCount = 0;
        var totalCount = itemsList.Count;

        foreach (var item in itemsList)
        {
            if (IsSelected(item))
            {
                selectedCount++;
            }
        }

        return selectedCount > 0 && selectedCount < totalCount;
    }

    /// <summary>
    /// Rebuilds the internal HashSet with the given comparer, preserving existing items.
    /// Pass null to revert to default reference equality.
    /// </summary>
    /// <param name="comparer">The equality comparer to use, or null for default.</param>
    public void SetComparer(IEqualityComparer<TData>? comparer) =>
        selectedItems = new HashSet<TData>(selectedItems, comparer);

    /// <summary>
    /// Sets the selection state for multiple items at once.
    /// </summary>
    /// <param name="items">The items to select.</param>
    /// <param name="selected">True to select, false to deselect.</param>
    public void SetSelection(IEnumerable<TData> items, bool selected)
    {
        if (selected)
        {
            SelectAll(items);
        }
        else
        {
            DeselectAll(items);
        }
    }
}
