using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorUI.Components.Field;

/// <summary>
/// A semantic fieldset container for grouping related form controls.
/// </summary>
/// <remarks>
/// <para>
/// The FieldSet component renders a native HTML fieldset element,
/// providing semantic grouping for related form fields. It maintains
/// proper accessibility for keyboard navigation and assistive technologies.
/// </para>
/// <para>
/// Features:
/// - Semantic HTML fieldset element
/// - Integration with FieldLegend for group labeling
/// - Proper accessibility for grouped controls
/// - Flexible styling via Tailwind CSS
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldSet&gt;
///     &lt;FieldLegend&gt;Personal Information&lt;/FieldLegend&gt;
///     &lt;Field&gt;...&lt;/Field&gt;
///     &lt;Field&gt;...&lt;/Field&gt;
/// &lt;/FieldSet&gt;
/// </code>
/// </example>
public partial class FieldSet : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the fieldset.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the fieldset.
    /// </summary>
    /// <remarks>
    /// Typically contains a FieldLegend followed by multiple Field components
    /// for grouped form controls.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the fieldset element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the fieldset element.
    /// </summary>
    /// <remarks>
    /// Combines base styles with custom classes using the cn() utility.
    /// Removes default browser styling and applies consistent spacing.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Remove default browser styling
        "border-0 p-0 m-0",
        // Spacing for grouped fields
        "space-y-4",
        // Custom classes (if provided)
        Class
    );
}
