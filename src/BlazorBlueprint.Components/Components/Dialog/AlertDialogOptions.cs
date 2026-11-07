namespace BlazorBlueprint.Components;

/// <summary>
/// Options for customizing an alert dialog shown via <see cref="DialogService"/>.
/// </summary>
public sealed class AlertDialogOptions
{
    /// <summary>
    /// The label for the acknowledgment button.
    /// When null, uses IBbLocalizer with key "AlertDialog.Button" ("OK").
    /// </summary>
    public string? ButtonText { get; set; }
}
