namespace BlazorBlueprint.Components;

/// <summary>
/// Preset width options for dialogs opened via <see cref="DialogService.OpenAsync{TComponent}(Dictionary{string, object?}, BlazorBlueprint.Components.DialogOpenOptions?)"/>.
/// </summary>
public enum DialogSize
{
    /// <summary>
    /// Small width dialog.
    /// </summary>
    Small,

    /// <summary>
    /// Default medium width dialog.
    /// </summary>
    Default,

    /// <summary>
    /// Large width dialog.
    /// </summary>
    Large,

    /// <summary>
    /// Extra large width dialog.
    /// </summary>
    ExtraLarge,

    /// <summary>
    /// Full width dialog.
    /// </summary>
    Full
}
