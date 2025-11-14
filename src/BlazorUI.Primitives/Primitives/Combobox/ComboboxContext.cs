using Microsoft.AspNetCore.Components;
using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.Combobox;

/// <summary>
/// State for the Combobox primitive context.
/// </summary>
public class ComboboxState
{
    /// <summary>
    /// Gets or sets the current search query.
    /// </summary>
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the index of the currently focused item for keyboard navigation.
    /// </summary>
    public int FocusedIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets whether the combobox is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the input element reference for focus management.
    /// </summary>
    public ElementReference? InputElement { get; set; }
}

/// <summary>
/// Context for Combobox primitive component and its children.
/// Manages combobox state, item registration, filtering, and keyboard navigation.
/// </summary>
public class ComboboxContext : PrimitiveContextWithEvents<ComboboxState>
{
    /// <summary>
    /// Callback invoked when the search query changes.
    /// </summary>
    public Action<string>? OnSearchQueryChange { get; set; }

    /// <summary>
    /// Callback invoked when an item is selected.
    /// </summary>
    public Action<string>? OnValueChange { get; set; }

    /// <summary>
    /// Custom filter function. If null, uses default Contains matching.
    /// </summary>
    public Func<ComboboxItemMetadata, string, bool>? FilterFunction { get; set; }

    /// <summary>
    /// Initializes a new instance of the ComboboxContext.
    /// </summary>
    public ComboboxContext() : base(new ComboboxState(), "combobox")
    {
    }

    /// <summary>
    /// Gets the ID for the combobox input element.
    /// </summary>
    public string InputId => GetScopedId("input");

    /// <summary>
    /// Gets the ID for the combobox content container.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets the current search query.
    /// </summary>
    public string SearchQuery => State.SearchQuery;

    /// <summary>
    /// Gets the currently focused item index.
    /// </summary>
    public int FocusedIndex => State.FocusedIndex;

    /// <summary>
    /// Gets whether the combobox is disabled.
    /// </summary>
    public bool Disabled => State.Disabled;

    /// <summary>
    /// Gets the list of registered items for keyboard navigation.
    /// </summary>
    private List<ComboboxItemMetadata> Items { get; } = new List<ComboboxItemMetadata>();

    /// <summary>
    /// Cached list of filtered items to avoid recomputing on every access.
    /// </summary>
    private List<ComboboxItemMetadata>? _cachedFilteredItems;

    /// <summary>
    /// Gets an item by its index in the registered items list.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item metadata, or null if the index is out of range.</returns>
    public ComboboxItemMetadata? GetItemByIndex(int index)
    {
        if (index < 0 || index >= Items.Count)
            return null;
        return Items[index];
    }

    /// <summary>
    /// Updates the search query and resets focus.
    /// </summary>
    /// <param name="query">The new search query.</param>
    public void UpdateSearchQuery(string query)
    {
        UpdateState(state =>
        {
            state.SearchQuery = query;
            state.FocusedIndex = -1; // Reset focus when query changes
        });

        InvalidateFilterCache();
        OnSearchQueryChange?.Invoke(query);
    }

    /// <summary>
    /// Clears the search query.
    /// </summary>
    public void ClearSearch()
    {
        UpdateSearchQuery(string.Empty);
    }

    /// <summary>
    /// Sets the disabled state.
    /// </summary>
    /// <param name="disabled">Whether the combobox is disabled.</param>
    public void SetDisabled(bool disabled)
    {
        UpdateState(state =>
        {
            state.Disabled = disabled;
        });
    }

    /// <summary>
    /// Sets the focused item index for keyboard navigation.
    /// </summary>
    /// <param name="index">The index of the item to focus.</param>
    public void SetFocusedIndex(int index)
    {
        UpdateState(state =>
        {
            state.FocusedIndex = index;
        });
    }

    /// <summary>
    /// Registers an item with the combobox context for filtering and keyboard navigation.
    /// </summary>
    /// <param name="value">The value of the item.</param>
    /// <param name="searchText">The text used for filtering.</param>
    /// <param name="disabled">Whether the item is disabled.</param>
    /// <param name="onSelect">The callback to invoke when the item is selected.</param>
    /// <returns>The index of the registered item.</returns>
    public int RegisterItem(string? value, string? searchText, bool disabled, EventCallback onSelect)
    {
        var metadata = new ComboboxItemMetadata
        {
            Value = value,
            SearchText = searchText,
            Disabled = disabled,
            OnSelect = onSelect
        };
        Items.Add(metadata);
        InvalidateFilterCache();
        return Items.Count - 1;
    }

