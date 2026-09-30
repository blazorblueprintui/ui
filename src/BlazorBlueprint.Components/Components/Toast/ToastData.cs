using Microsoft.AspNetCore.Components;

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
    /// Optional content rendered below the title and description: any markup or components.
    /// </summary>
    /// <remarks>
    /// Leave <see cref="Title"/> and <see cref="Description"/> empty to lay out the whole body
    /// yourself. For more than a line or two, write a component and show it with
    /// <see cref="ToastService.Show{TComponent}(Dictionary{string, object?}?, ToastData?)"/>.
    /// The fragment is rendered by the toast provider, not by the component that created it, so
    /// it does not re-render when that component's state changes.
    /// </remarks>
    public RenderFragment? Content { get; set; }

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
    /// Optional custom icon, such as an avatar or image, shown in place of the variant icon.
    /// </summary>
    /// <remarks>
    /// Shows for every variant, <see cref="ToastVariant.Default"/> included.
    /// <see cref="ShowIcon"/> set to false still hides it.
    /// </remarks>
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Duration in milliseconds before auto-dismiss.
    /// Set to 0 for no auto-dismiss. Default is 5000ms (5 seconds).
    /// </summary>
    /// <remarks>
    /// A toast with <see cref="Content"/> or <see cref="Actions"/> stops counting down for good
    /// once the user presses or moves focus inside it, so a menu opened from it can't be dismissed
    /// out from under them.
    /// </remarks>
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
    /// Optional buttons or other components shown in a row below the toast's content.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="ActionText"/>, clicking inside the row does not dismiss the toast;
    /// call <see cref="ToastService.Dismiss(string)"/> with <see cref="Id"/> when an action is done.
    /// </remarks>
    public RenderFragment? Actions { get; set; }

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
