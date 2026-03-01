using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Column visibility toggle dropdown for the DataGrid.
/// Renders a dropdown menu with checkboxes to show/hide individual columns.
/// Must be placed inside a <see cref="BbDataGrid{TData}"/> component (typically in the Toolbar).
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGridColumnVisibility<TData> : ComponentBase
    where TData : class
{
    /// <summary>
    /// The parent DataGrid component. Set via cascading parameter.
    /// </summary>
    [CascadingParameter]
    internal BbDataGrid<TData>? ParentGrid { get; set; }

    /// <summary>
    /// Custom trigger content. If not provided, renders a default "Columns" button.
    /// </summary>
    [Parameter]
    public RenderFragment? TriggerContent { get; set; }

    /// <summary>
    /// Whether the trigger uses the AsChild pattern. Default is false.
    /// </summary>
    [Parameter]
    public bool AsChild { get; set; }

    /// <summary>
    /// CSS classes for the trigger.
    /// </summary>
    [Parameter]
    public string? TriggerClass { get; set; }

    protected override void OnInitialized()
    {
        if (ParentGrid == null)
        {
            throw new InvalidOperationException(
                $"{nameof(BbDataGridColumnVisibility<TData>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }
    }

    private IEnumerable<IDataGridColumn<TData>> GetHideableColumns()
    {
        if (ParentGrid == null)
        {
            return Array.Empty<IDataGridColumn<TData>>();
        }

        return ParentGrid.GetAllColumns().Where(c => c.Hideable);
    }

    private Task HandleVisibilityChanged(string columnId, bool visible) =>
        ParentGrid?.HandleColumnVisibilityChangedAsync(columnId, visible) ?? Task.CompletedTask;
}
