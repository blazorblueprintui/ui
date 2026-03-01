namespace BlazorBlueprint.Components;

/// <summary>
/// Defines the layout orientation for the wizard step indicator.
/// </summary>
public enum WizardLayout
{
    /// <summary>
    /// Step indicator displayed horizontally across the top with content below.
    /// Best for wide forms and desktop layouts.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Step indicator displayed vertically on the left with content on the right.
    /// Best for narrow forms, mobile layouts, or when step titles are long.
    /// </summary>
    Vertical
}
