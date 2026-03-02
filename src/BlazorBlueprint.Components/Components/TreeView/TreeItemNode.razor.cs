using BlazorBlueprint.Primitives.TreeView;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Internal component for recursively rendering data-driven tree items.
/// </summary>
public partial class TreeItemNode<TItem> : ComponentBase
{
    [Parameter, EditorRequired]
    public TItem Item { get; set; } = default!;

    [Parameter, EditorRequired]
    public Func<TItem, string> TextField { get; set; } = null!;

    [Parameter, EditorRequired]
    public Func<TItem, string> ValueField { get; set; } = null!;

    [Parameter]
    public Func<TItem, string?>? IconField { get; set; }

    [Parameter, EditorRequired]
    public Func<TItem, IEnumerable<TItem>> ChildrenAccessor { get; set; } = null!;

    [Parameter, EditorRequired]
    public Func<TItem, bool> HasChildrenAccessor { get; set; } = null!;

    [Parameter, EditorRequired]
    public Func<TItem, bool> IsFilteredOut { get; set; } = null!;

    [Parameter]
    public bool IsMatchHighlighted { get; set; }

    [Parameter]
    public string? SearchText { get; set; }

    [Parameter]
    public bool ShowIcons { get; set; } = true;

    [Parameter]
    public bool Checkable { get; set; }

    [Parameter]
    public bool Draggable { get; set; }

    [Parameter]
    public Func<TItem, bool>? AllowDrag { get; set; }

    [Parameter]
    public HashSet<string> LoadingNodes { get; set; } = new();

    [Parameter]
    public HashSet<string> ErrorNodes { get; set; } = new();

    [Parameter]
    public EventCallback<string> OnRetryLoad { get; set; }

    [CascadingParameter]
    public TreeViewContext? TreeContext { get; set; }

    [CascadingParameter]
    public BlazorBlueprint.Primitives.TreeView.BbTreeItem? ParentPrimitiveItem { get; set; }

    private string NodeValue => ValueField(Item);

    private string LabelValue => TextField(Item);

    private string? IconValue => IconField?.Invoke(Item);

    private bool HasChildren => HasChildrenAccessor(Item);

    private bool IsLoading => LoadingNodes.Contains(NodeValue);

    private bool HasError => ErrorNodes.Contains(NodeValue);

    private bool IsExpanded => TreeContext?.IsExpanded(NodeValue) ?? false;

    private bool IsSelected => TreeContext?.IsSelected(NodeValue) ?? false;

    private bool IsChecked => TreeContext?.IsChecked(NodeValue) ?? false;

    private bool IsIndeterminate => TreeContext?.IsIndeterminate(NodeValue) ?? false;

    private bool IsDraggable => Draggable && (AllowDrag == null || AllowDrag(Item));

    private int Depth => (ParentPrimitiveItem?.Depth + 1) ?? 0;

    private IEnumerable<TItem> Children => ChildrenAccessor(Item);

    private string NodeCssClass => ClassNames.cn(
        "group/treeitem flex items-center gap-1.5 py-1 px-2 rounded-md cursor-pointer select-none",
        "hover:bg-accent/50 transition-colors",
        IsSelected ? "bg-accent text-accent-foreground" : "text-foreground"
    );

    private string NodeStyle => $"padding-left: {(Depth * 1.25) + 0.5}rem;";

    private string ChildNodeStyle => $"padding-left: {((Depth + 1) * 1.25) + 0.5}rem;";

    private string ChevronCssClass => ClassNames.cn(
        "h-4 w-4 shrink-0 text-muted-foreground transition-transform duration-200",
        IsExpanded ? "rotate-90" : null
    );

    private RenderFragment HighlightedLabel => builder =>
    {
        var text = LabelValue;
        if (string.IsNullOrEmpty(SearchText))
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "truncate");
            builder.AddContent(2, text);
            builder.CloseElement();
            return;
        }

        var index = text.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "truncate");
            builder.AddContent(2, text);
            builder.CloseElement();
            return;
        }

        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "truncate");

        if (index > 0)
        {
            builder.AddContent(2, text[..index]);
        }

        builder.OpenElement(3, "mark");
        builder.AddAttribute(4, "class", "bg-yellow-200 dark:bg-yellow-900/50 rounded-sm");
        builder.AddContent(5, text.Substring(index, SearchText.Length));
        builder.CloseElement();

        if (index + SearchText.Length < text.Length)
        {
            builder.AddContent(6, text[(index + SearchText.Length)..]);
        }

        builder.CloseElement();
    };

    private async Task HandleRetry()
    {
        if (OnRetryLoad.HasDelegate)
        {
            await OnRetryLoad.InvokeAsync(NodeValue);
        }
    }
}
