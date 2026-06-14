namespace BlazorBlueprint.Components;

/// <summary>
/// Controls how the select-all header checkbox of a <see cref="BbDataGridSelectColumn{TData}"/>
/// behaves when the grid is paginated.
/// </summary>
public enum DataGridSelectAllScope
{
    /// <summary>
    /// The default. When more than one page of data exists, clicking the header checkbox opens a
    /// menu offering "select all on this page" and "select all items" (across every page).
    /// </summary>
    Prompt,

    /// <summary>
    /// No menu is shown. The header checkbox toggles only the rows on the current page, leaving any
    /// selections on other pages untouched.
    /// </summary>
    CurrentPage
}
