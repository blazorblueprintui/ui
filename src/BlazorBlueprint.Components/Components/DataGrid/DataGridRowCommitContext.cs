namespace BlazorBlueprint.Components;

/// <summary>
/// Passed to a DataGrid's row-commit callback, carrying the edited item and letting the handler
/// keep the row in edit.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridRowCommitContext<TData> where TData : class
{
    /// <summary>
    /// Gets the edited model: the original record in Row mode, or an isolated draft in Cell mode.
    /// </summary>
    public required TData Item { get; init; }

    /// <summary>
    /// Gets the unmodified source record in buffered cell mode; null in legacy row mode.
    /// In cell mode Item is a draft and is applied only after this callback accepts it.
    /// </summary>
    public TData? OriginalItem { get; init; }

    /// <summary>
    /// Gets or sets whether to keep the row in edit instead of closing it.
    /// </summary>
    /// <remarks>
    /// Set this when the save fails — a server rejection, a conflict, a rule the grid cannot
    /// check — so the user keeps their typing and can correct it. Row mode keeps values on the original item; Cell mode retains the isolated draft.
    /// </remarks>
    public bool Cancel { get; set; }
}
