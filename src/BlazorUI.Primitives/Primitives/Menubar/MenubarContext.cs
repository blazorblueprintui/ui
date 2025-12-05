using BlazorUI.Primitives.Contexts;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Primitives.Menubar;

/// <summary>
/// State for the Menubar primitive context.
/// </summary>
public class MenubarState
{
    /// <summary>
    /// Gets or sets the index of the currently active (open) menu.
    /// -1 means no menu is open.
    /// </summary>
    public int ActiveIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the index of the currently focused trigger in the menubar.
    /// Used for keyboard navigation between menu triggers.
    /// </summary>
    public int FocusedTriggerIndex { get; set; } = 0;
}

/// <summary>
/// Context for Menubar primitive component and its children.
/// Manages menubar state, provides IDs for ARIA attributes, and handles keyboard navigation.
/// </summary>
public class MenubarContext : PrimitiveContextWithEvents<MenubarState>
{
    private readonly List<MenubarMenuContext> _menus = new();

    /// <summary>
    /// Initializes a new instance of the MenubarContext.
    /// </summary>
    public MenubarContext() : base(new MenubarState(), "menubar")
    {
    }

    /// <summary>
    /// Gets the ID for the menubar element.
    /// </summary>
    public string MenubarId => GetScopedId("bar");

    /// <summary>
    /// Gets the currently active menu index (-1 if no menu is open).
    /// </summary>
    public int ActiveIndex => State.ActiveIndex;

    /// <summary>
    /// Gets the currently focused trigger index.
    /// </summary>
    public int FocusedTriggerIndex => State.FocusedTriggerIndex;

    /// <summary>
    /// Gets the list of registered menus.
    /// </summary>
    public IReadOnlyList<MenubarMenuContext> Menus => _menus;

    /// <summary>
    /// Gets or sets the currently active submenu context.
    /// Used to close submenus when hovering over other items.
    /// </summary>
    public MenubarSubContext? ActiveSubMenu { get; set; }

    /// <summary>
    /// Registers a menu with the menubar.
    /// </summary>
    /// <param name="menu">The menu context to register.</param>
    public void RegisterMenu(MenubarMenuContext menu)
    {
        if (!_menus.Contains(menu))
        {
            _menus.Add(menu);
        }
    }

    /// <summary>
    /// Unregisters a menu from the menubar.
    /// </summary>
    /// <param name="menu">The menu context to unregister.</param>
    public void UnregisterMenu(MenubarMenuContext menu)
    {
        _menus.Remove(menu);
    }

    /// <summary>
    /// Opens the menu at the specified index.
    /// </summary>
    /// <param name="index">The index of the menu to open.</param>
    public void OpenMenu(int index)
    {
        if (index >= 0 && index < _menus.Count)
        {
            UpdateState(state =>
            {
                state.ActiveIndex = index;
                state.FocusedTriggerIndex = index;
            });
        }
    }

    /// <summary>
    /// Closes any open menu.
    /// </summary>
    public void CloseMenu()
    {
        // Close any active submenu first
        ActiveSubMenu?.Close();
        ActiveSubMenu = null;

        UpdateState(state =>
        {
            state.ActiveIndex = -1;
        });
    }

    /// <summary>
    /// Toggles the menu at the specified index.
    /// </summary>
    /// <param name="index">The index of the menu to toggle.</param>
    public void ToggleMenu(int index)
    {
        if (State.ActiveIndex == index)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu(index);
        }
    }

    /// <summary>
    /// Sets the focused trigger index.
    /// </summary>
    /// <param name="index">The index to focus.</param>
    public void SetFocusedTriggerIndex(int index)
    {
        if (index >= 0 && index < _menus.Count)
        {
            UpdateState(state =>
            {
                state.FocusedTriggerIndex = index;
            });
        }
    }

    /// <summary>
    /// Moves focus to the next trigger (with wrapping).
    /// </summary>
    public void FocusNextTrigger()
    {
        if (_menus.Count == 0) return;

        var nextIndex = (State.FocusedTriggerIndex + 1) % _menus.Count;
        UpdateState(state =>
        {
            state.FocusedTriggerIndex = nextIndex;
            // If a menu is open, also open the next one
            if (state.ActiveIndex >= 0)
            {
                state.ActiveIndex = nextIndex;
            }
        });
    }

    /// <summary>
    /// Moves focus to the previous trigger (with wrapping).
    /// </summary>
    public void FocusPreviousTrigger()
    {
        if (_menus.Count == 0) return;

        var prevIndex = State.FocusedTriggerIndex - 1;
        if (prevIndex < 0) prevIndex = _menus.Count - 1;

        UpdateState(state =>
        {
            state.FocusedTriggerIndex = prevIndex;
            // If a menu is open, also open the previous one
            if (state.ActiveIndex >= 0)
            {
                state.ActiveIndex = prevIndex;
            }
        });
    }

    /// <summary>
    /// Checks if the menu at the specified index is currently open.
    /// </summary>
    /// <param name="index">The index of the menu to check.</param>
    /// <returns>True if the menu is open, otherwise false.</returns>
    public bool IsMenuOpen(int index)
    {
        return State.ActiveIndex == index;
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
}

/// <summary>
/// Context for an individual menu within the menubar.
/// </summary>
public class MenubarMenuContext
{
    /// <summary>
    /// Gets the unique ID for this menu.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the index of this menu in the menubar.
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Gets the parent menubar context.
    /// </summary>
    public MenubarContext MenubarContext { get; }

    /// <summary>
    /// Gets or sets the trigger element reference for positioning.
    /// </summary>
    public ElementReference? TriggerElement { get; set; }

    /// <summary>
    /// Initializes a new instance of the MenubarMenuContext.
    /// </summary>
    /// <param name="menubarContext">The parent menubar context.</param>
    /// <param name="idSuffix">A suffix for the menu ID.</param>
    public MenubarMenuContext(MenubarContext menubarContext, string idSuffix)
    {
        MenubarContext = menubarContext ?? throw new ArgumentNullException(nameof(menubarContext));
        Id = menubarContext.GetScopedId($"menu-{idSuffix}");
    }

    /// <summary>
    /// Gets the ID for the trigger element.
    /// </summary>
    public string TriggerId => $"{Id}-trigger";

    /// <summary>
    /// Gets the ID for the content element.
    /// </summary>
    public string ContentId => $"{Id}-content";

    /// <summary>
    /// Gets whether this menu is currently open.
    /// </summary>
    public bool IsOpen => MenubarContext.IsMenuOpen(Index);

    /// <summary>
    /// Opens this menu.
    /// </summary>
    /// <param name="triggerElement">Optional trigger element reference for positioning.</param>
    public void Open(ElementReference? triggerElement = null)
    {
        if (triggerElement.HasValue)
        {
            TriggerElement = triggerElement;
        }
        MenubarContext.OpenMenu(Index);
    }

    /// <summary>
    /// Closes this menu.
    /// </summary>
    public void Close()
    {
        MenubarContext.CloseMenu();
    }

    /// <summary>
    /// Toggles this menu.
    /// </summary>
    /// <param name="triggerElement">Optional trigger element reference for positioning.</param>
    public void Toggle(ElementReference? triggerElement = null)
    {
        if (triggerElement.HasValue)
        {
            TriggerElement = triggerElement;
        }
        MenubarContext.ToggleMenu(Index);
    }
}
