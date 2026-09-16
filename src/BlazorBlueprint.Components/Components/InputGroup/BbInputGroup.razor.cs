using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A container component that groups input controls with additional content like icons, buttons, and text.
/// </summary>
/// <remarks>
/// <para>
/// The InputGroup component provides a flexible system for enhancing form inputs with contextual
/// information and actions. It supports multiple alignment positions for addons and manages
/// focus states across the entire group.
/// </para>
/// <para>
/// Features:
/// - Unified border and focus management for the entire group
/// - Multiple addon alignment options (inline-start, inline-end, block-start, block-end)
/// - Automatic height adjustment for inputs vs textareas
/// - Error state visualization via contained controls
/// - Dark mode compatible via CSS variables
/// - Seamless integration with existing Input and Button components
/// </para>
/// <para>
/// The component uses CSS selectors (has-[...]) to detect child component types and states,
/// enabling automatic styling adjustments based on content.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;InputGroup&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.InlineStart"&gt;
///         &lt;SearchIcon /&gt;
///     &lt;/InputGroupAddon&gt;
///     &lt;InputGroupInput Placeholder="Search..." /&gt;
/// &lt;/InputGroup&gt;
///
/// &lt;InputGroup&gt;
///     &lt;InputGroupInput Type="InputType.Email" Placeholder="Enter email" /&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.InlineEnd"&gt;
///         &lt;InputGroupButton&gt;Subscribe&lt;/InputGroupButton&gt;
///     &lt;/InputGroupAddon&gt;
/// &lt;/InputGroup&gt;
/// </code>
/// </example>
public partial class BbInputGroup : ComponentBase
{
    /// <summary>
    /// Gets or sets the child content to be rendered inside the input group.
    /// </summary>
    /// <remarks>
    /// Typically contains InputGroupInput/InputGroupTextarea and InputGroupAddon components.
    /// The order of child components affects the visual layout and keyboard navigation.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the input group container.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes using TailwindMerge,
    /// allowing for intelligent conflict resolution.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the input group container.
    /// </summary>
    /// <remarks>
    /// Allows for adding custom data attributes, ARIA attributes, or event handlers.
    /// </remarks>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the input group container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Combines base styles with dynamic classes based on child content:
    /// </para>
    /// <list type="bullet">
    /// <item>Base: Border, shadow, rounded corners, flex container</item>
    /// <item>Height: h-9 for inputs, h-auto for textareas or block addons</item>
    /// <item>Alignment: Dynamic padding based on inline addon positions</item>
    /// <item>Focus: Border color change when any child control is focused</item>
    /// <item>Error: Border color change when any child control has aria-invalid</item>
    /// </list>
    /// <para>
    /// Uses modern CSS :has() pseudo-class for detecting child states.
    /// </para>
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base container styles
        "bb:group/input-group bb:relative bb:flex bb:w-full",
        "bb:border bb:border-input bb:rounded-md bb:shadow-sm",
        "bb:bg-background bb:overflow-hidden",
        "bb:transition-colors bb:outline-none",

        // Default height and alignment for inputs
        "bb:h-9 bb:min-w-0 bb:items-center",

        // Auto height when containing textarea or block addons
        "bb:has-[>textarea]:h-auto bb:has-[>textarea]:items-start",
        "bb:has-[>[data-align=block-start]]:h-auto bb:has-[>[data-align=block-start]]:flex-col bb:has-[>[data-align=block-start]]:items-stretch",
        "bb:has-[>[data-align=block-end]]:h-auto bb:has-[>[data-align=block-end]]:flex-col bb:has-[>[data-align=block-end]]:items-stretch",

        // Reduce input padding when any inline addons present for compact spacing
        "bb:has-[>[data-align=inline-start]]:[&>input]:pl-1",
        "bb:has-[>[data-align=inline-start]]:[&>textarea]:pl-1",
        "bb:has-[>[data-align=inline-end]]:[&>input]:pr-1",
        "bb:has-[>[data-align=inline-end]]:[&>textarea]:pr-1",

        // Further reduce when button addons present for tight integration
        "bb:has-[>[data-align=inline-start]>button]:[&>input]:pl-1.5",
        "bb:has-[>[data-align=inline-start]>button]:[&>textarea]:pl-1.5",
        "bb:has-[>[data-align=inline-end]>button]:[&>input]:pr-1.5",
        "bb:has-[>[data-align=inline-end]>button]:[&>textarea]:pr-1.5",

        // Reduce input vertical padding when inline addons have buttons for uniform spacing
        "bb:has-[>[data-align=inline-start]>button]:[&>input]:py-1.5",
        "bb:has-[>[data-align=inline-end]>button]:[&>input]:py-1.5",
        "bb:has-[>[data-align=inline-start]>button]:[&>textarea]:py-1.5",
        "bb:has-[>[data-align=inline-end]>button]:[&>textarea]:py-1.5",

        // Focus state - when any control inside is focused
        "bb:has-[[data-slot=input-group-control]:focus-visible]:border-ring",

        // Error state - when any control inside has aria-invalid
        "bb:has-[[data-slot=input-group-control][aria-invalid=true]]:border-destructive",

        // Custom classes
        Class
    );
}
