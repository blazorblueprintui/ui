namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Serializable snapshot of DataGrid state for persistence.
/// Contains sorting, column visibility/order/width, and page size.
/// Consumers can serialize this to JSON and store in localStorage, a database, etc.
/// </summary>
public class DataGridStateSnapshot
{
    /// <summary>
    /// Gets or sets the sort definitions.
    /// </summary>
    public List<SortDefinitionSnapshot> SortDefinitions { get; set; } = new();

    /// <summary>
    /// Gets or sets the column state entries.
    /// </summary>
    public List<ColumnStateSnapshot> ColumnStates { get; set; } = new();

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// Serializable snapshot of a single sort definition.
/// </summary>
public class SortDefinitionSnapshot
{
    /// <summary>
    /// Gets or sets the column ID.
    /// </summary>
    public string ColumnId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public SortDirection Direction { get; set; }
}

/// <summary>
/// Serializable snapshot of a single column's state.
/// </summary>
public class ColumnStateSnapshot
{
    /// <summary>
    /// Gets or sets the column ID.
    /// </summary>
    public string ColumnId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the column is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the column width (e.g., "200px"). Null for auto.
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the display order of the column.
    /// </summary>
    public int Order { get; set; }
}
