using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Represents a single item/entry in a Timeline.
/// </summary>
/// <remarks>
/// Each TimelineItem renders a date column, icon/connector column, and content column.
/// It supports custom icons, status-based styling, and optional connector lines.
/// </remarks>
public partial class BbTimelineItem : ComponentBase
{
    private int _itemIndex;

    /// <summary>
    /// Gets or sets the parent Timeline component via cascading parameter.
    /// </summary>
    [CascadingParameter]
    public BbTimeline? ParentTimeline { get; set; }

    /// <summary>
    /// Gets or sets the color theme for the icon.
    /// </summary>
    [Parameter]
    public TimelineColor IconColor { get; set; } = TimelineColor.Primary;

    /// <summary>
    /// Gets or sets the current status of the item.
    /// </summary>
    [Parameter]
    public TimelineStatus Status { get; set; } = TimelineStatus.Completed;

    /// <summary>
    /// Gets or sets the color theme for the connector line.
    /// </summary>
    [Parameter]
    public TimelineColor? ConnectorColor { get; set; }

    /// <summary>
    /// Gets or sets whether to show the connector line below this item.
    /// </summary>
    [Parameter]
    public bool ShowConnector { get; set; } = true;

    /// <summary>
    /// Gets or sets the size of the icon.
    /// </summary>
    [Parameter]
    public TimelineSize IconSize { get; set; } = TimelineSize.Medium;

    /// <summary>
    /// Gets or sets the icon style variant (Solid or Outline).
    /// </summary>
    [Parameter]
    public TimelineIconVariant IconVariant { get; set; } = TimelineIconVariant.Solid;

    /// <summary>
    /// Gets or sets the connector line style (Solid, Dashed, or Dotted).
    /// </summary>
    [Parameter]
    public TimelineConnectorStyle ConnectorStyle { get; set; } = TimelineConnectorStyle.Solid;

    /// <summary>
    /// Gets or sets whether the item is in a loading state (shows pulse animation on icon).
    /// </summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>
    /// Gets or sets whether the item content is collapsible.
    /// </summary>
    [Parameter]
    public bool IsCollapsible { get; set; }

    /// <summary>
    /// Gets or sets the default open state when collapsible.
    /// </summary>
    [Parameter]
    public bool DefaultOpen { get; set; } = true;

    /// <summary>
    /// Gets or sets the detail content that is shown/hidden when collapsible.
    /// </summary>
    [Parameter]
    public RenderFragment? DetailContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the connector line.
    /// Use to customize the connector height (e.g., "min-h-16" for taller connectors).
    /// </summary>
    [Parameter]
    public string? ConnectorClass { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the item.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the main content (TimelineContent with header and description).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets custom icon content to replace the default icon.
    /// </summary>
    [Parameter]
    public RenderFragment? IconContent { get; set; }

    /// <summary>
    /// Gets or sets the time/date content displayed in the date column.
    /// </summary>
    [Parameter]
    public RenderFragment? TimeContent { get; set; }

    /// <summary>
    /// Shorthand: title text for the timeline entry.
    /// When set and <see cref="ChildContent"/> is null, auto-renders the full content sub-component tree.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Shorthand: time/date text for the timeline entry.
    /// Used with <see cref="Title"/> for auto-rendered content.
    /// </summary>
    [Parameter]
    public string? Time { get; set; }

    /// <summary>
    /// Shorthand: description text for the timeline entry.
    /// Used with <see cref="Title"/> for auto-rendered content.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    private bool UseShorthand => ChildContent == null && Title != null;

    /// <summary>
    /// Captures any additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private TimelineAlign Align => ParentTimeline?.Align ?? TimelineAlign.Center;

    private bool IsReversed => Align == TimelineAlign.Alternate && _itemIndex % 2 != 0;

    protected override void OnInitialized() =>
        _itemIndex = ParentTimeline?.RegisterItem() ?? 0;

    private string CssClass => ClassNames.cn(
        "bb:relative bb:w-full",
        Class
    );

    private string ContentGridClass => ClassNames.cn(
        "bb:grid bb:gap-4 bb:items-start",
        Align switch
        {
            TimelineAlign.Left => "bb:grid-cols-[auto_1fr]",
            TimelineAlign.Right => "bb:grid-cols-[1fr_auto]",
            _ => "bb:grid-cols-[1fr_auto_1fr]"
        },
        Status == TimelineStatus.InProgress ? "aria-current-step" : null
    );

    private const string IconWrapperClass = "bb:relative bb:z-[1]";

    private string IconMinHeightClass => IconSize switch
    {
        TimelineSize.Small => "bb:min-h-8",
        TimelineSize.Medium => "bb:min-h-10",
        TimelineSize.Large => "bb:min-h-12",
        _ => "bb:min-h-10"
    };

    private string ContentWrapperClass => ClassNames.cn(
        "bb:grid bb:items-center",
        IconMinHeightClass
    );

    private string ContentWrapperEndClass => ClassNames.cn(
        "bb:grid bb:items-center bb:justify-items-end",
        IconMinHeightClass
    );

    private string ConnectorGapClass => (ParentTimeline?.Size ?? TimelineSize.Medium) switch
    {
        TimelineSize.Small => "bb:-mb-2",
        TimelineSize.Medium => "bb:-mb-4",
        TimelineSize.Large => "bb:-mb-6",
        _ => "bb:-mb-4"
    };

    private string ComputedConnectorClass => ClassNames.cn(
        "bb:flex-1 bb:min-h-16",
        ConnectorGapClass,
        ConnectorClass
    );

    private string DateColumnClass => ClassNames.cn(
        "bb:flex bb:flex-col bb:justify-center",
        IconMinHeightClass
    );
}
