using BlazorBlueprint.Primitives.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Primitives.Collapsible;

/// <summary>
/// A headless trigger button component that toggles the open/closed state of a Collapsible.
/// </summary>
/// <remarks>
/// <para>
/// The CollapsibleTrigger component is a headless primitive that handles all interaction behavior,
/// keyboard navigation, and accessibility features for toggling a collapsible. It provides no
/// default styling.
/// </para>
/// <para>
/// This component must be used as a child of a <see cref="Collapsible"/> component.
/// It will automatically receive the collapsible state via Blazor's CascadingParameter.
/// </para>
/// <para>
/// Accessibility features (WCAG 2.1 AA):
/// <list type="bullet">
/// <item>Semantic button element with proper role</item>
/// <item>aria-expanded attribute reflects current state</item>
/// <item>Keyboard support (Space/Enter keys)</item>
/// <item>Disabled state with appropriate ARIA attributes</item>
/// <item>data-state attribute for CSS styling hooks</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic headless trigger:
/// <code>
/// &lt;Collapsible&gt;
///     &lt;CollapsibleTrigger&gt;
///         Toggle Content
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;...&lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
/// </code>
///
/// Styled trigger with custom classes:
/// <code>
/// &lt;CollapsibleTrigger class="flex items-center gap-2 px-4 py-2 hover:bg-accent"&gt;
///     &lt;svg&gt;...&lt;/svg&gt;
///     &lt;span&gt;Show More&lt;/span&gt;
/// &lt;/CollapsibleTrigger&gt;
/// </code>
/// </example>
public partial class CollapsibleTrigger : ComponentBase
{
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
    /// Gets or sets the content to be rendered inside the trigger.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the trigger content, or <c>null</c>.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// When true, the trigger does not render its own button element.
    /// Instead, it passes trigger behavior via TriggerContext to child components.
    /// The child component must consume TriggerContext and apply click behavior.
    /// </summary>
    [Parameter]
    public bool AsChild { get; set; } = false;

    /// <summary>
    /// Gets or sets additional attributes to be applied to the button element.
    /// </summary>
    /// <value>
    /// A dictionary of additional HTML attributes including class, style, etc.
    /// </value>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Context passed to child components when AsChild is true.
    /// </summary>
    private TriggerContext TriggerContext => new()
    {
        IsOpen = Context?.Open ?? false,
        Toggle = () => Context?.Toggle?.Invoke()
    };

    /// <summary>
    /// Handles click events on the trigger to toggle the collapsible state.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleClick(MouseEventArgs args)
    {
        if (Context?.Toggle != null && !(Context.Disabled))
        {
            await Context.Toggle.Invoke();
        }
    }

    /// <summary>
    /// Handles keyboard events to support keyboard navigation (Space/Enter keys).
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// Implements WCAG 2.1 keyboard accessibility by responding to Space and Enter keys.
    /// </remarks>
    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (Context?.Disabled ?? true)
        {
            return;
        }

        if (args.Key == " " || args.Key == "Enter")
        {
            if (Context?.Toggle != null)
            {
                await Context.Toggle.Invoke();
            }
        }
    }
}
