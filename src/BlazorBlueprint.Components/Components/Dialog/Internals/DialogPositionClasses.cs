namespace BlazorBlueprint.Components.Components.Dialog.Internals;

/// <summary>
/// Maps a <see cref="DialogPosition"/> to the placement classes shared by
/// <see cref="BbDialogContent"/> and <see cref="BbDialogProvider"/>.
/// </summary>
/// <remarks>
/// A native <c>&lt;dialog&gt;</c> opened with showModal() gets <c>inset: 0</c> from the browser, so an
/// edge position also sets the opposite inset to <c>auto</c>; otherwise the left/top inset wins and
/// the dialog never leaves the top-left. Positions against a side edge keep a 1rem gutter on both
/// sides on narrow screens, where <c>w-full</c> would push the dialog off the far edge.
/// </remarks>
internal static class DialogPositionClasses
{
    private const string SideWidth = "bb:w-[calc(100%-2rem)]";

    public static string Get(DialogPosition position) => position switch
    {
        DialogPosition.TopCenter => "bb:left-[50%] bb:top-4 bb:translate-x-[-50%]",
        DialogPosition.BottomCenter => "bb:left-[50%] bb:top-auto bb:bottom-4 bb:translate-x-[-50%]",
        DialogPosition.CenterLeft => $"bb:left-4 bb:top-[50%] bb:translate-y-[-50%] {SideWidth}",
        DialogPosition.CenterRight => $"bb:left-auto bb:right-4 bb:top-[50%] bb:translate-y-[-50%] {SideWidth}",
        DialogPosition.TopLeft => $"bb:left-4 bb:top-4 {SideWidth}",
        DialogPosition.TopRight => $"bb:left-auto bb:right-4 bb:top-4 {SideWidth}",
        DialogPosition.BottomLeft => $"bb:left-4 bb:top-auto bb:bottom-4 {SideWidth}",
        DialogPosition.BottomRight => $"bb:left-auto bb:right-4 bb:top-auto bb:bottom-4 {SideWidth}",
        _ => "bb:left-[50%] bb:top-[50%] bb:translate-x-[-50%] bb:translate-y-[-50%]"
    };
}
