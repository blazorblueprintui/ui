namespace BlazorUI.Components.Sidebar;

/// <summary>
/// Extension methods for converting SidebarMenuButton enums to their string representations.
/// </summary>
public static class SidebarMenuButtonExtensions
{
    /// <summary>
    /// Converts SidebarMenuButtonSize to its lowercase string representation.
    /// </summary>
    public static string ToValue(this SidebarMenuButtonSize size) => size switch
    {
        SidebarMenuButtonSize.Small => "sm",
        SidebarMenuButtonSize.Default => "default",
        SidebarMenuButtonSize.Large => "lg",
        _ => "default"
    };

    /// <summary>
    /// Converts SidebarMenuButtonVariant to its lowercase string representation.
    /// </summary>
    public static string ToValue(this SidebarMenuButtonVariant variant) => variant switch
    {
        SidebarMenuButtonVariant.Default => "default",
        SidebarMenuButtonVariant.Outline => "outline",
        _ => "default"
    };

    /// <summary>
    /// Converts SidebarMenuButtonElement to its lowercase string representation.
    /// </summary>
    public static string ToValue(this SidebarMenuButtonElement element) => element switch
    {
        SidebarMenuButtonElement.Button => "button",
        SidebarMenuButtonElement.Anchor => "a",
        _ => "button"
    };

    /// <summary>
    /// Converts SidebarGroupLabelElement to its lowercase string representation.
    /// </summary>
    public static string ToValue(this SidebarGroupLabelElement element) => element switch
    {
        SidebarGroupLabelElement.Div => "div",
        SidebarGroupLabelElement.Button => "button",
        _ => "div"
    };

    /// <summary>
    /// Converts SidebarMenuSubButtonSize to its lowercase string representation.
    /// </summary>
    public static string ToValue(this SidebarMenuSubButtonSize size) => size switch
    {
        SidebarMenuSubButtonSize.Small => "sm",
        SidebarMenuSubButtonSize.Medium => "md",
        _ => "md"
    };
}
