namespace BlazorBlueprint.Components;

/// <summary>
/// Provides control over a custom component dialog instance.
/// This interface is cascaded to components opened via
/// <see cref="DialogService.Open{TComponent}(DialogOpenOptions?)"/>.
/// </summary>
public interface IDialogReference
{
    /// <summary>
    /// Closes the dialog with a specified result.
    /// </summary>
    /// <param name="result">The result returned to the caller.</param>
    public Task Close(DialogResult result);

    /// <summary>
    /// Cancels the dialog.
    /// Equivalent to calling <see cref="Close(DialogResult)"/> with <see cref="DialogResult.Cancel"/>.
    /// </summary>
    public Task Cancel();
}
