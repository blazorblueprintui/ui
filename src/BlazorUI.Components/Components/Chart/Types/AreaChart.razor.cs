using ApexCharts;
using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Chart;

/// <summary>
/// An area chart component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The AreaChart component provides area-based visualizations that emphasize
/// volume or magnitude. It supports stacking for part-to-whole comparisons
/// and integrates with the BlazorUI theme system.
/// </para>
/// <para>
/// Features:
/// - 4 variants (Default, Spline, Stacked, Stepline)
/// - Multiple series support with gradient fills
/// - CSS variable theming (--chart-1 through --chart-5)
/// - Interactive tooltips
/// - Dark mode support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;AreaChart TItem="RevenueData"
///            Items="@revenueData"
///            Variant="AreaChartVariant.Stacked"&gt;
///     &lt;ApexPointSeries TItem="RevenueData"
///                      Items="@revenueData"
///                      Name="Product A"
///                      SeriesType="SeriesType.Area"
///                      XValue="@(item => item.Month)"
///                      YValue="@(item => item.ProductA)" /&gt;
/// &lt;/AreaChart&gt;
/// </code>
/// </example>
/// <typeparam name="TItem">The type of data items used in the chart.</typeparam>
public partial class AreaChart<TItem> : ChartBase<TItem> where TItem : class
{
    /// <summary>
    /// Gets or sets the area chart variant.
    /// </summary>
    /// <remarks>
    /// Controls the line interpolation style and stacking behavior.
    /// Default is <see cref="AreaChartVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public AreaChartVariant Variant { get; set; } = AreaChartVariant.Default;

    /// <summary>
    /// Gets or sets the series content for the chart.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the line stroke width.
    /// </summary>
    /// <remarks>
    /// Width of the area border line in pixels. Default is 2.
    /// </remarks>
    [Parameter]
    public int StrokeWidth { get; set; } = 2;

    /// <summary>
    /// Gets or sets the fill opacity.
    /// </summary>
    /// <remarks>
    /// Opacity of the area fill (0.0 to 1.0). Default is 0.4.
    /// </remarks>
    [Parameter]
    public double FillOpacity { get; set; } = 0.4;

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
            Width = StrokeWidth
        };

        // Configure fill with gradient
        _options.Fill = new Fill
        {
            Type = FillType.Gradient,
            Gradient = new FillGradient
            {
                ShadeIntensity = 1,
                OpacityFrom = FillOpacity,
                OpacityTo = 0.1,
                Stops = [0, 90, 100]
            }
        };

        // Configure stacking for stacked variant
        if (Variant == AreaChartVariant.Stacked)
        {
            _options.Chart!.Stacked = true;
            _options.Chart.StackType = StackType.Normal;
        }

        // Set colors from config or defaults
        _options.Colors = ChartColor.DefaultColors.ToList();

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
            AreaChartVariant.Spline => Curve.Smooth,
            AreaChartVariant.Stepline => Curve.Stepline,
            _ => Curve.Straight
        };
    }
}
