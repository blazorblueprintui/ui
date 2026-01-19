using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Command;

/// <summary>
/// Metadata for a registered command item.
/// </summary>
public class CommandItemMetadata
{
    /// <summary>
    /// Gets or sets the value of the command item.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the text used for search filtering.
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when this item is selected.
    /// </summary>
    public EventCallback OnSelect { get; set; }
}

/// <summary>
/// Context for Command component and its children.
/// Manages search query, item registration, filtering, and keyboard navigation.
/// </summary>
public class CommandContext
{
    private readonly List<CommandItemMetadata> _items = new();
    private string _searchQuery = string.Empty;
    private int _focusedIndex = -1;
    private Func<CommandItemMetadata, string, bool>? _filterFunction;
    private bool _closeOnSelect = true;
    private bool _disabled;
    private bool _hasRegisteredItems = false;

    /// <summary>
    /// Event that is raised when the state changes.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Gets or sets the callback invoked when an item is selected.
    /// </summary>
    public EventCallback<string> OnValueChange { get; set; }

    /// <summary>
    /// Gets the unique ID for this command context.
    /// </summary>
    public string Id { get; } = $"command-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the ID for the command input.
    /// </summary>
    public string InputId => $"{Id}-input";

    /// <summary>
    /// Gets the ID for the command list.
    /// </summary>
    public string ListId => $"{Id}-list";

    /// <summary>
    /// Gets the current search query.
    /// </summary>
    public string SearchQuery => _searchQuery;

    /// <summary>
    /// Gets or sets the custom filter function.
    /// </summary>
    public Func<CommandItemMetadata, string, bool>? FilterFunction
    {
        get => _filterFunction;
        set => _filterFunction = value;
    }

    /// <summary>
    /// Gets or sets whether to close the dropdown after selection.
    /// </summary>
    public bool CloseOnSelect
    {
        get => _closeOnSelect;
        set => _closeOnSelect = value;
    }

    /// <summary>
    /// Gets or sets whether the command is disabled.
    /// </summary>
    public bool Disabled
    {
        get => _disabled;
        set => _disabled = value;
    }

    /// <summary>
    /// Gets the currently focused item index within filtered items.
    /// </summary>
    public int FocusedIndex => _focusedIndex;

    /// <summary>
    /// Updates the search query and notifies subscribers.
    /// </summary>
    /// <param name="query">The new search query.</param>
    public void SetSearchQuery(string query)
    {
        if (_searchQuery != query)
        {
            _searchQuery = query;
            _focusedIndex = -1; // Reset focus when search changes
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Registers an item with the command context.
    /// </summary>
    /// <returns>The index of the registered item.</returns>
    public int RegisterItem(string? value, string? searchText, bool disabled, EventCallback onSelect)
    {
        var metadata = new CommandItemMetadata
        {
            Value = value,
            SearchText = searchText ?? value,
            Disabled = disabled,
            OnSelect = onSelect
        };
        _items.Add(metadata);
        _hasRegisteredItems = true;
        NotifyStateChanged(); // Notify so CommandEmpty can update
        return _items.Count - 1;
    }

    /// <summary>
    /// Updates an existing item's metadata.
    /// </summary>
    public void UpdateItem(int index, string? value, string? searchText, bool disabled, EventCallback onSelect)
    {
        if (index >= 0 && index < _items.Count)
        {
            _items[index].Value = value;
            _items[index].SearchText = searchText ?? value;
            _items[index].Disabled = disabled;
            _items[index].OnSelect = onSelect;
        }
    }

    /// <summary>
    /// Unregisters an item from the command context.
    /// </summary>
    public void UnregisterItem(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            _items[index] = null!; // Mark as null, don't remove to preserve indices
        }
    }

    /// <summary>
    /// Gets the list of filtered items based on the current search query.
    /// </summary>
    public List<CommandItemMetadata> GetFilteredItems()
    {
        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            return _items.Where(i => i != null).ToList();
        }

        if (_filterFunction != null)
        {
            return _items.Where(i => i != null && _filterFunction(i, _searchQuery)).ToList();
        }

        return _items
            .Where(i => i != null && (i.SearchText?.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }

    /// <summary>
    /// Gets an item by its registration index.
    /// </summary>
    public CommandItemMetadata? GetItemByIndex(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            return _items[index];
        }
        return null;
    }

    /// <summary>
    /// Gets the ID for an item by index.
    /// </summary>
    public string GetItemId(int index) => $"{Id}-item-{index}";

    /// <summary>
    /// Gets whether there are any visible items.
    /// Returns true if no items have been registered yet (to avoid showing "No results" on initial load).
    /// </summary>
    public bool HasVisibleItems()
    {
        // Don't show "No results" until items have had a chance to register
        if (!_hasRegisteredItems)
            return true;

        return GetFilteredItems().Any(i => !i.Disabled);
    }

    /// <summary>
    /// Sets the focused index within filtered items.
    /// </summary>
    public void SetFocusedIndex(int index)
    {
        if (_focusedIndex != index)
        {
            _focusedIndex = index;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Moves focus to the next/previous item.
    /// </summary>
    /// <param name="direction">1 for next, -1 for previous.</param>
    public void MoveFocus(int direction)
    {
        var filteredItems = GetFilteredItems().Where(i => !i.Disabled).ToList();
        if (filteredItems.Count == 0) return;

        var allFiltered = GetFilteredItems();
        int currentIndex = _focusedIndex;
        int newIndex = currentIndex;

        // Find next non-disabled item in filtered list
        do
        {
            newIndex += direction;

            if (newIndex < 0)
                newIndex = allFiltered.Count - 1;
            else if (newIndex >= allFiltered.Count)
                newIndex = 0;

            // Avoid infinite loop
            if (newIndex == currentIndex)
                break;

        } while (newIndex >= 0 && newIndex < allFiltered.Count && allFiltered[newIndex].Disabled);

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
    public async Task SelectFocusedItemAsync()
    {
        var filteredItems = GetFilteredItems();
        if (_focusedIndex >= 0 && _focusedIndex < filteredItems.Count)
        {
            var item = filteredItems[_focusedIndex];
            if (!item.Disabled)
            {
                await SelectItemAsync(item);
            }
        }
    }

    /// <summary>
    /// Selects an item by its value.
    /// </summary>
    public async Task SelectItemByValueAsync(string value)
    {
        var item = _items.FirstOrDefault(i => i != null && i.Value == value);
        if (item != null && !item.Disabled)
        {
            await SelectItemAsync(item);
        }
    }

    /// <summary>
    /// Selects an item and invokes callbacks.
    /// </summary>
    private async Task SelectItemAsync(CommandItemMetadata item)
    {
        if (item.OnSelect.HasDelegate)
        {
            await item.OnSelect.InvokeAsync();
        }

        if (OnValueChange.HasDelegate)
        {
            await OnValueChange.InvokeAsync(item.Value);
        }
    }

    /// <summary>
    /// Notifies subscribers that the state has changed.
    /// </summary>
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
