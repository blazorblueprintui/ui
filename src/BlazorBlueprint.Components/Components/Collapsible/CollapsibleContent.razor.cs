using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Collapsible;

/// <summary>
/// A styled content component that displays collapsible content controlled by a CollapsibleTrigger.
/// </summary>
/// <remarks>
/// <para>
/// The CollapsibleContent component is a styled wrapper around the primitive CollapsibleContent
/// that applies shadcn/ui styling while maintaining all the behavior and accessibility.
/// </para>
/// <para>
/// This component must be used as a child of a <see cref="Collapsible"/> component.
/// </para>
/// <para>
/// Accessibility features:
/// <list type="bullet">
/// <item>Semantic region role for screen readers</item>
/// <item>aria-hidden attribute for proper screen reader behavior</item>
/// <item>Conditional rendering (not just hidden with CSS)</item>
/// <item>Smooth animations via CSS transitions (when styled)</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic collapsible content:
/// <code>
/// &lt;Collapsible&gt;
///     &lt;CollapsibleTrigger&gt;
///         &lt;Button&gt;Show Content&lt;/Button&gt;
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;
///         &lt;p&gt;This content can be expanded or collapsed.&lt;/p&gt;
///     &lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
/// </code>
///
/// Styled content with padding:
/// <code>
/// &lt;CollapsibleContent Class="p-4 border-t"&gt;
///     &lt;div class="space-y-2"&gt;
///         &lt;p&gt;Paragraph 1&lt;/p&gt;
///         &lt;p&gt;Paragraph 2&lt;/p&gt;
///     &lt;/div&gt;
/// &lt;/CollapsibleContent&gt;
/// </code>
///
/// Animated content with transitions:
/// <code>
/// &lt;CollapsibleContent Class="transition-all duration-300 ease-in-out overflow-hidden"&gt;
///     &lt;div class="p-4"&gt;
///         &lt;p&gt;Content with smooth slide animation&lt;/p&gt;
///     &lt;/div&gt;
/// &lt;/CollapsibleContent&gt;
/// </code>
/// </example>
public partial class CollapsibleContent : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the content container element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS class names, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// Use this parameter to style the content area and add animations.
    /// Common Tailwind utilities include:
    /// <list type="bullet">
    /// <item>Padding: <c>p-4</c>, <c>px-6 py-4</c></item>
    /// <item>Borders: <c>border-t</c>, <c>border-x</c></item>
    /// <item>Background: <c>bg-muted</c>, <c>bg-card</c></item>
    /// <item>Transitions: <c>transition-all duration-300</c></item>
    /// <item>Animation: <c>animate-in slide-in-from-top</c></item>
    /// <item>Overflow: <c>overflow-hidden</c> (for smooth height transitions)</item>
    /// </list>
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered when the collapsible is expanded.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the collapsible content, or <c>null</c>.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the CSS classes for the grid container (for animation).
    /// </summary>
    /// <remarks>
    /// These classes are applied to the outer container to enable smooth height transitions.
    /// User's custom classes are applied to an inner wrapper to avoid padding affecting the grid collapse.
    /// </remarks>
    private string? CssClass => ClassNames.cn(Class);

    private static string GridCssClass =>
        "grid grid-rows-[0fr] transition-[grid-template-rows] duration-200 ease-out data-[state=open]:grid-rows-[1fr]";
}
