namespace BlazorBlueprint.Components;

/// <summary>
/// Configuration options for the BlazorBlueprint theme system.
/// </summary>
public class ThemeOptions
{
    /// <summary>
    /// The default base (gray scale) color palette. Defaults to <see cref="BaseColor.Zinc"/>.
    /// </summary>
    public BaseColor DefaultBaseColor { get; set; } = BaseColor.Zinc;

    /// <summary>
    /// The default primary accent color. Defaults to <see cref="PrimaryColor.Default"/> (inherits from base).
    /// </summary>
    public PrimaryColor DefaultPrimaryColor { get; set; } = PrimaryColor.Default;

    /// <summary>
    /// Whether dark mode is enabled by default. Defaults to <c>false</c>.
    /// When <see cref="DetectSystemPreference"/> is <c>true</c>, the system preference takes precedence.
    /// </summary>
    public bool DefaultDarkMode { get; set; }

    /// <summary>
    /// When <c>true</c>, detects the user's OS color scheme preference on first load.
    /// This overrides <see cref="DefaultDarkMode"/> when no saved preference exists. Defaults to <c>true</c>.
    /// </summary>
    public bool DetectSystemPreference { get; set; } = true;

    /// <summary>
    /// The default border radius in rem. Defaults to <c>0.5</c> (matching shadcn/ui default).
    /// Common values: 0, 0.3, 0.5, 0.75, 1.0.
    /// </summary>
    public double DefaultRadius { get; set; } = 0.5;

    /// <summary>
    /// When <c>true</c>, persists theme preferences to <c>localStorage</c>. Defaults to <c>true</c>.
    /// </summary>
    public bool PersistToLocalStorage { get; set; } = true;
}
