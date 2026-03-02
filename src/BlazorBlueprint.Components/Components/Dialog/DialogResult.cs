namespace BlazorBlueprint.Components;

/// <summary>
/// Represents the result of a custom component dialog opened via
/// <see cref="DialogService.Open{TComponent}(DialogOpenOptions?)"/>.
/// </summary>
public sealed class DialogResult
{
    private DialogResult(bool cancelled, object? data)
    {
        Cancelled = cancelled;
        Data = data;
    }

    /// <summary>
    /// Gets whether the dialog was cancelled.
    /// </summary>
    public bool Cancelled { get; }

    /// <summary>
    /// Gets optional data returned by the dialog.
    /// </summary>
    public object? Data { get; }

    /// <summary>
    /// Creates a cancelled result.
    /// </summary>
    public static DialogResult Cancel() => new(true, null);

    /// <summary>
    /// Creates a successful result with optional data.
    /// </summary>
    public static DialogResult Ok(object? data = null) => new(false, data);

    /// <summary>
    /// Attempts to retrieve the dialog result data as a strongly-typed value.
    /// Returns default if the data is not of the requested type.
    /// </summary>
    /// <typeparam name="T">Expected result type.</typeparam>
    public T? GetData<T>() => Data is T typed ? typed : default;
}
