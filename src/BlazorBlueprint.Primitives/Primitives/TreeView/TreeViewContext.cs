using BlazorBlueprint.Primitives.Contexts;

namespace BlazorBlueprint.Primitives.TreeView;

/// <summary>
/// Information about a registered tree node.
/// </summary>
internal sealed class TreeNodeInfo
{
    public string Value { get; set; } = string.Empty;
    public string? ParentValue { get; set; }
    public int Depth { get; set; }
    public bool Disabled { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// Context for TreeView primitive component and its children.
/// Manages expand/collapse, selection, and checkbox state for the tree.
/// </summary>
public class TreeViewContext : PrimitiveContextWithEvents<TreeViewState>
{
    private const string RootParentKey = "\0__root__";
    private readonly Dictionary<string, TreeNodeInfo> nodeRegistry = new();
    private readonly Dictionary<string, List<TreeNodeInfo>> childrenByParent = new();
    private int nextOrder;

    /// <summary>
    /// Gets or sets the currently focused node value for roving tabindex.
    /// Set by JS-invokable methods so Blazor re-renders preserve correct tabindex.
    /// </summary>
    public string? FocusedNodeValue { get; set; }

    private static string ParentKey(string? parentValue) => parentValue ?? RootParentKey;

    /// <summary>
    /// Initializes a new instance of the TreeViewContext.
    /// </summary>
    public TreeViewContext() : base(new TreeViewState(), "tree")
    {
    }

    /// <summary>
    /// Gets the ARIA ID for a specific tree node.
    /// </summary>
    public string GetNodeId(string value) => GetScopedId($"node-{value}");

    // --- Node registry ---

    /// <summary>
    /// Registers a node with the tree context.
    /// </summary>
    public void RegisterNode(string value, string? parentValue, int depth, bool disabled)
    {
        // Remove from old parent index if re-registering
        if (nodeRegistry.TryGetValue(value, out var existing))
        {
            RemoveFromChildrenIndex(existing);
        }

        var info = new TreeNodeInfo
        {
            Value = value,
            ParentValue = parentValue,
            Depth = depth,
            Disabled = disabled,
            Order = nextOrder++
        };
        nodeRegistry[value] = info;

        // Add to parent-to-children index
        var key = ParentKey(parentValue);
        if (!childrenByParent.TryGetValue(key, out var siblings))
        {
            siblings = new List<TreeNodeInfo>();
            childrenByParent[key] = siblings;
        }
        siblings.Add(info);
    }

    /// <summary>
    /// Unregisters a node from the tree context.
    /// </summary>
    public void UnregisterNode(string value)
    {
        if (nodeRegistry.TryGetValue(value, out var info))
        {
            RemoveFromChildrenIndex(info);
            nodeRegistry.Remove(value);
        }
    }

    private void RemoveFromChildrenIndex(TreeNodeInfo info)
    {
        var key = ParentKey(info.ParentValue);
        if (childrenByParent.TryGetValue(key, out var siblings))
        {
            siblings.Remove(info);
            if (siblings.Count == 0)
            {
                childrenByParent.Remove(key);
            }
        }
    }

    /// <summary>
    /// Updates the disabled state of a registered node.
    /// </summary>
    public void UpdateNodeDisabled(string value, bool disabled)
    {
        if (nodeRegistry.TryGetValue(value, out var info))
        {
            info.Disabled = disabled;
        }
    }

    /// <summary>
    /// Gets all child values of a given parent node.
    /// </summary>
    public List<string> GetChildValues(string? parentValue)
    {
        if (!childrenByParent.TryGetValue(ParentKey(parentValue), out var children))
        {
            return new List<string>();
        }

        return children
            .OrderBy(n => n.Order)
            .Select(n => n.Value)
            .ToList();
    }

    /// <summary>
    /// Gets all descendant values of a given node recursively.
    /// </summary>
    public List<string> GetAllDescendants(string value)
    {
        var result = new List<string>();
        CollectDescendants(value, result);
        return result;
    }

    private void CollectDescendants(string value, List<string> result)
    {
        var children = GetChildValues(value);
        foreach (var child in children)
        {
            result.Add(child);
            CollectDescendants(child, result);
        }
    }

    /// <summary>
    /// Gets the ancestor chain of a node (parent, grandparent, etc.), bottom-up.
    /// </summary>
    public List<string> GetAncestors(string value)
    {
        var result = new List<string>();
        if (!nodeRegistry.TryGetValue(value, out var info))
        {
            return result;
        }

        var current = info.ParentValue;
        while (current != null)
        {
            result.Add(current);
            if (!nodeRegistry.TryGetValue(current, out var parentInfo))
            {
                break;
            }
            current = parentInfo.ParentValue;
        }

        return result;
    }

    /// <summary>
    /// Gets sibling values of a node (nodes with the same parent), excluding itself.
    /// </summary>
    public List<string> GetSiblingValues(string value)
    {
        if (!nodeRegistry.TryGetValue(value, out var info))
        {
            return new List<string>();
        }

        if (!childrenByParent.TryGetValue(ParentKey(info.ParentValue), out var siblings))
        {
            return new List<string>();
        }

        return siblings
            .Where(n => n.Value != value)
            .OrderBy(n => n.Order)
            .Select(n => n.Value)
            .ToList();
    }

    /// <summary>
    /// Gets the depth of a node.
    /// </summary>
    public int GetNodeDepth(string value) =>
        nodeRegistry.TryGetValue(value, out var info) ? info.Depth : 0;

    /// <summary>
    /// Gets the number of siblings at the same level (including the node itself).
    /// </summary>
    public int GetSetSize(string value)
    {
        if (!nodeRegistry.TryGetValue(value, out var info))
        {
            return 0;
        }

        return childrenByParent.TryGetValue(ParentKey(info.ParentValue), out var siblings) ? siblings.Count : 0;
    }

    /// <summary>
    /// Gets the 1-based position of a node among its siblings.
    /// </summary>
    public int GetPosInSet(string value)
    {
        if (!nodeRegistry.TryGetValue(value, out var info))
        {
            return 0;
        }

        if (!childrenByParent.TryGetValue(ParentKey(info.ParentValue), out var siblings))
        {
            return 0;
        }

        var ordered = siblings.OrderBy(n => n.Order).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Value == value)
            {
                return i + 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Checks whether a node has registered children.
    /// </summary>
    public bool HasChildren(string value) =>
        childrenByParent.TryGetValue(value, out var children) && children.Count > 0;

    /// <summary>
    /// Returns true if the given value is the first enabled root node (for roving tabindex).
    /// </summary>
    public bool IsFirstEnabledRoot(string value)
    {
        if (!childrenByParent.TryGetValue(RootParentKey, out var roots))
        {
            return false;
        }

        var firstEnabled = roots
            .Where(n => !n.Disabled)
            .OrderBy(n => n.Order)
            .FirstOrDefault();

        return firstEnabled?.Value == value;
    }

    /// <summary>
    /// Checks whether a node is registered.
    /// </summary>
    public bool IsNodeRegistered(string value) =>
        nodeRegistry.ContainsKey(value);

    /// <summary>
    /// Gets the parent value of a node, or null if root.
    /// </summary>
    public string? GetParentValue(string value) =>
        nodeRegistry.TryGetValue(value, out var info) ? info.ParentValue : null;

    // --- Expand/Collapse ---

    /// <summary>
    /// Checks if the specified node is currently expanded.
    /// </summary>
    public bool IsExpanded(string value) => State.ExpandedValues.Contains(value);

    /// <summary>
    /// Toggles a node's expanded state.
    /// </summary>
    public void ToggleExpanded(string value)
    {
        UpdateState(state =>
        {
            if (!state.ExpandedValues.Remove(value))
            {
                state.ExpandedValues.Add(value);
            }
        });
    }

    /// <summary>
    /// Expands a node.
    /// </summary>
    public void ExpandNode(string value)
    {
        if (!State.ExpandedValues.Contains(value))
        {
            UpdateState(state => state.ExpandedValues.Add(value));
        }
    }

    /// <summary>
    /// Collapses a node.
    /// </summary>
    public void CollapseNode(string value)
    {
        if (State.ExpandedValues.Contains(value))
        {
            UpdateState(state => state.ExpandedValues.Remove(value));
        }
    }

    /// <summary>
    /// Expands all siblings of a given node.
    /// </summary>
    public void ExpandSiblings(string value)
    {
        var siblings = GetSiblingValues(value);
        UpdateState(state =>
        {
            foreach (var sibling in siblings)
            {
                if (HasChildren(sibling))
                {
                    state.ExpandedValues.Add(sibling);
                }
            }
            if (HasChildren(value))
            {
                state.ExpandedValues.Add(value);
            }
        });
    }

    /// <summary>
    /// Sets the expanded values. Used for controlled state.
    /// </summary>
    public void SetExpandedValues(HashSet<string> values) =>
        UpdateState(state => state.ExpandedValues = new HashSet<string>(values));

    /// <summary>
    /// Expands all registered nodes that have children.
    /// </summary>
    public void ExpandAllWithChildren()
    {
        // Build set of all parent values directly from the index keys
        var allExpandable = new HashSet<string>();
        foreach (var kvp in childrenByParent)
        {
            if (kvp.Key != RootParentKey && kvp.Value.Count > 0)
            {
                allExpandable.Add(kvp.Key);
            }
        }

        if (allExpandable.Count > 0)
        {
            UpdateState(state => state.ExpandedValues = allExpandable);
        }
    }

    // --- Selection ---

    /// <summary>
    /// Gets the selection mode.
    /// </summary>
    public TreeSelectionMode SelectionMode => State.SelectionMode;

    /// <summary>
    /// Checks if the specified node is selected.
    /// </summary>
    public bool IsSelected(string value) => State.SelectedValues.Contains(value);

    /// <summary>
    /// Selects a node, respecting the current selection mode.
    /// </summary>
    public void SelectNode(string value)
    {
        if (State.SelectionMode == TreeSelectionMode.None)
        {
            return;
        }

        if (nodeRegistry.TryGetValue(value, out var info) && info.Disabled)
        {
            return;
        }

        UpdateState(state =>
        {
            if (state.SelectionMode == TreeSelectionMode.Single)
            {
                state.SelectedValues.Clear();
                state.SelectedValues.Add(value);
            }
            else if (state.SelectionMode == TreeSelectionMode.Multiple)
            {
                if (!state.SelectedValues.Remove(value))
                {
                    state.SelectedValues.Add(value);
                }
            }
        });
    }

    /// <summary>
    /// Sets the selected values. Used for controlled state.
    /// </summary>
    public void SetSelectedValues(HashSet<string> values) =>
        UpdateState(state => state.SelectedValues = new HashSet<string>(values));

    // --- Checkable ---

    /// <summary>
    /// Gets whether checkboxes are enabled.
    /// </summary>
    public bool Checkable => State.Checkable;

    /// <summary>
    /// Gets whether cascade is disabled.
    /// </summary>
    public bool CheckStrictly => State.CheckStrictly;

    /// <summary>
    /// Checks if the specified node is checked.
    /// </summary>
    public bool IsChecked(string value) => State.CheckedValues.Contains(value);

    /// <summary>
    /// Checks if the specified node is in indeterminate state
    /// (some but not all descendants are checked).
    /// </summary>
    public bool IsIndeterminate(string value)
    {
        if (State.CheckStrictly || !HasChildren(value))
        {
            return false;
        }

        var descendants = GetAllDescendants(value);
        if (descendants.Count == 0)
        {
            return false;
        }

        var hasChecked = false;
        var hasUnchecked = false;

        foreach (var d in descendants)
        {
            if (nodeRegistry.TryGetValue(d, out var info) && info.Disabled)
            {
                continue;
            }

            if (State.CheckedValues.Contains(d))
            {
                hasChecked = true;
            }
            else
            {
                hasUnchecked = true;
            }

            if (hasChecked && hasUnchecked)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Toggles a node's checked state with cascade logic.
    /// </summary>
    public void ToggleChecked(string value)
    {
        if (!State.Checkable)
        {
            return;
        }

        if (nodeRegistry.TryGetValue(value, out var info) && info.Disabled)
        {
            return;
        }

        UpdateState(state =>
        {
            var newChecked = !state.CheckedValues.Contains(value);

            if (state.CheckStrictly)
            {
                if (newChecked)
                {
                    state.CheckedValues.Add(value);
                }
                else
                {
                    state.CheckedValues.Remove(value);
                }
                return;
            }

            // Cascade down: check/uncheck all descendants
            var descendants = GetAllDescendants(value);
            if (newChecked)
            {
                state.CheckedValues.Add(value);
                foreach (var d in descendants)
                {
                    if (!nodeRegistry.TryGetValue(d, out var dInfo) || !dInfo.Disabled)
                    {
                        state.CheckedValues.Add(d);
                    }
                }
            }
            else
            {
                state.CheckedValues.Remove(value);
                foreach (var d in descendants)
                {
                    if (!nodeRegistry.TryGetValue(d, out var dInfo) || !dInfo.Disabled)
                    {
                        state.CheckedValues.Remove(d);
                    }
                }
            }

            // Cascade up: recompute parent states
            var ancestors = GetAncestors(value);
            foreach (var ancestor in ancestors)
            {
                if (nodeRegistry.TryGetValue(ancestor, out var aInfo) && aInfo.Disabled)
                {
                    continue;
                }

                var children = GetChildValues(ancestor);
                var enabledChildren = children.Where(c =>
                    !nodeRegistry.TryGetValue(c, out var cInfo) || !cInfo.Disabled).ToList();

                if (enabledChildren.Count == 0)
                {
                    continue;
                }

                var allChecked = enabledChildren.All(c => state.CheckedValues.Contains(c));
                if (allChecked)
                {
                    state.CheckedValues.Add(ancestor);
                }
                else
                {
                    state.CheckedValues.Remove(ancestor);
                }
            }
        });
    }

    /// <summary>
    /// Sets the checked values. Used for controlled state.
    /// </summary>
    public void SetCheckedValues(HashSet<string> values) =>
        UpdateState(state => state.CheckedValues = new HashSet<string>(values));
}
