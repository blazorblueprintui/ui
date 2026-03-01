using BlazorBlueprint.Primitives.Contexts;
using BlazorBlueprint.Primitives.Table;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Context for the DataGrid primitive component and its children.
/// Manages state, column definitions, data processing, and event coordination.
/// Cascaded to all child DataGrid components via CascadingValue.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridContext<TData> : PrimitiveContextWithEvents<DataGridState<TData>>
    where TData : class
{
    /// <summary>
    /// Gets or sets the column definitions for the grid.
    /// </summary>
    public IReadOnlyList<IDataGridColumn<TData>> Columns { get; set; } = Array.Empty<IDataGridColumn<TData>>();

    /// <summary>
    /// Gets or sets the processed data for the current page.
    /// </summary>
    public IEnumerable<TData> ProcessedData { get; set; } = Array.Empty<TData>();

    /// <summary>
    /// Gets or sets the selection mode for the grid.
    /// </summary>
    public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

    /// <summary>
    /// Gets or sets whether the grid is currently loading data.
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets whether keyboard navigation is enabled.
    /// </summary>
    public bool EnableKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// Callback invoked when sorting changes.
    /// </summary>
    public Action<IReadOnlyList<SortDefinition>>? OnSortChange { get; set; }

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
    /// Callback invoked when a column's visibility changes.
    /// </summary>
    public Action<string, bool>? OnColumnVisibilityChange { get; set; }

    /// <summary>
    /// Callback invoked when a column is reordered.
    /// </summary>
    public Action<string, int>? OnColumnReorder { get; set; }

    /// <summary>
    /// Callback invoked when a column is resized.
    /// </summary>
    public Action<string, string?>? OnColumnResize { get; set; }

    /// <summary>
    /// Initializes a new instance of the DataGridContext.
    /// </summary>
    /// <param name="state">The initial grid state.</param>
    public DataGridContext(DataGridState<TData> state) : base(state, "datagrid")
    {
    }

    /// <summary>
    /// Gets the ID for the grid element.
    /// </summary>
    public string GridId => GetScopedId("grid");

    /// <summary>
    /// Gets the ID for a header cell.
    /// </summary>
    public string GetHeaderCellId(string columnId) => GetScopedId($"header-{columnId}");

    /// <summary>
    /// Gets the ID for a grid row.
    /// </summary>
    public string GetRowId(int rowIndex) => GetScopedId($"row-{rowIndex}");

    /// <summary>
    /// Gets the ID for a grid cell.
    /// </summary>
    public string GetCellId(int rowIndex, string columnId) => GetScopedId($"cell-{rowIndex}-{columnId}");

    /// <summary>
    /// Toggles sorting for a column. Uses multi-sort when <paramref name="multiSort"/> is true.
    /// Resets pagination to the first page.
    /// </summary>
    /// <param name="columnId">The ID of the column to sort.</param>
    /// <param name="multiSort">Whether to add to existing sorts (Ctrl+Click behavior).</param>
    public void ToggleSort(string columnId, bool multiSort = false)
    {
        UpdateState(state =>
        {
            state.Sorting.ToggleSort(columnId, multiSort);
            state.Pagination.Reset();
        });

        OnSortChange?.Invoke(State.Sorting.Definitions);
    }

    /// <summary>
    /// Sets a single sort definition, replacing all existing sorts.
    /// </summary>
    public void SetSort(string columnId, SortDirection direction)
    {
        UpdateState(state =>
        {
            state.Sorting.SetSort(columnId, direction);
            state.Pagination.Reset();
        });

        OnSortChange?.Invoke(State.Sorting.Definitions);
    }

    /// <summary>
    /// Toggles the selection state of a row.
    /// </summary>
    public void ToggleRowSelection(TData item)
    {
        UpdateState(state => state.Selection.Toggle(item));

        OnRowSelect?.Invoke(item);
        OnSelectionChange?.Invoke(State.Selection.SelectedItems);
    }

    /// <summary>
    /// Selects a specific row.
    /// </summary>
    public void SelectRow(TData item)
    {
        UpdateState(state => state.Selection.Select(item));

        OnRowSelect?.Invoke(item);
        OnSelectionChange?.Invoke(State.Selection.SelectedItems);
    }

    /// <summary>
    /// Selects all items on the current page. Only works in Multiple mode.
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
    /// Clears all selections.
    /// </summary>
    public void ClearSelection()
    {
        UpdateState(state => state.Selection.Clear());

        OnSelectionChange?.Invoke(State.Selection.SelectedItems);
    }

    /// <summary>
    /// Navigates to a specific page.
    /// </summary>
    public void GoToPage(int page)
    {
        UpdateState(state => state.Pagination.GoToPage(page));

        OnPageChange?.Invoke(page);
    }

    /// <summary>
    /// Navigates to the next page.
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
    /// Navigates to the previous page.
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
    /// Changes the page size. Resets to page 1 automatically.
    /// </summary>
    public void ChangePageSize(int pageSize)
    {
        UpdateState(state => state.Pagination.PageSize = pageSize);

        OnPageSizeChange?.Invoke(pageSize);
    }

    /// <summary>
    /// Sets the visibility of a column.
    /// </summary>
    /// <param name="columnId">The column ID.</param>
    /// <param name="visible">Whether the column should be visible.</param>
    public void SetColumnVisibility(string columnId, bool visible)
    {
        UpdateState(state => state.Columns.SetVisibility(columnId, visible));

        OnColumnVisibilityChange?.Invoke(columnId, visible);
    }

    /// <summary>
    /// Moves a column to a new position.
    /// </summary>
    /// <param name="columnId">The column ID to move.</param>
    /// <param name="newIndex">The new zero-based position.</param>
    public void ReorderColumn(string columnId, int newIndex)
    {
        UpdateState(state => state.Columns.ReorderColumn(columnId, newIndex));

        OnColumnReorder?.Invoke(columnId, newIndex);
    }

    /// <summary>
    /// Sets the width of a column.
    /// </summary>
    /// <param name="columnId">The column ID.</param>
    /// <param name="width">The width value (e.g., "200px"), or null for auto.</param>
    public void SetColumnWidth(string columnId, string? width)
    {
        UpdateState(state => state.Columns.SetWidth(columnId, width));

        OnColumnResize?.Invoke(columnId, width);
    }

    /// <summary>
    /// Checks if a specific item is selected.
    /// </summary>
    public bool IsSelected(TData item) => State.Selection.IsSelected(item);

    /// <summary>
    /// Checks if all items on the current page are selected.
    /// </summary>
    public bool AreAllSelected() => State.Selection.AreAllSelected(ProcessedData);

    /// <summary>
    /// Checks if some (but not all) items on the current page are selected.
    /// </summary>
    public bool AreSomeSelected() => State.Selection.AreSomeSelected(ProcessedData);
}
