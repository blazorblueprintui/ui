namespace BlazorBlueprint.Components;

/// <summary>
/// Controls when <c>ValueChanged</c> fires relative to user input.
/// </summary>
public enum UpdateTiming
{
    /// <summary>
    /// Fires <c>ValueChanged</c> on every keystroke, batched via requestAnimationFrame in JavaScript.
    /// </summary>
    Immediate,

    /// <summary>
    /// Fires <c>ValueChanged</c> only when the element loses focus (default).
    /// For single-line inputs, also fires when the user presses Enter.
    /// Zero C# interop calls during typing.
    /// </summary>
    OnChange,

    /// <summary>
    /// Fires <c>ValueChanged</c> after typing pauses for <c>DebounceInterval</c> milliseconds.
    /// Debounce timer runs in JavaScript.
    /// </summary>
    Debounced
}
