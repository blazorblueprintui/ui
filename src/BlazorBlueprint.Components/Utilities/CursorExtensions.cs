namespace BlazorBlueprint.Components;

/// <summary>
/// Extension methods for converting <see cref="CursorType"/> to Tailwind CSS class strings.
/// </summary>
public static class CursorExtensions
{
    /// <summary>
    /// Converts a <see cref="CursorType"/> value to its corresponding Tailwind CSS cursor class.
    /// </summary>
    /// <param name="cursor">The cursor type to convert.</param>
    /// <returns>The Tailwind CSS class string (e.g., <c>"cursor-pointer"</c>).</returns>
    public static string ToClass(this CursorType cursor) => cursor switch
    {
        CursorType.Default => "bb:cursor-default",
        CursorType.Pointer => "bb:cursor-pointer",
        CursorType.NotAllowed => "bb:cursor-not-allowed",
        CursorType.Crosshair => "bb:cursor-crosshair",
        CursorType.Grab => "bb:cursor-grab",
        CursorType.Grabbing => "bb:cursor-grabbing",
        CursorType.ColResize => "bb:cursor-col-resize",
        CursorType.RowResize => "bb:cursor-row-resize",
        CursorType.EResize => "bb:cursor-e-resize",
        CursorType.WResize => "bb:cursor-w-resize",
        CursorType.Wait => "bb:cursor-wait",
        CursorType.Text => "bb:cursor-text",
        CursorType.Move => "bb:cursor-move",
        CursorType.Help => "bb:cursor-help",
        CursorType.None => "bb:cursor-none",
        CursorType.Auto => "bb:cursor-auto",
        _ => "bb:cursor-default"
    };
}
