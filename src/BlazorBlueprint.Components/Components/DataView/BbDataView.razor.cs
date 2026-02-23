using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// A styled data view component that renders items in list or grid layout with automatic
/// sorting, filtering, pagination, and optional infinite scroll.
/// </summary>
/// <typeparam name="TItem">The type of data items in the view.</typeparam>
/// <remarks>
/// <para>
/// DataView provides a PrimeVue-like composition model: use ListTemplate and/or GridTemplate
/// render fragments to define how items are rendered in each layout mode.
/// If only one template is provided the component locks into that layout and hides the
/// layout-toggle buttons. If both are provided the user can switch freely between list and
/// grid views using the toolbar toggle.
/// </para>
/// <para>
/// Register BbDataViewColumn components inside the Fields fragment to configure sorting and
/// filtering. Infinite scroll works correctly for both flex-list and multi-column CSS
/// grid layouts.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataView TItem="Person" Data="@people"&gt;
///     &lt;ListTemplate Context="person"&gt;
///         &lt;div class="p-4 border rounded-lg"&gt;@person.Name&lt;/div&gt;
///     &lt;/ListTemplate&gt;
///     &lt;Fields&gt;
///         &lt;BbDataViewColumn TItem="Person" TValue="string" Property="@(p => p.Name)" Header="Name" Sortable Filterable /&gt;
///     &lt;/Fields&gt;
/// &lt;/BbDataView&gt;
/// </code>
/// </example>
public partial class BbDataView<TItem> : ComponentBase, IAsyncDisposable where TItem : class
{
    /// <summary>
    /// Internal class for storing field metadata without component parameters.
    /// This avoids BL0005 warnings when creating field instances programmatically.
    /// </summary>
    public class FieldData
    {
        public string Id { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public Func<TItem, object?>? Property { get; set; }
        public bool Sortable { get; set; }
        public bool Filterable { get; set; }
    }

    private List<FieldData> _fields = new();
    private SortingState _sortingState = new();
    private PaginationState _paginationState = new();
    private IEnumerable<TItem> _filteredSortedData = Array.Empty<TItem>();
    private IEnumerable<TItem> _visibleData = Array.Empty<TItem>();
    private string _searchValue = string.Empty;
    private int _currentInfinitePage = 1;

    // Backing fields for slot-component registrations (BbDataViewListTemplate/GridTemplate).
    // The effective value always prefers the named [Parameter] over the registered one,
    // so both the direct-parameter API and the slot-component API work side-by-side.
    private RenderFragment<TItem>? _registeredListTemplate;
    private RenderFragment<TItem>? _registeredGridTemplate;

    // Infinite scroll
    private ElementReference _scrollContainerRef;
    private IJSObjectReference? _jsModule;
    private bool _isLoadingMore;
    private int _infiniteScrollVersion;
    private int _sortingVersion;

    // ShouldRender tracking fields
    private IEnumerable<TItem>? _lastData;
    private DataViewLayout _lastLayout;
    private bool _lastIsLoading;
    private int _fieldsVersion;
    private int _lastFieldsVersion;
    private string _lastSearchValue = string.Empty;
    private int _paginationVersion;
    private int _lastPaginationVersion;
    private int _slotVersion;
    private int _lastSlotVersion;
    private int _lastInfiniteScrollVersion;
    private int _lastSortingVersion;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the data source for the view.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Data { get; set; } = Array.Empty<TItem>();

    /// <summary>
    /// Gets or sets the template used to render each item in list layout mode.
    /// The context variable provides the current data item of type TItem.
    /// When only ListTemplate is provided the component locks into list mode and hides
    /// the layout-toggle buttons. Provide both ListTemplate and GridTemplate to allow
    /// the user to switch between layouts via the toolbar toggle.
    /// Alternatively, place a BbDataViewListTemplate inside the Fields fragment.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ListTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template used to render each item in grid layout mode.
    /// The context variable provides the current data item of type TItem.
    /// When only GridTemplate is provided the component locks into grid mode and hides
    /// the layout-toggle buttons. Provide both ListTemplate and GridTemplate to allow
    /// the user to switch between layouts via the toolbar toggle.
    /// Alternatively, place a BbDataViewGridTemplate inside the Fields fragment.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? GridTemplate { get; set; }

