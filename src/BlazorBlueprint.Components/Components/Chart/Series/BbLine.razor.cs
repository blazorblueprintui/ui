using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A line series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders a line series within a parent chart. Supports smooth curves, step interpolation,
/// dashed lines, and configurable dot visibility and size.
/// </para>
/// <para>
/// Must be placed inside a chart component that provides a <see cref="BbChartBase"/> cascading value.
/// Supports child <see cref="BbFill"/> components for gradient fills.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;LineChart Data="@data"&gt;
///     &lt;XAxis DataKey="month" /&gt;
///     &lt;YAxis /&gt;
///     &lt;Line DataKey="desktop" Name="Desktop" Curve="CurveType.Smooth" /&gt;
///     &lt;Line DataKey="mobile" Name="Mobile" Dashed="true" /&gt;
/// &lt;/LineChart&gt;
/// </code>
/// </example>
public partial class BbLine : SeriesBase
{
    /// <summary>
    /// Gets or sets the curve interpolation type.
    /// </summary>
    /// <remarks>
    /// Controls how data points are connected. Default is <see cref="CurveType.Linear"/>.
    /// </remarks>
    [Parameter]
    public CurveType Curve { get; set; } = CurveType.Linear;

    /// <summary>
    /// Gets or sets the line stroke width in pixels.
    /// </summary>
    [Parameter]
    public int StrokeWidth { get; set; } = 2;

    /// <summary>
    /// Gets or sets whether data point dots are visible.
    /// </summary>
    [Parameter]
    public bool ShowDots { get; set; } = true;

    /// <summary>
    /// Gets or sets the size of data point dots in pixels.
    /// </summary>
    [Parameter]
    public int DotSize { get; set; } = 4;

    /// <summary>
    /// Gets or sets whether the line is rendered with a dashed stroke.
    /// </summary>
    [Parameter]
    public bool Dashed { get; set; }

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var resolvedColor = GetResolvedColor();

        var series = new EChartsSeriesOption
        {
            Type = "line",
            Name = GetResolvedName(),
            Data = GetSeriesData(),
            ShowSymbol = ShowDots,
            SymbolSize = ShowDots ? DotSize : 0,
            LineStyle = new EChartsLineStyleOption
            {
                Width = StrokeWidth,
                Color = resolvedColor,
                Type = Dashed ? "dashed" : null
            },
            ItemStyle = resolvedColor != null
                ? new EChartsItemStyleOption { Color = FillColor ?? resolvedColor }
                : FillColor != null
                    ? new EChartsItemStyleOption { Color = FillColor }
                    : null,
            Emphasis = new EChartsEmphasisOption { Disabled = true }
        };

        if (Stacked)
        {
            series.Stack = StackGroup;
        }

        switch (Curve)
        {
            case CurveType.Smooth:
                series.Smooth = true;
                break;
            case CurveType.Step:
                series.Step = "middle";
                break;
            case CurveType.StepBefore:
                series.Step = "start";
                break;
            case CurveType.StepAfter:
                series.Step = "end";
                break;
        }

        return series;
    }
}
