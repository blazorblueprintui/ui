namespace BlazorBlueprint.Components;

/// <summary>
/// Controls how <c>BbThemeSwitcher</c> presents its color choices.
/// </summary>
public enum ThemeSwitcherColorLayout
{
    /// <summary>
    /// Base and primary colors are shown in separate sections, each with its own
    /// independent selection indicator. Selecting one never resets the other.
    /// </summary>
    Split,

    /// <summary>
    /// Base and primary colors are shown in a single combined grid with one selection
    /// indicator. Selecting a base color resets the primary color to
    /// <see cref="PrimaryColor.Default"/> (legacy behavior).
    /// </summary>
    Combined
}
