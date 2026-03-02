namespace BlazorBlueprint.Components;

/// <summary>
/// Represents the result of a dialog interaction.
/// </summary>
/// <remarks>
/// A dialog result indicates whether the dialog was cancelled and may
/// optionally carry additional data supplied by the dialog.
/// </remarks>
public class DialogResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogResult"/> class.
    /// </summary>
    /// <param name="cancelled">
    /// <see langword="true"/> if the dialog was cancelled; otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="data">
    /// Optional data returned by the dialog.
    /// </param>
    protected DialogResult(bool cancelled, object? data = null)
    {
        Cancelled = cancelled;
        Data = data;
    }

    /// <summary>
    /// Gets a value indicating whether the dialog was cancelled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the user dismissed or cancelled the dialog;
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool Cancelled { get; }

    /// <summary>
    /// Gets the raw data returned by the dialog.
    /// </summary>
    /// <remarks>
    /// The concrete data type depends on the dialog implementation.
    /// This value is intended for internal use. Consumers should prefer
    /// strongly typed wrappers or <see cref="GetData{T}"/>.
    /// </remarks>
    internal object? Data { get; }

    /// <summary>
    /// Attempts to retrieve the dialog data as the specified type.
    /// </summary>
    /// <typeparam name="T">The expected data type.</typeparam>
    /// <returns>
    /// The data cast to <typeparamref name="T"/> if compatible;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </returns>
    public T? GetData<T>()
        => Data is T typed ? typed : default;

    /// <summary>
    /// Creates a cancelled dialog result.
    /// </summary>
    /// <returns>A <see cref="DialogResult"/> representing a cancelled dialog.</returns>
    public static DialogResult Cancel()
        => new(true);

    /// <summary>
    /// Creates a successful dialog result.
    /// </summary>
    /// <param name="data">Optional data returned by the dialog.</param>
    /// <returns>A <see cref="DialogResult"/> representing a successful dialog.</returns>
    public static DialogResult Ok(object? data = null)
        => new(false, data);
}
