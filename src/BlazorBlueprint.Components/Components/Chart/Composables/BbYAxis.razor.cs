using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Configures the Y-axis for a chart. Must be placed inside a chart component.
/// </summary>
/// <remarks>
/// <para>
/// YAxis is a composable chart component that registers with its parent chart via
/// cascading parameter and applies Y-axis configuration during option building.
/// </para>
/// <para>
/// By default, YAxis uses value type with visible grid lines for readability.
/// Axis lines and ticks are hidden for a clean, modern appearance.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;LineChart Data="@data"&gt;
///     &lt;XAxis DataKey="month" /&gt;
///     &lt;YAxis ShowGrid="true" /&gt;
/// &lt;/LineChart&gt;
/// </code>
/// </example>
public partial class BbYAxis : ComponentBase, IChartComponent, IDisposable
{
    [CascadingParameter]
    private BbChartBase? ParentChart { get; set; }

    /// <summary>
    /// Gets or sets the property name used to extract category values from the chart data.
    /// </summary>
    /// <remarks>
    /// When set, category labels are automatically extracted from each data item using this property name.
    /// Typically used with <see cref="AxisType.Category"/> for heatmap Y-axes.
    /// </remarks>
    [Parameter]
    public string? DataKey { get; set; }

    /// <summary>
    /// Gets or sets the axis type.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="AxisType.Value"/> for numeric data.
    /// </remarks>
    [Parameter]
    public AxisType Type { get; set; } = AxisType.Value;

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
    /// <remarks>
    /// Defaults to <c>true</c> for Y-axis to improve readability of value comparisons.
    /// </remarks>
    [Parameter]
    public bool ShowGrid { get; set; } = true;

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
    [Parameter]
    public int? LabelRotate { get; set; }

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

    /// <summary>
    /// Gets or sets whether the axis scale should auto-fit to the data range.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the axis range is determined by the data rather than starting at zero.
    /// Essential for candlestick and other financial charts where values cluster in a narrow range.
    /// </remarks>
    [Parameter]
    public bool? Scale { get; set; }

    protected override void OnInitialized() =>
        ParentChart?.RegisterComponent(this);

    void IChartComponent.ApplyTo(EChartsOption option)
    {
        var effectiveType = !string.IsNullOrEmpty(DataKey) && Type == AxisType.Value
            ? AxisType.Category
            : Type;

        var axis = new EChartsAxisOption
        {
            Type = effectiveType switch
            {
                AxisType.Category => "category",
                AxisType.Value => "value",
                AxisType.Time => "time",
                AxisType.Log => "log",
                _ => "value"
            },
            Show = Show,
            Name = Name,
            Min = Min,
            Max = Max,
            Scale = Scale,
            AxisLine = new EChartsAxisLineOption
            {
                Show = Show && ShowAxisLine
            },
            AxisTick = new EChartsAxisTickOption
            {
                Show = Show && ShowTicks
            },
            SplitLine = new EChartsSplitLineOption
            {
                Show = Show && ShowGrid,
                LineStyle = Show && ShowGrid
                    ? new EChartsLineStyleOption
                    {
                        Color = GridColor ?? "var(--border)",
                        Type = ToEChartsLineStyle(GridType)
                    }
                    : null
            },
            AxisLabel = Show
                ? new EChartsAxisLabelOption
                {
                    Color = LabelColor ?? "var(--muted-foreground)",
                    FontSize = LabelFontSize,
                    Rotate = LabelRotate
                }
                : new EChartsAxisLabelOption { Show = false }
        };

        if (!string.IsNullOrEmpty(DataKey) && ParentChart?.Data != null)
        {
            axis.Data = DataExtractor.ExtractDistinctStringValues(ParentChart.Data, DataKey);
        }

        // When parent chart swaps axes (e.g., horizontal BarChart),
        // YAxis config goes to the physical X-axis
        if (ParentChart?.SwapAxes == true)
        {
            option.XAxis = axis;
        }
        else
        {
            option.YAxis = axis;
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
