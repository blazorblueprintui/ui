namespace BlazorUI.Primitives.Table;

/// <summary>
/// Defines the selection behavior for table rows.
/// </summary>
public enum SelectionMode
{
    /// <summary>
    /// No row selection allowed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Only one row can be selected at a time.
    /// </summary>
    Single = 1,

    /// <summary>
    /// Multiple rows can be selected simultaneously.
    /// </summary>
    Multiple = 2
}
