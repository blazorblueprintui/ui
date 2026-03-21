using Microsoft.Extensions.Logging;

namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Generic utility class for managing hierarchical (tree) data structures.
/// Provides indexing, traversal, and flattening operations with support for
/// sorting, filtering, and child-level pagination.
/// </summary>
/// <typeparam name="TItem">The type of items in the hierarchy.</typeparam>
public partial class HierarchyManager<TItem>
{
    private const string RootParentKey = "\0__root__";

    private readonly Func<TItem, string> valueSelector;
    private readonly ILogger? logger;
    private readonly Dictionary<string, TItem> itemsByValue = new();
    private readonly Dictionary<string, string?> parentByValue = new();
    private readonly Dictionary<string, List<string>> childrenByParent = new();
    private readonly Dictionary<string, int> depthByValue = new();

    /// <summary>
    /// Creates a new HierarchyManager with the specified value selector.
    /// </summary>
    /// <param name="valueSelector">Function that extracts a unique string identifier from each item.</param>
    /// <param name="logger">Optional logger for reporting duplicate key warnings.</param>
    public HierarchyManager(Func<TItem, string> valueSelector, ILogger? logger = null)
    {
        this.valueSelector = valueSelector ?? throw new ArgumentNullException(nameof(valueSelector));
        this.logger = logger;
    }

    // --- Indexing ---

    /// <summary>
    /// Builds the hierarchy from nested data where each item may have child items.
    /// </summary>
    /// <param name="rootItems">The top-level items.</param>
    /// <param name="childrenSelector">Function that returns the children of an item, or null/empty if none.</param>
    public void BuildFromNested(
        IEnumerable<TItem> rootItems,
        Func<TItem, IEnumerable<TItem>?> childrenSelector)
    {
        Clear();
        foreach (var item in rootItems)
        {
            IndexNested(item, null, 0, childrenSelector);
        }
    }

    private void IndexNested(
        TItem item, string? parentValue, int depth,
        Func<TItem, IEnumerable<TItem>?> childrenSelector)
    {
        var value = valueSelector(item);
        if (!RegisterItem(value, item, parentValue, depth))
        {
            return;
        }

        var children = childrenSelector(item);
        if (children != null)
        {
            foreach (var child in children)
            {
                IndexNested(child, value, depth + 1, childrenSelector);
            }
        }
    }

    /// <summary>
    /// Builds the hierarchy from a flat list where each item has a parent reference.
    /// </summary>
    /// <param name="allItems">All items in the hierarchy.</param>
    /// <param name="parentValueSelector">Function that returns the parent's value, or null for root items.</param>
    public void BuildFromFlat(
        IEnumerable<TItem> allItems,
        Func<TItem, string?> parentValueSelector)
    {
        Clear();

        // First pass: register all items without depth (skip duplicates)
        var items = allItems as IList<TItem> ?? allItems.ToList();
        foreach (var item in items)
        {
            var value = valueSelector(item);
            if (itemsByValue.ContainsKey(value))
            {
                if (logger != null)
            {
                Log.DuplicateKeyDetected(logger, value);
            }
                continue;
            }

            var parentVal = parentValueSelector(item);
            itemsByValue[value] = item;
            parentByValue[value] = parentVal;

            var parentKey = parentVal ?? RootParentKey;
            if (!childrenByParent.TryGetValue(parentKey, out var children))
            {
                children = new List<string>();
                childrenByParent[parentKey] = children;
            }
            children.Add(value);
        }

        // Second pass: compute depths
        foreach (var value in itemsByValue.Keys)
        {
            depthByValue[value] = ComputeDepth(value);
        }
    }

    private int ComputeDepth(string value)
    {
        if (depthByValue.TryGetValue(value, out var cached))
        {
            return cached;
        }

        if (!parentByValue.TryGetValue(value, out var parentVal) || parentVal == null)
        {
            return 0;
        }

        return ComputeDepth(parentVal) + 1;
    }

