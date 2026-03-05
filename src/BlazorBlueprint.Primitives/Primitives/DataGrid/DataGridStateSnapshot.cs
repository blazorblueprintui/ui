using BlazorBlueprint.Primitives.Filtering;
using BlazorBlueprint.Primitives.Table;

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

    /// <summary>
    /// Gets or sets the column filter snapshots.
    /// </summary>
    public List<ColumnFilterSnapshot> ColumnFilters { get; set; } = new();

    /// <summary>
    /// Gets or sets the group definition snapshot.
    /// </summary>
    public GroupDefinitionSnapshot? GroupDefinition { get; set; }
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

/// <summary>
/// Serializable snapshot of a single column's filter state.
/// </summary>
public class ColumnFilterSnapshot
{
    /// <summary>
    /// Gets or sets the column ID.
    /// </summary>
    public string ColumnId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filter operator.
    /// </summary>
    public FilterOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the primary filter value.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the secondary filter value (for range operators).
    /// </summary>
    public object? ValueEnd { get; set; }
}

/// <summary>
/// Serializable snapshot of a group definition.
/// </summary>
public class GroupDefinitionSnapshot
{
    /// <summary>
    /// Gets or sets the column ID to group by.
    /// </summary>
    public string ColumnId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sort direction for group ordering.
    /// </summary>
    public SortDirection GroupSortDirection { get; set; }
}
