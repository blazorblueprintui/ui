namespace BlazorBlueprint.Components;

/// <summary>
/// Identifies a region of a dock target. Used both for the initial placement of a
/// <see cref="BbDockPanel"/> and for the drop zone resolved while dragging a tab.
/// </summary>
public enum DockZone
{
    /// <summary>
    /// The center of the target. Dropping here adds the panel as a tab to the target group.
    /// </summary>
    Center,

    /// <summary>
    /// The left edge of the target. Dropping here creates a horizontal split with the panel on the left.
    /// </summary>
    Left,

    /// <summary>
    /// The right edge of the target. Dropping here creates a horizontal split with the panel on the right.
    /// </summary>
    Right,

    /// <summary>
    /// The top edge of the target. Dropping here creates a vertical split with the panel on top.
    /// </summary>
    Top,

    /// <summary>
    /// The bottom edge of the target. Dropping here creates a vertical split with the panel on the bottom.
    /// </summary>
    Bottom
}
