namespace BlazorUI.Components.Button;

/// <summary>
/// Defines the size variant for a Button component.
/// </summary>
/// <remarks>
/// Button sizes control padding, font size, and overall dimensions.
/// All sizes maintain proper touch target sizes (minimum 44x44px) for accessibility.
/// Follows shadcn/ui size scale conventions.
/// </remarks>
public enum ButtonSize
{
    /// <summary>
    /// Small button size (sm).
    /// Height: ~36px, Padding: 0.5rem horizontal.
    /// Suitable for compact UIs, toolbars, or inline actions.
    /// </summary>
    Small,

    /// <summary>
    /// Default/medium button size (md).
    /// Height: ~40px, Padding: 1rem horizontal.
    /// Recommended default size for most use cases.
    /// </summary>
    Default,

    /// <summary>
    /// Large button size (lg).
    /// Height: ~44px, Padding: 1.5rem horizontal.
    /// Best for primary CTAs or prominent actions.
    /// </summary>
    Large,

    /// <summary>
    /// Icon-only button size (icon).
    /// Square dimensions: 40x40px (h-10 w-10).
    /// Designed for buttons containing only an icon without text.
    /// Maintains accessibility with proper aria-label required.
    /// </summary>
    Icon,

    /// <summary>
    /// Small icon-only button size (icon-sm).
    /// Square dimensions: 36x36px (h-9 w-9).
    /// Compact icon button for toolbars or tight layouts.
    /// Requires aria-label for accessibility.
    /// </summary>
    IconSmall,

    /// <summary>
    /// Large icon-only button size (icon-lg).
    /// Square dimensions: 44x44px (h-11 w-11).
    /// Prominent icon button for primary icon actions.
    /// Requires aria-label for accessibility.
    /// </summary>
    IconLarge
}
