using BlazorBlueprint.Primitives.Filtering;
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
    /// Gets the per-column filter state.
    /// </summary>
    public DataGridFilterState Filtering { get; } = new();

    /// <summary>
    /// Gets the row expansion state. Tracks which rows have their detail content visible.
    /// Expansion is transient and not persisted in snapshots.
    /// </summary>
    public ExpandedRowState<TData> Expanded { get; } = new();

    /// <summary>
    /// Gets a version counter that increments whenever state is mutated
    /// through <see cref="Restore"/> or <see cref="Reset"/>.
    /// Used by the grid component to detect external state changes.
    /// </summary>
    public int Version { get; private set; }

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
    /// Gets whether any rows are expanded.
    /// </summary>
    public bool HasExpanded => Expanded.HasExpanded;

    /// <summary>
    /// Gets whether any column filters are active.
    /// </summary>
    public bool HasFiltering => Filtering.HasFilters;

    /// <summary>
    /// Gets whether pagination is active (more than one page).
    /// </summary>
    public bool HasPagination => Pagination.TotalPages > 1;

    /// <summary>
    /// Configures a key-based equality comparer for selection and expansion state.
    /// When set, items are compared by their key (e.g., <c>item => item.Id</c>) instead
    /// of by reference equality, so state survives data re-fetches.
    /// Pass null to revert to default reference equality.
    /// </summary>
    /// <param name="itemKey">A function that returns a stable identity key for each item, or null.</param>
    public void SetItemKey(Func<TData, object>? itemKey)
    {
        var comparer = itemKey != null ? new KeyBasedEqualityComparer<TData>(itemKey) : null;
        Selection.SetComparer(comparer);
        Expanded.SetComparer(comparer);
    }

    /// <summary>
    /// Resets all state to default values.
    /// </summary>
    public void Reset()
    {
        Sorting.ClearSort();
        Pagination.Reset();
        Selection.Clear();
        Expanded.Clear();
        Columns.Reset();
        Filtering.ClearAll();
        Version++;
    }

    /// <summary>
    /// Resets only the pagination state while preserving other state.
    /// </summary>
    public void ResetPagination() => Pagination.Reset();

    /// <summary>
    /// Creates a serializable snapshot of the current grid state.
    /// Captures sorting, column visibility/order/widths, and page size.
    /// Selection state is intentionally excluded as it is transient.
    /// </summary>
    /// <returns>A snapshot that can be serialized and persisted.</returns>
    public DataGridStateSnapshot Save()
    {
        var snapshot = new DataGridStateSnapshot
        {
            PageSize = Pagination.PageSize
        };

        foreach (var sort in Sorting.Definitions)
        {
            snapshot.SortDefinitions.Add(new SortDefinitionSnapshot
            {
                ColumnId = sort.ColumnId,
                Direction = sort.Direction
            });
        }

        foreach (var entry in Columns.Entries)
        {
            snapshot.ColumnStates.Add(new ColumnStateSnapshot
            {
                ColumnId = entry.ColumnId,
                Visible = entry.Visible,
                Width = entry.Width,
                Order = entry.Order
            });
        }

        foreach (var (columnId, condition) in Filtering.Filters)
        {
            snapshot.ColumnFilters.Add(new ColumnFilterSnapshot
            {
                ColumnId = columnId,
                Operator = condition.Operator,
                Value = condition.Value,
                ValueEnd = condition.ValueEnd
            });
        }

        return snapshot;
    }

    /// <summary>
    /// Restores grid state from a previously saved snapshot.
    /// Applies sorting, column visibility/order/widths, and page size.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    public void Restore(DataGridStateSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        Sorting.ClearSort();
        foreach (var sort in snapshot.SortDefinitions)
        {
            Sorting.AddSort(sort.ColumnId, sort.Direction);
        }

        Columns.RestoreFromSnapshots(snapshot.ColumnStates);
        Pagination.PageSize = snapshot.PageSize;

        Filtering.ClearAll();
        foreach (var filter in snapshot.ColumnFilters)
        {
            Filtering.SetFilter(filter.ColumnId, new FilterCondition
            {
                Field = filter.ColumnId,
                Operator = filter.Operator,
                Value = filter.Value,
                ValueEnd = filter.ValueEnd
            });
        }

        Version++;
    }
}
