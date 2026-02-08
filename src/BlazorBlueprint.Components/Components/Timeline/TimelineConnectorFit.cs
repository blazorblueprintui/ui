namespace BlazorBlueprint.Components.Timeline;

/// <summary>
/// Defines how the connector line fits between timeline icons.
/// </summary>
public enum TimelineConnectorFit
{
    /// <summary>
    /// Connector has a small gap between the icon and the line (ring creates visual separation).
    /// </summary>
    Spaced,

    /// <summary>
    /// Connector line connects directly to each icon with no gap.
    /// </summary>
    Connected
}
