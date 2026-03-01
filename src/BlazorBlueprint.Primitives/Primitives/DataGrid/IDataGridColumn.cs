using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Defines a column in a DataGrid with type-erased access to properties and rendering.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public interface IDataGridColumn<TData> where TData : class
{
    /// <summary>
    /// Gets the unique identifier for this column.
    /// </summary>
    public string ColumnId { get; }

    /// <summary>
    /// Gets the display title for the column header.
    /// </summary>
    public string? Title { get; }

    /// <summary>
    /// Gets whether this column supports sorting.
    /// </summary>
    public bool Sortable { get; }

    /// <summary>
    /// Gets whether this column is currently visible.
    /// </summary>
    public bool Visible { get; }

    /// <summary>
    /// Gets the column width (e.g., "200px", "20%", "auto").
    /// </summary>
    public string? Width { get; }

    /// <summary>
    /// Gets whether the user can toggle this column's visibility via a column chooser.
    /// Default is true. Set to false for columns that must always be visible (e.g., selection columns).
    /// </summary>
    public bool Hideable { get; }

    /// <summary>
    /// Gets whether this column supports resizing via drag handles.
    /// Default is true. Set to false for fixed-width columns.
    /// </summary>
    public bool Resizable { get; }

    /// <summary>
    /// Gets whether this column supports reordering via drag-and-drop.
    /// Default is true. Set to false for fixed-position columns (e.g., selection columns).
    /// </summary>
    public bool Reorderable { get; }

    /// <summary>
    /// Gets whether this column is pinned to an edge of the scrollable viewport.
    /// Pinned columns use CSS position: sticky with automatically computed offsets.
    /// Default is <see cref="ColumnPinning.None"/>.
    /// </summary>
    public ColumnPinning Pinned { get; }

    /// <summary>
    /// Gets the value from a data item for this column (type-erased).
    /// </summary>
    /// <param name="item">The data item.</param>
    /// <returns>The column value, boxed as object.</returns>
    public object? GetValue(TData item);

    /// <summary>
    /// Compares two data items based on this column's values.
    /// Used for in-memory sorting.
    /// </summary>
    /// <param name="x">The first item.</param>
    /// <param name="y">The second item.</param>
    /// <returns>Negative if x &lt; y, zero if equal, positive if x &gt; y.</returns>
    public int Compare(TData x, TData y);

    /// <summary>
    /// Gets the LINQ sort expression for IQueryable sorting.
    /// Returns null if the column does not support expression-based sorting.
    /// </summary>
    public LambdaExpression? GetSortExpression();

    /// <summary>
    /// Gets the custom cell template for this column.
    /// </summary>
    public RenderFragment<DataGridCellContext<TData>>? CellTemplate { get; }

    /// <summary>
    /// Gets the custom header template for this column.
    /// </summary>
    public RenderFragment<DataGridHeaderContext<TData>>? HeaderTemplate { get; }

    /// <summary>
    /// Gets additional CSS classes for cells in this column.
    /// </summary>
    public string? CellClass { get; }

    /// <summary>
    /// Gets additional CSS classes for the header cell.
    /// </summary>
    public string? HeaderClass { get; }
}

/// <summary>
/// Context provided to DataGrid cell templates.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridCellContext<TData> where TData : class
{
    /// <summary>
    /// Gets the data item for this row.
    /// </summary>
    public required TData Item { get; init; }

    /// <summary>
    /// Gets the column being rendered.
    /// </summary>
    public required IDataGridColumn<TData> Column { get; init; }

    /// <summary>
    /// Gets the cell value (boxed).
    /// </summary>
    public required object? Value { get; init; }

    /// <summary>
    /// Gets the zero-based row index.
    /// </summary>
    public int RowIndex { get; init; }

    /// <summary>
    /// Gets the zero-based column index.
    /// </summary>
    public int ColumnIndex { get; init; }
}

/// <summary>
/// Context provided to DataGrid header templates.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridHeaderContext<TData> where TData : class
{
    /// <summary>
    /// Gets the column being rendered.
    /// </summary>
    public required IDataGridColumn<TData> Column { get; init; }

    /// <summary>
    /// Gets the current grid state.
    /// </summary>
    public required DataGridState<TData> State { get; init; }
}
