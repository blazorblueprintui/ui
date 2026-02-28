namespace BlazorBlueprint.Components;

/// <summary>
/// A gauge chart component using the declarative composition API with Apache ECharts.
/// </summary>
/// <remarks>
/// <para>
/// The GaugeChart component provides speedometer/meter visualizations for KPI display.
/// Supports configurable pointer, progress arc, axis styling, and value detail display.
/// </para>
/// <para>
/// Usage:
/// <code>
/// &lt;BbGaugeChart Data="@data" Height="350px"&gt;
///     &lt;BbGauge DataKey="value" Name="Speed" Min="0" Max="200"
///              ShowProgress="true" Color="var(--chart-1)" /&gt;
/// &lt;/BbGaugeChart&gt;
/// </code>
/// </para>
/// </remarks>
public class BbGaugeChart : BbChartBase
{
    internal override string DataSlot => "gauge-chart";

    internal override string SeriesType => "gauge";

    internal override void ApplyChartDefaults(EChartsOption option)
    {
        // Gauge charts don't use XAxis, YAxis, or Grid
        option.XAxis = null;
        option.YAxis = null;
        option.Grid = null;
    }
}
