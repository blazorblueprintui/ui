using ApexCharts;
using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Chart;

/// <summary>
/// A radar chart component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The RadarChart component (also known as spider or web chart) displays
/// multivariate data on axes starting from the same point. It's ideal for
/// comparing multiple variables across entities.
/// </para>
/// <para>
/// Features:
/// - 3 variants (Default, PolygonFill, MultiSeries)
/// - CSS variable theming (--chart-1 through --chart-5)
/// - Polygon or circle grid options
/// - Interactive tooltips
/// - Dark mode support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;RadarChart TItem="SkillScore"
///             Items="@skillData"
///             Variant="RadarChartVariant.PolygonFill"&gt;
///     &lt;ApexPointSeries TItem="SkillScore"
///                      Items="@skillData"
///                      Name="Score"
///                      SeriesType="SeriesType.Radar"
///                      XValue="@(item => item.Skill)"
///                      YValue="@(item => item.Score)" /&gt;
/// &lt;/RadarChart&gt;
/// </code>
/// </example>
/// <typeparam name="TItem">The type of data items used in the chart.</typeparam>
public partial class RadarChart<TItem> : ChartBase<TItem> where TItem : class
{
    /// <summary>
    /// Gets or sets the radar chart variant.
    /// </summary>
    /// <remarks>
    /// Controls the fill style and grid appearance.
    /// Default is <see cref="RadarChartVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public RadarChartVariant Variant { get; set; } = RadarChartVariant.Default;

    /// <summary>
    /// Gets or sets the series content for the chart.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets whether to show polygon markers at data points.
    /// </summary>
    /// <remarks>
    /// Default is true. Shows dots at each vertex of the radar shape.
    /// </remarks>
    [Parameter]
    public bool ShowMarkers { get; set; } = true;

    /// <summary>
    /// Gets or sets the marker size.
    /// </summary>
    [Parameter]
    public int MarkerSize { get; set; } = 4;

    /// <summary>
    /// Gets or sets the fill opacity for filled variants.
    /// </summary>
    /// <remarks>
    /// Opacity of the fill area (0.0 to 1.0). Default is 0.25.
    /// </remarks>
    [Parameter]
    public double FillOpacity { get; set; } = 0.25;

    private ApexChartOptions<TItem> _options = new();

    /// <summary>
    /// Gets the computed CSS classes for the chart container.
    /// </summary>
    private string ContainerCssClass => ClassNames.cn(
        "w-full",
        Class
    );

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        ConfigureOptions();
    }

    private void ConfigureOptions()
    {
        _options = CreateBaseOptions();

        // Set colors from config or defaults
        _options.Colors = ChartColor.DefaultColors.ToList();

        // Configure radar-specific plot options
        _options.PlotOptions = new PlotOptions
        {
            Radar = new PlotOptionsRadar
            {
                Polygons = new RadarPolygons
                {
                    StrokeColors = "var(--border)",
                    ConnectorColors = "var(--border)"
                }
            }
        };

        // Configure fill based on variant
        if (Variant == RadarChartVariant.PolygonFill || Variant == RadarChartVariant.MultiSeries)
        {
            _options.Fill = new Fill
            {
                Opacity = FillOpacity
            };
        }
        else
        {
            _options.Fill = new Fill
            {
                Opacity = 0
            };
        }

        // Stroke configuration
        _options.Stroke = new Stroke
        {
            Width = 2
        };

        // Markers
        _options.Markers = new Markers
        {
            Size = ShowMarkers ? MarkerSize : 0
        };

        // X-Axis (categories) styling
        _options.Xaxis = new XAxis
        {
            Labels = new XAxisLabels
            {
                Style = new AxisLabelStyle
                {
                    Colors = "var(--muted-foreground)",
                    FontSize = "12px"
                }
            }
        };

        // Y-Axis styling
        _options.Yaxis =
        [
            new YAxis
            {
                Labels = new YAxisLabels
                {
                    Style = new AxisLabelStyle
                    {
                        Colors = "var(--muted-foreground)"
                    }
                }
            }
        ];
    }
}
