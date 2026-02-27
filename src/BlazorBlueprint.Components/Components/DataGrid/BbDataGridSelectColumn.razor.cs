using System.Linq.Expressions;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines a selection checkbox column in a DataGrid.
/// Renders a header checkbox (select all) and per-row checkboxes.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridSelectColumn<TData> : ComponentBase, IDataGridColumn<TData>
    where TData : class
{
    /// <summary>
    /// Column width. Defaults to a compact checkbox width.
    /// </summary>
    [Parameter]
    public string? Width { get; set; } = "48px";

    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation

    public string ColumnId => "__select";

    string? IDataGridColumn<TData>.Title => null;

    bool IDataGridColumn<TData>.Sortable => false;

    bool IDataGridColumn<TData>.Visible => true;

    string? IDataGridColumn<TData>.Width => Width;

    bool IDataGridColumn<TData>.Hideable => false;

    bool IDataGridColumn<TData>.Resizable => false;

    bool IDataGridColumn<TData>.Reorderable => false;

    RenderFragment<DataGridCellContext<TData>>? IDataGridColumn<TData>.CellTemplate => null;

    RenderFragment<DataGridHeaderContext<TData>>? IDataGridColumn<TData>.HeaderTemplate => null;

    public object? GetValue(TData item) => null;

    public int Compare(TData x, TData y) => 0;

    public LambdaExpression? GetSortExpression() => null;

    protected override void OnInitialized()
    {
        if (ParentGrid == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataGridSelectColumn<TData>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }

        ParentGrid.RegisterColumn(this);
    }
}
