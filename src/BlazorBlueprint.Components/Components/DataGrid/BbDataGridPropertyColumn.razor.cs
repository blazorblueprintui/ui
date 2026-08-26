using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Defines a data-bound column in a DataGrid using a compile-time type-safe expression.
/// Auto-infers the column title from the property name and generates sort expressions for IQueryable.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
/// <typeparam name="TProp">The type of the property this column binds to.</typeparam>
public partial class BbDataGridPropertyColumn<TData, TProp> : ComponentBase, IDataGridColumn<TData>, IFilterableColumn
    where TData : class
{
    private string? resolvedTitle;
    private Func<TData, TProp>? compiledProperty;

    /// <summary>
    /// Explicit unique identifier for this column. When not set, falls back to the
    /// property member name (lowercase). Provide this when property names may collide
    /// or when persisted state must remain stable across refactors.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// The property expression for this column. Used for type-safe data access,
    /// auto title inference, and IQueryable sort expression generation.
    /// </summary>
    [Parameter, EditorRequired]
    public Expression<Func<TData, TProp>> Property { get; set; } = default!;

    /// <summary>
    /// Override the auto-inferred title. If null, the title is derived from the property name.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Format string for displaying the property value (e.g., "C2", "N0", "d").
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Whether this column can be sorted. Default is false.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>
    /// Whether rows can be grouped by this column from the column header menu. Default is false.
    /// When true, an ellipsis menu appears in the column header offering a "Group by" action.
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
    /// When true, a filter icon appears in the column header that opens a filter popover.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; }

    /// <summary>
    /// Override the auto-inferred filter field type. When null, the type is inferred from <typeparamref name="TProp"/>.
    /// </summary>
    [Parameter]
    public FilterFieldType? FilterType { get; set; }

    /// <summary>
    /// Predefined options for Enum filter fields. Required when <see cref="FilterType"/>
    /// is <see cref="FilterFieldType.Enum"/> or when <typeparamref name="TProp"/> is an enum type.
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<string>>? FilterOptions { get; set; }

    /// <summary>
    /// The aggregate function to compute for this column when grouping is active.
    /// Default is <see cref="AggregateFunction.None"/>.
    /// </summary>
    [Parameter]
    public AggregateFunction Aggregate { get; set; } = AggregateFunction.None;

    /// <summary>
    /// Format string for displaying aggregate values (e.g., "N0", "C2").
    /// When null, falls back to <see cref="Format"/> if set, otherwise uses default formatting.
    /// </summary>
    [Parameter]
    public string? AggregateFormat { get; set; }

    /// <summary>
    /// Custom cell template. If provided, overrides the default value rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<TData>? CellTemplate { get; set; }

    /// <summary>
    /// Custom header content. If provided, replaces the header title text only — the grid
    /// still renders its own sort indicator, filter icon, pin icon, column menu and resize
    /// handle around it, so a <see cref="Sortable"/> or <see cref="Groupable"/> column keeps
    /// every affordance. Use it to show an icon or richer markup instead of
    /// <see cref="Title"/>.
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
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation

    public string ColumnId => Id ?? GetColumnId();

    string? IDataGridColumn<TData>.Title => Title ?? resolvedTitle;

    bool IDataGridColumn<TData>.Sortable => Sortable;

    bool IDataGridColumn<TData>.Filterable => Filterable;

    bool IDataGridColumn<TData>.Groupable => Groupable;

    bool IDataGridColumn<TData>.Visible => Visible;

    int? IDataGridColumn<TData>.Order => Order;

    string? IDataGridColumn<TData>.Width => Width;

    bool IDataGridColumn<TData>.Hideable => Hideable;

    bool IDataGridColumn<TData>.Resizable => Resizable;

    bool IDataGridColumn<TData>.Reorderable => Reorderable && Pinned == ColumnPinning.None;

    ColumnPinning IDataGridColumn<TData>.Pinned => Pinned;

    RenderFragment<DataGridCellContext<TData>>? IDataGridColumn<TData>.CellTemplate =>
        CellTemplate != null
            ? context => CellTemplate(context.Item)
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

    string? IDataGridColumn<TData>.AggregateFormat => AggregateFormat ?? Format;

    public object? GetValue(TData item)
    {
        compiledProperty ??= Property.Compile();
        var value = compiledProperty(item);

        if (value == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(Format) && value is IFormattable formattable)
        {
            return formattable.ToString(Format, CultureInfo.CurrentCulture);
        }

        return value;
    }

    public object? GetRawValue(TData item)
    {
        compiledProperty ??= Property.Compile();
        return compiledProperty(item);
    }

    public int Compare(TData x, TData y)
    {
        compiledProperty ??= Property.Compile();
        var xVal = compiledProperty(x);
        var yVal = compiledProperty(y);
        return Comparer<TProp>.Default.Compare(xVal, yVal);
    }

    public LambdaExpression? GetSortExpression() => Property;

    public LambdaExpression? GetFilterExpression() => Filterable ? Property : null;

    // IFilterableColumn implementation

    FilterFieldType IFilterableColumn.GetFilterFieldType() => FilterType ?? InferFilterFieldType();

    IEnumerable<SelectOption<string>>? IFilterableColumn.GetFilterOptions() => FilterOptions ?? InferEnumOptions();

    string IFilterableColumn.GetFilterFieldName()
    {
        if (Property.Body is MemberExpression member)
        {
            return member.Member.Name;
        }
        return ColumnId;
    }

    private static FilterFieldType InferFilterFieldType()
    {
        var propType = Nullable.GetUnderlyingType(typeof(TProp)) ?? typeof(TProp);

        if (propType == typeof(string))
        {
            return FilterFieldType.Text;
        }
        if (propType == typeof(bool))
        {
            return FilterFieldType.Boolean;
        }
        if (propType == typeof(DateTime) || propType == typeof(DateTimeOffset))
        {
            return FilterFieldType.DateTime;
        }
        if (propType == typeof(DateOnly))
        {
            return FilterFieldType.Date;
        }
        if (propType.IsEnum)
        {
            return FilterFieldType.Enum;
        }
        if (IsNumericType(propType))
        {
            return FilterFieldType.Number;
        }

        return FilterFieldType.Text;
    }

    private static IEnumerable<SelectOption<string>>? InferEnumOptions()
    {
        var enumType = Nullable.GetUnderlyingType(typeof(TProp)) ?? typeof(TProp);
        if (!enumType.IsEnum)
        {
            return null;
        }

        return Enum.GetNames(enumType)
            .Select(name => new SelectOption<string>(name, FormatEnumName(name)));
    }

    private static string FormatEnumName(string name) =>
        Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1");

    private static bool IsNumericType(Type type) =>
        type == typeof(int) || type == typeof(long) || type == typeof(float) ||
        type == typeof(double) || type == typeof(decimal) || type == typeof(short) ||
        type == typeof(byte) || type == typeof(sbyte) || type == typeof(ushort) ||
        type == typeof(uint) || type == typeof(ulong);

    protected override void OnInitialized()
    {
        resolvedTitle = InferTitleFromExpression();

        if (ParentGrid == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataGridPropertyColumn<TData, TProp>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }

        ParentGrid.RegisterColumn(this);
    }

    private string GetColumnId()
    {
        // Derive ID from the property expression member name
        if (Property.Body is MemberExpression member)
        {
            return member.Member.Name.ToLowerInvariant();
        }

        return (Title ?? resolvedTitle ?? "column").ToLowerInvariant().Replace(" ", "-");
    }

    private string InferTitleFromExpression()
    {
        if (Property.Body is MemberExpression member)
        {
            // Convert "FirstName" -> "First Name", "HTMLParser" -> "HTML Parser"
            return SplitPascalCase(member.Member.Name);
        }

        return string.Empty;
    }

    private static string SplitPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Insert space before each uppercase letter that follows a lowercase letter or precedes a lowercase letter in an acronym
        return Regex.Replace(input, @"(?<=[a-z])([A-Z])|(?<=[A-Z])([A-Z][a-z])", " $1$2").Trim();
    }
}
