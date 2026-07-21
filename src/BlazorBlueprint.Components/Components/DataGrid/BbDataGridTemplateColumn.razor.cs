using System.Linq.Expressions;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines a template-based column in a DataGrid with custom cell rendering.
/// Use this for columns that display actions, badges, or other custom content.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridTemplateColumn<TData> : ComponentBase, IDataGridColumn<TData>, IFilterableColumn
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
    /// Custom header content. If provided, replaces the header title text only — the grid still
    /// renders its own sort indicator, filter icon, pin icon, column menu and resize handle
    /// around it. Use it to show an icon or richer markup instead of <see cref="Title"/>.
    /// </summary>
    /// <remarks>
    /// While a template is supplied, the grid names the header cell with the column's title
    /// (<c>aria-label</c>), so an icon-only header is still announced — no <c>sr-only</c> span of
    /// your own is needed, and one that is already there is not announced twice. That label
    /// replaces the template's own text rather than adding to it, which keeps the header agreeing
    /// with the column chooser, the column menu and the filter labels, all of which already use
    /// the title. Leave <see cref="Title"/> set to something meaningful for this reason; if it is
    /// blank, no label is applied and the cell falls back to being announced from the template's
    /// content.
    /// </remarks>
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
    /// Whether rows can be grouped by this column from the column header menu. Default is false.
    /// Requires <see cref="SortBy"/> to be set when true, since the group key is taken from it.
    /// </summary>
    [Parameter]
    public bool Groupable { get; set; }

    /// <summary>
    /// Whether this column is visible. Default is true.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Explicit position for this column, as a zero-based index among the grid's data columns.
    /// When not set (the default), the column keeps its registration order — the order in which
    /// its component initializes, which for a column written directly in the grid's markup is
    /// the order it was declared in.
    /// </summary>
    /// <remarks>
    /// Set this on a column produced indirectly — by a wrapper component, or by a fragment that
    /// only renders after an await — where initialization order does not match declaration order.
    /// <para>
    /// Columns that leave <c>Order</c> unset are laid out first, in registration order. Each
    /// column that sets it is then inserted at that index, lowest value first; an index past the
    /// end appends. Two columns sharing an <c>Order</c> keep their registration order relative to
    /// each other. Because unset columns retain their relative positions, a grid where no column
    /// sets <c>Order</c> is laid out exactly as it would be without this parameter.
    /// </para>
    /// <para>
    /// <see cref="BbDataGridSelectColumn{TData}"/> and <see cref="BbDataGridExpandColumn{TData}"/>
    /// keep their fixed leading positions and are not counted by this index.
    /// </para>
    /// <para>
    /// The value is read when the column registers with the grid; changing it afterwards has no
    /// effect on an already rendered grid.
    /// </para>
    /// </remarks>
    [Parameter]
    public int? Order { get; set; }

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
    /// Computes additional CSS classes for a cell from the row's data item,
    /// applied in addition to <see cref="CellClass"/>. Use for conditional
    /// per-cell formatting, e.g. <c>CellClassFunc="@(o => o.Total &lt; 0 ? "text-destructive" : null)"</c>.
    /// </summary>
    [Parameter]
    public Func<TData, string?>? CellClassFunc { get; set; }

    /// <summary>
    /// Additional CSS classes for the header cell.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Whether text in this column should not wrap. When true, cell content uses
    /// white-space: nowrap and truncates with an ellipsis on overflow. Default is false.
    /// </summary>
    [Parameter]
    public bool NoWrap { get; set; }

    /// <summary>
    /// Whether this column supports per-column filtering. Default is false.
    /// Requires <see cref="FilterBy"/> to be set when true.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; }

    /// <summary>
    /// Expression used to build filter Where clauses. Required when <see cref="Filterable"/> is true.
    /// </summary>
    [Parameter]
    public Expression<Func<TData, object>>? FilterBy { get; set; }

    /// <summary>
    /// Override the auto-inferred filter field type. When null, defaults to <see cref="FilterFieldType.Text"/>.
    /// </summary>
    [Parameter]
    public FilterFieldType? FilterType { get; set; }

    /// <summary>
    /// Predefined options for Enum filter fields. Required when <see cref="FilterType"/>
    /// is <see cref="FilterFieldType.Enum"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<string>>? FilterOptions { get; set; }

    /// <summary>
    /// The aggregate function to compute for this column when grouping is active.
    /// Default is <see cref="AggregateFunction.None"/>. Requires <see cref="SortBy"/> to provide a value accessor.
    /// </summary>
    [Parameter]
    public AggregateFunction Aggregate { get; set; } = AggregateFunction.None;

    /// <summary>
    /// Format string for displaying aggregate values (e.g., "N0", "C2").
    /// When null, uses default formatting.
    /// </summary>
    [Parameter]
    public string? AggregateFormat { get; set; }

    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation

    public string ColumnId => Id ?? Title.ToLowerInvariant().Replace(" ", "-");

    string? IDataGridColumn<TData>.Title => Title;

    bool IDataGridColumn<TData>.Sortable => Sortable && SortBy != null;

    bool IDataGridColumn<TData>.Filterable => Filterable && FilterBy != null;

    bool IDataGridColumn<TData>.Groupable => Groupable && SortBy != null;

    bool IDataGridColumn<TData>.Visible => Visible;

    int? IDataGridColumn<TData>.Order => Order;

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

    Func<TData, string?>? IDataGridColumn<TData>.CellClassFunc => CellClassFunc;

    string? IDataGridColumn<TData>.HeaderClass => HeaderClass;

    bool IDataGridColumn<TData>.NoWrap => NoWrap;

    AggregateFunction IDataGridColumn<TData>.Aggregate => Aggregate;

    string? IDataGridColumn<TData>.AggregateFormat => AggregateFormat;

    public object? GetValue(TData item) => null;

    object? IDataGridColumn<TData>.GetRawValue(TData item)
    {
        if (SortBy == null)
        {
            return null;
        }

        if (compiledSortBy == null || !ReferenceEquals(lastSortBy, SortBy))
        {
            compiledSortBy = SortBy.Compile();
            lastSortBy = SortBy;
        }

        return compiledSortBy(item);
    }

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

    public LambdaExpression? GetFilterExpression()
    {
        if (FilterBy == null)
        {
            return null;
        }

        // Unwrap Convert(..., object) to expose the underlying member's real type.
        if (FilterBy.Body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            return Expression.Lambda(unary.Operand, FilterBy.Parameters);
        }

        return FilterBy;
    }

    // IFilterableColumn implementation

    FilterFieldType IFilterableColumn.GetFilterFieldType() => FilterType ?? FilterFieldType.Text;

    IEnumerable<SelectOption<string>>? IFilterableColumn.GetFilterOptions() => FilterOptions;

    string IFilterableColumn.GetFilterFieldName()
    {
        if (FilterBy?.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        if (FilterBy?.Body is UnaryExpression { Operand: MemberExpression innerMember })
        {
            return innerMember.Member.Name;
        }

        return ColumnId;
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
