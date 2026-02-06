using ApexCharts;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Chart;

/// <summary>
/// Abstract base class for all BlazorBlueprint chart components.
/// </summary>
/// <remarks>
/// <para>
/// ChartBase provides common functionality shared across all chart types including:
/// - Configuration mapping with <see cref="ChartConfig"/>
/// - Common parameters (Height, Width, Class, ShowLegend, etc.)
/// - Theme integration via CSS custom properties
/// - ApexCharts options configuration
/// </para>
/// </remarks>
/// <typeparam name="TItem">The type of data items used in the chart.</typeparam>
public abstract class ChartBase<TItem> : ComponentBase where TItem : class
{
    /// <summary>
    /// Gets or sets the data items to display in the chart.
    /// </summary>
    /// <remarks>
    /// The collection of data objects that will be visualized.
    /// Each item typically contains X-axis values and one or more Y-axis values.
    /// </remarks>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Gets or sets the chart configuration for series labels and colors.
    /// </summary>
    /// <remarks>
    /// Maps series names to their display labels and colors.
    /// If not provided, default colors from --chart-1 through --chart-5 are used.
    /// </remarks>
    [Parameter]
    public ChartConfig? Config { get; set; }

    /// <summary>
    /// Gets or sets the height of the chart.
    /// </summary>
    /// <remarks>
    /// Accepts any valid CSS height value (e.g., "350px", "100%", "20rem").
    /// Default is "350px".
    /// </remarks>
    [Parameter]
    public string Height { get; set; } = "350px";

    /// <summary>
    /// Gets or sets the width of the chart.
    /// </summary>
    /// <remarks>
    /// Accepts any valid CSS width value (e.g., "100%", "500px").
    /// Default is "100%" to fill the container.
    /// </remarks>
    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the chart container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets whether to show the chart legend.
    /// </summary>
    /// <remarks>
    /// Default is true. Set to false to hide the legend and maximize chart area.
    /// </remarks>
    [Parameter]
    public bool ShowLegend { get; set; } = true;

    /// <summary>
    /// Gets or sets the position of the legend.
    /// </summary>
    /// <remarks>
    /// Controls where the legend is displayed relative to the chart.
    /// Default is <see cref="LegendPosition.Bottom"/>.
    /// </remarks>
    [Parameter]
    public LegendPosition LegendPosition { get; set; } = LegendPosition.Bottom;

    /// <summary>
    /// Gets or sets whether to show data labels on the chart.
    /// </summary>
    /// <remarks>
    /// Data labels display values directly on chart elements.
    /// May clutter the chart with many data points.
    /// </remarks>
    [Parameter]
    public bool ShowDataLabels { get; set; }

    /// <summary>
    /// Gets or sets whether to show the tooltip on hover.
    /// </summary>
    /// <remarks>
    /// Default is true. Tooltips show detailed information on hover.
    /// </remarks>
    [Parameter]
    public bool ShowTooltip { get; set; } = true;

    /// <summary>
    /// Gets or sets the chart title.
    /// </summary>
    /// <remarks>
    /// Displayed at the top of the chart. Leave null for no title.
    /// </remarks>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets whether to enable animations.
    /// </summary>
    /// <remarks>
    /// Default is true. Set to false for better performance with large datasets.
    /// </remarks>
    [Parameter]
    public bool EnableAnimations { get; set; } = true;

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the chart container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the colors array for the chart series.
    /// </summary>
    /// <param name="seriesCount">The number of series to get colors for.</param>
    /// <returns>An array of color strings.</returns>
    protected string[] GetSeriesColors(int seriesCount) =>
        ChartColor.GetColors(seriesCount);

    /// <summary>
    /// Gets the label for a series from the config.
    /// </summary>
    /// <param name="seriesKey">The series key.</param>
    /// <returns>The display label.</returns>
    protected string GetSeriesLabel(string seriesKey) =>
        Config?.GetLabel(seriesKey) ?? seriesKey;

    /// <summary>
    /// Gets the color for a series from the config.
    /// </summary>
    /// <param name="seriesKey">The series key.</param>
    /// <param name="index">The series index for default color fallback.</param>
    /// <returns>The color string.</returns>
    protected string GetSeriesColor(string seriesKey, int index) =>
        Config?.GetColor(seriesKey, index) ?? ChartColor.GetDefault(index);

    /// <summary>
    /// Converts LegendPosition enum to ApexCharts LegendPosition.
    /// </summary>
    protected ApexCharts.LegendPosition GetApexLegendPosition()
    {
        return LegendPosition switch
        {
            LegendPosition.Top => ApexCharts.LegendPosition.Top,
            LegendPosition.Bottom => ApexCharts.LegendPosition.Bottom,
            LegendPosition.Left => ApexCharts.LegendPosition.Left,
            LegendPosition.Right => ApexCharts.LegendPosition.Right,
            _ => ApexCharts.LegendPosition.Bottom
        };
    }

    /// <summary>
    /// Creates base ApexChartOptions with common settings.
    /// </summary>
    protected ApexChartOptions<TItem> CreateBaseOptions()
    {
        var options = new ApexChartOptions<TItem>();

        // Chart settings
        options.Chart = new ApexCharts.Chart
        {
            Height = Height,
            Width = Width,
            Animations = new Animations
            {
                Enabled = EnableAnimations
            },
            Toolbar = new Toolbar
            {
                Show = false
            },
            Background = "transparent"
        };

        // Legend
        options.Legend = new Legend
        {
            Show = ShowLegend && LegendPosition != LegendPosition.Hidden,
            Position = GetApexLegendPosition(),
            Labels = new LegendLabels
            {
                Colors = "var(--foreground)"
            }
        };

        // Tooltip - use cssClass for theme-aware styling
        options.Tooltip = new ApexCharts.Tooltip
        {
            Enabled = ShowTooltip,
            CssClass = "blazorblueprint-chart-tooltip"
        };

        // Data labels
        options.DataLabels = new DataLabels
        {
            Enabled = ShowDataLabels
        };

        // Title
        if (!string.IsNullOrEmpty(Title))
        {
            options.Title = new Title
            {
                Text = Title,
                Align = Align.Center
            };
        }

        return options;
    }
}