    /// <summary>
    /// Gets or sets the column definitions as child content.
    /// Use BbDataViewColumn components to define columns declaratively.
    /// BbDataViewListTemplate and BbDataViewGridTemplate may also be placed here.
    /// </summary>
    [Parameter]
    public RenderFragment? Fields { get; set; }

    /// <summary>
    /// Gets or sets the layout mode.
    /// Default is List.
    /// </summary>
    [Parameter]
    public DataViewLayout Layout { get; set; } = DataViewLayout.List;

    /// <summary>
    /// Gets or sets whether to show the toolbar with global search and sort controls.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show pagination controls.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool ShowPagination { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the view is in a loading state.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets the available page size options.
    /// Default is [5, 10, 20, 50, 100].
    /// </summary>
    [Parameter]
    public int[] PageSizes { get; set; } = { 5, 10, 20, 50, 100 };

    /// <summary>
    /// Gets or sets the initial page size (also the batch size in infinite scroll mode).
    /// Default is 5.
    /// </summary>
    [Parameter]
    public int InitialPageSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether infinite scroll mode is enabled.
    /// In this mode pagination is replaced by progressive item loading.
    /// Use ShowLoadMoreButton for an explicit button, or let the component
    /// auto-detect when the scroll container reaches its bottom edge.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool EnableInfiniteScroll { get; set; }

    /// <summary>
    /// Gets or sets whether to show an explicit "Load more" button when infinite scroll is enabled.
    /// When true, a full-width button is rendered below the items container (outside the grid),
    /// so it always spans the full width regardless of the current grid column count.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool ShowLoadMoreButton { get; set; }

    /// <summary>
    /// Gets or sets the height of the scroll container when auto-scroll infinite scroll is active
    /// (EnableInfiniteScroll = true, ShowLoadMoreButton = false).
    /// Accepts any valid CSS height value, e.g. "400px", "60vh", "30rem".
    /// When null, no inline height is applied and the container inherits its height from the layout.
    /// </summary>
    [Parameter]
    public string? ScrollHeight { get; set; }

    /// <summary>
    /// Gets or sets a custom template for the empty state.
    /// If null, displays default "No results found" message via BbEmpty.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>
    /// Gets or sets a custom template for the loading state.
    /// If null, displays default "Loading..." message.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Gets or sets a function to preprocess data before automatic processing.
    /// </summary>
    [Parameter]
    public Func<IEnumerable<TItem>, Task<IEnumerable<TItem>>>? PreprocessData { get; set; }

