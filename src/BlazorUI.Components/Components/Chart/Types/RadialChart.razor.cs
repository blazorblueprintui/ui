using ApexCharts;
using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Chart;

/// <summary>
/// A radial bar/gauge chart component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The RadialChart component (also known as radial bar or gauge) displays
/// values as circular arcs around a center point. It's ideal for showing
/// progress, completion percentages, or comparative values.
/// </para>
/// <para>
/// Features:
/// - 4 variants (Default, SemiCircle, Gauge, Gradient)
/// - CSS variable theming (--chart-1 through --chart-5)
/// - Center label and value display
/// - Customizable start/end angles
/// - Dark mode support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;RadialChart TItem="ProgressData"
///              Items="@progressData"
///              Variant="RadialChartVariant.SemiCircle"
///              CenterLabel="Complete"&gt;
///     &lt;ApexPointSeries TItem="ProgressData"
///                      Items="@progressData"
///                      Name="Progress"
///                      SeriesType="SeriesType.RadialBar"
///                      XValue="@(item => item.Label)"
///                      YValue="@(item => item.Percentage)" /&gt;
/// &lt;/RadialChart&gt;
/// </code>
/// </example>
/// <typeparam name="TItem">The type of data items used in the chart.</typeparam>
public partial class RadialChart<TItem> : ChartBase<TItem> where TItem : class
{
    /// <summary>
    /// Gets or sets the radial chart variant.
    /// </summary>
    /// <remarks>
    /// Controls the arc style and visual effects.
    /// Default is <see cref="RadialChartVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public RadialChartVariant Variant { get; set; } = RadialChartVariant.Default;

    /// <summary>
    /// Gets or sets the series content for the chart.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the text to display in the center.
    /// </summary>
    /// <remarks>
    /// Displayed in the center of the radial chart, typically as a label.
    /// </remarks>
    [Parameter]
    public string? CenterLabel { get; set; }

    /// <summary>
    /// Gets or sets the value to display in the center.
    /// </summary>
    /// <remarks>
    /// Displayed larger than the label, typically shows a total or percentage.
    /// </remarks>
    [Parameter]
    public string? CenterValue { get; set; }

    /// <summary>
    /// Gets or sets the starting angle in degrees.
    /// </summary>
    /// <remarks>
    /// Default varies by variant. Full circle: -90, Semi-circle: -90.
    /// </remarks>
    [Parameter]
    public int? StartAngle { get; set; }

    /// <summary>
    /// Gets or sets the ending angle in degrees.
    /// </summary>
    /// <remarks>
    /// Default varies by variant. Full circle: 270, Semi-circle: 90.
    /// </remarks>
    [Parameter]
    public int? EndAngle { get; set; }

    /// <summary>
    /// Gets or sets the inner hollow radius percentage.
    /// </summary>
    /// <remarks>
    /// Size of the hollow center (0-100). Default is 55.
    /// </remarks>
    [Parameter]
    public int HollowSize { get; set; } = 55;

    /// <summary>
    /// Gets or sets the track background color.
    /// </summary>
    /// <remarks>
    /// Color of the unfilled portion of the radial bar.
    /// Default uses the muted theme color.
    /// </remarks>
    [Parameter]
    public string TrackBackground { get; set; } = "var(--muted)";

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

        // Get angles based on variant
        var (startAngle, endAngle) = GetAngles();

        // Configure radial bar specific options
        _options.PlotOptions = new PlotOptions
        {
            RadialBar = new PlotOptionsRadialBar
            {
                StartAngle = startAngle,
                EndAngle = endAngle,
                Hollow = new Hollow
                {
                    Size = $"{HollowSize}%",
                    Background = "transparent"
                },
                Track = new Track
                {
                    Background = TrackBackground
                },
                DataLabels = new RadialBarDataLabels
                {
                    Show = !string.IsNullOrEmpty(CenterLabel) || !string.IsNullOrEmpty(CenterValue),
                    Name = new RadialBarDataLabelsName
                    {
                        Show = !string.IsNullOrEmpty(CenterLabel),
                        FontSize = "14px",
                        Color = "var(--muted-foreground)",
                        OffsetY = string.IsNullOrEmpty(CenterValue) ? 0 : -10
                    },
                    Value = new RadialBarDataLabelsValue
                    {
                        Show = !string.IsNullOrEmpty(CenterValue),
                        FontSize = "28px",
                        Color = "var(--foreground)",
                        FontWeight = "600",
                        OffsetY = 5
                    },
                    Total = new RadialBarDataLabelsTotal
                    {
                        Show = !string.IsNullOrEmpty(CenterLabel) || !string.IsNullOrEmpty(CenterValue),
                        Label = CenterLabel ?? "",
                        Color = "var(--muted-foreground)"
                    }
                }
            }
        };

        // Configure fill based on variant
        if (Variant == RadialChartVariant.Gradient)
        {
            _options.Fill = new Fill
            {
                Type = FillType.Gradient,
                Gradient = new FillGradient
                {
                    Shade = GradientShade.Dark,
                    Type = GradientType.Horizontal,
                    ShadeIntensity = 0.5,
                    GradientToColors = ChartColor.DefaultColors.Skip(1).Take(1).ToList(),
                    OpacityFrom = 1,
                    OpacityTo = 0.8,
                    Stops = [0, 100]
                }
            };
        }

        // Stroke
        _options.Stroke = new Stroke
        {
            LineCap = LineCap.Round
        };
    }

    private (int StartAngle, int EndAngle) GetAngles()
    {
        // Use custom angles if provided
        if (StartAngle.HasValue && EndAngle.HasValue)
        {
            return (StartAngle.Value, EndAngle.Value);
        }

        return Variant switch
        {
            RadialChartVariant.SemiCircle => (-90, 90),
            RadialChartVariant.Gauge => (-135, 135),
            _ => (-90, 270) // Full circle
        };
    }
}
