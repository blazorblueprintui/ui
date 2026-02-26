using BlazorBlueprint.Primitives.Table;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Aggregate state container for the DataGrid component.
/// Manages sorting, pagination, selection, and column state.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridState<TData> where TData : class
{
    /// <summary>
    /// Gets the multi-column sorting state.
    /// </summary>
    public DataGridSortState Sorting { get; } = new();

    /// <summary>
    /// Gets the pagination state.
    /// </summary>
    public PaginationState Pagination { get; } = new();

    /// <summary>
    /// Gets the row selection state.
    /// </summary>
    public SelectionState<TData> Selection { get; } = new();

    /// <summary>
    /// Gets the column state (visibility, order, widths).
    /// </summary>
    public DataGridColumnState Columns { get; } = new();

    /// <summary>
    /// Gets whether the grid has any active sorting.
    /// </summary>
    public bool HasSorting => Sorting.HasSorting;

    /// <summary>
    /// Gets whether any rows are selected.
    /// </summary>
    public bool HasSelection => Selection.HasSelection;

    /// <summary>
    /// Gets the total number of selected rows.
    /// </summary>
    public int TotalSelected => Selection.SelectedCount;

    /// <summary>
    /// Gets whether pagination is active (more than one page).
    /// </summary>
    public bool HasPagination => Pagination.TotalPages > 1;

    /// <summary>
    /// Resets all state to default values.
    /// </summary>
    public void Reset()
    {
        Sorting.ClearSort();
        Pagination.Reset();
        Selection.Clear();
        Columns.Reset();
    }

    /// <summary>
    /// Resets only the pagination state while preserving other state.
    /// </summary>
    public void ResetPagination() => Pagination.Reset();
}
