using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Timeline;

/// <summary>
/// A timeline component for displaying a vertical list of events or items.
/// </summary>
/// <remarks>
/// <para>
/// The Timeline component provides a chronological display of events with customizable
/// icons, connectors, and content. It follows the shadcn-timeline design system.
/// </para>
/// <para>
/// Features:
/// - 3 size variants (Small, Medium, Large)
/// - Multiple color themes (Primary, Secondary, Muted, Accent, Destructive)
/// - Status-based styling (Completed, InProgress, Pending)
/// - Custom icons per item
/// - Accessible with semantic HTML (ordered list)
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Timeline&gt;
///     &lt;TimelineItem&gt;
///         &lt;TimelineContent&gt;
///             &lt;TimelineHeader&gt;
///                 &lt;TimelineTitle&gt;Event Title&lt;/TimelineTitle&gt;
///                 &lt;TimelineTime&gt;Jan 2024&lt;/TimelineTime&gt;
///             &lt;/TimelineHeader&gt;
///             &lt;TimelineDescription&gt;Event description&lt;/TimelineDescription&gt;
///         &lt;/TimelineContent&gt;
///     &lt;/TimelineItem&gt;
/// &lt;/Timeline&gt;
/// </code>
/// </example>
public partial class Timeline : ComponentBase
{
    private int _itemCounter;

    /// <summary>
    /// Gets or sets the size variant controlling gap spacing between items.
    /// </summary>
    [Parameter]
    public TimelineSize Size { get; set; } = TimelineSize.Medium;

    /// <summary>
    /// Gets or sets the layout alignment for timeline items.
    /// </summary>
    [Parameter]
    public TimelineAlign Align { get; set; } = TimelineAlign.Center;

    /// <summary>
    /// Gets or sets how the connector line fits between icons.
    /// Spaced (default) leaves a small ring gap; Connected makes the line touch each icon.
    /// </summary>
    [Parameter]
    public TimelineConnectorFit ConnectorFit { get; set; } = TimelineConnectorFit.Spaced;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the timeline.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the timeline (TimelineItem elements).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures any additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Registers a timeline item and returns its index (used for Alternate layout).
    /// </summary>
    internal int RegisterItem() => _itemCounter++;

    protected override void OnParametersSet() =>
        _itemCounter = 0;

    private string CssClass => ClassNames.cn(
        // Base styles
        "flex flex-col relative",
        // Size variants
        Size switch
        {
            TimelineSize.Small => "gap-2",
            TimelineSize.Medium => "gap-4",
            TimelineSize.Large => "gap-6",
            _ => "gap-4"
        },
        Class
    );
}
