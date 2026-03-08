using BlazorBlueprint.Primitives.Table;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Defines how data should be grouped in the DataGrid.
/// </summary>
public class GroupDefinition
{
    /// <summary>
    /// Gets the ID of the column to group by.
    /// </summary>
    public required string ColumnId { get; init; }

    /// <summary>
    /// Gets the sort direction for group ordering. Default is ascending.
    /// </summary>
    public SortDirection GroupSortDirection { get; init; } = SortDirection.Ascending;
}
