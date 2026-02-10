using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBlueprint.Components.Field;

/// <summary>
/// A label component for form inputs with proper accessibility attributes.
/// </summary>
/// <remarks>
/// <para>
/// The FieldLabel component provides a styled label element for form controls.
/// It supports the 'for' attribute to associate with input elements and includes
/// peer-based error styling for validation feedback.
/// </para>
/// <para>
/// Features:
/// - Semantic label element with 'for' attribute support
/// - Typography and spacing consistent with design system
/// - Error state styling via peer selectors
/// - Support for nested fields within FieldContent
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldLabel For="email-input"&gt;Email Address&lt;/FieldLabel&gt;
/// &lt;Input Id="email-input" Type="email" /&gt;
/// </code>
/// </example>
public partial class FieldLabel : ComponentBase
{
    /// <summary>
    /// Gets or sets the ID of the form control this label is associated with.
    /// </summary>
    /// <remarks>
    /// Should match the ID of the input element for proper accessibility.
    /// Enables clicking the label to focus the associated input.
    /// </remarks>
    [Parameter]
    public string? For { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the label.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the label.
    /// </summary>
    /// <remarks>
    /// Typically contains text describing the associated form control.
    /// Can also include required indicators, tooltips, etc.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the label element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the label element.
    /// </summary>
    /// <remarks>
    /// Combines typography, spacing, error state, and custom classes using the cn() utility.
    /// Includes peer-based styling for error states when used with aria-invalid inputs.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Typography
        "text-sm font-medium leading-none",
        // Prevent shrinking in horizontal layouts
        "shrink-0",
        // Error state via peer selector (when input has aria-invalid="true")
        "peer-aria-[invalid=true]:text-destructive",
        // Custom classes (if provided)
        Class
    );
}
