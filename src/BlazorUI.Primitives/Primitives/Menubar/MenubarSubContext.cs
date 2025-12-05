using Microsoft.AspNetCore.Components;

namespace BlazorUI.Primitives.Menubar;

/// <summary>
/// Context class for sharing state between MenubarSub, MenubarSubTrigger, and MenubarSubContent components.
/// </summary>
public class MenubarSubContext
{
    /// <summary>
    /// Gets or sets whether the submenu is open.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the trigger element reference for positioning.
    /// </summary>
    public ElementReference? TriggerElement { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the open state changes.
    /// </summary>
    public Func<bool, Task>? OnOpenChange { get; set; }

    /// <summary>
    /// Opens the submenu.
    /// </summary>
    public void Open(ElementReference? triggerElement = null)
    {
        if (triggerElement.HasValue)
        {
            TriggerElement = triggerElement;
        }
        IsOpen = true;
        OnOpenChange?.Invoke(true);
        NotifyStateChanged();
    }

    /// <summary>
    /// Gets or sets the nested submenu context (for recursive closing).
    /// </summary>
    public MenubarSubContext? ActiveSubMenu { get; set; }

    /// <summary>
    /// Closes the submenu and any nested submenus.
    /// </summary>
    public void Close()
    {
        // Close any nested submenu first (recursive)
        ActiveSubMenu?.Close();
        ActiveSubMenu = null;
        
        IsOpen = false;
        OnOpenChange?.Invoke(false);
        NotifyStateChanged();
    }

    /// <summary>
    /// Closes the active nested submenu.
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
    /// Event raised when state changes.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Notifies listeners of state changes.
    /// </summary>
    public void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }
}
