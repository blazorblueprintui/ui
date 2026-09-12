using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Describes the data point a user clicked in a chart.
/// </summary>
/// <remarks>
/// ECharts reports a point's value as whatever the series put there: a number for a bar or line,
/// an array for scatter or candlestick, occasionally an object. Rather than surface that raw,
/// <see cref="Value"/> carries the scalar case and <see cref="Values"/> the array case, so the two
/// common shapes arrive typed. When neither applies both are <c>null</c>, and
/// <see cref="Name"/> with <see cref="DataIndex"/> still identify the point.
/// </remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "EventArgs suffix is intentional for this DTO, matching SelectionChangeEventArgs and PanelResizeEventArgs")]
public sealed record ChartClickEventArgs
{
    /// <summary>The name of the series the point belongs to, if the series has one.</summary>
    public string? SeriesName { get; init; }

    /// <summary>Zero-based index of the series, or <c>-1</c> if the chart did not report one.</summary>
    public int SeriesIndex { get; init; } = -1;

    /// <summary>
    /// Zero-based index of the point within its series, or <c>-1</c> if the chart did not report
    /// one. This is the field to key a drill-down on: it maps straight back to the position in the
    /// collection you bound.
    /// </summary>
    public int DataIndex { get; init; } = -1;

    /// <summary>The point's category or label — the x-axis name for a bar, the slice name for a pie.</summary>
    public string? Name { get; init; }

    /// <summary>
    /// Which part of the chart was clicked, as ECharts names it — <c>"series"</c> for a data point,
    /// and other values such as <c>"markPoint"</c> for decorations.
    /// </summary>
    public string? ComponentType { get; init; }

    /// <summary>The point's value when it is a single number, which covers bar, line, area and pie.</summary>
    public double? Value { get; init; }

    /// <summary>
    /// The point's values when it is an array, which covers scatter (x, y) and candlestick
    /// (open, close, low, high).
    /// </summary>
    public IReadOnlyList<double>? Values { get; init; }
}
