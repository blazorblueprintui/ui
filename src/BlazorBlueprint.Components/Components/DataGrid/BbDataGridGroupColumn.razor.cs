using System.Linq.Expressions;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Table;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Declarative component that configures row grouping on a DataGrid.
/// Place inside the Columns render fragment to group rows by a property.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
/// <typeparam name="TProperty">The type of the property to group by.</typeparam>
public partial class BbDataGridGroupColumn<TData, TProperty> : ComponentBase
    where TData : class
{
    /// <summary>
    /// The property expression to group rows by.
    /// </summary>
    [Parameter, EditorRequired]
    public Expression<Func<TData, TProperty>> Property { get; set; } = default!;

    /// <summary>
    /// Override the display name for the group column. When null, inferred from the property name.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// The sort direction for group ordering. Default is ascending.
    /// </summary>
    [Parameter]
    public SortDirection GroupSortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// Whether groups should be collapsed by default. Default is false.
    /// </summary>
    [Parameter]
    public bool CollapsedByDefault { get; set; }

    /// <summary>
    /// Custom template for the group header row content.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGridGroupContext<TData>>? HeaderTemplate { get; set; }

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
                $"{nameof(BbDataGridGroupColumn<TData, TProperty>)} must be placed inside a {nameof(BbDataGrid<TData>)} component.");
        }

        ParentGrid.SetGrouping(this);
    }

    internal string GetColumnId()
    {
        if (Property.Body is MemberExpression member)
        {
            return member.Member.Name.ToLowerInvariant();
        }

        return "group";
    }

    internal string GetTitle()
    {
        if (Title != null)
        {
            return Title;
        }

        if (Property.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        return "Group";
    }

    internal Func<TData, object?> GetValueAccessor()
    {
        var compiled = Property.Compile();
        return item => compiled(item);
    }
}
