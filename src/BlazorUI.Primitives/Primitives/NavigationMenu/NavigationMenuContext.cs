using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.NavigationMenu;

/// <summary>
/// State for the NavigationMenu primitive context.
/// </summary>
public class NavigationMenuState
{
    /// <summary>
    /// Gets or sets the currently active/open item value.
    /// Empty string means no item is active.
    /// </summary>
    public string ActiveValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the orientation of the navigation menu.
    /// </summary>
    public NavigationMenuOrientation Orientation { get; set; } = NavigationMenuOrientation.Horizontal;
}

/// <summary>
/// Orientation of the navigation menu.
/// </summary>
public enum NavigationMenuOrientation
{
    /// <summary>
    /// Horizontal navigation menu.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical navigation menu.
    /// </summary>
    Vertical
}

/// <summary>
/// Context for NavigationMenu primitive component and its children.
/// Manages active item state and provides IDs for ARIA attributes.
/// </summary>
public class NavigationMenuContext : PrimitiveContextWithEvents<NavigationMenuState>
{
    private readonly List<string> _items = new();

    /// <summary>
    /// Initializes a new instance of the NavigationMenuContext.
    /// </summary>
    public NavigationMenuContext() : base(new NavigationMenuState(), "nav-menu")
    {
    }

    /// <summary>
    /// Gets the ID for a specific navigation item trigger.
    /// </summary>
    public string GetTriggerId(string value) => GetScopedId($"trigger-{value}");

    /// <summary>
    /// Gets the ID for a specific navigation item content.
    /// </summary>
    public string GetContentId(string value) => GetScopedId($"content-{value}");

    /// <summary>
    /// Gets the currently active value.
    /// </summary>
    public string ActiveValue => State.ActiveValue;

    /// <summary>
    /// Gets the orientation of the navigation menu.
    /// </summary>
    public NavigationMenuOrientation Orientation => State.Orientation;

    /// <summary>
    /// Gets the registered items.
    /// </summary>
    public IReadOnlyList<string> Items => _items.AsReadOnly();

    /// <summary>
    /// Registers a navigation item.
    /// </summary>
    /// <param name="value">The item value.</param>
    public void RegisterItem(string value)
    {
        if (!_items.Contains(value))
        {
            _items.Add(value);
        }
    }

    /// <summary>
    /// Unregisters a navigation item.
    /// </summary>
    /// <param name="value">The item value.</param>
    public void UnregisterItem(string value)
    {
        _items.Remove(value);
    }

    /// <summary>
    /// Checks if the specified item is currently active.
    /// </summary>
    /// <param name="value">The item value to check.</param>
    public bool IsItemActive(string value)
    {
        return State.ActiveValue == value;
    }

    /// <summary>
    /// Sets the active item.
    /// </summary>
    /// <param name="value">The value of the item to activate.</param>
    public void SetActiveItem(string value)
    {
        UpdateState(state =>
        {
            state.ActiveValue = value;
        });
    }

    /// <summary>
    /// Clears the active item (closes all menus).
    /// </summary>
    public void ClearActiveItem()
    {
        UpdateState(state =>
        {
            state.ActiveValue = string.Empty;
        });
    }

    /// <summary>
    /// Navigates to the next item.
    /// </summary>
    public void NavigateNext()
    {
        if (_items.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(State.ActiveValue) 
            ? -1 
            : _items.IndexOf(State.ActiveValue);
        
        var nextIndex = (currentIndex + 1) % _items.Count;
        SetActiveItem(_items[nextIndex]);
    }

    /// <summary>
    /// Navigates to the previous item.
    /// </summary>
    public void NavigatePrevious()
    {
        if (_items.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(State.ActiveValue) 
            ? 0 
            : _items.IndexOf(State.ActiveValue);
        
        var prevIndex = currentIndex <= 0 ? _items.Count - 1 : currentIndex - 1;
        SetActiveItem(_items[prevIndex]);
    }
}

/// <summary>
/// Context for an individual navigation menu item.
/// </summary>
public class NavigationMenuItemContext
{
    /// <summary>
    /// The unique value for this item.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The ID for the trigger element.
    /// </summary>
    public string TriggerId { get; set; } = string.Empty;

    /// <summary>
    /// The ID for the content element.
    /// </summary>
    public string ContentId { get; set; } = string.Empty;
}
