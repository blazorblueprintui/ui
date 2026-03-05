namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Manages grouping state for a DataGrid.
/// Tracks which groups are collapsed and the active group definition.
/// </summary>
public class DataGridGroupState
{
    private readonly HashSet<object> collapsedKeys = new();

    /// <summary>
    /// Gets or sets the active group definition, or null if no grouping is active.
    /// </summary>
    public GroupDefinition? ActiveGroup { get; private set; }

    /// <summary>
    /// Gets whether a group definition is currently active.
    /// </summary>
    public bool HasGroups => ActiveGroup != null;

    /// <summary>
    /// Gets the set of collapsed group keys.
    /// </summary>
    public IReadOnlyCollection<object> CollapsedKeys => collapsedKeys;

    /// <summary>
    /// Sets the active group definition.
    /// </summary>
    /// <param name="group">The group definition, or null to clear grouping.</param>
    public void SetGroup(GroupDefinition? group)
    {
        ActiveGroup = group;
        collapsedKeys.Clear();
    }

    /// <summary>
    /// Clears the active group definition and all collapsed state.
    /// </summary>
    public void ClearGroup()
    {
        ActiveGroup = null;
        collapsedKeys.Clear();
    }

    /// <summary>
    /// Checks whether a group is collapsed.
    /// </summary>
    /// <param name="key">The group key to check.</param>
    /// <returns>True if the group is collapsed, false otherwise.</returns>
    public bool IsCollapsed(object key) => collapsedKeys.Contains(key);

    /// <summary>
    /// Toggles the collapsed state of a group.
    /// </summary>
    /// <param name="key">The group key to toggle.</param>
    public void Toggle(object key)
    {
        if (!collapsedKeys.Remove(key))
        {
            collapsedKeys.Add(key);
        }
    }

    /// <summary>
    /// Expands all groups by clearing the collapsed set.
    /// </summary>
    public void ExpandAll() => collapsedKeys.Clear();

    /// <summary>
    /// Collapses all groups by adding all provided keys to the collapsed set.
    /// </summary>
    /// <param name="keys">The group keys to collapse.</param>
    public void CollapseAll(IEnumerable<object> keys)
    {
        foreach (var key in keys)
        {
            collapsedKeys.Add(key);
        }
    }

    /// <summary>
    /// Clears all collapsed state and the active group definition.
    /// </summary>
    public void Clear()
    {
        collapsedKeys.Clear();
        ActiveGroup = null;
    }
}
