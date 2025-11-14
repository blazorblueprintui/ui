using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Card;

/// <summary>
/// A card component that displays structured content in a visually distinct container.
/// </summary>
/// <remarks>
/// <para>
/// The Card component serves as the root container for card content, providing a bordered,
/// rounded container with shadow effects. It follows the shadcn/ui design system and
/// integrates seamlessly with other card subcomponents.
/// </para>
/// <para>
/// Features:
/// - Rounded corners with subtle shadow
/// - Responsive border and background styling
/// - Customizable through additional CSS classes
/// - Dark mode compatible via CSS variables
/// - Semantic HTML structure for accessibility
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Card&gt;
///     &lt;CardHeader&gt;
///         &lt;CardTitle&gt;Card Title&lt;/CardTitle&gt;
///         &lt;CardDescription&gt;Card description&lt;/CardDescription&gt;
///     &lt;/CardHeader&gt;
///     &lt;CardContent&gt;
///         Card content goes here
///     &lt;/CardContent&gt;
///     &lt;CardFooter&gt;
///         Footer content
///     &lt;/CardFooter&gt;
/// &lt;/Card&gt;
/// </code>
/// </example>
public partial class Card : ComponentBase
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the card.
    /// </summary>
    /// <remarks>
    /// Typically contains CardHeader, CardContent, and CardFooter components.
    /// Can contain any Blazor markup.
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the card.
    /// </summary>
    /// <remarks>
    /// Custom classes are appended after the component's base classes,
    /// allowing for style overrides and extensions.
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the card element.
    /// </summary>
    /// <remarks>
    /// Combines base card styles with custom classes.
    /// Uses the cn() utility for intelligent class merging and Tailwind conflict resolution.
    /// </remarks>
    private string CssClass => ClassNames.cn(
        // Base card styles (from shadcn/ui)
        "rounded-lg border bg-card text-card-foreground shadow-sm",
        // Custom classes (if provided)
        Class
    );
}
