using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a detail row inside a DataGrid's expanded section.
/// Uses the parent grid's column structure so cell content aligns
/// with the main data rows.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridDetailRow<TData> : ComponentBase
    where TData : class
{
    /// <summary>
    /// The data item to render in this detail row.
    /// Column cell templates and value accessors are applied to this item.
    /// </summary>
    [Parameter, EditorRequired]
    public TData Item { get; set; } = default!;

    /// <summary>
    /// Additional CSS classes for the detail row.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    protected override void OnInitialized()
    {
        if (ParentGrid == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataGridDetailRow<TData>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }
    }
}
