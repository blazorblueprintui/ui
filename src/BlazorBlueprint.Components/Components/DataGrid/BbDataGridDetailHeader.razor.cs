using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a header row inside a DataGrid's expanded detail section.
/// Spans all visible columns to display a section label above detail rows.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridDetailHeader<TData> : ComponentBase
    where TData : class
{
    /// <summary>
    /// Content to display in the detail header row.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes for the detail header row.
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
                $"{nameof(BbDataGridDetailHeader<TData>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }
    }
}
