namespace BlazorBlueprint.Primitives.TreeView;

/// <summary>
/// State for the TreeView primitive context.
/// </summary>
public class TreeViewState
{
    /// <summary>
    /// Gets or sets the set of expanded node values.
    /// </summary>
    public HashSet<string> ExpandedValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the set of selected node values.
    /// </summary>
    public HashSet<string> SelectedValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the set of checked node values.
    /// </summary>
    public HashSet<string> CheckedValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the selection mode.
    /// </summary>
    public TreeSelectionMode SelectionMode { get; set; } = TreeSelectionMode.None;

    /// <summary>
    /// Gets or sets whether nodes show checkboxes.
    /// </summary>
    public bool Checkable { get; set; }

    /// <summary>
    /// Gets or sets whether checkbox cascade is disabled (each node independent).
    /// </summary>
    public bool CheckStrictly { get; set; }

    /// <summary>
    /// Gets or sets whether connector lines are shown.
    /// </summary>
    public bool ShowLines { get; set; }

    /// <summary>
    /// Gets or sets whether icons are shown (default: true).
    /// </summary>
    public bool ShowIcons { get; set; } = true;
}
