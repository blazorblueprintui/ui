namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a pending alert dialog managed by <see cref="DialogService"/>.
/// </summary>
/// <remarks>
/// An alert dialog presents a message with a single acknowledgment button.
/// It does not return a value and completes when the user dismisses the dialog.
/// </remarks>
public sealed class AlertDialogData : DialogData
{
    private readonly TaskCompletionSource tcs = new();

    /// <summary>
    /// Gets or sets the customization options for the alert dialog.
    /// </summary>
    public AlertDialogOptions Options { get; set; } = new();

    /// <summary>
    /// Gets the task that completes when the dialog is acknowledged.
    /// </summary>
    internal override Task Completion => tcs.Task;

    /// <summary>
    /// Completes the dialog task.
    /// </summary>
    /// <param name="result">
    /// Ignored. Alert dialogs do not produce a result value.
    /// </param>
    internal override void SetResult(object? result)
        => tcs.TrySetResult();
}
