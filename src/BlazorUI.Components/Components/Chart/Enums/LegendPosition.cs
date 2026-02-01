namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the position of the chart legend.
/// </summary>
/// <remarks>
/// Legend position affects chart layout and available space for the
/// visualization area. Consider the chart dimensions when choosing
/// position to maintain readability.
/// </remarks>
public enum LegendPosition
{
    /// <summary>
    /// Legend positioned at the top of the chart.
    /// Works well for wider charts.
    /// </summary>
    Top,

    /// <summary>
    /// Legend positioned at the bottom of the chart.
    /// Default position, keeps focus on the chart.
    /// </summary>
    Bottom,

    /// <summary>
    /// Legend positioned to the left of the chart.
    /// Good for taller charts with many series.
    /// </summary>
    Left,

    /// <summary>
    /// Legend positioned to the right of the chart.
    /// Traditional layout for dashboards.
    /// </summary>
    Right,

    /// <summary>
    /// No legend displayed.
    /// Use when series are self-explanatory or space is limited.
    /// </summary>
    Hidden
}
