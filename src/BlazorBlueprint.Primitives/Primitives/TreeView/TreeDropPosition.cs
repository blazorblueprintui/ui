namespace BlazorBlueprint.Primitives;

/// <summary>
/// Position where a dragged tree node is dropped relative to the target node.
/// </summary>
public enum TreeDropPosition
{
    /// <summary>
    /// Drop before the target node (reorder as previous sibling).
    /// </summary>
    Before,

    /// <summary>
    /// Drop after the target node (reorder as next sibling).
    /// </summary>
    After,

    /// <summary>
    /// Drop inside the target node (reparent as child).
    /// </summary>
    Inside
}
