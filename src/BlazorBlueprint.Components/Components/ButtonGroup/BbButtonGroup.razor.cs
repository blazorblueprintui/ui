using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBlueprint.Components;

/// <summary>
/// A button group component that visually connects related buttons together.
/// </summary>
/// <remarks>
/// <para>
/// The ButtonGroup component groups related buttons together with connected styling,
/// following the shadcn/ui design system. It supports both horizontal and vertical
/// orientation and handles focus states, borders, and rounded corners automatically.
/// </para>
/// <para>
/// Features:
/// - Horizontal and vertical orientation
/// - Automatic border and corner styling for connected appearance
/// - Focus state management with z-index
/// - Proper ARIA role="group" for accessibility
/// - Support for nested button groups (gaps between groups)
/// - Seamless integration with Button components
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;ButtonGroup&gt;
///     &lt;Button Variant="ButtonVariant.Outline"&gt;Archive&lt;/Button&gt;
///     &lt;Button Variant="ButtonVariant.Outline"&gt;Report&lt;/Button&gt;
/// &lt;/ButtonGroup&gt;
/// </code>
/// </example>
public partial class BbButtonGroup : ComponentBase
{
    private static readonly string[] HorizontalClasses =
    [
        // Remove borders and rounded corners between adjacent buttons
        "bb:[&>button:not(:first-child):not(:last-child)]:rounded-none",
        "bb:[&>button:not(:first-child)]:border-l-0",
        "bb:[&>button:first-child:not(:only-child)]:rounded-r-none",
        "bb:[&>button:last-child:not(:only-child)]:rounded-l-none",

        // Handle separators - buttons adjacent to separators keep their borders
        "bb:[&>button:has(+[data-slot=separator])]:rounded-r-none",
        "bb:[&>[data-slot=separator]+button]:rounded-l-none bb:[&>[data-slot=separator]+button]:!border-l bb:[&>[data-slot=separator]+button]:border-input",

        // Handle nested button groups with gaps
        "bb:[&>[data-slot=button-group]:not(:first-child)]:ml-2",

        // Focus state z-index (bring focused button above others)
        "bb:[&>button:focus]:relative bb:[&>button:focus]:z-10"
    ];

    private static readonly string[] VerticalClasses =
    [
        // Vertical layout
        "bb:flex-col",

        // Remove borders and rounded corners between adjacent buttons
        "bb:[&>button:not(:first-child):not(:last-child)]:rounded-none",
        "bb:[&>button:not(:first-child)]:border-t-0",
        "bb:[&>button:first-child:not(:only-child)]:rounded-b-none",
        "bb:[&>button:last-child:not(:only-child)]:rounded-t-none",

        // Handle separators - buttons adjacent to separators keep their borders
        "bb:[&>button:has(+[data-slot=separator])]:rounded-b-none",
        "bb:[&>[data-slot=separator]+button]:rounded-t-none bb:[&>[data-slot=separator]+button]:!border-t bb:[&>[data-slot=separator]+button]:border-input",

        // Handle nested button groups with gaps
        "bb:[&>[data-slot=button-group]:not(:first-child)]:mt-2",

        // Focus state z-index (bring focused button above others)
        "bb:[&>button:focus]:relative bb:[&>button:focus]:z-10"
    ];

    /// <summary>
    /// Gets or sets the orientation of the button group.
    /// </summary>
    /// <remarks>
    /// Controls whether buttons are arranged horizontally (default) or vertically.
    /// Default value is <see cref="ButtonGroupOrientation.Horizontal"/>.
    /// </remarks>
    [Parameter]
    public ButtonGroupOrientation Orientation { get; set; } = ButtonGroupOrientation.Horizontal;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the button group.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the button group.
    /// </summary>
    /// <remarks>
    /// Provides a name for the group when role="group" is used.
    /// Helps assistive technologies convey the group's purpose.
    /// Recommended for button groups that perform a specific function (e.g., "Text formatting", "Email actions").
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the button group.
    /// </summary>
    /// <remarks>
    /// Typically contains Button components, but can also contain nested
    /// ButtonGroup components for creating complex layouts.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the button group element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the button group element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base flex styles for layout
    /// - Orientation-specific classes (horizontal or vertical)
    /// - Border and rounded corner removal for connected buttons
    /// - Focus state z-index management
    /// - Gap handling for nested button groups
    /// - Custom classes from the Class parameter
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base styles - flex container with role="group"
        "bb:isolate bb:inline-flex bb:items-stretch",

        // Orientation-specific styles
        Orientation switch
        {
            ButtonGroupOrientation.Horizontal => string.Join(" ", HorizontalClasses),
            ButtonGroupOrientation.Vertical => string.Join(" ", VerticalClasses),
            _ => string.Empty
        },

        // Custom classes (if provided)
        Class
    );
}

/// <summary>
/// Defines the orientation options for a button group.
/// </summary>
public enum ButtonGroupOrientation
{
    /// <summary>
    /// Buttons are arranged horizontally (left to right).
    /// </summary>
    Horizontal,

    /// <summary>
    /// Buttons are arranged vertically (top to bottom).
    /// </summary>
    Vertical
}
