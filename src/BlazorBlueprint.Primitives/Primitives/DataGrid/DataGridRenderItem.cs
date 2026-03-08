using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Represents either a group header or a data row in the flattened render list.
/// Used when grouping is active to interleave group headers with data rows.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
    Justification = "Factory methods for discriminated union pattern")]
public class DataGridRenderItem<TData> where TData : class
{
    private DataGridRenderItem()
    {
    }

    /// <summary>
    /// Gets whether this render item is a group header row.
    /// </summary>
    public bool IsGroupHeader { get; private init; }

    /// <summary>
    /// Gets whether this render item is a data row.
    /// </summary>
    public bool IsDataRow { get; private init; }

    /// <summary>
    /// Gets the data item. Non-null when <see cref="IsDataRow"/> is true.
    /// </summary>
    public TData? Item { get; private init; }

    /// <summary>
    /// Gets the group row. Non-null when <see cref="IsGroupHeader"/> is true.
    /// </summary>
    public DataGridGroupRow<TData>? GroupRow { get; private init; }

    /// <summary>
    /// Creates a render item for a data row.
    /// </summary>
    public static DataGridRenderItem<TData> ForData(TData item) => new()
    {
        IsDataRow = true,
        Item = item
    };

    /// <summary>
    /// Creates a render item for a group header.
    /// </summary>
    public static DataGridRenderItem<TData> ForGroup(DataGridGroupRow<TData> group) => new()
    {
        IsGroupHeader = true,
        GroupRow = group
    };
}
