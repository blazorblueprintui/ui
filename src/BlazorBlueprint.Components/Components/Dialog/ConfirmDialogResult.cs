namespace BlazorBlueprint.Components;

/// <summary>
/// Represents the result of a confirm dialog interaction.
/// </summary>
/// <remarks>
/// A confirm dialog presents confirm and cancel actions to the user.
/// This type provides a strongly typed abstraction over
/// <see cref="DialogResult"/> for convenience.
/// </remarks>
public sealed class ConfirmDialogResult : DialogResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmDialogResult"/> class.
    /// </summary>
    /// <param name="result">The underlying dialog result.</param>
    internal ConfirmDialogResult(DialogResult result)
        : base(result.Cancelled, result.Data)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the user confirmed the dialog.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the dialog was confirmed;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool Confirmed => !Cancelled;
}
