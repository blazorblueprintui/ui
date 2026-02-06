using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Primitives.Table;

/// <summary>
/// Headless table primitive component providing sorting, pagination, and selection capabilities.
/// This component wraps the HTML table element and provides behavior without styling.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
public partial class Table<TData> : ComponentBase, IDisposable where TData : class
{
    private TableContext<TData> _context = null!;
    private TableState<TData> _internalState = new();
    private IEnumerable<TData> _processedData = Array.Empty<TData>();
    private int _stateVersion;
    private readonly List<Task> _pendingTasks = new();

    // ShouldRender tracking fields
    private int _lastRenderVersion;
    private IEnumerable<TData>? _lastData;
    private SelectionMode _lastSelectionMode;
    private bool _lastManualPagination;

    /// <summary>
    /// The data source for the table.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TData> Data { get; set; } = Array.Empty<TData>();

    /// <summary>
    /// The table state (controlled mode).
    /// When provided, the table uses external state management.
    /// </summary>
    [Parameter]
    public TableState<TData>? State { get; set; }

    /// <summary>
    /// Event callback invoked when the table state changes (for controlled mode).
    /// Use with @bind-State for two-way binding.
    /// </summary>
    [Parameter]
    public EventCallback<TableState<TData>> StateChanged { get; set; }

    /// <summary>
    /// The selection mode for the table.
    /// </summary>
    [Parameter]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.None;

    /// <summary>
    /// Event callback invoked when sorting changes.
    /// </summary>
    [Parameter]
    public EventCallback<(string ColumnId, SortDirection Direction)> OnSortChange { get; set; }

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
    /// Child content for the table (TableHeader, TableBody, etc.).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// ARIA label for the table.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the table element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional attributes to apply to the table element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// When true, the table will not automatically set TotalItems based on data count.
    /// Use this when the parent component handles pagination and passes pre-paginated data.
    /// </summary>
    [Parameter]
    public bool ManualPagination { get; set; } = false;

    /// <summary>
    /// When true, enables keyboard navigation for table rows (arrow keys to navigate, Enter/Space to select).
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool EnableKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// Gets whether the table is in controlled mode.
    /// </summary>
    private bool IsControlled => State != null;

    /// <summary>
    /// Gets the current effective state (external or internal).
    /// </summary>
    private TableState<TData> EffectiveState => IsControlled ? State! : _internalState;

    /// <summary>
    /// Gets the table context provided to child components.
    /// </summary>
    private TableContext<TData> Context => _context;

    /// <summary>
    /// Initializes the table component, setting up context, event handlers, and processing initial data.
    /// </summary>
    protected override void OnInitialized()
    {
        InitializeContext();
        SetupEventHandlers();
        ProcessData();
    }

    /// <summary>
    /// Initializes the table context with effective state.
    /// </summary>
    private void InitializeContext()
    {
        _context = new TableContext<TData>(EffectiveState)
        {
            SelectionMode = SelectionMode,
            EnableKeyboardNavigation = EnableKeyboardNavigation
        };

        SyncSelectionMode();
    }

    /// <summary>
    /// Synchronizes the SelectionMode parameter with the TableState's Selection.Mode property.
    /// This ensures consistency between the primitive's parameter and internal state, allowing
    /// selection operations to function correctly. Without this synchronization, the SelectionState
    /// would retain its default Mode value (None), causing Select() to return early without effect.
    /// </summary>
    private void SyncSelectionMode() => EffectiveState.Selection.Mode = SelectionMode;

    /// <summary>
    /// Sets up event handlers for sorting, pagination, and selection.
    /// </summary>
    private void SetupEventHandlers()
    {
        _context.OnSortChange = (columnId, direction) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnSortChange.HasDelegate)
                    {
                        await OnSortChange.InvokeAsync((columnId, direction));
                    }

