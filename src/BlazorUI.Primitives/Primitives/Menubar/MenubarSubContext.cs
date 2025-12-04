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
    /// Closes the submenu.
    /// </summary>
    public void Close()
    {
        IsOpen = false;
        OnOpenChange?.Invoke(false);
        NotifyStateChanged();
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
