using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components.Skeleton;

/// <summary>
/// A skeleton component that displays a placeholder preview of content before the data is loaded.
/// </summary>
/// <remarks>
/// <para>
/// The Skeleton component follows the shadcn/ui design system and provides visual feedback
/// during data loading states. It uses CSS animations to create a pulse effect that indicates
/// content is being loaded.
/// </para>
/// <para>
/// Common use cases include:
/// <list type="bullet">
/// <item>Loading states for lists, cards, and tables</item>
/// <item>Profile avatars and user information placeholders</item>
/// <item>Form field loading indicators</item>
/// <item>Image and media content placeholders</item>
/// </list>
/// </para>
/// <para>
/// The component supports two shape variants (rectangular and circular) and uses Tailwind CSS
/// for styling with the animate-pulse utility for the loading animation.
/// </para>
/// </remarks>
/// <example>
/// Basic rectangular skeleton:
/// <code>
/// &lt;Skeleton Class="h-12 w-12" /&gt;
/// </code>
///
/// Circular skeleton for avatar:
/// <code>
/// &lt;Skeleton Shape="SkeletonShape.Circular" Class="h-12 w-12" /&gt;
/// </code>
///
/// Multiple skeletons for a card:
/// <code>
/// &lt;div class="space-y-2"&gt;
///     &lt;Skeleton Class="h-4 w-[250px]" /&gt;
///     &lt;Skeleton Class="h-4 w-[200px]" /&gt;
/// &lt;/div&gt;
/// </code>
/// </example>
public partial class Skeleton : ComponentBase
{
    /// <summary>
    /// Gets or sets the shape variant of the skeleton.
    /// </summary>
    /// <value>
    /// A <see cref="SkeletonShape"/> value. Default is <see cref="SkeletonShape.Rectangular"/>.
    /// </value>
    /// <remarks>
    /// <list type="bullet">
    /// <item><see cref="SkeletonShape.Rectangular"/>: Default rectangular shape with rounded corners</item>
    /// <item><see cref="SkeletonShape.Circular"/>: Circular shape, ideal for avatar placeholders</item>
    /// </list>
    /// </remarks>
    [Parameter]
    public SkeletonShape Shape { get; set; } = SkeletonShape.Rectangular;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the skeleton element.
    /// </summary>
    /// <value>
    /// A string containing one or more CSS class names, or <c>null</c>.
    /// </value>
    /// <remarks>
    /// Use this parameter to customize the skeleton's dimensions and spacing.
    /// Common Tailwind utilities include:
    /// <list type="bullet">
    /// <item>Height: <c>h-4</c>, <c>h-12</c>, <c>h-[200px]</c></item>
    /// <item>Width: <c>w-full</c>, <c>w-[250px]</c>, <c>w-1/2</c></item>
    /// <item>Margin: <c>mb-2</c>, <c>mt-4</c></item>
    /// </list>
    /// </remarks>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets the computed CSS class string for the skeleton element.
    /// </summary>
    /// <value>
    /// A string containing all CSS classes to be applied to the skeleton div element.
    /// </value>
    /// <remarks>
    /// The class string includes:
    /// <list type="bullet">
    /// <item>Base animation: <c>animate-pulse</c> for loading effect</item>
    /// <item>Background: <c>bg-muted</c> using theme's muted color</item>
    /// <item>Shape-specific classes: <c>rounded-md</c> or <c>rounded-full</c></item>
    /// <item>Custom classes: Any classes provided via the <see cref="Class"/> parameter</item>
    /// </list>
    /// </remarks>
    private string CssClass => ClassNames.cn(
        "animate-pulse bg-muted",
        Shape switch
        {
            SkeletonShape.Circular => "rounded-full",
            SkeletonShape.Rectangular => "rounded-md",
            _ => "rounded-md"
        },
        Class
    );
}
