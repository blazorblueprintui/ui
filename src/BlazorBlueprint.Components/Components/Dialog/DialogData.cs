namespace BlazorBlueprint.Components;

/// <summary>
/// Represents the base model for all dialog instances managed by <see cref="DialogService"/>.
/// </summary>
/// <remarks>
/// This type provides shared metadata and lifecycle management for dialog instances.
/// All dialogs resolve with a <see cref="DialogResult"/>.
/// </remarks>
public abstract class DialogData
{
    private readonly TaskCompletionSource<DialogResult> tcs = new();

    /// <summary>
    /// Gets or sets the dialog description or message content.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the dialog instance.
    /// </summary>
    /// <remarks>
    /// This identifier is generated automatically and is used internally
    /// by <see cref="DialogService"/> to resolve dialog instances.
    /// </remarks>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets the task that completes when the dialog is resolved.
    /// </summary>
    /// <remarks>
    /// This task is awaited internally by <see cref="DialogService"/>.
    /// </remarks>
    internal Task<DialogResult> Completion => tcs.Task;

    /// <summary>
    /// Resolves the dialog with the specified result.
    /// </summary>
    /// <param name="result">The result supplied by the dialog renderer.</param>
    internal void SetResult(DialogResult result)
        => tcs.TrySetResult(result);
}
