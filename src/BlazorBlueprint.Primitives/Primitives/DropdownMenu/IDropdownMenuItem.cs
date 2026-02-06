namespace BlazorBlueprint.Primitives.DropdownMenu;

/// <summary>
/// Interface for dropdown menu items to support keyboard navigation.
/// </summary>
public interface IDropdownMenuItem
{
    /// <summary>
    /// Gets whether the menu item is disabled.
    /// </summary>
    public bool Disabled { get; }

    /// <summary>
    /// Focuses this menu item.
    /// </summary>
    public Task FocusAsync();

    /// <summary>
    /// Triggers a click on this menu item programmatically.
    /// </summary>
    public Task ClickAsync();
}
