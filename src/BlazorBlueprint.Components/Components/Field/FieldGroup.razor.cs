using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBlueprint.Components.Field;

/// <summary>
/// A layout wrapper for stacking multiple Field components.
/// </summary>
/// <remarks>
/// <para>
/// The FieldGroup component provides consistent spacing and layout for
/// multiple related fields. It supports vertical, horizontal, and responsive
/// orientations with automatic gap handling and container query support.
/// </para>
/// <para>
/// Features:
/// - Three orientation modes: vertical (default), horizontal, and responsive
/// - Automatic gap spacing between fields
/// - Container query support for responsive layouts (@container/field-group)
/// - Integration with Field components
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldGroup Orientation="FieldGroupOrientation.Responsive"&gt;
///     &lt;Field&gt;
///         &lt;FieldLabel&gt;First Name&lt;/FieldLabel&gt;
///         &lt;Input /&gt;
///     &lt;/Field&gt;
///     &lt;Field&gt;
///         &lt;FieldLabel&gt;Last Name&lt;/FieldLabel&gt;
///         &lt;Input /&gt;
///     &lt;/Field&gt;
/// &lt;/FieldGroup&gt;
/// </code>
/// </example>
public partial class FieldGroup : ComponentBase
{
    private static readonly string[] ResponsiveClasses = new[]
    {
        // Vertical by default
        "flex-col",
        // Container query setup for responsive behavior
        "@container/field-group",
        // Horizontal at medium container width
        "@md:flex-row @md:flex-wrap"
    };

    /// <summary>
    /// Gets or sets the orientation of the field group layout.
    /// </summary>
    /// <remarks>
    /// Controls the layout direction:
    /// - Vertical: Stacks fields vertically (default)
    /// - Horizontal: Places fields horizontally with wrap support
    /// - Responsive: Uses container queries to adapt layout based on available space
    /// Default value is <see cref="FieldGroupOrientation.Vertical"/>.
    /// </remarks>
    [Parameter]
    public FieldGroupOrientation Orientation { get; set; } = FieldGroupOrientation.Vertical;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the field group.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the field group.
    /// </summary>
    /// <remarks>
    /// Typically contains multiple Field components.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the field group element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the field group element.
    /// </summary>
    /// <remarks>
    /// Combines layout styles, spacing, and custom classes using the cn() utility.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base flex container
        "flex gap-4",

        // Orientation-specific styles
        Orientation switch
        {
            FieldGroupOrientation.Vertical => "flex-col",

            FieldGroupOrientation.Horizontal => "flex-row flex-wrap",

            FieldGroupOrientation.Responsive => string.Join(" ", ResponsiveClasses),

            _ => string.Empty
        },

        // Custom classes (if provided)
        Class
    );
}

/// <summary>
/// Defines the orientation options for a field group.
/// </summary>
public enum FieldGroupOrientation
{
    /// <summary>
    /// Fields are stacked vertically (default).
    /// </summary>
    Vertical,

    /// <summary>
    /// Fields are arranged horizontally with wrapping support.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Automatically adapts layout using container queries.
    /// </summary>
    Responsive
}
