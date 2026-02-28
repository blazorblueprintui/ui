namespace BlazorBlueprint.Components;

/// <summary>
/// A general-purpose composite chart component that allows mixing any series types.
/// </summary>
/// <remarks>
/// <para>
/// The Chart component is a flexible container that supports combining different series
/// types (Bar, Line, Area, Scatter, etc.) in a single chart. It provides grid-based
/// defaults suitable for most composite visualizations.
/// </para>
/// <para>
/// Usage:
/// <code>
/// &lt;BbChart Data="@data" Height="350px"&gt;
///     &lt;BbXAxis DataKey="month" /&gt;
///     &lt;BbYAxis /&gt;
///     &lt;BbChartTooltip /&gt;
///     &lt;BbChartLegend /&gt;
///     &lt;BbBar DataKey="revenue" Name="Revenue" Color="var(--chart-1)" /&gt;
///     &lt;BbLine DataKey="profit" Name="Profit" Color="var(--chart-2)" Curve="CurveType.Smooth" /&gt;
///     &lt;BbArea DataKey="forecast" Name="Forecast" Color="var(--chart-3)" FillOpacity="0.2" /&gt;
/// &lt;/BbChart&gt;
/// </code>
/// </para>
/// </remarks>
public class BbChart : BbChartBase
{
    internal override string DataSlot => "chart";

    internal override string SeriesType => "composite";

    internal override void ApplyChartDefaults(EChartsOption option)
    {
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
