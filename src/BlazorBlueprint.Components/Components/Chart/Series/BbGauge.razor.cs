using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A gauge series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a gauge (speedometer/meter) series within a parent chart. Used for KPI
/// visualization with configurable pointer, progress arc, and value display.
/// </para>
/// <para>
/// Data is provided as a simple value via <see cref="SeriesBase.DataKey"/> which extracts
/// the first data point's value, or via the chart's data source.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BbGaugeChart Data="@data"&gt;
///     &lt;BbGauge DataKey="value" Name="Speed" Min="0" Max="100" ShowProgress="true" /&gt;
/// &lt;/BbGaugeChart&gt;
/// </code>
/// </example>
public partial class BbGauge : SeriesBase
{
    /// <summary>
    /// Gets or sets the minimum value of the gauge scale.
    /// </summary>
    [Parameter]
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the gauge scale.
    /// </summary>
    [Parameter]
    public double Max { get; set; } = 100;

    /// <summary>
    /// Gets or sets the starting angle of the gauge in degrees.
    /// </summary>
    /// <remarks>
    /// 0 is at the 3 o'clock position. Default is 225 (roughly 7 o'clock).
    /// </remarks>
    [Parameter]
    public int StartAngle { get; set; } = 225;

    /// <summary>
    /// Gets or sets the ending angle of the gauge in degrees.
    /// </summary>
    /// <remarks>
    /// Default is -45, creating a 270-degree arc with the default <see cref="StartAngle"/>.
    /// </remarks>
    [Parameter]
    public int EndAngle { get; set; } = -45;

    /// <summary>
    /// Gets or sets the number of split sections on the gauge scale.
    /// </summary>
    [Parameter]
    public int SplitNumber { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to show the progress arc.
    /// </summary>
    [Parameter]
    public bool ShowProgress { get; set; }

    /// <summary>
    /// Gets or sets the width of the progress arc in pixels.
    /// </summary>
    [Parameter]
    public int ProgressWidth { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether the progress arc has round caps.
    /// </summary>
    [Parameter]
    public bool ProgressRoundCap { get; set; } = true;

    /// <summary>
    /// Gets or sets the color of the progress arc.
    /// </summary>
    [Parameter]
    public string? ProgressColor { get; set; }

    /// <summary>
    /// Gets or sets whether to show the pointer needle.
    /// </summary>
    [Parameter]
    public bool ShowPointer { get; set; } = true;

    /// <summary>
    /// Gets or sets the length of the pointer as a percentage of the gauge radius.
    /// </summary>
    [Parameter]
    public string PointerLength { get; set; } = "60%";

    /// <summary>
    /// Gets or sets the width of the pointer in pixels.
    /// </summary>
    [Parameter]
    public int PointerWidth { get; set; } = 6;

    /// <summary>
    /// Gets or sets whether to show the axis arc line.
    /// </summary>
    [Parameter]
    public bool ShowAxisLine { get; set; } = true;

    /// <summary>
    /// Gets or sets the width of the axis arc line in pixels.
    /// </summary>
    [Parameter]
    public int AxisLineWidth { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to show the value detail text.
    /// </summary>
    [Parameter]
    public bool ShowDetail { get; set; } = true;

    /// <summary>
    /// Gets or sets the ECharts formatter string for the detail value.
    /// </summary>
    [Parameter]
    public string DetailFormatter { get; set; } = "{value}";

    /// <summary>
    /// Gets or sets the font size of the detail value in pixels.
    /// </summary>
    [Parameter]
    public int DetailFontSize { get; set; } = 24;

    /// <summary>
    /// Gets or sets the color of the detail value text.
    /// </summary>
    [Parameter]
    public string? DetailColor { get; set; }

    /// <summary>
    /// Gets or sets whether to show the title text below the gauge.
    /// </summary>
    [Parameter]
    public bool ShowTitle { get; set; } = true;

    /// <summary>
    /// Gets or sets the font size of the title text in pixels.
    /// </summary>
    [Parameter]
    public int TitleFontSize { get; set; } = 14;

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var resolvedColor = GetResolvedColor();

        var series = new EChartsSeriesOption
        {
            Type = "gauge",
            Name = GetResolvedName(),
            Min = Min,
            Max = Max,
            StartAngle = StartAngle,
            EndAngle = EndAngle,
            SplitNumber = SplitNumber,
            Pointer = new EChartsGaugePointerOption
            {
                Show = ShowPointer,
                Length = PointerLength,
                Width = PointerWidth,
                ItemStyle = resolvedColor != null
                    ? new EChartsItemStyleOption { Color = resolvedColor }
                    : null
            },
            AxisLine = new EChartsGaugeAxisLineOption
            {
                Show = ShowAxisLine,
                LineStyle = new EChartsGaugeAxisLineStyleOption
                {
                    Width = AxisLineWidth,
                    Color = new object[]
                    {
                        new object[] { 1, "var(--muted)" }
                    }
                }
            },
            AxisTick = new EChartsAxisTickOption
            {
                Show = false
            },
            SplitLine = new EChartsGaugeSplitLineOption
            {
                Show = true,
                Length = 10,
                LineStyle = new EChartsLineStyleOption
                {
                    Color = "var(--muted-foreground)",
                    Width = 1
                }
            },
            Detail = new EChartsGaugeDetailOption
            {
                Show = ShowDetail,
                Formatter = DetailFormatter,
                FontSize = DetailFontSize,
                FontWeight = "bold",
                Color = DetailColor ?? "var(--foreground)",
                ValueAnimation = true,
                OffsetCenter = [0, "70%"]
            },
            Title = new EChartsSeriesTitleOption
            {
                Show = ShowTitle,
                FontSize = TitleFontSize,
                Color = "var(--muted-foreground)",
                OffsetCenter = [0, "90%"]
            }
        };

        if (ShowProgress)
        {
            series.Progress = new EChartsGaugeProgressOption
            {
                Show = true,
                Width = ProgressWidth,
                RoundCap = ProgressRoundCap,
                Color = ProgressColor ?? resolvedColor
            };
        }

        // Extract gauge data: [{value: X, name: "Label"}]
        var rawData = GetSeriesData();
        if (rawData.Count > 0)
        {
            var gaugeData = new List<object?>();
            var resolvedName = GetResolvedName();

            foreach (var value in rawData)
            {
                gaugeData.Add(new Dictionary<string, object?>
                {
                    ["value"] = value,
                    ["name"] = resolvedName
                });
            }

            series.Data = gaugeData;
        }

        return series;
    }
}
