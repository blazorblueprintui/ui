using BlazorUI.Components.Button;
using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.InputGroup;

/// <summary>
/// A button component optimized for use within InputGroup.
/// </summary>
/// <remarks>
/// <para>
/// InputGroupButton is a specialized button designed for integration with InputGroup.
/// It removes the shadow and adjusts sizing to work seamlessly alongside input controls.
/// </para>
/// <para>
/// Features:
/// - Smaller default size (Small) for better proportions
/// - No shadow for cleaner integration
/// - Full Button component parameter compatibility
/// - Automatic sizing adjustments within group context
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;InputGroup&gt;
///     &lt;InputGroupInput Placeholder="Search..." /&gt;
///     &lt;InputGroupAddon Align="InputGroupAlign.InlineEnd"&gt;
///         &lt;InputGroupButton&gt;Search&lt;/InputGroupButton&gt;
///     &lt;/InputGroupAddon&gt;
/// &lt;/InputGroup&gt;
/// </code>
/// </example>
public partial class InputGroupButton : ComponentBase
{
    /// <summary>
    /// Gets or sets the button type (submit, button, reset).
    /// </summary>
    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    /// <summary>
    /// Gets or sets the button visual variant.
    /// </summary>
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Default;

    /// <summary>
    /// Gets or sets the button size.
    /// </summary>
    /// <remarks>
    /// Default is Small for better proportions within input groups.
    /// </remarks>
    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Small;

    /// <summary>
    /// Gets or sets whether the button is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for accessibility.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the click event handler.
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <summary>
    /// Gets or sets the icon to display in the button.
    /// </summary>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Gets or sets the position of the icon relative to button text.
    /// </summary>
    [Parameter]
    public IconPosition IconPosition { get; set; } = IconPosition.Start;

    /// <summary>
    /// Gets or sets the child content (button text).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the button.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides minimal default styling for integration with InputGroup.
    /// The component removes border and shadow (provided by InputGroup) and ensures proper sizing.
    /// </para>
    /// <para>
    /// For custom styling (padding, border radius, etc.), use the Class parameter.
    /// TailwindMerge will intelligently merge your custom classes with the defaults.
    /// </para>
    /// <para>
    /// Common customization patterns:
    /// </para>
    /// <code>
    /// &lt;!-- Compact ghost button with rounded hover --&gt;
    /// &lt;InputGroupButton Variant="ButtonVariant.Ghost" Class="!px-1 !py-1 !rounded-md"&gt;
    ///     Search In...
    /// &lt;/InputGroupButton&gt;
    ///
    /// &lt;!-- Standard action button --&gt;
    /// &lt;InputGroupButton Class="!px-2.5 !py-0.5 !rounded-sm"&gt;
    ///     Search
    /// &lt;/InputGroupButton&gt;
    ///
    /// &lt;!-- Icon-only button --&gt;
    /// &lt;InputGroupButton Variant="ButtonVariant.Ghost" Class="!px-1.5 !py-1.5 !rounded-md"&gt;
    ///     &lt;MoreIcon /&gt;
    /// &lt;/InputGroupButton&gt;
    /// </code>
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Essential styling for InputGroup integration
        "!border-0",        // No border (InputGroup provides border)
        "!shadow-none",     // No shadow (InputGroup provides shadow)
        "!h-auto",          // Auto height to fit container
        "!text-sm",         // Small text appropriate for input context
        "transition-colors", // Smooth color transitions

        // User customization (padding, border-radius, etc.)
        Class
    );
}
