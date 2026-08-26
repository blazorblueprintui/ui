using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Abstract base class for all composable chart series components.
/// </summary>
/// <remarks>
/// <para>
/// SeriesBase provides shared functionality for series components including data extraction,
/// color resolution, name resolution, and stacking support. Each concrete series (Line, Bar,
/// Area, Pie, Radar, RadialBar) inherits from this class and implements BuildSeriesCore.
/// </para>
/// <para>
/// Series components register with their parent <see cref="BbChartBase"/> via cascading parameter
/// and contribute an <see cref="EChartsSeriesOption"/> to the chart's option tree during rendering.
/// </para>
/// </remarks>
public abstract class SeriesBase : ComponentBase, IChartSeries, IDisposable
{
    [CascadingParameter]
    protected BbChartBase? ParentChart { get; set; }

    /// <summary>
    /// Gets or sets the property name used to extract series values from the chart data.
    /// </summary>
    /// <remarks>
    /// Maps to a property on each data item. For example, if data items have a "Desktop" property,
    /// set DataKey to "Desktop" to extract those values for this series.
    /// </remarks>
    [Parameter]
    public string? DataKey { get; set; }

    /// <summary>
    /// Gets or sets the display name for this series.
    /// </summary>
    /// <remarks>
    /// Used in legends and tooltips. If not set, falls back to the configured label
    /// in <see cref="ChartConfig"/>, then to the <see cref="DataKey"/> value.
    /// </remarks>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an explicit color for this series.
    /// </summary>
    /// <remarks>
    /// Accepts any valid CSS color value or CSS variable reference (e.g., "var(--chart-1)").
    /// If not set, falls back to the color configured in <see cref="ChartConfig"/>.
    /// </remarks>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets whether this series is stacked with other series.
    /// </summary>
    /// <remarks>
    /// When true, series values are stacked on top of each other.
    /// Use <see cref="StackGroup"/> to control which series stack together.
    /// </remarks>
    [Parameter]
    public bool Stacked { get; set; }

    /// <summary>
    /// Gets or sets the stack group identifier.
    /// </summary>
    /// <remarks>
    /// Series with the same StackGroup value are stacked together.
    /// Default is "stack". Only applies when <see cref="Stacked"/> is true.
    /// </remarks>
    [Parameter]
    public string StackGroup { get; set; } = "stack";

    /// <summary>
    /// Gets or sets the child content, which may include <see cref="BbFill"/> components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets a direct fill color value (string or gradient object).
    /// </summary>
    internal object? FillColor { get; set; }

    /// <summary>
    /// Gets or sets a fill provider for lazy evaluation.
    /// </summary>
    /// <remarks>
    /// Set by the <see cref="BbFill"/> component. BuildFill is called lazily during
    /// <see cref="BuildSeriesCore"/> (in OnAfterRenderAsync) when all child components exist.
    /// </remarks>
    internal IFillComponent? FillProvider { get; set; }

    /// <summary>
    /// Gets the resolved fill color, preferring lazy evaluation via FillProvider.
    /// </summary>
    protected object? GetResolvedFillColor() =>
        FillProvider?.BuildFill() ?? FillColor;

    protected override void OnInitialized() =>
        ParentChart?.RegisterComponent(this);

    void IChartComponent.ApplyTo(EChartsOption option) =>
        ApplyToOption(option);

    /// <summary>
    /// Applies this series to the chart option. Override to modify non-series option properties
    /// (e.g., axis configuration) in addition to adding the series.
    /// </summary>
    private protected virtual void ApplyToOption(EChartsOption option)
    {
        var series = BuildSeriesCore();
        option.Series.Add(series);
    }

    EChartsSeriesOption IChartSeries.BuildSeries() => BuildSeriesCore();

    /// <summary>
    /// Builds the ECharts series option for this series type.
    /// </summary>
    /// <returns>A fully configured <see cref="EChartsSeriesOption"/>.</returns>
    internal abstract EChartsSeriesOption BuildSeriesCore();

    /// <summary>
    /// Extracts numeric/value data for this series from the parent chart's data.
    /// </summary>
    /// <returns>A list of values extracted using the <see cref="DataKey"/> property.</returns>
    protected List<object?> GetSeriesData()
    {
        if (string.IsNullOrEmpty(DataKey))
        {
            return [];
        }

        return DataExtractor.ExtractValues(ParentChart?.Data, DataKey);
    }

