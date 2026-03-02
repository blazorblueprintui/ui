using System.Timers;
using BlazorBlueprint.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

public partial class BbTreeView<TItem> : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private BlazorBlueprint.Primitives.TreeView.BbTreeView? primitiveTreeRef;
    private IJSObjectReference? dragDropModule;
    private DotNetObjectReference<BbTreeView<TItem>>? dotNetRef;
    private string searchText = string.Empty;
    private HashSet<string>? preSearchExpandedValues;
    private HashSet<string> searchExpandedValues = new();
    private HashSet<string>? managedExpandedValues;
    private Dictionary<string, List<TItem>> loadedChildren = new();
    private HashSet<string> loadingNodes = new();
    private HashSet<string> errorNodes = new();
    private Dictionary<string, TItem>? itemsByValue;
    private Dictionary<string, List<TItem>>? childrenByParent;
    private System.Timers.Timer? debounceTimer;
    private bool disposed;
    private const string RootSentinel = "\0__tree_root__";
    private string instanceId = Guid.NewGuid().ToString("N")[..8];

    // --- Declarative mode ---

    /// <summary>
    /// Child content for declarative mode (BbTreeItem components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    // --- Data-driven mode ---

    /// <summary>
    /// Data source for data-driven mode.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Function to get child items from a parent item (nested data mode).
    /// </summary>
    [Parameter]
    public Func<TItem, IEnumerable<TItem>?>? ChildrenProperty { get; set; }

    /// <summary>
    /// Function to get the parent ID from an item (flat data mode).
    /// </summary>
    [Parameter]
    public Func<TItem, string?>? ParentField { get; set; }

    /// <summary>
    /// Function to get the display text from an item.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? TextField { get; set; }

    /// <summary>
    /// Function to get the unique value/ID from an item.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? ValueField { get; set; }

    /// <summary>
    /// Function to get an icon name from an item.
    /// </summary>
    [Parameter]
    public Func<TItem, string?>? IconField { get; set; }

    /// <summary>
    /// Function to determine if an item has children (for lazy loading).
    /// </summary>
    [Parameter]
    public Func<TItem, bool>? HasChildrenField { get; set; }

    // --- Selection ---

    /// <summary>
    /// Selected value in single selection mode (two-way bindable).
    /// </summary>
    [Parameter]
    public string? SelectedValue { get; set; }

    /// <summary>
    /// Event callback invoked when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> SelectedValueChanged { get; set; }

    /// <summary>
    /// Selected values in multiple selection mode (two-way bindable).
    /// </summary>
    [Parameter]
    public HashSet<string>? SelectedValues { get; set; }

    /// <summary>
    /// Event callback invoked when selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<string>> SelectedValuesChanged { get; set; }

    /// <summary>
    /// Selection mode: None, Single, or Multiple.
    /// </summary>
    [Parameter]
    public TreeSelectionMode SelectionMode { get; set; } = TreeSelectionMode.None;

    // --- Expand/Collapse ---

    /// <summary>
    /// Controls which nodes are expanded (two-way bindable).
    /// </summary>
    [Parameter]
    public HashSet<string>? ExpandedValues { get; set; }

    /// <summary>
    /// Event callback invoked when expanded nodes change.
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<string>> ExpandedValuesChanged { get; set; }

    /// <summary>
    /// Whether to expand all nodes on initial render.
    /// </summary>
    [Parameter]
    public bool DefaultExpandAll { get; set; }

    /// <summary>
    /// Expand nodes to this depth on initial render.
    /// </summary>
    [Parameter]
    public int? DefaultExpandDepth { get; set; }

    // --- Checkable ---

    /// <summary>
    /// Whether nodes display checkboxes.
    /// </summary>
    [Parameter]
    public bool Checkable { get; set; }

    /// <summary>
    /// Whether checkbox cascade is disabled.
    /// </summary>
    [Parameter]
    public bool CheckStrictly { get; set; }

    /// <summary>
    /// Checked node values (two-way bindable).
    /// </summary>
    [Parameter]
    public HashSet<string>? CheckedValues { get; set; }

    /// <summary>
    /// Event callback invoked when checked values change.
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<string>> CheckedValuesChanged { get; set; }

    // --- Appearance ---

    /// <summary>
    /// Whether to show connector lines between nodes.
    /// </summary>
    [Parameter]
    public bool ShowLines { get; set; }

    /// <summary>
    /// Whether to show node icons. Default is true.
    /// </summary>
    [Parameter]
    public bool ShowIcons { get; set; } = true;

    /// <summary>
    /// Whether clicking a parent node also toggles its expanded state.
    /// When true, clicking anywhere on a node with children will expand or collapse it
    /// in addition to selecting it. When false (default), only the chevron toggle expands/collapses.
    /// </summary>
    [Parameter]
    public bool ExpandOnClick { get; set; }

    /// <summary>
    /// Additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// ARIA label for the tree.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    // --- Search ---

    /// <summary>
    /// Whether to show the built-in search input.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Placeholder text for the search input.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search...";

    /// <summary>
    /// External search text for filtering (two-way bindable).
    /// </summary>
    [Parameter]
    public string? SearchText { get; set; }

    /// <summary>
    /// Event callback invoked when search text changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> SearchTextChanged { get; set; }

    /// <summary>
    /// Callback for server-side search scenarios.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    // --- Lazy Loading ---

    /// <summary>
    /// Async callback to load children of a node on demand.
    /// </summary>
    [Parameter]
    public Func<TItem, Task<IEnumerable<TItem>>>? OnLoadChildren { get; set; }

    // --- Drag and Drop ---

    /// <summary>
    /// Whether drag and drop is enabled.
    /// </summary>
    [Parameter]
    public bool Draggable { get; set; }

    /// <summary>
    /// Per-node drag permission callback. Returns true if the node can be dragged.
    /// </summary>
    [Parameter]
    public Func<TItem, bool>? AllowDrag { get; set; }

    /// <summary>
    /// Per-pair drop permission callback. Returns true if source can be dropped on target.
    /// </summary>
    [Parameter]
    public Func<TItem, TItem, bool>? AllowDrop { get; set; }

    /// <summary>
    /// Event callback when a node is dropped.
    /// </summary>
    [Parameter]
    public EventCallback<TreeDropEventArgs<TItem>> OnNodeDrop { get; set; }

    // --- Events ---

    /// <summary>
    /// Fired when a node is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> OnNodeClick { get; set; }

    /// <summary>
    /// Fired when a node is expanded.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> OnNodeExpand { get; set; }

    /// <summary>
    /// Fired when a node is collapsed.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> OnNodeCollapse { get; set; }

    /// <summary>
    /// Fired when checked values change.
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<string>> OnCheckedChanged { get; set; }

    // --- Computed properties ---

    private string CssClass => ClassNames.cn("text-sm", Class);

    private string ActiveSearchText => SearchText ?? searchText;

    private bool IsSearchActive => !string.IsNullOrWhiteSpace(ActiveSearchText);

    private HashSet<string>? effectiveExpandedValues =>
        IsSearchActive ? searchExpandedValues : (ExpandedValues ?? managedExpandedValues);

    protected override void OnInitialized()
    {
        BuildItemIndex();

        if (DefaultExpandAll)
        {
            ExpandAllNodes();
        }
        else if (DefaultExpandDepth.HasValue)
        {
            ExpandToDepth(DefaultExpandDepth.Value);
        }
    }

    protected override void OnParametersSet()
    {
        BuildItemIndex();

        // Sync external SearchText
        if (SearchText != null && searchText != SearchText)
        {
            searchText = SearchText;
            ApplySearch();
        }
    }

    private void BuildItemIndex()
    {
        if (Items == null || ValueField == null)
        {
            itemsByValue = null;
            childrenByParent = null;
            return;
        }

        itemsByValue = new Dictionary<string, TItem>();
        childrenByParent = new Dictionary<string, List<TItem>>();

        if (ParentField != null)
        {
            // Flat data mode: build tree from parent references
            foreach (var item in Items)
            {
                var value = ValueField(item);
                itemsByValue[value] = item;
            }

            foreach (var item in Items)
            {
                var parentId = ParentField(item);
                var key = parentId ?? RootSentinel;
                if (!childrenByParent.TryGetValue(key, out var list))
                {
                    list = new List<TItem>();
                    childrenByParent[key] = list;
                }
                list.Add(item);
            }
        }
        else
        {
            // Nested data mode: index all items recursively
            foreach (var item in Items)
            {
                IndexItemRecursive(item, null);
            }
        }
    }

    private void IndexItemRecursive(TItem item, string? parentKey)
    {
        if (ValueField == null)
        {
            return;
        }

        var value = ValueField(item);
        itemsByValue![value] = item;

        var key = parentKey ?? RootSentinel;
        if (!childrenByParent!.TryGetValue(key, out var list))
        {
            list = new List<TItem>();
            childrenByParent[key] = list;
        }
        list.Add(item);

        if (ChildrenProperty != null)
        {
            var children = ChildrenProperty(item);
            if (children != null)
            {
                foreach (var child in children)
                {
                    IndexItemRecursive(child, value);
                }
            }
        }
    }

    private IEnumerable<TItem> GetRootItems()
    {
        if (childrenByParent != null && childrenByParent.TryGetValue(RootSentinel, out var roots))
        {
            return roots;
        }

        return Items ?? Enumerable.Empty<TItem>();
    }

    internal IEnumerable<TItem> GetChildren(TItem item)
    {
        if (ValueField == null)
        {
            return Enumerable.Empty<TItem>();
        }

        var value = ValueField(item);

        // Check for lazily loaded children
        if (loadedChildren.TryGetValue(value, out var lazyChildren))
        {
            return lazyChildren;
        }

        if (ChildrenProperty != null)
        {
            return ChildrenProperty(item) ?? Enumerable.Empty<TItem>();
        }

        if (childrenByParent != null && childrenByParent.TryGetValue(value, out var children))
        {
            return children;
        }

        return Enumerable.Empty<TItem>();
    }

    internal bool GetHasChildren(TItem item)
    {
        if (ValueField == null)
        {
            return false;
        }

        var value = ValueField(item);

        if (loadedChildren.TryGetValue(value, out var children))
        {
            return children.Count > 0;
        }

        if (HasChildrenField != null)
        {
            return HasChildrenField(item);
        }

        return GetChildren(item).Any();
    }

    internal TItem? GetItemByValue(string value)
    {
        if (itemsByValue != null && itemsByValue.TryGetValue(value, out var item))
        {
            return item;
        }

        return default;
    }

    // --- Search ---

    private void HandleSearchTextChanged(string? value)
    {
        searchText = value ?? string.Empty;

        if (SearchTextChanged.HasDelegate)
        {
            _ = SearchTextChanged.InvokeAsync(value);
        }

        // Debounce the search
        debounceTimer?.Stop();
        debounceTimer?.Dispose();
        debounceTimer = new System.Timers.Timer(300);
        debounceTimer.Elapsed += OnSearchDebounced;
        debounceTimer.AutoReset = false;
        debounceTimer.Start();
    }

    private void OnSearchDebounced(object? sender, ElapsedEventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            ApplySearch();
            StateHasChanged();
        });
    }

    private void ApplySearch()
    {
        if (!IsSearchActive)
        {
            // Restore pre-search expand state
            if (preSearchExpandedValues != null)
            {
                searchExpandedValues = new HashSet<string>();
                managedExpandedValues = preSearchExpandedValues;
                if (ExpandedValuesChanged.HasDelegate)
                {
                    _ = ExpandedValuesChanged.InvokeAsync(preSearchExpandedValues);
                }
                preSearchExpandedValues = null;
            }

            if (OnSearch.HasDelegate)
            {
                _ = OnSearch.InvokeAsync(string.Empty);
            }

            return;
        }

        // Save current expand state before search
        if (preSearchExpandedValues == null)
        {
            preSearchExpandedValues = new HashSet<string>(effectiveExpandedValues ?? new HashSet<string>());
        }

        // Compute which nodes to show and expand
        searchExpandedValues = new HashSet<string>();
        if (Items != null && ValueField != null && TextField != null)
        {
            foreach (var item in GetAllItems())
            {
                if (MatchesSearch(item))
                {
                    // Expand all ancestors of matching nodes
                    ExpandAncestorsOf(item);
                }
            }
        }

        if (OnSearch.HasDelegate)
        {
            _ = OnSearch.InvokeAsync(ActiveSearchText);
        }
    }

    private bool MatchesSearch(TItem item)
    {
        if (!IsSearchActive || TextField == null)
        {
            return true;
        }

        return TextField(item).Contains(ActiveSearchText, StringComparison.OrdinalIgnoreCase);
    }

    internal bool IsFilteredOut(TItem item)
    {
        if (!IsSearchActive || TextField == null || ValueField == null)
        {
            return false;
        }

        // If this node matches, it's not filtered out
        if (MatchesSearch(item))
        {
            return false;
        }

        // If any descendant matches, this node is not filtered out
        return !HasMatchingDescendant(item);
    }

    private bool HasMatchingDescendant(TItem item)
    {
        var children = GetChildren(item);
        foreach (var child in children)
        {
            if (MatchesSearch(child) || HasMatchingDescendant(child))
            {
                return true;
            }
        }
        return false;
    }

    private void ExpandAncestorsOf(TItem item)
    {
        if (ValueField == null || (ParentField == null && ChildrenProperty == null))
        {
            return;
        }

        if (ParentField != null)
        {
            // Flat data: walk up parent chain
            var parentId = ParentField(item);
            while (parentId != null)
            {
                searchExpandedValues.Add(parentId);
                var parent = GetItemByValue(parentId);
                if (parent == null)
                {
                    break;
                }
                parentId = ParentField(parent);
            }
        }
        else
        {
            // Nested data: we need to find ancestors via index
            var value = ValueField(item);
            if (childrenByParent == null)
            {
                return;
            }

            // Build a reverse map from child to parent
            foreach (var kvp in childrenByParent)
            {
                foreach (var child in kvp.Value)
                {
                    if (ValueField(child) == value && kvp.Key != RootSentinel)
                    {
                        searchExpandedValues.Add(kvp.Key);
                        var parentItem = GetItemByValue(kvp.Key);
                        if (parentItem != null)
                        {
                            ExpandAncestorsOf(parentItem);
                        }
                        return;
                    }
                }
            }
        }
    }

    private IEnumerable<TItem> GetAllItems()
    {
        if (itemsByValue != null)
        {
            return itemsByValue.Values;
        }

        return Items ?? Enumerable.Empty<TItem>();
    }

    // --- Lazy Loading ---

    private async Task HandleNodeExpand(string value)
    {
        if (OnLoadChildren != null && ValueField != null)
        {
            var item = GetItemByValue(value);
            if (item != null && !loadedChildren.ContainsKey(value) && GetHasChildren(item))
            {
                // Check if children are already provided by ChildrenProperty
                var existingChildren = ChildrenProperty != null ? ChildrenProperty(item) : null;
                if (existingChildren == null || !existingChildren.Any())
                {
                    await LoadChildrenAsync(value, item);
                }
            }
        }

        if (OnNodeExpand.HasDelegate)
        {
            var item = GetItemByValue(value);
            if (item != null)
            {
                await OnNodeExpand.InvokeAsync(item);
            }
        }
    }

    private async Task LoadChildrenAsync(string value, TItem item)
    {
        if (OnLoadChildren == null)
        {
            return;
        }

        loadingNodes.Add(value);
        errorNodes.Remove(value);
        StateHasChanged();

        try
        {
            var children = await OnLoadChildren(item);
            var childList = children.ToList();
            loadedChildren[value] = childList;

            // Index newly loaded children
            if (itemsByValue != null)
            {
                foreach (var child in childList)
                {
                    if (ValueField != null)
                    {
                        itemsByValue[ValueField(child)] = child;
                    }
                }
            }
        }
        catch
        {
            errorNodes.Add(value);
        }
        finally
        {
            loadingNodes.Remove(value);
            StateHasChanged();
        }
    }

    internal async Task HandleRetryLoad(string value)
    {
        var item = GetItemByValue(value);
        if (item != null)
        {
            await LoadChildrenAsync(value, item);
        }
    }

    // --- Event handlers ---

    private async Task HandleNodeClick(string value)
    {
        if (OnNodeClick.HasDelegate)
        {
            var item = GetItemByValue(value);
            if (item != null)
            {
                await OnNodeClick.InvokeAsync(item);
            }
        }
    }

    private async Task HandleNodeCollapse(string value)
    {
        if (OnNodeCollapse.HasDelegate)
        {
            var item = GetItemByValue(value);
            if (item != null)
            {
                await OnNodeCollapse.InvokeAsync(item);
            }
        }
    }

    private void HandleExpandedValuesChanged(HashSet<string> values)
    {
        managedExpandedValues = values;
        if (ExpandedValuesChanged.HasDelegate)
        {
            _ = ExpandedValuesChanged.InvokeAsync(values);
        }
    }

    private async Task HandleCheckedValuesChanged(HashSet<string> values)
    {
        if (CheckedValuesChanged.HasDelegate)
        {
            await CheckedValuesChanged.InvokeAsync(values);
        }

        if (OnCheckedChanged.HasDelegate)
        {
            await OnCheckedChanged.InvokeAsync(values);
        }
    }

    // --- Expand helpers ---

    private void ExpandAllNodes()
    {
        if (Items == null || ValueField == null)
        {
            return;
        }

        var allExpandable = new HashSet<string>();
        foreach (var item in GetAllItems())
        {
            if (GetHasChildren(item))
            {
                allExpandable.Add(ValueField(item));
            }
        }

        managedExpandedValues = allExpandable;
        if (ExpandedValuesChanged.HasDelegate)
        {
            _ = ExpandedValuesChanged.InvokeAsync(allExpandable);
        }
    }

    private void ExpandToDepth(int depth)
    {
        if (Items == null || ValueField == null)
        {
            return;
        }

        var expandSet = new HashSet<string>();
        foreach (var item in GetRootItems())
        {
            ExpandToDepthRecursive(item, 0, depth, expandSet);
        }

        managedExpandedValues = expandSet;
        if (ExpandedValuesChanged.HasDelegate)
        {
            _ = ExpandedValuesChanged.InvokeAsync(expandSet);
        }
    }

    private void ExpandToDepthRecursive(TItem item, int currentDepth, int maxDepth, HashSet<string> expandSet)
    {
        if (currentDepth >= maxDepth || ValueField == null)
        {
            return;
        }

        if (GetHasChildren(item))
        {
            expandSet.Add(ValueField(item));
            foreach (var child in GetChildren(item))
            {
                ExpandToDepthRecursive(child, currentDepth + 1, maxDepth, expandSet);
            }
        }
    }

    // --- Drag and drop (JS invokable) ---

    /// <summary>
    /// Called from JavaScript when a node is dropped.
    /// </summary>
    [Microsoft.JSInterop.JSInvokable]
    public async Task JsOnNodeDrop(string sourceValue, string targetValue, string position)
    {
        if (!OnNodeDrop.HasDelegate || ValueField == null)
        {
            return;
        }

        var source = GetItemByValue(sourceValue);
        var target = GetItemByValue(targetValue);
        if (source == null || target == null)
        {
            return;
        }

        if (AllowDrop != null && !AllowDrop(source, target))
        {
            return;
        }

        var dropPosition = position switch
        {
            "before" => TreeDropPosition.Before,
            "after" => TreeDropPosition.After,
            _ => TreeDropPosition.Inside
        };

        await OnNodeDrop.InvokeAsync(new TreeDropEventArgs<TItem>(source, target, dropPosition));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Draggable && primitiveTreeRef != null)
        {
            try
            {
                dotNetRef = DotNetObjectReference.Create(this);
                dragDropModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/tree-view.js");

                await dragDropModule.InvokeVoidAsync("initializeDragDropById",
                    primitiveTreeRef.Context.Id,
                    dotNetRef,
                    instanceId);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (disposed)
        {
            return;
        }

        disposed = true;
        debounceTimer?.Stop();
        debounceTimer?.Dispose();

        if (dragDropModule is not null)
        {
            try
            {
                await dragDropModule.InvokeVoidAsync("disposeDragDrop", instanceId);
                await dragDropModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }

        dotNetRef?.Dispose();
    }
}
