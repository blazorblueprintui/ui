namespace BlazorBlueprint.Primitives;

/// <summary>
/// Selection mode for a tree view.
/// </summary>
public enum TreeSelectionMode
{
    /// <summary>
    /// No selection allowed.
    /// </summary>
    None,

    /// <summary>
    /// Only one node can be selected at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple nodes can be selected simultaneously.
    /// </summary>
    Multiple
}
