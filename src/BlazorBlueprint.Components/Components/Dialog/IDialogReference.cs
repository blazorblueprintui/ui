namespace BlazorBlueprint.Components;

/// <summary>
/// Provides control over a custom component dialog instance.
/// This interface is cascaded to components opened via
/// <see cref="DialogService.OpenAsync{TComponent}(Dictionary{string, object?}, BlazorBlueprint.Components.DialogOpenOptions?)"/>.
/// </summary>
public interface IDialogReference
{
    /// <summary>
    /// Closes the dialog with a specified result.
    /// </summary>
    /// <param name="result">The result returned to the caller.</param>
    public Task CloseAsync(DialogResult result);

    /// <summary>
    /// Cancels the dialog.
    /// Equivalent to calling <see cref="CloseAsync(DialogResult)"/> with <see cref="DialogResult.Cancel"/>.
    /// </summary>
    public Task CancelAsync();
}
