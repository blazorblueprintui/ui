namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Specifies whether a DataGrid column is pinned to an edge of the scrollable viewport.
/// Pinned columns use CSS <c>position: sticky</c> with automatically computed offsets.
/// </summary>
public enum ColumnPinning
{
    /// <summary>The column is not pinned and scrolls normally.</summary>
    None,

    /// <summary>The column is pinned to the left edge of the grid.</summary>
    Left,

    /// <summary>The column is pinned to the right edge of the grid.</summary>
    Right
}
