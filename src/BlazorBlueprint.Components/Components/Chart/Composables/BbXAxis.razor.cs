using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Configures the X-axis for a chart. Must be placed inside a chart component.
/// </summary>
/// <remarks>
/// <para>
/// XAxis is a composable chart component that registers with its parent chart via
/// cascading parameter and applies X-axis configuration during option building.
/// </para>
/// <para>
/// By default, XAxis uses category type with hidden axis lines and ticks for a clean appearance.
/// When <see cref="DataKey"/> is set, category labels are automatically extracted from the chart data.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;BarChart Data="@data"&gt;
///     &lt;XAxis DataKey="month" /&gt;
///     &lt;YAxis /&gt;
/// &lt;/BarChart&gt;
/// </code>
/// </example>
public partial class BbXAxis : ComponentBase, IChartComponent, IDisposable
{
    [CascadingParameter]
    private BbChartBase? ParentChart { get; set; }

    /// <summary>
    /// Gets or sets the property name used to extract category values from the chart data.
    /// </summary>
    /// <remarks>
    /// When set, category labels are automatically extracted from each data item using this property name.
    /// </remarks>
    [Parameter]
    public string? DataKey { get; set; }

    /// <summary>
    /// Gets or sets the axis type.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="AxisType.Category"/> for categorical data.
    /// Use <see cref="AxisType.Value"/> for numeric axes, <see cref="AxisType.Time"/> for time series,
    /// or <see cref="AxisType.Log"/> for logarithmic scales.
    /// </remarks>
    [Parameter]
    public AxisType Type { get; set; } = AxisType.Category;

    /// <summary>
    /// Gets or sets whether the axis is visible.
    /// </summary>
    [Parameter]
    public bool Show { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the axis line is visible.
    /// </summary>
    [Parameter]
    public bool ShowAxisLine { get; set; }

    /// <summary>
    /// Gets or sets whether axis tick marks are visible.
    /// </summary>
    [Parameter]
    public bool ShowTicks { get; set; }

    /// <summary>
    /// Gets or sets whether grid lines are shown for this axis.
    /// </summary>
    [Parameter]
    public bool ShowGrid { get; set; }

    /// <summary>
    /// Gets or sets the color of grid lines.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>var(--border)</c> when not specified, matching the design system border color.
    /// </remarks>
    [Parameter]
    public string? GridColor { get; set; }

    /// <summary>
    /// Gets or sets the style of grid lines.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="LineStyleType.Dashed"/>.
    /// </remarks>
    [Parameter]
    public LineStyleType GridType { get; set; } = LineStyleType.Dashed;

    /// <summary>
    /// Gets or sets whether axis labels are rendered inside the chart area rather than outside.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, category labels are positioned inside the chart area overlapping
    /// the bars or data region. Useful for horizontal bar charts where labels should appear
    /// inside the bars. Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    public bool LabelInside { get; set; }

    /// <summary>
    /// Gets or sets the color of axis labels.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>var(--muted-foreground)</c> when not specified, matching the design system muted text color.
    /// </remarks>
    [Parameter]
    public string? LabelColor { get; set; }

    /// <summary>
    /// Gets or sets the font size of axis labels in pixels.
    /// </summary>
    [Parameter]
    public int? LabelFontSize { get; set; }

    /// <summary>
    /// Gets or sets the rotation angle of axis labels in degrees.
    /// </summary>
    /// <remarks>
    /// Useful for long category labels that would otherwise overlap. Positive values rotate clockwise.
    /// </remarks>
    [Parameter]
    public int? LabelRotate { get; set; }

    /// <summary>
    /// Gets or sets whether there is a gap between the axis boundary and the data.
    /// </summary>
    /// <remarks>
    /// When <c>true</c> (default for bar charts), data is centered in each category band.
    /// When <c>false</c>, data points align to category boundaries (recommended for line/area charts).
    /// When <c>null</c>, uses the ECharts default (true for category axes).
    /// </remarks>
    [Parameter]
    public bool? BoundaryGap { get; set; }

    /// <summary>
    /// Gets or sets the axis name displayed alongside the axis.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the minimum value of the axis.
    /// </summary>
    /// <remarks>
    /// Can be a number or the string <c>"dataMin"</c> to use the minimum data value.
    /// </remarks>
    [Parameter]
    public object? Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value of the axis.
    /// </summary>
    /// <remarks>
    /// Can be a number or the string <c>"dataMax"</c> to use the maximum data value.
    /// </remarks>
    [Parameter]
    public object? Max { get; set; }

    protected override void OnInitialized() =>
        ParentChart?.RegisterComponent(this);

    void IChartComponent.ApplyTo(EChartsOption option)
    {
        var axis = new EChartsAxisOption
        {
            Type = Type switch
            {
                AxisType.Category => "category",
                AxisType.Value => "value",
                AxisType.Time => "time",
                AxisType.Log => "log",
                _ => "category"
            },
            BoundaryGap = BoundaryGap ?? (ParentChart?.SeriesType == "line" ? false : null),
            Show = Show,
            Name = Name,
            Min = Min,
            Max = Max,
            Z = LabelInside ? 10 : null,
            AxisLine = new EChartsAxisLineOption
            {
                Show = ShowAxisLine
            },
            AxisTick = new EChartsAxisTickOption
            {
                Show = ShowTicks
            },
            SplitLine = new EChartsSplitLineOption
            {
                Show = ShowGrid,
                LineStyle = ShowGrid
                    ? new EChartsLineStyleOption
                    {
                        Color = GridColor ?? "var(--border)",
                        Type = ToEChartsLineStyle(GridType)
                    }
                    : null
            },
            AxisLabel = new EChartsAxisLabelOption
            {
                Inside = LabelInside ? true : null,
                Color = LabelColor ?? "var(--muted-foreground)",
                FontSize = LabelFontSize,
                Rotate = LabelRotate
            }
        };

        if (!string.IsNullOrEmpty(DataKey) && ParentChart?.Data != null)
        {
            axis.Data = DataExtractor.ExtractDistinctStringValues(ParentChart.Data, DataKey);
        }

        // When parent chart swaps axes (e.g., horizontal BarChart),
        // XAxis config goes to the physical Y-axis with inverse
        // so categories render top-to-bottom instead of bottom-to-top
        if (ParentChart?.SwapAxes == true)
        {
            axis.Inverse = true;
            option.YAxis = axis;
        }
        else
        {
            option.XAxis = axis;
        }
    }

    private static string ToEChartsLineStyle(LineStyleType type) => type switch
    {
        LineStyleType.Solid => "solid",
        LineStyleType.Dotted => "dotted",
        _ => "dashed"
    };

    public void Dispose()
    {
        ParentChart?.UnregisterComponent(this);
        GC.SuppressFinalize(this);
    }
}
