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
public partial class BbDataGridPropertyColumn<TData, TProp> : ComponentBase, IDataGridColumn<TData>
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
    /// Whether text in this column should not wrap. When true, cell content uses
    /// white-space: nowrap and truncates with an ellipsis on overflow. Default is false.
    /// </summary>
    [Parameter]
    public bool NoWrap { get; set; }

    /// <summary>
    /// Custom cell template. If provided, overrides the default value rendering.
    /// </summary>
    [Parameter]
    public RenderFragment<TData>? CellTemplate { get; set; }

    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    // IDataGridColumn implementation

    public string ColumnId => Id ?? GetColumnId();

    string? IDataGridColumn<TData>.Title => Title ?? resolvedTitle;

    bool IDataGridColumn<TData>.Sortable => Sortable;

    bool IDataGridColumn<TData>.Visible => Visible;

    string? IDataGridColumn<TData>.Width => Width;

    bool IDataGridColumn<TData>.Hideable => Hideable;

    bool IDataGridColumn<TData>.Resizable => Resizable;

    bool IDataGridColumn<TData>.Reorderable => Reorderable && Pinned == ColumnPinning.None;

    ColumnPinning IDataGridColumn<TData>.Pinned => Pinned;

    RenderFragment<DataGridCellContext<TData>>? IDataGridColumn<TData>.CellTemplate =>
        CellTemplate != null
            ? context => CellTemplate(context.Item)
            : null;

    RenderFragment<DataGridHeaderContext<TData>>? IDataGridColumn<TData>.HeaderTemplate => null;

    string? IDataGridColumn<TData>.CellClass => CellClass;

    string? IDataGridColumn<TData>.HeaderClass => HeaderClass;

    bool IDataGridColumn<TData>.NoWrap => NoWrap;

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

    public int Compare(TData x, TData y)
    {
        compiledProperty ??= Property.Compile();
        var xVal = compiledProperty(x);
        var yVal = compiledProperty(y);
        return Comparer<TProp>.Default.Compare(xVal, yVal);
    }

    public LambdaExpression? GetSortExpression() => Property;

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
