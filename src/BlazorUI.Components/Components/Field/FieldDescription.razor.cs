using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorUI.Components.Field;

/// <summary>
/// A helper text component for providing additional information about form fields.
/// </summary>
/// <remarks>
/// <para>
/// The FieldDescription component displays supplementary information about a form field,
/// such as format requirements, examples, or helpful hints. It uses muted text color
/// and smaller font size to differentiate from the main label.
/// </para>
/// <para>
/// Features:
/// - Smaller, muted text styling for helper content
/// - Automatic line balancing for improved readability
/// - Proper spacing within FieldContent
/// - Support for aria-describedby association with inputs
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldContent&gt;
///     &lt;Input Type="email" aria-describedby="email-description" /&gt;
///     &lt;FieldDescription Id="email-description"&gt;
///         We'll never share your email with anyone else.
///     &lt;/FieldDescription&gt;
/// &lt;/FieldContent&gt;
/// </code>
/// </example>
public partial class FieldDescription : ComponentBase
{
    /// <summary>
    /// Gets or sets the ID for aria-describedby association.
    /// </summary>
    /// <remarks>
    /// When set, this ID should be referenced by the associated input's
    /// aria-describedby attribute for proper accessibility.
    /// </remarks>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the description.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the description.
    /// </summary>
    /// <remarks>
    /// Typically contains helpful text explaining the purpose or format
    /// of the associated form field.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the description element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the description element.
    /// </summary>
    /// <remarks>
    /// Combines typography, color, and custom classes using the cn() utility.
    /// Includes text-balance for improved line wrapping.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Typography - smaller size
        "text-sm",
        // Color - muted for helper text
        "text-muted-foreground",
        // Text balancing for better line breaks
        "text-balance",
        // Custom classes (if provided)
        Class
    );
}
