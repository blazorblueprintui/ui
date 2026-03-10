namespace BlazorBlueprint.Components;

/// <summary>
/// Controls how row selection interacts with hierarchy in a <see cref="BbDataGrid{TData}"/>.
/// </summary>
public enum HierarchySelectionMode
{
    /// <summary>
    /// Selection is independent per row. Selecting a parent does not affect children.
    /// This is the default behavior.
    /// </summary>
    Independent = 0,

    /// <summary>
    /// Selecting a parent cascades to all descendants. Deselecting a parent deselects all descendants.
    /// When some (but not all) descendants are selected, the parent checkbox shows an indeterminate state.
    /// Only applies when <see cref="BbDataGrid{TData}.SelectionMode"/> is <see cref="DataTableSelectionMode.Multiple"/>
    /// and hierarchy mode is active.
    /// </summary>
    Cascade = 1
}
