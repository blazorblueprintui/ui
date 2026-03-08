namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a pending confirm dialog managed by <see cref="DialogService"/>.
/// </summary>
/// <remarks>
/// A confirm dialog presents confirm and cancel actions.
/// The result indicates whether the user confirmed the action.
/// </remarks>
public sealed class ConfirmDialogData : DialogData
{
    /// <summary>
    /// Gets or sets customization options for the dialog.
    /// </summary>
    public ConfirmDialogOptions Options { get; set; } = new();
}
