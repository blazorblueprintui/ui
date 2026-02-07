namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Container for all table state including sorting, pagination, and selection.
/// This class serves as the single source of truth for table configuration.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public class TableState<TData> where TData : class
{
    /// <summary>
    /// Gets the sorting state for the table.
    /// </summary>
    public SortingState Sorting { get; } = new();

    /// <summary>
    /// Gets the pagination state for the table.
    /// </summary>
    public PaginationState Pagination { get; } = new();

    /// <summary>
    /// Gets the selection state for the table.
    /// </summary>
    public SelectionState<TData> Selection { get; } = new();

    /// <summary>
    /// Gets whether the table has any active sorting.
    /// </summary>
    public bool HasSorting => Sorting.Direction != SortDirection.None;

    /// <summary>
    /// Gets whether the table has any selected items.
    /// </summary>
    public bool HasSelection => Selection.HasSelection;

    /// <summary>
    /// Gets the total number of selected items.
    /// </summary>
    public int TotalSelected => Selection.SelectedCount;

    /// <summary>
    /// Gets whether pagination is enabled and active.
    /// </summary>
    public bool HasPagination => Pagination.TotalPages > 1;

    /// <summary>
    /// Resets all state to default values.
    /// Clears sorting, resets to first page, and clears selection.
    /// </summary>
    public void Reset()
    {
        Sorting.ClearSort();
        Pagination.Reset();
        Selection.Clear();
    }

    /// <summary>
    /// Resets only the pagination state while preserving sorting and selection.
    /// Useful when data changes.
    /// </summary>
    public void ResetPagination() => Pagination.Reset();
}
