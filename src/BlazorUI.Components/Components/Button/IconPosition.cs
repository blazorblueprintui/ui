namespace BlazorUI.Components.Button;

/// <summary>
/// Defines the position of an icon relative to the button text.
/// </summary>
/// <remarks>
/// Icon positioning automatically adapts to RTL (Right-to-Left) layouts
/// using Tailwind's directional utilities (ltr:/rtl:).
/// </remarks>
public enum IconPosition
{
    /// <summary>
    /// Icon appears before the text (left in LTR, right in RTL).
    /// Uses margin-end for RTL-aware spacing.
    /// </summary>
    Start,

    /// <summary>
    /// Icon appears after the text (right in LTR, left in RTL).
    /// Uses margin-start for RTL-aware spacing.
    /// </summary>
    End
}
