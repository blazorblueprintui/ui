namespace BlazorBlueprint.Components;

/// <summary>
/// Passed to a DataGrid's row-commit callback, carrying the edited item and letting the handler
/// keep the row in edit.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridRowCommitContext<TData> where TData : class
{
    /// <summary>
    /// Gets the item that was edited, with the user's changes already applied to it.
    /// </summary>
    public required TData Item { get; init; }

    /// <summary>
    /// Gets or sets whether to keep the row in edit instead of closing it.
    /// </summary>
    /// <remarks>
    /// Set this when the save fails — a server rejection, a conflict, a rule the grid cannot
    /// check — so the user keeps their typing and can correct it. The values stay on the item
    /// either way; this only decides whether the row closes.
    /// </remarks>
    public bool Cancel { get; set; }
}
