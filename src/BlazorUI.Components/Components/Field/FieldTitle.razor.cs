using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorUI.Components.Field;

/// <summary>
/// A title element for use within FieldContent.
/// </summary>
/// <remarks>
/// <para>
/// The FieldTitle component provides a styled title for complex field layouts
/// where you need a heading within the FieldContent area. It uses medium font
/// weight and consistent typography for visual hierarchy.
/// </para>
/// <para>
/// Features:
/// - Medium font weight for visual prominence
/// - Consistent typography with the design system
/// - Proper spacing when used with other field components
/// - Useful for grouped controls or complex field layouts
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldContent&gt;
///     &lt;FieldTitle&gt;Notification Preferences&lt;/FieldTitle&gt;
///     &lt;Checkbox&gt;Email notifications&lt;/Checkbox&gt;
///     &lt;Checkbox&gt;SMS notifications&lt;/Checkbox&gt;
/// &lt;/FieldContent&gt;
/// </code>
/// </example>
public partial class FieldTitle : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the title.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the title.
    /// </summary>
    /// <remarks>
    /// Typically contains text describing the field or group of controls.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the title element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the title element.
    /// </summary>
    /// <remarks>
    /// Combines typography and custom classes using the cn() utility.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Typography - medium weight for prominence
        "text-sm font-medium leading-none",
        // Custom classes (if provided)
        Class
    );
}
