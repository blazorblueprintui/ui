using Microsoft.AspNetCore.Components;
using BlazorBlueprint.Icons.FontAwesome.Data;

namespace BlazorBlueprint.Icons.FontAwesome.Components;

/// <summary>
/// A Blazor component for rendering Font Awesome Free SVG icons.
/// Supports 3 variants: Solid, Regular, and Brands.
/// </summary>
public partial class FontAwesomeIcon : ComponentBase
{
    /// <summary>
    /// The name of the icon to render (case-insensitive, kebab-case).
    /// Example: "camera", "user", "github"
    /// </summary>
    [Parameter, EditorRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The icon variant to render.
    /// Default is Solid.
    /// </summary>
    [Parameter]
    public FontAwesomeIconVariant Variant { get; set; } = FontAwesomeIconVariant.Solid;

    /// <summary>
    /// The size of the icon in pixels (applies to width).
    /// Height is scaled proportionally to preserve aspect ratio (Brands icons in particular are not square).
    /// Default is 16px.
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

    private const int DefaultSize = 16;

    /// <summary>
    /// The icon entry (body + intrinsic width/height) for the current Name and Variant.
    /// </summary>
    private FontAwesomeIconEntry? IconEntry => FontAwesomeIconData.GetIcon(Name, Variant);

    /// <summary>
    /// Icon SVG body with hardcoded fill/stroke attributes stripped, so the outer
    /// &lt;svg&gt; element's fill (driven by the Color parameter) is honored.
    /// </summary>
    private string SvgBody => IconEntry is null
        ? string.Empty
        : System.Text.RegularExpressions.Regex.Replace(
            IconEntry.Body,
            @"\s+(stroke|fill)=""[^""]*""",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    /// <summary>
    /// The computed width in pixels.
    /// </summary>
    private int ComputedSize => Size ?? DefaultSize;

    /// <summary>
    /// The computed height in pixels, scaled to preserve the icon's intrinsic aspect ratio.
    /// </summary>
    private int ComputedHeight
    {
        get
        {
            if (IconEntry is null || IconEntry.Width == 0)
            {
                return ComputedSize;
            }

            return (int)Math.Round(ComputedSize * ((double)IconEntry.Height / IconEntry.Width));
        }
    }

    /// <summary>
    /// The viewBox derived from the icon's intrinsic width and height.
    /// </summary>
    private string ViewBox => IconEntry is null
        ? "0 0 512 512"
        : $"0 0 {IconEntry.Width} {IconEntry.Height}";

    /// <summary>
    /// The combined CSS class string.
    /// </summary>
    private string CssClass => string.IsNullOrEmpty(Class) ? string.Empty : Class;
}
