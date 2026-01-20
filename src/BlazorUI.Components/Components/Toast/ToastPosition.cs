namespace BlazorUI.Components.Toast;

/// <summary>
/// Defines the position where toasts are displayed on the screen.
/// </summary>
public enum ToastPosition
{
    /// <summary>
    /// Top-right corner of the viewport.
    /// </summary>
    TopRight,

    /// <summary>
    /// Top-left corner of the viewport.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Top-center of the viewport.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Bottom-right corner of the viewport (default).
    /// </summary>
    BottomRight,

    /// <summary>
    /// Bottom-left corner of the viewport.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Bottom-center of the viewport.
    /// </summary>
    BottomCenter
}
