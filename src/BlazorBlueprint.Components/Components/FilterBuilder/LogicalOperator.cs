namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the logical operator used to combine filter conditions within a group.
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// All conditions must match (logical AND).
    /// </summary>
    And,

    /// <summary>
    /// Any condition can match (logical OR).
    /// </summary>
    Or
}
