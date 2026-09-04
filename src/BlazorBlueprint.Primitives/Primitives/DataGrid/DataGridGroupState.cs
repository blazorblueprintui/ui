namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Manages grouping state for a DataGrid.
/// Tracks the ordered group levels and which groups are collapsed.
/// </summary>
public class DataGridGroupState
{
    private readonly HashSet<GroupPath> collapsedPaths = new();
    private readonly List<GroupDefinition> activeGroups = new();

    /// <summary>
    /// Gets the active group levels, outermost first. Empty when no grouping is active.
    /// </summary>
    public IReadOnlyList<GroupDefinition> ActiveGroups => activeGroups;

    /// <summary>
    /// Gets the outermost active group definition, or null if no grouping is active.
    /// </summary>
    /// <remarks>
    /// Kept for grids written against single-level grouping. Read
    /// <see cref="ActiveGroups"/> to see every level.
    /// </remarks>
    public GroupDefinition? ActiveGroup => activeGroups.Count > 0 ? activeGroups[0] : null;

    /// <summary>
    /// Gets whether any group definition is currently active.
    /// </summary>
    public bool HasGroups => activeGroups.Count > 0;

    /// <summary>
    /// Gets how many levels of grouping are active.
    /// </summary>
    public int Depth => activeGroups.Count;

    /// <summary>
    /// Gets the set of collapsed group paths.
    /// </summary>
    public IReadOnlyCollection<GroupPath> CollapsedPaths => collapsedPaths;

    /// <summary>
    /// Gets the keys of collapsed outermost groups.
    /// </summary>
    /// <remarks>
    /// Kept for grids written against single-level grouping, where a key identified a group on its
    /// own. Read <see cref="CollapsedPaths"/> to see collapsed groups at every level.
    /// </remarks>
    public IReadOnlyCollection<object> CollapsedKeys =>
        collapsedPaths.Where(p => p.Depth == 0 && p.Key != null).Select(p => p.Key!).ToList();

    /// <summary>
    /// Gets a version counter that increments whenever the active group definitions change.
    /// Used by the grid component to detect grouping changes made directly against this state.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Replaces all grouping with a single level.
    /// </summary>
    /// <param name="group">The group definition, or null to clear grouping.</param>
    public void SetGroup(GroupDefinition? group)
    {
        activeGroups.Clear();
        if (group != null)
        {
            activeGroups.Add(group);
        }

        collapsedPaths.Clear();
        Version++;
    }

