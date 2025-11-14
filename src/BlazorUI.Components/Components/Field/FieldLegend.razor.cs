using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorUI.Components.Field;

/// <summary>
/// A legend element for labeling fieldsets or field groups.
/// </summary>
/// <remarks>
/// <para>
/// The FieldLegend component provides a title/label for grouped form controls.
/// It supports two variants: a semantic legend element (for use with FieldSet)
/// or a div with role="group" (for use with FieldGroup).
/// </para>
/// <para>
/// Features:
/// - Two rendering modes: legend element or div with role="group"
/// - Consistent typography and spacing
/// - ARIA support for accessibility
/// - Integration with FieldSet and FieldGroup
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;FieldSet&gt;
///     &lt;FieldLegend Variant="FieldLegendVariant.Legend"&gt;
///         Shipping Address
///     &lt;/FieldLegend&gt;
///     &lt;Field&gt;...&lt;/Field&gt;
/// &lt;/FieldSet&gt;
/// </code>
/// </example>
public partial class FieldLegend : ComponentBase
{
    /// <summary>
    /// Gets or sets the variant of the legend element.
    /// </summary>
    /// <remarks>
    /// Controls the HTML element used:
    /// - Legend: Renders a semantic legend element (use with FieldSet)
    /// - Label: Renders a div with role="group" (use with FieldGroup)
    /// Default value is <see cref="FieldLegendVariant.Legend"/>.
    /// </remarks>
    [Parameter]
    public FieldLegendVariant Variant { get; set; } = FieldLegendVariant.Legend;

    /// <summary>
    /// Gets or sets the ARIA label when using Label variant.
    /// </summary>
    /// <remarks>
    /// Provides an accessible name for the group when variant is Label.
    /// Only applicable when Variant is FieldLegendVariant.Label.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the legend.
    /// </summary>
    /// <remarks>
    /// Custom classes are merged with the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the legend.
    /// </summary>
    /// <remarks>
    /// Typically contains text describing the grouped form fields.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the legend element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the legend element.
    /// </summary>
    /// <remarks>
    /// Combines typography, spacing, and custom classes using the cn() utility.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Typography
        "text-sm font-medium leading-none",
        // Spacing
        "mb-3",
        // Custom classes (if provided)
        Class
    );
}

/// <summary>
/// Defines the variant options for a field legend.
/// </summary>
public enum FieldLegendVariant
{
    /// <summary>
    /// Renders a semantic legend element (use with FieldSet).
    /// </summary>
    Legend,

    /// <summary>
    /// Renders a div with role="group" (use with FieldGroup).
    /// </summary>
    Label
}
