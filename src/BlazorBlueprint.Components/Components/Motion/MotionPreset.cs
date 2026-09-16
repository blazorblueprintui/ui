namespace BlazorBlueprint.Components;

/// <summary>Reusable animation shapes; custom keyframes take precedence.</summary>
public enum MotionPreset
{
    Fade, SlideUp, SlideDown, SlideLeft, SlideRight, Scale, Bounce, Pulse, ShakeX, ShakeY, Spring
}

/// <summary>Controls when a motion animation starts.</summary>
public enum MotionTrigger
{
    /// <summary>Animate visibility changes, and optionally the first interactive render.</summary>
    Visibility,
    /// <summary>Animate when the element enters the viewport.</summary>
    InView,
    /// <summary>Animate on pointer hover or keyboard focus.</summary>
    Hover,
    /// <summary>Animate on pointer press or Space/Enter.</summary>
    Press,
    /// <summary>Animate only when PlayAsync is called.</summary>
    Manual
}
