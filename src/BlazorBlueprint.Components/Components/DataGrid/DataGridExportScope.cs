namespace BlazorBlueprint.Components;

/// <summary>
/// Defines which rows a DataGrid export writes.
/// </summary>
public enum DataGridExportScope
{
    /// <summary>
    /// Every row that survives the current filters, search and sort, across all pages.
    /// This is the default, because exporting only the page on screen is the usual complaint
    /// about grid export.
    /// </summary>
    /// <remarks>
    /// A grid backed by an <c>ItemsProvider</c> only holds the page it fetched, so it exports
    /// that page and logs a warning. Fetch the full set yourself and export it if you need more.
    /// </remarks>
    FilteredRows,

    /// <summary>
    /// Only the rows on the page currently on screen.
    /// </summary>
    CurrentPage
}
