using ApexCharts;
using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Chart;

/// <summary>
/// A line chart component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The LineChart component provides line-based visualizations with support
/// for different interpolation styles and visual effects. It integrates with
/// the BlazorUI theme system through CSS custom properties.
/// </para>
/// <para>
/// Features:
/// - 5 variants (Default, Spline, Stepline, Dashed, Gradient)
/// - Multiple series support
/// - CSS variable theming (--chart-1 through --chart-5)
/// - Interactive tooltips with crosshairs
/// - Markers on data points
/// - Dark mode support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;LineChart TItem="TrendData"
///            Items="@trendData"
///            Variant="LineChartVariant.Spline"&gt;
///     &lt;ApexPointSeries TItem="TrendData"
///                      Items="@trendData"
///                      Name="Users"
///                      SeriesType="SeriesType.Line"
///                      XValue="@(item => item.Date)"
///                      YValue="@(item => item.Users)" /&gt;
/// &lt;/LineChart&gt;
/// </code>
/// </example>
/// <typeparam name="TItem">The type of data items used in the chart.</typeparam>
public partial class LineChart<TItem> : ChartBase<TItem> where TItem : class
{
    /// <summary>
    /// Gets or sets the line chart variant.
    /// </summary>
    /// <remarks>
    /// Controls the line interpolation style and visual effects.
    /// Default is <see cref="LineChartVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public LineChartVariant Variant { get; set; } = LineChartVariant.Default;

    /// <summary>
    /// Gets or sets the series content for the chart.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the line stroke width.
    /// </summary>
    /// <remarks>
    /// Width of the line in pixels. Default is 2.
    /// </remarks>
    [Parameter]
    public int StrokeWidth { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether to show markers at data points.
    /// </summary>
    /// <remarks>
    /// Default is true. Set to false for cleaner lines with many data points.
    /// </remarks>
    [Parameter]
    public bool ShowMarkers { get; set; } = true;

    /// <summary>
    /// Gets or sets the marker size.
    /// </summary>
    /// <remarks>
    /// Size of markers in pixels. Default is 4.
    /// </remarks>
    [Parameter]
    public int MarkerSize { get; set; } = 4;

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

        // Configure stroke based on variant
        _options.Stroke = new Stroke
        {
            Curve = GetCurveType(),
            Width = StrokeWidth,
            DashArray = Variant == LineChartVariant.Dashed ? 5 : 0
        };

        // Configure markers
        _options.Markers = new Markers
        {
            Size = ShowMarkers ? MarkerSize : 0,
            StrokeWidth = 0,
            Hover = new MarkersHover
            {
                Size = ShowMarkers ? MarkerSize + 2 : 0
            }
        };

        // Set colors from config or defaults
        _options.Colors = ChartColor.DefaultColors.ToList();

        // Configure fill for gradient variant
        if (Variant == LineChartVariant.Gradient)
        {
            _options.Fill = new Fill
            {
                Type = FillType.Gradient,
                Gradient = new FillGradient
                {
                    ShadeIntensity = 1,
                    OpacityFrom = 0.4,
                    OpacityTo = 0.1,
                    Stops = [0, 90, 100]
                }
            };
        }

        // Grid styling
        _options.Grid = new Grid
        {
            BorderColor = "var(--border)",
            StrokeDashArray = 4
        };

        // X-Axis styling
        _options.Xaxis = new XAxis
        {
            Labels = new XAxisLabels
            {
                Style = new AxisLabelStyle
                {
                    Colors = "var(--muted-foreground)"
                }
            },
            AxisBorder = new AxisBorder
            {
                Show = false
            },
            AxisTicks = new AxisTicks
            {
                Show = false
            },
            Crosshairs = new AxisCrosshairs
            {
                Show = true
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

    private Curve GetCurveType()
    {
        return Variant switch
        {
            LineChartVariant.Spline or LineChartVariant.Gradient => Curve.Smooth,
            LineChartVariant.Stepline => Curve.Stepline,
            _ => Curve.Straight
        };
    }
}
