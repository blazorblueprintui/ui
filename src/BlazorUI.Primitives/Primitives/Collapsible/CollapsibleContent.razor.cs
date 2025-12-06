using Microsoft.AspNetCore.Components;

namespace BlazorUI.Primitives.Collapsible;

/// <summary>
/// A headless content component that displays collapsible content controlled by a CollapsibleTrigger.
/// </summary>
/// <remarks>
/// <para>
/// The CollapsibleContent component is a headless primitive that handles content visibility
/// based on the collapsible state. It provides no default styling, only behavior and accessibility.
/// </para>
/// <para>
/// This component must be used as a child of a <see cref="Collapsible"/> component.
/// It receives the collapsible state via Blazor's CascadingParameter and renders
/// content conditionally based on the Open state.
/// </para>
/// <para>
/// Accessibility features (WCAG 2.1 AA):
/// <list type="bullet">
/// <item>Semantic region role for screen readers</item>
/// <item>aria-hidden attribute for proper screen reader behavior</item>
/// <item>Conditional rendering via display: none</item>
/// <item>data-state attribute for CSS styling hooks</item>
/// </list>
/// </para>
/// <para>
/// The component uses inline display: none for hiding content, which:
/// <list type="bullet">
/// <item>Prevents focus trap issues with hidden interactive elements</item>
/// <item>Ensures screen readers don't navigate hidden content</item>
/// <item>Allows CSS transitions when combined with data-state attribute</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic headless content:
/// <code>
/// &lt;Collapsible&gt;
///     &lt;CollapsibleTrigger&gt;
///         Show Content
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;
///         This content can be expanded or collapsed.
///     &lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
/// </code>
///
/// Styled content with custom classes:
/// <code>
/// &lt;CollapsibleContent class="p-4 border-t transition-all"&gt;
///     &lt;div class="space-y-2"&gt;
///         &lt;p&gt;Paragraph 1&lt;/p&gt;
///         &lt;p&gt;Paragraph 2&lt;/p&gt;
///     &lt;/div&gt;
/// &lt;/CollapsibleContent&gt;
/// </code>
///
/// Animated content using data-state:
/// <code>
/// &lt;CollapsibleContent class="overflow-hidden data-[state=open]:animate-slideDown data-[state=closed]:animate-slideUp"&gt;
///     &lt;div class="p-4"&gt;
///         &lt;p&gt;Content with smooth animation&lt;/p&gt;
///     &lt;/div&gt;
/// &lt;/CollapsibleContent&gt;
/// </code>
/// </example>
public partial class CollapsibleContent : ComponentBase
{
    private bool _shouldRender => ForceMount || (Context?.Open ?? false);

    /// <summary>
    /// Gets the cascaded collapsible context from the parent Collapsible component.
    /// </summary>
    /// <value>
    /// A <see cref="CollapsibleContext"/> instance provided by the parent, or <c>null</c>
    /// if this component is not nested within a <see cref="Collapsible"/>.
    /// </value>
    [CascadingParameter]
    public CollapsibleContext? Context { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered when the collapsible is expanded.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the collapsible content, or <c>null</c>.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether to force mount the content even when the collapsible is closed.
    /// When true, content remains mounted (useful for CSS animations when styled).
    /// When false (default), content is unmounted when closed.
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; } = false;

    /// <summary>
    /// Gets or sets additional attributes to be applied to the content container element.
    /// </summary>
    /// <value>
    /// A dictionary of additional HTML attributes including class, style, etc.
    /// </value>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}
