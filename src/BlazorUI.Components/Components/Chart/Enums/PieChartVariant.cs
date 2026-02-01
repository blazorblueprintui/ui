namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the visual variant for a PieChart component.
/// </summary>
/// <remarks>
/// Pie chart variants control whether the chart is a full pie, donut,
/// or has special visual effects like gradients.
/// </remarks>
public enum PieChartVariant
{
    /// <summary>
    /// Standard pie chart showing proportional slices.
    /// Best for showing parts of a whole (5-7 categories max).
    /// </summary>
    Pie,

    /// <summary>
    /// Donut chart with a hollow center.
    /// Allows displaying a summary value or label in the center.
    /// </summary>
    Donut,

    /// <summary>
    /// Donut chart with gradient fill on slices.
    /// Adds visual depth and modern appearance.
    /// </summary>
    GradientDonut
}
