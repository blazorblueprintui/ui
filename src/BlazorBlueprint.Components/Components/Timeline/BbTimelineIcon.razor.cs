using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders the circular icon indicator for a timeline item.
/// </summary>
/// <remarks>
/// Displays a colored circular badge that can contain a custom icon or render
/// as a simple dot. Supports multiple color themes and size variants.
/// </remarks>
public partial class BbTimelineIcon : ComponentBase
{
    /// <summary>
    /// Gets or sets the parent Timeline component via cascading parameter.
    /// Used to read ConnectorFit for ring styling.
    /// </summary>
    [CascadingParameter]
    public BbTimeline? ParentTimeline { get; set; }

    /// <summary>
    /// Gets or sets the color theme for the icon.
    /// </summary>
    [Parameter]
    public TimelineColor Color { get; set; } = TimelineColor.Primary;

    /// <summary>
    /// Gets or sets the status of the timeline item.
    /// When the status is Pending, the color automatically becomes Muted
    /// unless an explicit Color is set.
    /// </summary>
    [Parameter]
    public TimelineStatus Status { get; set; } = TimelineStatus.Completed;

    /// <summary>
    /// Gets or sets the size of the icon.
    /// </summary>
    [Parameter]
    public TimelineSize Size { get; set; } = TimelineSize.Medium;

    /// <summary>
    /// Gets or sets the visual style variant (Solid or Outline).
    /// </summary>
    [Parameter]
    public TimelineIconVariant Variant { get; set; } = TimelineIconVariant.Solid;

    /// <summary>
    /// Gets or sets whether the icon is in a loading state (shows pulse animation).
    /// </summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets custom icon content to render inside the circle.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures any additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private TimelineColor EffectiveColor =>
        Status == TimelineStatus.Pending ? TimelineColor.Muted : Color;

    private bool IsConnected => ParentTimeline?.ConnectorFit == TimelineConnectorFit.Connected;

    private string RingClass => ClassNames.cn(
        "bb:relative bb:rounded-full bb:shadow-sm",
        IsConnected ? null : "bb:ring-8 bb:ring-background",
        Class
    );

    private string CssClass => ClassNames.cn(
        "bb:flex bb:items-center bb:justify-center bb:rounded-full",
        Size switch
        {
            TimelineSize.Small => "bb:h-8 bb:w-8",
            TimelineSize.Medium => "bb:h-10 bb:w-10",
            TimelineSize.Large => "bb:h-12 bb:w-12",
            _ => "bb:h-10 bb:w-10"
        },
        Variant == TimelineIconVariant.Outline
            ? EffectiveColor switch
            {
                TimelineColor.Primary => "bb:bg-background bb:border-2 bb:border-primary bb:text-primary",
                TimelineColor.Secondary => "bb:bg-background bb:border-2 bb:border-secondary bb:text-secondary",
                TimelineColor.Muted => "bb:bg-background bb:border-2 bb:border-muted bb:text-muted-foreground",
                TimelineColor.Accent => "bb:bg-background bb:border-2 bb:border-accent bb:text-accent",
                TimelineColor.Destructive => "bb:bg-background bb:border-2 bb:border-destructive bb:text-destructive",
                _ => "bb:bg-background bb:border-2 bb:border-primary bb:text-primary"
            }
            : EffectiveColor switch
            {
                TimelineColor.Primary => "bb:bg-primary bb:text-primary-foreground",
                TimelineColor.Secondary => "bb:bg-secondary bb:text-secondary-foreground",
                TimelineColor.Muted => "bb:bg-muted bb:text-muted-foreground",
                TimelineColor.Accent => "bb:bg-accent bb:text-accent-foreground",
                TimelineColor.Destructive => "bb:bg-destructive bb:text-destructive-foreground",
                _ => "bb:bg-primary bb:text-primary-foreground"
            },
        Loading ? "bb:animate-pulse" : null
    );

    private string IconSizeClass => Size switch
    {
        TimelineSize.Small => "bb:h-4 bb:w-4",
        TimelineSize.Medium => "bb:h-5 bb:w-5",
        TimelineSize.Large => "bb:h-6 bb:w-6",
        _ => "bb:h-5 bb:w-5"
    };
}