    /// <summary>
    /// Gets or sets optional custom actions rendered on the right side of the toolbar.
    /// Use this to add buttons (e.g. Add, Export) alongside the built-in search, sort, and
    /// layout-toggle controls. Only visible when ShowToolbar is true.
    /// </summary>
    [Parameter]
    public RenderFragment? ToolbarActions { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the root container div.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional Tailwind CSS classes applied to the grid layout container.
    /// Merged on top of the default <c>grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4</c>
    /// via TailwindMerge, so any class you provide that conflicts with a default (e.g.
    /// <c>grid-cols-2</c>, <c>gap-6</c>) will win. Supply only the classes you want to
    /// override — the rest of the defaults are preserved.
    /// </summary>
    [Parameter]
    public string? GridClass { get; set; }

    /// <summary>
    /// Gets or sets additional Tailwind CSS classes applied to the list layout container.
    /// Merged on top of the default <c>flex flex-col gap-2</c> via TailwindMerge, so any
    /// conflicting class (e.g. <c>gap-4</c>) wins. To remove the gap entirely and use
    /// dividers instead, pass <c>gap-0 divide-y</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// // Wider spacing between rows
    /// ListClass="gap-4"
    ///
    /// // Compact divider-style list with no gap
    /// ListClass="gap-0 divide-y"
    /// </code>
    /// </example>
    [Parameter]
    public string? ListClass { get; set; }

    /// <summary>
    /// Event callback invoked when sorting changes.
    /// </summary>
    [Parameter]
    public EventCallback<(string FieldId, SortDirection Direction)> OnSort { get; set; }

    /// <summary>
    /// Event callback invoked when the global search value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnFilter { get; set; }

    // ── Computed properties ──────────────────────────────────────────────────

    private string ContainerCssClass => ClassNames.cn("w-full space-y-4", Class);

    private string ItemContainerCssClass => _effectiveLayout == DataViewLayout.Grid
        ? ClassNames.cn("grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4", GridClass)
        : ClassNames.cn("flex flex-col gap-2", ListClass);

    /// <summary>
    /// The resolved layout, accounting for which templates are available.
    /// If only one template is set the layout is locked to that mode regardless of the
    /// Layout parameter. If both are set the Layout parameter (and the toolbar toggle)
    /// control the active layout.
    /// </summary>
    private DataViewLayout _effectiveLayout
    {
        get
        {
            var hasList = EffectiveListTemplate != null;
            var hasGrid = EffectiveGridTemplate != null;
            if (hasList && !hasGrid)
            {
                return DataViewLayout.List;
            }

            if (hasGrid && !hasList)
            {
                return DataViewLayout.Grid;
            }

            return Layout;
        }
    }

    /// <summary>
    /// True when both a list and a grid template are provided, enabling the layout-toggle buttons.
    /// </summary>
    private bool CanToggleLayout => EffectiveListTemplate != null && EffectiveGridTemplate != null;

    /// <summary>
    /// True when there are more batched items to reveal in infinite scroll mode.
    /// </summary>
    private bool CanLoadMore => EnableInfiniteScroll
        && _currentInfinitePage * _paginationState.PageSize < _filteredSortedData.Count();

    /// <summary>
    /// The list template in effect: the named parameter takes precedence over any
    /// slot component that called SetListTemplate (BbDataViewListTemplate).
    /// </summary>
    private RenderFragment<TItem>? EffectiveListTemplate => ListTemplate ?? _registeredListTemplate;

    /// <summary>
    /// The grid template in effect: the named parameter takes precedence over any
    /// slot component that called SetGridTemplate (BbDataViewGridTemplate).
    /// </summary>
    private RenderFragment<TItem>? EffectiveGridTemplate => GridTemplate ?? _registeredGridTemplate;

    /// <summary>
    /// The active item template based on the current effective layout.
    /// </summary>
    private RenderFragment<TItem>? EffectiveActiveTemplate
        => _effectiveLayout == DataViewLayout.Grid ? EffectiveGridTemplate : EffectiveListTemplate;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    protected override void OnInitialized()
    {
        _paginationState.PageSize = InitialPageSize;
        _paginationState.CurrentPage = 1;
    }

    protected override async Task OnParametersSetAsync() => await ProcessDataAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Load the JS module once, only when scroll-based infinite scroll is active.
        if (firstRender && EnableInfiniteScroll && !ShowLoadMoreButton && _jsModule == null)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/data-view.js");
        }
    }

    // ── Slot registration ────────────────────────────────────────────────────

    /// <summary>
    /// Registers a column with the data view.
    /// Called by BbDataViewColumn during initialization.
    /// </summary>
    internal void RegisterField<TValue>(BbDataViewColumn<TItem, TValue> field)
    {
        _fields.Add(new FieldData
        {
            Id = field.EffectiveId,
            Header = field.Header,
            Property = field.Property != null ? item => field.Property(item) : null,
            Sortable = field.Sortable,
            Filterable = field.Filterable
        });

        _fieldsVersion++;
    }

