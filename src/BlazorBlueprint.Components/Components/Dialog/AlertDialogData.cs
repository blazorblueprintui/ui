namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a pending alert dialog managed by <see cref="DialogService"/>.
/// </summary>
/// <remarks>
/// An alert dialog presents a message with a single acknowledgment button.
/// It resolves with a confirmed <see cref="DialogResult"/> when dismissed.
/// </remarks>
public sealed class AlertDialogData : DialogData
{
    /// <summary>
    /// Gets or sets the customization options for the alert dialog.
    /// </summary>
    public AlertDialogOptions Options { get; set; } = new();
}
