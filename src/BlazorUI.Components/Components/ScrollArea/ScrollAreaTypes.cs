namespace BlazorUI.Components.ScrollArea;

/// <summary>
/// Type of scrollbar behavior in a ScrollArea.
/// </summary>
public enum ScrollAreaType
{
    /// <summary>
    /// Scrollbars are visible when content overflows.
    /// </summary>
    Auto,

    /// <summary>
    /// Scrollbars are always visible.
    /// </summary>
    Always,

    /// <summary>
    /// Scrollbars appear only when scrolling.
    /// </summary>
    Scroll,

    /// <summary>
    /// Scrollbars appear only on hover.
    /// </summary>
    Hover
}

/// <summary>
/// Orientation of a scrollbar.
/// </summary>
public enum Orientation
{
    /// <summary>
    /// Vertical scrollbar.
    /// </summary>
    Vertical,

    /// <summary>
    /// Horizontal scrollbar.
    /// </summary>
    Horizontal
}
