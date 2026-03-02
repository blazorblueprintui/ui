using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Primitives.TreeView;

public partial class BbTreeView : IAsyncDisposable
{
    private TreeViewContext context = new();
    private ElementReference elementRef;
    private IJSObjectReference? keyboardModule;
    private DotNetObjectReference<BbTreeView>? dotNetRef;
    private bool disposed;
    private bool defaultExpandApplied;

    // Track last-synced parameter values to prevent stale re-renders from
    // overwriting context state. Without this, an intermediate render (triggered
    // by EventCallback on a wrapper component) could pass stale parameter values
    // through SyncStateToContext and revert a user-initiated selection/check.
    private string? lastSyncedSelectedValue;
    private HashSet<string>? lastSyncedSelectedValues;
    private HashSet<string>? lastSyncedCheckedValues;
    private HashSet<string>? lastSyncedExpandedValues;

    /// <summary>
    /// The child content to render within the tree. Should contain BbTreeItem components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// ARIA label for the tree.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

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
    /// The selected value in single selection mode (two-way bindable).
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

    /// <summary>
    /// Whether nodes display checkboxes.
    /// </summary>
    [Parameter]
    public bool Checkable { get; set; }

    /// <summary>
    /// Whether checkbox cascade is disabled (each checkbox independent).
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
    /// Fired when a node is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnNodeClick { get; set; }

    /// <summary>
    /// Fired when a node is expanded.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnNodeExpand { get; set; }

    /// <summary>
    /// Fired when a node is collapsed.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnNodeCollapse { get; set; }

    /// <summary>
    /// Additional attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the tree context for child components.
    /// </summary>
    public TreeViewContext Context => context;

    protected override void OnInitialized()
    {
        SyncStateToContext();
        context.OnStateChanged += HandleContextStateChanged;

        // Set auto-expand flags before child BbTreeItems render so nodes
        // expand progressively during registration (one level per render pass).
        // Only applies in uncontrolled mode (ExpandedValues not bound).
        if (ExpandedValues == null)
        {
            context.AutoExpandAll = DefaultExpandAll;
            context.AutoExpandDepth = DefaultExpandDepth;
        }
    }

    protected override void OnParametersSet() =>
        SyncStateToContext();

