namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Manages row selection state for a table.
/// Supports single and multiple selection modes with reference equality tracking.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public class SelectionState<TData> where TData : class
{
    private readonly HashSet<TData> selectedItems = new();

    /// <summary>
    /// Gets the collection of selected items.
    /// </summary>
    public IReadOnlyCollection<TData> SelectedItems => selectedItems;

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public SelectionMode Mode { get; set; } = SelectionMode.None;

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
    /// Clears all selections.
    /// </summary>
    public void Clear() => selectedItems.Clear();

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
