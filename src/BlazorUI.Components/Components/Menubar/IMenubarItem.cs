namespace BlazorUI.Components.Menubar;

/// <summary>
/// Interface for menubar items to support keyboard navigation.
/// </summary>
public interface IMenubarItem
{
    /// <summary>
    /// Gets whether the menu item is disabled.
    /// </summary>
    bool IsDisabled { get; }

    /// <summary>
    /// Focuses this menu item.
    /// </summary>
    Task FocusAsync();
}
