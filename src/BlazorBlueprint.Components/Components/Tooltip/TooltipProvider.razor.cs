using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Tooltip;

/// <summary>
/// A provider component that configures tooltip behavior for all child Tooltip components.
/// </summary>
/// <remarks>
/// <para>
/// The TooltipProvider component follows the shadcn/ui design system and wraps a section
/// of the application to provide shared tooltip configuration. It cascades a
/// <see cref="TooltipProviderContext"/> to all descendant <see cref="Tooltip"/> components.
/// </para>
/// <para>
/// <strong>When to use:</strong>
/// <list type="bullet">
/// <item>Wrap your entire application root to provide default tooltip behavior</item>
/// <item>Wrap specific sections to customize tooltip timing for that area</item>
/// <item>Nest providers to override tooltip configuration in sub-trees</item>
/// </list>
/// </para>
/// <para>
/// <strong>Benefits of using TooltipProvider:</strong>
/// <list type="bullet">
/// <item>Consistent tooltip delays across your application</item>
/// <item>"Tooltip discovery mode" - faster subsequent tooltips after first interaction</item>
/// <item>Centralized configuration - change behavior without modifying individual tooltips</item>
/// <item>Section-specific customization - different timing for different UI areas</item>
/// </list>
/// </para>
/// <para>
/// <strong>Note:</strong> While TooltipProvider is recommended for consistent behavior,
/// individual <see cref="Tooltip"/> components can function without a provider using
/// default delay values.
/// </para>
/// </remarks>
/// <example>
/// Basic provider wrapping the entire application:
/// <code>
/// &lt;TooltipProvider&gt;
///     &lt;Router AppAssembly="typeof(App).Assembly"&gt;
///         @* Application content with tooltips *@
///     &lt;/Router&gt;
/// &lt;/TooltipProvider&gt;
/// </code>
///
/// Custom delay configuration for a toolbar:
/// <code>
/// &lt;TooltipProvider DelayDuration="200" SkipDelayDuration="500"&gt;
///     &lt;Toolbar&gt;
///         &lt;Tooltip&gt;
///             &lt;TooltipTrigger&gt;&lt;Button&gt;Save&lt;/Button&gt;&lt;/TooltipTrigger&gt;
///             &lt;TooltipContent&gt;Save document&lt;/TooltipContent&gt;
///         &lt;/Tooltip&gt;
///         @* More toolbar items with tooltips *@
///     &lt;/Toolbar&gt;
/// &lt;/TooltipProvider&gt;
/// </code>
///
/// Nested providers for different timing in sections:
/// <code>
/// &lt;TooltipProvider DelayDuration="700"&gt; @* Default for app *@
///     &lt;MainContent /&gt;
///
///     &lt;TooltipProvider DelayDuration="200"&gt; @* Fast for toolbar *@
///         &lt;Toolbar /&gt;
///     &lt;/TooltipProvider&gt;
/// &lt;/TooltipProvider&gt;
/// </code>
/// </example>
public partial class TooltipProvider : ComponentBase
{
    private TooltipProviderContext providerContext = new();

    /// <summary>
    /// Gets or sets the duration in milliseconds to wait before showing a tooltip.
    /// </summary>
    /// <value>
    /// The delay duration in milliseconds. Default is 700ms.
    /// </value>
    /// <remarks>
    /// <para>
    /// This delay applies to all child <see cref="Tooltip"/> components. When a user
    /// hovers over or focuses a tooltip trigger, the tooltip will wait this duration
    /// before appearing.
    /// </para>
    /// <para>
    /// Recommended values:
    /// <list type="bullet">
    /// <item><strong>0-200ms:</strong> Very fast (toolbars, frequently accessed UI)</item>
    /// <item><strong>300-500ms:</strong> Fast (interactive elements)</item>
    /// <item><strong>600-800ms:</strong> Standard (balanced, default)</item>
    /// <item><strong>900-1200ms:</strong> Slow (detailed information)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Individual tooltips cannot override this value - they inherit it from their
    /// nearest TooltipProvider ancestor.
    /// </para>
    /// </remarks>
    [Parameter]
    public int DelayDuration { get; set; } = 700;

    /// <summary>
    /// Gets or sets the duration in milliseconds during which the delay is skipped for subsequent tooltips.
    /// </summary>
    /// <value>
    /// The skip delay duration in milliseconds. Default is 300ms.
    /// </value>
    /// <remarks>
    /// <para>
    /// After a user sees one tooltip, subsequent tooltips within this time window appear
    /// immediately without delay. This creates a fluid "tooltip discovery mode" experience.
    /// </para>
    /// <para>
    /// Recommended values:
    /// <list type="bullet">
    /// <item><strong>0ms:</strong> Disable skip behavior</item>
    /// <item><strong>200-400ms:</strong> Short window (closely grouped elements)</item>
    /// <item><strong>500-1000ms:</strong> Standard window (comfortable exploration)</item>
    /// <item><strong>1500-3000ms:</strong> Long window (very forgiving)</item>
    /// </list>
    /// </para>
    /// <para>
    /// Set to 0 to disable the skip delay feature entirely. All tooltips will always
    /// wait the full <see cref="DelayDuration"/>.
    /// </para>
    /// </remarks>
    [Parameter]
    public int SkipDelayDuration { get; set; } = 300;

    /// <summary>
    /// Gets or sets the child content to be rendered within the provider.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing child components, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// All child <see cref="Tooltip"/> components will receive the tooltip configuration
    /// from this provider via Blazor's CascadingValue mechanism.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Initializes the provider context on first render.
    /// </summary>
    /// <remarks>
    /// Creates the <see cref="TooltipProviderContext"/> with the configured delay values.
    /// This context is then cascaded to all descendant components.
    /// </remarks>
    protected override void OnInitialized() =>
        providerContext = new TooltipProviderContext(DelayDuration, SkipDelayDuration);

    /// <summary>
    /// Updates the provider context when parameters change.
    /// </summary>
    /// <remarks>
    /// Ensures that changes to <see cref="DelayDuration"/> or <see cref="SkipDelayDuration"/>
    /// parameters are reflected in the cascaded context. This allows dynamic tooltip
    /// behavior changes at runtime.
    /// </remarks>
    protected override void OnParametersSet()
    {
        providerContext.DelayDuration = DelayDuration;
        providerContext.SkipDelayDuration = SkipDelayDuration;
    }
}
