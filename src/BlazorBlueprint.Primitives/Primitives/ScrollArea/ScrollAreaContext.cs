namespace BlazorBlueprint.Primitives.ScrollArea;

/// <summary>
/// Shared state between ScrollArea sub-components.
/// </summary>
public class ScrollAreaContext
{
    /// <summary>
    /// Gets the unique ID for the viewport element.
    /// </summary>
    public string ViewportId { get; } = $"bb-scrollarea-viewport-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the unique ID for the vertical scrollbar element.
    /// </summary>
    public string ScrollbarVerticalId { get; } = $"bb-scrollarea-scrollbar-v-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the unique ID for the horizontal scrollbar element.
    /// </summary>
    public string ScrollbarHorizontalId { get; } = $"bb-scrollarea-scrollbar-h-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the orientation of the scroll area.
    /// </summary>
    public ScrollAreaOrientation Orientation { get; init; }
}
