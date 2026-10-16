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
    /// The Motion preset played when the dialog opens. When null, the dialog uses its built-in
    /// fade and zoom. <see cref="EnterKeyframes"/> take precedence.
    /// Default: null.
    /// </summary>
    public MotionPreset? EnterPreset { get; set; }

    /// <summary>
    /// The Motion preset played, in reverse, when the dialog closes; the dialog stays on the page
    /// until it ends. When null, the dialog leaves as soon as it closes.
    /// <see cref="ExitKeyframes"/> take precedence.
    /// Default: null.
    /// </summary>
    public MotionPreset? ExitPreset { get; set; }

    /// <summary>
    /// Custom Web Animations keyframes played when the dialog opens. Each dictionary maps CSS
    /// property names to values. Overrides <see cref="EnterPreset"/>.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object>>? EnterKeyframes { get; set; }

    /// <summary>
    /// Custom Web Animations keyframes played, as written, when the dialog closes; the dialog stays
    /// on the page until they end. Overrides <see cref="ExitPreset"/>.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object>>? ExitKeyframes { get; set; }

    /// <summary>
    /// Duration in milliseconds of the enter and exit animations, and of the overlay's fade while
    /// either is set. Reduced-motion preferences skip the animations.
    /// Default: 200.
    /// </summary>
    public int AnimationDuration { get; set; } = 200;

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
