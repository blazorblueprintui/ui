using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorUI.Components.Field;

/// <summary>
/// A visual divider for separating sections within field groups.
/// </summary>
/// <remarks>
/// <para>
/// The FieldSeparator component provides a horizontal line to visually
/// separate sections of fields within a FieldGroup or FieldSet. It uses
/// a subtle border color from the design system.
/// </para>
/// <para>
/// Features:
/// - Horizontal divider line
/// - Consistent border color from theme
/// - Proper spacing for visual separation
/// - Self-contained with no child content
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldGroup&gt;
///     &lt;Field&gt;...&lt;/Field&gt;
///     &lt;FieldSeparator /&gt;
///     &lt;Field&gt;...&lt;/Field&gt;
/// &lt;/FieldGroup&gt;
/// </code>
/// </example>
public partial class FieldSeparator : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the separator.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the separator element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the separator element.
    /// </summary>
    /// <remarks>
    /// Combines border styling and custom classes using the cn() utility.
    /// Creates a horizontal line with subtle border color.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Full width
        "w-full",
        // Horizontal border
        "border-t border-border",
        // Vertical spacing
        "my-2",
        // Custom classes (if provided)
        Class
    );
}
