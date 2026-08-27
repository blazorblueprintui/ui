using System.Linq.Expressions;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the column that starts, commits and cancels a row edit.
/// Shows an Edit button on a row at rest, and Save and Cancel buttons on the row being edited.
/// </summary>
/// <remarks>
/// Needs <c>EditMode</c> set on the grid; without it the buttons do nothing. The column is not
/// hideable, resizable or reorderable, for the same reason the select and expand columns are not:
/// it is a control, not data.
/// </remarks>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridEditColumn<TData> : ComponentBase, IDataGridColumn<TData>
    where TData : class
{
    /// <summary>
    /// The column id used by every edit column.
    /// </summary>
    internal const string EditColumnId = "__edit";

    /// <summary>
    /// Column width. Defaults to a width that fits the Save and Cancel buttons.
    /// </summary>
    [Parameter]
    public string? Width { get; set; } = "120px";

    /// <summary>
    /// Whether this column is pinned to an edge of the scrollable viewport.
    /// Default is <see cref="ColumnPinning.None"/>.
    /// </summary>
    /// <remarks>
    /// Pin it right on a wide grid, so the Save button stays reachable without scrolling back.
    /// </remarks>
    [Parameter]
    public ColumnPinning Pinned { get; set; } = ColumnPinning.None;

    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation

    public string ColumnId => EditColumnId;

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

    bool IDataGridColumn<TData>.NoWrap => true;

    AggregateFunction IDataGridColumn<TData>.Aggregate => AggregateFunction.None;

    public object? GetValue(TData item) => null;

    public int Compare(TData x, TData y) => 0;

    public LambdaExpression? GetSortExpression() => null;

    public LambdaExpression? GetFilterExpression() => null;

    protected override void OnInitialized()
    {
        if (ParentGrid == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataGridEditColumn<TData>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }

        ParentGrid.RegisterColumn(this);
    }
}
