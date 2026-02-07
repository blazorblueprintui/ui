using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorBlueprint.Components.Field;

/// <summary>
/// A field component that organizes form elements with flexible layout options.
/// </summary>
/// <remarks>
/// <para>
/// The Field component enables developers to combine labels, controls, and help text
/// to compose accessible form fields. It renders with role="group" for proper semantic
/// labeling and supports vertical, horizontal, and responsive orientations.
/// </para>
/// <para>
/// Features:
/// - Three orientation modes: vertical (default), horizontal, and responsive
/// - Flexible layout using CSS grid and flexbox
/// - Support for invalid/error states via data-invalid attribute
/// - Proper ARIA role="group" for accessibility
/// - Integration with FieldLabel, FieldContent, FieldDescription, and FieldError
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Field Orientation="FieldOrientation.Vertical"&gt;
///     &lt;FieldLabel&gt;Email&lt;/FieldLabel&gt;
///     &lt;FieldContent&gt;
///         &lt;Input Type="email" /&gt;
///         &lt;FieldDescription&gt;We'll never share your email.&lt;/FieldDescription&gt;
///     &lt;/FieldContent&gt;
/// &lt;/Field&gt;
/// </code>
/// </example>
public partial class Field : ComponentBase
{
    private static readonly string[] VerticalClasses = new[]
    {
        // Full-width vertical stacking (default)
        "flex-col w-full"
    };

    private static readonly string[] HorizontalClasses = new[]
    {
        // Row-based layout with aligned items
        "flex-row items-start",
        // Adjust spacing for checkbox/radio alignment
        "[&:has([data-slot=checkbox])]:items-center",
        "[&:has([data-slot=radio])]:items-center",
        "[&:has([data-slot=switch])]:items-center"
    };

    private static readonly string[] ResponsiveClasses = new[]
    {
        // Vertical by default, horizontal at medium breakpoint
        "flex-col w-full",
        "@md:flex-row @md:items-start",
        // Adjust spacing for checkbox/radio alignment at medium+
        "@md:[&:has([data-slot=checkbox])]:items-center",
        "@md:[&:has([data-slot=radio])]:items-center",
        "@md:[&:has([data-slot=switch])]:items-center"
    };

    /// <summary>
    /// Gets or sets the orientation of the field layout.
    /// </summary>
    /// <remarks>
    /// Controls the layout direction and behavior:
    /// - Vertical: Stacks label above control (default, mobile-first)
    /// - Horizontal: Places label beside control with aligned items
    /// - Responsive: Automatically switches from vertical to horizontal at medium breakpoint
    /// Default value is <see cref="FieldOrientation.Vertical"/>.
    /// </remarks>
    [Parameter]
    public FieldOrientation Orientation { get; set; } = FieldOrientation.Vertical;

    /// <summary>
    /// Gets or sets whether the field is in an invalid/error state.
    /// </summary>
    /// <remarks>
    /// When true, applies error styling via the data-invalid attribute.
    /// This enables conditional styling for validation errors.
    /// Default value is false.
    /// </remarks>
    [Parameter]
    public bool IsInvalid { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the field.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the field.
    /// </summary>
    /// <remarks>
    /// Typically contains FieldLabel, FieldContent, FieldDescription,
    /// and FieldError components for a complete form field.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the field element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the field element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base flex/grid layout with gap spacing
    /// - Orientation-specific classes (vertical, horizontal, or responsive)
    /// - Error state styling via data-invalid attribute selector
    /// - Custom classes from the Class parameter
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base styles - flex container with gap
        "flex gap-2",

        // Orientation-specific styles
        Orientation switch
        {
            FieldOrientation.Vertical => string.Join(" ", VerticalClasses),

            FieldOrientation.Horizontal => string.Join(" ", HorizontalClasses),

            FieldOrientation.Responsive => string.Join(" ", ResponsiveClasses),

            _ => string.Empty
        },

        // Error state styling
        "data-[invalid=true]:text-destructive",

        // Custom classes (if provided)
        Class
    );
}

/// <summary>
/// Defines the orientation options for a field.
/// </summary>
public enum FieldOrientation
{
    /// <summary>
    /// Label stacked above control (mobile-first, default).
    /// </summary>
    Vertical,

    /// <summary>
    /// Label beside control with horizontal alignment.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Automatically switches from vertical to horizontal at medium breakpoint.
    /// Uses container queries with @md prefix.
    /// </summary>
    Responsive
}
