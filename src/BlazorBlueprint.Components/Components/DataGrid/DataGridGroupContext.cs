using BlazorBlueprint.Primitives.DataGrid;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Context object provided to group header templates.
/// Contains the group row data, collapse state, and a toggle callback.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public class DataGridGroupContext<TData> where TData : class
{
    /// <summary>
    /// Gets the group row data including key, item count, items, and aggregates.
    /// </summary>
    public required DataGridGroupRow<TData> Group { get; init; }

    /// <summary>
    /// Gets whether this group is currently collapsed.
    /// </summary>
    public bool IsCollapsed { get; init; }

    /// <summary>
    /// Gets the number of visible columns (for colspan calculations).
    /// </summary>
    public int VisibleColumnCount { get; init; }

    /// <summary>
    /// Gets the callback to toggle this group's collapsed state.
    /// </summary>
    public EventCallback ToggleCollapse { get; init; }
}
