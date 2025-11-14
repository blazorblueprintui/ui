namespace BlazorUI.Components.Skeleton;

/// <summary>
/// Defines the shape variant for a Skeleton component.
/// </summary>
/// <remarks>
/// Skeleton shapes determine the border-radius applied to the placeholder element.
/// Choose the shape based on the content being loaded:
/// <list type="bullet">
/// <item><see cref="Rectangular"/>: For text, images, cards, and general content blocks</item>
/// <item><see cref="Circular"/>: For avatars, profile pictures, and circular icons</item>
/// </list>
/// </remarks>
public enum SkeletonShape
{
    /// <summary>
    /// Rectangular skeleton with rounded corners (rounded-md).
    /// </summary>
    /// <remarks>
    /// Default shape suitable for most loading placeholders including:
    /// <list type="bullet">
    /// <item>Text lines and paragraphs</item>
    /// <item>Images and thumbnails</item>
    /// <item>Cards and panels</item>
    /// <item>Buttons and form fields</item>
    /// </list>
    /// Uses Tailwind's <c>rounded-md</c> class for subtle rounded corners.
    /// </remarks>
    Rectangular,

    /// <summary>
    /// Circular skeleton with fully rounded borders (rounded-full).
    /// </summary>
    /// <remarks>
    /// Ideal for avatar and icon placeholders including:
    /// <list type="bullet">
    /// <item>User profile pictures</item>
    /// <item>Circular badges or indicators</item>
    /// <item>Icon placeholders</item>
    /// </list>
    /// Uses Tailwind's <c>rounded-full</c> class for perfectly circular shape.
    /// Ensure equal width and height (e.g., <c>h-12 w-12</c>) for proper circles.
    /// </remarks>
    Circular
}
