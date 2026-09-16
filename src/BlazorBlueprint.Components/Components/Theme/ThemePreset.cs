namespace BlazorBlueprint.Components;

/// <summary>Spacing scale for prefixed Bb utilities.</summary>
public enum ThemeDensity
{
    /// <summary>Standard component spacing.</summary>
    Standard,
    /// <summary>Compact application spacing.</summary>
    Compact,
    /// <summary>Dense data-oriented spacing.</summary>
    Dense,
    /// <summary>Roomier component spacing.</summary>
    Spacious
}

/// <summary>Font stacks. Named fonts must be supplied by the application.</summary>
public enum ThemeFont
{
    /// <summary>The platform's UI font.</summary>
    System,
    /// <summary>Inter with system fallbacks.</summary>
    Inter,
    /// <summary>Geist with system fallbacks.</summary>
    Geist,
    /// <summary>Roboto with system fallbacks.</summary>
    Roboto,
    /// <summary>Nunito Sans with system fallbacks.</summary>
    NunitoSans,
    /// <summary>The platform's serif font.</summary>
    Serif,
    /// <summary>The platform's monospace font.</summary>
    Mono
}

/// <summary>Card and menu surface treatment.</summary>
public enum ThemeSurface
{
    /// <summary>Standard shadows and opaque backgrounds.</summary>
    Standard,
    /// <summary>Surfaces without shadows.</summary>
    Flat,
    /// <summary>Stronger shadows.</summary>
    Elevated,
    /// <summary>Translucent menu surfaces with background blur.</summary>
    Glass
}

/// <summary>The menu surface palette.</summary>
public enum ThemeMenuColor
{
    /// <summary>The standard popover palette.</summary>
    Default,
    /// <summary>The muted palette.</summary>
    Muted,
    /// <summary>The primary palette.</summary>
    Primary,
    /// <summary>Inverted foreground and background.</summary>
    Inverse
}

/// <summary>The menu focus and hover treatment.</summary>
public enum ThemeMenuAccent
{
    /// <summary>A subtle foreground tint.</summary>
    Subtle,
    /// <summary>The primary palette.</summary>
    Primary
}

/// <summary>Appearance settings that can be applied globally or to a BbThemeScope.</summary>
public sealed record ThemeDesign
{
    /// <summary>Component spacing density.</summary>
    public ThemeDensity Density { get; init; }
    /// <summary>The font stack; fonts are never downloaded automatically.</summary>
    public ThemeFont Font { get; init; }
    /// <summary>Card and menu surface treatment.</summary>
    public ThemeSurface Surface { get; init; }
    /// <summary>Menu background and text colors.</summary>
    public ThemeMenuColor MenuColor { get; init; }
    /// <summary>Menu focus and hover colors.</summary>
    public ThemeMenuAccent MenuAccent { get; init; }

    internal void Validate()
    {
        if (!Enum.IsDefined(Density) || !Enum.IsDefined(Font) || !Enum.IsDefined(Surface)
            || !Enum.IsDefined(MenuColor) || !Enum.IsDefined(MenuAccent))
        {
            throw new ArgumentOutOfRangeException(nameof(ThemeDesign), "Theme design values must be defined enum members.");
        }
    }

    internal object ToJs() => new
    {
        density = Density.ToString().ToLowerInvariant(),
        font = Font.ToString().ToLowerInvariant(),
        surface = Surface.ToString().ToLowerInvariant(),
        menuColor = MenuColor.ToString().ToLowerInvariant(),
        menuAccent = MenuAccent.ToString().ToLowerInvariant()
    };
}

/// <summary>A complete, immutable theme configuration.</summary>
public sealed record ThemePreset
{
    /// <summary>Whether dark mode is active.</summary>
    public bool DarkMode { get; init; }
    /// <summary>The neutral palette.</summary>
    public BaseColor BaseColor { get; init; } = BaseColor.Zinc;
    /// <summary>The primary palette.</summary>
    public PrimaryColor PrimaryColor { get; init; }
    /// <summary>Border radius in rem.</summary>
    public double Radius { get; init; } = 0.5;
    /// <summary>The component appearance configuration.</summary>
    public ThemeDesign Design { get; init; } = new();

    internal void Validate()
    {
        if (!Enum.IsDefined(BaseColor) || !Enum.IsDefined(PrimaryColor) || !double.IsFinite(Radius) || Radius < 0 || Radius > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(ThemePreset), "Colors must be defined enum members and radius must be between 0 and 4 rem.");
        }
        ArgumentNullException.ThrowIfNull(Design);
        Design.Validate();
    }
}

/// <summary>Seven starting points, customizable with record with-expressions.</summary>
public static class ThemePresets
{
    /// <summary>The existing Bb appearance.</summary>
    public static ThemePreset Standard { get; } = new();
    /// <summary>A balanced professional appearance.</summary>
    public static ThemePreset Balanced { get; } = new() { Radius = 0.375, Design = new() { Font = ThemeFont.Inter } };
    /// <summary>Compact dashboard spacing.</summary>
    public static ThemePreset Compact { get; } = new() { Radius = 0.375, Design = new() { Density = ThemeDensity.Compact, Surface = ThemeSurface.Flat } };
    /// <summary>Roomy rounded controls.</summary>
    public static ThemePreset Spacious { get; } = new() { Radius = 0.875, Design = new() { Density = ThemeDensity.Spacious, Font = ThemeFont.NunitoSans } };
    /// <summary>Square controls for developer tools.</summary>
    public static ThemePreset Sharp { get; } = new() { Radius = 0, Design = new() { Font = ThemeFont.Mono, Surface = ThemeSurface.Flat } };
    /// <summary>Dense data-oriented spacing.</summary>
    public static ThemePreset Dense { get; } = new() { Radius = 0.25, Design = new() { Density = ThemeDensity.Dense, Surface = ThemeSurface.Flat } };
    /// <summary>Translucent menu surfaces.</summary>
    public static ThemePreset Glass { get; } = new() { Radius = 0.75, Design = new() { Surface = ThemeSurface.Glass, Font = ThemeFont.Geist } };
}
