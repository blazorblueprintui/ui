using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components.Components.Dialog.Internals;

/// <summary>
/// A dialog's enter and exit animations, shared by <see cref="BbDialog"/> (cascaded to
/// <see cref="BbDialogContent"/>) and <see cref="BbDialogProvider"/>. Each plays through Motion's
/// presets or custom keyframes, and Motion's reduced-motion rule skips both.
/// </summary>
internal sealed record DialogAnimation(
    MotionPreset? EnterPreset,
    MotionPreset? ExitPreset,
    IReadOnlyList<IReadOnlyDictionary<string, object>>? EnterKeyframes,
    IReadOnlyList<IReadOnlyDictionary<string, object>>? ExitKeyframes,
    int Duration)
{
    /// <summary>The slide distance in pixels, Motion's own default.</summary>
    private const double Distance = 16;

    public bool HasEnter => EnterPreset is not null || EnterKeyframes is { Count: > 0 };

    public bool HasExit => ExitPreset is not null || ExitKeyframes is { Count: > 0 };

    /// <summary>
    /// Inline style for the overlay, so its CSS fade runs as long as the dialog's animation and an
    /// exit fade holds until the dialog leaves.
    /// </summary>
    public string OverlayStyle => $"animation-duration: {Duration}ms; animation-fill-mode: forwards";

    public static DialogAnimation? Create(
        MotionPreset? enterPreset,
        MotionPreset? exitPreset,
        IReadOnlyList<IReadOnlyDictionary<string, object>>? enterKeyframes,
        IReadOnlyList<IReadOnlyDictionary<string, object>>? exitKeyframes,
        int duration)
    {
        var animation = new DialogAnimation(enterPreset, exitPreset, enterKeyframes, exitKeyframes, duration);
        return animation.HasEnter || animation.HasExit ? animation : null;
    }

    /// <summary>
    /// Plays the enter or exit animation on <paramref name="element"/>, and ends when it does. An
    /// enter with nothing configured still runs, with no duration, so a dialog reopened mid-exit drops
    /// the exit's held last frame.
    /// </summary>
    public async Task PlayAsync(MotionInterop motion, ElementReference element, bool exiting)
    {
        var preset = exiting ? ExitPreset : EnterPreset;
        var keyframes = exiting ? ExitKeyframes : EnterKeyframes;
        var played = exiting ? HasExit : HasEnter;

        try
        {
            await motion.InvokeAsync("transition", element, new
            {
                preset = (preset ?? MotionPreset.Fade).ToString(),
                keyframes,
                duration = played ? Duration : 0,
                easing = exiting ? "ease-in" : "ease-out",
                distance = Distance,
                exiting
            });
        }
        catch (JSException)
        {
            // A failed animation must not keep the dialog on the page.
        }
    }
}
