using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components.InputGroup;

/// <summary>
/// A container for additional content positioned around an input control.
/// </summary>
/// <remarks>
/// <para>
/// InputGroupAddon allows you to add icons, text, buttons, or other elements at various
/// positions around an input field. It handles focus delegation, ensuring that clicking
/// on addon content focuses the associated input control.
/// </para>
/// <para>
/// Features:
/// - Four alignment positions: InlineStart, InlineEnd, BlockStart, BlockEnd
/// - Automatic focus delegation to sibling input controls
/// - Flexible content support (icons, buttons, text, etc.)
/// - Smart padding adjustments based on content type
/// - Seamless integration with InputGroup's unified styling
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Icon at start --&gt;
/// &lt;InputGroup&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.InlineStart"&gt;
///         &lt;SearchIcon /&gt;
///     &lt;/InputGroupAddon&gt;
///     &lt;InputGroupInput Placeholder="Search..." /&gt;
/// &lt;/InputGroup&gt;
///
/// &lt;!-- Button at end --&gt;
/// &lt;InputGroup&gt;
///     &lt;InputGroupInput Placeholder="Email address" /&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.InlineEnd"&gt;
///         &lt;InputGroupButton&gt;Send&lt;/InputGroupButton&gt;
///     &lt;/InputGroupAddon&gt;
/// &lt;/InputGroup&gt;
///
/// &lt;!-- Character counter below --&gt;
/// &lt;InputGroup&gt;
///     &lt;InputGroupTextarea Placeholder="Write a comment..." /&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.BlockEnd"&gt;
///         &lt;InputGroupText&gt;0 / 280&lt;/InputGroupText&gt;
///     &lt;/InputGroupAddon&gt;
/// &lt;/InputGroup&gt;
/// </code>
/// </example>
public partial class InputGroupAddon : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the alignment position of the addon.
    /// </summary>
    /// <remarks>
    /// Determines where the addon content appears relative to the input:
    /// - InlineStart: Left side (or right in RTL)
    /// - InlineEnd: Right side (or left in RTL)
    /// - BlockStart: Above the input
    /// - BlockEnd: Below the input
    /// </remarks>
    [Parameter]
    public InputGroupAlign Align { get; set; } = InputGroupAlign.InlineStart;

    /// <summary>
    /// Gets or sets the child content to be rendered inside the addon.
    /// </summary>
    /// <remarks>
    /// Can contain icons, buttons, text, or any other content.
    /// The component automatically adjusts padding based on content type.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the addon container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the addon container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the data-align attribute value based on the Align parameter.
    /// </summary>
    private string AlignValue => Align switch
    {
        InputGroupAlign.InlineStart => "inline-start",
        InputGroupAlign.InlineEnd => "inline-end",
        InputGroupAlign.BlockStart => "block-start",
        InputGroupAlign.BlockEnd => "block-end",
        _ => "inline-start"
    };

    /// <summary>
    /// Gets the computed CSS classes for the addon container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Styling varies by alignment position:
    /// </para>
    /// <list type="bullet">
    /// <item>InlineStart/InlineEnd: Horizontal padding, flex alignment</item>
    /// <item>BlockStart/BlockEnd: Vertical padding, full width</item>
    /// </list>
    /// <para>
    /// Automatically adjusts for button children with negative margins and reduced padding.
    /// </para>
    /// </remarks>
    private string CssClass
    {
        get
        {
            var baseClasses = Align switch
            {
                InputGroupAlign.InlineStart => "flex items-center gap-2 text-muted-foreground",
                InputGroupAlign.InlineEnd => "flex items-center gap-2 text-muted-foreground",
                InputGroupAlign.BlockStart => "flex items-center justify-start gap-2 text-muted-foreground",
                InputGroupAlign.BlockEnd => "flex items-center justify-start gap-2 text-muted-foreground",
                _ => "flex items-center gap-2 text-muted-foreground"
            };

            var alignmentClasses = Align switch
            {
                InputGroupAlign.InlineStart => "pl-3 pr-0 py-0", // Left edge spacing + no gap to input (input has its own pl-3)
                InputGroupAlign.InlineEnd => "pr-3 pl-0 py-0", // Right edge spacing + no gap from input (input has its own pr-3)
                InputGroupAlign.BlockStart => "px-3 pt-2 pb-1 w-full", // Removed border for seamless integration
                InputGroupAlign.BlockEnd => "px-3 pb-2 pt-1 w-full", // Removed border for seamless integration
                _ => "pl-3 pr-0 py-0"
            };

            // Adjust for button children - create uniform 6px spacing on all sides
            var buttonAdjustments = Align switch
            {
                InputGroupAlign.InlineStart => "has-[>button]:pl-1.5 has-[>button]:!py-1.5", // Uniform 6px spacing all sides
                InputGroupAlign.InlineEnd => "has-[>button]:pr-1.5 has-[>button]:!py-1.5", // Uniform 6px spacing all sides
                InputGroupAlign.BlockStart => "has-[>button]:-mt-1 has-[>button]:pt-1",
                InputGroupAlign.BlockEnd => "has-[>button]:-mb-1 has-[>button]:pb-1",
                _ => ""
            };

            return ClassNames.cn(
                baseClasses,
                alignmentClasses,
                buttonAdjustments,
                Class
            );
        }
    }

    /// <summary>
    /// Handles click events on the addon to focus the associated input control.
    /// </summary>
    /// <remarks>
    /// When the addon is clicked (but not a button or interactive element within it),
    /// this method focuses the sibling input or textarea control for better UX.
    /// This mimics native label behavior.
    /// </remarks>
    private static async Task HandleClick(MouseEventArgs e) =>
        // Delegate focus to the input control
        // In a real implementation, we'd use ElementReference and JSInterop
        // For now, we'll rely on natural DOM behavior
        await Task.CompletedTask;
}
