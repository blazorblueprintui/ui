using BlazorBlueprint.Primitives.Table;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.DataGrid;

/// <summary>
/// Headless DataGrid primitive component providing multi-sort, pagination, selection,
/// and keyboard navigation. Supports IQueryable, IEnumerable, and async ItemsProvider data sources.
/// </summary>
/// <typeparam name="TData">The type of data items in the grid.</typeparam>
public partial class BbDataGrid<TData> : ComponentBase, IDisposable where TData : class
{
    private DataGridContext<TData> context = null!;
    private DataGridState<TData> internalState = new();
    private IEnumerable<TData> processedData = Array.Empty<TData>();
    private int stateVersion;
    private readonly List<Task> pendingTasks = new();
    private CancellationTokenSource? loadCts;

    // ShouldRender tracking
    private int lastRenderVersion;
    private object? lastItems;
    private SelectionMode lastSelectionMode;
    private bool lastManualPagination;

    /// <summary>
    /// The data source for the grid. Can be IQueryable&lt;TData&gt; or IEnumerable&lt;TData&gt;.
    /// Mutually exclusive with <see cref="ItemsProvider"/>.
    /// When IQueryable, sort and pagination are composed as LINQ expressions.
    /// </summary>
    [Parameter]
    public IEnumerable<TData>? Items { get; set; }

    /// <summary>
    /// Async data provider delegate for server-side data fetching.
    /// Mutually exclusive with <see cref="Items"/>.
    /// </summary>
    [Parameter]
    public DataGridItemsProvider<TData>? ItemsProvider { get; set; }

    /// <summary>
    /// The grid state (controlled mode). Use with @bind-State for two-way binding.
    /// </summary>
    [Parameter]
    public DataGridState<TData>? State { get; set; }

    /// <summary>
    /// Event callback for controlled state changes.
    /// </summary>
    [Parameter]
    public EventCallback<DataGridState<TData>> StateChanged { get; set; }

    /// <summary>
    /// The selection mode for the grid.
    /// </summary>
    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

    /// <summary>
    /// Event callback invoked when sorting changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<SortDefinition>> OnSortChange { get; set; }

    /// <summary>
    /// Event callback invoked when a row is selected.
    /// </summary>
    [Parameter]
    public EventCallback<TData> OnRowSelect { get; set; }

    /// <summary>
    /// Event callback invoked when the current page changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPageChange { get; set; }

    /// <summary>
    /// Event callback invoked when the page size changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> OnPageSizeChange { get; set; }

    /// <summary>
    /// Event callback invoked when the selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<TData>> OnSelectionChange { get; set; }

    /// <summary>
    /// Child content (header, body, etc.).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// ARIA label for the grid.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// When true, the grid will not automatically manage TotalItems or paginate data.
    /// Use when the parent handles pagination and passes pre-paginated data.
    /// </summary>
    [Parameter]
    public bool ManualPagination { get; set; }

    /// <summary>
    /// When true, enables keyboard navigation for grid rows.
    /// </summary>
    [Parameter]
    public bool EnableKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// The registered column definitions. Set by the Components layer during column registration.
    /// </summary>
    public IReadOnlyList<IDataGridColumn<TData>> Columns
    {
        get => context.Columns;
        set => context.Columns = value;
    }

    private bool IsControlled => State != null;

    private DataGridState<TData> EffectiveState => IsControlled ? State! : internalState;

    internal DataGridContext<TData> Context => context;

    protected override void OnInitialized()
    {
        InitializeContext();
        SetupEventHandlers();
    }

    protected override async Task OnParametersSetAsync()
    {
        context.SelectionMode = SelectionMode;
        context.EnableKeyboardNavigation = EnableKeyboardNavigation;
        EffectiveState.Selection.Mode = SelectionMode;

        if (IsControlled && context.State != State)
        {
            context.State = State!;
            stateVersion++;
        }

        await ProcessDataAsync();
    }

    private void InitializeContext()
    {
        context = new DataGridContext<TData>(EffectiveState)
        {
            SelectionMode = SelectionMode,
            EnableKeyboardNavigation = EnableKeyboardNavigation
        };

        EffectiveState.Selection.Mode = SelectionMode;
    }

    private void SetupEventHandlers()
    {
        context.OnSortChange = (definitions) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnSortChange.HasDelegate)
                    {
                        await OnSortChange.InvokeAsync(definitions);
                    }

                    await ProcessDataAndNotifyAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnSortChange: {ex.Message}");
                }
            });
            pendingTasks.Add(task);
        };

        context.OnPageChange = (page) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnPageChange.HasDelegate)
                    {
                        await OnPageChange.InvokeAsync(page);
                    }

                    await ProcessDataAndNotifyAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnPageChange: {ex.Message}");
                }
            });
            pendingTasks.Add(task);
        };

        context.OnPageSizeChange = (pageSize) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnPageSizeChange.HasDelegate)
                    {
                        await OnPageSizeChange.InvokeAsync(pageSize);
                    }

                    await ProcessDataAndNotifyAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnPageSizeChange: {ex.Message}");
                }
            });
            pendingTasks.Add(task);
        };

        context.OnRowSelect = (item) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnRowSelect.HasDelegate)
                    {
                        await OnRowSelect.InvokeAsync(item);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnRowSelect: {ex.Message}");
                }
            });
            pendingTasks.Add(task);
        };

        context.OnSelectionChange = (selectedItems) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnSelectionChange.HasDelegate)
                    {
                        await OnSelectionChange.InvokeAsync(selectedItems);
                    }

                    await NotifyStateChangedAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnSelectionChange: {ex.Message}");
                }
            });
            pendingTasks.Add(task);
        };

        context.OnStateChanged += HandleContextStateChanged;
    }

    private async Task ProcessDataAsync()
    {
        var currentState = EffectiveState;

        if (ItemsProvider != null)
        {
            await LoadFromProviderAsync(currentState);
        }
        else if (Items != null)
        {
            ProcessInMemoryData(currentState);
        }
        else
        {
            processedData = Array.Empty<TData>();
            context.ProcessedData = processedData;
        }
    }

    private void ProcessInMemoryData(DataGridState<TData> currentState)
    {
        var data = Items ?? Array.Empty<TData>();

        // When ManualPagination is true, the parent (Components layer) handles
        // sorting and pagination. Just pass the data through as-is.
        if (ManualPagination)
        {
            processedData = data;
            context.ProcessedData = processedData;
            return;
        }

        if (data is IQueryable<TData> queryable)
        {
            // IQueryable path: compose LINQ expressions
            var sorted = queryable.ApplyMultiSort(
                currentState.Sorting.Definitions,
                context.Columns);

            // Count before pagination
            currentState.Pagination.TotalItems = sorted.Count();
            processedData = sorted
                .Skip(currentState.Pagination.StartIndex)
                .Take(currentState.Pagination.PageSize)
                .ToList();
        }
        else
        {
            // IEnumerable path: in-memory sort + paginate
            var sorted = data.ApplyMultiSort(
                currentState.Sorting.Definitions,
                context.Columns);

            var list = sorted as IList<TData> ?? sorted.ToList();
            currentState.Pagination.TotalItems = list.Count;

            var start = Math.Min(currentState.Pagination.StartIndex, list.Count);
            var take = Math.Min(currentState.Pagination.PageSize, list.Count - start);

            if (list is List<TData> typedList)
            {
                processedData = take > 0
                    ? typedList.GetRange(start, take)
                    : Array.Empty<TData>();
            }
            else
            {
                var result = new TData[take];
                for (var i = 0; i < take; i++)
                {
                    result[i] = list[start + i];
                }

                processedData = result;
            }
        }

        context.ProcessedData = processedData;
    }

    private async Task LoadFromProviderAsync(DataGridState<TData> currentState)
    {
        // Cancel any previous load
        loadCts?.Cancel();
        loadCts = new CancellationTokenSource();
        var token = loadCts.Token;

        context.IsLoading = true;

        try
        {
            var request = new DataGridRequest
            {
                SortDefinitions = currentState.Sorting.Definitions,
                StartIndex = ManualPagination ? 0 : currentState.Pagination.StartIndex,
                Count = ManualPagination ? null : currentState.Pagination.PageSize,
                CancellationToken = token
            };

            var result = await ItemsProvider!(request);

            if (!token.IsCancellationRequested)
            {
                processedData = result.Items;
                if (!ManualPagination)
                {
                    currentState.Pagination.TotalItems = result.TotalItemCount;
                }

                context.ProcessedData = processedData;
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when a new request supersedes the previous one
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                context.IsLoading = false;
            }
        }
    }

    private void HandleContextStateChanged()
    {
        stateVersion++;

        if (IsControlled && StateChanged.HasDelegate)
        {
            _ = StateChanged.InvokeAsync(State!);
        }

        StateHasChanged();
    }

    private async Task ProcessDataAndNotifyAsync()
    {
        stateVersion++;
        await ProcessDataAsync();

        if (IsControlled && StateChanged.HasDelegate)
        {
            await StateChanged.InvokeAsync(State!);
        }

        StateHasChanged();
    }

    private async Task NotifyStateChangedAsync()
    {
        stateVersion++;

        if (IsControlled && StateChanged.HasDelegate)
        {
            await StateChanged.InvokeAsync(State!);
        }

        StateHasChanged();
    }

    private int GetAriaRowCount() => EffectiveState.Pagination.TotalItems + 1; // +1 for header row

    protected override bool ShouldRender()
    {
        if (IsControlled)
        {
            return true;
        }

        var itemsChanged = !ReferenceEquals(lastItems, Items);
        var selectionModeChanged = lastSelectionMode != SelectionMode;
        var paginationChanged = lastManualPagination != ManualPagination;
        var versionChanged = lastRenderVersion != stateVersion;

        if (itemsChanged || selectionModeChanged || paginationChanged || versionChanged)
        {
            lastItems = Items;
            lastSelectionMode = SelectionMode;
            lastManualPagination = ManualPagination;
            lastRenderVersion = stateVersion;
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        context.OnStateChanged -= HandleContextStateChanged;
        loadCts?.Cancel();
        loadCts?.Dispose();

        if (pendingTasks.Count > 0)
        {
            try
            {
                if (!OperatingSystem.IsBrowser())
                {
                    Task.WaitAll(pendingTasks.ToArray(), TimeSpan.FromSeconds(1));
                }
            }
            catch (AggregateException)
            {
                // Tasks cancelled or timed out — expected during disposal
            }
            finally
            {
                pendingTasks.Clear();
            }
        }
    }
}
