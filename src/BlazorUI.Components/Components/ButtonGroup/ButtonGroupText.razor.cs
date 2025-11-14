using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorUI.Components.ButtonGroup;

/// <summary>
/// A text/label component designed to be used within a ButtonGroup.
/// </summary>
/// <remarks>
/// <para>
/// The ButtonGroupText component provides a styled container for text or labels
/// within a button group. It uses muted background styling and proper padding
/// to visually distinguish text sections from interactive buttons.
/// </para>
/// <para>
/// Features:
/// - Muted background styling for visual distinction
/// - Consistent padding and spacing with buttons
/// - Proper icon sizing for SVG elements
/// - Seamless integration with ButtonGroup layouts
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;ButtonGroup&gt;
///     &lt;ButtonGroupText&gt;Actions:&lt;/ButtonGroupText&gt;
///     &lt;Button Variant="ButtonVariant.Outline"&gt;Edit&lt;/Button&gt;
///     &lt;Button Variant="ButtonVariant.Outline"&gt;Delete&lt;/Button&gt;
/// &lt;/ButtonGroup&gt;
/// </code>
/// </example>
public partial class ButtonGroupText : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the text container.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the text container.
    /// </summary>
    /// <remarks>
    /// Can contain text, icons, or any other markup.
    /// Icons will automatically be sized to match the button group's icon size.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the text container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the text container element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base flex layout and padding
    /// - Muted background styling
    /// - Icon sizing for SVG elements
    /// - Custom classes from the Class parameter
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base styles - inline flex with padding
        "inline-flex items-center justify-center",
        "px-3 py-2",

        // Muted background styling
        "bg-muted text-muted-foreground",
        "text-sm font-medium",

        // Icon sizing (SVG elements)
        "[&_svg]:size-4 [&_svg]:shrink-0",

        // Custom classes (if provided)
        Class
    );
}
