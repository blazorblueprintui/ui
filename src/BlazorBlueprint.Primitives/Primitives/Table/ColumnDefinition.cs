using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Defines a table column with type-safe data access and rendering configuration.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
/// <typeparam name="TValue">The type of the column's value.</typeparam>
public class ColumnDefinition<TData, TValue> : IColumnDefinition<TData> where TData : class
{
    /// <summary>
    /// Gets or sets the unique identifier for this column.
    /// Used for sorting, filtering, and visibility management.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the header text displayed for this column.
    /// </summary>
    public required string Header { get; set; }

    /// <summary>
    /// Gets or sets the function that extracts the column value from a data item.
    /// This enables type-safe data access.
    /// </summary>
    public required Func<TData, TValue> Accessor { get; set; }

    /// <summary>
    /// Gets or sets whether this column can be sorted.
    /// Default is true.
    /// </summary>
    public bool CanSort { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this column is currently visible.
    /// Default is true.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the width of the column (e.g., "200px", "20%", "auto").
    /// Null means the column will size automatically.
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the minimum width of the column (e.g., "100px").
    /// Useful for responsive layouts.
    /// </summary>
    public string? MinWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum width of the column (e.g., "400px").
    /// Useful for preventing excessively wide columns.
    /// </summary>
    public string? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets a custom comparer for sorting this column.
    /// If null, uses the default comparer for TValue.
    /// </summary>
    public IComparer<TValue>? CustomComparer { get; set; }

    /// <summary>
    /// Gets or sets a custom template for rendering the column header.
    /// If null, the Header text is displayed.
    /// </summary>
    public RenderFragment<HeaderContext<TData>>? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets a custom template for rendering cell values.
    /// If null, the value is rendered using ToString().
    /// </summary>
    public RenderFragment<CellContext<TData>>? CellTemplate { get; set; }

    /// <summary>
    /// Gets the value from a data item for this column (type-erased).
    /// </summary>
    /// <param name="item">The data item.</param>
    /// <returns>The column value, boxed as object.</returns>
    public object? GetValue(TData item) => Accessor(item);

    /// <summary>
    /// Gets the typed value from a data item for this column.
    /// </summary>
    /// <param name="item">The data item.</param>
    /// <returns>The column value with its original type.</returns>
    public TValue GetTypedValue(TData item) => Accessor(item);

    /// <summary>
    /// Compares two data items based on this column's values.
    /// Uses CustomComparer if provided, otherwise uses the default comparer for TValue.
    /// </summary>
    /// <param name="x">The first item to compare.</param>
    /// <param name="y">The second item to compare.</param>
    /// <returns>
    /// A negative value if x &lt; y, zero if x == y, or a positive value if x &gt; y.
    /// </returns>
    public int Compare(TData x, TData y)
    {
        var xValue = Accessor(x);
        var yValue = Accessor(y);

        // Use custom comparer if provided
        if (CustomComparer != null)
        {
            return CustomComparer.Compare(xValue, yValue);
        }

        // Handle null values
        if (xValue == null && yValue == null)
        {
            return 0;
        }

        if (xValue == null)
        {
            return -1;
        }

        if (yValue == null)
        {
            return 1;
        }


        // Use default comparer
        return Comparer<TValue>.Default.Compare(xValue, yValue);
    }

    /// <summary>
    /// Creates a cell context for rendering with this column.
    /// </summary>
    /// <param name="item">The data item.</param>
    /// <param name="rowIndex">The zero-based row index.</param>
    /// <param name="columnIndex">The zero-based column index.</param>
    /// <returns>A cell context with the column value and metadata.</returns>
    public CellContext<TData> CreateCellContext(TData item, int rowIndex, int columnIndex)
    {
        return new CellContext<TData>
        {
            Item = item,
            Column = this,
            Value = GetValue(item),
            RowIndex = rowIndex,
            ColumnIndex = columnIndex
        };
    }
}

/// <summary>
/// Helper methods for creating column definitions with type inference.
/// </summary>
public static class ColumnDefinition
{
    /// <summary>
    /// Creates a column definition with automatic type inference.
    /// </summary>
    /// <typeparam name="TData">The type of data items in the table.</typeparam>
    /// <typeparam name="TValue">The type of the column's value.</typeparam>
    /// <param name="id">The unique column identifier.</param>
    /// <param name="header">The header text.</param>
    /// <param name="accessor">The function that extracts the column value.</param>
    /// <returns>A configured column definition.</returns>
    public static ColumnDefinition<TData, TValue> Create<TData, TValue>(
        string id,
        string header,
        Func<TData, TValue> accessor) where TData : class
    {
        return new ColumnDefinition<TData, TValue>
        {
            Id = id,
            Header = header,
            Accessor = accessor
        };
    }

    /// <summary>
    /// Creates a column definition with a custom comparer.
    /// </summary>
    /// <typeparam name="TData">The type of data items in the table.</typeparam>
    /// <typeparam name="TValue">The type of the column's value.</typeparam>
    /// <param name="id">The unique column identifier.</param>
    /// <param name="header">The header text.</param>
    /// <param name="accessor">The function that extracts the column value.</param>
    /// <param name="comparer">The custom comparer for sorting.</param>
    /// <returns>A configured column definition with custom comparer.</returns>
    public static ColumnDefinition<TData, TValue> Create<TData, TValue>(
        string id,
        string header,
        Func<TData, TValue> accessor,
        IComparer<TValue> comparer) where TData : class
    {
        return new ColumnDefinition<TData, TValue>
        {
            Id = id,
            Header = header,
            Accessor = accessor,
            CustomComparer = comparer
        };
    }
}