    private bool RegisterItem(string value, TItem item, string? parentValue, int depth)
    {
        if (itemsByValue.ContainsKey(value))
        {
            if (logger != null)
            {
                Log.DuplicateKeyDetected(logger, value);
            }
            return false;
        }

        itemsByValue[value] = item;
        parentByValue[value] = parentValue;
        depthByValue[value] = depth;

        var parentKey = parentValue ?? RootParentKey;
        if (!childrenByParent.TryGetValue(parentKey, out var children))
        {
            children = new List<string>();
            childrenByParent[parentKey] = children;
        }
        children.Add(value);
        return true;
    }

    /// <summary>
    /// Adds children to an already-indexed parent node. Used for lazy loading.
    /// </summary>
    /// <param name="parentValue">The parent node's value.</param>
    /// <param name="children">The children to add.</param>
    /// <param name="childrenSelector">Optional nested children selector for recursive indexing.</param>
    public void AddChildren(
        string parentValue,
        IEnumerable<TItem> children,
        Func<TItem, IEnumerable<TItem>?>? childrenSelector = null)
    {
        var parentDepth = GetDepth(parentValue);

        // Remove existing children for this parent
        if (childrenByParent.TryGetValue(parentValue, out var existingChildren))
        {
            foreach (var childVal in existingChildren.ToList())
            {
                RemoveSubtree(childVal);
            }
            existingChildren.Clear();
        }

        foreach (var child in children)
        {
            if (childrenSelector != null)
            {
                IndexNested(child, parentValue, parentDepth + 1, childrenSelector);
            }
            else
            {
                var value = valueSelector(child);
                RegisterItem(value, child, parentValue, parentDepth + 1);
            }
        }
    }

    private void RemoveSubtree(string value)
    {
        // Recursively remove children first
        if (childrenByParent.TryGetValue(value, out var children))
        {
            foreach (var child in children.ToList())
            {
                RemoveSubtree(child);
            }
            childrenByParent.Remove(value);
        }

        itemsByValue.Remove(value);
        parentByValue.Remove(value);
        depthByValue.Remove(value);
    }

    /// <summary>
    /// Clears all indexed data.
    /// </summary>
    public void Clear()
    {
        itemsByValue.Clear();
        parentByValue.Clear();
        childrenByParent.Clear();
        depthByValue.Clear();
    }

    // --- Lookups ---

    /// <summary>
    /// Gets an item by its unique value.
    /// </summary>
    public TItem? GetItemByValue(string value) =>
        itemsByValue.TryGetValue(value, out var item) ? item : default;

