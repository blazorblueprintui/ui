namespace BlazorBlueprint.Components;

/// <summary>
/// Options controlling behavior and appearance of a dialog opened via
/// <see cref="DialogService.OpenAsync{TComponent}(Dictionary{string, object?}, BlazorBlueprint.Components.DialogOpenOptions?)"/>.
/// </summary>
public sealed class DialogOpenOptions
{
    /// <summary>
    /// Optional dialog title. If null, the component may render its own header.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional dialog description displayed below the title.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the close (X) button should be displayed.
    /// Default: true.
    /// </summary>
    public bool ShowClose { get; set; } = true;

    /// <summary>
    /// Controls the dialog width preset.
    /// Default: <see cref="DialogSize.Default"/>.
    /// </summary>
    public DialogSize Size { get; set; } = DialogSize.Default;

    /// <summary>
    /// Where the dialog sits in the viewport.
    /// Default: <see cref="DialogPosition.Center"/>.
    /// </summary>
    public DialogPosition Position { get; set; } = DialogPosition.Center;

    /// <summary>
    /// Prevents closing via Escape key or backdrop click.
    /// Default: false.
    /// </summary>
    public bool PreventClose { get; set; }
}
