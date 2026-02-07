using Microsoft.AspNetCore.Components;
using BlazorBlueprint.Icons.Feather.Data;

namespace BlazorBlueprint.Icons.Feather.Components;

/// <summary>
/// A Blazor component for rendering Feather SVG icons.
/// Feather icons are minimalist, stroke-based icons at 24x24 with 2px strokes.
/// </summary>
public partial class FeatherIcon : ComponentBase
{
    /// <summary>
    /// The name of the icon to render (case-insensitive).
    /// Example: "camera", "home", "user"
    /// </summary>
    [Parameter, EditorRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The size of the icon in pixels.
    /// Default is 24 (Feather icons are designed at 24x24).
    /// </summary>
    [Parameter]
    public int Size { get; set; } = 24;

    /// <summary>
    /// The color of the icon. Supports CSS color values.
    /// Default is "currentColor" (inherits from parent).
    /// Examples: "red", "#FF0000", "var(--primary)", "rgb(255, 0, 0)"
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "currentColor";

    /// <summary>
    /// The stroke width for the icon.
    /// Default is 2 (Feather icons design standard).
    /// </summary>
    [Parameter]
    public double StrokeWidth { get; set; } = 2.0;

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
    /// Retrieved from FeatherIconData based on Name.
    /// Removes hardcoded stroke attributes to allow the Color parameter to take effect.
    /// </summary>
    private string? SvgContent
    {
        get
        {
            var content = FeatherIconData.GetIcon(Name);
            if (content == null)
            {
                return null;
            }

            // Remove stroke="currentColor" and stroke="..." attributes from the SVG content
            // This allows the outer SVG's stroke attribute (from Color parameter) to take effect
            content = System.Text.RegularExpressions.Regex.Replace(
                content,
                @"\s+stroke=""[^""]*""",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return content;
        }
    }

    /// <summary>
    /// The combined CSS class string.
    /// </summary>
    private string CssClass => string.IsNullOrEmpty(Class) ? string.Empty : Class;
}
