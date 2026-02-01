namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the visual variant for a BarChart component.
/// </summary>
/// <remarks>
/// Bar chart variants control the orientation and stacking behavior of bars.
/// Each variant serves different data visualization purposes.
/// </remarks>
public enum BarChartVariant
{
    /// <summary>
    /// Vertical bars (columns) with each series side-by-side.
    /// Best for comparing values across categories.
    /// </summary>
    Vertical,

    /// <summary>
    /// Horizontal bars with each series side-by-side.
    /// Ideal when category labels are long or numerous.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical stacked bars showing part-to-whole relationships.
    /// Values are stacked on top of each other.
    /// </summary>
    Stacked,

    /// <summary>
    /// Horizontal stacked bars showing part-to-whole relationships.
    /// Combines stacking with horizontal orientation.
    /// </summary>
    StackedHorizontal,

    /// <summary>
    /// Vertical 100% stacked bars showing proportions.
    /// Each bar totals to 100%, showing percentage contribution.
    /// </summary>
    FullStacked,

    /// <summary>
    /// Horizontal 100% stacked bars showing proportions.
    /// Combines percentage stacking with horizontal orientation.
    /// </summary>
    FullStackedHorizontal,

    /// <summary>
    /// Grouped vertical bars with series side-by-side.
    /// Explicit grouping behavior for multi-series comparison.
    /// </summary>
    Grouped
}
