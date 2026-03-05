using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// An area series component for composable charts.
/// </summary>
/// <remarks>
/// <para>
/// Renders an area series (line with filled region beneath) within a parent chart.
/// Supports smooth curves, step interpolation, configurable fill opacity, and gradient
/// fills via child <see cref="BbFill"/> components.
/// </para>
/// <para>
/// Internally uses ECharts line type with an areaStyle configuration.
/// Must be placed inside a chart component that provides a <see cref="BbChartBase"/> cascading value.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;AreaChart Data="@data"&gt;
///     &lt;XAxis DataKey="month" /&gt;
///     &lt;YAxis /&gt;
///     &lt;Area DataKey="desktop" Name="Desktop" Curve="CurveType.Smooth" FillOpacity="0.3"&gt;
///         &lt;Fill&gt;
///             &lt;LinearGradient&gt;
///                 &lt;GradientStop Offset="0" Color="var(--chart-1)" Opacity="0.8" /&gt;
///                 &lt;GradientStop Offset="1" Color="var(--chart-1)" Opacity="0.1" /&gt;
///             &lt;/LinearGradient&gt;
///         &lt;/Fill&gt;
///     &lt;/Area&gt;
/// &lt;/AreaChart&gt;
/// </code>
/// </example>
public partial class BbArea : SeriesBase
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
    /// Gets or sets the opacity of the filled area beneath the line.
    /// </summary>
    /// <remarks>
    /// Value between 0 (transparent) and 1 (opaque). Default is 0.4.
    /// </remarks>
    [Parameter]
    public double FillOpacity { get; set; } = 0.4;

    /// <summary>
    /// Gets or sets whether data point dots are visible.
    /// </summary>
    [Parameter]
    public bool ShowDots { get; set; }

    /// <inheritdoc />
    internal override EChartsSeriesOption BuildSeriesCore()
    {
        var resolvedColor = GetResolvedColor();
        var fillColor = GetResolvedFillColor();

        var series = new EChartsSeriesOption
        {
            Type = "line",
            Name = GetResolvedName(),
            Data = GetSeriesData(),
            ShowSymbol = ShowDots,
            AreaStyle = new EChartsAreaStyleOption
            {
                Opacity = fillColor != null ? 1.0 : FillOpacity,
                Color = fillColor
            },
            LineStyle = new EChartsLineStyleOption
            {
                Width = StrokeWidth,
                Color = resolvedColor
            },
            ItemStyle = resolvedColor != null
                ? new EChartsItemStyleOption { Color = resolvedColor }
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