    private void SyncStateToContext()
    {
        context.State.SelectionMode = SelectionMode;
        context.State.Checkable = Checkable;
        context.State.CheckStrictly = CheckStrictly;
        context.State.ShowLines = ShowLines;
        context.State.ShowIcons = ShowIcons;

        // Only sync collection state when the PARAMETER value has actually changed
        // since the last sync. This prevents stale re-renders (caused by intermediate
        // StateHasChanged calls on wrapper components) from overwriting context state
        // that was just updated by user interaction.

        if (ExpandedValues != null)
        {
            if (lastSyncedExpandedValues == null || !lastSyncedExpandedValues.SetEquals(ExpandedValues))
            {
                lastSyncedExpandedValues = new HashSet<string>(ExpandedValues);
                if (!context.State.ExpandedValues.SetEquals(ExpandedValues))
                {
                    context.SetExpandedValues(ExpandedValues);
                }
            }
        }

        if (SelectedValues != null)
        {
            if (lastSyncedSelectedValues == null || !lastSyncedSelectedValues.SetEquals(SelectedValues))
            {
                lastSyncedSelectedValues = new HashSet<string>(SelectedValues);
                if (!context.State.SelectedValues.SetEquals(SelectedValues))
                {
                    context.SetSelectedValues(SelectedValues);
                }
            }
        }
        else if (SelectedValue != lastSyncedSelectedValue && SelectionMode == TreeSelectionMode.Single)
        {
            lastSyncedSelectedValue = SelectedValue;
            if (SelectedValue != null)
            {
                var expected = new HashSet<string> { SelectedValue };
                if (!context.State.SelectedValues.SetEquals(expected))
                {
                    context.SetSelectedValues(expected);
                }
            }
            else if (context.State.SelectedValues.Count > 0)
            {
                context.SetSelectedValues(new HashSet<string>());
            }
        }

        if (CheckedValues != null)
        {
            if (lastSyncedCheckedValues == null || !lastSyncedCheckedValues.SetEquals(CheckedValues))
            {
                lastSyncedCheckedValues = new HashSet<string>(CheckedValues);
                if (!context.State.CheckedValues.SetEquals(CheckedValues))
                {
                    context.SetCheckedValues(CheckedValues);
                }
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Apply default expand after first render — child BbTreeItems have
            // registered in the node registry during their OnInitialized, so
            // we can now determine which nodes have children.
            if (!defaultExpandApplied && ExpandedValues == null)
            {
                defaultExpandApplied = true;

                // Clear auto-expand flags now that the initial expansion pass
                // is complete. Keeping them set would cause re-registered nodes
                // (from collapse/expand cycles with lazy rendering) to be
                // force-expanded, overriding user-collapsed state.
                context.AutoExpandAll = false;
                context.AutoExpandDepth = null;

                if (DefaultExpandAll)
                {
                    context.ExpandAllWithChildren();
                }
                else if (DefaultExpandDepth.HasValue)
                {
                    ExpandToDepth(DefaultExpandDepth.Value);
                }
            }

            try
            {
                dotNetRef = DotNetObjectReference.Create(this);
                keyboardModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Primitives/js/primitives/tree-keyboard.js");

                await keyboardModule.InvokeVoidAsync("initialize", elementRef, dotNetRef, context.Id);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }
    }

    private void ExpandToDepth(int maxDepth)
    {
        var expandSet = new HashSet<string>();
        // Start from root nodes (nodes with no parent)
        foreach (var rootValue in context.GetChildValues(null))
        {
            ExpandToDepthRecursive(rootValue, 0, maxDepth, expandSet);
        }

        if (expandSet.Count > 0)
        {
            context.SetExpandedValues(expandSet);
        }
    }

    private void ExpandToDepthRecursive(string value, int currentDepth, int maxDepth, HashSet<string> expandSet)
    {
        if (currentDepth >= maxDepth)
        {
            return;
        }

        if (context.HasChildren(value))
        {
            expandSet.Add(value);
            foreach (var child in context.GetChildValues(value))
            {
                ExpandToDepthRecursive(child, currentDepth + 1, maxDepth, expandSet);
            }
        }
    }

    /// <summary>
    /// Called from JavaScript when a node is activated via click or keyboard (Enter/Space).
    /// </summary>
    [JSInvokable]
    public async Task JsOnNodeActivate(string value, bool hasChildren = false)
    {
        context.FocusedNodeValue = value;

        // Update context selection BEFORE firing OnNodeClick. The EventCallback's
        // InvokeAsync triggers StateHasChanged on the receiver (wrapper component),
        // which could re-render with stale parameter values. By selecting first,
        // the context already has the correct state when SyncStateToContext runs.
        context.SelectNode(value);

        if (ExpandOnClick && (hasChildren || context.HasChildren(value)))
        {
            context.ToggleExpanded(value);

            if (context.IsExpanded(value) && OnNodeExpand.HasDelegate)
            {
                await OnNodeExpand.InvokeAsync(value);
            }
            else if (!context.IsExpanded(value) && OnNodeCollapse.HasDelegate)
            {
                await OnNodeCollapse.InvokeAsync(value);
            }
        }

        if (OnNodeClick.HasDelegate)
        {
            await OnNodeClick.InvokeAsync(value);
        }
    }

    /// <summary>
    /// Called from JavaScript when a node checkbox is toggled via keyboard (Space).
    /// </summary>
    [JSInvokable]
    public void JsOnNodeCheck(string value)
    {
        context.FocusedNodeValue = value;
        context.ToggleChecked(value);
    }

    /// <summary>
    /// Called from JavaScript when a node should be expanded.
    /// </summary>
    [JSInvokable]
    public async Task JsOnNodeExpand(string value)
    {
        context.FocusedNodeValue = value;
        context.ExpandNode(value);
        if (OnNodeExpand.HasDelegate)
        {
            await OnNodeExpand.InvokeAsync(value);
        }
    }

    /// <summary>
    /// Called from JavaScript when a node should be collapsed.
    /// </summary>
    [JSInvokable]
    public async Task JsOnNodeCollapse(string value)
    {
        context.FocusedNodeValue = value;
        context.CollapseNode(value);
        if (OnNodeCollapse.HasDelegate)
        {
            await OnNodeCollapse.InvokeAsync(value);
        }
    }

    /// <summary>
    /// Called from JavaScript to expand all siblings of a node.
    /// </summary>
    [JSInvokable]
    public void JsOnExpandSiblings(string value)
    {
        context.FocusedNodeValue = value;
        context.ExpandSiblings(value);
    }

    private async void HandleContextStateChanged()
    {
        // Propagate context state changes to bound parameters via EventCallbacks.
        // Do NOT call StateHasChanged() here — BbTreeItem instances re-render via their
        // own OnStateChanged subscription, and an intermediate render would cause
        // SyncStateToContext to overwrite context state with stale parameter values.
        try
        {
            if (ExpandedValuesChanged.HasDelegate)
            {
                await ExpandedValuesChanged.InvokeAsync(new HashSet<string>(context.State.ExpandedValues));
            }

            if (SelectionMode == TreeSelectionMode.Single && SelectedValueChanged.HasDelegate)
            {
                var selected = context.State.SelectedValues.FirstOrDefault();
                await SelectedValueChanged.InvokeAsync(selected);
            }

            if (SelectionMode == TreeSelectionMode.Multiple && SelectedValuesChanged.HasDelegate)
            {
                await SelectedValuesChanged.InvokeAsync(new HashSet<string>(context.State.SelectedValues));
            }

            if (Checkable && CheckedValuesChanged.HasDelegate)
            {
                await CheckedValuesChanged.InvokeAsync(new HashSet<string>(context.State.CheckedValues));
            }
        }
        catch (Exception)
        {
            // Consumer callback exception — prevent unobserved task exceptions
            // from crashing the application.
        }
    }

    /// <summary>
    /// Expands all nodes in the tree.
    /// </summary>
    public void ExpandAll() =>
        context.ExpandAllWithChildren();

    /// <summary>
    /// Collapses all nodes in the tree.
    /// </summary>
    public void CollapseAll() =>
        context.SetExpandedValues(new HashSet<string>());

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (disposed)
        {
            return;
        }

        disposed = true;
        context.OnStateChanged -= HandleContextStateChanged;

        if (keyboardModule is not null)
        {
            try
            {
                await keyboardModule.InvokeVoidAsync("dispose", context.Id);
                await keyboardModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }

        dotNetRef?.Dispose();
    }
}
