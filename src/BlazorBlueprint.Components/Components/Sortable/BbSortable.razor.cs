using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// A drag-and-drop sortable list or grid component powered by Sortable.js.
/// </summary>
/// <remarks>
/// <para>
/// Supports single lists, connected multi-lists (items move between lists), and grid
/// layouts. Drag-and-drop is handled by Sortable.js; state is managed in .NET via the
/// <see cref="OnUpdate"/> and <see cref="OnRemove"/> callbacks.
/// </para>
/// <para>
/// When using a Razor component as the root element of <see cref="ItemTemplate"/>, wrap
/// it in a plain <c>&lt;div&gt;</c> so that Sortable.js can track it as a single DOM node.
/// </para>
/// </remarks>
/// <typeparam name="TItem">The type of items in the sortable list.</typeparam>
public partial class BbSortable<TItem> : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<BbSortable<TItem>>? _ref;
    private string _liveAnnouncement = string.Empty;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    /// <summary>
    /// Gets or sets the render fragment used to display each item.
    /// The <c>context</c> parameter provides the data item of type <typeparamref name="TItem"/>.
    /// </summary>
    /// <remarks>
    /// When using a Razor component as the root element, wrap it in a plain
    /// <c>&lt;div&gt;</c> so that Sortable.js can track the item as a single DOM node.
    /// </remarks>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the list of items to render and sort.
    /// </summary>
    [Parameter, AllowNull]
    public List<TItem> Items { get; set; }

    /// <summary>
    /// Raised when an item is reordered within this list.
    /// The tuple provides the original index and the new index after the move.
    /// </summary>
    [Parameter]
    public EventCallback<(int OldIndex, int NewIndex)> OnUpdate { get; set; }

    /// <summary>
    /// Raised when an item is dragged out of this list into another connected list.
    /// The tuple provides the item's original index in this list and its landing index
    /// in the target list.
    /// </summary>
    [Parameter]
    public EventCallback<(int OldIndex, int NewIndex)> OnRemove { get; set; }

    /// <summary>
    /// Raised when an item from a connected list is dropped into this list.
    /// The tuple provides the item's original index in the source list and its
    /// landing index in this list.
    /// </summary>
    /// <remarks>
    /// Use alongside <see cref="OnRemove"/> on the source list to coordinate cross-list
    /// moves: in the <see cref="OnRemove"/> handler remove the item from the source and
    /// store it; in the <see cref="OnAdd"/> handler insert the stored item into this list.
    /// </remarks>
    [Parameter]
    public EventCallback<(int OldIndex, int NewIndex)> OnAdd { get; set; }

    /// <summary>
    /// Gets or sets the HTML element id for the sortable container.
    /// Defaults to a new GUID.
    /// </summary>
    [Parameter]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the Sortable.js group name used to connect multiple lists.
    /// Lists sharing the same group name can exchange items.
    /// Defaults to a new GUID (isolated list).
    /// </summary>
    [Parameter]
    public string Group { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the pull behaviour for the group.
    /// Use <c>"clone"</c> to copy items instead of moving them,
    /// or <c>null</c> (default) to move them.
    /// </summary>
    [Parameter]
    public string? Pull { get; set; }

    /// <summary>
    /// Gets or sets whether items from connected lists can be dropped into this one.
    /// Defaults to <c>true</c>.
    /// </summary>
    [Parameter]
    public bool Put { get; set; } = true;

    /// <summary>
    /// Gets or sets whether items within this list can be reordered.
    /// Set to <c>false</c> to create a list that only accepts items from other lists.
    /// Defaults to <c>true</c>.
    /// </summary>
    [Parameter]
    public bool Sort { get; set; } = true;

    /// <summary>
    /// Gets or sets a CSS selector used as the drag-handle element inside each item.
    /// When set, only the matched child element initiates a drag.
    /// </summary>
    [Parameter]
    public string Handle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a CSS selector for child elements that cannot be dragged.
    /// </summary>
    [Parameter]
    public string? Filter { get; set; }

    /// <summary>
    /// Gets or sets whether to use a software fallback renderer instead of the
    /// browser's native HTML5 drag-and-drop API.
    /// Defaults to <c>true</c> for consistent cross-browser behaviour.
    /// </summary>
    [Parameter]
    public bool ForceFallback { get; set; } = true;

    /// <summary>
    /// Gets or sets the visual layout of the sortable container.
    /// </summary>
    [Parameter]
    public SortableLayout Layout { get; set; } = SortableLayout.List;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the sortable container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets an accessible label for the sortable container,
    /// exposed as <c>aria-label</c> on the container element.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets any additional HTML attributes to apply to the container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        Layout switch
        {
            SortableLayout.Grid => "grid grid-cols-2 gap-2",
            _ => "flex flex-col gap-2"
        },
        Class
    );

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _ref = DotNetObjectReference.Create(this);
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/sortable.js");
            await _module.InvokeAsync<string>(
                "init", Id, Group, Pull, Put, Sort, Handle, Filter, _ref, ForceFallback);
        }
    }

    /// <summary>
    /// Invoked from JavaScript when an item is reordered within this list.
    /// </summary>
    [JSInvokable]
    public async Task OnUpdateJS(int oldIndex, int newIndex)
    {
        _liveAnnouncement = $"Item moved from position {oldIndex + 1} to position {newIndex + 1}.";
        await OnUpdate.InvokeAsync((oldIndex, newIndex));
        StateHasChanged();
    }

    /// <summary>
    /// Invoked from JavaScript when an item is moved out of this list into another.
    /// </summary>
    [JSInvokable]
    public async Task OnRemoveJS(int oldIndex, int newIndex)
    {
        _liveAnnouncement = $"Item removed from position {oldIndex + 1} and placed at position {newIndex + 1} in another list.";
        await OnRemove.InvokeAsync((oldIndex, newIndex));
        StateHasChanged();
    }

    /// <summary>
    /// Invoked from JavaScript when an item from a connected list is dropped into this list.
    /// </summary>
    [JSInvokable]
    public async Task OnAddJS(int oldIndex, int newIndex)
    {
        _liveAnnouncement = $"Item received at position {newIndex + 1}.";
        await OnAdd.InvokeAsync((oldIndex, newIndex));
        StateHasChanged();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _ref?.Dispose();
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
