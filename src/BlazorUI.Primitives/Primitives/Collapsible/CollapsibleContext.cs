namespace BlazorUI.Primitives.Collapsible;

/// <summary>
/// Provides context state for a Collapsible component and its sub-components.
/// </summary>
/// <remarks>
/// <para>
/// CollapsibleContext is used internally by the Collapsible primitive system to coordinate
/// state between the container (Collapsible), trigger (CollapsibleTrigger), and content
/// (CollapsibleContent) components via Blazor's CascadingValue mechanism.
/// </para>
/// <para>
/// This context pattern enables:
/// <list type="bullet">
/// <item>Shared open/closed state across trigger and content</item>
/// <item>Centralized toggle behavior</item>
/// <item>Disabled state propagation to child components</item>
/// <item>Coordinated ARIA attributes</item>
/// </list>
/// </para>
/// </remarks>
public class CollapsibleContext
{
    /// <summary>
    /// Gets or sets a value indicating whether the collapsible content is expanded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is open (content visible); otherwise, <c>false</c>.
    /// </value>
    public bool Open { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collapsible is disabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is disabled and cannot be toggled;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the callback method to toggle the open/closed state.
    /// </summary>
    /// <value>
    /// A delegate that toggles the <see cref="Open"/> state when invoked,
    /// or <c>null</c> if no toggle handler is set.
    /// </value>
    public Func<Task>? Toggle { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollapsibleContext"/> class.
    /// </summary>
    public CollapsibleContext()
    {
        Open = false;
        Disabled = false;
        Toggle = null;
    }
}