                    await NotifyStateChanged();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnSortChange: {ex.Message}");
                }
            });
            _pendingTasks.Add(task);
        };

        _context.OnPageChange = (page) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnPageChange.HasDelegate)
                    {
                        await OnPageChange.InvokeAsync(page);
                    }

                    await NotifyStateChanged();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnPageChange: {ex.Message}");
                }
            });
            _pendingTasks.Add(task);
        };

        _context.OnPageSizeChange = (pageSize) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnPageSizeChange.HasDelegate)
                    {
                        await OnPageSizeChange.InvokeAsync(pageSize);
                    }

                    await NotifyStateChanged();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnPageSizeChange: {ex.Message}");
                }
            });
            _pendingTasks.Add(task);
        };

        _context.OnRowSelect = (item) =>
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
            _pendingTasks.Add(task);
        };

        _context.OnSelectionChange = (selectedItems) =>
        {
            var task = InvokeAsync(async () =>
            {
                try
                {
                    if (OnSelectionChange.HasDelegate)
                    {
                        await OnSelectionChange.InvokeAsync(selectedItems);
                    }

                    await NotifyStateChanged();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error in OnSelectionChange: {ex.Message}");
                }
            });
            _pendingTasks.Add(task);
        };

        _context.OnStateChanged += HandleContextStateChanged;
    }

    /// <summary>
    /// Updates the table when parameters change, synchronizing state and reprocessing data.
    /// Handles both controlled (external state) and uncontrolled (internal state) modes.
    /// </summary>
    protected override void OnParametersSet()
    {
        // Update context with new parameters
        _context.SelectionMode = SelectionMode;
        _context.EnableKeyboardNavigation = EnableKeyboardNavigation;

        // Sync SelectionMode to the actual TableState
        SyncSelectionMode();

        // If controlled mode and state changed, update context
        if (IsControlled && _context.State != State)
        {
            _context.State = State!;
            _stateVersion++;
        }

        // Reprocess data when parameters change
        ProcessData();
    }

    /// <summary>
    /// Processes the data through pagination pipeline.
    /// Note: Sorting is handled by the developer - provide pre-sorted data to the Data parameter.
    /// </summary>
    private void ProcessData()
    {
        var currentState = EffectiveState;
        var data = Data ?? Array.Empty<TData>();

        // Update total items for pagination (unless parent handles it manually)
        if (!ManualPagination)
        {
            currentState.Pagination.TotalItems = data.Count();
        }

        // Apply pagination (skip if parent already paginated the data)
        _processedData = ManualPagination
            ? data.ToArray()
            : data.ApplyPagination(currentState.Pagination) ?? Array.Empty<TData>();

        // Update context with processed data
        _context.ProcessedData = _processedData;
    }

    /// <summary>
    /// Handles state changes from the context.
    /// </summary>
    private void HandleContextStateChanged()
    {
        _stateVersion++;
        ProcessData();

        // Notify parent if in controlled mode
        if (IsControlled && StateChanged.HasDelegate)
        {
            _ = StateChanged.InvokeAsync(State!);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Notifies that state has changed and triggers re-processing.
    /// </summary>
    private async Task NotifyStateChanged()
    {
        _stateVersion++;
        ProcessData();

        // Notify parent if in controlled mode
        if (IsControlled && StateChanged.HasDelegate)
        {
            await StateChanged.InvokeAsync(State!);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Determines whether the component should re-render based on tracked state changes.
    /// For controlled mode (with @bind-State), always returns true since state may be modified in-place.
    /// For uncontrolled mode, tracks parameter and version changes for optimization.
    /// </summary>
    /// <returns>True if the component should re-render.</returns>
    protected override bool ShouldRender()
    {
        // For controlled mode, always allow re-render since state may be modified in-place
        if (IsControlled)
        {
            return true;
        }

        // For uncontrolled mode, check if anything relevant has changed
        var dataChanged = !ReferenceEquals(_lastData, Data);
        var selectionModeChanged = _lastSelectionMode != SelectionMode;
        var paginationChanged = _lastManualPagination != ManualPagination;
        var versionChanged = _lastRenderVersion != _stateVersion;

        if (dataChanged || selectionModeChanged || paginationChanged || versionChanged)
        {
            _lastData = Data;
            _lastSelectionMode = SelectionMode;
            _lastManualPagination = ManualPagination;
            _lastRenderVersion = _stateVersion;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the ARIA row count for the table.
    /// </summary>
    private int GetAriaRowCount() => EffectiveState.Pagination.TotalItems + 1; // +1 for header row

    /// <summary>
    /// Disposes the component, cleaning up event subscriptions and waiting for pending async operations.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _context.OnStateChanged -= HandleContextStateChanged;

        // Wait for pending tasks with timeout to prevent memory leaks
        // Note: Task.WaitAll with timeout is not supported in browser/WASM context
        if (_pendingTasks.Count > 0)
        {
            try
            {
                if (!OperatingSystem.IsBrowser())
                {
                    Task.WaitAll(_pendingTasks.ToArray(), TimeSpan.FromSeconds(1));
                }
            }
            catch (AggregateException)
            {
                // Tasks were cancelled or timed out - this is expected during disposal
            }
            finally
            {
                _pendingTasks.Clear();
            }
        }
    }
}
