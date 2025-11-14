namespace BlazorUI.Primitives.Table;

/// <summary>
/// Represents a filter applied to a specific column.
/// </summary>
public class ColumnFilter
{
    /// <summary>
    /// Gets or sets the unique identifier of the column being filtered.
    /// </summary>
    public string ColumnId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filter value (e.g., search text, selected option).
    /// Null indicates no filter is applied for this column.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="ColumnFilter"/>.
    /// </summary>
    public ColumnFilter()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="ColumnFilter"/> with the specified column ID and value.
    /// </summary>
    /// <param name="columnId">The unique identifier of the column.</param>
    /// <param name="value">The filter value.</param>
    public ColumnFilter(string columnId, string? value)
    {
        ColumnId = columnId;
        Value = value;
    }
}
