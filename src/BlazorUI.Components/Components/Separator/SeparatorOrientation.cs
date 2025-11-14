namespace BlazorUI.Components.Separator;

/// <summary>
/// Defines the orientation of a Separator component.
/// </summary>
/// <remarks>
/// Controls whether the separator is displayed as a horizontal divider (spanning the width)
/// or a vertical divider (spanning the height). This affects both the visual appearance
/// and the aria-orientation attribute for screen readers.
/// </remarks>
public enum SeparatorOrientation
{
    /// <summary>
    /// Horizontal separator that spans the full width.
    /// Creates a horizontal line divider between vertically stacked content.
    /// CSS: h-[1px] w-full
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical separator that spans the full height.
    /// Creates a vertical line divider between horizontally arranged content.
    /// CSS: h-full w-[1px]
    /// </summary>
    Vertical
}
