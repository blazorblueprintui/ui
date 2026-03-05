using BlazorBlueprint.Primitives.DataGrid;

namespace BlazorBlueprint.Components;

/// <summary>
/// Context provided to the <see cref="BbDataGrid{TData}.RowContextMenu"/> render fragment.
/// Gives access to the right-clicked item, selected items, visible columns, and a clipboard helper.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridRowMenuContext<TData> where TData : class
{
    /// <summary>
    /// The data item for the right-clicked row.
    /// </summary>
    public required TData Item { get; init; }

    /// <summary>
    /// The currently selected items. Empty when selection mode is None.
    /// </summary>
    public required IReadOnlyCollection<TData> SelectedItems { get; init; }

    /// <summary>
    /// The currently visible data columns (excludes select/expand columns), in display order.
    /// Use with <see cref="GetVisibleValues"/> to build copy strings that respect column visibility.
    /// </summary>
    public required IReadOnlyList<IDataGridColumn<TData>> VisibleColumns { get; init; }

    /// <summary>
    /// Copies the specified text to the system clipboard.
    /// Returns true on success, false on failure.
    /// </summary>
    public required Func<string, Task<bool>> CopyToClipboardAsync { get; init; }

    /// <summary>
    /// Returns the visible column values for an item, in display order.
    /// Respects column visibility and ordering — hidden columns are excluded.
    /// </summary>
    public IEnumerable<string> GetVisibleValues(TData item)
    {
        foreach (var col in VisibleColumns)
        {
            yield return col.GetValue(item)?.ToString() ?? "";
        }
    }

    /// <summary>
    /// Formats a single row's visible column values as a delimiter-separated string.
    /// </summary>
    public string FormatRow(TData item, string separator = ",") =>
        string.Join(separator, GetVisibleValues(item));

    /// <summary>
    /// Formats multiple rows' visible column values, one row per line.
    /// </summary>
    public string FormatRows(IEnumerable<TData> items, string separator = ",") =>
        string.Join("\n", items.Select(item => FormatRow(item, separator)));
}
