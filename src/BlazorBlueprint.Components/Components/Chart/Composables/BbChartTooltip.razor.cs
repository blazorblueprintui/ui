using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Configures the tooltip for a chart. Must be placed inside a chart component.
/// </summary>
/// <remarks>
/// <para>
/// ChartTooltip is named to avoid collision with the existing Primitives Tooltip component.
/// It auto-detects the trigger type based on the chart's series type: "item" for pie and radar
/// charts, "axis" for all other chart types.
/// </para>
/// <para>
/// Default styling uses design system CSS variables for consistent theming with the rest
/// of the application.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BarChart Data="@data"&gt;
///     &lt;ChartTooltip Indicator="TooltipIndicator.Line" /&gt;
///     &lt;XAxis DataKey="month" /&gt;
///     &lt;YAxis /&gt;
/// &lt;/BarChart&gt;
/// </code>
/// </example>
public partial class BbChartTooltip : ComponentBase, IChartComponent, IDisposable
{
    [CascadingParameter]
    private BbChartBase? ParentChart { get; set; }

    /// <summary>
    /// Gets or sets the tooltip indicator style.
    /// </summary>
    /// <remarks>
    /// Controls how the axis pointer is rendered. <see cref="TooltipIndicator.Dot"/> uses a shadow
    /// pointer, <see cref="TooltipIndicator.Line"/> and <see cref="TooltipIndicator.Dashed"/> use line
    /// pointers, and <see cref="TooltipIndicator.None"/> hides the pointer entirely.
    /// </remarks>
    [Parameter]
    public TooltipIndicator Indicator { get; set; } = TooltipIndicator.Dot;

    /// <summary>
    /// Gets or sets the tooltip background color.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>var(--popover)</c> when not specified, matching the design system popover background.
    /// </remarks>
    [Parameter]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the tooltip border color.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>var(--border)</c> when not specified, matching the design system border color.
    /// </remarks>
    [Parameter]
    public string? BorderColor { get; set; }

    /// <summary>
    /// Gets or sets the tooltip text color.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>var(--popover-foreground)</c> when not specified, matching the design system popover text color.
    /// </remarks>
    [Parameter]
    public string? TextColor { get; set; }

    protected override void OnInitialized() =>
        ParentChart?.RegisterComponent(this);

    void IChartComponent.ApplyTo(EChartsOption option)
    {
        var trigger = DetectTrigger();

        var tooltip = new EChartsTooltipOption
        {
            Trigger = trigger,
            BackgroundColor = BackgroundColor ?? "var(--popover)",
            BorderColor = BorderColor ?? "var(--border)",
            TextStyle = new EChartsTextStyleOption
            {
                Color = TextColor ?? "var(--popover-foreground)"
            },
            ExtraCssText = "border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(0,0,0,.1), 0 2px 4px -2px rgba(0,0,0,.1);"
        };

        if (trigger == "axis")
        {
            tooltip.AxisPointer = BuildAxisPointer();
        }

        option.Tooltip = tooltip;
    }

    private string DetectTrigger()
    {
        if (ParentChart == null)
        {
            return "axis";
        }

        var seriesType = ParentChart.SeriesType;
        if (seriesType is "pie" or "radar" or "funnel" or "gauge")
        {
            return "item";
        }

        return "axis";
    }

    private EChartsAxisPointerOption BuildAxisPointer()
    {
        return Indicator switch
        {
            TooltipIndicator.Line => new EChartsAxisPointerOption
            {
                Type = "line",
                LineStyle = new EChartsLineStyleOption
                {
                    Color = "var(--border)",
                    Width = 1,
                    Type = "solid"
                }
            },
            TooltipIndicator.Dashed => new EChartsAxisPointerOption
            {
                Type = "line",
                LineStyle = new EChartsLineStyleOption
                {
                    Color = "var(--border)",
                    Width = 1,
                    Type = "dashed"
                }
            },
            TooltipIndicator.None => new EChartsAxisPointerOption
            {
                Type = "none"
            },
            _ => new EChartsAxisPointerOption
            {
                Type = "shadow"
            }
        };
    }

    public void Dispose()
    {
        ParentChart?.UnregisterComponent(this);
        GC.SuppressFinalize(this);
    }
}
