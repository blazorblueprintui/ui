namespace BlazorUI.Components.Tooltip;

/// <summary>
/// Provides shared configuration context for all tooltips within a TooltipProvider.
/// </summary>
/// <remarks>
/// <para>
/// TooltipProviderContext is used to configure tooltip behavior globally within a section
/// of the application. It is provided by the <see cref="TooltipProvider"/> component and
/// cascaded to all child <see cref="Tooltip"/> components.
/// </para>
/// <para>
/// This context enables:
/// <list type="bullet">
/// <item>Consistent delay durations across multiple tooltips</item>
/// <item>Skip delay behavior for rapid tooltip switching</item>
/// <item>Centralized tooltip configuration management</item>
/// </list>
/// </para>
/// <para>
/// The provider pattern is particularly useful for:
/// <list type="bullet">
/// <item>Applications with many tooltips that should behave consistently</item>
/// <item>Implementing "tooltip discovery mode" where delays are skipped after first interaction</item>
/// <item>Customizing tooltip timing for different sections (e.g., faster for toolbar, slower for content)</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Typical usage (internal to TooltipProvider component):
/// <code>
/// &lt;CascadingValue Value="@providerContext"&gt;
///     @ChildContent
/// &lt;/CascadingValue&gt;
/// </code>
/// </example>
public class TooltipProviderContext
{
    /// <summary>
    /// Gets or sets the duration in milliseconds to wait before showing a tooltip.
    /// </summary>
    /// <value>
    /// The delay duration in milliseconds. Default is 700ms.
    /// </value>
    /// <remarks>
    /// <para>
    /// This delay applies when a user hovers over or focuses a tooltip trigger.
    /// The tooltip will not appear immediately but will wait for this duration to prevent
    /// accidental tooltip displays during quick mouse movements.
    /// </para>
    /// <para>
    /// Recommended values based on use case:
    /// <list type="bullet">
    /// <item><strong>0-200ms:</strong> Very fast, for frequently accessed UI (toolbars, icon buttons)</item>
    /// <item><strong>300-500ms:</strong> Fast, for interactive elements where hints are expected</item>
    /// <item><strong>600-800ms:</strong> Standard, balanced between discovery and intrusion (default)</item>
    /// <item><strong>900-1200ms:</strong> Slow, for detailed information that shouldn't distract</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Accessibility consideration:</strong>
    /// Longer delays reduce cognitive load for users with motor impairments who may
    /// accidentally hover over elements. However, too long delays can frustrate users
    /// seeking information.
    /// </para>
    /// </remarks>
    public int DelayDuration { get; set; } = 700;

    /// <summary>
    /// Gets or sets the duration in milliseconds during which the delay is skipped for subsequent tooltips.
    /// </summary>
    /// <value>
    /// The skip delay duration in milliseconds. Default is 300ms.
    /// </value>
    /// <remarks>
    /// <para>
    /// This implements "tooltip discovery mode" - after a user has seen one tooltip,
    /// subsequent tooltips within this time window will appear immediately without delay.
    /// This creates a more fluid experience when exploring multiple tooltip-enabled elements.
    /// </para>
    /// <para>
    /// <strong>How it works:</strong>
    /// <list type="number">
    /// <item>User hovers over Element A, waits DelayDuration (700ms), tooltip appears</item>
    /// <item>User moves to Element B within SkipDelayDuration (300ms)</item>
    /// <item>Tooltip for Element B appears immediately without delay</item>
    /// <item>If user waits longer than SkipDelayDuration before moving to Element C, the delay resets</item>
    /// </list>
    /// </para>
    /// <para>
    /// Recommended values:
    /// <list type="bullet">
    /// <item><strong>0ms:</strong> Disable skip behavior, always wait full delay</item>
    /// <item><strong>200-400ms:</strong> Short window, for closely grouped elements</item>
    /// <item><strong>500-1000ms:</strong> Standard window, allows comfortable exploration</item>
    /// <item><strong>1500-3000ms:</strong> Long window, very forgiving for slow interactions</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance note:</strong>
    /// This feature requires tracking the last tooltip close time. The overhead is minimal
    /// but can be disabled by setting this to 0.
    /// </para>
    /// </remarks>
    public int SkipDelayDuration { get; set; } = 300;

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipProviderContext"/> class with default values.
    /// </summary>
    /// <remarks>
    /// Creates a new context with:
    /// <list type="bullet">
    /// <item><see cref="DelayDuration"/> = 700ms (standard delay)</item>
    /// <item><see cref="SkipDelayDuration"/> = 300ms (short skip window)</item>
    /// </list>
    /// These defaults provide a balanced user experience suitable for most applications.
    /// </remarks>
    public TooltipProviderContext()
    {
        DelayDuration = 700;
        SkipDelayDuration = 300;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TooltipProviderContext"/> class with custom delay values.
    /// </summary>
    /// <param name="delayDuration">The duration in milliseconds to wait before showing a tooltip.</param>
    /// <param name="skipDelayDuration">
    /// The duration in milliseconds during which the delay is skipped for subsequent tooltips.
    /// </param>
    /// <remarks>
    /// Use this constructor to create a provider context with custom timing values
    /// tailored to your application's specific needs.
    /// </remarks>
    /// <example>
    /// Create a fast-response tooltip provider for a toolbar:
    /// <code>
    /// var context = new TooltipProviderContext(200, 500);
    /// </code>
    /// </example>
    public TooltipProviderContext(int delayDuration, int skipDelayDuration)
    {
        DelayDuration = delayDuration;
        SkipDelayDuration = skipDelayDuration;
    }
}
