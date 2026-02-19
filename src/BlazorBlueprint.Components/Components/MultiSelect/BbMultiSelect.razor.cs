using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorBlueprint.Components;

/// <summary>
/// A multi-select component that allows users to select multiple options from a searchable dropdown.
/// </summary>
/// <typeparam name="TValue">The type of the selected values.</typeparam>
public partial class BbMultiSelect<TValue> : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private FieldIdentifier _fieldIdentifier;
    private EditContext? _editContext;
    private IJSObjectReference? _multiSelectModule;
    private DotNetObjectReference<BbMultiSelect<TValue>>? _dotNetRef;
    private ElementReference _searchInputRef;
    private bool _jsSetupDone;
    private bool _focusDone;

    // ShouldRender tracking fields
    private IEnumerable<SelectOption<TValue>>? _lastOptions;
    private IEnumerable<TValue>? _lastValues;
    private bool _lastIsOpen;
    private string _lastSearchQuery = string.Empty;
    private bool _lastDisabled;

    // Cached event handlers to avoid allocations on every render
    // CS8714 suppressed: Option values used as keys are always non-null in practice
#pragma warning disable CS8714
    private readonly Dictionary<TValue, Func<Task>> _toggleHandlerCache = new(EqualityComparer<TValue>.Default);
    private readonly Dictionary<TValue, Func<Task>> _removeHandlerCache = new(EqualityComparer<TValue>.Default);
#pragma warning restore CS8714

    // Cached CSS class strings to avoid recomputation on every render
    // TriggerCssClass caching fields removed - ClassNames.cn handles this

    /// <summary>
    /// Item text registry for compositional mode display text lookup and EffectiveOptions.
    /// </summary>
#pragma warning disable CS8714 // TValue may be null but dictionary keys are non-null in practice
    private readonly Dictionary<TValue, string> _itemTextRegistry = new(EqualityComparer<TValue>.Default);
