namespace BlazorUI.Components.Spinner;

/// <summary>
/// Defines the size options for a Spinner component.
/// </summary>
public enum SpinnerSize
{
    /// <summary>
    /// Small spinner (16x16 pixels).
    /// Suitable for inline indicators within buttons or text.
    /// </summary>
    Small,

    /// <summary>
    /// Default medium spinner (24x24 pixels).
    /// Standard size for most loading states.
    /// </summary>
    Default,

    /// <summary>
    /// Large spinner (40x40 pixels).
    /// For prominent loading states or full-page loaders.
    /// </summary>
    Large
}
