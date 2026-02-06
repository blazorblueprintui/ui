using ApexCharts;
using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Chart;

/// <summary>
/// A pie/donut chart component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The PieChart component provides circular visualizations for showing
/// proportions and part-to-whole relationships. It supports both standard
/// pie and donut variants with optional gradient effects.
/// </para>
/// <para>
/// Features:
/// - 3 variants (Pie, Donut, GradientDonut)
/// - CSS variable theming (--chart-1 through --chart-5)
/// - Interactive tooltips
/// - Customizable center content for donut charts
/// - Dark mode support
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PieChart TItem="BrowserShare"
///           Items="@browserData"
///           Variant="PieChartVariant.Donut"&gt;
///     &lt;ApexPointSeries TItem="BrowserShare"
///                      Items="@browserData"
///                      Name="Share"
///                      SeriesType="SeriesType.Donut"
///                      XValue="@(item => item.Browser)"
///                      YValue="@(item => (decimal)item.Percentage)" /&gt;
/// &lt;/PieChart&gt;
/// </code>
/// </example>
/// <typeparam name="TItem">The type of data items used in the chart.</typeparam>
public partial class PieChart<TItem> : ChartBase<TItem> where TItem : class
{
    /// <summary>
    /// Gets or sets the pie chart variant.
    /// </summary>
    /// <remarks>
    /// Controls whether the chart is a full pie, donut, or donut with gradients.
    /// Default is <see cref="PieChartVariant.Pie"/>.
    /// </remarks>
    [Parameter]
    public PieChartVariant Variant { get; set; } = PieChartVariant.Pie;

    /// <summary>
    /// Gets or sets the series content for the chart.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the donut hole size as a percentage.
    /// </summary>
    /// <remarks>
    /// Size of the center hole in donut variants (0-100). Default is 55.
    /// Only applies to Donut and GradientDonut variants.
    /// </remarks>
    [Parameter]
    public int DonutSize { get; set; } = 55;

    /// <summary>
    /// Gets or sets the text to display in the donut center.
    /// </summary>
    /// <remarks>
    /// Only visible in Donut variants. Typically used for totals or labels.
    /// </remarks>
    [Parameter]
    public string? CenterLabel { get; set; }

    /// <summary>
    /// Gets or sets the value to display in the donut center.
    /// </summary>
    /// <remarks>
    /// Only visible in Donut variants. Displayed larger than the label.
    /// </remarks>
    [Parameter]
    public string? CenterValue { get; set; }

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

        // Configure pie/donut specific options
        _options.PlotOptions = new PlotOptions
        {
            Pie = new PlotOptionsPie
            {
                Donut = new PlotOptionsDonut
                {
                    Size = IsDonut() ? $"{DonutSize}%" : "0%",
                    Labels = new DonutLabels
                    {
                        Show = IsDonut() && (!string.IsNullOrEmpty(CenterLabel) || !string.IsNullOrEmpty(CenterValue)),
                        Name = new DonutLabelName
                        {
                            Show = !string.IsNullOrEmpty(CenterLabel),
                            FontSize = "12px",
                            Color = "var(--muted-foreground)"
                        },
                        Value = new DonutLabelValue
                        {
                            Show = !string.IsNullOrEmpty(CenterValue),
                            FontSize = "24px",
                            Color = "var(--foreground)",
                            FontWeight = "600"
                        },
                        Total = new DonutLabelTotal
                        {
                            Show = !string.IsNullOrEmpty(CenterLabel) || !string.IsNullOrEmpty(CenterValue),
                            ShowAlways = true,
                            Label = CenterLabel ?? "",
                            Color = "var(--muted-foreground)"
                        }
                    }
                }
            }
        };

        // Configure fill for gradient variant
        if (Variant == PieChartVariant.GradientDonut)
        {
            _options.Fill = new Fill
            {
                Type = FillType.Gradient,
                Gradient = new FillGradient
                {
                    Shade = GradientShade.Dark,
                    Type = GradientType.Horizontal,
                    ShadeIntensity = 0.5,
                    OpacityFrom = 1,
                    OpacityTo = 0.8,
                    Stops = [0, 100]
                }
            };
        }

        // Stroke between slices
        _options.Stroke = new Stroke
        {
            Width = 2,
            Colors = ["var(--background)"]
        };
    }

    private bool IsDonut() =>
        Variant is PieChartVariant.Donut or PieChartVariant.GradientDonut;
}
