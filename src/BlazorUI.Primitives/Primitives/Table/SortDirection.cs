namespace BlazorUI.Primitives.Table;

/// <summary>
/// Defines the direction for sorting table columns.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// No sorting applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// Sort in ascending order (A to Z, 0 to 9, oldest to newest).
    /// </summary>
    Ascending = 1,

    /// <summary>
    /// Sort in descending order (Z to A, 9 to 0, newest to oldest).
    /// </summary>
    Descending = 2
}