    /// <summary>
    /// Sets the list-mode item rendering template.
    /// Called by BbDataViewListTemplate during initialization.
    /// </summary>
    internal void SetListTemplate(RenderFragment<TItem>? template)
    {
        _registeredListTemplate = template;
        _slotVersion++;
        StateHasChanged();
    }

    /// <summary>
    /// Sets the grid-mode item rendering template.
    /// Called by BbDataViewGridTemplate during initialization.
    /// </summary>
    internal void SetGridTemplate(RenderFragment<TItem>? template)
    {
        _registeredGridTemplate = template;
        _slotVersion++;
        StateHasChanged();
    }

    // ── Data processing ──────────────────────────────────────────────────────

    private async Task ProcessDataAsync()
    {
        var data = Data ?? Array.Empty<TItem>();

        if (PreprocessData != null)
        {
            data = await PreprocessData(data);
        }

        var filtered = ApplyFiltering(data);
        var sorted = ApplySorting(filtered);

        _filteredSortedData = sorted.ToList();
        _paginationState.TotalItems = _filteredSortedData.Count();

        if (EnableInfiniteScroll)
        {
            // Reveal items from pages 1..N; N is incremented by LoadMore / scroll.
            _visibleData = _filteredSortedData
                .Take(_currentInfinitePage * _paginationState.PageSize)
                .ToList();
        }
        else
        {
            _visibleData = _filteredSortedData
                .Skip(_paginationState.StartIndex)
                .Take(_paginationState.PageSize)
                .ToList();
        }
    }

    private IEnumerable<TItem> ApplyFiltering(IEnumerable<TItem> data)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
        {
            return data;
        }

        var searchValue = _searchValue;

        var filterableFields = _fields.Where(f => f.Filterable).ToList();
        if (filterableFields.Count == 0)
        {
            // Fall back to all fields that expose a Property selector.
            filterableFields = _fields.Where(f => f.Property != null).ToList();
        }

        if (filterableFields.Count == 0)
        {
            return data;
        }

