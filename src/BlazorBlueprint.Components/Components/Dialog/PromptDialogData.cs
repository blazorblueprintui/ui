namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a pending prompt dialog that returns a string value.
/// </summary>
/// <remarks>
/// A prompt dialog displays a text input field and allows the user
/// to submit a string value or cancel the dialog.
/// </remarks>
public class PromptDialogData : DialogData<string?>
{
    /// <summary>
    /// Gets or sets the customization options for the prompt dialog.
    /// </summary>
    public PromptDialogOptions Options { get; set; } = new();
}
