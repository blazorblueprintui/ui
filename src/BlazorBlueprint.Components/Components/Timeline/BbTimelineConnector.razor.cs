using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders a vertical connector line between timeline items.
/// </summary>
/// <remarks>
/// The connector color adapts based on the item's status:
/// - Completed: solid primary color
/// - InProgress: gradient from primary to muted
/// - Pending: muted color
/// A custom color can override the status-based coloring.
/// </remarks>
public partial class BbTimelineConnector : ComponentBase
{
    /// <summary>
    /// Gets or sets the status to determine connector styling.
    /// </summary>
    [Parameter]
    public TimelineStatus Status { get; set; } = TimelineStatus.Completed;

    /// <summary>
    /// Gets or sets an explicit color override for the connector.
    /// When set, this takes precedence over the status-based color.
    /// </summary>
    [Parameter]
    public TimelineColor? Color { get; set; }

    /// <summary>
    /// Gets or sets the connector line style (Solid, Dashed, or Dotted).
    /// </summary>
    [Parameter]
    public TimelineConnectorStyle ConnectorStyle { get; set; } = TimelineConnectorStyle.Solid;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the connector.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Captures any additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsSolid => ConnectorStyle == TimelineConnectorStyle.Solid;

    private string CssClass => ClassNames.cn(
        "bb:w-0.5",
        !IsSolid ? "bb:border-l-2" : null,
        !IsSolid ? ConnectorStyle switch
        {
            TimelineConnectorStyle.Dashed => "bb:border-dashed",
            TimelineConnectorStyle.Dotted => "bb:border-dotted",
            _ => null
        } : null,
        IsSolid
            ? Color switch
            {
                TimelineColor.Primary => "bb:bg-primary",
                TimelineColor.Secondary => "bb:bg-secondary",
                TimelineColor.Muted => "bb:bg-muted",
                TimelineColor.Accent => "bb:bg-accent",
                TimelineColor.Destructive => "bb:bg-destructive",
                _ => Status switch
                {
                    TimelineStatus.Completed => "bb:bg-primary",
                    TimelineStatus.InProgress => "bb:bg-linear-to-b bb:from-primary bb:to-muted",
                    TimelineStatus.Pending => "bb:bg-muted",
                    _ => "bb:bg-primary"
                }
            }
            : Color switch
            {
                TimelineColor.Primary => "bb:border-primary",
                TimelineColor.Secondary => "bb:border-secondary",
                TimelineColor.Muted => "bb:border-muted",
                TimelineColor.Accent => "bb:border-accent",
                TimelineColor.Destructive => "bb:border-destructive",
                _ => Status switch
                {
                    TimelineStatus.Completed => "bb:border-primary",
                    TimelineStatus.InProgress => "bb:border-primary",
                    TimelineStatus.Pending => "bb:border-muted",
                    _ => "bb:border-primary"
                }
            },
        Class
    );
}
