using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.DataTable;

/// <summary>
/// Defines a column in a DataTable with declarative syntax.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
/// <typeparam name="TValue">The type of the column's value.</typeparam>
/// <remarks>
/// <para>
/// DataTableColumn provides a declarative way to define table columns using Razor syntax.
/// Each column specifies how to extract data (Property), display headers (Header), and
/// optionally render custom cell content (CellTemplate).
/// </para>
/// <para>
/// Features:
/// - Type-safe data access via Property parameter (Func&lt;TData, TValue&gt;)
/// - Sortable and Filterable flags for automatic behavior
/// - Custom cell rendering via CellTemplate
/// - Column visibility toggle support
/// - Width configuration (Width, MinWidth, MaxWidth)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;DataTableColumn TData="Person" TValue="string"
///                  Property="@(p => p.Name)"
///                  Header="Full Name"
///                  Sortable="true"
///                  Filterable="true" /&gt;
/// </code>
/// </example>
public partial class DataTableColumn<TData, TValue> : ComponentBase where TData : class
{
    /// <summary>
    /// Gets or sets the unique identifier for this column.
    /// If not provided, it will be auto-generated from the Header.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the header text displayed for this column.
    /// </summary>
    [Parameter, EditorRequired]
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the function that extracts the column value from a data item.
    /// This enables type-safe data access.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TData, TValue?> Property { get; set; } = null!;

    /// <summary>
    /// Gets or sets the format string used to format the cell value.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets whether this column can be sorted.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; }

    /// <summary>
    /// Gets or sets whether this column can be filtered.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; }

    /// <summary>
    /// Gets or sets whether this column is currently visible.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the width of the column (e.g., "200px", "20%", "auto").
    /// Null means the column will size automatically.
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// Gets or sets the minimum width of the column (e.g., "100px").
    /// Useful for responsive layouts.
    /// </summary>
    [Parameter]
    public string? MinWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum width of the column (e.g., "400px").
    /// Useful for preventing excessively wide columns.
    /// </summary>
    [Parameter]
    public string? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets a custom template for rendering cell values.
    /// If null, the value is rendered using ToString().
    /// </summary>
    /// <remarks>
    /// The context parameter provides the data item (TData) for the row.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;DataTableColumn Property="@(p => p.Status)" Header="Status"&gt;
    ///     &lt;CellTemplate Context="person"&gt;
    ///         &lt;Badge Variant="@(person.Status == "Active" ? BadgeVariant.Default : BadgeVariant.Destructive)"&gt;
    ///             @person.Status
    ///         &lt;/Badge&gt;
    ///     &lt;/CellTemplate&gt;
    /// &lt;/DataTableColumn&gt;
    /// </code>
    /// </example>
    [Parameter]
    public RenderFragment<TData>? CellTemplate { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to cells in this column.
    /// </summary>
    [Parameter]
    public string? CellClass { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the header cell.
    /// </summary>
    [Parameter]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Gets or sets the parent DataTable component.
    /// Automatically set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal DataTable<TData>? ParentTable { get; set; }

    /// <summary>
    /// Gets the effective column ID (uses Id if provided, otherwise generates from Header).
    /// </summary>
    internal string EffectiveId => Id ?? Header.ToLowerInvariant().Replace(" ", "-");

    protected override void OnInitialized()
    {
        if (ParentTable == null)
        {
            throw new InvalidOperationException(
                $"{nameof(DataTableColumn<TData, TValue>)} must be placed inside a {nameof(DataTable<TData>)} component.");
        }

        // Register this column with the parent table
        ParentTable.RegisterColumn(this);
    }
}
