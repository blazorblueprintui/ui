namespace BlazorBlueprint.Components;

/// <summary>
/// Defines visual variants for a bubble surface.
/// </summary>
public enum BubbleVariant
{
    /// <summary>
    /// Strong primary bubble.
    /// </summary>
    Default,

    /// <summary>
    /// Secondary neutral bubble.
    /// </summary>
    Secondary,

    /// <summary>
    /// Lower-emphasis muted bubble.
    /// </summary>
    Muted,

    /// <summary>
    /// Subtle primary-tinted bubble.
    /// </summary>
    Tinted,

    /// <summary>
    /// Bordered bubble treatment.
    /// </summary>
    Outline,

    /// <summary>
    /// Unframed bubble content.
    /// </summary>
    Ghost,

    /// <summary>
    /// Destructive bubble for failed actions.
    /// </summary>
    Destructive
}
