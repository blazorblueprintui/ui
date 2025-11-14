using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace BlazorUI.Components.Field;

/// <summary>
/// An accessible error message container for displaying validation errors.
/// </summary>
/// <remarks>
/// <para>
/// The FieldError component displays validation error messages with proper
/// styling and accessibility attributes. It supports both an errors array
/// for multiple messages and a ChildContent slot for custom error rendering.
/// </para>
/// <para>
/// Features:
/// - Array-based error display with bullet points
/// - Custom error content via ChildContent
/// - Destructive color styling for visibility
/// - Support for aria-describedby association with inputs
/// - Automatic rendering only when errors are present
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldError Errors="@validationErrors" /&gt;
///
/// // Or with custom content:
/// &lt;FieldError&gt;
///     &lt;span&gt;Password must be at least 8 characters&lt;/span&gt;
/// &lt;/FieldError&gt;
/// </code>
/// </example>
public partial class FieldError : ComponentBase
{
    /// <summary>
    /// Gets or sets the array of error messages to display.
    /// </summary>
    /// <remarks>
    /// When provided, each error is rendered as a bulleted list item.
    /// If both Errors and ChildContent are provided, Errors takes precedence.
    /// Component only renders when errors are present or ChildContent is not null.
    /// </remarks>
    [Parameter]
    public IEnumerable<string>? Errors { get; set; }

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
    /// Gets or sets additional CSS classes to apply to the error container.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets custom error content to be rendered.
    /// </summary>
    /// <remarks>
    /// Used when you need custom error rendering beyond simple text messages.
    /// If both Errors and ChildContent are provided, Errors takes precedence.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the error element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the error element.
    /// </summary>
    /// <remarks>
    /// Combines typography, destructive color, and custom classes using the cn() utility.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Typography - smaller size
        "text-sm",
        // Font weight for visibility
        "font-medium",
        // Destructive color for errors
        "text-destructive",
        // Custom classes (if provided)
        Class
    );
}
