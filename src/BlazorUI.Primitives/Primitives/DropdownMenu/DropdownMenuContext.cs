using Microsoft.AspNetCore.Components;
using BlazorUI.Primitives.Contexts;

namespace BlazorUI.Primitives.DropdownMenu;

/// <summary>
/// State for the DropdownMenu primitive context.
/// </summary>
public class DropdownMenuState
{
    /// <summary>
    /// Gets or sets whether the dropdown menu is currently open.
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// Gets or sets the element that triggered the dropdown menu opening.
    /// Used for positioning.
    /// </summary>
    public ElementReference? TriggerElement { get; set; }

    /// <summary>
    /// Gets or sets the index of the currently focused menu item.
    /// Used for keyboard navigation.
    /// </summary>
    public int FocusedIndex { get; set; } = -1;
}

/// <summary>
/// Context for DropdownMenu primitive component and its children.
/// Manages dropdown menu state, provides IDs for ARIA attributes, and handles keyboard navigation.
/// </summary>
public class DropdownMenuContext : PrimitiveContextWithEvents<DropdownMenuState>
{
    /// <summary>
    /// Initializes a new instance of the DropdownMenuContext.
    /// </summary>
    public DropdownMenuContext() : base(new DropdownMenuState(), "dropdown-menu")
    {
    }

    /// <summary>
    /// Gets the ID for the dropdown menu trigger button.
    /// </summary>
    public string TriggerId => GetScopedId("trigger");

    /// <summary>
    /// Gets the ID for the dropdown menu content container.
    /// </summary>
    public string ContentId => GetScopedId("content");

    /// <summary>
    /// Gets whether the dropdown menu is currently open.
    /// </summary>
    public bool IsOpen => State.IsOpen;

    /// <summary>
    /// Gets the currently focused menu item index.
    /// </summary>
    public int FocusedIndex => State.FocusedIndex;

    /// <summary>
    /// Gets or sets the currently active submenu context.
    /// Used to close submenus when hovering over other items.
    /// </summary>
    public DropdownMenuSubContext? ActiveSubMenu { get; set; }

    /// <summary>
    /// Opens the dropdown menu.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the dropdown menu.</param>
    public void Open(ElementReference? triggerElement = null)
    {
        UpdateState(state =>
        {
            state.IsOpen = true;
            state.TriggerElement = triggerElement;
            state.FocusedIndex = -1; // Reset focus on open
        });
    }

    /// <summary>
    /// Closes the dropdown menu.
    /// </summary>
    public void Close()
    {
        // Close any active submenu first
        ActiveSubMenu?.Close();
        ActiveSubMenu = null;

        UpdateState(state =>
        {
            state.IsOpen = false;
            state.FocusedIndex = -1;
        });
    }

    /// <summary>
    /// Toggles the dropdown menu open/closed state.
    /// </summary>
    /// <param name="triggerElement">Optional element that triggered the toggle.</param>
    public void Toggle(ElementReference? triggerElement = null)
    {
        if (State.IsOpen)
        {
            Close();
        }
        else
        {
            Open(triggerElement);
        }
    }

    /// <summary>
    /// Sets the focused menu item index for keyboard navigation.
    /// </summary>
    /// <param name="index">The index of the menu item to focus.</param>
    public void SetFocusedIndex(int index)
    {
        UpdateState(state =>
        {
            state.FocusedIndex = index;
        });
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
