namespace BlazorUI.Primitives.Services;

/// <summary>
/// Extension methods for converting popover enums to their string representations.
/// </summary>
public static class PopoverEnumExtensions
{
    /// <summary>
    /// Converts PopoverSide to its lowercase string representation.
    /// </summary>
    public static string ToValue(this PopoverSide side) => side switch
    {
        PopoverSide.Top => "top",
        PopoverSide.Right => "right",
        PopoverSide.Bottom => "bottom",
        PopoverSide.Left => "left",
        _ => "bottom"
    };

    /// <summary>
    /// Converts PopoverAlign to its lowercase string representation.
    /// </summary>
    public static string ToValue(this PopoverAlign align) => align switch
    {
        PopoverAlign.Start => "start",
        PopoverAlign.Center => "center",
        PopoverAlign.End => "end",
        _ => "center"
    };

    /// <summary>
    /// Converts PositioningStrategy to its lowercase string representation.
    /// </summary>
    public static string ToValue(this PositioningStrategy strategy) => strategy switch
    {
        PositioningStrategy.Absolute => "absolute",
        PositioningStrategy.Fixed => "fixed",
        _ => "absolute"
    };

    /// <summary>
    /// Converts PopoverPlacement to its kebab-case string representation.
    /// </summary>
    public static string ToValue(this PopoverPlacement placement) => placement switch
    {
        PopoverPlacement.Top => "top",
        PopoverPlacement.TopStart => "top-start",
        PopoverPlacement.TopEnd => "top-end",
        PopoverPlacement.Right => "right",
        PopoverPlacement.RightStart => "right-start",
        PopoverPlacement.RightEnd => "right-end",
        PopoverPlacement.Bottom => "bottom",
        PopoverPlacement.BottomStart => "bottom-start",
        PopoverPlacement.BottomEnd => "bottom-end",
        PopoverPlacement.Left => "left",
        PopoverPlacement.LeftStart => "left-start",
        PopoverPlacement.LeftEnd => "left-end",
        _ => "bottom"
    };
}