    /// <summary>
    /// Replaces all grouping with the given levels, outermost first.
    /// </summary>
    /// <remarks>
    /// A column appearing more than once is kept only at its first position: grouping by the same
    /// column twice would produce a level where every group holds exactly one child group.
    /// </remarks>
    /// <param name="groups">The group definitions, outermost first. Null or empty clears grouping.</param>
    public void SetGroups(IEnumerable<GroupDefinition>? groups)
    {
        activeGroups.Clear();

        if (groups != null)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var group in groups)
            {
                if (group != null && seen.Add(group.ColumnId))
                {
                    activeGroups.Add(group);
                }
            }
        }

        collapsedPaths.Clear();
        Version++;
    }

    /// <summary>
    /// Adds a level of grouping inside the existing levels.
    /// </summary>
    /// <remarks>
    /// Does nothing when the column already groups at some level. Collapsed state is cleared,
    /// because every existing path is now one level short of identifying a group.
    /// </remarks>
    /// <param name="group">The group definition to add as the innermost level.</param>
    /// <returns>True when the level was added.</returns>
    public bool AddGroup(GroupDefinition group)
    {
        ArgumentNullException.ThrowIfNull(group);

        if (activeGroups.Any(g => string.Equals(g.ColumnId, group.ColumnId, StringComparison.Ordinal)))
        {
            return false;
        }

        activeGroups.Add(group);
        collapsedPaths.Clear();
        Version++;
        return true;
    }

    /// <summary>
    /// Removes the grouping level for a column, keeping the other levels in order.
    /// </summary>
    /// <param name="columnId">The column to stop grouping by.</param>
    /// <returns>True when a level was removed.</returns>
    public bool RemoveGroup(string columnId)
    {
        var removed = activeGroups.RemoveAll(
            g => string.Equals(g.ColumnId, columnId, StringComparison.Ordinal)) > 0;

        if (removed)
        {
            collapsedPaths.Clear();
            Version++;
        }

        return removed;
    }

    /// <summary>
    /// Moves a grouping level to a new position, with remove-then-insert semantics.
    /// </summary>
    /// <param name="columnId">The column whose level should move.</param>
    /// <param name="newIndex">
    /// The zero-based position among the remaining levels. Values outside the range are clamped.
    /// </param>
    /// <returns>True when the level moved.</returns>
    public bool MoveGroup(string columnId, int newIndex)
    {
        var currentIndex = activeGroups.FindIndex(
            g => string.Equals(g.ColumnId, columnId, StringComparison.Ordinal));

        if (currentIndex < 0)
        {
            return false;
        }

        var group = activeGroups[currentIndex];
        activeGroups.RemoveAt(currentIndex);
        newIndex = Math.Clamp(newIndex, 0, activeGroups.Count);
        activeGroups.Insert(newIndex, group);

        if (currentIndex == newIndex)
        {
            return false;
        }

        collapsedPaths.Clear();
        Version++;
        return true;
    }

    /// <summary>
    /// Gets the zero-based level a column groups at, or -1 when it does not group.
    /// </summary>
    /// <param name="columnId">The column to look for.</param>
    /// <returns>The level, or -1.</returns>
    public int GetLevel(string columnId) =>
        activeGroups.FindIndex(g => string.Equals(g.ColumnId, columnId, StringComparison.Ordinal));

    /// <summary>
    /// Gets whether a column groups at any level.
    /// </summary>
    /// <param name="columnId">The column to look for.</param>
    /// <returns>True when the column groups.</returns>
    public bool IsGroupedBy(string columnId) => GetLevel(columnId) >= 0;

    /// <summary>
    /// Clears all group definitions and all collapsed state.
    /// </summary>
    public void ClearGroup()
    {
        activeGroups.Clear();
        collapsedPaths.Clear();
        Version++;
    }

    /// <summary>
    /// Checks whether a group is collapsed in its own right.
    /// </summary>
    /// <remarks>
    /// This does not consider ancestors. A group inside a collapsed parent is not itself collapsed,
    /// it is simply not rendered — use <see cref="IsHiddenByCollapse"/> for that question. Keeping
    /// the two apart is what lets a subtree remember its own expansion while its parent is shut.
    /// </remarks>
    /// <param name="path">The group path to check.</param>
    /// <returns>True if the group is collapsed.</returns>
    public bool IsCollapsed(GroupPath path) => collapsedPaths.Contains(path);

    /// <summary>
    /// Checks whether an outermost group is collapsed.
    /// </summary>
    /// <remarks>
    /// Kept for grids written against single-level grouping.
    /// </remarks>
    /// <param name="key">The group key to check.</param>
    /// <returns>True if the group is collapsed.</returns>
    public bool IsCollapsed(object key) => collapsedPaths.Contains(new GroupPath(key));

    /// <summary>
    /// Checks whether any ancestor of a group is collapsed, which is what hides it from the render
    /// list even when it is expanded itself.
    /// </summary>
    /// <param name="path">The group path to check.</param>
    /// <returns>True when an ancestor is collapsed.</returns>
    public bool IsHiddenByCollapse(GroupPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        // Walk up rather than materializing descendants: the number of ancestors is the nesting
        // depth, which is small, whereas the number of descendants is unbounded.
        for (var parent = path.Parent(); parent.Keys.Count > 0; parent = parent.Parent())
        {
            if (collapsedPaths.Contains(parent))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Toggles the collapsed state of a group.
    /// </summary>
    /// <param name="path">The group path to toggle.</param>
    public void Toggle(GroupPath path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!collapsedPaths.Remove(path))
        {
            collapsedPaths.Add(path);
        }
    }

    /// <summary>
    /// Toggles the collapsed state of an outermost group.
    /// </summary>
    /// <remarks>
    /// Kept for grids written against single-level grouping.
    /// </remarks>
    /// <param name="key">The group key to toggle.</param>
    public void Toggle(object key) => Toggle(new GroupPath(key));

    /// <summary>
    /// Expands all groups by clearing the collapsed set.
    /// </summary>
    public void ExpandAll() => collapsedPaths.Clear();

    /// <summary>
    /// Collapses the given groups.
    /// </summary>
    /// <param name="paths">The group paths to collapse.</param>
    public void CollapseAll(IEnumerable<GroupPath> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        foreach (var path in paths)
        {
            collapsedPaths.Add(path);
        }
    }

    /// <summary>
    /// Collapses the given outermost groups.
    /// </summary>
    /// <remarks>
    /// Kept for grids written against single-level grouping.
    /// </remarks>
    /// <param name="keys">The group keys to collapse.</param>
    public void CollapseAll(IEnumerable<object> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys)
        {
            collapsedPaths.Add(new GroupPath(key));
        }
    }

    /// <summary>
    /// Clears all collapsed state and all group definitions.
    /// </summary>
    public void Clear()
    {
        collapsedPaths.Clear();
        activeGroups.Clear();
        Version++;
    }
}
