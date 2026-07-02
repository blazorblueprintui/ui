namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the visual layout mode for the <see cref="BbSortable{TItem}"/> container.
/// </summary>
public enum SortableLayout
{
    /// <summary>
    /// Items are stacked vertically in a single column (default).
    /// </summary>
    List,

    /// <summary>
    /// Items are arranged in a two-column grid.
    /// Override the column count by supplying a Tailwind <c>grid-cols-*</c> class
    /// via the <c>Class</c> parameter (e.g., <c>Class="grid-cols-3 gap-4"</c>).
    /// </summary>
    Grid
}
