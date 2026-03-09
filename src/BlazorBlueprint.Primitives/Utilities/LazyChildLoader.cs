namespace BlazorBlueprint.Primitives.Utilities;

/// <summary>
/// Manages asynchronous loading of child items in a hierarchy with caching and state tracking.
/// Supports both full (load-all) and paged (load-page) patterns.
/// </summary>
/// <typeparam name="TItem">The type of items in the hierarchy.</typeparam>
public class LazyChildLoader<TItem>
{
    private readonly Dictionary<string, IReadOnlyList<TItem>> cache = new();
    private readonly HashSet<string> loadingNodes = new();
    private readonly HashSet<string> errorNodes = new();

    /// <summary>
    /// Gets whether the specified node is currently loading its children.
    /// </summary>
    public bool IsLoading(string value) => loadingNodes.Contains(value);

    /// <summary>
    /// Gets whether the specified node had an error loading its children.
    /// </summary>
    public bool HasError(string value) => errorNodes.Contains(value);

    /// <summary>
    /// Loads all children for a node using the provided loader function.
    /// Results are cached; subsequent calls return the cached value.
    /// </summary>
    /// <param name="value">The node's unique value.</param>
    /// <param name="item">The node's data item.</param>
    /// <param name="loader">Async function that returns the children.</param>
    /// <returns>The loaded children.</returns>
    public async Task<IReadOnlyList<TItem>> LoadAsync(
        string value, TItem item,
        Func<TItem, Task<IEnumerable<TItem>>> loader)
    {
        if (cache.TryGetValue(value, out var cached))
        {
            return cached;
        }

        loadingNodes.Add(value);
        errorNodes.Remove(value);

        try
        {
            var children = await loader(item);
            var list = children as IReadOnlyList<TItem> ?? children.ToList();
            cache[value] = list;
            return list;
        }
        catch
        {
            errorNodes.Add(value);
            throw;
        }
        finally
        {
            loadingNodes.Remove(value);
        }
    }

    /// <summary>
    /// Loads a page of children for a node using the provided paged loader function.
    /// Paged results are not cached (each page request fetches fresh data).
    /// </summary>
    /// <param name="value">The node's unique value.</param>
    /// <param name="item">The node's data item.</param>
    /// <param name="skip">Number of items to skip.</param>
    /// <param name="take">Number of items to take.</param>
    /// <param name="pagedLoader">Async function that returns a page of children with total count.</param>
    /// <returns>The page result containing items and total count.</returns>
    public async Task<ChildPageResult<TItem>> LoadPageAsync(
        string value, TItem item, int skip, int take,
        Func<TItem, int, int, Task<ChildPageResult<TItem>>> pagedLoader)
    {
        loadingNodes.Add(value);
        errorNodes.Remove(value);

        try
        {
            var result = await pagedLoader(item, skip, take);
            return result;
        }
        catch
        {
            errorNodes.Add(value);
            throw;
        }
        finally
        {
            loadingNodes.Remove(value);
        }
    }

    /// <summary>
    /// Gets previously cached children for a node, or null if not cached.
    /// </summary>
    public IReadOnlyList<TItem>? GetCachedChildren(string value) =>
        cache.TryGetValue(value, out var cached) ? cached : null;

    /// <summary>
    /// Clears the cache for a specific node, or all nodes if value is null.
    /// </summary>
    /// <param name="value">The node value to clear, or null to clear all.</param>
    public void ClearCache(string? value = null)
    {
        if (value != null)
        {
            cache.Remove(value);
            errorNodes.Remove(value);
        }
        else
        {
            cache.Clear();
            errorNodes.Clear();
        }
    }
}