#pragma warning restore CS8714

    /// <summary>
    /// Gets or sets the cascaded EditContext from a parent EditForm.
    /// </summary>
    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the collection of options to display in the multiselect.
    /// When provided, uses Options mode (data-driven). When null, uses Compositional mode (ChildContent).
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<TValue>>? Options { get; set; }

    /// <summary>
    /// Gets or sets the child content for compositional mode.
    /// Use MultiSelectItem components as children for rich item rendering.
    /// Only used when Options is null.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the currently selected values.
    /// </summary>
    [Parameter]
    public IEnumerable<TValue>? Values { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when the selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TValue>?> ValuesChanged { get; set; }

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
    /// Gets or sets the ID(s) of the element(s) that describe this multiselect for accessibility.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

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
    /// Gets or sets CSS classes applied to the trigger when the dropdown is open.
    /// Default is <c>"bg-accent text-accent-foreground"</c>.
    /// Set to <c>null</c> or empty to disable the active style.
    /// </summary>
    [Parameter]
    public string? ActiveClass { get; set; } = "bg-accent text-accent-foreground";

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
    public Expression<Func<IEnumerable<TValue>?>>? ValuesExpression { get; set; }

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
    private List<TValue> SelectedValues => Values?.ToList() ?? new List<TValue>();

    /// <summary>
    /// Gets the number of selected items that exceed MaxDisplayTags.
    /// </summary>
    private int OverflowCount => Math.Max(0, SelectedValues.Count - MaxDisplayTags);

    /// <summary>
    /// Gets whether any items are selected.
    /// </summary>
    private bool HasSelectedItems => SelectedValues.Count > 0;

    /// <summary>
    /// Gets whether the component is in compositional mode (ChildContent with MultiSelectItem children).
    /// </summary>
    private bool IsCompositionalMode => Options is null;

    /// <summary>
    /// Gets the effective options for the current mode.
    /// In Options mode, returns Options. In Compositional mode, returns registered items as SelectOptions.
    /// </summary>
    private IEnumerable<SelectOption<TValue>> EffectiveOptions =>
        Options ?? _itemTextRegistry.Select(kvp => new SelectOption<TValue>(kvp.Key, kvp.Value));

    /// <summary>
    /// Gets the filtered options based on current search query.
    /// </summary>
    private IEnumerable<SelectOption<TValue>> FilteredOptions => GetFilteredOptions();

    /// <summary>
    /// Gets whether there are any options visible after filtering.
    /// </summary>
    private bool HasFilteredItems => FilteredOptions.Any();

    /// <summary>
    /// Gets the options that match the current search query.
    /// </summary>
    private IEnumerable<SelectOption<TValue>> GetFilteredOptions()
    {
        var options = EffectiveOptions;

        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return options;
        }

        return options.Where(o =>
            o.Text.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));
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

        // Filter out null options for safety (Options mode only)
        Options = Options?.Where(o => o != null);

        // Clear handler caches when Options reference changes to avoid stale delegates
        if (!ReferenceEquals(_lastOptions, Options))
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
    /// Handles option toggle (selection/deselection).
    /// </summary>
    private async Task HandleToggle(SelectOption<TValue> option)
    {
        var currentValues = SelectedValues;

        if (!currentValues.Remove(option.Value))
        {
            currentValues.Add(option.Value);
        }

        await UpdateValues(currentValues);
    }

    /// <summary>
    /// Gets a cached toggle handler for the specified option to avoid delegate allocations.
    /// </summary>
    private Func<Task> GetToggleHandler(SelectOption<TValue> option)
    {
        var key = option.Value;
        if (!_toggleHandlerCache.TryGetValue(key, out var handler))
        {
            handler = () => HandleToggle(option);
            _toggleHandlerCache[key] = handler;
        }
        return handler;
    }

    /// <summary>
    /// Gets a cached remove handler for the specified value to avoid delegate allocations.
    /// </summary>
    private Func<Task> GetRemoveHandler(TValue value)
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
        var filteredValues = FilteredOptions.Select(o => o.Value).ToList();
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
    private async Task RemoveValue(TValue value)
    {
        var currentValues = SelectedValues;
        currentValues.Remove(value);
        await UpdateValues(currentValues);
    }

    /// <summary>
    /// Clears all selections.
    /// </summary>
    private async Task ClearAll() =>
        await UpdateValues(new List<TValue>());

    /// <summary>
    /// Updates the selected values and notifies parent.
    /// </summary>
    private async Task UpdateValues(List<TValue> values)
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
    /// Checks if an option is currently selected.
    /// </summary>
    private bool IsSelected(SelectOption<TValue> option) =>
        SelectedValues.Contains(option.Value);

    /// <summary>
    /// Gets the Select All state based on filtered options.
    /// </summary>
    private SelectAllState GetSelectAllState()
    {
        var filteredValues = new HashSet<TValue>(FilteredOptions.Select(o => o.Value), EqualityComparer<TValue>.Default);
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
    /// Checks Options first, then the item text registry (Compositional mode).
    /// </summary>
    private string GetDisplayText(TValue value)
    {
        var option = Options?.FirstOrDefault(o => EqualityComparer<TValue>.Default.Equals(o.Value, value));
        if (option is not null)
        {
            return option.Text;
        }

        if (value is not null && _itemTextRegistry.TryGetValue(value, out var registeredText))
        {
            return registeredText;
        }

        return value?.ToString() ?? "";
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
            try
            {
                await _multiSelectModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
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
        var optionsChanged = !ReferenceEquals(_lastOptions, Options);
        var valuesChanged = !ReferenceEquals(_lastValues, Values);
        var openChanged = _lastIsOpen != _isOpen;
        var searchChanged = _lastSearchQuery != _searchQuery;
        var disabledChanged = _lastDisabled != Disabled;

        if (optionsChanged || valuesChanged || openChanged || searchChanged || disabledChanged)
        {
            _lastOptions = Options;
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
        "transition-colors focus-visible:outline-none",
        "disabled:opacity-50 disabled:pointer-events-none",
        "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
        _isOpen ? ActiveClass : null,
        "min-h-9 px-3 py-1.5",
        PopoverWidth,
        Class
    );

    /// <summary>
    /// Gets the CSS class for the tag remove button.
    /// </summary>
    private static string TagRemoveButtonCssClass =>
        "ml-0.5 rounded-full outline-none hover:bg-secondary-foreground/20";

    /// <summary>
    /// Gets the CSS class for the dropdown item.
    /// </summary>
    internal static string ItemCssClass =>
        "relative flex cursor-pointer select-none items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none " +
        "data-[focused=true]:bg-accent data-[focused=true]:text-accent-foreground " +
        "data-[disabled=true]:pointer-events-none data-[disabled=true]:opacity-50";

    /// <summary>
    /// Gets the CSS class for the checkbox.
    /// Uses data-state attribute from Checkbox primitive for checked/unchecked/indeterminate styling.
    /// </summary>
    internal static string CheckboxCssClass =>
        "h-4 w-4 shrink-0 rounded-sm border border-primary flex items-center justify-center " +
        "focus-visible:outline-none " +
        "disabled:cursor-not-allowed disabled:opacity-50 " +
        "data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground " +
        "data-[state=indeterminate]:bg-primary data-[state=indeterminate]:text-primary-foreground " +
        "data-[state=unchecked]:bg-background";

    // ── Compositional mode support (used by MultiSelectItem) ───────────

    /// <summary>
    /// Gets the current search query for item visibility filtering.
    /// </summary>
    internal string SearchQuery => _searchQuery;

    /// <summary>
    /// Registers an item's value and display text for badge display and EffectiveOptions.
    /// Called by MultiSelectItem on initialization.
    /// </summary>
    internal void RegisterItem(TValue value, string text)
    {
        if (value is not null)
        {
            _itemTextRegistry[value] = text;
        }
    }

    /// <summary>
    /// Unregisters an item's value from the text registry.
    /// Called by MultiSelectItem on disposal.
    /// </summary>
    internal void UnregisterItem(TValue value)
    {
        if (value is not null)
        {
            _itemTextRegistry.Remove(value);
        }
    }

    /// <summary>
    /// Checks if a value is currently selected.
    /// Used by MultiSelectItem to render the checkbox state.
    /// </summary>
    internal bool IsValueSelected(TValue value) =>
        SelectedValues.Contains(value);

    /// <summary>
    /// Toggles a value's selection state.
    /// Used by MultiSelectItem on click.
    /// </summary>
    internal async Task ToggleItemValue(TValue value)
    {
        var currentValues = SelectedValues;

        if (!currentValues.Remove(value))
        {
            currentValues.Add(value);
        }

        await UpdateValues(currentValues);
    }
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
