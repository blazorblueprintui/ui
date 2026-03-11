using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the Calendar component.
/// </summary>
public sealed class BbCalendarLabels
{
    /// <summary>
    /// Aria label for the previous month navigation button.
    /// </summary>
    [DisallowNull] public string GoToPreviousMonth { get; set; } = "Go to previous month";

    /// <summary>
    /// Aria label for the next month navigation button.
    /// </summary>
    [DisallowNull] public string GoToNextMonth { get; set; } = "Go to next month";
}
