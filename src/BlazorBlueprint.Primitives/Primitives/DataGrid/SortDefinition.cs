namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Represents a single column sort configuration for multi-column sorting.
/// </summary>
public class SortDefinition
{
    /// <summary>
    /// Gets or sets the ID of the column being sorted.
    /// </summary>
    public required string ColumnId { get; set; }

    /// <summary>
    /// Gets or sets the sort direction for this column.
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
}
