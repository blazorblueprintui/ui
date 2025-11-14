namespace BlazorUI.Components.Collapsible;

/// <summary>
/// Provides context state for a Collapsible component and its sub-components.
/// </summary>
/// <remarks>
/// <para>
/// CollapsibleContext is used internally by the Collapsible component system to coordinate
/// state between the container (Collapsible), trigger (CollapsibleTrigger), and content
/// (CollapsibleContent) components via Blazor's CascadingValue mechanism.
/// </para>
/// <para>
/// This context pattern enables:
/// <list type="bullet">
/// <item>Shared open/closed state across trigger and content</item>
/// <item>Centralized toggle behavior</item>
/// <item>Disabled state propagation to child components</item>
/// <item>Coordinated animations and ARIA attributes</item>
/// </list>
/// </para>
/// <para>
/// Components consuming this context:
/// <list type="bullet">
/// <item><see cref="CollapsibleTrigger"/> - Reads state and calls toggle</item>
/// <item><see cref="CollapsibleContent"/> - Reads state to show/hide content</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Typical usage (internal to Collapsible component):
/// <code>
/// &lt;CascadingValue Value="@context"&gt;
///     @ChildContent
/// &lt;/CascadingValue&gt;
/// </code>
/// </example>
public class CollapsibleContext
{
    /// <summary>
    /// Gets or sets a value indicating whether the collapsible content is expanded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is open (content visible); otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property reflects the current state of the collapsible component.
    /// When <c>true</c>, the <see cref="CollapsibleContent"/> will be visible and
    /// accessible. When <c>false</c>, the content will be hidden (typically via
    /// CSS display: none or height: 0).
    /// <para>
    /// This state is controlled by the parent <see cref="Collapsible"/> component
    /// and can be toggled via the <see cref="Toggle"/> method or by setting the
    /// Open parameter on the Collapsible component.
    /// </para>
    /// </remarks>
    public bool Open { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collapsible is disabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is disabled and cannot be toggled;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// When <c>true</c>, the collapsible cannot be toggled by user interaction.
    /// The <see cref="CollapsibleTrigger"/> will be marked as disabled and will not
    /// respond to clicks. The <see cref="Toggle"/> method will not perform any action
    /// when the collapsible is disabled.
    /// <para>
    /// Use cases for disabled state:
    /// <list type="bullet">
    /// <item>During async operations that need to prevent user input</item>
    /// <item>When form validation rules prevent expansion</item>
    /// <item>When content is loading or unavailable</item>
    /// </list>
    /// </para>
    /// </remarks>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the callback method to toggle the open/closed state.
    /// </summary>
    /// <value>
    /// A delegate that toggles the <see cref="Open"/> state when invoked,
    /// or <c>null</c> if no toggle handler is set.
    /// </value>
    /// <remarks>
    /// This method is provided by the parent <see cref="Collapsible"/> component
    /// and should be invoked by the <see cref="CollapsibleTrigger"/> when clicked.
    /// The method will only execute if <see cref="Disabled"/> is <c>false</c>.
    /// <para>
    /// The toggle operation typically:
    /// <list type="bullet">
    /// <item>Flips the <see cref="Open"/> state (true ↔ false)</item>
    /// <item>Triggers a re-render of the Collapsible component tree</item>
    /// <item>Updates ARIA attributes (aria-expanded) on the trigger</item>
    /// <item>Shows/hides the <see cref="CollapsibleContent"/> with animation</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// Internal usage in CollapsibleTrigger:
    /// <code>
    /// private async Task HandleClick()
    /// {
    ///     if (Context?.Toggle != null &amp;&amp; !Context.Disabled)
    ///     {
    ///         await Context.Toggle.Invoke();
    ///     }
    /// }
    /// </code>
    /// </example>
    public Func<Task>? Toggle { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollapsibleContext"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a new context with default values:
    /// <list type="bullet">
    /// <item><see cref="Open"/> = <c>false</c> (closed by default)</item>
    /// <item><see cref="Disabled"/> = <c>false</c> (enabled by default)</item>
    /// <item><see cref="Toggle"/> = <c>null</c> (must be set by parent component)</item>
    /// </list>
    /// </remarks>
    public CollapsibleContext()
    {
        Open = false;
        Disabled = false;
        Toggle = null;
    }
}