        return data.Where(item => MatchesSearch(item, searchValue, filterableFields));
    }

    private static bool MatchesSearch(TItem item, string searchValue, List<FieldData> fields)
    {
        foreach (var field in fields)
        {
            if (field.Property == null)
            {
                continue;
            }

            try
            {
                var value = field.Property(item);
                if (value == null)
                {
                    continue;
                }

                var stringValue = value.ToString();
                if (!string.IsNullOrEmpty(stringValue) &&
                    stringValue.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch
            {
                // Skip fields that cause errors during property access.
            }
        }

        return false;
    }

    private IEnumerable<TItem> ApplySorting(IEnumerable<TItem> data)
    {
        if (_sortingState.Direction == SortDirection.None)
        {
            return data;
        }

        var field = _fields.FirstOrDefault(f => f.Id == _sortingState.SortedColumn);
        if (field?.Property == null)
        {
            return data;
        }

        return _sortingState.Direction == SortDirection.Ascending
            ? data.OrderBy(item => field.Property(item))
            : data.OrderByDescending(item => field.Property(item));
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private async Task HandleSearchChanged()
    {
        _paginationState.CurrentPage = 1;
        _currentInfinitePage = 1;

        if (OnFilter.HasDelegate)
        {
            await OnFilter.InvokeAsync(_searchValue);
        }

        await ProcessDataAsync();
    }

    private async Task HandleSortFieldChanged(string? fieldId)
    {
        if (string.IsNullOrEmpty(fieldId))
        {
            _sortingState.ClearSort();
        }
        else if (_sortingState.SortedColumn == fieldId)
        {
            // 3-state cycle on the same field: none → asc → desc → none
            _sortingState.ToggleSort(fieldId);
        }
        else
        {
            _sortingState.SetSort(fieldId, SortDirection.Ascending);
        }

        _paginationState.CurrentPage = 1;
        _currentInfinitePage = 1;
        _sortingVersion++;

        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync((_sortingState.SortedColumn ?? string.Empty, _sortingState.Direction));
        }

        await ProcessDataAsync();
        StateHasChanged();
    }

    private async Task HandlePageChanged(int newPage)
    {
        _paginationVersion++;
        await ProcessDataAsync();
        StateHasChanged();
    }

    private async Task HandlePageSizeChanged(int newPageSize)
    {
        _paginationVersion++;
        await ProcessDataAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Fires on scroll events when the infinite-scroll container is active
    /// (EnableInfiniteScroll = true, ShowLoadMoreButton = false).
    /// Uses the data-view.js module to check whether the container is near its
    /// bottom edge — this works correctly regardless of whether the inner content
    /// uses a flex list or a multi-column CSS grid.
    /// </summary>
    private async Task HandleScroll(EventArgs e)
    {
        if (_isLoadingMore || !CanLoadMore || _jsModule == null)
        {
            return;
        }

        try
        {
            var nearBottom = await _jsModule.InvokeAsync<bool>("isNearBottom", _scrollContainerRef, 80.0);
            if (nearBottom)
            {
                await LoadMore();
            }
        }
        catch
        {
            // Ignore JS errors (e.g. during server-side prerendering).
            _isLoadingMore = false;
        }
    }

    /// <summary>
    /// Loads the next batch of items in infinite scroll mode.
    /// </summary>
    private async Task LoadMore()
    {
        if (!CanLoadMore)
        {
            return;
        }

        _isLoadingMore = true;
        _currentInfinitePage++;
        _infiniteScrollVersion++;
        await ProcessDataAsync();
        _isLoadingMore = false;
        StateHasChanged();
    }

    private void SetLayout(DataViewLayout layout)
    {
        if (!CanToggleLayout)
        {
            return;
        }

        Layout = layout;
        StateHasChanged();
    }

    private string GetSortFieldLabel()
    {
        if (string.IsNullOrEmpty(_sortingState.SortedColumn))
        {
            return "Sort";
        }

        var field = _fields.FirstOrDefault(f => f.Id == _sortingState.SortedColumn);
        return field?.Header ?? "Sort";
    }

    // ── ShouldRender ─────────────────────────────────────────────────────────

    protected override bool ShouldRender()
    {
        var dataChanged = !ReferenceEquals(_lastData, Data);
        var layoutChanged = _lastLayout != Layout;
        var loadingChanged = _lastIsLoading != IsLoading;
        var fieldsChanged = _lastFieldsVersion != _fieldsVersion;
        var searchChanged = _lastSearchValue != _searchValue;
        var paginationChanged = _lastPaginationVersion != _paginationVersion;
        var slotChanged = _lastSlotVersion != _slotVersion;
        var infiniteScrollChanged = _lastInfiniteScrollVersion != _infiniteScrollVersion;
        var sortingChanged = _lastSortingVersion != _sortingVersion;

        if (dataChanged || layoutChanged || loadingChanged || fieldsChanged || searchChanged || paginationChanged || slotChanged || infiniteScrollChanged || sortingChanged)
        {
            _lastData = Data;
            _lastLayout = Layout;
            _lastIsLoading = IsLoading;
            _lastFieldsVersion = _fieldsVersion;
            _lastSearchValue = _searchValue;
            _lastPaginationVersion = _paginationVersion;
            _lastSlotVersion = _slotVersion;
            _lastInfiniteScrollVersion = _infiniteScrollVersion;
            _lastSortingVersion = _sortingVersion;
            return true;
        }

        return false;
    }

    // ── Disposal ─────────────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            await _jsModule.DisposeAsync();
            _jsModule = null;
        }

        GC.SuppressFinalize(this);
    }
}
