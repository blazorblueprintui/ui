namespace BlazorUI.Components.Tooltip;

/// <summary>
/// Provides context state for an individual Tooltip component and its sub-components.
/// </summary>
/// <remarks>
/// <para>
/// TooltipContext is used internally by the Tooltip component system to coordinate state
/// between the container (Tooltip), trigger (TooltipTrigger), and content (TooltipContent)
/// components via Blazor's CascadingValue mechanism.
/// </para>
/// <para>
/// This context pattern enables:
/// <list type="bullet">
/// <item>Shared open/closed state between trigger and content</item>
/// <item>Coordinated hover and focus event handling</item>
/// <item>Consistent positioning information</item>
/// <item>Delayed show/hide behavior</item>
/// </list>
/// </para>
/// <para>
/// Components consuming this context:
/// <list type="bullet">
/// <item><see cref="TooltipTrigger"/> - Triggers tooltip on hover/focus</item>
/// <item><see cref="TooltipContent"/> - Displays tooltip content when open</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Typical usage (internal to Tooltip component):
/// <code>
/// &lt;CascadingValue Value="@context"&gt;
///     @ChildContent
/// &lt;/CascadingValue&gt;
/// </code>
/// </example>
public class TooltipContext
{
    /// <summary>
    /// Gets or sets a value indicating whether the tooltip is currently visible.
    /// </summary>
    /// <value>
    /// <c>true</c> if the tooltip is open (content visible); otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property reflects the current visibility state of the tooltip content.
    /// The state transitions occur based on:
    /// <list type="bullet">
    /// <item>Mouse enter/leave events on the trigger</item>
    /// <item>Focus/blur events on the trigger</item>
    /// <item>Configured delay durations from <see cref="TooltipProviderContext"/></item>
    /// <item>Programmatic control via <see cref="SetOpen"/> method</item>
    /// </list>
    /// </remarks>
    public bool Open { get; set; }

    /// <summary>
    /// Gets or sets the callback method to programmatically control the tooltip's open state.
    /// </summary>
    /// <value>
    /// A delegate that sets the tooltip's open state, or <c>null</c> if no handler is set.
    /// </value>
    /// <remarks>
    /// This method is provided by the parent <see cref="Tooltip"/> component and can be
    /// invoked by child components to manually show or hide the tooltip.
    /// <para>
    /// The method handles:
    /// <list type="bullet">
    /// <item>Immediate state updates for programmatic control</item>
    /// <item>Cancellation of pending delayed show/hide operations</item>
    /// <item>Coordination with hover and focus event handlers</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Internal usage in TooltipTrigger:
    /// <code>
    /// private async Task HandleMouseEnter()
    /// {
    ///     if (Context?.SetOpen != null)
    ///     {
    ///         await Context.SetOpen.Invoke(true);
    ///     }
    /// }
    /// </code>
    /// </example>
    public Func<bool, Task>? SetOpen { get; set; }

    /// <summary>
    /// Gets or sets the preferred placement position for the tooltip relative to its trigger.
    /// </summary>
    /// <value>
    /// A string indicating the tooltip placement. Default is "top".
    /// </value>
    /// <remarks>
    /// <para>
    /// Standard placement values (following Popper.js/Floating UI conventions):
    /// <list type="bullet">
    /// <item><strong>top:</strong> Above the trigger, centered</item>
    /// <item><strong>top-start:</strong> Above the trigger, aligned to start edge</item>
    /// <item><strong>top-end:</strong> Above the trigger, aligned to end edge</item>
    /// <item><strong>bottom:</strong> Below the trigger, centered (common default)</item>
    /// <item><strong>bottom-start:</strong> Below the trigger, aligned to start edge</item>
    /// <item><strong>bottom-end:</strong> Below the trigger, aligned to end edge</item>
    /// <item><strong>left:</strong> To the left of the trigger, centered vertically</item>
    /// <item><strong>left-start:</strong> To the left, aligned to top edge</item>
    /// <item><strong>left-end:</strong> To the left, aligned to bottom edge</item>
    /// <item><strong>right:</strong> To the right of the trigger, centered vertically</item>
    /// <item><strong>right-start:</strong> To the right, aligned to top edge</item>
    /// <item><strong>right-end:</strong> To the right, aligned to bottom edge</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Note:</strong> The actual placement may differ from the preferred placement
    /// if there isn't enough space in the viewport. Positioning logic should implement
    /// automatic flipping to ensure the tooltip remains visible.
    /// </para>
    /// <para>
    /// <strong>Future enhancement:</strong> This could be extended to return computed
    /// placement information including actual position, arrow coordinates, and flip status.
    /// </para>
    /// </remarks>
    public string Placement { get; set; } = "top";

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipContext"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a new context with default values:
    /// <list type="bullet">
    /// <item><see cref="Open"/> = <c>false</c> (hidden by default)</item>
    /// <item><see cref="SetOpen"/> = <c>null</c> (must be set by parent component)</item>
    /// <item><see cref="Placement"/> = "top" (default positioning)</item>
    /// </list>
    /// </remarks>
    public TooltipContext()
    {
        Open = false;
        SetOpen = null;
        Placement = "top";
    }
}
