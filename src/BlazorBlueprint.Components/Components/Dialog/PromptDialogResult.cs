namespace BlazorBlueprint.Components;

/// <summary>
/// Represents the result of a prompt dialog interaction.
/// </summary>
/// <remarks>
/// A prompt dialog allows the user to enter text input.
/// When confirmed, the entered value is exposed via <see cref="Value"/>.
/// </remarks>
public sealed class PromptDialogResult : DialogResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PromptDialogResult"/> class.
    /// </summary>
    /// <param name="result">The underlying dialog result.</param>
    internal PromptDialogResult(DialogResult result)
        : base(result.Cancelled, result.Data)
    {
    }

    /// <summary>
    /// Gets the value entered by the user.
    /// </summary>
    /// <value>
    /// The submitted string when the dialog was confirmed;
    /// otherwise, <see langword="null"/>.
    /// </value>
    public string? Value => Cancelled ? null : GetData<string>();
}
