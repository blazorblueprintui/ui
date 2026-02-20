namespace BlazorBlueprint.Components;

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
    /// The size variant of the toast. Compact reduces padding and font sizes.
    /// </summary>
    public ToastSize Size { get; set; } = ToastSize.Default;

    /// <summary>
    /// Optional position override for this specific toast.
    /// When null (default), uses the provider's position.
    /// </summary>
    public ToastPosition? Position { get; set; }

    /// <summary>
    /// Whether to show the variant-specific icon. Default variant has no icon regardless.
    /// </summary>
    public bool ShowIcon { get; set; } = true;

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
    /// Whether to show a visual countdown progress bar at the bottom of the toast.
    /// When null, uses the BbToastProvider's ShowCountdown setting.
    /// </summary>
    public bool? ShowCountdown { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the toast element.
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// Timestamp when the toast was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
