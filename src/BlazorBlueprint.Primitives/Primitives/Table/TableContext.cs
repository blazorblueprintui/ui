using BlazorBlueprint.Primitives.Contexts;

namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Context for Table primitive component and its children.
/// Manages table state, column definitions, data processing, and event coordination.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public class TableContext<TData> : PrimitiveContextWithEvents<TableState<TData>>
    where TData : class
{
    /// <summary>
    /// Gets or sets the column definitions for the table.
    /// </summary>
    public IEnumerable<IColumnDefinition<TData>> Columns { get; set; } = Array.Empty<IColumnDefinition<TData>>();

    /// <summary>
    /// Gets or sets the processed data for the current page.
    /// This data has already been sorted and paginated.
    /// </summary>
    public IEnumerable<TData> ProcessedData { get; set; } = Array.Empty<TData>();

    /// <summary>
    /// Gets or sets the selection mode for the table.
    /// </summary>
    public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

    /// <summary>
    /// Gets or sets whether keyboard navigation is enabled for table rows.
    /// When true, rows can be navigated with arrow keys and selected with Enter/Space.
    /// Default is true.
    /// </summary>
    public bool EnableKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// Callback invoked when sorting changes.
    /// </summary>
    public Action<string, SortDirection>? OnSortChange { get; set; }

    /// <summary>
    /// Callback invoked when a row is selected.
    /// </summary>
    public Action<TData>? OnRowSelect { get; set; }

    /// <summary>
    /// Callback invoked when the current page changes.
    /// </summary>
    public Action<int>? OnPageChange { get; set; }

    /// <summary>
    /// Callback invoked when the page size changes.
    /// </summary>
    public Action<int>? OnPageSizeChange { get; set; }

    /// <summary>
    /// Callback invoked when the selection changes.
    /// </summary>
    public Action<IReadOnlyCollection<TData>>? OnSelectionChange { get; set; }

    /// <summary>
    /// Initializes a new instance of the TableContext.
    /// </summary>
    /// <param name="state">The initial table state.</param>
    public TableContext(TableState<TData> state) : base(state, "table")
    {
    }

    /// <summary>
    /// Gets the ID for the table element.
    /// </summary>
    public string TableId => GetScopedId("table");

    /// <summary>
    /// Gets the ID for a header cell.
    /// </summary>
    /// <param name="columnId">The column identifier.</param>
    public string GetHeaderCellId(string columnId) => GetScopedId($"header-{columnId}");

    /// <summary>
    /// Gets the ID for a table row.
    /// </summary>
    /// <param name="rowIndex">The zero-based row index.</param>
    public string GetRowId(int rowIndex) => GetScopedId($"row-{rowIndex}");

    /// <summary>
    /// Gets the ID for a table cell.
    /// </summary>
    /// <param name="rowIndex">The zero-based row index.</param>
    /// <param name="columnId">The column identifier.</param>
    public string GetCellId(int rowIndex, string columnId) => GetScopedId($"cell-{rowIndex}-{columnId}");

    /// <summary>
    /// Toggles sorting for a column and notifies subscribers.
    /// </summary>
    /// <param name="columnId">The ID of the column to sort.</param>
    public void ToggleSort(string columnId)
    {
        UpdateState(state =>
        {
            state.Sorting.ToggleSort(columnId);
            // Reset to first page when sorting changes
            state.Pagination.Reset();
        });

        OnSortChange?.Invoke(columnId, State.Sorting.Direction);
    }

    /// <summary>
    /// Sets the sort direction for a column and notifies subscribers.
    /// </summary>
    /// <param name="columnId">The ID of the column to sort.</param>
    /// <param name="direction">The sort direction.</param>
    public void SetSort(string columnId, SortDirection direction)
    {
        UpdateState(state =>
        {
            state.Sorting.SetSort(columnId, direction);
            // Reset to first page when sorting changes
            state.Pagination.Reset();
        });

        OnSortChange?.Invoke(columnId, direction);
    }

    /// <summary>
    /// Toggles the selection state of a row and notifies subscribers.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    public void ToggleRowSelection(TData item)
    {
        UpdateState(state => state.Selection.Toggle(item));

        OnRowSelect?.Invoke(item);
        OnSelectionChange?.Invoke(State.Selection.SelectedItems);
    }

    /// <summary>
    /// Selects a specific row and notifies subscribers.
    /// </summary>
    /// <param name="item">The item to select.</param>
    public void SelectRow(TData item)
    {
        UpdateState(state => state.Selection.Select(item));

        OnRowSelect?.Invoke(item);
        OnSelectionChange?.Invoke(State.Selection.SelectedItems);
    }

    /// <summary>
    /// Selects all items on the current page and notifies subscribers.
    /// Only works in Multiple selection mode.
    /// </summary>
    public void SelectAllOnPage()
    {
        if (SelectionMode != SelectionMode.Multiple)
        {
            return;
        }

        UpdateState(state => state.Selection.SelectAll(ProcessedData));

        OnSelectionChange?.Invoke(State.Selection.SelectedItems);
    }

    /// <summary>
    /// Clears all selections and notifies subscribers.
    /// </summary>
    public void ClearSelection()
    {
        UpdateState(state => state.Selection.Clear());

        OnSelectionChange?.Invoke(State.Selection.SelectedItems);
    }

    /// <summary>
    /// Navigates to a specific page and notifies subscribers.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    public void GoToPage(int page)
    {
        UpdateState(state => state.Pagination.GoToPage(page));

        OnPageChange?.Invoke(page);
    }

    /// <summary>
    /// Navigates to the next page and notifies subscribers.
    /// </summary>
    public void NextPage()
    {
        if (State.Pagination.CanGoNext)
        {
            var newPage = State.Pagination.CurrentPage + 1;
            UpdateState(state => state.Pagination.NextPage());

            OnPageChange?.Invoke(newPage);
        }
    }

    /// <summary>
    /// Navigates to the previous page and notifies subscribers.
    /// </summary>
    public void PreviousPage()
    {
        if (State.Pagination.CanGoPrevious)
        {
            var newPage = State.Pagination.CurrentPage - 1;
            UpdateState(state => state.Pagination.PreviousPage());

            OnPageChange?.Invoke(newPage);
        }
    }

    /// <summary>
    /// Changes the page size and notifies subscribers.
    /// Resets to page 1 automatically.
    /// </summary>
    /// <param name="pageSize">The new page size.</param>
    public void ChangePageSize(int pageSize)
    {
        UpdateState(state => state.Pagination.PageSize = pageSize);

        OnPageSizeChange?.Invoke(pageSize);
    }

    /// <summary>
    /// Checks if a specific item is selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is selected, false otherwise.</returns>
    public bool IsSelected(TData item) => State.Selection.IsSelected(item);

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
