namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the visual variant for an AreaChart component.
/// </summary>
/// <remarks>
/// Area chart variants control the interpolation style and stacking behavior.
/// The filled area emphasizes volume or magnitude over time.
/// </remarks>
public enum AreaChartVariant
{
    /// <summary>
    /// Standard area chart with straight lines.
    /// Shows magnitude/volume with direct line connections.
    /// </summary>
    Default,

    /// <summary>
    /// Smooth curved area using spline interpolation.
    /// Creates a more fluid, organic appearance.
    /// </summary>
    Spline,

    /// <summary>
    /// Stacked areas showing part-to-whole relationships.
    /// Multiple series stack on top of each other.
    /// </summary>
    Stacked,

    /// <summary>
    /// Step area chart with horizontal-then-vertical lines.
    /// Shows discrete changes or constant values between points.
    /// </summary>
    Stepline
}
