using BlazorBlueprint.Primitives.Filtering;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Manages per-column filter state for the DataGrid.
/// Each column filter is stored as a <see cref="FilterCondition"/> keyed by column ID.
/// </summary>
public class DataGridFilterState
{
    private readonly Dictionary<string, FilterCondition> filters = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the active column filters as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, FilterCondition> Filters => filters;

    /// <summary>
    /// Gets whether any filters are active.
    /// </summary>
    public bool HasFilters => filters.Count > 0;

    /// <summary>
    /// Gets the number of active column filters.
    /// </summary>
    public int ActiveFilterCount => filters.Count;

    /// <summary>
    /// Sets or updates a filter for a specific column. Pass null to remove the filter.
    /// </summary>
    /// <param name="columnId">The column identifier.</param>
    /// <param name="condition">The filter condition, or null to clear.</param>
    public void SetFilter(string columnId, FilterCondition? condition)
    {
        if (condition == null)
        {
            filters.Remove(columnId);
        }
        else
        {
            filters[columnId] = condition;
        }
    }

    /// <summary>
    /// Gets the filter condition for a specific column, or null if not filtered.
    /// </summary>
    /// <param name="columnId">The column identifier.</param>
    /// <returns>The filter condition, or null.</returns>
    public FilterCondition? GetFilter(string columnId) =>
        filters.TryGetValue(columnId, out var condition) ? condition : null;

    /// <summary>
    /// Gets whether a specific column has an active filter.
    /// </summary>
    /// <param name="columnId">The column identifier.</param>
    /// <returns>True if the column has an active filter.</returns>
    public bool HasFilter(string columnId) =>
        filters.ContainsKey(columnId);

    /// <summary>
    /// Removes the filter for a specific column.
    /// </summary>
    /// <param name="columnId">The column identifier.</param>
    public void ClearFilter(string columnId) =>
        filters.Remove(columnId);

    /// <summary>
    /// Removes all column filters.
    /// </summary>
    public void ClearAll() =>
        filters.Clear();
}
