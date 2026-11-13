using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A column whose cell template only needs the row's item, not the cell context.
/// </summary>
/// <remarks>
/// Property and template columns take a <c>RenderFragment&lt;TData&gt;</c> and adapt it to the
/// column interface's <c>RenderFragment&lt;DataGridCellContext&lt;TData&gt;&gt;</c>. Going through
/// that adapter cost a delegate per cell and, for a property column, reading and formatting the
/// cell's value only for the template to ignore it (#655). The grid renders these templates from
/// the item directly.
/// </remarks>
internal interface IItemCellTemplateColumn<TData>
    where TData : class
{
    /// <summary>
    /// The column's cell template, or null if the cell renders its value as text.
    /// </summary>
    public RenderFragment<TData>? ItemCellTemplate { get; }
}
