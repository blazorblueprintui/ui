namespace BlazorUI.Components.Chart;

/// <summary>
/// Defines the style of indicator shown in chart tooltips.
/// </summary>
/// <remarks>
/// The tooltip indicator is the visual element that shows which series
/// a value belongs to, typically matching the series color.
/// </remarks>
public enum TooltipIndicator
{
    /// <summary>
    /// Small dot/circle indicator.
    /// Compact and minimal visual footprint.
    /// </summary>
    Dot,

    /// <summary>
    /// Horizontal line indicator.
    /// Matches line chart visual language.
    /// </summary>
    Line,

    /// <summary>
    /// Dashed line indicator.
    /// For secondary or reference series.
    /// </summary>
    Dashed,

    /// <summary>
    /// No indicator shown.
    /// Text-only tooltip for minimal design.
    /// </summary>
    None
}
