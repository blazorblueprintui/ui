namespace BlazorBlueprint.Primitives.Menubar;

/// <summary>
/// Shared context cascaded from <see cref="BbMenubar"/> to coordinate open/close state
/// and horizontal keyboard navigation across menus.
/// </summary>
public class MenubarContext
{
    private readonly List<MenubarMenuContext> menus = new();
    private readonly Action stateChanged;

    /// <summary>
    /// The ID of the currently open menu, or <c>null</c> if all menus are closed.
    /// </summary>
    public string? ActiveMenu { get; private set; }

    /// <summary>
    /// Creates a new <see cref="MenubarContext"/>.
    /// </summary>
    /// <param name="stateChanged">Callback invoked when active menu changes (triggers re-render).</param>
    public MenubarContext(Action stateChanged)
    {
        this.stateChanged = stateChanged;
    }

    /// <summary>
    /// Registers a menu with this context.
    /// </summary>
    public void RegisterMenu(MenubarMenuContext menu)
    {
        if (!menus.Contains(menu))
        {
            menus.Add(menu);
        }
    }

    /// <summary>
    /// Unregisters a menu from this context.
    /// </summary>
    public void UnregisterMenu(MenubarMenuContext menu) =>
        menus.Remove(menu);

    /// <summary>
    /// Sets the active (open) menu. Pass <c>null</c> to close all menus.
    /// </summary>
    public void SetActiveMenu(string? menuId)
    {
        ActiveMenu = menuId;
        stateChanged();
    }

    /// <summary>
    /// Navigates to the next menu in the menubar (wraps around).
    /// </summary>
    public void NavigateToNextMenu()
    {
        if (menus.Count == 0 || ActiveMenu == null)
        {
            return;
        }

        var currentIndex = menus.FindIndex(m => m.MenuId == ActiveMenu);
        if (currentIndex == -1)
        {
            return;
        }

        var nextIndex = (currentIndex + 1) % menus.Count;
        menus[nextIndex].Open();
    }

    /// <summary>
    /// Navigates to the previous menu in the menubar (wraps around).
    /// </summary>
    public void NavigateToPreviousMenu()
    {
        if (menus.Count == 0 || ActiveMenu == null)
        {
            return;
        }

        var currentIndex = menus.FindIndex(m => m.MenuId == ActiveMenu);
        if (currentIndex == -1)
        {
            return;
        }

        var prevIndex = currentIndex == 0 ? menus.Count - 1 : currentIndex - 1;
        menus[prevIndex].Open();
    }
}

/// <summary>
/// Per-menu context holding the menu's identity and providing open/close methods.
/// </summary>
public class MenubarMenuContext
{
    private readonly MenubarContext parent;

    /// <summary>
    /// Unique identifier for this menu.
    /// </summary>
    public string MenuId { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Whether this menu is currently open.
    /// </summary>
    public bool IsOpen => parent.ActiveMenu == MenuId;

    /// <summary>
    /// Creates a new <see cref="MenubarMenuContext"/>.
    /// </summary>
    public MenubarMenuContext(MenubarContext parent)
    {
        this.parent = parent;
    }

    /// <summary>Opens this menu (closes any other open menu).</summary>
    public void Open() => parent.SetActiveMenu(MenuId);

    /// <summary>Closes this menu if it is currently open.</summary>
    public void Close()
    {
        if (IsOpen)
        {
            parent.SetActiveMenu(null);
        }
    }

    /// <summary>Toggles this menu open/closed.</summary>
    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }
}
