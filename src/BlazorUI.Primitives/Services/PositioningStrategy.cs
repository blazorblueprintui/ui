namespace BlazorUI.Primitives.Services;

/// <summary>
/// Defines the CSS positioning strategy for floating elements.
/// </summary>
public enum PositioningStrategy
{
    /// <summary>
    /// Position relative to the nearest positioned ancestor (position: absolute).
    /// The element is positioned relative to its offset parent.
    /// </summary>
    Absolute,

    /// <summary>
    /// Position relative to the viewport (position: fixed).
    /// The element remains in the same position even when scrolling.
    /// </summary>
    Fixed
}
