namespace BlazorUI.Components.Chart;

/// <summary>
/// Configuration for a single chart series including its label and color.
/// </summary>
/// <remarks>
/// <para>
/// Used in conjunction with <see cref="ChartConfig"/> to define the appearance
/// and labeling of chart series. Colors can use CSS custom properties for
/// seamless integration with theme systems.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var config = new ChartSeriesConfig
/// {
///     Label = "Desktop Users",
///     Color = "var(--chart-1)"
/// };
/// </code>
/// </example>
public class ChartSeriesConfig
{
    /// <summary>
    /// Gets or sets the display label for this series.
    /// </summary>
    /// <remarks>
    /// This label is shown in tooltips, legends, and accessibility descriptions.
    /// Should be concise but descriptive.
    /// </remarks>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the color for this series.
    /// </summary>
    /// <remarks>
    /// Supports CSS custom properties (e.g., "var(--chart-1)"), hex colors,
    /// RGB/RGBA values, or any valid CSS color value.
    /// Default chart colors use --chart-1 through --chart-5 CSS variables
    /// which are defined in the BlazorUI theme system.
    /// </remarks>
    public string Color { get; set; } = string.Empty;
}
