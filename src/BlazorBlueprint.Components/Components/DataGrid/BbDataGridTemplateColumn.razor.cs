using System.Linq.Expressions;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines a template-based column in a DataGrid with custom cell rendering.
/// Use this for columns that display actions, badges, or other custom content.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridTemplateColumn<TData> : ComponentBase, IDataGridColumn<TData>
    where TData : class
{
    private Func<TData, object>? compiledSortBy;
    private Expression<Func<TData, object>>? lastSortBy;

    /// <summary>
    /// Explicit unique identifier for this column. When not set, falls back to a slug
    /// derived from <see cref="Title"/>. Provide this when titles may collide, change
    /// under localization, or when persisted state must remain stable.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// The column title displayed in the header.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Custom cell template. The context provides the data item for the row.
    /// </summary>
    [Parameter]
    public RenderFragment<TData>? ChildContent { get; set; }

    /// <summary>
    /// Custom header template.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Whether this column can be sorted. Default is false.
    /// Requires <see cref="SortBy"/> to be set when true.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>
    /// Custom sort expression. Required when <see cref="Sortable"/> is true.
    /// </summary>
    [Parameter]
    public Expression<Func<TData, object>>? SortBy { get; set; }

    /// <summary>
    /// Whether this column is visible. Default is true.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Column width (e.g., "200px", "20%", "auto").
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// Whether the user can toggle this column's visibility via a column chooser. Default is true.
    /// </summary>
    [Parameter]
    public bool Hideable { get; set; } = true;

    /// <summary>
    /// Whether this column can be resized via drag handles. Default is true.
    /// </summary>
    [Parameter]
    public bool Resizable { get; set; } = true;

    /// <summary>
    /// Whether this column can be reordered via drag-and-drop. Default is true.
    /// </summary>
    [Parameter]
    public bool Reorderable { get; set; } = true;

    /// <summary>
    /// Whether this column is pinned to an edge of the scrollable viewport.
    /// Pinned columns use CSS position: sticky. Default is <see cref="ColumnPinning.None"/>.
    /// </summary>
    [Parameter]
    public ColumnPinning Pinned { get; set; } = ColumnPinning.None;

    /// <summary>
    /// Additional CSS classes for cells in this column.
    /// </summary>
    [Parameter]
    public string? CellClass { get; set; }

    /// <summary>
    /// Additional CSS classes for the header cell.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation

    public string ColumnId => Id ?? Title.ToLowerInvariant().Replace(" ", "-");

    string? IDataGridColumn<TData>.Title => Title;

    bool IDataGridColumn<TData>.Sortable => Sortable && SortBy != null;

    bool IDataGridColumn<TData>.Visible => Visible;

    string? IDataGridColumn<TData>.Width => Width;

    bool IDataGridColumn<TData>.Hideable => Hideable;

    bool IDataGridColumn<TData>.Resizable => Resizable;

    bool IDataGridColumn<TData>.Reorderable => Reorderable && Pinned == ColumnPinning.None;

    ColumnPinning IDataGridColumn<TData>.Pinned => Pinned;

    RenderFragment<DataGridCellContext<TData>>? IDataGridColumn<TData>.CellTemplate =>
        ChildContent != null
            ? context => ChildContent(context.Item)
            : null;

    RenderFragment<DataGridHeaderContext<TData>>? IDataGridColumn<TData>.HeaderTemplate =>
        HeaderTemplate != null
            ? _ => HeaderTemplate
            : null;

    string? IDataGridColumn<TData>.CellClass => CellClass;

    string? IDataGridColumn<TData>.HeaderClass => HeaderClass;

    public object? GetValue(TData item) => null;

    public int Compare(TData x, TData y)
    {
        if (SortBy == null)
        {
            return 0;
        }

        if (compiledSortBy == null || !ReferenceEquals(lastSortBy, SortBy))
        {
            compiledSortBy = SortBy.Compile();
            lastSortBy = SortBy;
        }

        var xVal = compiledSortBy(x);
        var yVal = compiledSortBy(y);
        return Comparer<object>.Default.Compare(xVal, yVal);
    }

    public LambdaExpression? GetSortExpression()
    {
        if (SortBy == null)
        {
            return null;
        }

        // Unwrap Convert(..., object) to expose the underlying member's real type.
        // Without this, IQueryable providers (e.g. EF Core) fail to translate
        // OrderBy<TData, object> because the boxing conversion has no SQL equivalent.
        if (SortBy.Body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            return Expression.Lambda(unary.Operand, SortBy.Parameters);
        }

        return SortBy;
    }

    protected override void OnInitialized()
    {
        if (ParentGrid == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataGridTemplateColumn<TData>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }

        ParentGrid.RegisterColumn(this);
    }
}
