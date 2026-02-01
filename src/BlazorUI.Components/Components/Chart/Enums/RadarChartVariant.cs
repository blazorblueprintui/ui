namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the visual variant for a RadarChart component.
/// </summary>
/// <remarks>
/// Radar chart variants control the fill style and grid appearance.
/// Radar charts are ideal for comparing multiple variables across categories.
/// </remarks>
public enum RadarChartVariant
{
    /// <summary>
    /// Standard radar chart with lines connecting points.
    /// Shows the outline of values across dimensions.
    /// </summary>
    Default,

    /// <summary>
    /// Radar chart with polygon fill showing the area.
    /// Emphasizes the overall "shape" of the data.
    /// </summary>
    PolygonFill,

    /// <summary>
    /// Multiple overlapping radar series for comparison.
    /// Ideal for comparing profiles across entities.
    /// </summary>
    MultiSeries
}
