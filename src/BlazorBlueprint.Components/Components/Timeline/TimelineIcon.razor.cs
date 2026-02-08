using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Timeline;

/// <summary>
/// Renders the circular icon indicator for a timeline item.
/// </summary>
/// <remarks>
/// Displays a colored circular badge that can contain a custom icon or render
/// as a simple dot. Supports multiple color themes and size variants.
/// </remarks>
public partial class TimelineIcon : ComponentBase
{
    /// <summary>
    /// Gets or sets the parent Timeline component via cascading parameter.
    /// Used to read ConnectorFit for ring styling.
    /// </summary>
    [CascadingParameter]
    public Timeline? ParentTimeline { get; set; }

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
        "relative rounded-full shadow-sm",
        IsConnected ? null : "ring-8 ring-background",
        Class
    );

    private string CssClass => ClassNames.cn(
        "flex items-center justify-center rounded-full",
        Size switch
        {
            TimelineSize.Small => "h-8 w-8",
            TimelineSize.Medium => "h-10 w-10",
            TimelineSize.Large => "h-12 w-12",
            _ => "h-10 w-10"
        },
        Variant == TimelineIconVariant.Outline
            ? EffectiveColor switch
            {
                TimelineColor.Primary => "bg-background border-2 border-primary text-primary",
                TimelineColor.Secondary => "bg-background border-2 border-secondary text-secondary",
                TimelineColor.Muted => "bg-background border-2 border-muted text-muted-foreground",
                TimelineColor.Accent => "bg-background border-2 border-accent text-accent",
                TimelineColor.Destructive => "bg-background border-2 border-destructive text-destructive",
                _ => "bg-background border-2 border-primary text-primary"
            }
            : EffectiveColor switch
            {
                TimelineColor.Primary => "bg-primary text-primary-foreground",
                TimelineColor.Secondary => "bg-secondary text-secondary-foreground",
                TimelineColor.Muted => "bg-muted text-muted-foreground",
                TimelineColor.Accent => "bg-accent text-accent-foreground",
                TimelineColor.Destructive => "bg-destructive text-destructive-foreground",
                _ => "bg-primary text-primary-foreground"
            },
        Loading ? "animate-pulse" : null
    );

    private string IconSizeClass => Size switch
    {
        TimelineSize.Small => "h-4 w-4",
        TimelineSize.Medium => "h-5 w-5",
        TimelineSize.Large => "h-6 w-6",
        _ => "h-5 w-5"
    };
}
