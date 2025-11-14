namespace BlazorUI.Components.Avatar;

/// <summary>
/// Defines the size variants for Avatar components.
/// </summary>
/// <remarks>
/// Avatar sizes control the width, height, and font-size of the avatar container
/// and its content (image or fallback text).
/// </remarks>
public enum AvatarSize
{
    /// <summary>
    /// Small avatar (32px).
    /// </summary>
    /// <remarks>
    /// Suitable for compact layouts, user lists, or inline mentions.
    /// </remarks>
    Small,

    /// <summary>
    /// Default avatar size (40px).
    /// </summary>
    /// <remarks>
    /// Standard size for most use cases including navigation bars and comment sections.
    /// </remarks>
    Default,

    /// <summary>
    /// Large avatar (48px).
    /// </summary>
    /// <remarks>
    /// Used for prominent user displays or profile headers.
    /// </remarks>
    Large,

    /// <summary>
    /// Extra large avatar (64px).
    /// </summary>
    /// <remarks>
    /// Reserved for profile pages or settings where user identity is focal.
    /// </remarks>
    ExtraLarge
}
