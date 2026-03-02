namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a pending prompt dialog that returns a string value.
/// </summary>
/// <remarks>
/// A prompt dialog displays a text input field and allows the user
/// to submit a string value or cancel the dialog.
/// The submitted value is available via <see cref="DialogResult.Data"/>.
/// </remarks>
public sealed class PromptDialogData : DialogData
{
    /// <summary>
    /// Gets or sets the customization options for the prompt dialog.
    /// </summary>
    public PromptDialogOptions Options { get; set; } = new();
}
