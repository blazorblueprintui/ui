using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// Enterprise-grade DataGrid component with multi-column sorting, pagination, selection,
/// row virtualization, and IQueryable/ItemsProvider data sources.
/// Built on the headless DataGrid primitives with Tailwind CSS styling.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGrid<TData> : ComponentBase, IAsyncDisposable where TData : class
{
    private DataGridState<TData> _gridState = new();
    private readonly List<IDataGridColumn<TData>> _columns = new();
    private IEnumerable<TData> _processedData = Array.Empty<TData>();
    private IEnumerable<TData> _allSortedData = Array.Empty<TData>();
    private IEnumerable<TData>? _lastProcessedData;
    private List<TData>? _processedDataList;
    private CancellationTokenSource? _loadCts;
    private bool _selectAllDropdownOpen;
    private bool _needsDataRefresh = true;
    private bool columnStateInitialized;

    // Cached per-render visible column data to avoid recomputing per row
    private List<IDataGridColumn<TData>> _cachedVisibleColumns = new();
    private string? _cachedLastLeftId;
    private string? _cachedFirstRightId;

    // JS interop state
    private IJSObjectReference? columnsModule;
    private DotNetObjectReference<BbDataGrid<TData>>? selfRef;
    private ElementReference containerRef;
    private readonly string gridId = Guid.NewGuid().ToString("N");
    private bool jsInitialized;

    // ShouldRender tracking
    private IEnumerable<TData>? _lastItems;
    private int _columnsVersion;
    private int _lastColumnsVersion;
    private int _stateVersion;
    private int _lastStateVersion;
    private int _lastGridStateVersion;

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

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
    /// Keep the header row visible while scrolling. Useful with virtualized grids
    /// inside a fixed-height scroll container. Default is false.
    /// </summary>
    [Parameter]
    public bool StickyHeader { get; set; }

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
    /// Optional callback for applying custom CSS classes to data rows.
    /// Receives the row's data item and returns additional CSS classes, or null.
    /// </summary>
    [Parameter]
    public Func<TData, string?>? RowClass { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Toolbar content rendered above the grid. Use for column visibility toggles,
    /// search inputs, or other controls that need grid context.
    /// </summary>
    [Parameter]
    public RenderFragment? Toolbar { get; set; }

    /// <summary>
    /// Event callback invoked when sorting changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<SortDefinition>> OnSort { get; set; }

    /// <summary>
    /// Enable column resizing by dragging header cell borders. Default is false.
    /// </summary>
    [Parameter]
    public bool Resizable { get; set; }

    /// <summary>
    /// Minimum column width in pixels when resizing. Default is 50.
    /// </summary>
    [Parameter]
    public int MinColumnWidth { get; set; } = 50;

    /// <summary>
    /// Event callback invoked when a column is resized.
    /// </summary>
    [Parameter]
    public EventCallback<(string ColumnId, string Width)> OnColumnResize { get; set; }

    /// <summary>
    /// Enable column reordering by dragging header cells. Default is false.
    /// </summary>
    [Parameter]
    public bool Reorderable { get; set; }

    /// <summary>
    /// Event callback invoked when a column is reordered.
    /// </summary>
    [Parameter]
    public EventCallback<(string ColumnId, int NewIndex)> OnColumnReorder { get; set; }

    internal DataGridState<TData> EffectiveState => State ?? _gridState;

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

        // Detect external state mutations (e.g., Restore() or Reset() called from outside)
        var externalStateChanged = _gridState.Version != _lastGridStateVersion;
        if (externalStateChanged)
        {
            _lastGridStateVersion = _gridState.Version;
            // Reset() clears column entries — re-initialize on next access.
            // Restore() repopulates entries with saved order — skip re-init to preserve it.
            if (_gridState.Columns.Entries.Count == 0)
            {
                columnStateInitialized = false;
            }
        }

        // Only reprocess data when something meaningful changed
        var itemsChanged = !ReferenceEquals(_lastItems, Items);
        if (itemsChanged || _needsDataRefresh || externalStateChanged)
        {
            _needsDataRefresh = false;
            await ProcessDataAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!Resizable && !Reorderable)
        {
            return;
        }

        try
        {
            if (!jsInitialized)
            {
                columnsModule = await Js.InvokeAsync<IJSObjectReference>("import",
                    "./_content/BlazorBlueprint.Components/js/datagrid-columns.js");
                selfRef = DotNetObjectReference.Create(this);
                jsInitialized = true;

                if (Resizable)
                {
                    await columnsModule.InvokeVoidAsync("initColumnResize", containerRef, selfRef, gridId, MinColumnWidth);
                }

                if (Reorderable)
                {
                    await columnsModule.InvokeVoidAsync("initColumnReorder", containerRef, selfRef, gridId);
                }
            }

            if (jsInitialized)
            {
                if (Resizable)
                {
                    await columnsModule!.InvokeVoidAsync("setupResizeHandles", gridId);
                }

                if (Reorderable)
                {
                    var reorderableIds = GetVisibleColumns()
                        .Where(c => c.Reorderable)
                        .Select(c => c.ColumnId)
                        .ToArray();
                    await columnsModule!.InvokeVoidAsync("setupDraggableHeaders", gridId, reorderableIds);
                }
            }
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            // Expected during circuit disconnect in Blazor Server
        }
        catch (InvalidOperationException)
        {
            // JS interop not available during prerendering
        }
    }

    /// <summary>
    /// Registers a column definition from a child column component.
    /// Called during the child's OnInitialized.
    /// </summary>
    internal void RegisterColumn<TProp>(BbDataGridPropertyColumn<TData, TProp> column)
    {
        _columns.Add(column);
        _columnsVersion++;
        StateHasChanged();
    }

    /// <summary>
    /// Registers a template column.
    /// </summary>
    internal void RegisterColumn(BbDataGridTemplateColumn<TData> column)
    {
        _columns.Add(column);
        _columnsVersion++;
        StateHasChanged();
    }

    /// <summary>
    /// Registers a select column.
    /// </summary>
    internal void RegisterColumn(BbDataGridSelectColumn<TData> column)
    {
        // Insert select column at the beginning
        _columns.Insert(0, column);
        _columnsVersion++;
        StateHasChanged();
    }

    /// <summary>
    /// Gets all registered columns (for child components like column visibility toggle).
    /// </summary>
    internal IReadOnlyList<IDataGridColumn<TData>> GetAllColumns() => _columns.AsReadOnly();

    private List<IDataGridColumn<TData>> GetVisibleColumns()
    {
        // Column components register in OnInitialized (during render), which runs
        // after ProcessDataAsync in OnParametersSetAsync. Ensure the column state
        // is initialized once columns have registered.
        InitializeColumnState();

        if (!columnStateInitialized)
        {
            var visible = _columns.Where(c => c.Visible).ToList();
            return PartitionByPinning(visible);
        }

        var visibleIds = _gridState.Columns.GetVisibleColumnIds();
        var columnMap = new Dictionary<string, IDataGridColumn<TData>>(_columns.Count);
        foreach (var col in _columns)
        {
            columnMap[col.ColumnId] = col;
        }

        var result = new List<IDataGridColumn<TData>>(visibleIds.Count);
        foreach (var id in visibleIds)
        {
            if (columnMap.TryGetValue(id, out var column))
            {
                result.Add(column);
            }
        }

        return PartitionByPinning(result);
    }

    /// <summary>
    /// Refreshes the cached visible columns, pin boundary IDs. Called once per render
    /// pass so that row rendering can reuse the result without recomputing per row.
    /// </summary>
    private void RefreshVisibleColumnsCache()
    {
        _cachedVisibleColumns = GetVisibleColumns();
        _cachedLastLeftId = _cachedVisibleColumns.LastOrDefault(c => c.Pinned == ColumnPinning.Left)?.ColumnId;
        _cachedFirstRightId = _cachedVisibleColumns.FirstOrDefault(c => c.Pinned == ColumnPinning.Right)?.ColumnId;
    }

    /// <summary>
    /// Reorders a list of columns so left-pinned come first, unpinned in the middle,
    /// and right-pinned come last. Preserves relative order within each group.
    /// </summary>
    private static List<IDataGridColumn<TData>> PartitionByPinning(List<IDataGridColumn<TData>> columns)
    {
        if (!columns.Any(c => c.Pinned != ColumnPinning.None))
        {
            return columns;
        }

        var leftPinned = new List<IDataGridColumn<TData>>();
        var unpinned = new List<IDataGridColumn<TData>>();
        var rightPinned = new List<IDataGridColumn<TData>>();

        foreach (var col in columns)
        {
            switch (col.Pinned)
            {
                case ColumnPinning.Left:
                    leftPinned.Add(col);
                    break;
                case ColumnPinning.Right:
                    rightPinned.Add(col);
                    break;
                default:
                    unpinned.Add(col);
                    break;
            }
        }

        var result = new List<IDataGridColumn<TData>>(columns.Count);
        result.AddRange(leftPinned);
        result.AddRange(unpinned);
        result.AddRange(rightPinned);
        return result;
    }

    private void InitializeColumnState()
    {
        if (columnStateInitialized || _columns.Count == 0)
        {
            return;
        }

        _gridState.Columns.Initialize(_columns.Select(c => c.ColumnId));
        columnStateInitialized = true;
    }

    private async Task ProcessDataAsync()
    {
        InitializeColumnState();

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

            if (Virtualize)
            {
                // Virtualization: skip pagination, pass all data to the Virtualize component
                _processedData = sorted.ToList();
            }
            else
            {
                _processedData = sorted
                    .Skip(_gridState.Pagination.StartIndex)
                    .Take(_gridState.Pagination.PageSize)
                    .ToList();
            }

            _allSortedData = Array.Empty<TData>();
        }
        else
        {
            var sorted = data.ApplyMultiSort(
                _gridState.Sorting.Definitions, columns);

            var list = sorted as IList<TData> ?? sorted.ToList();
            _allSortedData = list;
            _gridState.Pagination.TotalItems = list.Count;

            if (Virtualize)
            {
                // Virtualization: skip pagination, pass all data to the Virtualize component
                _processedData = list;
            }
            else
            {
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
        }

        // Only recreate the virtualization list when the data reference actually changes
        if (Virtualize)
        {
            if (_processedDataList == null || !ReferenceEquals(_lastProcessedData, _processedData))
            {
                _processedDataList = _processedData as List<TData> ?? _processedData.ToList();
                _lastProcessedData = _processedData;
            }
        }
        else
        {
            _processedDataList = null;
            _lastProcessedData = null;
        }
    }

    private async Task LoadFromProviderAsync()
    {
        var oldCts = _loadCts;
        oldCts?.Cancel();
        oldCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        try
        {
            var request = new DataGridRequest
            {
                SortDefinitions = _gridState.Sorting.Definitions,
                StartIndex = Virtualize ? 0 : _gridState.Pagination.StartIndex,
                Count = Virtualize ? null : _gridState.Pagination.PageSize,
                CancellationToken = token
            };

            var result = await ItemsProvider!(request);

            if (!token.IsCancellationRequested)
            {
                _processedData = result.Items;
                _gridState.Pagination.TotalItems = result.TotalItemCount;

                if (Virtualize)
                {
                    if (_processedDataList == null || !ReferenceEquals(_lastProcessedData, _processedData))
                    {
                        _processedDataList = _processedData as List<TData> ?? _processedData.ToList();
                        _lastProcessedData = _processedData;
                    }
                }
                else
                {
                    _processedDataList = null;
                    _lastProcessedData = null;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when a new request supersedes the previous
        }
    }

    private async Task NotifyStateChangedAsync()
    {
        if (StateChanged.HasDelegate)
        {
            await StateChanged.InvokeAsync(EffectiveState);
        }
    }

    private async Task HandleSortChange(IReadOnlyList<SortDefinition> definitions)
    {
        await ProcessDataAsync();

        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync(definitions);
        }

        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    private async Task HandleSelectionChange(IReadOnlyCollection<TData> selectedItems)
    {
        _stateVersion++;

        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(selectedItems);
        }

        await NotifyStateChangedAsync();
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

        await NotifyStateChangedAsync();
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

        await NotifyStateChangedAsync();
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

        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Handles column visibility change from the toggle component.
    /// </summary>
    internal async Task HandleColumnVisibilityChangedAsync(string columnId, bool visible)
    {
        _gridState.Columns.SetVisibility(columnId, visible);
        _stateVersion++;
        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Called from JS when a column resize drag completes.
    /// Receives all column widths to commit to state at once.
    /// </summary>
    [JSInvokable]
    public async Task OnResizeCompleted(string resizedColumnId, Dictionary<string, double> widths)
    {
        foreach (var (colId, widthPx) in widths)
        {
            _gridState.Columns.SetWidth(colId, $"{Math.Round(widthPx)}px");
        }

        _stateVersion++;

        if (OnColumnResize.HasDelegate)
        {
            var width = widths.TryGetValue(resizedColumnId, out var w) ? $"{Math.Round(w)}px" : "";
            await OnColumnResize.InvokeAsync((resizedColumnId, width));
        }

        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Called from JS when a column is reordered via drag-and-drop.
    /// </summary>
    [JSInvokable]
    public async Task OnColumnReordered(string columnId, int newIndex)
    {
        _gridState.Columns.ReorderColumn(columnId, newIndex);
        _stateVersion++;

        if (OnColumnReorder.HasDelegate)
        {
            await OnColumnReorder.InvokeAsync((columnId, newIndex));
        }

        await NotifyStateChangedAsync();
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

        _stateVersion++;

        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync(_gridState.Selection.SelectedItems);
        }

        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    private async Task HandlePageChanged(int page)
    {
        _gridState.Pagination.GoToPage(page);
        await ProcessDataAsync();
        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    private async Task HandlePageSizeChanged(int pageSize)
    {
        _gridState.Pagination.PageSize = pageSize;
        await ProcessDataAsync();
        await NotifyStateChangedAsync();
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

    private bool HasTableFixed() =>
        Resizable || _columns.Any(c => c.Pinned != ColumnPinning.None);

    private string GetHeaderCellClass(IDataGridColumn<TData> column, bool isSelectColumn,
        bool isLastLeft, bool isFirstRight)
    {
        var baseClass = "h-12 px-4 text-left align-middle font-medium text-muted-foreground";

        var pinnedClass = "";
        if (column.Pinned != ColumnPinning.None)
        {
            var zClass = StickyHeader ? "z-20" : "z-10";
            pinnedClass = ClassNames.cn("bg-background", zClass);
        }

        var separatorClass = "";
        if (isLastLeft)
        {
            separatorClass = "border-r border-border";
        }
        else if (isFirstRight)
        {
            separatorClass = "border-l border-border";
        }

        if (isSelectColumn)
        {
            return ClassNames.cn(baseClass, "w-12", pinnedClass, separatorClass);
        }

        var needsGroup = column.Sortable || (Reorderable && column.Reorderable);
        var sortClass = column.Sortable ? "cursor-pointer select-none" : "";
        var groupClass = needsGroup ? "group/header" : "";
        var needsRelative = (Resizable && column.Resizable) || (Reorderable && column.Reorderable);
        var positionClass = needsRelative ? "relative" : "";
        var overflowClass = Resizable ? "overflow-hidden" : "";

        return ClassNames.cn(baseClass, sortClass, groupClass, positionClass, overflowClass,
            pinnedClass, separatorClass, column.HeaderClass);
    }

    private string GetCellClass(IDataGridColumn<TData> column, bool isSelectColumn,
        bool isLastLeft, bool isFirstRight)
    {
        var baseClass = "p-4 align-middle";

        var pinnedClass = "";
        if (column.Pinned != ColumnPinning.None)
        {
            pinnedClass = ClassNames.cn("bg-background", "z-10");
        }

        var separatorClass = "";
        if (isLastLeft)
        {
            separatorClass = "border-r border-border";
        }
        else if (isFirstRight)
        {
            separatorClass = "border-l border-border";
        }

        if (isSelectColumn)
        {
            return ClassNames.cn(baseClass, "w-12", pinnedClass, separatorClass);
        }

        var cellClass = column.CellClass;

        var overflowClass = Resizable ? "overflow-hidden" : "";

        return ClassNames.cn(baseClass, cellClass, overflowClass, pinnedClass, separatorClass);
    }

    private string? GetColumnWidthStyle(IDataGridColumn<TData> column)
    {
        var stateWidth = columnStateInitialized ? _gridState.Columns.GetWidth(column.ColumnId) : null;
        var width = stateWidth ?? column.Width;
        return width != null ? $"width: {width}" : null;
    }

    /// <summary>
    /// Resolves a column's effective pixel width from state (post-resize) or the column's
    /// declared Width parameter. Falls back to 150px for non-pixel or missing widths,
    /// since sticky offset calculation requires a numeric value.
    /// </summary>
    private double GetEffectiveWidthPx(IDataGridColumn<TData> column)
    {
        var stateWidth = columnStateInitialized ? _gridState.Columns.GetWidth(column.ColumnId) : null;
        var width = stateWidth ?? column.Width;

        if (width != null && width.EndsWith("px", StringComparison.OrdinalIgnoreCase)
            && double.TryParse(width.AsSpan(0, width.Length - 2),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var px))
        {
            return px;
        }

        return 150.0;
    }

    /// <summary>
    /// Computes the CSS style string for a pinned column (position: sticky + left/right offset).
    /// Returns null for unpinned columns.
    /// </summary>
    private string? GetPinnedStyle(IDataGridColumn<TData> column,
        IReadOnlyList<IDataGridColumn<TData>> visibleColumns)
    {
        if (column.Pinned == ColumnPinning.None)
        {
            return null;
        }

        var offset = 0.0;

        if (column.Pinned == ColumnPinning.Left)
        {
            foreach (var col in visibleColumns)
            {
                if (col.ColumnId == column.ColumnId)
                {
                    break;
                }

                if (col.Pinned == ColumnPinning.Left)
                {
                    offset += GetEffectiveWidthPx(col);
                }
            }

            return $"position: sticky; left: {offset.ToString(System.Globalization.CultureInfo.InvariantCulture)}px";
        }
        else
        {
            for (var i = visibleColumns.Count - 1; i >= 0; i--)
            {
                var col = visibleColumns[i];
                if (col.ColumnId == column.ColumnId)
                {
                    break;
                }

                if (col.Pinned == ColumnPinning.Right)
                {
                    offset += GetEffectiveWidthPx(col);
                }
            }

            return $"position: sticky; right: {offset.ToString(System.Globalization.CultureInfo.InvariantCulture)}px";
        }
    }

    /// <summary>
    /// Merges the column width style and pinned style into a single style string.
    /// </summary>
    private string? GetColumnStyle(IDataGridColumn<TData> column,
        IReadOnlyList<IDataGridColumn<TData>> visibleColumns)
    {
        var widthStyle = GetColumnWidthStyle(column);
        var pinnedStyle = GetPinnedStyle(column, visibleColumns);

        if (pinnedStyle != null && widthStyle != null)
        {
            return $"{widthStyle}; {pinnedStyle}";
        }

        return pinnedStyle ?? widthStyle;
    }

    protected override bool ShouldRender()
    {
        var itemsChanged = !ReferenceEquals(_lastItems, Items);
        var columnsChanged = _lastColumnsVersion != _columnsVersion;
        var stateChanged = _lastStateVersion != _stateVersion;
        var externalStateChanged = _gridState.Version != _lastGridStateVersion;

        if (itemsChanged || columnsChanged || stateChanged || externalStateChanged)
        {
            _lastItems = Items;
            _lastColumnsVersion = _columnsVersion;
            _lastStateVersion = _stateVersion;
            _lastGridStateVersion = _gridState.Version;
            return true;
        }

        return false;
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _loadCts?.Cancel();
        _loadCts?.Dispose();

        if (columnsModule != null && jsInitialized)
        {
            try
            {
                await columnsModule.InvokeVoidAsync("dispose", gridId);
            }
            catch
            {
                // Cleanup may already be disposed
            }

            try
            {
                await columnsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
        }

        selfRef?.Dispose();
    }
}
