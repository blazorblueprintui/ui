namespace BlazorBlueprint.Components;

/// <summary>
/// Context cascaded from <see cref="BbFilterBuilder"/> to child components.
/// Provides access to available fields and configuration.
/// </summary>
public class FilterBuilderContext
{
    /// <summary>
    /// Gets the available fields to filter on.
    /// </summary>
    public required IEnumerable<FilterField> Fields { get; init; }

    /// <summary>
    /// Gets the maximum nesting depth for filter groups.
    /// </summary>
    public int MaxDepth { get; init; }

    /// <summary>
    /// Gets the maximum total conditions allowed, or null for unlimited.
    /// </summary>
    public int? MaxConditions { get; init; }

    /// <summary>
    /// Gets the default logical operator for new groups.
    /// </summary>
    public LogicalOperator DefaultOperator { get; init; }

    /// <summary>
    /// Gets a value indicating whether compact layout mode is enabled.
    /// </summary>
    public bool Compact { get; init; }

    /// <summary>
    /// Gets the root filter definition to compute total condition count.
    /// </summary>
    public required FilterDefinition RootFilter { get; init; }

    /// <summary>
    /// Gets the callback to invoke when the filter changes.
    /// </summary>
    public required Action NotifyChanged { get; init; }

    /// <summary>
    /// Returns true if adding a new condition would exceed the MaxConditions limit.
    /// </summary>
    public bool IsAtConditionLimit => MaxConditions.HasValue && RootFilter.TotalConditionCount >= MaxConditions.Value;
}