    /// <summary>
    /// Extracts data for this series as explicit <c>[x, y]</c> pairs when <paramref name="xDataKey"/>
    /// is supplied, falling back to the flat <see cref="GetSeriesData"/> list when it is not.
    /// </summary>
    /// <param name="xDataKey">
    /// The property holding each point's X value, or <see langword="null"/> for positional X.
    /// </param>
    /// <remarks>
    /// <para>
    /// A flat list of Y values leaves ECharts to derive X from the point's *index*, which is correct
    /// on a category axis but silently discards real X values on a <see cref="AxisType.Value"/>,
    /// <see cref="AxisType.Time"/> or <see cref="AxisType.Log"/> axis — the axis ignores its own
    /// <c>data</c>, so the points end up plotted against ordinal position. Emitting pairs is what
    /// makes a genuine X:Y plot expressible.
    /// </para>
    /// <para>
    /// A row missing either side yields a null entry rather than a partial pair, which ECharts would
    /// render against a coerced zero. Null entries are skipped when drawn, and — because callers such
    /// as per-point symbol sizing zip this list against the source data by index — the returned list
    /// stays parallel to the chart's data rather than being compacted.
    /// </para>
    /// </remarks>
    /// <returns>A list of two-element <c>[x, y]</c> arrays, or the flat Y list.</returns>
    protected List<object?> GetPointData(string? xDataKey)
    {
        if (string.IsNullOrEmpty(xDataKey))
        {
            return GetSeriesData();
        }

        if (string.IsNullOrEmpty(DataKey))
        {
            return [];
        }

        var xValues = DataExtractor.ExtractValues(ParentChart?.Data, xDataKey);
        var yValues = DataExtractor.ExtractValues(ParentChart?.Data, DataKey);
        var count = Math.Min(xValues.Count, yValues.Count);
        var points = new List<object?>(count);

        for (var i = 0; i < count; i++)
        {
            points.Add(xValues[i] == null || yValues[i] == null
                ? null
                : new[] { xValues[i], yValues[i] });
        }

        return points;
    }

    /// <summary>
    /// Resolves the color for this series by checking the explicit Color parameter first,
    /// then the ChartConfig, returning null if neither is set.
    /// </summary>
    /// <returns>A CSS color string, or null if no color is configured.</returns>
    protected string? GetResolvedColor()
    {
        if (!string.IsNullOrEmpty(Color))
        {
            return Color;
        }

        if (!string.IsNullOrEmpty(DataKey) && ParentChart?.Config != null)
        {
            var configColor = ParentChart.Config.GetColor(DataKey);
            return configColor;
        }

        return null;
    }

    /// <summary>
    /// Resolves the display name for this series by checking the explicit Name parameter first,
    /// then the ChartConfig label, falling back to the DataKey.
    /// </summary>
    /// <returns>The resolved display name for the series.</returns>
    protected string? GetResolvedName()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return Name;
        }

        if (!string.IsNullOrEmpty(DataKey) && ParentChart?.Config != null)
        {
            return ParentChart.Config.GetLabel(DataKey);
        }

        return DataKey;
    }

    /// <summary>
    /// Converts a <see cref="LabelPosition"/> enum value to its ECharts string representation.
    /// </summary>
    protected static string ToEChartsPosition(LabelPosition position) => position switch
    {
        LabelPosition.Top => "top",
        LabelPosition.Bottom => "bottom",
        LabelPosition.Left => "left",
        LabelPosition.Right => "right",
        LabelPosition.Inside => "inside",
        LabelPosition.InsideLeft => "insideLeft",
        LabelPosition.InsideRight => "insideRight",
        LabelPosition.InsideTop => "insideTop",
        LabelPosition.InsideBottom => "insideBottom",
        LabelPosition.Outside => "outside",
        LabelPosition.Center => "center",
        LabelPosition.Middle => "middle",
        _ => "top"
    };

    public void Dispose()
    {
        ParentChart?.UnregisterComponent(this);
        GC.SuppressFinalize(this);
    }
}
