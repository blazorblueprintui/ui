using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A separator component that visually or semantically divides content.
/// </summary>
/// <remarks>
/// <para>
/// The Separator component provides a flexible divider that can be used to create
/// visual separation between sections of content. It follows the shadcn/ui design system
/// and supports both horizontal and vertical orientations.
/// </para>
/// <para>
/// Features:
/// - Horizontal and vertical orientation
/// - Semantic vs decorative modes (affects ARIA attributes)
/// - Accessible with proper ARIA roles
/// - RTL (Right-to-Left) support
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Separator Orientation="SeparatorOrientation.Horizontal" /&gt;
///
/// &lt;Separator Orientation="SeparatorOrientation.Vertical" Decorative="false" /&gt;
/// </code>
/// </example>
public partial class BbSeparator : ComponentBase
{
    /// <summary>
    /// Gets or sets the orientation of the separator.
    /// </summary>
    /// <remarks>
    /// Determines whether the separator is displayed horizontally or vertically.
    /// Default value is <see cref="SeparatorOrientation.Horizontal"/>.
    /// </remarks>
    [Parameter]
    public SeparatorOrientation Orientation { get; set; } = SeparatorOrientation.Horizontal;

    /// <summary>
    /// Gets or sets whether the separator is purely decorative.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When true (default), the separator is treated as decorative (role="none") and
    /// hidden from assistive technologies.
    /// </para>
    /// <para>
    /// When false, the separator is semantic (role="separator") and will be announced
    /// to screen readers, with the orientation specified via aria-orientation.
    /// </para>
    /// <para>
    /// Set to false when the separator provides meaningful structural information
    /// about content hierarchy.
    /// </para>
    /// </remarks>
    [Parameter]
    public bool Decorative { get; set; } = true;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the separator.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the separator element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base separator styles (shrink-0, bg-border)
    /// - Orientation-specific classes (width/height)
    /// - Custom classes from the Class parameter
    /// </remarks>
    /// <summary>The visible line pattern.</summary>
    [Parameter] public SeparatorLineStyle LineStyle { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:shrink-0 bb:bg-border",
        Orientation == SeparatorOrientation.Horizontal
            ? "bb:h-[1px] bb:w-full"
            : "bb:h-full bb:w-[1px]",
        LineStyle != SeparatorLineStyle.Solid ? "bb:bg-transparent bb:border-border" : null,
        LineStyle == SeparatorLineStyle.Dashed ? "bb:border-dashed" : LineStyle == SeparatorLineStyle.Dotted ? "bb:border-dotted" : null,
        LineStyle != SeparatorLineStyle.Solid ? Orientation == SeparatorOrientation.Horizontal ? "bb:h-0 bb:border-t" : "bb:w-0 bb:border-l" : null,
        Class
    );

    private BlazorBlueprint.Primitives.Separator.SeparatorOrientation MappedOrientation =>
        Orientation == SeparatorOrientation.Vertical
            ? BlazorBlueprint.Primitives.Separator.SeparatorOrientation.Vertical
            : BlazorBlueprint.Primitives.Separator.SeparatorOrientation.Horizontal;
}
