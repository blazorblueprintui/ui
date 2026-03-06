using System.Linq.Expressions;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Filtering;
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

    // Grouping state
    private List<DataGridRenderItem<TData>>? _groupedRenderItems;
    private Func<TData, object?>? _groupByAccessor;
    private string? _groupByColumnId;
    private string? _groupByColumnTitle;
    private bool _groupsCollapsedByDefault;
    private RenderFragment<DataGridGroupContext<TData>>? _groupColumnHeaderTemplate;
    private Expression<Func<TData, object>>? _lastGroupBy;
    private List<object>? _allGroupKeys;

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

    // Context menu state
    private BbContextMenu? rowContextMenu;
    private TData? contextMenuItem;
    private IJSObjectReference? clipboardModule;

    // ItemKey tracking
    private Func<TData, object>? _lastItemKey;

    // ShouldRender tracking
    private bool _parametersChanged;
    private IEnumerable<TData>? _lastItems;
    private bool _lastIsLoading;
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
    /// Template for expanded row detail content. When set with an expand column,
    /// clicking the expand chevron reveals this content beneath the row.
    /// </summary>
    [Parameter]
    public RenderFragment<TData>? DetailTemplate { get; set; }

    /// <summary>
    /// Event callback invoked when a row is expanded.
    /// </summary>
    [Parameter]
    public EventCallback<TData> OnRowExpand { get; set; }

    /// <summary>
    /// Event callback invoked when a row is collapsed.
    /// </summary>
    [Parameter]
    public EventCallback<TData> OnRowCollapse { get; set; }

    /// <summary>
    /// Event callback invoked when a row is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<TData> OnRowClick { get; set; }

    /// <summary>
    /// Render fragment for the row context menu. When set, right-clicking a row opens
    /// a context menu with the provided content. Receives a <see cref="DataGridRowMenuContext{TData}"/>
    /// with the clicked item and a clipboard helper.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGridRowMenuContext<TData>>? RowContextMenu { get; set; }

    /// <summary>
    /// Event callback invoked when sorting changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<SortDefinition>> OnSort { get; set; }

    /// <summary>
    /// Event callback invoked when column filters change.
    /// Receives the active filters keyed by column ID.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyDictionary<string, FilterCondition>> OnFilter { get; set; }

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

    /// <summary>
    /// A function that returns a stable identity key for each data item
    /// (e.g., <c>item =&gt; item.Id</c>). When set, selection and expansion state
    /// is tracked by key instead of reference equality, so state survives data re-fetches.
    /// Also forwarded to the Virtualize component when virtualization is enabled.
    /// </summary>
    [Parameter]
    public Func<TData, object>? ItemKey { get; set; }

    /// <summary>
    /// Expression to group rows by. When set, rows are grouped by the value of this expression
    /// and group headers are rendered between groups.
    /// </summary>
    [Parameter]
    public Expression<Func<TData, object>>? GroupBy { get; set; }

    /// <summary>
    /// Custom template for group header rows. If not set, a default header is rendered
    /// showing the group key, item count, and aggregates.
    /// </summary>
    [Parameter]
    public RenderFragment<DataGridGroupContext<TData>>? GroupHeaderTemplate { get; set; }

    /// <summary>
    /// Event callback invoked when a group is expanded.
    /// </summary>
    [Parameter]
    public EventCallback<DataGridGroupRow<TData>> OnGroupExpand { get; set; }

    /// <summary>
    /// Event callback invoked when a group is collapsed.
    /// </summary>
    [Parameter]
    public EventCallback<DataGridGroupRow<TData>> OnGroupCollapse { get; set; }

    /// <summary>
    /// Whether groups should be collapsed by default. Default is false.
    /// </summary>
    [Parameter]
    public bool GroupsCollapsedByDefault { get; set; }

    /// <summary>
    /// Async grouped data provider for server-side grouped data fetching.
    /// When set and grouping is active, this provider is called instead of <see cref="ItemsProvider"/>.
    /// </summary>
    [Parameter]
    public DataGridGroupedItemsProvider<TData>? GroupedItemsProvider { get; set; }

    internal DataGridState<TData> EffectiveState => State ?? _gridState;

    protected override void OnInitialized()
    {
        _gridState.Pagination.PageSize = InitialPageSize;
        _gridState.Selection.Mode = GetPrimitiveSelectionMode();
        ApplyItemKeyComparer();
    }

    protected override async Task OnParametersSetAsync()
    {
        _parametersChanged = true;

        if (State != null)
        {
            _gridState = State;
        }

        _gridState.Selection.Mode = GetPrimitiveSelectionMode();

        if (!ReferenceEquals(_lastItemKey, ItemKey))
        {
            ApplyItemKeyComparer();
        }

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

        // Apply GroupBy parameter (GroupColumn takes precedence if both are set)
        if (GroupBy != null && _groupByAccessor == null)
        {
            InitializeGroupBy();
        }
        else if (GroupBy != null && !ReferenceEquals(GroupBy, _lastGroupBy))
        {
            // Expression instances are recreated on every parent render, so compare
            // the string representation to detect actual logical changes.
            if (GroupBy.ToString() != _lastGroupBy?.ToString())
            {
                InitializeGroupBy();
            }
            else
            {
                _lastGroupBy = GroupBy;
            }
        }
        else if (GroupBy == null && _groupByAccessor != null && _groupColumnHeaderTemplate == null)
        {
            // GroupBy was removed and no GroupColumn is active
            _groupByAccessor = null;
            _groupByColumnId = null;
            _groupByColumnTitle = null;
            _gridState.Grouping.ClearGroup();
            _groupedRenderItems = null;
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
        if (_needsDataRefresh)
        {
            _needsDataRefresh = false;
            await ProcessDataAsync();
            StateHasChanged();
        }

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
        OnColumnRegistered();
    }

    /// <summary>
    /// Registers a template column.
    /// </summary>
    internal void RegisterColumn(BbDataGridTemplateColumn<TData> column)
    {
        _columns.Add(column);
        OnColumnRegistered();
    }

    /// <summary>
    /// Registers a select column.
    /// </summary>
    internal void RegisterColumn(BbDataGridSelectColumn<TData> column)
    {
        // Insert select column at the beginning
        _columns.Insert(0, column);
        OnColumnRegistered();
    }

    /// <summary>
    /// Registers an expand column. Inserted after the select column (if present),
    /// or at position 0.
    /// </summary>
    internal void RegisterColumn(BbDataGridExpandColumn<TData> column)
    {
        var selectIndex = _columns.FindIndex(c => c.ColumnId == "__select");
        var insertIndex = selectIndex >= 0 ? selectIndex + 1 : 0;
        _columns.Insert(insertIndex, column);
        OnColumnRegistered();
    }

    private void OnColumnRegistered()
    {
        _columnsVersion++;

        // When grouping is active, aggregates computed before all columns registered
        // will be empty — mark for reprocessing so aggregates are recomputed.
        if (_groupedRenderItems != null && _columns.Any(c => c.Aggregate != AggregateFunction.None))
        {
            _needsDataRefresh = true;
        }

        StateHasChanged();
    }

    /// <summary>
    /// Configures grouping from a <see cref="BbDataGridGroupColumn{TData,TProperty}"/> child component.
    /// </summary>
    internal void SetGrouping<TProperty>(BbDataGridGroupColumn<TData, TProperty> groupColumn)
    {
        _groupByAccessor = groupColumn.GetValueAccessor();
        _groupByColumnId = groupColumn.GetColumnId();
        _groupByColumnTitle = groupColumn.GetTitle();
        _groupsCollapsedByDefault = groupColumn.CollapsedByDefault;
        _groupColumnHeaderTemplate = groupColumn.HeaderTemplate;

        _gridState.Grouping.SetGroup(new GroupDefinition
        {
            ColumnId = _groupByColumnId,
            GroupSortDirection = groupColumn.GroupSortDirection
        });

        _needsDataRefresh = true;
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

    private void InitializeGroupBy()
    {
        var compiled = GroupBy!.Compile();
        _groupByAccessor = item => compiled(item);

        // Unwrap boxing conversions (e.g., value-type member to object) so we can
        // reliably detect the underlying MemberExpression.
        var body = (Expression)GroupBy.Body;
        if (body is UnaryExpression unary &&
            (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
        {
            body = unary.Operand;
        }

        if (body is MemberExpression member)
        {
            _groupByColumnId = member.Member.Name.ToLowerInvariant();
            _groupByColumnTitle = member.Member.Name;
        }
        else
        {
            _groupByColumnId = "group";
            _groupByColumnTitle = "Group";
        }

        _groupsCollapsedByDefault = GroupsCollapsedByDefault;
        _lastGroupBy = GroupBy;

        _gridState.Grouping.SetGroup(new GroupDefinition
        {
            ColumnId = _groupByColumnId,
            GroupSortDirection = SortDirection.Ascending
        });

        _needsDataRefresh = true;
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
            var filtered = ApplyColumnFilters(queryable);
            var sorted = filtered.ApplyMultiSort(
                _gridState.Sorting.Definitions, columns);

            var sortedList = sorted.ToList();

            if (_groupByAccessor != null)
            {
                ProcessGroupedData(sortedList);
                return;
            }

            _groupedRenderItems = null;
            _gridState.Pagination.TotalItems = sortedList.Count;

            if (Virtualize)
            {
                _processedData = sortedList;
            }
            else
            {
                _processedData = sortedList
                    .Skip(_gridState.Pagination.StartIndex)
                    .Take(_gridState.Pagination.PageSize)
                    .ToList();
            }

            _allSortedData = Array.Empty<TData>();
        }
        else
        {
            var filtered = ApplyColumnFilters(data);
            var sorted = filtered.ApplyMultiSort(
                _gridState.Sorting.Definitions, columns);

            var list = sorted as IList<TData> ?? sorted.ToList();

            if (_groupByAccessor != null)
            {
                ProcessGroupedData(list);
                return;
            }

            _groupedRenderItems = null;
            _allSortedData = list;
            _gridState.Pagination.TotalItems = list.Count;

            if (Virtualize)
            {
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

        UpdateVirtualizationList();
    }

    private void ProcessGroupedData(IList<TData> sortedData)
    {
        var groupDef = _gridState.Grouping.ActiveGroup;
        var accessor = _groupByAccessor!;

        // Group the sorted data
        var groups = sortedData
            .GroupBy(item => accessor(item))
            .ToList();

        // Sort groups by key
        if (groupDef?.GroupSortDirection == SortDirection.Descending)
        {
            groups = groups.OrderByDescending(g => g.Key).ToList();
        }
        else
        {
            groups = groups.OrderBy(g => g.Key).ToList();
        }

        // Handle collapsed-by-default on first process
        var isFirstProcess = _gridState.Grouping.CollapsedKeys.Count == 0
            && _groupsCollapsedByDefault
            && _groupedRenderItems == null;

        // Build ordered list of groups with their metadata
        var groupInfos = new List<(object Key, List<TData> Items, DataGridGroupRow<TData> Row)>();
        foreach (var group in groups)
        {
            var items = group.ToList();
            var groupKey = group.Key ?? "(empty)";

            if (isFirstProcess && _groupsCollapsedByDefault)
            {
                _gridState.Grouping.CollapseAll(new[] { groupKey });
            }

            var aggregates = ComputeAggregates(items, _columns);
            var groupRow = new DataGridGroupRow<TData>
            {
                Key = groupKey,
                ColumnId = _groupByColumnId ?? "group",
                ColumnTitle = _groupByColumnTitle,
                ItemCount = items.Count,
                Items = items,
                Aggregates = aggregates
            };

            groupInfos.Add((groupKey, items, groupRow));
        }

        _allSortedData = sortedData;
        _allGroupKeys = groupInfos.Select(g => g.Key).ToList();

        // Count total visible data rows (collapsed groups contribute 0 data rows)
        var totalDataRows = 0;
        foreach (var (key, items, _) in groupInfos)
        {
            if (!_gridState.Grouping.IsCollapsed(key))
            {
                totalDataRows += items.Count;
            }
        }

        _gridState.Pagination.TotalItems = totalDataRows;

        if (Virtualize)
        {
            // For virtualization, build the full flattened list
            var allRenderItems = new List<DataGridRenderItem<TData>>();
            var allDataItems = new List<TData>();
            foreach (var (key, items, row) in groupInfos)
            {
                allRenderItems.Add(DataGridRenderItem<TData>.ForGroup(row));
                if (!_gridState.Grouping.IsCollapsed(key))
                {
                    foreach (var item in items)
                    {
                        allRenderItems.Add(DataGridRenderItem<TData>.ForData(item));
                        allDataItems.Add(item);
                    }
                }
            }

            _groupedRenderItems = allRenderItems;
            _processedData = allDataItems;
        }
        else
        {
            // Paginate by data rows only — group headers are "free" and don't count.
            // Groups that span a page boundary get their header repeated on each page.
            // Collapsed groups appear at their natural position between expanded groups.
            var pageStart = _gridState.Pagination.StartIndex;
            var pageSize = _gridState.Pagination.PageSize;
            var pageEnd = pageStart + pageSize;
            var pageRenderItems = new List<DataGridRenderItem<TData>>();
            var pageDataItems = new List<TData>();
            var dataRowIndex = 0;

            foreach (var (key, items, row) in groupInfos)
            {
                var isCollapsed = _gridState.Grouping.IsCollapsed(key);

                if (isCollapsed)
                {
                    // Collapsed groups appear at the current data row position.
                    // Show them if that position falls within (or at the boundary of) the current page.
                    if (dataRowIndex >= pageStart && dataRowIndex < pageEnd)
                    {
                        pageRenderItems.Add(DataGridRenderItem<TData>.ForGroup(row));
                    }
                }
                else
                {
                    var groupDataEnd = dataRowIndex + items.Count;

                    if (groupDataEnd > pageStart && dataRowIndex < pageEnd)
                    {
                        // This group has data rows on the current page — emit header
                        pageRenderItems.Add(DataGridRenderItem<TData>.ForGroup(row));

                        // Emit only the data rows that fall within the page window
                        var skipInGroup = Math.Max(0, pageStart - dataRowIndex);
                        var takeFromGroup = Math.Min(items.Count - skipInGroup, pageEnd - (dataRowIndex + skipInGroup));

                        for (var i = skipInGroup; i < skipInGroup + takeFromGroup; i++)
                        {
                            pageRenderItems.Add(DataGridRenderItem<TData>.ForData(items[i]));
                            pageDataItems.Add(items[i]);
                        }
                    }

                    dataRowIndex += items.Count;
                }
            }

            _groupedRenderItems = pageRenderItems;
            _processedData = pageDataItems;
        }

        UpdateVirtualizationList();
    }

    private static Dictionary<string, AggregateResult> ComputeAggregates(
        IReadOnlyList<TData> items, IReadOnlyList<IDataGridColumn<TData>> columns)
    {
        var results = new Dictionary<string, AggregateResult>();

        foreach (var column in columns)
        {
            if (column.Aggregate == AggregateFunction.None)
            {
                continue;
            }

            var value = ComputeAggregate(items, column, column.Aggregate);
            var format = column.AggregateFormat;
            results[column.ColumnId] = new AggregateResult
            {
                Function = column.Aggregate,
                Value = value,
                ColumnId = column.ColumnId,
                Format = format
            };
        }

        return results;
    }

    private static object? ComputeAggregate(
        IReadOnlyList<TData> items, IDataGridColumn<TData> column, AggregateFunction function)
    {
        if (items.Count == 0)
        {
            return null;
        }

        if (function == AggregateFunction.Count)
        {
            return items.Count;
        }

        var values = new List<double>();
        foreach (var item in items)
        {
            var raw = column.GetRawValue(item);
            if (raw != null && TryConvertToDouble(raw, out var numericValue))
            {
                values.Add(numericValue);
            }
        }

        if (values.Count == 0)
        {
            return null;
        }

        return function switch
        {
            AggregateFunction.Sum => values.Sum(),
            AggregateFunction.Average => values.Average(),
            AggregateFunction.Min => values.Min(),
            AggregateFunction.Max => values.Max(),
            _ => null
        };
    }

    private static bool TryConvertToDouble(object value, out double result)
    {
        try
        {
            result = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            result = 0;
            return false;
        }
    }

    private void UpdateVirtualizationList()
    {
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
            var aggregateColumns = _columns
                .Where(c => c.Aggregate != AggregateFunction.None)
                .Select(c => c.ColumnId)
                .ToList();

            // When grouping client-side from a flat provider, fetch all items so
            // ProcessGroupedData can correctly paginate across groups.
            var isClientSideGrouping = _groupByAccessor != null && GroupedItemsProvider == null;

            var request = new DataGridRequest
            {
                SortDefinitions = _gridState.Sorting.Definitions,
                StartIndex = (Virtualize || isClientSideGrouping) ? 0 : _gridState.Pagination.StartIndex,
                Count = (Virtualize || isClientSideGrouping) ? null : _gridState.Pagination.PageSize,
                CancellationToken = token,
                Filters = _gridState.Filtering.Filters,
                GroupDefinition = _gridState.Grouping.ActiveGroup,
                AggregateColumns = aggregateColumns.Count > 0 ? aggregateColumns : null
            };

            // Use grouped provider when grouping is active and provider is available
            if (_groupByAccessor != null && GroupedItemsProvider != null)
            {
                var groupedResult = await GroupedItemsProvider(request);

                if (!token.IsCancellationRequested)
                {
                    BuildRenderItemsFromGroupedResult(groupedResult);
                }
            }
            else
            {
                var result = await ItemsProvider!(request);

                if (!token.IsCancellationRequested)
                {
                    if (_groupByAccessor != null)
                    {
                        // Group client-side from flat provider results
                        var items = result.Items as IList<TData> ?? result.Items.ToList();
                        ProcessGroupedData(items);
                    }
                    else
                    {
                        _groupedRenderItems = null;
                        _processedData = result.Items;
                        _gridState.Pagination.TotalItems = result.TotalItemCount;
                        UpdateVirtualizationList();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when a new request supersedes the previous
        }
    }

    private void BuildRenderItemsFromGroupedResult(DataGridGroupedResult<TData> groupedResult)
    {
        var allRenderItems = new List<DataGridRenderItem<TData>>();
        var allDataItems = new List<TData>();

        foreach (var group in groupedResult.Groups)
        {
            var groupKey = group.Key ?? "(empty)";
            var items = group.Items.ToList();

            var groupRow = new DataGridGroupRow<TData>
            {
                Key = groupKey,
                ColumnId = _groupByColumnId ?? "group",
                ColumnTitle = _groupByColumnTitle,
                ItemCount = group.ItemCount > 0 ? group.ItemCount : items.Count,
                Items = items,
                Aggregates = group.Aggregates ?? new Dictionary<string, AggregateResult>()
            };

            allRenderItems.Add(DataGridRenderItem<TData>.ForGroup(groupRow));

            if (!_gridState.Grouping.IsCollapsed(groupKey))
            {
                foreach (var item in items)
                {
                    allRenderItems.Add(DataGridRenderItem<TData>.ForData(item));
                    allDataItems.Add(item);
                }
            }
        }

        _gridState.Pagination.TotalItems = groupedResult.TotalItemCount;
        _groupedRenderItems = allRenderItems;
        _processedData = allDataItems;
        UpdateVirtualizationList();
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

    /// <summary>
    /// Handles a column filter being applied or cleared.
    /// Updates state, resets pagination, reprocesses data, and fires OnFilter.
    /// </summary>
    internal async Task HandleColumnFilterChanged(string columnId, FilterCondition? condition)
    {
        _gridState.Filtering.SetFilter(columnId, condition);
        _gridState.Pagination.GoToPage(1);
        await ProcessDataAsync();

        if (OnFilter.HasDelegate)
        {
            await OnFilter.InvokeAsync(_gridState.Filtering.Filters);
        }

        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Clears all column filters, reprocesses data, and fires OnFilter.
    /// </summary>
    internal async Task HandleClearAllFilters()
    {
        _gridState.Filtering.ClearAll();
        _gridState.Pagination.GoToPage(1);
        await ProcessDataAsync();

        if (OnFilter.HasDelegate)
        {
            await OnFilter.InvokeAsync(_gridState.Filtering.Filters);
        }

        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    private IQueryable<TData> ApplyColumnFilters(IQueryable<TData> queryable)
    {
        if (!_gridState.Filtering.HasFilters)
        {
            return queryable;
        }

        var columnMap = new Dictionary<string, IDataGridColumn<TData>>(_columns.Count);
        foreach (var col in _columns)
        {
            columnMap[col.ColumnId] = col;
        }

        foreach (var (columnId, condition) in _gridState.Filtering.Filters)
        {
            if (!columnMap.TryGetValue(columnId, out var column) || !column.Filterable)
            {
                continue;
            }

            var field = GetFilterFieldForColumn(column);
            if (field == null)
            {
                continue;
            }

            var filterDef = new FilterDefinition
            {
                Conditions = new List<FilterCondition> { condition }
            };

            var expression = filterDef.ToExpression<TData>(new[] { field });
            queryable = queryable.Where(expression);
        }

        return queryable;
    }

    private IEnumerable<TData> ApplyColumnFilters(IEnumerable<TData> data)
    {
        if (!_gridState.Filtering.HasFilters)
        {
            return data;
        }

        var columnMap = new Dictionary<string, IDataGridColumn<TData>>(_columns.Count);
        foreach (var col in _columns)
        {
            columnMap[col.ColumnId] = col;
        }

        foreach (var (columnId, condition) in _gridState.Filtering.Filters)
        {
            if (!columnMap.TryGetValue(columnId, out var column) || !column.Filterable)
            {
                continue;
            }

            var field = GetFilterFieldForColumn(column);
            if (field == null)
            {
                continue;
            }

            var filterDef = new FilterDefinition
            {
                Conditions = new List<FilterCondition> { condition }
            };

            var predicate = filterDef.ToFunc<TData>(new[] { field });
            data = data.Where(predicate);
        }

        return data;
    }

    private static FilterField? GetFilterFieldForColumn(IDataGridColumn<TData> column)
    {
        if (column is not IFilterableColumn filterable)
        {
            return null;
        }

        return new FilterField
        {
            Name = filterable.GetFilterFieldName(),
            Label = column.Title ?? column.ColumnId,
            Type = filterable.GetFilterFieldType(),
            Options = filterable.GetFilterOptions()
        };
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

    internal async Task HandleGroupToggle(DataGridGroupRow<TData> groupRow)
    {
        var key = groupRow.Key ?? "(empty)";
        var wasCollapsed = _gridState.Grouping.IsCollapsed(key);
        _gridState.Grouping.Toggle(key);

        // Reprocess to rebuild the flattened render items
        await ProcessDataAsync();

        if (wasCollapsed)
        {
            if (OnGroupExpand.HasDelegate)
            {
                await OnGroupExpand.InvokeAsync(groupRow);
            }
        }
        else
        {
            if (OnGroupCollapse.HasDelegate)
            {
                await OnGroupCollapse.InvokeAsync(groupRow);
            }
        }

        _stateVersion++;
        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Expands all groups, showing their data rows.
    /// </summary>
    public async Task ExpandAllGroups()
    {
        _gridState.Grouping.ExpandAll();
        await ProcessDataAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Collapses all groups, hiding their data rows.
    /// </summary>
    public async Task CollapseAllGroups()
    {
        if (_allGroupKeys == null || _allGroupKeys.Count == 0)
        {
            return;
        }

        _gridState.Grouping.CollapseAll(_allGroupKeys);
        await ProcessDataAsync();
        StateHasChanged();
    }

    internal async Task HandleRowExpandToggle(TData item)
    {
        var wasExpanded = _gridState.Expanded.IsExpanded(item);
        _gridState.Expanded.Toggle(item);
        _stateVersion++;

        if (wasExpanded)
        {
            if (OnRowCollapse.HasDelegate)
            {
                await OnRowCollapse.InvokeAsync(item);
            }
        }
        else
        {
            if (OnRowExpand.HasDelegate)
            {
                await OnRowExpand.InvokeAsync(item);
            }
        }

        await NotifyStateChangedAsync();
        StateHasChanged();
    }

    private List<IDataGridColumn<TData>> GetVisibleDataColumns() =>
        GetVisibleColumns()
            .Where(c => c.ColumnId != "__select" && c.ColumnId != "__expand")
            .ToList();

    private async Task HandleRowClick(TData item)
    {
        if (OnRowClick.HasDelegate)
        {
            await OnRowClick.InvokeAsync(item);
        }
    }

    private async Task HandleRowContextMenu((TData Item, double ClientX, double ClientY) args)
    {
        contextMenuItem = args.Item;

        if (rowContextMenu != null)
        {
            await rowContextMenu.OpenAt(args.ClientX, args.ClientY);
        }
    }

    private async Task<bool> CopyToClipboardAsync(string text)
    {
        try
        {
            clipboardModule ??= await Js.InvokeAsync<IJSObjectReference>("import",
                "./_content/BlazorBlueprint.Components/js/clipboard.js");
            return await clipboardModule.InvokeAsync<bool>("copyToClipboard", text);
        }
        catch
        {
            return false;
        }
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

    private void ApplyItemKeyComparer()
    {
        _lastItemKey = ItemKey;
        EffectiveState.SetItemKey(ItemKey);
    }

    private SelectionMode GetPrimitiveSelectionMode() => SelectionMode switch
    {
        DataTableSelectionMode.Single => Primitives.Table.SelectionMode.Single,
        DataTableSelectionMode.Multiple => Primitives.Table.SelectionMode.Multiple,
        _ => Primitives.Table.SelectionMode.None
    };

    private bool HasTableFixed() =>
        Resizable || _columns.Any(c => c.Pinned != ColumnPinning.None);

    private string GetHeaderCellClass(IDataGridColumn<TData> column, bool isSelectColumn,
        bool isExpandColumn, bool isLastLeft, bool isFirstRight)
    {
        var baseClass = "h-12 px-4 text-left align-middle font-medium text-muted-foreground";

        var pinnedClass = "";
        if (column.Pinned != ColumnPinning.None)
        {
            var zClass = StickyHeader ? "z-30" : "z-10";
            pinnedClass = zClass;
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

        if (isSelectColumn || isExpandColumn)
        {
            return ClassNames.cn(baseClass, "w-12", pinnedClass, separatorClass);
        }

        var needsGroup = column.Sortable || column.Filterable || (Reorderable && column.Reorderable);
        var sortClass = column.Sortable ? "cursor-pointer select-none" : "";
        var groupClass = needsGroup ? "group/header" : "";
        var needsRelative = (Resizable && column.Resizable) || (Reorderable && column.Reorderable);
        var positionClass = needsRelative ? "relative" : "";
        var overflowClass = HasTableFixed() ? "overflow-hidden" : "";

        return ClassNames.cn(baseClass, sortClass, groupClass, positionClass, overflowClass,
            pinnedClass, separatorClass, column.HeaderClass);
    }

    /// <summary>
    /// Gets the columns that have aggregate results for a given group, in visible order.
    /// Used to render per-column aggregate cells in the group header row.
    /// </summary>
    private IReadOnlyList<IDataGridColumn<TData>> GetGroupAggregateColumns(DataGridGroupRow<TData> group)
    {
        if (group.Aggregates.Count == 0)
        {
            return Array.Empty<IDataGridColumn<TData>>();
        }

        return _cachedVisibleColumns
            .Where(c => group.Aggregates.ContainsKey(c.ColumnId))
            .ToList();
    }

    /// <summary>
    /// Computes how many columns the group label cell should span — all visible columns
    /// minus those occupied by per-column aggregate cells.
    /// </summary>
    private int GetGroupLabelColSpan(DataGridGroupRow<TData> group)
    {
        var aggregateColumnCount = group.Aggregates.Count == 0
            ? 0
            : _cachedVisibleColumns.Count(c => group.Aggregates.ContainsKey(c.ColumnId));

        return Math.Max(1, _cachedVisibleColumns.Count - aggregateColumnCount);
    }

    private string GetCellClass(IDataGridColumn<TData> column, bool isSelectColumn,
        bool isExpandColumn, bool isLastLeft, bool isFirstRight)
    {
        var baseClass = "p-4 align-middle transition-colors";

        var pinnedClass = "";
        if (column.Pinned != ColumnPinning.None)
        {
            pinnedClass = "z-10";
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

        if (isSelectColumn || isExpandColumn)
        {
            return ClassNames.cn(baseClass, "w-12", pinnedClass, separatorClass);
        }

        var cellClass = column.CellClass;

        var overflowClass = HasTableFixed() ? "overflow-hidden" : "";
        var noWrapClass = column.NoWrap ? "whitespace-nowrap overflow-hidden text-ellipsis" : "";

        return ClassNames.cn(baseClass, cellClass, overflowClass, noWrapClass, pinnedClass, separatorClass);
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
        if (_parametersChanged)
        {
            _parametersChanged = false;
            _lastItems = Items;
            _lastIsLoading = IsLoading;
            _lastColumnsVersion = _columnsVersion;
            _lastStateVersion = _stateVersion;
            _lastGridStateVersion = _gridState.Version;
            return true;
        }

        var itemsChanged = !ReferenceEquals(_lastItems, Items);
        var loadingChanged = _lastIsLoading != IsLoading;
        var columnsChanged = _lastColumnsVersion != _columnsVersion;
        var stateChanged = _lastStateVersion != _stateVersion;
        var externalStateChanged = _gridState.Version != _lastGridStateVersion;

        if (itemsChanged || loadingChanged || columnsChanged || stateChanged || externalStateChanged)
        {
            _lastItems = Items;
            _lastIsLoading = IsLoading;
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

        if (clipboardModule != null)
        {
            try
            {
                await clipboardModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
        }
    }
}
