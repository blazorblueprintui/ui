using BlazorUI.Components.Utilities;
using BlazorUI.Components.Separator;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.ButtonGroup;

/// <summary>
/// A separator component designed to be used within a ButtonGroup.
/// </summary>
/// <remarks>
/// <para>
/// The ButtonGroupSeparator component provides a visual divider between sections
/// within a button group. It automatically adjusts its orientation and spacing
/// based on the parent button group's layout.
/// </para>
/// <para>
/// Features:
/// - Automatic orientation support (horizontal/vertical)
/// - Proper margins for visual spacing within groups
/// - Wraps the Separator component with button-group-specific styling
/// - Seamless integration with ButtonGroup layouts
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;ButtonGroup&gt;
///     &lt;Button Variant="ButtonVariant.Outline"&gt;Edit&lt;/Button&gt;
///     &lt;ButtonGroupSeparator /&gt;
///     &lt;Button Variant="ButtonVariant.Outline"&gt;Delete&lt;/Button&gt;
/// &lt;/ButtonGroup&gt;
/// </code>
/// </example>
public partial class ButtonGroupSeparator : ComponentBase
{
    /// <summary>
    /// Gets or sets the orientation of the separator.
    /// </summary>
    /// <remarks>
    /// Should match the parent ButtonGroup's orientation.
    /// Default value is <see cref="SeparatorOrientation.Vertical"/> (for horizontal button groups).
    /// </remarks>
    [Parameter]
    public SeparatorOrientation Orientation { get; set; } = SeparatorOrientation.Vertical;

    /// <summary>
    /// Gets or sets whether the separator is purely decorative.
    /// </summary>
    /// <remarks>
    /// When true (default), the separator is treated as decorative and
    /// hidden from assistive technologies.
    /// </remarks>
    [Parameter]
    public bool Decorative { get; set; } = true;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the separator.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the separator wrapper element.
    /// </summary>
    /// <remarks>
    /// The wrapper provides flex display and orientation-specific margins.
    /// </remarks>
    private string WrapperCssClass => ClassNames.cn(
        // Flex display to properly contain the separator
        "flex items-center",

        // Orientation-specific margins (subtle spacing)
        Orientation == SeparatorOrientation.Vertical ? "mx-1" : "my-1"
    );

    /// <summary>
    /// Gets the computed CSS classes for the separator element itself.
    /// </summary>
    /// <remarks>
    /// Custom classes from the Class parameter.
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string? CssClass => Class;
}
