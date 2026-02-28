namespace BlazorBlueprint.Components;

/// <summary>
/// Specifies the sort order for funnel chart segments.
/// </summary>
public enum FunnelSort
{
    /// <summary>
    /// Largest segment at the top, smallest at the bottom.
    /// </summary>
    Descending,

    /// <summary>
    /// Smallest segment at the top, largest at the bottom.
    /// </summary>
    Ascending,

    /// <summary>
    /// Segments appear in data order without sorting.
    /// </summary>
    None
}
