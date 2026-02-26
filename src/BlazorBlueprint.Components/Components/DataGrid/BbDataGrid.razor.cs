using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Table;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Enterprise-grade DataGrid component with multi-column sorting, pagination, selection,
/// row virtualization, and IQueryable/ItemsProvider data sources.
/// Built on the headless DataGrid primitives with Tailwind CSS styling.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGrid<TData> : ComponentBase, IDisposable where TData : class
{
    private DataGridState<TData> _gridState = new();
    private readonly List<IDataGridColumn<TData>> _columns = new();
    private IEnumerable<TData> _processedData = Array.Empty<TData>();
    private IEnumerable<TData> _allSortedData = Array.Empty<TData>();
    private List<TData>? _processedDataList;
    private CancellationTokenSource? _loadCts;
    private bool _selectAllDropdownOpen;

    // ShouldRender tracking
    private IEnumerable<TData>? _lastItems;
    private int _columnsVersion;
    private int _lastColumnsVersion;
    private int _stateVersion;
    private int _lastStateVersion;

    /// <summary>
    /// The data source for the grid. Can be IQueryable&lt;TData&gt; or IEnumerable&lt;TData&gt;.
    /// Mutually exclusive with <see cref="ItemsProvider"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<TData>? Items { get; set; }

    /// <summary>
    /// Async data provider for server-side data fetching.
    /// Mutually exclusive with <see cref="Items"/>.
    /// </summary>
    [Parameter]
    public DataGridItemsProvider<TData>? ItemsProvider { get; set; }

    /// <summary>
    /// Declarative column definitions (PropertyColumn, TemplateColumn, SelectColumn).
    /// </summary>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <summary>
    /// External grid state for controlled mode. Use with @bind-State.
    /// </summary>
    [Parameter]
    public DataGridState<TData>? State { get; set; }

    /// <summary>
    /// Event callback for controlled state changes.
    /// </summary>
    [Parameter]
    public EventCallback<DataGridState<TData>> StateChanged { get; set; }

    /// <summary>
    /// Selection mode: None, Single, or Multiple.
    /// </summary>
    [Parameter]
    public DataTableSelectionMode SelectionMode { get; set; } = DataTableSelectionMode.None;

    /// <summary>
    /// Event callback for selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<TData>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Whether to show the pagination footer. Default is true.
    /// </summary>
    [Parameter]
    public bool ShowPagination { get; set; } = true;

    /// <summary>
    /// Initial page size. Default is 10.
    /// </summary>
    [Parameter]
    public int InitialPageSize { get; set; } = 10;

    /// <summary>
    /// Available page sizes for the pagination selector.
    /// </summary>
    [Parameter]
    public int[] PageSizes { get; set; } = { 5, 10, 20, 50, 100 };

    /// <summary>
    /// Whether the grid is loading data.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Custom loading template.
    /// </summary>
    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    /// <summary>
    /// Custom empty state template.
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>
    /// Enable row virtualization for large datasets. Default is false.
    /// </summary>
    [Parameter]
    public bool Virtualize { get; set; }

    /// <summary>
    /// Row height in pixels for virtualization. Default is 48.
    /// </summary>
    [Parameter]
    public float ItemSize { get; set; } = 48f;

    /// <summary>
    /// Enable keyboard navigation for rows. Default is true.
    /// </summary>
    [Parameter]
    public bool EnableKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// ARIA label for the grid.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional CSS classes applied to the root container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Event callback invoked when sorting changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<SortDefinition>> OnSort { get; set; }

    private DataGridState<TData> EffectiveState => State ?? _gridState;

    protected override void OnInitialized()
    {
        _gridState.Pagination.PageSize = InitialPageSize;
        _gridState.Selection.Mode = GetPrimitiveSelectionMode();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (State != null)
        {
            _gridState = State;
        }

        _gridState.Selection.Mode = GetPrimitiveSelectionMode();

        await ProcessDataAsync();
    }

    /// <summary>
    /// Registers a column definition from a child column component.
    /// Called during the child's OnInitialized.
    /// </summary>
    internal void RegisterColumn<TProp>(BbDataGridPropertyColumn<TData, TProp> column)
    {
        _columns.Add(column);
        _columnsVersion++;
    }

    /// <summary>
    /// Registers a template column.
    /// </summary>
    internal void RegisterColumn(BbDataGridTemplateColumn<TData> column)
    {
        _columns.Add(column);
        _columnsVersion++;
    }

    /// <summary>
    /// Registers a select column.
    /// </summary>
    internal void RegisterColumn(BbDataGridSelectColumn<TData> column)
    {
        // Insert select column at the beginning
        _columns.Insert(0, column);
        _columnsVersion++;
    }

    private IEnumerable<IDataGridColumn<TData>> GetVisibleColumns() =>
        _columns.Where(c => c.Visible);

    private async Task ProcessDataAsync()
    {
        if (ItemsProvider != null)
        {
            await LoadFromProviderAsync();
        }
        else if (Items != null)
        {
            ProcessInMemoryData();
        }
        else
        {
            _processedData = Array.Empty<TData>();
            _processedDataList = null;
        }

        _stateVersion++;
    }

    private void ProcessInMemoryData()
    {
        var data = Items ?? Array.Empty<TData>();
        var columns = _columns.AsReadOnly();

        if (data is IQueryable<TData> queryable)
        {
            var sorted = queryable.ApplyMultiSort(
                _gridState.Sorting.Definitions, columns);

            _gridState.Pagination.TotalItems = sorted.Count();
            _processedData = sorted
                .Skip(_gridState.Pagination.StartIndex)
                .Take(_gridState.Pagination.PageSize)
                .ToList();
            _allSortedData = Array.Empty<TData>();
        }
        else
        {
            var sorted = data.ApplyMultiSort(
                _gridState.Sorting.Definitions, columns);

            var list = sorted as IList<TData> ?? sorted.ToList();
            _allSortedData = list;
            _gridState.Pagination.TotalItems = list.Count;

            var start = Math.Min(_gridState.Pagination.StartIndex, list.Count);
            var take = Math.Min(_gridState.Pagination.PageSize, list.Count - start);

            if (list is List<TData> typedList && take > 0)
            {
                _processedData = typedList.GetRange(start, take);
            }
            else if (take > 0)
            {
                var result = new TData[take];
                for (var i = 0; i < take; i++)
                {
                    result[i] = list[start + i];
                }

                _processedData = result;
            }
            else
            {
                _processedData = Array.Empty<TData>();
            }
        }

        _processedDataList = Virtualize ? _processedData.ToList() : null;
    }

    private async Task LoadFromProviderAsync()
    {
        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        try
        {
            var request = new DataGridRequest
            {
                SortDefinitions = _gridState.Sorting.Definitions,
                StartIndex = _gridState.Pagination.StartIndex,
                Count = _gridState.Pagination.PageSize,
                CancellationToken = token
            };

            var result = await ItemsProvider!(request);

            if (!token.IsCancellationRequested)
            {
                _processedData = result.Items;
                _gridState.Pagination.TotalItems = result.TotalItemCount;
                _processedDataList = Virtualize ? _processedData.ToList() : null;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when a new request supersedes the previous
        }
    }

    private async Task HandleSortChange(IReadOnlyList<SortDefinition> definitions)
    {
        await ProcessDataAsync();

        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync(definitions);
        }

        StateHasChanged();
    }

    private async Task HandleSelectionChange(IReadOnlyCollection<TData> selectedItems)
    {
        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(selectedItems);
        }

        StateHasChanged();
    }

    private async Task HandleSelectAllChanged(bool isChecked)
    {
        if (!isChecked)
        {
            await HandleClearSelection();
            return;
        }

        if (ShouldShowSelectAllPrompt())
        {
            _selectAllDropdownOpen = true;
            StateHasChanged();
            return;
        }

        await HandleSelectAllOnCurrentPage();
    }

    private async Task HandleSelectAllOnCurrentPage()
    {
        foreach (var item in _processedData)
        {
            _gridState.Selection.Select(item);
        }

        _selectAllDropdownOpen = false;
        _stateVersion++;

        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(_gridState.Selection.SelectedItems);
        }

        StateHasChanged();
    }

    private async Task HandleSelectAllItems()
    {
        foreach (var item in _allSortedData)
        {
            _gridState.Selection.Select(item);
        }

        _selectAllDropdownOpen = false;
        _stateVersion++;

        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(_gridState.Selection.SelectedItems);
        }

        StateHasChanged();
    }

    private async Task HandleClearSelection()
    {
        _gridState.Selection.Clear();
        _selectAllDropdownOpen = false;
        _stateVersion++;

        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(_gridState.Selection.SelectedItems);
        }

        StateHasChanged();
    }

    private bool ShouldShowSelectAllPrompt() =>
        _allSortedData.Any() && _gridState.Pagination.TotalItems > _processedData.Count();

    private async Task HandleRowSelectionChanged(TData item, bool isChecked)
    {
        if (isChecked)
        {
            _gridState.Selection.Select(item);
        }
        else
        {
            _gridState.Selection.Deselect(item);
        }

        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(_gridState.Selection.SelectedItems);
        }

        StateHasChanged();
    }

    private async Task HandlePageChanged(int page)
    {
        _gridState.Pagination.GoToPage(page);
        await ProcessDataAsync();
        StateHasChanged();
    }

    private async Task HandlePageSizeChanged(int pageSize)
    {
        _gridState.Pagination.PageSize = pageSize;
        await ProcessDataAsync();
        StateHasChanged();
    }

    private bool IsAllSelected() =>
        _processedData.Any() && _gridState.Selection.AreAllSelected(_processedData);

    private bool IsSomeSelected() =>
        _gridState.Selection.AreSomeSelected(_processedData);

    private SelectionMode GetPrimitiveSelectionMode() => SelectionMode switch
    {
        DataTableSelectionMode.Single => Primitives.Table.SelectionMode.Single,
        DataTableSelectionMode.Multiple => Primitives.Table.SelectionMode.Multiple,
        _ => Primitives.Table.SelectionMode.None
    };

    private static string GetHeaderCellClass(IDataGridColumn<TData> column, bool isSelectColumn)
    {
        var baseClass = "h-12 px-4 text-left align-middle font-medium text-muted-foreground";

        if (isSelectColumn)
        {
            return ClassNames.cn(baseClass, "w-12");
        }

        return ClassNames.cn(baseClass, column.Sortable ? "cursor-pointer select-none group/header" : "");
    }

    private static string GetCellClass(IDataGridColumn<TData> column, bool isSelectColumn)
    {
        var baseClass = "p-4 align-middle";

        if (isSelectColumn)
        {
            return ClassNames.cn(baseClass, "w-12");
        }

        // Check if the column has a CellClass via the property or template column
        var cellClass = column switch
        {
            BbDataGridPropertyColumn<TData, string> pc => pc.CellClass,
            _ => null
        };

        return ClassNames.cn(baseClass, cellClass);
    }

    private static string? GetColumnWidthStyle(IDataGridColumn<TData> column) =>
        column.Width != null ? $"width: {column.Width}" : null;

    protected override bool ShouldRender()
    {
        var itemsChanged = !ReferenceEquals(_lastItems, Items);
        var columnsChanged = _lastColumnsVersion != _columnsVersion;
        var stateChanged = _lastStateVersion != _stateVersion;

        if (itemsChanged || columnsChanged || stateChanged)
        {
            _lastItems = Items;
            _lastColumnsVersion = _columnsVersion;
            _lastStateVersion = _stateVersion;
            return true;
        }

        return true; // Default to true for now — optimize later once stable
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _loadCts?.Cancel();
        _loadCts?.Dispose();
    }
}
