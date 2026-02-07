using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Collapsible;

/// <summary>
/// A styled trigger button component that toggles the open/closed state of a Collapsible.
/// </summary>
/// <remarks>
/// <para>
/// The CollapsibleTrigger component is a styled wrapper around the primitive CollapsibleTrigger
/// that applies shadcn/ui styling while maintaining all the behavior and accessibility.
/// </para>
/// <para>
/// This component must be used as a child of a <see cref="Collapsible"/> component.
/// </para>
/// <para>
/// Accessibility features:
/// <list type="bullet">
/// <item>Semantic button element with proper role</item>
/// <item>aria-expanded attribute reflects current state</item>
/// <item>Keyboard support (Space/Enter keys)</item>
/// <item>Disabled state with appropriate ARIA attributes</item>
/// <item>Focus management for keyboard navigation</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic trigger:
/// <code>
/// &lt;Collapsible&gt;
///     &lt;CollapsibleTrigger&gt;
///         Toggle Content
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;...&lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
/// </code>
///
/// Styled trigger with icon:
/// <code>
/// &lt;CollapsibleTrigger Class="flex items-center gap-2 px-4 py-2 hover:bg-accent"&gt;
///     &lt;LucideIcon Name="chevron-down" /&gt;
///     &lt;span&gt;Show More&lt;/span&gt;
/// &lt;/CollapsibleTrigger&gt;
/// </code>
/// </example>
public partial class CollapsibleTrigger : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the trigger button element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS class names, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// Common Tailwind utilities for styling triggers:
    /// <list type="bullet">
    /// <item>Flex layout: <c>flex items-center gap-2</c></item>
    /// <item>Padding: <c>px-4 py-2</c></item>
    /// <item>Hover states: <c>hover:bg-accent</c></item>
    /// <item>Transitions: <c>transition-all duration-200</c></item>
    /// </list>
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// When true, the trigger does not render its own button element.
    /// Instead, it passes trigger behavior via TriggerContext to child components.
    /// Use this when you want a custom component (like Button) to act as the trigger.
    /// </summary>
    [Parameter]
    public bool AsChild { get; set; } = false;

    /// <summary>
    /// Gets or sets the content to be rendered inside the trigger.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the trigger content, or <c>null</c>.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS class string for the trigger button element.
    /// </summary>
    /// <value>
    /// A string containing all CSS classes to be applied to the button element.
    /// </value>
    private string CssClass => ClassNames.cn("group", Class);
}
