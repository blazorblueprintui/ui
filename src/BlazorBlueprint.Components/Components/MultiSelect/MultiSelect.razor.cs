using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components.MultiSelect;

/// <summary>
/// A multi-select component that allows users to select multiple options from a searchable dropdown.
/// </summary>
/// <typeparam name="TItem">The type of items in the multiselect list.</typeparam>
public partial class MultiSelect<TItem> : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;
    private IJSObjectReference? _multiSelectModule;
    private DotNetObjectReference<MultiSelect<TItem>>? _dotNetRef;
    private ElementReference _searchInputRef;
    private bool _jsSetupDone;
    private bool _focusDone;

    // ShouldRender tracking fields
    private IEnumerable<TItem>? _lastItems;
    private IEnumerable<string>? _lastValues;
    private bool _lastIsOpen;
    private string _lastSearchQuery = string.Empty;
    private bool _lastDisabled;

    // Cached event handlers to avoid allocations on every render
    private readonly Dictionary<string, Func<Task>> _toggleHandlerCache = new();
    private readonly Dictionary<string, Func<Task>> _removeHandlerCache = new();

    // Cached CSS class strings to avoid recomputation on every render
    // TriggerCssClass caching fields removed - ClassNames.cn handles this

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the collection of items to display in the multiselect.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();

    /// <summary>
    /// Gets or sets the currently selected values.
    /// </summary>
    [Parameter]
    public IEnumerable<string>? Values { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<string>?> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets the function to extract the value from an item.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> ValueSelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the function to extract the display text from an item.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> DisplaySelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the placeholder text shown when no items are selected.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select items...";

    /// <summary>
    /// Gets or sets the placeholder text shown in the search input.
    /// </summary>
    [Parameter]
    public string SearchPlaceholder { get; set; } = "Search...";

    /// <summary>
    /// Gets or sets the message displayed when no items match the search.
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No results found.";

    /// <summary>
    /// Gets or sets the label for the Select All option.
    /// </summary>
    [Parameter]
    public string SelectAllLabel { get; set; } = "Select All";

    /// <summary>
    /// Gets or sets whether to show the Select All option.
    /// </summary>
    [Parameter]
    public bool ShowSelectAll { get; set; } = true;

    /// <summary>
    /// Gets or sets the label for the Clear button.
    /// </summary>
    [Parameter]
    public string ClearLabel { get; set; } = "Clear";

    /// <summary>
    /// Gets or sets the label for the Close button.
    /// </summary>
    [Parameter]
    public string CloseLabel { get; set; } = "Close";

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the multiselect container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether the multiselect is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tags to display before showing "+N more".
    /// </summary>
    [Parameter]
    public int MaxDisplayTags { get; set; } = 3;

    /// <summary>
    /// Gets or sets the width of the popover content.
    /// Ignored when MatchTriggerWidth is true.
    /// </summary>
    [Parameter]
    public string PopoverWidth { get; set; } = "w-[300px]";

    /// <summary>
    /// Gets or sets whether to match the dropdown width to the trigger element width.
    /// When true, PopoverWidth is ignored.
    /// </summary>
    [Parameter]
    public bool MatchTriggerWidth { get; set; } = false;

    /// <summary>
    /// Gets or sets whether clicking outside the dropdown should close it.
    /// When true, the dropdown will close and the search query will be cleared.
    /// Default is true.
    /// </summary>
    [Parameter]
    public bool AutoClose { get; set; } = true;

    /// <summary>
    /// Gets or sets an expression that identifies the bound values.
    /// Used for form validation integration.
    /// </summary>
    [Parameter]
    public Expression<Func<IEnumerable<string>?>>? ValuesExpression { get; set; }

    /// <summary>
    /// Tracks whether the popover is currently open.
    /// </summary>
    private bool _isOpen { get; set; }

    /// <summary>
    /// Tracks the current search query for filtering.
    /// </summary>
    private string _searchQuery = string.Empty;

    /// <summary>
    /// Gets a unique identifier for this multiselect instance.
    /// </summary>
    private string Id { get; set; } = $"multiselect-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the list of currently selected values.
    /// </summary>
    private List<string> SelectedValues => Values?.ToList() ?? new List<string>();

    /// <summary>
    /// Gets the number of selected items that exceed MaxDisplayTags.
    /// </summary>
    private int OverflowCount => Math.Max(0, SelectedValues.Count - MaxDisplayTags);

    /// <summary>
    /// Gets whether any items are selected.
    /// </summary>
    private bool HasSelectedItems => SelectedValues.Count > 0;

    /// <summary>
    /// Gets the filtered items based on current search query.
    /// </summary>
    private IEnumerable<TItem> FilteredItems => GetFilteredItems();

    /// <summary>
    /// Gets whether there are any items visible after filtering.
    /// </summary>
    private bool HasFilteredItems => FilteredItems.Any();

    /// <summary>
    /// Gets the items that match the current search query.
    /// </summary>
    private IEnumerable<TItem> GetFilteredItems()
    {
        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return Items;
        }

        return Items.Where(item =>
            DisplaySelector(item).Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets whether the multiselect is in an invalid state (for validation).
    /// </summary>
    private bool IsInvalid
    {
        get
        {
            if (_editContext != null && ValuesExpression != null && _fieldIdentifier.FieldName != null)
            {
                return _editContext.GetValidationMessages(_fieldIdentifier).Any();
            }
            return false;
        }
    }

    /// <summary>
    /// Validates required parameters.
    /// </summary>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ValueSelector == null)
        {
            throw new InvalidOperationException(
                $"{nameof(MultiSelect<TItem>)}: {nameof(ValueSelector)} parameter is required.");
        }

        if (DisplaySelector == null)
        {
            throw new InvalidOperationException(
                $"{nameof(MultiSelect<TItem>)}: {nameof(DisplaySelector)} parameter is required.");
        }

        // Filter out null items for safety
        Items = Items?.Where(item => item != null) ?? Enumerable.Empty<TItem>();

        // Clear handler caches when Items reference changes to avoid stale delegates
        if (!ReferenceEquals(_lastItems, Items))
        {
            _toggleHandlerCache.Clear();
        }

        // Initialize EditContext integration if available
        if (CascadedEditContext != null && ValuesExpression != null)
        {
            _editContext = CascadedEditContext;
            _fieldIdentifier = FieldIdentifier.Create(ValuesExpression);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Setup JS when popover opens
        if (_isOpen && !_jsSetupDone)
        {
            _jsSetupDone = true;

            try
            {
                _multiSelectModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/multiselect.js");

                _dotNetRef = DotNetObjectReference.Create(this);

                await _multiSelectModule.InvokeVoidAsync(
                    "setupMultiSelectInput",
                    _searchInputRef,
                    _dotNetRef,
                    $"{Id}-search",
                    $"{Id}-listbox");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MultiSelect JS setup failed: {ex.Message}");
            }
        }
        // Cleanup JS when popover closes
        else if (!_isOpen && _jsSetupDone)
        {
            await CleanupJsAsync();
        }
    }

    private async Task CleanupJsAsync()
    {
        if (_multiSelectModule != null)
        {
            try
            {
                await _multiSelectModule.InvokeVoidAsync("removeMultiSelectInput", $"{Id}-search");
            }
            catch
            {
                // Module may already be disposed
            }
        }
        _jsSetupDone = false;
        _focusDone = false;
    }

    /// <summary>
    /// Handles the open state change of the popover.
    /// Resets focus tracking when the popover closes.
    /// </summary>
    /// <param name="isOpen">Whether the popover is now open.</param>
    private void HandleOpenChanged(bool isOpen)
    {
        _isOpen = isOpen;
        if (!isOpen)
        {
            _focusDone = false; // Reset for next open
        }
    }

    /// <summary>
    /// Opens the dropdown.
    /// </summary>
    private void Open()
    {
        if (Disabled)
        {
            return;
        }

        _isOpen = true;
    }

    /// <summary>
    /// Closes the dropdown.
    /// </summary>
    private void Close()
    {
        _isOpen = false;
        _searchQuery = string.Empty;
    }

    /// <summary>
    /// Gets the click-outside event handler based on AutoClose setting.
    /// Returns an empty EventCallback when AutoClose is false.
    /// </summary>
    private EventCallback GetClickOutsideHandler()
    {
        return AutoClose
            ? EventCallback.Factory.Create(this, HandleClickOutside)
            : default;
    }

    /// <summary>
    /// Handles click-outside events when AutoClose is enabled.
    /// </summary>
    private void HandleClickOutside() => Close();

    /// <summary>
    /// Handles the popover content ready event to focus the search input.
    /// This is called when the popover is fully positioned and visible.
    /// </summary>
    private async Task HandleContentReady()
    {
        // Guard against multiple calls per open
        if (_focusDone)
        {
            return;
        }

        _focusDone = true;

        try
        {
            // Small delay to let browser finish processing DOM changes
            await Task.Delay(50);
            await _searchInputRef.FocusAsync();
        }
        catch
        {
            // Ignore focus errors
        }
    }

    /// <summary>
    /// Handles search input changes.
    /// </summary>
    private void HandleSearchInput(ChangeEventArgs args) =>
        _searchQuery = args.Value?.ToString() ?? string.Empty;

    /// <summary>
    /// Handles keyboard events on the search input.
    /// </summary>
    private void HandleSearchKeyDown(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Enter":
                HandleEnter();
                break;
            case "Escape":
                HandleEscape();
                break;
        }
    }

    /// <summary>
    /// Handles item toggle (selection/deselection).
    /// </summary>
    private async Task HandleToggle(TItem item)
    {
        var itemValue = ValueSelector(item);
        var currentValues = SelectedValues;

        if (!currentValues.Remove(itemValue))

        {
            currentValues.Add(itemValue);
        }

        await UpdateValues(currentValues);
    }

    /// <summary>
    /// Gets a cached toggle handler for the specified item to avoid delegate allocations.
    /// </summary>
    private Func<Task> GetToggleHandler(TItem item)
    {
        var key = ValueSelector(item);
        if (!_toggleHandlerCache.TryGetValue(key, out var handler))
        {
            handler = () => HandleToggle(item);
            _toggleHandlerCache[key] = handler;
        }
        return handler;
    }

    /// <summary>
    /// Gets a cached remove handler for the specified value to avoid delegate allocations.
    /// </summary>
    private Func<Task> GetRemoveHandler(string value)
    {
        if (!_removeHandlerCache.TryGetValue(value, out var handler))
        {
            handler = () => RemoveValue(value);
            _removeHandlerCache[value] = handler;
        }
        return handler;
    }

    /// <summary>
    /// Handles Select All toggle. Only affects filtered/visible items.
    /// </summary>
    private async Task HandleSelectAllToggle()
    {
        var filteredValues = FilteredItems.Select(ValueSelector).ToList();
        var currentValues = SelectedValues;

        // Check if all FILTERED items are selected
        var allFilteredSelected = filteredValues.All(v => currentValues.Contains(v));

        if (allFilteredSelected)
        {
            // Deselect only the filtered items
            currentValues.RemoveAll(v => filteredValues.Contains(v));
        }
        else
        {
            // Select all filtered items (add to existing selection)
            foreach (var value in filteredValues.Where(v => !currentValues.Contains(v)))
            {
                currentValues.Add(value);
            }
        }

        await UpdateValues(currentValues);
    }

    /// <summary>
    /// Removes a value from the selection.
    /// </summary>
    private async Task RemoveValue(string value)
    {
        var currentValues = SelectedValues;
        currentValues.Remove(value);
        await UpdateValues(currentValues);
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    private async Task ClearAll() =>
        await UpdateValues(new List<string>());

    /// <summary>
    /// Updates the selected values and notifies parent.
    /// </summary>
    private async Task UpdateValues(List<string> values)
    {
        Values = values.Count > 0 ? values : null;
        await ValuesChanged.InvokeAsync(Values);

        // Notify EditContext of field change for validation
        if (_editContext != null && ValuesExpression != null && _fieldIdentifier.FieldName != null)
        {
            _editContext.NotifyFieldChanged(_fieldIdentifier);
        }
    }

    /// <summary>
    /// Checks if an item is currently selected.
    /// </summary>
    private bool IsSelected(TItem item) =>
        SelectedValues.Contains(ValueSelector(item));

    /// <summary>
    /// Gets the Select All state based on filtered items.
    /// </summary>
    private SelectAllState GetSelectAllState()
    {
        var filteredValues = FilteredItems.Select(ValueSelector).ToHashSet();
        var selectedFilteredCount = SelectedValues.Count(v => filteredValues.Contains(v));

        if (selectedFilteredCount == 0)
        {
            return SelectAllState.None;
        }

        if (selectedFilteredCount == filteredValues.Count)
        {
            return SelectAllState.All;
        }

        return SelectAllState.Indeterminate;
    }

    /// <summary>
    /// Gets the display text for a value.
    /// </summary>
    private string GetDisplayText(string value)
    {
        var item = Items.FirstOrDefault(i => ValueSelector(i) == value);
        return item != null ? DisplaySelector(item) : value;
    }

    // JSInvokable callbacks for keyboard navigation
    [JSInvokable]
    public void HandleSpace()
    {
        // Space toggles checkbox - item click already handled by JS
        // No additional action needed, dropdown stays open
    }

    [JSInvokable]
    public void HandleEnter()
    {
        // Enter closes the dropdown after item toggle
        Close();
        StateHasChanged();
    }

    [JSInvokable]
    public void HandleEscape()
    {
        // Escape closes the dropdown
        Close();
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await CleanupJsAsync();

        if (_multiSelectModule != null)
        {
            await _multiSelectModule.DisposeAsync();
            _multiSelectModule = null;
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    /// <summary>
    /// Determines whether the component should re-render based on tracked state changes.
    /// This optimization reduces unnecessary render cycles.
    /// </summary>
    protected override bool ShouldRender()
    {
        var itemsChanged = !ReferenceEquals(_lastItems, Items);
        var valuesChanged = !ReferenceEquals(_lastValues, Values);
        var openChanged = _lastIsOpen != _isOpen;
        var searchChanged = _lastSearchQuery != _searchQuery;
        var disabledChanged = _lastDisabled != Disabled;

        if (itemsChanged || valuesChanged || openChanged || searchChanged || disabledChanged)
        {
            _lastItems = Items;
            _lastValues = Values;
            _lastIsOpen = _isOpen;
            _lastSearchQuery = _searchQuery;
            _lastDisabled = Disabled;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the CSS class for the multiselect container.
    /// </summary>
    private static string ContainerClass => "relative";

    /// <summary>
    /// Gets the CSS class for the trigger button.
    /// Uses caching to avoid recomputation on every render.
    /// </summary>
    private string TriggerCssClass => ClassNames.cn(
        "inline-flex items-center justify-between rounded-md text-sm font-medium",
        "transition-colors focus-visible:outline-none focus-visible:ring-2",
        "focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:opacity-50 disabled:pointer-events-none",
        "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
        "min-h-9 px-3 py-1.5",
        PopoverWidth,
        Class
    );

    /// <summary>
    /// Gets the CSS class for the tag remove button.
    /// </summary>
    private static string TagRemoveButtonCssClass =>
        "ml-0.5 rounded-full outline-none hover:bg-secondary-foreground/20 focus:ring-1 focus:ring-ring";

    /// <summary>
    /// Gets the CSS class for the dropdown item.
    /// </summary>
    private static string ItemCssClass =>
        "relative flex cursor-pointer select-none items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none " +
        "data-[focused=true]:bg-accent data-[focused=true]:text-accent-foreground " +
        "data-[disabled=true]:pointer-events-none data-[disabled=true]:opacity-50";

    /// <summary>
    /// Gets the CSS class for the checkbox.
    /// Uses data-state attribute from Checkbox primitive for checked/unchecked/indeterminate styling.
    /// </summary>
    private static string CheckboxCssClass =>
        "h-4 w-4 shrink-0 rounded-sm border border-primary ring-offset-background flex items-center justify-center " +
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 " +
        "disabled:cursor-not-allowed disabled:opacity-50 " +
        "data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground " +
        "data-[state=indeterminate]:bg-primary data-[state=indeterminate]:text-primary-foreground " +
        "data-[state=unchecked]:bg-background";
}

/// <summary>
/// Represents the state of the Select All checkbox.
/// </summary>
public enum SelectAllState
{
    /// <summary>No items are selected.</summary>
    None,
    /// <summary>Some items are selected.</summary>
    Indeterminate,
    /// <summary>All items are selected.</summary>
    All
}
