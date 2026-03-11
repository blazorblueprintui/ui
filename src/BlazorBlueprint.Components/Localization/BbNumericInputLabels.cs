using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the NumericInput component.
/// </summary>
public sealed class BbNumericInputLabels
{
    [DisallowNull] public string IncreaseValue { get; set; } = "Increase value";
    [DisallowNull] public string DecreaseValue { get; set; } = "Decrease value";
}
