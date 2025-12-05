using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.ContextMenu;

/// <summary>
/// State for the ContextMenu primitive context.
/// </summary>
public class ContextMenuState
{
    /// <summary>
    /// Gets or sets whether the context menu is open.
    /// </summary>
    public bool IsOpen { get; set; } = false;

    /// <summary>
    /// Gets or sets the X coordinate of the context menu.
    /// </summary>
    public double X { get; set; } = 0;

    /// <summary>
    /// Gets or sets the Y coordinate of the context menu.
    /// </summary>
    public double Y { get; set; } = 0;
}

/// <summary>
/// Context for ContextMenu primitive component and its children.
/// Manages open state and position.
/// </summary>
public class ContextMenuContext : PrimitiveContextWithEvents<ContextMenuState>
{
    private readonly List<IContextMenuItem> _items = new();
    private readonly object _lock = new();
    private int _focusedItemIndex = -1;

    /// <summary>
    /// Initializes a new instance of the ContextMenuContext.
    /// </summary>
    public ContextMenuContext() : base(new ContextMenuState(), "ctx-menu")
    {
    }

    /// <summary>
    /// Gets the ID for the context menu trigger.
    /// </summary>
    public string TriggerId => GetScopedId("trigger");

    /// <summary>
    /// Gets the ID for the context menu content.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets whether the context menu is open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Gets the X position of the context menu.
    /// </summary>
    public double X => State.X;

    /// <summary>
    /// Gets the Y position of the context menu.
    /// </summary>
    public double Y => State.Y;

    /// <summary>
    /// Gets or sets the currently active submenu context.
    /// Used to close submenus when hovering over other items.
    /// </summary>
    public ContextMenuSubContext? ActiveSubMenu { get; set; }

    /// <summary>
    /// Gets the registered menu items.
    /// </summary>
    public IReadOnlyList<IContextMenuItem> Items
    {
        get
        {
            lock (_lock)
            {
                return _items.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets the currently focused item index.
    /// </summary>
    public int FocusedItemIndex => _focusedItemIndex;

    /// <summary>
    /// Opens the context menu at the specified position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public void Open(double x, double y)
    {
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.X = x;
            state.Y = y;
        });
        _focusedItemIndex = -1;
    }

    /// <summary>
    /// Closes the context menu.
    /// </summary>
    public void Close()
    {
        // Close any active submenu first
        ActiveSubMenu?.Close();
        ActiveSubMenu = null;

        UpdateState(state =>
        {
            state.IsOpen = false;
        });
        _focusedItemIndex = -1;
    }

    /// <summary>
    /// Registers a menu item for keyboard navigation.
    /// </summary>
    /// <param name="item">The menu item.</param>
    public void RegisterItem(IContextMenuItem item)
    {
        lock (_lock)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
            }
        }
    }

    /// <summary>
    /// Unregisters a menu item.
    /// </summary>
    /// <param name="item">The menu item.</param>
    public void UnregisterItem(IContextMenuItem item)
    {
        lock (_lock)
        {
            _items.Remove(item);
        }
    }

    /// <summary>
    /// Closes any active submenu. Called when hovering over sibling menu items.
    /// </summary>
    public void CloseActiveSubMenu()
    {
        if (ActiveSubMenu != null)
        {
            ActiveSubMenu.Close();
            ActiveSubMenu = null;
        }
    }

    /// <summary>
    /// Sets the focused item index.
    /// </summary>
    public void SetFocusedIndex(int index)
    {
        _focusedItemIndex = index;
    }

    /// <summary>
    /// Focuses the next enabled item.
    /// </summary>
    public void FocusNext()
    {
        lock (_lock)
        {
            if (_items.Count == 0) return;

            var startIndex = _focusedItemIndex == -1 ? 0 : _focusedItemIndex + 1;

            for (int i = 0; i < _items.Count; i++)
            {
                var index = (startIndex + i) % _items.Count;
                if (!_items[index].Disabled)
                {
                    _focusedItemIndex = index;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Focuses the previous enabled item.
    /// </summary>
    public void FocusPrevious()
    {
        lock (_lock)
        {
            if (_items.Count == 0) return;

            var startIndex = _focusedItemIndex <= 0 ? _items.Count - 1 : _focusedItemIndex - 1;

            for (int i = 0; i < _items.Count; i++)
            {
                // Use proper modulo formula to avoid negative index issues
                var index = (startIndex - i + _items.Count) % _items.Count;

                if (!_items[index].Disabled)
                {
                    _focusedItemIndex = index;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Focuses the first enabled item.
    /// </summary>
    public void FocusFirst()
    {
        lock (_lock)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (!_items[i].Disabled)
                {
                    _focusedItemIndex = i;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Focuses the last enabled item.
    /// </summary>
    public void FocusLast()
    {
        lock (_lock)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (!_items[i].Disabled)
                {
                    _focusedItemIndex = i;
                    return;
                }
            }
        }
    }
}

/// <summary>
/// Interface for context menu items that support keyboard navigation.
/// </summary>
public interface IContextMenuItem
{
    /// <summary>
    /// Gets whether the item is disabled.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Focuses the item.
    /// </summary>
    Task FocusAsync();
}
