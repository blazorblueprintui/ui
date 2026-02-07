using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Non-generic interface for column definitions.
/// Provides type-erased access to column properties and operations.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public interface IColumnDefinition<TData> where TData : class
{
    /// <summary>
    /// Gets the unique identifier for this column.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the header text displayed for this column.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets whether this column can be sorted.
    /// </summary>
    public bool CanSort { get; }

    /// <summary>
    /// Gets whether this column is currently visible.
    /// </summary>
    public bool Visible { get; }

    /// <summary>
    /// Gets the width of the column (e.g., "200px", "20%", "auto").
    /// </summary>
    public string? Width { get; }

    /// <summary>
    /// Gets the minimum width of the column (e.g., "100px").
    /// </summary>
    public string? MinWidth { get; }

    /// <summary>
    /// Gets the maximum width of the column (e.g., "400px").
    /// </summary>
    public string? MaxWidth { get; }

    /// <summary>
    /// Gets the value from a data item for this column.
    /// </summary>
    /// <param name="item">The data item.</param>
    /// <returns>The column value, boxed as object.</returns>
    public object? GetValue(TData item);

    /// <summary>
    /// Compares two data items based on this column's values.
    /// </summary>
    /// <param name="x">The first item to compare.</param>
    /// <param name="y">The second item to compare.</param>
    /// <returns>
    /// A negative value if x &lt; y, zero if x == y, or a positive value if x &gt; y.
    /// </returns>
    public int Compare(TData x, TData y);

    /// <summary>
    /// Gets the custom header template for this column.
    /// </summary>
    public RenderFragment<HeaderContext<TData>>? HeaderTemplate { get; }

    /// <summary>
    /// Gets the custom cell template for this column.
    /// </summary>
    public RenderFragment<CellContext<TData>>? CellTemplate { get; }
}

/// <summary>
/// Context provided to header templates.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public class HeaderContext<TData> where TData : class
{
    /// <summary>
    /// Gets or sets the column being rendered.
    /// </summary>
    public required IColumnDefinition<TData> Column { get; set; }

    /// <summary>
    /// Gets or sets the current table state.
    /// </summary>
    public required TableState<TData> State { get; set; }
}

/// <summary>
/// Context provided to cell templates.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public class CellContext<TData> where TData : class
{
    /// <summary>
    /// Gets or sets the data item being rendered.
    /// </summary>
    public required TData Item { get; set; }

    /// <summary>
    /// Gets or sets the column being rendered.
    /// </summary>
    public required IColumnDefinition<TData> Column { get; set; }

    /// <summary>
    /// Gets or sets the cell value (boxed).
    /// </summary>
    public required object? Value { get; set; }

    /// <summary>
    /// Gets or sets the zero-based row index.
    /// </summary>
    public int RowIndex { get; set; }

    /// <summary>
    /// Gets or sets the zero-based column index.
    /// </summary>
    public int ColumnIndex { get; set; }
}
