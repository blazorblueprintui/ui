namespace BlazorBlueprint.Components.Chart;

/// <summary>
/// Provides utilities for working with chart colors and CSS variables.
/// </summary>
/// <remarks>
/// <para>
/// BlazorBlueprint uses CSS custom properties for chart colors, allowing seamless
/// integration with theme systems and dark mode support. The default chart
/// colors (--chart-1 through --chart-5) are defined in OKLCH color space
/// for perceptual uniformity.
/// </para>
/// <para>
/// Color variables are defined in the BlazorBlueprint Theme CSS:
/// - Light mode uses vibrant, distinct hues
/// - Dark mode adjusts lightness for visibility on dark backgrounds
/// </para>
/// </remarks>
public static class ChartColor
{
    /// <summary>
    /// Default chart color CSS variables in order.
    /// </summary>
    /// <remarks>
    /// These variables correspond to --chart-1 through --chart-5 defined in the theme.
    /// Additional colors cycle back to the beginning with modified opacity.
    /// </remarks>
    public static readonly string[] DefaultColors =
    [
        "var(--chart-1)",
        "var(--chart-2)",
        "var(--chart-3)",
        "var(--chart-4)",
        "var(--chart-5)"
    ];

    /// <summary>
    /// Gets the default chart color for the specified index.
    /// </summary>
    /// <param name="index">The series index (0-based).</param>
    /// <returns>A CSS color value (custom property reference).</returns>
    /// <remarks>
    /// Colors cycle through the 5 default chart colors. For indices beyond 4,
    /// colors repeat from the beginning.
    /// </remarks>
    public static string GetDefault(int index)
    {
        var normalizedIndex = Math.Abs(index) % DefaultColors.Length;
        return DefaultColors[normalizedIndex];
    }

    /// <summary>
    /// Chart color 1 - Primary chart color (blue/cyan family).
    /// </summary>
    public const string Chart1 = "var(--chart-1)";

    /// <summary>
    /// Chart color 2 - Secondary chart color (green/teal family).
    /// </summary>
    public const string Chart2 = "var(--chart-2)";

    /// <summary>
    /// Chart color 3 - Tertiary chart color (gold/yellow family).
    /// </summary>
    public const string Chart3 = "var(--chart-3)";

    /// <summary>
    /// Chart color 4 - Quaternary chart color (purple/violet family).
    /// </summary>
    public const string Chart4 = "var(--chart-4)";

    /// <summary>
    /// Chart color 5 - Quinary chart color (coral/red family).
    /// </summary>
    public const string Chart5 = "var(--chart-5)";

    /// <summary>
    /// Creates an array of colors for the specified number of series.
    /// </summary>
    /// <param name="count">The number of series requiring colors.</param>
    /// <returns>An array of CSS color values.</returns>
    public static string[] GetColors(int count)
    {
        var colors = new string[count];
        for (var i = 0; i < count; i++)
        {
            colors[i] = GetDefault(i);
        }
        return colors;
    }
}
