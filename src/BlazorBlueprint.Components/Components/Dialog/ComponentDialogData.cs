namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a pending custom component dialog.
/// </summary>
public sealed class ComponentDialogData(DialogService dialogService) : DialogData<DialogResult>, IDialogReference
{
    /// <summary>
    /// The component type to render.
    /// </summary>
    public required Type ComponentType { get; init; }

    /// <summary>
    /// Parameters passed to the component.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; init; } = new();

    /// <summary>
    /// Options controlling dialog appearance and behavior.
    /// </summary>
    public DialogOpenOptions Options { get; init; } = new();

    /// <inheritdoc />
    public Task CloseAsync(DialogResult result)
    {
        dialogService.Resolve(Id, result);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CancelAsync()
    {
        dialogService.Resolve(Id, DialogResult.Cancel());
        return Task.CompletedTask;
    }
}
