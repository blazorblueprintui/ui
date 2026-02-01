namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the visual variant for a RadialChart component.
/// </summary>
/// <remarks>
/// Radial chart variants (also known as radial bar or gauge charts) control
/// the arc style and visual effects. These are ideal for showing progress
/// or percentage completion.
/// </remarks>
public enum RadialChartVariant
{
    /// <summary>
    /// Full circle radial bars.
    /// Shows values as arcs around a complete circle.
    /// </summary>
    Default,

    /// <summary>
    /// Semi-circle (180 degree) gauge style.
    /// Classic gauge appearance for single metrics.
    /// </summary>
    SemiCircle,

    /// <summary>
    /// Gauge with tick marks and value display.
    /// Mimics a physical gauge or speedometer.
    /// </summary>
    Gauge,

    /// <summary>
    /// Radial bars with gradient fill.
    /// Adds visual depth and modern appearance.
    /// </summary>
    Gradient
}
