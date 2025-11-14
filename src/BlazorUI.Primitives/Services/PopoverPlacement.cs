namespace BlazorUI.Primitives.Services;

/// <summary>
/// Combined placement values for simplified positioning configuration.
/// Combines both side and alignment in a single value.
/// </summary>
public enum PopoverPlacement
{
    /// <summary>
    /// Position above the trigger, centered horizontally.
    /// </summary>
    Top,

    /// <summary>
    /// Position above the trigger, aligned to the start (left).
    /// </summary>
    TopStart,

    /// <summary>
    /// Position above the trigger, aligned to the end (right).
    /// </summary>
    TopEnd,

    /// <summary>
    /// Position to the right of the trigger, centered vertically.
    /// </summary>
    Right,

    /// <summary>
    /// Position to the right of the trigger, aligned to the start (top).
    /// </summary>
    RightStart,

    /// <summary>
    /// Position to the right of the trigger, aligned to the end (bottom).
    /// </summary>
    RightEnd,

    /// <summary>
    /// Position below the trigger, centered horizontally.
    /// </summary>
    Bottom,

    /// <summary>
    /// Position below the trigger, aligned to the start (left).
    /// </summary>
    BottomStart,

    /// <summary>
    /// Position below the trigger, aligned to the end (right).
    /// </summary>
    BottomEnd,

    /// <summary>
    /// Position to the left of the trigger, centered vertically.
    /// </summary>
    Left,

    /// <summary>
    /// Position to the left of the trigger, aligned to the start (top).
    /// </summary>
    LeftStart,

    /// <summary>
    /// Position to the left of the trigger, aligned to the end (bottom).
    /// </summary>
    LeftEnd
}
