using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a group of filter conditions with AND/OR toggle, nested sub-groups,
/// and buttons to add conditions or groups.
/// </summary>
public partial class BbFilterGroup : ComponentBase
{
    private static readonly IEnumerable<SelectOption<LogicalOperator>> logicalOperatorOptions = new[]
    {
        new SelectOption<LogicalOperator>(LogicalOperator.And, "AND"),
        new SelectOption<LogicalOperator>(LogicalOperator.Or, "OR")
    };

    /// <summary>
    /// Gets or sets the filter group data.
    /// </summary>
    [Parameter]
    public FilterDefinition Group { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current nesting depth of this group.
    /// </summary>
    [Parameter]
    public int Depth { get; set; }

    /// <summary>
    /// Gets or sets whether this is the root group.
    /// </summary>
    [Parameter]
    public bool IsRoot { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when this group should be removed.
    /// </summary>
    [Parameter]
    public EventCallback OnRemove { get; set; }

    /// <summary>
    /// Gets the cascaded filter builder context.
    /// </summary>
    [CascadingParameter]
    private FilterBuilderContext? Context { get; set; }

    private Task HandleOperatorChanged(LogicalOperator newOperator)
    {
        Group.Operator = newOperator;
        HandleChanged();
        return Task.CompletedTask;
    }

    private void AddCondition()
    {
        if (Context?.IsAtConditionLimit == true)
        {
            return;
        }

        Group.Conditions.Add(new FilterCondition());
        HandleChanged();
    }

    private void AddGroup()
    {
        if (Context == null || Depth >= Context.MaxDepth || Context.IsAtConditionLimit)
        {
            return;
        }

        var newGroup = new FilterDefinition
        {
            Operator = Context.DefaultOperator
        };
        newGroup.Conditions.Add(new FilterCondition());
        Group.Groups.Add(newGroup);
        HandleChanged();
    }

    private void RemoveCondition(FilterCondition condition)
    {
        Group.Conditions.Remove(condition);
        HandleChanged();
    }

    private void RemoveGroup(FilterDefinition group)
    {
        Group.Groups.Remove(group);
        HandleChanged();
    }

    private async Task HandleRemoveGroup() =>
        await OnRemove.InvokeAsync();

    private void HandleChanged()
    {
        Context?.NotifyChanged();
        StateHasChanged();
    }

    private string GroupAriaLabel => IsRoot ? "Root filter group" : $"Nested filter group at depth {Depth}";

    // Depth-based accent colors for visual nesting
    private static readonly string[] depthBorderColors = new[]
    {
        "border-l-primary",
        "border-l-blue-500",
        "border-l-purple-500",
        "border-l-orange-500"
    };

    private string DepthBorderColor => depthBorderColors[Depth % depthBorderColors.Length];

    private string GroupCssClass => ClassNames.cn(
        "rounded-lg border p-3 space-y-3",
        !IsRoot ? ClassNames.cn("border-l-4", DepthBorderColor, "bg-muted/30") : "bg-background"
    );

    private static string HeaderCssClass => "flex items-center justify-between";

    private static string ConditionsCssClass => "space-y-2";
}
