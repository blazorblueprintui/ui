using System.Linq.Expressions;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines an expand/collapse column in a DataGrid.
/// Renders a chevron button per row to toggle detail content visibility.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridExpandColumn<TData> : ComponentBase, IDataGridColumn<TData>
    where TData : class
{
    /// <summary>
    /// Column width. Defaults to a compact chevron button width.
    /// </summary>
    [Parameter]
    public string? Width { get; set; } = "48px";

    /// <summary>
    /// Whether this column is pinned to an edge of the scrollable viewport.
    /// Default is <see cref="ColumnPinning.None"/>.
    /// </summary>
    [Parameter]
    public ColumnPinning Pinned { get; set; } = ColumnPinning.None;

    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation

    public string ColumnId => "__expand";

    string? IDataGridColumn<TData>.Title => null;

    bool IDataGridColumn<TData>.Sortable => false;

    bool IDataGridColumn<TData>.Filterable => false;

    bool IDataGridColumn<TData>.Visible => true;

    string? IDataGridColumn<TData>.Width => Width;

    bool IDataGridColumn<TData>.Hideable => false;

    bool IDataGridColumn<TData>.Resizable => false;

    bool IDataGridColumn<TData>.Reorderable => false;

    ColumnPinning IDataGridColumn<TData>.Pinned => Pinned;

    RenderFragment<DataGridCellContext<TData>>? IDataGridColumn<TData>.CellTemplate => null;

    RenderFragment<DataGridHeaderContext<TData>>? IDataGridColumn<TData>.HeaderTemplate => null;

    string? IDataGridColumn<TData>.CellClass => null;

    string? IDataGridColumn<TData>.HeaderClass => null;

    bool IDataGridColumn<TData>.NoWrap => false;

    public object? GetValue(TData item) => null;

    public int Compare(TData x, TData y) => 0;

    public LambdaExpression? GetSortExpression() => null;

    public LambdaExpression? GetFilterExpression() => null;

    protected override void OnInitialized()
    {
        if (ParentGrid == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataGridExpandColumn<TData>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }

        ParentGrid.RegisterColumn(this);
    }
}
