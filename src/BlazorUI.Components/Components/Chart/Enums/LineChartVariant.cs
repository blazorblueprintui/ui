namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the visual variant for a LineChart component.
/// </summary>
/// <remarks>
/// Line chart variants control the interpolation style and visual effects of lines.
/// Different variants suit different types of data and presentation needs.
/// </remarks>
public enum LineChartVariant
{
    /// <summary>
    /// Straight lines connecting data points.
    /// Best for showing exact values and direct trends.
    /// </summary>
    Default,

    /// <summary>
    /// Smooth curved lines using spline interpolation.
    /// Creates a more fluid, organic appearance.
    /// </summary>
    Spline,

    /// <summary>
    /// Step lines that move horizontally then vertically.
    /// Ideal for discrete data or showing constant values between points.
    /// </summary>
    Stepline,

    /// <summary>
    /// Dashed lines for secondary or reference data.
    /// Useful for targets, thresholds, or comparison series.
    /// </summary>
    Dashed,

    /// <summary>
    /// Lines with a gradient fill beneath.
    /// Adds visual emphasis while showing the area under the curve.
    /// </summary>
    Gradient
}
