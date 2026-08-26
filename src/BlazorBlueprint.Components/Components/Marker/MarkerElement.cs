namespace BlazorBlueprint.Components;

/// <summary>
/// Element type to render for <see cref="BbMarker"/>.
/// </summary>
public enum MarkerElement
{
    /// <summary>
    /// Render as a div element for static markers.
    /// </summary>
    Div,

    /// <summary>
    /// Render as an anchor element for navigation links.
    /// </summary>
    Anchor,

    /// <summary>
    /// Render as a button element for actions.
    /// </summary>
    Button
}
