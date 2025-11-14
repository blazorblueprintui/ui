namespace BlazorUI.Primitives.Services;

/// <summary>
/// Defines the alignment of a positioned element relative to its trigger.
/// Alignment is perpendicular to the side placement.
/// </summary>
public enum PopoverAlign
{
    /// <summary>
    /// Align to the start (left for vertical sides, top for horizontal sides).
    /// </summary>
    Start,

    /// <summary>
    /// Align to the center of the trigger element.
    /// </summary>
    Center,

    /// <summary>
    /// Align to the end (right for vertical sides, bottom for horizontal sides).
    /// </summary>
    End
}
