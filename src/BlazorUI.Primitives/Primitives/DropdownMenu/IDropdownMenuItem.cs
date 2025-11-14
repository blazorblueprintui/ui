namespace BlazorUI.Primitives.DropdownMenu;

/// <summary>
/// Interface for dropdown menu items to support keyboard navigation.
/// </summary>
public interface IDropdownMenuItem
{
    /// <summary>
    /// Gets whether the menu item is disabled.
    /// </summary>
    bool Disabled { get; }

    /// <summary>
    /// Focuses this menu item.
    /// </summary>
    Task FocusAsync();

    /// <summary>
    /// Triggers a click on this menu item programmatically.
    /// </summary>
    Task ClickAsync();
}
