namespace BlazorUI.Components.DataTable;

/// <summary>
/// Specifies the row selection behavior for a data table.
/// </summary>
public enum DataTableSelectionMode
{
    /// <summary>
    /// No row selection is allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Only a single row can be selected at a time.
    /// Selecting a new row deselects the previously selected row.
    /// </summary>
    Single = 1,

    /// <summary>
    /// Multiple rows can be selected simultaneously using checkboxes.
    /// Includes a select-all checkbox in the header.
    /// </summary>
    Multiple = 2
}
