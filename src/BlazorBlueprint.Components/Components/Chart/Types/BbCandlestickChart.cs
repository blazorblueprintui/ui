namespace BlazorBlueprint.Components;

/// <summary>
/// A candlestick chart component using the declarative composition API with Apache ECharts.
/// </summary>
/// <remarks>
/// <para>
/// The CandlestickChart component provides financial OHLC (Open-High-Low-Close) candlestick
/// visualizations. Bullish and bearish candles are automatically color-coded.
/// </para>
/// <para>
/// Usage:
/// <code>
/// &lt;BbCandlestickChart Data="@data" Height="350px"&gt;
///     &lt;BbXAxis DataKey="date" /&gt;
///     &lt;BbYAxis /&gt;
///     &lt;BbChartTooltip /&gt;
///     &lt;BbCandlestick OpenKey="open" CloseKey="close" HighKey="high" LowKey="low" /&gt;
/// &lt;/BbCandlestickChart&gt;
/// </code>
/// </para>
/// </remarks>
public class BbCandlestickChart : BbChartBase
{
    internal override string DataSlot => "candlestick-chart";

    internal override string SeriesType => "candlestick";

    internal override void ApplyChartDefaults(EChartsOption option)
    {
        // Candlestick uses itemStyle.color/color0 for bull/bear colors.
        // Prevent the global color palette from overriding series itemStyle.
        option.Color = new List<object> { "transparent" };

        option.Grid ??= new EChartsGridOption
        {
            Left = "12",
            Right = "12",
            Top = "10",
            Bottom = "0",
            ContainLabel = true
        };

        option.YAxis ??= new EChartsAxisOption
        {
            Type = "value",
            Scale = true,
            SplitLine = new EChartsSplitLineOption
            {
                Show = true,
                LineStyle = new EChartsLineStyleOption
                {
                    Color = "var(--border)",
                    Type = "dashed"
                }
            },
            AxisLabel = new EChartsAxisLabelOption
            {
                Color = "var(--muted-foreground)"
            }
        };
    }
}
