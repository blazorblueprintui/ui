namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Context provided to table render templates.
/// Contains all information needed for custom table rendering.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public class TableRenderContext<TData> where TData : class
{
    /// <summary>
    /// Gets or sets the column definitions for the table.
    /// </summary>
    public required IEnumerable<IColumnDefinition<TData>> Columns { get; set; }

    /// <summary>
    /// Gets or sets the current table state (sorting, pagination, selection).
    /// </summary>
    public required TableState<TData> State { get; set; }

    /// <summary>
    /// Gets or sets the processed data for the current page.
    /// This data has already been sorted and paginated.
    /// </summary>
    public required IEnumerable<TData> ProcessedData { get; set; }

    /// <summary>
    /// Gets or sets the selection mode for the table.
    /// </summary>
    public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

    /// <summary>
    /// Checks if a specific item is selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is selected, false otherwise.</returns>
    public bool IsSelected(TData item) => State.Selection.IsSelected(item);

    /// <summary>
    /// Toggles the selection state of an item.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    public void ToggleSelection(TData item) => State.Selection.Toggle(item);

    /// <summary>
    /// Toggles sorting for a column.
    /// </summary>
    /// <param name="columnId">The ID of the column to sort.</param>
    public void ToggleSort(string columnId) => State.Sorting.ToggleSort(columnId);

    /// <summary>
    /// Navigates to a specific page.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    public void GoToPage(int page) => State.Pagination.GoToPage(page);

    /// <summary>
    /// Selects all items on the current page.
    /// Only works in Multiple selection mode.
    /// </summary>
    public void SelectAll()
    {
        if (SelectionMode == SelectionMode.Multiple)
        {
            State.Selection.SelectAll(ProcessedData);
        }
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    public void ClearSelection() => State.Selection.Clear();

    /// <summary>
    /// Checks if all items on the current page are selected.
    /// </summary>
    /// <returns>True if all items are selected, false otherwise.</returns>
    public bool AreAllSelected() => State.Selection.AreAllSelected(ProcessedData);

    /// <summary>
    /// Checks if some (but not all) items on the current page are selected.
    /// </summary>
    /// <returns>True if some items are selected, false otherwise.</returns>
    public bool AreSomeSelected() => State.Selection.AreSomeSelected(ProcessedData);
}
