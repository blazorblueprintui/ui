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
/// DataView provides a PrimeVue-like composition model: use the ItemTemplate render fragment
/// (or the BbDataViewTemplate child component) to define item rendering, BbDataViewField
/// components inside the Fields fragment to register fields for sorting/filtering, and
/// HeaderContent / FooterContent render fragments (or BbDataViewHeader / BbDataViewFooter
/// child components) for optional slot content.
/// </para>
/// <para>
/// Infinite scroll works correctly for both flex-list and multi-column CSS grid layouts:
/// the scroll container is measured as a unit, and the Load More button / scroll sentinel
/// are always placed outside the grid container so they span the full width.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbDataView TItem="Person" Data="@people"&gt;
///     &lt;ItemTemplate Context="person"&gt;
///         &lt;div class="p-4 border rounded-lg"&gt;@person.Name&lt;/div&gt;
///     &lt;/ItemTemplate&gt;
///     &lt;Fields&gt;
///         &lt;BbDataViewField TItem="Person" TValue="string" Property="@(p => p.Name)" Header="Name" Sortable Filterable /&gt;
///     &lt;/Fields&gt;
/// &lt;/BbDataView&gt;
/// </code>
/// </example>
public partial class BbDataView<TItem> : ComponentBase, IDataViewParent, IAsyncDisposable where TItem : class
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

    // Backing fields for slot-component registrations (BbDataViewHeader/Footer/Template).
    // The effective value always prefers the named [Parameter] over the registered one,
    // so both the direct-parameter API and the slot-component API work side-by-side.
    private RenderFragment? _registeredHeaderContent;
    private RenderFragment? _footerContentRegistered;
    private RenderFragment<TItem>? _registeredItemTemplate;

    // Infinite scroll
    private ElementReference _scrollContainerRef;
    private IJSObjectReference? _jsModule;
    private bool _isLoadingMore;

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

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the data source for the view.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Data { get; set; } = Array.Empty<TItem>();

    /// <summary>
    /// Gets or sets the template used to render each item.
    /// The context variable provides the current data item of type TItem.
    /// Alternatively, place a BbDataViewTemplate child component inside the Fields fragment.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets optional header content rendered above the toolbar.
    /// Alternatively, place a BbDataViewHeader child component inside the Fields fragment.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Gets or sets optional footer content rendered below items and pagination.
    /// Alternatively, place a BbDataViewFooter child component inside the Fields fragment.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterContent { get; set; }

    /// <summary>
    /// Gets or sets the field definitions as child content.
    /// Use BbDataViewField components to define fields declaratively.
    /// BbDataViewHeader, BbDataViewFooter, and BbDataViewTemplate may also be placed here.
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
    /// Gets or sets additional CSS classes for the container div.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

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

    private string ItemContainerCssClass => Layout == DataViewLayout.Grid
        ? "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4"
        : "flex flex-col gap-2";

    /// <summary>
    /// True when there are more batched items to reveal in infinite scroll mode.
    /// </summary>
    private bool CanLoadMore => EnableInfiniteScroll
        && _currentInfinitePage * _paginationState.PageSize < _filteredSortedData.Count();

    /// <summary>
    /// The item template in effect: the named parameter takes precedence over any
    /// slot component that called SetItemTemplate (BbDataViewTemplate).
    /// </summary>
    private RenderFragment<TItem>? EffectiveItemTemplate => ItemTemplate ?? _registeredItemTemplate;

    /// <summary>
    /// The header content in effect: the named parameter takes precedence over any
    /// slot component that called SetHeaderContent (BbDataViewHeader).
    /// </summary>
    private RenderFragment? EffectiveHeaderContent => HeaderContent ?? _registeredHeaderContent;

    /// <summary>
    /// The footer content in effect: the named parameter takes precedence over any
    /// slot component that called SetFooterContent (BbDataViewFooter).
    /// </summary>
    private RenderFragment? EffectiveFooterContent => FooterContent ?? _footerContentRegistered;

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

    // ── Slot registration (called by BbDataViewHeader/Footer/Template) ───────

    /// <summary>
    /// Registers a field with the data view.
    /// Called by BbDataViewField during initialization.
    /// </summary>
    internal void RegisterField<TValue>(BbDataViewField<TItem, TValue> field)
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

    /// <inheritdoc />
    void IDataViewParent.SetHeaderContent(RenderFragment? content)
    {
        _registeredHeaderContent = content;
        _slotVersion++;
        StateHasChanged();
    }

    /// <inheritdoc />
    void IDataViewParent.SetFooterContent(RenderFragment? content)
    {
        _footerContentRegistered = content;
        _slotVersion++;
        StateHasChanged();
    }

    /// <summary>
    /// Sets the item rendering template.
    /// Called by BbDataViewTemplate during initialization.
    /// </summary>
    internal void SetItemTemplate(RenderFragment<TItem>? template)
    {
        _registeredItemTemplate = template;
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
        else
        {
            _sortingState.SetSort(fieldId, SortDirection.Ascending);
        }

        _paginationState.CurrentPage = 1;
        _currentInfinitePage = 1;

        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync((_sortingState.SortedColumn ?? string.Empty, _sortingState.Direction));
        }

        await ProcessDataAsync();
        StateHasChanged();
    }

    private async Task HandleSortDirectionToggle()
    {
        if (string.IsNullOrEmpty(_sortingState.SortedColumn))
        {
            return;
        }

        _sortingState.ToggleSort(_sortingState.SortedColumn);

        _paginationState.CurrentPage = 1;
        _currentInfinitePage = 1;

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
        await ProcessDataAsync();
        _isLoadingMore = false;
        StateHasChanged();
    }

    private void SetLayout(DataViewLayout layout)
    {
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

        if (dataChanged || layoutChanged || loadingChanged || fieldsChanged || searchChanged || paginationChanged || slotChanged)
        {
            _lastData = Data;
            _lastLayout = Layout;
            _lastIsLoading = IsLoading;
            _lastFieldsVersion = _fieldsVersion;
            _lastSearchValue = _searchValue;
            _lastPaginationVersion = _paginationVersion;
            _lastSlotVersion = _slotVersion;
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
