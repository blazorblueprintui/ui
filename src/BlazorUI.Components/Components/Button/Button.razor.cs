using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorUI.Components.Button;

/// <summary>
/// A button component that follows the shadcn/ui design system.
/// </summary>
/// <remarks>
/// <para>
/// The Button component provides a customizable, accessible button that supports
/// multiple visual variants, sizes, and states. It follows WCAG 2.1 AA standards
/// for accessibility and integrates with Blazor's event system.
/// </para>
/// <para>
/// Features:
/// - 6 visual variants (Default, Destructive, Outline, Secondary, Ghost, Link)
/// - 4 size options (Small, Default, Large, Icon)
/// - Disabled state support
/// - Full keyboard navigation (Tab, Enter, Space)
/// - ARIA attributes for screen readers
/// - RTL (Right-to-Left) support
/// - Dark mode compatible via CSS variables
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Button Variant="ButtonVariant.Default" Size="ButtonSize.Default" OnClick="HandleClick"&gt;
///     Click me
/// &lt;/Button&gt;
/// </code>
/// </example>
public partial class Button : ComponentBase
{
    /// <summary>
    /// Gets or sets the visual style variant of the button.
    /// </summary>
    /// <remarks>
    /// Controls the color scheme and visual appearance using CSS custom properties.
    /// Default value is <see cref="ButtonVariant.Default"/>.
    /// </remarks>
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Default;

    /// <summary>
    /// Gets or sets the size of the button.
    /// </summary>
    /// <remarks>
    /// Controls padding, font size, and overall dimensions.
    /// Default value is <see cref="ButtonSize.Default"/>.
    /// All sizes maintain minimum touch target sizes (44x44px) for accessibility.
    /// </remarks>
    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Default;

    /// <summary>
    /// Gets or sets the HTML button type attribute.
    /// </summary>
    /// <remarks>
    /// Controls form submission behavior when button is inside a form.
    /// Default value is <see cref="ButtonType.Button"/> to prevent accidental form submissions.
    /// </remarks>
    [Parameter]
    public ButtonType Type { get; set; } = ButtonType.Button;

    /// <summary>
    /// Gets or sets whether the button is disabled.
    /// </summary>
    /// <remarks>
    /// When disabled:
    /// - Button cannot be clicked or focused
    /// - Opacity is reduced (via disabled:opacity-50 Tailwind class)
    /// - Pointer events are disabled (via disabled:pointer-events-none)
    /// - aria-disabled attribute is set to true
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the button.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the button is clicked.
    /// </summary>
    /// <remarks>
    /// The event handler receives a <see cref="MouseEventArgs"/> parameter with click details.
    /// If the button is disabled, this callback will not be invoked.
    /// </remarks>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the button.
    /// </summary>
    /// <remarks>
    /// Can contain text, icons, or any other Blazor markup.
    /// For icon-only buttons, use <see cref="ButtonSize.Icon"/> and provide an aria-label.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the button.
    /// </summary>
    /// <remarks>
    /// Required for icon-only buttons to provide accessible text for screen readers.
    /// Optional for buttons with text content.
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the icon to display in the button.
    /// </summary>
    /// <remarks>
    /// Can be any RenderFragment (SVG, icon font, image).
    /// Position is controlled by <see cref="IconPosition"/>.
    /// Automatically adds RTL-aware spacing between icon and text.
    /// </remarks>
    [Parameter]
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// Gets or sets the position of the icon relative to the button text.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="IconPosition.Start"/> (before text in LTR).
    /// Automatically adapts to RTL layouts using Tailwind directional utilities.
    /// </remarks>
    [Parameter]
    public IconPosition IconPosition { get; set; } = IconPosition.Start;

    /// <summary>
    /// Gets the computed CSS classes for the button element.
    /// </summary>
    /// <remarks>
    /// Combines:
    /// - Base button styles (positioning, transitions, focus states)
    /// - Variant-specific classes (colors, backgrounds, borders)
    /// - Size-specific classes (padding, font size, height)
    /// - Custom classes from the Class parameter
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base button styles (from shadcn/ui)
        "inline-flex items-center justify-center gap-2 rounded-md text-sm font-medium",
        "transition-colors focus-visible:outline-none focus-visible:ring-2",
        "focus-visible:ring-ring focus-visible:ring-offset-2",
        "disabled:opacity-50 disabled:pointer-events-none",
        // Variant-specific styles
        Variant switch
        {
            ButtonVariant.Default => "bg-primary text-primary-foreground hover:bg-primary/90",
            ButtonVariant.Destructive => "bg-destructive text-destructive-foreground hover:bg-destructive/90",
            ButtonVariant.Outline => "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
            ButtonVariant.Secondary => "bg-secondary text-secondary-foreground hover:bg-secondary/80",
            ButtonVariant.Ghost => "hover:bg-accent hover:text-accent-foreground",
            ButtonVariant.Link => "text-primary underline-offset-4 hover:underline",
            _ => "bg-primary text-primary-foreground hover:bg-primary/90"
        },
        // Size-specific styles
        Size switch
        {
            ButtonSize.Small => "h-9 rounded-md px-3 text-xs",
            ButtonSize.Default => "h-10 px-4 py-2",
            ButtonSize.Large => "h-11 rounded-md px-8",
            ButtonSize.Icon => "h-10 w-10",
            ButtonSize.IconSmall => "h-9 w-9",
            ButtonSize.IconLarge => "h-11 w-11",
            _ => "h-10 px-4 py-2"
        },
        // Custom classes (if provided)
        Class
    );

    /// <summary>
    /// Gets the HTML button type attribute value.
    /// </summary>
    private string HtmlType => Type switch
    {
        ButtonType.Submit => "submit",
        ButtonType.Reset => "reset",
        ButtonType.Button => "button",
        _ => "button"
    };

    /// <summary>
    /// Handles the button click event.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <remarks>
    /// This method is invoked when the button is clicked and not disabled.
    /// It triggers the OnClick callback if one is registered.
    /// </remarks>
    private async Task HandleClick(MouseEventArgs args)
    {
        if (!Disabled && OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(args);
        }
    }
}
