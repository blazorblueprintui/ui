namespace BlazorBlueprint.Components;

/// <summary>
/// The primary accent color used for buttons, links, focus rings, and interactive elements.
/// Each value maps to a distinct set of OKLCH CSS custom properties for <c>--primary</c> and <c>--primary-foreground</c>.
/// </summary>
public enum PrimaryColor
{
    /// <summary>Inherits the primary color from the base color palette (no override).</summary>
    Default,

    /// <summary>Blue primary — oklch hue ~260.</summary>
    Blue,

    /// <summary>Violet primary — oklch hue ~280.</summary>
    Violet,

    /// <summary>Purple primary — oklch hue ~290.</summary>
    Purple,

    /// <summary>Rose primary — oklch hue ~350.</summary>
    Rose,

    /// <summary>Red primary — oklch hue ~25.</summary>
    Red,

    /// <summary>Orange primary — oklch hue ~45.</summary>
    Orange,

    /// <summary>Amber primary — oklch hue ~75.</summary>
    Amber,

    /// <summary>Yellow primary — oklch hue ~90.</summary>
    Yellow,

    /// <summary>Lime primary — oklch hue ~125.</summary>
    Lime,

    /// <summary>Green primary — oklch hue ~145.</summary>
    Green,

    /// <summary>Emerald primary — oklch hue ~160.</summary>
    Emerald,

    /// <summary>Teal primary — oklch hue ~180.</summary>
    Teal,

    /// <summary>Cyan primary — oklch hue ~200.</summary>
    Cyan,

    /// <summary>Sky primary — oklch hue ~220.</summary>
    Sky,

    /// <summary>Indigo primary — oklch hue ~270.</summary>
    Indigo,

    /// <summary>Fuchsia primary — oklch hue ~320.</summary>
    Fuchsia,

    /// <summary>Pink primary — oklch hue ~340.</summary>
    Pink
}
