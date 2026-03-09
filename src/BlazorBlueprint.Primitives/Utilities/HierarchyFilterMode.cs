namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Controls how hierarchy filtering treats descendants of matching nodes.
/// </summary>
public enum HierarchyFilterMode
{
    /// <summary>
    /// When a parent matches the filter, show its subtree structure with descendants
    /// visible as context. The filter is still applied — descendants that fail the filter
    /// are excluded, but non-matching intermediary nodes are shown at reduced opacity
    /// to preserve the tree structure. This is the default behavior, useful when a match
    /// at a higher level implies the subtree is relevant but individual items should
    /// still be filtered (e.g., filtering by "Engineer" under a matching VP shows the
    /// org structure but still excludes Interns).
    /// </summary>
    ShowMatchedSubtree,

    /// <summary>
    /// Only show items that directly match the filter, plus their ancestors for context.
    /// Descendants of matching items are NOT automatically included unless they also match.
    /// This is useful for strict per-item filtering (e.g., filtering by job title should
    /// only show people with that specific title).
    /// </summary>
    ShowMatchedOnly
}
