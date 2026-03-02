namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a pending confirm dialog managed by <see cref="DialogService"/>.
/// </summary>
public class ConfirmDialogData : DialogData<bool>
{
    /// <summary>
    /// Customization options for the dialog.
    /// </summary>
    public ConfirmDialogOptions Options { get; set; } = new();
}