    /// <summary>
    /// Gets the direct children of a parent. Pass null for root items.
    /// </summary>
    public IReadOnlyList<TItem> GetChildren(string? parentValue)
    {
        var key = parentValue ?? RootParentKey;
        if (!childrenByParent.TryGetValue(key, out var childValues))
        {
            return Array.Empty<TItem>();
        }

        var result = new List<TItem>(childValues.Count);
        foreach (var cv in childValues)
        {
            if (itemsByValue.TryGetValue(cv, out var item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the root-level items.
    /// </summary>
    public IReadOnlyList<TItem> GetRootItems() => GetChildren(null);

    /// <summary>
    /// Gets the parent's value for an item, or null if it's a root item.
    /// </summary>
    public string? GetParentValue(string value) =>
        parentByValue.TryGetValue(value, out var pv) ? pv : null;

    /// <summary>
    /// Gets the depth of an item (0 = root).
    /// </summary>
    public int GetDepth(string value) =>
        depthByValue.TryGetValue(value, out var d) ? d : 0;

    /// <summary>
    /// Gets whether an item has children.
    /// </summary>
    public bool HasChildren(string value) =>
        childrenByParent.TryGetValue(value, out var children) && children.Count > 0;

    /// <summary>
    /// Gets the total number of direct children for a parent.
    /// </summary>
    public int GetChildCount(string parentValue) =>
        childrenByParent.TryGetValue(parentValue, out var children) ? children.Count : 0;

    /// <summary>
    /// Gets whether an item is registered in the hierarchy.
    /// </summary>
    public bool IsRegistered(string value) => itemsByValue.ContainsKey(value);

    /// <summary>
    /// Gets the total number of items in the hierarchy.
    /// </summary>
    public int Count => itemsByValue.Count;

    /// <summary>
    /// Gets all items in the hierarchy (unordered).
    /// </summary>
    public IEnumerable<TItem> GetAllItems() => itemsByValue.Values;

    /// <summary>
    /// Gets the direct child values of a parent. Pass null for root items.
    /// </summary>
    public IReadOnlyList<string> GetDirectChildValues(string? parentValue)
    {
        var key = parentValue ?? RootParentKey;
        return childrenByParent.TryGetValue(key, out var children)
            ? children
            : Array.Empty<string>();
    }

    // --- Traversal ---

    /// <summary>
    /// Gets all descendant values of a node recursively.
    /// </summary>
    public IReadOnlyList<string> GetAllDescendantValues(string value)
    {
        var result = new List<string>();
        CollectDescendants(value, result);
        return result;
    }

    private void CollectDescendants(string value, List<string> result)
    {
        if (!childrenByParent.TryGetValue(value, out var children))
        {
            return;
        }

        foreach (var child in children)
        {
            result.Add(child);
            CollectDescendants(child, result);
        }
    }

    /// <summary>
    /// Gets the ancestor chain of a node (parent, grandparent, ...), bottom-up.
    /// </summary>
    public IReadOnlyList<string> GetAncestorValues(string value)
    {
        var result = new List<string>();
        if (!parentByValue.TryGetValue(value, out var current))
        {
            return result;
        }

        while (current != null)
        {
            result.Add(current);
            if (!parentByValue.TryGetValue(current, out current))
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets sibling values of a node (nodes with the same parent), excluding itself.
    /// </summary>
    public IReadOnlyList<string> GetSiblingValues(string value)
    {
        if (!parentByValue.TryGetValue(value, out var parentVal))
        {
            return Array.Empty<string>();
        }

        var key = parentVal ?? RootParentKey;
        if (!childrenByParent.TryGetValue(key, out var siblings))
        {
            return Array.Empty<string>();
        }

        return siblings.Where(s => s != value).ToList();
    }

    /// <summary>
    /// Gets the number of siblings at the same level (including the node itself).
    /// </summary>
    public int GetSetSize(string value)
    {
        if (!parentByValue.TryGetValue(value, out var parentVal))
        {
            return 0;
        }

        var key = parentVal ?? RootParentKey;
        return childrenByParent.TryGetValue(key, out var siblings) ? siblings.Count : 0;
    }

    /// <summary>
    /// Gets the 1-based position of a node among its siblings.
    /// </summary>
    public int GetPosInSet(string value)
    {
        if (!parentByValue.TryGetValue(value, out var parentVal))
        {
            return 0;
        }

        var key = parentVal ?? RootParentKey;
        if (!childrenByParent.TryGetValue(key, out var siblings))
        {
            return 0;
        }

        for (var i = 0; i < siblings.Count; i++)
        {
            if (siblings[i] == value)
            {
                return i + 1;
            }
        }

        return 0;
    }

    // --- Flattening ---

    /// <summary>
    /// Flattens the hierarchy into a display-ordered list, respecting expansion state,
    /// sorting, filtering, and child pagination.
    /// </summary>
    /// <param name="expandedValues">Set of expanded node values.</param>
    /// <param name="sortComparison">Optional comparator for sorting siblings at each level.</param>
    /// <param name="filter">Optional filter predicate. When set, items not matching but having matching descendants are included as context.</param>
    /// <param name="childPageSize">Optional page size for child-level pagination. Null means show all children.</param>
    /// <param name="childPageIndexes">Per-node child page indexes (node value → 0-based page index).</param>
    /// <param name="hasChildrenPredicate">Optional predicate to determine if an item has children (for lazy loading where children may not be indexed yet).</param>
    /// <param name="filterMode">Controls how descendants of matching items are handled. Default is <see cref="HierarchyFilterMode.ShowMatchedSubtree"/>.</param>
    /// <returns>Flattened list of items with hierarchy metadata.</returns>
    public IReadOnlyList<HierarchyFlattenResult<TItem>> Flatten(
        HashSet<string> expandedValues,
        Comparison<TItem>? sortComparison = null,
        Func<TItem, bool>? filter = null,
        int? childPageSize = null,
        IReadOnlyDictionary<string, int>? childPageIndexes = null,
        Func<TItem, bool>? hasChildrenPredicate = null,
        HierarchyFilterMode filterMode = HierarchyFilterMode.ShowMatchedSubtree)
    {
        var result = new List<HierarchyFlattenResult<TItem>>();

        // Pre-compute filter matches if a filter is active
        HashSet<string>? matchingValues = null;
        HashSet<string>? valuesWithMatchingDescendants = null;
        if (filter != null)
        {
            matchingValues = new HashSet<string>();
            valuesWithMatchingDescendants = new HashSet<string>();

            foreach (var kvp in itemsByValue)
            {
                if (filter(kvp.Value))
                {
                    matchingValues.Add(kvp.Key);
                }
            }

            // Mark ancestors of matching items
            foreach (var matchValue in matchingValues)
            {
                var ancestors = GetAncestorValues(matchValue);
                foreach (var ancestor in ancestors)
                {
                    valuesWithMatchingDescendants.Add(ancestor);
                }
            }

            // In ShowMatchedSubtree mode, when a parent matches the filter,
            // mark all its descendants as "having matching descendants" so they
            // are visible as context within the subtree. They still won't appear
            // in matchingValues unless they independently match the filter.
            if (filterMode == HierarchyFilterMode.ShowMatchedSubtree)
            {
                foreach (var matchValue in matchingValues)
                {
                    if (HasChildren(matchValue))
                    {
                        var descendants = GetAllDescendantValues(matchValue);
                        foreach (var desc in descendants)
                        {
                            valuesWithMatchingDescendants.Add(desc);
                        }
                    }
                }
            }
        }

        var rootChildren = GetChildValues(RootParentKey);
        var rootItems = ResolveItems(rootChildren);

        if (sortComparison != null)
        {
            rootItems.Sort(sortComparison);
        }

        FlattenRecursive(
            result, rootItems, 0, expandedValues,
            sortComparison, filter, matchingValues, valuesWithMatchingDescendants,
            childPageSize, childPageIndexes, hasChildrenPredicate, filterMode);

        return result;
    }

    private List<string> GetChildValues(string parentKey)
    {
        return childrenByParent.TryGetValue(parentKey, out var children)
            ? children
            : new List<string>();
    }

    private List<TItem> ResolveItems(List<string> values)
    {
        var items = new List<TItem>(values.Count);
        foreach (var v in values)
        {
            if (itemsByValue.TryGetValue(v, out var item))
            {
                items.Add(item);
            }
        }
        return items;
    }

    private void FlattenRecursive(
        List<HierarchyFlattenResult<TItem>> result,
        List<TItem> items,
        int depth,
        HashSet<string> expandedValues,
        Comparison<TItem>? sortComparison,
        Func<TItem, bool>? filter,
        HashSet<string>? matchingValues,
        HashSet<string>? valuesWithMatchingDescendants,
        int? childPageSize,
        IReadOnlyDictionary<string, int>? childPageIndexes,
        Func<TItem, bool>? hasChildrenPredicate,
        HierarchyFilterMode filterMode)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var value = valueSelector(item);
            var itemHasChildren = HasChildren(value) ||
                (hasChildrenPredicate != null && hasChildrenPredicate(item));
            var isExpanded = expandedValues.Contains(value);
            var isLastChild = i == items.Count - 1;

            // Filter logic: skip items that don't match and have no matching descendants
            var isFilterActive = filter != null && matchingValues != null && valuesWithMatchingDescendants != null;
            var directlyMatchesFilter = isFilterActive && matchingValues!.Contains(value);

            if (isFilterActive)
            {
                var hasMatchingDesc = valuesWithMatchingDescendants!.Contains(value);

                if (!directlyMatchesFilter && !hasMatchingDesc)
                {
                    continue;
                }

                result.Add(new HierarchyFlattenResult<TItem>
                {
                    Item = item,
                    Depth = depth,
                    HasChildren = itemHasChildren,
                    IsExpanded = isExpanded && itemHasChildren,
                    IsLastChild = isLastChild,
                    MatchesFilter = directlyMatchesFilter
                });
            }
            else
            {
                result.Add(new HierarchyFlattenResult<TItem>
                {
                    Item = item,
                    Depth = depth,
                    HasChildren = itemHasChildren,
                    IsExpanded = isExpanded && itemHasChildren,
                    IsLastChild = isLastChild,
                    MatchesFilter = true
                });
            }

            // Recurse into children if expanded
            if (isExpanded && itemHasChildren)
            {
                var childValues = GetChildValues(value);
                var childItems = ResolveItems(childValues);

                if (sortComparison != null)
                {
                    childItems.Sort(sortComparison);
                }

                // When filtering is active, disable child pagination so all
                // matching/context children are visible without paging confusion.
                var effectivePageSize = isFilterActive ? null : childPageSize;

                // Apply child pagination (only when no filter is active)
                var totalChildren = childItems.Count;
                if (effectivePageSize.HasValue && totalChildren > effectivePageSize.Value)
                {
                    var pageIndex = 0;
                    if (childPageIndexes != null && childPageIndexes.TryGetValue(value, out var pi))
                    {
                        pageIndex = pi;
                    }

                    var skip = pageIndex * effectivePageSize.Value;
                    var take = Math.Min(effectivePageSize.Value, totalChildren - skip);

                    // Only emit the current page of children
                    var pagedChildren = childItems.GetRange(skip, take);

                    FlattenRecursive(
                        result, pagedChildren, depth + 1, expandedValues,
                        sortComparison, filter, matchingValues, valuesWithMatchingDescendants,
                        effectivePageSize, childPageIndexes, hasChildrenPredicate, filterMode);

                    // Append child pager row
                    result.Add(new HierarchyFlattenResult<TItem>
                    {
                        Depth = depth + 1,
                        IsChildPagerRow = true,
                        ParentValue = value,
                        ChildPageIndex = pageIndex,
                        TotalChildren = totalChildren,
                        MatchesFilter = true
                    });
                }
                else
                {
                    FlattenRecursive(
                        result, childItems, depth + 1, expandedValues,
                        sortComparison, filter, matchingValues, valuesWithMatchingDescendants,
                        effectivePageSize, childPageIndexes, hasChildrenPredicate, filterMode);
                }
            }
        }
    }

    // --- Expansion helpers ---

    /// <summary>
    /// Gets a set of all node values that have children (for expand-all).
    /// </summary>
    public HashSet<string> GetExpandAllValues()
    {
        var result = new HashSet<string>();
        foreach (var kvp in childrenByParent)
        {
            if (kvp.Key != RootParentKey && kvp.Value.Count > 0)
            {
                result.Add(kvp.Key);
            }
        }
        return result;
    }

    /// <summary>
    /// Gets a set of node values at depth less than maxDepth that have children.
    /// </summary>
    /// <param name="maxDepth">Maximum depth to expand to (exclusive). Depth 0 = expand roots.</param>
    public HashSet<string> GetExpandToDepthValues(int maxDepth)
    {
        var result = new HashSet<string>();
        foreach (var kvp in childrenByParent)
        {
            if (kvp.Key == RootParentKey || kvp.Value.Count == 0)
            {
                continue;
            }

            if (depthByValue.TryGetValue(kvp.Key, out var depth) && depth < maxDepth)
            {
                result.Add(kvp.Key);
            }
        }
        return result;
    }

    /// <summary>
    /// Expands all ancestors of the specified node in the given expansion set.
    /// </summary>
    public void ExpandAncestorsOf(string value, HashSet<string> expandedValues)
    {
        var ancestors = GetAncestorValues(value);
        foreach (var ancestor in ancestors)
        {
            expandedValues.Add(ancestor);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Warning, Message = "HierarchyManager: Duplicate key '{Key}' detected. The item will be skipped. Ensure ItemValueSelector returns unique values for each item.")]
        public static partial void DuplicateKeyDetected(ILogger logger, string key);
    }
}
