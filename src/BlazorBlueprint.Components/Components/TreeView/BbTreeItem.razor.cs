using BlazorBlueprint.Primitives.TreeView;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

public partial class BbTreeItem : ComponentBase
{
    private BlazorBlueprint.Primitives.TreeView.BbTreeItem? primitiveRef;

    /// <summary>
    /// Unique value/identifier for this node.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Display text for the node.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Lucide icon name for the node.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Badge text shown after the label.
    /// </summary>
    [Parameter]
    public string? Badge { get; set; }

    /// <summary>
    /// Whether this node is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Override parent tree's checkable setting for this node.
    /// </summary>
    [Parameter]
    public bool? Checkable { get; set; }

    /// <summary>
    /// Explicitly mark as leaf (no expand arrow even if children not loaded).
    /// </summary>
    [Parameter]
    public bool? IsLeaf { get; set; }

    /// <summary>
    /// Custom label content replacing default text.
    /// </summary>
    [Parameter]
    public RenderFragment? LabelTemplate { get; set; }

    /// <summary>
    /// Actions shown on hover (edit, delete buttons).
    /// </summary>
    [Parameter]
    public RenderFragment? ActionsTemplate { get; set; }

    /// <summary>
    /// Child BbTreeItem nodes.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes for the node row.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Whether this node is draggable (when tree has Draggable enabled).
    /// </summary>
    [Parameter]
    public bool? IsDraggable { get; set; }

    /// <summary>
    /// Cascading parameter to receive the tree context.
    /// </summary>
    [CascadingParameter]
    public TreeViewContext? TreeContext { get; set; }

    /// <summary>
    /// Cascading parameter to receive the parent primitive tree item.
    /// </summary>
    [CascadingParameter]
    public BlazorBlueprint.Primitives.TreeView.BbTreeItem? ParentPrimitiveItem { get; set; }

    private int Depth => (ParentPrimitiveItem?.Depth + 1) ?? 0;

    private bool IsExpanded => TreeContext?.IsExpanded(Value) ?? false;

    private bool IsSelected => TreeContext?.IsSelected(Value) ?? false;

    private bool IsChecked => TreeContext?.IsChecked(Value) ?? false;

    private bool IsIndeterminate => TreeContext?.IsIndeterminate(Value) ?? false;

    private bool ShowCheckbox =>
        ((Checkable ?? TreeContext?.Checkable ?? false) && !Disabled) || (Checkable == true);

    private bool HasExpandableChildren =>
        IsLeaf != true && (ChildContent != null || (TreeContext?.HasChildren(Value) ?? false));

    private string NodeCssClass => ClassNames.cn(
        "bb:group/treeitem bb:flex bb:items-center bb:gap-1.5 bb:py-1 bb:px-2 bb:rounded-md bb:cursor-pointer bb:select-none",
        "bb:hover:bg-accent/50 bb:transition-colors",
        IsSelected ? "bb:bg-accent bb:text-accent-foreground" : "bb:text-foreground",
        Disabled ? "bb:opacity-50 bb:cursor-not-allowed" : null,
        Class
    );

    private string NodeStyle => $"padding-left: {(Depth * 1.25) + 0.5}rem;";

    private string ChevronCssClass => ClassNames.cn(
        "bb:h-4 bb:w-4 bb:shrink-0 bb:text-muted-foreground bb:transition-transform bb:duration-200",
        IsExpanded ? "bb:rotate-90" : null
    );
}
