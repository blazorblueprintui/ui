using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBlueprint.Components;

/// <summary>
/// A flex column container for grouping form controls and descriptions.
/// </summary>
/// <remarks>
/// <para>
/// The FieldContent component provides a consistent layout for stacking
/// form controls, descriptions, and error messages within a Field.
/// It uses flexbox with vertical stacking and automatic gap spacing.
/// </para>
/// <para>
/// Features:
/// - Flex column layout with consistent gap spacing
/// - Automatic stacking of child elements
/// - Integration with Input, FieldDescription, and FieldError components
/// - Full width by default for responsive layouts
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Field&gt;
///     &lt;FieldLabel&gt;Username&lt;/FieldLabel&gt;
///     &lt;FieldContent&gt;
///         &lt;Input Type="text" /&gt;
///         &lt;FieldDescription&gt;Choose a unique username&lt;/FieldDescription&gt;
///     &lt;/FieldContent&gt;
/// &lt;/Field&gt;
/// </code>
/// </example>
public partial class BbFieldContent : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the field content container.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the field content container.
    /// </summary>
    /// <remarks>
    /// Typically contains an input control, followed by optional
    /// FieldDescription and FieldError components.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the field content element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the field content element.
    /// </summary>
    /// <remarks>
    /// Combines flex layout, spacing, and custom classes using the cn() utility.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Flex column layout with gap
        "flex flex-col gap-1.5",
        // Full width for responsive behavior
        "w-full",
        // Custom classes (if provided)
        Class
    );
}
