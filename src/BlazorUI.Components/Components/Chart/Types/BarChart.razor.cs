using ApexCharts;
using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Chart;

/// <summary>
/// A bar chart component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The BarChart component provides vertical and horizontal bar visualizations
/// with support for grouped and stacked layouts. It integrates with the
/// BlazorUI theme system through CSS custom properties.
/// </para>
/// <para>
/// Features:
/// - 7 variants (Vertical, Horizontal, Stacked, StackedHorizontal, FullStacked, FullStackedHorizontal, Grouped)
/// - Multiple series support
/// - CSS variable theming (--chart-1 through --chart-5)
/// - Interactive tooltips
/// - Responsive design
/// - Dark mode support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BarChart TItem="SalesData"
///           Items="@salesData"
///           XValue="@(item => item.Month)"
///           Variant="BarChartVariant.Stacked"&gt;
///     &lt;ApexPointSeries TItem="SalesData"
///                       Items="@salesData"
///                       Name="Desktop"
///                       SeriesType="SeriesType.Bar"
///                       XValue="@(item => item.Month)"
///                       YValue="@(item => item.Desktop)" /&gt;
///     &lt;ApexPointSeries TItem="SalesData"
///                       Items="@salesData"
///                       Name="Mobile"
///                       SeriesType="SeriesType.Bar"
///                       XValue="@(item => item.Month)"
///                       YValue="@(item => item.Mobile)" /&gt;
/// &lt;/BarChart&gt;
/// </code>
/// </example>
/// <typeparam name="TItem">The type of data items used in the chart.</typeparam>
public partial class BarChart<TItem> : ChartBase<TItem> where TItem : class
{
    /// <summary>
    /// Gets or sets the bar chart variant.
    /// </summary>
    /// <remarks>
    /// Controls the orientation (vertical/horizontal) and stacking behavior.
    /// Default is <see cref="BarChartVariant.Vertical"/>.
    /// </remarks>
    [Parameter]
    public BarChartVariant Variant { get; set; } = BarChartVariant.Vertical;

    /// <summary>
    /// Gets or sets the series content for the chart.
    /// </summary>
    /// <remarks>
    /// Use ApexPointSeries components to define each data series.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the corner radius for bars.
    /// </summary>
    /// <remarks>
    /// Adds rounded corners to bar ends. Default is 4 pixels.
    /// </remarks>
    [Parameter]
    public int BorderRadius { get; set; } = 4;

    /// <summary>
    /// Gets or sets the width of bars as a percentage of available space.
    /// </summary>
    /// <remarks>
    /// Value between 0 and 100. Default is 70%.
    /// </remarks>
    [Parameter]
    public int BarWidth { get; set; } = 70;

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

        // Configure bar-specific options
        _options.PlotOptions = new PlotOptions
        {
            Bar = new PlotOptionsBar
            {
                Horizontal = IsHorizontal(),
                BorderRadius = BorderRadius,
                ColumnWidth = $"{BarWidth}%",
                BarHeight = $"{BarWidth}%"
            }
        };

        // Configure stacking
        var (stacked, stackType) = GetStackingConfig();
        _options.Chart!.Stacked = stacked;
        _options.Chart.StackType = stackType;

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

    private bool IsHorizontal()
    {
        return Variant is BarChartVariant.Horizontal
            or BarChartVariant.StackedHorizontal
            or BarChartVariant.FullStackedHorizontal;
    }

    private (bool Stacked, StackType? StackType) GetStackingConfig()
    {
        return Variant switch
        {
            BarChartVariant.Stacked or BarChartVariant.StackedHorizontal => (true, ApexCharts.StackType.Normal),
            BarChartVariant.FullStacked or BarChartVariant.FullStackedHorizontal => (true, ApexCharts.StackType.Percent100),
            _ => (false, null)
        };
    }
}
