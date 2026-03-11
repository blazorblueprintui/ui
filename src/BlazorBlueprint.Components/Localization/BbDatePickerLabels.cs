using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the DatePicker component.
/// </summary>
public sealed class BbDatePickerLabels
{
    [DisallowNull] public string Placeholder { get; set; } = "Pick a date";
}
