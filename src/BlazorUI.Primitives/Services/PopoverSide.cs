namespace BlazorUI.Primitives.Services;

/// <summary>
/// Defines the side where a positioned element appears relative to its trigger.
/// Used by tooltips, popovers, dropdowns, and other floating content.
/// </summary>
public enum PopoverSide
{
    /// <summary>
    /// Position above the trigger element.
    /// </summary>
    Top,

    /// <summary>
    /// Position to the right of the trigger element.
    /// </summary>
    Right,

    /// <summary>
    /// Position below the trigger element.
    /// </summary>
    Bottom,

    /// <summary>
    /// Position to the left of the trigger element.
    /// </summary>
    Left
}
