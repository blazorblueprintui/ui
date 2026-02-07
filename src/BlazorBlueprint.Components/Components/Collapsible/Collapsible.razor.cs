using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Collapsible;

/// <summary>
/// A styled container component that provides collapsible functionality with expandable/collapsible content sections.
/// </summary>
/// <remarks>
/// <para>
/// The Collapsible component is a styled wrapper around Collapsible that follows the shadcn/ui
/// design system and implements an accessible disclosure pattern (WCAG 2.1 APG). It applies default
/// shadcn styling while maintaining all the behavior and accessibility of the primitive.
/// </para>
/// <para>
/// This is a composite component pattern consisting of:
/// <list type="bullet">
/// <item><see cref="Collapsible"/> - Styled container that manages state</item>
/// <item><see cref="CollapsibleTrigger"/> - Styled button that toggles the open/closed state</item>
/// <item><see cref="CollapsibleContent"/> - Styled content area that expands/collapses</item>
/// </list>
/// </para>
/// <para>
/// Common use cases include:
/// <list type="bullet">
/// <item>Accordion-style FAQ sections</item>
/// <item>Expandable navigation menus</item>
/// <item>Collapsible form sections</item>
/// <item>Sidebar menu items with sub-items</item>
/// </list>
/// </para>
/// <para>
/// The component supports controlled and uncontrolled modes:
/// <list type="bullet">
/// <item>Controlled: Parent manages state via Open parameter and OpenChanged callback</item>
/// <item>Uncontrolled: Component manages its own internal state</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Basic collapsible (uncontrolled):
/// <code>
/// &lt;Collapsible&gt;
///     &lt;CollapsibleTrigger&gt;
///         &lt;Button&gt;Toggle&lt;/Button&gt;
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;
///         &lt;p&gt;This content can be expanded or collapsed.&lt;/p&gt;
///     &lt;/Collapsible&gt;
/// </code>
///
/// Controlled collapsible with custom state:
/// <code>
/// &lt;Collapsible Open="@isOpen" OpenChanged="@((bool open) => isOpen = open)"&gt;
///     &lt;CollapsibleTrigger&gt;
///         &lt;Button&gt;@(isOpen ? "Collapse" : "Expand")&lt;/Button&gt;
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;
///         &lt;p&gt;Controlled content visibility.&lt;/p&gt;
///     &lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
///
/// @@code {
///     private bool isOpen = false;
/// }
/// </code>
///
/// Disabled collapsible:
/// <code>
/// &lt;Collapsible Disabled&gt;
///     &lt;CollapsibleTrigger&gt;
///         &lt;Button&gt;Cannot Toggle&lt;/Button&gt;
///     &lt;/CollapsibleTrigger&gt;
///     &lt;CollapsibleContent&gt;
///         &lt;p&gt;This collapsible cannot be toggled.&lt;/p&gt;
///     &lt;/CollapsibleContent&gt;
/// &lt;/Collapsible&gt;
/// </code>
/// </example>
public partial class Collapsible : ComponentBase
{
    private bool _isOpen;

    /// <summary>
    /// Gets or sets a value indicating whether the collapsible is currently expanded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is open (content visible); otherwise, <c>false</c>.
    /// Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// Supports two-way binding via @bind-Open syntax.
    /// </remarks>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the open state changes.
    /// </summary>
    /// <value>
    /// An <see cref="EventCallback{Boolean}"/> that receives the new open state,
    /// or <c>null</c> if no callback is provided.
    /// </value>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Gets or sets the default open state for uncontrolled mode.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible should be open by default; otherwise, <c>false</c>.
    /// Default is <c>false</c>.
    /// </value>
    [Parameter]
    public bool DefaultOpen { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the collapsible is disabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the collapsible is disabled and cannot be toggled;
    /// otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS class names, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// Use this parameter to customize the container's appearance and layout.
    /// Common Tailwind utilities include:
    /// <list type="bullet">
    /// <item>Borders: <c>border rounded-lg</c></item>
    /// <item>Padding: <c>p-4</c>, <c>px-6</c></item>
    /// <item>Background: <c>bg-muted</c>, <c>bg-card</c></item>
    /// <item>Spacing: <c>space-y-2</c>, <c>mb-4</c></item>
    /// </list>
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the child content to be rendered inside the collapsible container.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> containing the child components, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// Typically contains:
    /// <list type="bullet">
    /// <item><see cref="CollapsibleTrigger"/> - The button/trigger element</item>
    /// <item><see cref="CollapsibleContent"/> - The expandable content area</item>
    /// </list>
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized() =>
        // Initialize with DefaultOpen if not controlled
        _isOpen = OpenChanged.HasDelegate ? Open : DefaultOpen;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // If controlled (has callback), sync with Open parameter
        if (OpenChanged.HasDelegate)
        {
            _isOpen = Open;
        }
    }

    private string? CssClass => ClassNames.cn(Class);

    private async Task HandleOpenChanged(bool newValue)
    {
        _isOpen = newValue;

        if (OpenChanged.HasDelegate)
        {
            await OpenChanged.InvokeAsync(newValue);
        }
    }
}
