namespace BlazorBlueprint.Components.Toast;

/// <summary>
/// Represents the data for a single toast notification.
/// </summary>
public class ToastData
{
    /// <summary>
    /// Unique identifier for the toast.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The title of the toast.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The description/message of the toast.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The visual variant of the toast.
    /// </summary>
    public ToastVariant Variant { get; set; } = ToastVariant.Default;

    /// <summary>
    /// Duration in milliseconds before auto-dismiss.
    /// Set to 0 for no auto-dismiss. Default is 5000ms (5 seconds).
    /// </summary>
    public int Duration { get; set; } = 5000;

    /// <summary>
    /// Whether to show a close button.
    /// </summary>
    public bool ShowClose { get; set; } = true;

    /// <summary>
    /// Optional action button text.
    /// </summary>
    public string? ActionText { get; set; }

    /// <summary>
    /// Optional action button callback.
    /// </summary>
    public Action? OnAction { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the toast element.
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// Timestamp when the toast was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
