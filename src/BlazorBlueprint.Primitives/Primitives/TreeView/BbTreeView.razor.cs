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
    }

    protected override void OnParametersSet()
    {
        SyncStateToContext();
    }

    private void SyncStateToContext()
    {
        context.State.SelectionMode = SelectionMode;
        context.State.Checkable = Checkable;
        context.State.CheckStrictly = CheckStrictly;
        context.State.ShowLines = ShowLines;
        context.State.ShowIcons = ShowIcons;

        if (ExpandedValues != null)
        {
            context.State.ExpandedValues = new HashSet<string>(ExpandedValues);
        }

        if (SelectedValues != null)
        {
            context.State.SelectedValues = new HashSet<string>(SelectedValues);
        }
        else if (SelectedValue != null && SelectionMode == TreeSelectionMode.Single)
        {
            context.State.SelectedValues = new HashSet<string> { SelectedValue };
        }

        if (CheckedValues != null)
        {
            context.State.CheckedValues = new HashSet<string>(CheckedValues);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
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

    /// <summary>
    /// Called from JavaScript when a node is activated via keyboard (Enter/Space).
    /// </summary>
    [JSInvokable]
    public async Task JsOnNodeActivate(string value)
    {
        if (OnNodeClick.HasDelegate)
        {
            await OnNodeClick.InvokeAsync(value);
        }

        context.SelectNode(value);
    }

    /// <summary>
    /// Called from JavaScript when a node checkbox is toggled via keyboard (Space).
    /// </summary>
    [JSInvokable]
    public void JsOnNodeCheck(string value)
    {
        context.ToggleChecked(value);
    }

    /// <summary>
    /// Called from JavaScript when a node should be expanded.
    /// </summary>
    [JSInvokable]
    public async Task JsOnNodeExpand(string value)
    {
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
        context.ExpandSiblings(value);
    }

    private void HandleContextStateChanged()
    {
        // Sync context state back to bound parameters
        if (ExpandedValuesChanged.HasDelegate)
        {
            _ = ExpandedValuesChanged.InvokeAsync(new HashSet<string>(context.State.ExpandedValues));
        }

        if (SelectionMode == TreeSelectionMode.Single && SelectedValueChanged.HasDelegate)
        {
            var selected = context.State.SelectedValues.FirstOrDefault();
            _ = SelectedValueChanged.InvokeAsync(selected);
        }

        if (SelectionMode == TreeSelectionMode.Multiple && SelectedValuesChanged.HasDelegate)
        {
            _ = SelectedValuesChanged.InvokeAsync(new HashSet<string>(context.State.SelectedValues));
        }

        if (Checkable && CheckedValuesChanged.HasDelegate)
        {
            _ = CheckedValuesChanged.InvokeAsync(new HashSet<string>(context.State.CheckedValues));
        }

        StateHasChanged();
    }

    /// <summary>
    /// Expands all nodes in the tree.
    /// </summary>
    public void ExpandAll()
    {
        var allExpandable = new HashSet<string>();
        foreach (var value in context.State.ExpandedValues.ToList())
        {
            allExpandable.Add(value);
        }

        // Find all nodes that have children and expand them
        context.SetExpandedValues(allExpandable);
    }

    /// <summary>
    /// Collapses all nodes in the tree.
    /// </summary>
    public void CollapseAll()
    {
        context.SetExpandedValues(new HashSet<string>());
    }

    public async ValueTask DisposeAsync()
    {
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
