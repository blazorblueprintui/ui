using Microsoft.AspNetCore.Components;
using BlazorBlueprint.Icons.Heroicons.Data;

namespace BlazorBlueprint.Icons.Heroicons.Components;

/// <summary>
/// A Blazor component for rendering Heroicons SVG icons.
/// Supports 4 variants: Outline (24x24), Solid (24x24), Mini (20x20), and Micro (16x16).
/// </summary>
public partial class HeroIcon : ComponentBase
{
    /// <summary>
    /// The name of the icon to render (case-insensitive).
    /// Example: "camera", "home", "user"
    /// </summary>
    [Parameter, EditorRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The icon variant to render.
    /// Default is Outline (24x24, stroke-based).
    /// </summary>
    [Parameter]
    public HeroIconVariant Variant { get; set; } = HeroIconVariant.Outline;

    /// <summary>
    /// The size of the icon in pixels.
    /// If not specified, uses the default size for the variant:
    /// - Outline: 24px
    /// - Solid: 24px
    /// - Mini: 20px
    /// - Micro: 16px
    /// </summary>
    [Parameter]
    public int? Size { get; set; }

    /// <summary>
    /// The color of the icon. Supports CSS color values.
    /// Default is "currentColor" (inherits from parent).
    /// Examples: "red", "#FF0000", "var(--primary)", "rgb(255, 0, 0)"
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "currentColor";

    /// <summary>
    /// The stroke width for outline icons.
    /// Only applies to Outline variant. Ignored for Solid, Mini, and Micro.
    /// Default is 1.5 (Heroicons design standard for outline).
    /// </summary>
    [Parameter]
    public double? StrokeWidth { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the icon.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// ARIA label for accessibility (screen readers).
    /// Recommended for icon-only buttons.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the SVG element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// The SVG content for the icon.
    /// Retrieved from HeroIconData based on Name and Variant.
    /// Removes hardcoded stroke and fill attributes to allow the Color parameter to take effect.
    /// </summary>
    private string? SvgContent
    {
        get
        {
            var content = HeroIconData.GetIcon(Name, Variant);
            if (content == null)
            {
                return null;
            }

            // Remove stroke and fill attributes from the SVG content
            // This allows the outer SVG's stroke/fill attributes (from Color parameter) to take effect
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"\s+(stroke|fill)=""[^""]*""",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return content;
        }
    }

    /// <summary>
    /// The computed size based on variant or user-specified size.
    /// </summary>
    private int ComputedSize
    {
        get
        {
            if (Size.HasValue)
            {
                return Size.Value;
            }

            return Variant switch
            {
                HeroIconVariant.Mini => 20,
                HeroIconVariant.Micro => 16,
                _ => 24  // Outline and Solid default to 24
            };
        }
    }

    /// <summary>
    /// The computed stroke width for outline icons.
    /// </summary>
    private double ComputedStrokeWidth => StrokeWidth ?? 1.5;

    /// <summary>
    /// The viewBox for the icon based on variant.
    /// </summary>
    private string ViewBox => Variant switch
    {
        HeroIconVariant.Mini => "0 0 20 20",
        HeroIconVariant.Micro => "0 0 16 16",
        _ => "0 0 24 24"  // Outline and Solid use 24x24 viewBox
    };

    /// <summary>
    /// Whether to apply stroke attributes (only for Outline variant).
    /// </summary>
    private bool IsOutline => Variant == HeroIconVariant.Outline;

    /// <summary>
    /// The fill attribute value based on variant.
    /// </summary>
    private string Fill => IsOutline ? "none" : Color;

    /// <summary>
    /// The stroke attribute value (only for Outline variant).
    /// </summary>
    private string? Stroke => IsOutline ? Color : null;

    /// <summary>
    /// The combined CSS class string.
    /// </summary>
    private string CssClass => string.IsNullOrEmpty(Class) ? string.Empty : Class;
}
