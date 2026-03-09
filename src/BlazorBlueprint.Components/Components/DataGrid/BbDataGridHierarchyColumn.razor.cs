using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A DataGrid column that renders expand/collapse controls and depth-based indentation
/// for hierarchical data. Extends BbDataGridPropertyColumn with tree-specific rendering.
/// Only one BbDataGridHierarchyColumn is allowed per DataGrid.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
/// <typeparam name="TProp">The type of the property this column binds to.</typeparam>
public partial class BbDataGridHierarchyColumn<TData, TProp> : ComponentBase, IDataGridColumn<TData>, IFilterableColumn
    where TData : class
{
    private string? resolvedTitle;
    private Func<TData, TProp>? compiledProperty;

    /// <summary>
    /// Explicit unique identifier for this column.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// The property expression for this column.
    /// </summary>
    [Parameter, EditorRequired]
    public Expression<Func<TData, TProp>> Property { get; set; } = default!;

    /// <summary>
    /// Override the auto-inferred title.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Format string for displaying the property value.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Whether this column can be sorted. Default is false.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>
    /// Whether this column is visible. Default is true.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Column width.
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// Whether the user can toggle this column's visibility. Default is true.
    /// </summary>
    [Parameter]
    public bool Hideable { get; set; } = true;

    /// <summary>
    /// Whether this column can be resized. Default is true.
    /// </summary>
    [Parameter]
    public bool Resizable { get; set; } = true;

    /// <summary>
    /// Whether this column can be reordered. Default is true.
    /// </summary>
    [Parameter]
    public bool Reorderable { get; set; } = true;

    /// <summary>
    /// Column pinning.
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
    /// Whether text in this column should not wrap. Default is false.
    /// </summary>
    [Parameter]
    public bool NoWrap { get; set; }

    /// <summary>
    /// Whether this column supports per-column filtering. Default is false.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; }

    /// <summary>
    /// Override the auto-inferred filter field type.
    /// </summary>
    [Parameter]
    public FilterFieldType? FilterType { get; set; }

    /// <summary>
    /// Predefined options for Enum filter fields.
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<string>>? FilterOptions { get; set; }

    /// <summary>
    /// Custom cell template. Receives the item as context.
    /// When set, renders this template instead of the default property value text.
    /// The template is rendered after the expand/collapse chevron and indentation.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGridCellContext<TData>>? CellTemplate { get; set; }

    /// <summary>
    /// Indentation per depth level in pixels. Default is 24.
    /// </summary>
    [Parameter]
    public int IndentSize { get; set; } = 24;

    /// <summary>
    /// The parent DataGrid component.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation
    public string ColumnId => Id ?? GetColumnId();
    string? IDataGridColumn<TData>.Title => Title ?? resolvedTitle;
    bool IDataGridColumn<TData>.Sortable => Sortable;
    bool IDataGridColumn<TData>.Filterable => Filterable;
    bool IDataGridColumn<TData>.Visible => Visible;
    string? IDataGridColumn<TData>.Width => Width;
    bool IDataGridColumn<TData>.Hideable => Hideable;
    bool IDataGridColumn<TData>.Resizable => Resizable;
    bool IDataGridColumn<TData>.Reorderable => Reorderable && Pinned == ColumnPinning.None;
    ColumnPinning IDataGridColumn<TData>.Pinned => Pinned;
    RenderFragment<DataGridHeaderContext<TData>>? IDataGridColumn<TData>.HeaderTemplate => null;
    string? IDataGridColumn<TData>.CellClass => CellClass;
    string? IDataGridColumn<TData>.HeaderClass => HeaderClass;
    bool IDataGridColumn<TData>.NoWrap => NoWrap;
    AggregateFunction IDataGridColumn<TData>.Aggregate => AggregateFunction.None;
    string? IDataGridColumn<TData>.AggregateFormat => null;

    RenderFragment<DataGridCellContext<TData>>? IDataGridColumn<TData>.CellTemplate => CellTemplate;

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
    IEnumerable<SelectOption<string>>? IFilterableColumn.GetFilterOptions() => FilterOptions;
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
        if (propType == typeof(DateTime))
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
                $"{nameof(BbDataGridHierarchyColumn<TData, TProp>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }

        ParentGrid.RegisterHierarchyColumn(this);
        ParentGrid.RegisterHierarchyColumnDef(this);
    }

    private string GetColumnId()
    {
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
        return Regex.Replace(input, @"(?<=[a-z])([A-Z])|(?<=[A-Z])([A-Z][a-z])", " $1$2").Trim();
    }
}
