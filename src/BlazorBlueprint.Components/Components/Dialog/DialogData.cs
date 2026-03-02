namespace BlazorBlueprint.Components;

/// <summary>
/// Represents the base model for all dialog instances managed by <see cref="DialogService"/>.
/// </summary>
/// <remarks>
/// This type provides shared metadata and lifecycle management for dialog instances.
/// Concrete dialog implementations either return a typed result via
/// <see cref="DialogData{TResult}"/> or complete without a result.
/// </remarks>
public abstract class DialogData
{
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
    /// Gets or sets the dialog description or message content.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the task that completes when the dialog is resolved.
    /// </summary>
    /// <remarks>
    /// This task is awaited internally by <see cref="DialogService"/>.
    /// </remarks>
    internal abstract Task Completion { get; }

    /// <summary>
    /// Resolves the dialog with the specified result.
    /// </summary>
    /// <param name="result">
    /// The result value supplied by the dialog renderer.
    /// The expected type depends on the concrete dialog implementation.
    /// </param>
    internal abstract void SetResult(object? result);
}

/// <summary>
/// Represents a dialog that produces a strongly typed result when completed.
/// </summary>
/// <typeparam name="TResult">
/// The type of value returned when the dialog is resolved.
/// </typeparam>
/// <remarks>
/// This base type is used for dialogs such as confirmations, prompts,
/// and custom component dialogs that return data to the caller.
/// </remarks>
public abstract class DialogData<TResult> : DialogData
{
    /// <summary>
    /// Gets the underlying <see cref="TaskCompletionSource{TResult}"/>
    /// used to complete the dialog.
    /// </summary>
    internal TaskCompletionSource<TResult> Tcs { get; } = new();

    /// <summary>
    /// Gets the task that completes when the dialog is resolved.
    /// </summary>
    internal override Task Completion => Tcs.Task;

    /// <summary>
    /// Resolves the dialog with the specified result.
    /// </summary>
    /// <param name="result">
    /// The value to assign to the dialog result.
    /// If the value cannot be cast to <typeparamref name="TResult"/>,
    /// the default value of <typeparamref name="TResult"/> is used.
    /// </param>
    internal override void SetResult(object? result)
    {
        if (result is TResult typed)
        {
            Tcs.TrySetResult(typed);
        }
        else
        {
            Tcs.TrySetResult(default!);
        }
    }
}
