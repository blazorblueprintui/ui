using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

public partial class BbDataView<TItem> where TItem : class
{
    /// <summary>Whether item selection is disabled, single or multiple.</summary>
    [Parameter] public DataTableSelectionMode SelectionMode { get; set; }
    /// <summary>The selected items. ItemKey preserves selection across reloaded instances.</summary>
    [Parameter] public IReadOnlyCollection<TItem> SelectedItems { get; set; } = [];
    /// <summary>Called when item selection changes.</summary>
    [Parameter] public EventCallback<IReadOnlyCollection<TItem>> SelectedItemsChanged { get; set; }
    /// <summary>A unique, stable key for an item. Defaults to the item's equality semantics.</summary>
    [Parameter] public Func<TItem, object>? ItemKey { get; set; }
    /// <summary>An accessible label for an item's selection control.</summary>
    [Parameter] public Func<TItem, string>? ItemLabel { get; set; }
    /// <summary>Whether an item's selection control is disabled.</summary>
    [Parameter] public Func<TItem, bool>? IsItemDisabled { get; set; }
    /// <summary>Optional grouping of the currently loaded page/batches, in first-seen order.</summary>
    [Parameter] public Func<TItem, string?>? GroupBy { get; set; }
    /// <summary>Custom group heading. Receives the key and currently loaded group items.</summary>
    [Parameter] public RenderFragment<DataViewGroupContext<TItem>>? GroupHeaderTemplate { get; set; }
    /// <summary>Virtualizes list rows. Grid layout renders normally; provider loading remains page/batch based.</summary>
    [Parameter] public bool EnableVirtualization { get; set; }
    /// <summary>Estimated list row height in pixels, including row spacing.</summary>
    [Parameter] public float VirtualItemSize { get; set; } = 56;
    /// <summary>Additional rows rendered before and after the visible window.</summary>
    [Parameter] public int OverscanCount { get; set; } = 3;

    private HashSet<object> selectedKeys = [];
    private bool VirtualizeList => EnableVirtualization && _effectiveLayout == DataViewLayout.List;
    private string ScrollContainerStyle => $"height: {ScrollHeight ?? "400px"}";
    private object GetItemKey(TItem item) => ItemKey?.Invoke(item) ?? item;
    private bool IsSelected(TItem item) => selectedKeys.Contains(GetItemKey(item));
    private bool ItemDisabled(TItem item) => IsItemDisabled?.Invoke(item) == true;
    private string SelectionLabel(TItem item) => ItemLabel?.Invoke(item) ?? Localizer["DataView.SelectItem"];

    private void SyncSelection()
    {
        if (!Enum.IsDefined(SelectionMode)) { throw new ArgumentOutOfRangeException(nameof(SelectionMode)); }
        if (!float.IsFinite(VirtualItemSize) || VirtualItemSize <= 0) { throw new ArgumentOutOfRangeException(nameof(VirtualItemSize)); }
        if (OverscanCount < 0) { throw new ArgumentOutOfRangeException(nameof(OverscanCount)); }
        selectedKeys = SelectedItems.Select(GetItemKey).ToHashSet();
    }

    private async Task ToggleSelectionAsync(TItem item, bool selected)
    {
        if (SelectionMode == DataTableSelectionMode.None || ItemDisabled(item)) { return; }
        var key = GetItemKey(item);
        if (selected == selectedKeys.Contains(key)) { return; }
        var next = SelectionMode == DataTableSelectionMode.Single
            ? new List<TItem>()
            : SelectedItems.Where(candidate => !Equals(GetItemKey(candidate), key)).ToList();
        if (selected) { next.Add(item); }
        SelectedItems = next;
        selectedKeys = next.Select(GetItemKey).ToHashSet();
        _parametersChanged = true;
        await SelectedItemsChanged.InvokeAsync(next);
        StateHasChanged();
    }

    private IEnumerable<DataViewGroupContext<TItem>> VisibleGroups =>
        _visibleData.GroupBy(item => GroupBy!(item))
            .Select(group => new DataViewGroupContext<TItem>(group.Key, group.ToList()));
}
