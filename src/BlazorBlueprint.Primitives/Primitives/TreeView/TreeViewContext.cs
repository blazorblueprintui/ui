using BlazorBlueprint.Primitives.Contexts;

namespace BlazorBlueprint.Primitives.TreeView;

/// <summary>
/// Information about a registered tree node.
/// </summary>
internal class TreeNodeInfo
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
    private readonly Dictionary<string, TreeNodeInfo> nodeRegistry = new();
    private int nextOrder;

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
        nodeRegistry[value] = new TreeNodeInfo
        {
            Value = value,
            ParentValue = parentValue,
            Depth = depth,
            Disabled = disabled,
            Order = nextOrder++
        };
    }

    /// <summary>
    /// Unregisters a node from the tree context.
    /// </summary>
    public void UnregisterNode(string value)
    {
        nodeRegistry.Remove(value);
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
        return nodeRegistry.Values
            .Where(n => n.ParentValue == parentValue)
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

        return nodeRegistry.Values
            .Where(n => n.ParentValue == info.ParentValue && n.Value != value)
            .OrderBy(n => n.Order)
            .Select(n => n.Value)
            .ToList();
    }

    /// <summary>
    /// Gets the depth of a node.
    /// </summary>
    public int GetNodeDepth(string value)
    {
        return nodeRegistry.TryGetValue(value, out var info) ? info.Depth : 0;
    }

    /// <summary>
    /// Gets the number of siblings at the same level (including the node itself).
    /// </summary>
    public int GetSetSize(string value)
    {
        if (!nodeRegistry.TryGetValue(value, out var info))
        {
            return 0;
        }

        return nodeRegistry.Values.Count(n => n.ParentValue == info.ParentValue);
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

        var siblings = nodeRegistry.Values
            .Where(n => n.ParentValue == info.ParentValue)
            .OrderBy(n => n.Order)
            .ToList();

        for (int i = 0; i < siblings.Count; i++)
        {
            if (siblings[i].Value == value)
            {
                return i + 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Checks whether a node has registered children.
    /// </summary>
    public bool HasChildren(string value)
    {
        return nodeRegistry.Values.Any(n => n.ParentValue == value);
    }

    /// <summary>
    /// Checks whether a node is registered.
    /// </summary>
    public bool IsNodeRegistered(string value)
    {
        return nodeRegistry.ContainsKey(value);
    }

    /// <summary>
    /// Gets the parent value of a node, or null if root.
    /// </summary>
    public string? GetParentValue(string value)
    {
        return nodeRegistry.TryGetValue(value, out var info) ? info.ParentValue : null;
    }

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

        bool hasChecked = false;
        bool hasUnchecked = false;

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
            bool newChecked = !state.CheckedValues.Contains(value);

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

                bool allChecked = enabledChildren.All(c => state.CheckedValues.Contains(c));
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