    /// <summary>
    /// Unregisters an item from the combobox context.
    /// </summary>
    /// <param name="index">The index of the item to unregister.</param>
    public void UnregisterItem(int index)
    {
        if (index >= 0 && index < Items.Count)
        {
            Items.RemoveAt(index);
            InvalidateFilterCache();
        }
    }

    /// <summary>
    /// Clears all registered items.
    /// </summary>
    public void ClearItems()
    {
        Items.Clear();
        InvalidateFilterCache();
    }

    /// <summary>
    /// Gets the list of filtered items based on the current search query.
    /// Uses custom FilterFunction if provided, otherwise uses default Contains matching.
    /// </summary>
    public List<ComboboxItemMetadata> GetFilteredItems()
    {
        if (_cachedFilteredItems != null)
        {
            return _cachedFilteredItems;
        }

        if (string.IsNullOrWhiteSpace(State.SearchQuery))
        {
            _cachedFilteredItems = Items;
        }
        else
        {
            if (FilterFunction != null)
            {
                // Use custom filter function
                _cachedFilteredItems = Items.Where(item =>
                    FilterFunction(item, State.SearchQuery)
                ).ToList();
            }
            else
            {
                // Use default Contains matching
                _cachedFilteredItems = Items.Where(item =>
                    item.SearchText?.Contains(State.SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false
                ).ToList();
            }
        }

        return _cachedFilteredItems;
    }

    /// <summary>
    /// Invalidates the cached filtered items.
    /// </summary>
    private void InvalidateFilterCache()
    {
        _cachedFilteredItems = null;
    }

    /// <summary>
    /// Moves focus to the next or previous item that is not disabled.
    /// </summary>
    /// <param name="direction">1 for next, -1 for previous.</param>
    public void MoveFocus(int direction)
    {
        var filteredItems = GetFilteredItems();
        if (filteredItems.Count == 0) return;

        int startIndex = State.FocusedIndex;
        int newIndex = startIndex;

        // Find next non-disabled item
        do
        {
            newIndex += direction;

            // Wrap around
            if (newIndex < 0)
                newIndex = filteredItems.Count - 1;
            else if (newIndex >= filteredItems.Count)
                newIndex = 0;

            // Avoid infinite loop if all items are disabled
            if (newIndex == startIndex)
                break;

        } while (newIndex >= 0 && newIndex < filteredItems.Count && filteredItems[newIndex].Disabled);

        SetFocusedIndex(newIndex);
    }

    /// <summary>
    /// Moves focus to the first enabled item.
    /// </summary>
    public void FocusFirst()
    {
        var filteredItems = GetFilteredItems();
        for (int i = 0; i < filteredItems.Count; i++)
        {
            if (!filteredItems[i].Disabled)
            {
                SetFocusedIndex(i);
                return;
            }
        }
    }

    /// <summary>
    /// Moves focus to the last enabled item.
    /// </summary>
    public void FocusLast()
    {
        var filteredItems = GetFilteredItems();
        for (int i = filteredItems.Count - 1; i >= 0; i--)
        {
            if (!filteredItems[i].Disabled)
            {
                SetFocusedIndex(i);
                return;
            }
        }
    }

    /// <summary>
    /// Selects the currently focused item.
    /// </summary>
    public async Task SelectFocusedItem()
    {
        var filteredItems = GetFilteredItems();
        if (State.FocusedIndex >= 0 && State.FocusedIndex < filteredItems.Count)
        {
            var item = filteredItems[State.FocusedIndex];
            if (!item.Disabled)
            {
                await item.OnSelect.InvokeAsync();
                OnValueChange?.Invoke(item.Value ?? item.SearchText ?? string.Empty);

                // Clear search to close dropdown (for command/combobox pattern)
                ClearSearch();
            }
        }
    }

    /// <summary>
    /// Selects an item by its value.
    /// </summary>
    /// <param name="value">The value of the item to select.</param>
    public async Task SelectItemByValue(string value)
    {
        var item = Items.FirstOrDefault(i => i.Value == value);
        if (item != null && !item.Disabled)
        {
            await item.OnSelect.InvokeAsync();
            OnValueChange?.Invoke(item.Value ?? item.SearchText ?? string.Empty);

            // Clear search to close dropdown (for command/combobox pattern)
            ClearSearch();
        }
    }

    /// <summary>
    /// Gets an item ID for ARIA attributes.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    public string GetItemId(int index) => GetScopedId($"item-{index}");
}

/// <summary>
/// Metadata for a registered combobox item.
/// </summary>
public class ComboboxItemMetadata
{
    /// <summary>
    /// Gets or sets the value associated with this item.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the text used for search filtering.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Gets or sets whether this item is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when this item is selected.
    /// </summary>
    public EventCallback OnSelect { get; set; }
}
