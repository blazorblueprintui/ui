using BlazorBlueprint.Components.Utilities;
using BlazorBlueprint.Primitives.Table;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.DataTable;

/// <summary>
/// A styled data table component that wraps the Table Primitive with automatic sorting,
/// filtering, pagination, and row selection capabilities.
/// </summary>
/// <typeparam name="TData">The type of data items in the table.</typeparam>
/// <remarks>
/// <para>
/// DataTable provides a complete table solution with declarative column definitions,
/// automatic data processing, and shadcn styling. It handles common table features
/// out-of-the-box while maintaining flexibility through templates and callbacks.
/// </para>
/// <para>
/// Features:
/// - Declarative column API via DataTableColumn child components
/// - Automatic sorting, filtering, and pagination (hybrid mode with overrides)
/// - Row selection (single/multiple) with checkboxes
/// - Optional toolbar with global search and column visibility toggle
/// - Empty and loading state templates
/// - Full shadcn styling with hover states and transitions
/// - Accessibility support (ARIA attributes, keyboard navigation)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;DataTable TData="Person" Data="@people" SelectionMode="DataTableSelectionMode.Multiple"&gt;
///     &lt;Columns&gt;
///         &lt;DataTableColumn Property="@(p => p.Name)" Header="Name" Sortable Filterable /&gt;
///         &lt;DataTableColumn Property="@(p => p.Age)" Header="Age" Sortable /&gt;
///     &lt;/Columns&gt;
/// &lt;/DataTable&gt;
/// </code>
/// </example>
public partial class DataTable<TData> : ComponentBase where TData : class
{
    /// <summary>
    /// Public class for storing column data without component parameters.
    /// This avoids BL0005 warnings when creating column instances programmatically.
    /// </summary>
    public class ColumnData
    {
        public string Id { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public Func<TData, object> Property { get; set; } = null!;
        public bool Sortable { get; set; }
        public bool Filterable { get; set; }
        public bool Visible { get; set; } = true;
        public string? Width { get; set; }
        public string? MinWidth { get; set; }
        public string? MaxWidth { get; set; }
        public RenderFragment<TData>? CellTemplate { get; set; }
        public string? CellClass { get; set; }
        public string? HeaderClass { get; set; }
    }

    private List<ColumnData> _columns = new();
    private TableState<TData> _tableState = new();
    private IEnumerable<TData> _processedData = Array.Empty<TData>();
    private IEnumerable<TData> _filteredData = Array.Empty<TData>();
    private string _globalSearchValue = string.Empty;
    private int _columnsVersion;
    private bool _selectAllDropdownOpen;

    // ShouldRender tracking fields
    private IEnumerable<TData>? _lastData;
    private DataTableSelectionMode _lastSelectionMode;
    private bool _lastIsLoading;
    private int _lastColumnsVersion;
    private string _lastGlobalSearchValue = string.Empty;
    private int _selectionVersion;
    private int _lastSelectionVersion;
    private IReadOnlyCollection<TData>? _lastSelectedItems;
    private int _paginationVersion;
    private int _lastPaginationVersion;

    /// <summary>
    /// Gets or sets the data source for the table.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TData> Data { get; set; } = Array.Empty<TData>();

    /// <summary>
    /// Gets or sets the column definitions as child content.
    /// Use DataTableColumn components to define columns declaratively.
    /// </summary>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <summary>
    /// Gets or sets the row selection mode.
    /// Default is None (no selection).
    /// </summary>
    [Parameter]
    public DataTableSelectionMode SelectionMode { get; set; } = DataTableSelectionMode.None;

    /// <summary>
    /// Gets or sets whether to show the toolbar with global search and column visibility.
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
    /// Gets or sets whether the table is in a loading state.
    /// Default is false.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets whether keyboard navigation is enabled for table rows.
    /// When true, rows can be navigated with arrow keys and selected with Enter/Space.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool EnableKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// Gets or sets the available page size options.
    /// Default is [5, 10, 20, 50, 100].
    /// </summary>
    [Parameter]
    public int[] PageSizes { get; set; } = { 5, 10, 20, 50, 100 };

    /// <summary>
    /// Gets or sets the initial page size.
    /// Default is 5.
    /// </summary>
    [Parameter]
    public int InitialPageSize { get; set; } = 5;

    /// <summary>
    /// Gets or sets custom toolbar actions (buttons, etc.).
    /// </summary>
    [Parameter]
    public RenderFragment? ToolbarActions { get; set; }

