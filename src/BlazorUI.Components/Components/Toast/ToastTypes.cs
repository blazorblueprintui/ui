namespace BlazorUI.Components.Toast;

/// <summary>
/// Options for displaying a toast notification.
/// </summary>
public class ToastOptions
{
    /// <summary>
    /// The unique identifier for the toast.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The title of the toast.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The description/content of the toast.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The variant/style of the toast.
    /// </summary>
    public ToastVariant Variant { get; set; } = ToastVariant.Default;

    /// <summary>
    /// How long the toast should be visible. Null means indefinite.
    /// </summary>
    public TimeSpan? Duration { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Action button label.
    /// </summary>
    public string? ActionLabel { get; set; }

    /// <summary>
    /// Callback when action button is clicked.
    /// </summary>
    public Action? OnAction { get; set; }

    /// <summary>
    /// Callback when the toast is dismissed.
    /// </summary>
    public Action? OnDismiss { get; set; }
}

/// <summary>
/// Variant/style of a toast notification.
/// </summary>
public enum ToastVariant
{
    /// <summary>
    /// Default/neutral toast.
    /// </summary>
    Default,

    /// <summary>
    /// Success toast (green).
    /// </summary>
    Success,

    /// <summary>
    /// Warning toast (yellow/orange).
    /// </summary>
    Warning,

    /// <summary>
    /// Error/destructive toast (red).
    /// </summary>
    Destructive,

    /// <summary>
    /// Info toast (blue).
    /// </summary>
    Info
}

/// <summary>
/// Position of the toast viewport.
/// </summary>
public enum ToastPosition
{
    /// <summary>
    /// Top left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Top center.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Top right corner.
    /// </summary>
    TopRight,

    /// <summary>
    /// Bottom left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Bottom center.
    /// </summary>
    BottomCenter,

    /// <summary>
    /// Bottom right corner.
    /// </summary>
    BottomRight
}
