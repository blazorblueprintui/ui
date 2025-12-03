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
    /// Gets the registered menu items.
    /// </summary>
    public IReadOnlyList<IContextMenuItem> Items => _items.AsReadOnly();

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
        if (!_items.Contains(item))
        {
            _items.Add(item);
        }
    }

    /// <summary>
    /// Unregisters a menu item.
    /// </summary>
    /// <param name="item">The menu item.</param>
    public void UnregisterItem(IContextMenuItem item)
    {
        _items.Remove(item);
    }

    /// <summary>
    /// Focuses the next enabled item.
    /// </summary>
    public void FocusNext()
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

    /// <summary>
    /// Focuses the previous enabled item.
    /// </summary>
    public void FocusPrevious()
    {
        if (_items.Count == 0) return;

        var startIndex = _focusedItemIndex <= 0 ? _items.Count - 1 : _focusedItemIndex - 1;

        for (int i = 0; i < _items.Count; i++)
        {
            var index = startIndex - i;
            if (index < 0) index = _items.Count + index;

            if (!_items[index].Disabled)
            {
                _focusedItemIndex = index;
                return;
            }
        }
    }

    /// <summary>
    /// Focuses the first enabled item.
    /// </summary>
    public void FocusFirst()
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

    /// <summary>
    /// Focuses the last enabled item.
    /// </summary>
    public void FocusLast()
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
