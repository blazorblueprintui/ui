using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Represents a group header, data row, or child pager row in the flattened render list.
/// Used when grouping or hierarchy mode is active to interleave special rows with data rows.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
    Justification = "Factory methods for discriminated union pattern")]
public class DataGridRenderItem<TData> where TData : class
{
    private DataGridRenderItem()
    {
    }

    /// <summary>
    /// Gets whether this render item is a group header row.
    /// </summary>
    public bool IsGroupHeader { get; private init; }

    /// <summary>
    /// Gets whether this render item is a data row.
    /// </summary>
    public bool IsDataRow { get; private init; }

    /// <summary>
    /// Gets whether this render item is a child pagination control row (hierarchy mode).
    /// </summary>
    public bool IsChildPagerRow { get; private init; }

    /// <summary>
    /// Gets the data item. Non-null when <see cref="IsDataRow"/> is true.
    /// </summary>
    public TData? Item { get; private init; }

    /// <summary>
    /// Gets the group row. Non-null when <see cref="IsGroupHeader"/> is true.
    /// </summary>
    public DataGridGroupRow<TData>? GroupRow { get; private init; }

    // --- Hierarchy metadata (defaults to 0/false/null for flat data) ---

    /// <summary>
    /// Gets the depth level of this item in the hierarchy (0 = root). Only meaningful in hierarchy mode.
    /// </summary>
    public int Depth { get; private init; }

    /// <summary>
    /// Gets whether this item has children in the hierarchy.
    /// </summary>
    public bool HasChildren { get; private init; }

    /// <summary>
    /// Gets whether this hierarchy node is currently expanded.
    /// </summary>
    public bool IsExpanded { get; private init; }

    /// <summary>
    /// Gets whether this item directly matches the active filter in hierarchy mode.
    /// False when shown only as an ancestor providing structural context.
    /// Defaults to true for non-hierarchy grids.
    /// </summary>
    public bool MatchesFilter { get; private init; } = true;

    /// <summary>
    /// Gets the parent node value for child pager rows in hierarchy mode.
    /// </summary>
    public string? ParentValue { get; private init; }

    /// <summary>
    /// Gets the current child page index (0-based) for child pager rows.
    /// </summary>
    public int ChildPageIndex { get; private init; }

    /// <summary>
    /// Gets the total number of children for the parent of a child pager row.
    /// </summary>
    public int TotalChildren { get; private init; }

    /// <summary>
    /// Creates a render item for a data row.
    /// </summary>
    public static DataGridRenderItem<TData> ForData(TData item) => new()
    {
        IsDataRow = true,
        Item = item
    };

    /// <summary>
    /// Creates a render item for a data row with hierarchy metadata.
    /// </summary>
    public static DataGridRenderItem<TData> ForHierarchyData(
        TData item, int depth, bool hasChildren, bool isExpanded, bool matchesFilter) => new()
    {
        IsDataRow = true,
        Item = item,
        Depth = depth,
        HasChildren = hasChildren,
        IsExpanded = isExpanded,
        MatchesFilter = matchesFilter
    };

    /// <summary>
    /// Creates a render item for a group header.
    /// </summary>
    public static DataGridRenderItem<TData> ForGroup(DataGridGroupRow<TData> group) => new()
    {
        IsGroupHeader = true,
        GroupRow = group
    };

    /// <summary>
    /// Creates a render item for a child pagination control row in hierarchy mode.
    /// </summary>
    public static DataGridRenderItem<TData> ForChildPager(
        int depth, string parentValue, int childPageIndex, int totalChildren) => new()
    {
        IsChildPagerRow = true,
        Depth = depth,
        ParentValue = parentValue,
        ChildPageIndex = childPageIndex,
        TotalChildren = totalChildren
    };
}