    /// <summary>
    /// Gets or sets a custom template for the empty state.
    /// If null, displays default "No results found" message.
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
    /// Gets or sets additional CSS classes for the container div.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the table.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the selected items.
    /// Use @bind-SelectedItems for two-way binding.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<TData> SelectedItems { get; set; } = Array.Empty<TData>();

    /// <summary>
    /// Event callback invoked when the selected items change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<TData>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Event callback invoked when sorting changes.
    /// Use for custom sorting logic (hybrid mode).
    /// </summary>
    [Parameter]
    public EventCallback<(string ColumnId, SortDirection Direction)> OnSort { get; set; }

    /// <summary>
    /// Event callback invoked when the global search value changes.
    /// Use for custom filtering logic (hybrid mode).
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnFilter { get; set; }

    /// <summary>
    /// Gets or sets a function to preprocess data before automatic processing.
    /// Use for custom transformations or server-side data fetching.
    /// </summary>
    [Parameter]
    public Func<IEnumerable<TData>, Task<IEnumerable<TData>>>? PreprocessData { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the container.
    /// </summary>
    private string ContainerCssClass => ClassNames.cn(
        "w-full space-y-4",
        Class
    );

    /// <summary>
    /// Gets the computed CSS classes for the table container.
    /// </summary>
    private static string TableContainerCssClass => ClassNames.cn(
        "rounded-md border"
    );

    /// <summary>
    /// Gets the computed CSS classes for the table element.
    /// </summary>
    private static string TableCssClass => ClassNames.cn(
        "w-full caption-bottom text-sm"
    );

    protected override void OnInitialized()
    {
        _tableState.Pagination.PageSize = InitialPageSize;
        _tableState.Pagination.CurrentPage = 1;
        // Set selection mode on the state so Select/Deselect methods work correctly
        _tableState.Selection.Mode = GetPrimitiveSelectionMode();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Keep selection mode in sync with parameter
        _tableState.Selection.Mode = GetPrimitiveSelectionMode();

        // Sync SelectedItems parameter to internal state if changed externally
        // Skip if SelectedItems is the same reference as our internal collection (shouldn't happen with the copy we make, but defensive)
        if (!ReferenceEquals(SelectedItems, _lastSelectedItems) &&
            !ReferenceEquals(SelectedItems, _tableState.Selection.SelectedItems))
        {
            _tableState.Selection.Clear();
            foreach (var item in SelectedItems)
            {
                _tableState.Selection.Select(item);
            }
            _lastSelectedItems = SelectedItems;
            _selectionVersion++;
        }

        await ProcessDataAsync();
    }

    /// <summary>
    /// Registers a column with the data table.
    /// Called by DataTableColumn during initialization.
    /// </summary>
    internal void RegisterColumn<TValue>(DataTableColumn<TData, TValue> column) where TValue : notnull
    {
        // Create internal column data structure (avoids BL0005 component parameter warnings)
        var columnData = new ColumnData
        {
            Id = column.Id ?? column.Header.ToLowerInvariant().Replace(" ", "-"),
            Header = column.Header,
            Property = item =>
            {
                var value = column.Property(item);
                return value ?? throw new InvalidOperationException($"Column '{column.Header}' returned null for a non-nullable type.");
            },
            Sortable = column.Sortable,
            Filterable = column.Filterable,
            Visible = column.Visible,
            Width = column.Width,
            MinWidth = column.MinWidth,
            MaxWidth = column.MaxWidth,
            CellTemplate = column.CellTemplate,
            CellClass = column.CellClass,
            HeaderClass = column.HeaderClass
        };

        _columns.Add(columnData);
    }

    /// <summary>
    /// Processes the data through filtering, sorting, and pagination pipelines.
    /// </summary>
    private async Task ProcessDataAsync()
    {
        var data = Data ?? Array.Empty<TData>();

        // 1. Preprocess (if custom function provided)
        if (PreprocessData != null)
        {
            data = await PreprocessData(data);
        }

        // 2. Apply filtering (column filters + global search)
        _filteredData = ApplyFiltering(data);

        // 3. Apply sorting
        var sortedData = ApplySorting(_filteredData);

        // 4. Update pagination total items BEFORE pagination
        _tableState.Pagination.TotalItems = sortedData.Count();

        // 5. Apply pagination
        _processedData = sortedData
            .Skip(_tableState.Pagination.StartIndex)
            .Take(_tableState.Pagination.PageSize)
            .ToList();
    }

    /// <summary>
    /// Applies global search filtering to the data.
    /// </summary>
    private IEnumerable<TData> ApplyFiltering(IEnumerable<TData> data)
    {
        if (string.IsNullOrWhiteSpace(_globalSearchValue))
        {
            return data;
        }

        // Cache search value to avoid repeated property access in closure
        var searchValue = _globalSearchValue;

        // Pre-filter to only filterable columns to reduce iterations
        var filterableColumns = _columns.Where(c => c.Filterable).ToList();
        if (filterableColumns.Count == 0)
        {
            filterableColumns = _columns; // Fall back to all columns if none marked filterable
        }

        return data.Where(item => MatchesSearch(item, searchValue, filterableColumns));
    }

    /// <summary>
    /// Checks if an item matches the search criteria across the specified columns.
    /// Extracted method to reduce closure overhead and improve JIT optimization.
    /// </summary>
    private static bool MatchesSearch(TData item, string searchValue, List<ColumnData> columns)
    {
        foreach (var column in columns)
        {
            try
            {
                var value = column.Property(item);
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
                // Skip columns that cause errors during property access
            }
        }
        return false;
    }

    /// <summary>
    /// Applies sorting to the data based on current sort state.
    /// </summary>
    private IEnumerable<TData> ApplySorting(IEnumerable<TData> data)
    {
        if (_tableState.Sorting.Direction == SortDirection.None)
        {
            return data;
        }

        var column = _columns.FirstOrDefault(c => c.Id == _tableState.Sorting.SortedColumn);
        if (column == null)
        {
            return data;
        }

        var sorted = _tableState.Sorting.Direction == SortDirection.Ascending
            ? data.OrderBy(item => column.Property(item))
            : data.OrderByDescending(item => column.Property(item));

        return sorted;
    }

    /// <summary>
    /// Handles sort change events from the table primitive.
    /// </summary>
    private async Task HandleSortChange((string ColumnId, SortDirection Direction) sortInfo)
    {
        // Invoke custom callback if provided
        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync(sortInfo);
        }

        // Automatic sorting will happen in ProcessDataAsync via state binding
        await ProcessDataAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Handles global search value changes.
    /// </summary>
    private async Task HandleGlobalSearchChanged(string value)
    {
        _globalSearchValue = value;

        // Invoke custom callback if provided
        if (OnFilter.HasDelegate)
        {
            await OnFilter.InvokeAsync(_globalSearchValue);
        }

        // Reset to first page when filtering
        _tableState.Pagination.CurrentPage = 1;

        await ProcessDataAsync();
        // StateHasChanged() not needed - Blazor auto-renders after async event handlers
    }

    /// <summary>
    /// Handles column visibility changes.
    /// </summary>
    private void HandleColumnVisibilityChanged(string columnId, bool visible)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column != null)
        {
            column.Visible = visible;

            // Increment version to signal change without list recreation
            _columnsVersion++;

            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles selection change events from the table primitive.
    /// </summary>
    private async Task HandleSelectionChange(IReadOnlyCollection<TData> selectedItems)
    {
        if (SelectedItemsChanged.HasDelegate)
        {
            // Pass a copy to avoid reference aliasing - if parent stores the reference
            // and we later call Clear(), it would clear the parent's collection too
            await SelectedItemsChanged.InvokeAsync(selectedItems.ToList().AsReadOnly());
        }
    }

    /// <summary>
    /// Determines whether to show the select-all dropdown prompt.
    /// Returns true when total items exceed the current page count.
    /// </summary>
    private bool ShouldShowSelectAllPrompt() =>
        _tableState.Pagination.TotalItems > _processedData.Count();

    /// <summary>
    /// Gets the total count of filtered items across all pages.
    /// </summary>
    private int GetTotalFilteredItemCount() =>
        _filteredData.Count();

    /// <summary>
    /// Opens the select-all dropdown menu.
    /// </summary>
    private void OpenSelectAllDropdown()
    {
        _selectAllDropdownOpen = true;
        StateHasChanged();
    }

    /// <summary>
    /// Handles select all checkbox changes.
    /// When multiple pages exist, opens a dropdown for user to choose scope.
    /// </summary>
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

    /// <summary>
    /// Selects all items on the current page only.
    /// </summary>
    private async Task HandleSelectAllOnCurrentPage()
    {
        foreach (var item in _processedData)
        {
            _tableState.Selection.Select(item);
        }
        _selectAllDropdownOpen = false;
        _selectionVersion++;
        await HandleSelectionChange(_tableState.Selection.SelectedItems);
        StateHasChanged();
    }

    /// <summary>
    /// Selects all items across all pages (entire filtered dataset).
    /// </summary>
    private async Task HandleSelectAllItems()
    {
        foreach (var item in _filteredData)
        {
            _tableState.Selection.Select(item);
        }
        _selectAllDropdownOpen = false;
        _selectionVersion++;
        await HandleSelectionChange(_tableState.Selection.SelectedItems);
        StateHasChanged();
    }

    /// <summary>
    /// Clears all selected items.
    /// </summary>
    private async Task HandleClearSelection()
    {
        _tableState.Selection.Clear();
        _selectAllDropdownOpen = false;
        _selectionVersion++;
        await HandleSelectionChange(_tableState.Selection.SelectedItems);
        StateHasChanged();
    }

    /// <summary>
    /// Handles individual row selection changes.
    /// </summary>
    private async Task HandleRowSelectionChanged(TData item, bool isChecked)
    {
        if (isChecked)
        {
            _tableState.Selection.Select(item);
        }
        else
        {
            _tableState.Selection.Deselect(item);
        }

        _selectionVersion++;  // Track selection change for ShouldRender
        await HandleSelectionChange(_tableState.Selection.SelectedItems);
        StateHasChanged();
    }

    /// <summary>
    /// Checks if all rows on the current page are selected.
    /// </summary>
    private bool IsAllSelected()
    {
        if (!_processedData.Any())
        {
            return false;
        }

        return _processedData.All(item => _tableState.Selection.IsSelected(item));
    }

    /// <summary>
    /// Checks if some (but not all) rows on the current page are selected.
    /// Used for the indeterminate state of the select-all checkbox.
    /// </summary>
    private bool IsSomeSelected()
    {
        if (!_processedData.Any())
        {
            return false;
        }

        var selectedCount = _processedData.Count(item => _tableState.Selection.IsSelected(item));
        return selectedCount > 0 && selectedCount < _processedData.Count();
    }

    /// <summary>
    /// Gets the column width style attribute.
    /// </summary>
    private static string? GetColumnWidthStyle(ColumnData column)
    {
        var styles = new List<string>();

        if (!string.IsNullOrWhiteSpace(column.Width))
        {
            styles.Add($"width: {column.Width}");
        }

        if (!string.IsNullOrWhiteSpace(column.MinWidth))
        {
            styles.Add($"min-width: {column.MinWidth}");
        }

        if (!string.IsNullOrWhiteSpace(column.MaxWidth))
        {
            styles.Add($"max-width: {column.MaxWidth}");
        }

        return styles.Count > 0 ? string.Join("; ", styles) : null;
    }

    /// <summary>
    /// Converts DataTableSelectionMode to primitive SelectionMode.
    /// </summary>
    private SelectionMode GetPrimitiveSelectionMode()
    {
        return SelectionMode switch
        {
            DataTableSelectionMode.None => Primitives.Table.SelectionMode.None,
            DataTableSelectionMode.Single => Primitives.Table.SelectionMode.Single,
            DataTableSelectionMode.Multiple => Primitives.Table.SelectionMode.Multiple,
            _ => Primitives.Table.SelectionMode.None
        };
    }

    /// <summary>
    /// Handles page change events from pagination component.
    /// </summary>
    private async Task HandlePageChanged(int newPage)
    {
        _paginationVersion++;  // Track pagination change for ShouldRender
        await ProcessDataAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Handles page size change events from pagination component.
    /// </summary>
    private async Task HandlePageSizeChanged(int newPageSize)
    {
        _paginationVersion++;  // Track pagination change for ShouldRender
        await ProcessDataAsync();
        StateHasChanged();
    }

    /// <summary>
    /// Determines whether the component should re-render based on tracked state changes.
    /// This optimization reduces unnecessary render cycles for complex tables.
    /// </summary>
    protected override bool ShouldRender()
    {
        var dataChanged = !ReferenceEquals(_lastData, Data);
        var selectionModeChanged = _lastSelectionMode != SelectionMode;
        var loadingChanged = _lastIsLoading != IsLoading;
        var columnsChanged = _lastColumnsVersion != _columnsVersion;
        var searchChanged = _lastGlobalSearchValue != _globalSearchValue;
        var selectionChanged = _lastSelectionVersion != _selectionVersion;
        var paginationChanged = _lastPaginationVersion != _paginationVersion;

        if (dataChanged || selectionModeChanged || loadingChanged || columnsChanged || searchChanged || selectionChanged || paginationChanged)
        {
            _lastData = Data;
            _lastSelectionMode = SelectionMode;
            _lastIsLoading = IsLoading;
            _lastColumnsVersion = _columnsVersion;
            _lastGlobalSearchValue = _globalSearchValue;
            _lastSelectionVersion = _selectionVersion;
            _lastPaginationVersion = _paginationVersion;
            return true;
        }

        return false;
    }
}
