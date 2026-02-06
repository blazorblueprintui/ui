using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Services;

/// <summary>
/// Service for positioning floating elements relative to reference elements using Floating UI.
/// </summary>
public interface IPositioningService
{
    /// <summary>
    /// Computes the optimal position for a floating element relative to a reference element.
    /// </summary>
    /// <param name="reference">The reference element to position relative to.</param>
    /// <param name="floating">The floating element to be positioned.</param>
    /// <param name="options">Positioning options.</param>
    /// <returns>Position data including x, y coordinates.</returns>
    public Task<PositionResult> ComputePositionAsync(
        ElementReference reference,
        ElementReference floating,
        PositioningOptions? options = null);

    /// <summary>
    /// Applies the computed position to a floating element.
    /// </summary>
    /// <param name="floating">The element to position.</param>
    /// <param name="position">The position data to apply.</param>
    /// <param name="makeVisible">Whether to make the element visible after positioning.</param>
    public Task ApplyPositionAsync(ElementReference floating, PositionResult position, bool makeVisible = false);

    /// <summary>
    /// Sets up auto-update for dynamic positioning (e.g., on scroll/resize).
    /// </summary>
    /// <param name="reference">The reference element.</param>
    /// <param name="floating">The floating element.</param>
    /// <param name="options">Positioning options.</param>
    /// <returns>A disposable handle to clean up auto-update.</returns>
    public Task<IAsyncDisposable> AutoUpdateAsync(
        ElementReference reference,
        ElementReference floating,
        PositioningOptions? options = null);
}

/// <summary>
/// Options for positioning a floating element.
/// </summary>
public class PositioningOptions
{
    /// <summary>
    /// Preferred placement of the floating element.
    /// Values: "top", "top-start", "top-end", "bottom", "bottom-start", "bottom-end",
    /// "left", "left-start", "left-end", "right", "right-start", "right-end"
    /// </summary>
    public string Placement { get; set; } = "bottom";

    /// <summary>
    /// Offset from the reference element in pixels.
    /// </summary>
    public int Offset { get; set; } = 8;

    /// <summary>
    /// Whether to flip to opposite side if there's not enough space.
    /// </summary>
    public bool Flip { get; set; } = true;

    /// <summary>
    /// Whether to shift along the axis to keep element in view.
    /// </summary>
    public bool Shift { get; set; } = true;

    /// <summary>
    /// Padding from viewport edges in pixels.
    /// </summary>
    public int Padding { get; set; } = 8;

    /// <summary>
    /// Positioning strategy: "absolute" or "fixed".
    /// Absolute positions relative to offset parent, fixed relative to viewport.
    /// </summary>
    public string Strategy { get; set; } = "absolute";

    /// <summary>
    /// Whether to match the floating element width to the reference element width.
    /// </summary>
    public bool MatchReferenceWidth { get; set; }
}

/// <summary>
/// Result of position computation.
/// </summary>
public class PositionResult
{
    /// <summary>
    /// X coordinate for the floating element.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate for the floating element.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Final placement after adjustments.
    /// </summary>
    public string Placement { get; set; } = "bottom";

    /// <summary>
    /// CSS transform-origin for animations.
    /// </summary>
    public string? TransformOrigin { get; set; }

    /// <summary>
    /// Positioning strategy used: "absolute" or "fixed".
    /// </summary>
    public string Strategy { get; set; } = "absolute";
}
