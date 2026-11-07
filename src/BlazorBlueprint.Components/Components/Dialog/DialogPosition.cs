namespace BlazorBlueprint.Components;

/// <summary>
/// Defines where a dialog sits in the viewport.
/// Used by <see cref="BbDialog.Position"/> and <see cref="DialogOpenOptions.Position"/>.
/// </summary>
public enum DialogPosition
{
    /// <summary>
    /// Centred in the viewport (default).
    /// </summary>
    Center,

    /// <summary>
    /// Top edge, centred horizontally.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Bottom edge, centred horizontally.
    /// </summary>
    BottomCenter,

    /// <summary>
    /// Left edge, centred vertically.
    /// </summary>
    CenterLeft,

    /// <summary>
    /// Right edge, centred vertically.
    /// </summary>
    CenterRight,

    /// <summary>
    /// Top-left corner of the viewport.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Top-right corner of the viewport.
    /// </summary>
    TopRight,

    /// <summary>
    /// Bottom-left corner of the viewport.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Bottom-right corner of the viewport.
    /// </summary>
    BottomRight
}
