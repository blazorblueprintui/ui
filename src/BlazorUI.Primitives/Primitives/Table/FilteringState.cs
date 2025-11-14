namespace BlazorUI.Primitives.Table;

/// <summary>
/// Manages filtering state for table data, including column-specific filters and global search.
/// </summary>
public class FilteringState
{
    private readonly Dictionary<string, string?> _columnFilters = new();

    /// <summary>
    /// Gets the collection of column filters currently applied.
    /// </summary>
    public IReadOnlyDictionary<string, string?> ColumnFilters => _columnFilters;

    /// <summary>
    /// Gets or sets the global filter value that searches across all columns.
    /// Null or empty string indicates no global filter is applied.
    /// </summary>
    public string? GlobalFilter { get; set; }

    /// <summary>
    /// Gets a value indicating whether any column filters are currently applied.
    /// </summary>
    public bool HasColumnFilters => _columnFilters.Any(f => !string.IsNullOrWhiteSpace(f.Value));

    /// <summary>
    /// Gets a value indicating whether a global filter is currently applied.
    /// </summary>
    public bool HasGlobalFilter => !string.IsNullOrWhiteSpace(GlobalFilter);

    /// <summary>
    /// Gets a value indicating whether any filters (column or global) are currently applied.
    /// </summary>
    public bool HasAnyFilter => HasColumnFilters || HasGlobalFilter;

    /// <summary>
    /// Sets a filter value for a specific column.
    /// </summary>
    /// <param name="columnId">The unique identifier of the column to filter.</param>
    /// <param name="value">The filter value. Null or empty string removes the filter.</param>
    /// <exception cref="ArgumentException">Thrown when columnId is null or whitespace.</exception>
    public void SetColumnFilter(string columnId, string? value)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            throw new ArgumentException("Column ID cannot be null or whitespace.", nameof(columnId));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            _columnFilters.Remove(columnId);
        }
        else
        {
            _columnFilters[columnId] = value;
        }
    }

    /// <summary>
    /// Gets the filter value for a specific column.
    /// </summary>
    /// <param name="columnId">The unique identifier of the column.</param>
    /// <returns>The filter value, or null if no filter is applied to this column.</returns>
    public string? GetColumnFilter(string columnId)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            return null;
        }

        return _columnFilters.TryGetValue(columnId, out var value) ? value : null;
    }

    /// <summary>
    /// Removes the filter for a specific column.
    /// </summary>
    /// <param name="columnId">The unique identifier of the column.</param>
    public void ClearColumnFilter(string columnId)
    {
        if (!string.IsNullOrWhiteSpace(columnId))
        {
            _columnFilters.Remove(columnId);
        }
    }

    /// <summary>
    /// Removes all column filters.
    /// </summary>
    public void ClearAllColumnFilters()
    {
        _columnFilters.Clear();
    }

    /// <summary>
    /// Removes all filters (both column and global).
    /// </summary>
    public void ClearAllFilters()
    {
        _columnFilters.Clear();
        GlobalFilter = null;
    }

    /// <summary>
    /// Checks if a specific column has a filter applied.
    /// </summary>
    /// <param name="columnId">The unique identifier of the column.</param>
    /// <returns>True if the column has a non-empty filter value; otherwise, false.</returns>
    public bool IsColumnFiltered(string columnId)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            return false;
        }

        return _columnFilters.TryGetValue(columnId, out var value) && !string.IsNullOrWhiteSpace(value);
    }
}
