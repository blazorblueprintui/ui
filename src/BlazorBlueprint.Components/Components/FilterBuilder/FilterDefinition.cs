namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a tree of filter conditions and nested groups combined with a logical operator.
/// Used as the model for <see cref="BbFilterBuilder"/>.
/// </summary>
public class FilterDefinition
{
    /// <summary>
    /// Gets or sets the logical operator (AND/OR) used to combine conditions and groups.
    /// </summary>
    public LogicalOperator Operator { get; set; } = LogicalOperator.And;

    /// <summary>
    /// Gets or sets the list of filter conditions in this group.
    /// </summary>
    public List<FilterCondition> Conditions { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of nested filter groups.
    /// </summary>
    public List<FilterDefinition> Groups { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether this filter has no conditions or groups.
    /// </summary>
    public bool IsEmpty => Conditions.Count == 0 && Groups.Count == 0;

    /// <summary>
    /// Gets the total number of conditions across this group and all nested groups.
    /// </summary>
    public int TotalConditionCount
    {
        get
        {
            var count = Conditions.Count;
            foreach (var group in Groups)
            {
                count += group.TotalConditionCount;
            }
            return count;
        }
    }

    /// <summary>
    /// Creates a deep clone of this filter definition, including all conditions and nested groups.
    /// </summary>
    public FilterDefinition Clone()
    {
        return new FilterDefinition
        {
            Operator = Operator,
            Conditions = Conditions.Select(c => c.Clone()).ToList(),
            Groups = Groups.Select(g => g.Clone()).ToList()
        };
    }
}
