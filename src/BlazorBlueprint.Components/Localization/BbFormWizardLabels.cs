using System.Diagnostics.CodeAnalysis;

namespace BlazorBlueprint.Components;

/// <summary>
/// Localizable labels for the FormWizard component.
/// </summary>
public sealed class BbFormWizardLabels
{
    [DisallowNull] public string WizardProgress { get; set; } = "Wizard progress";
    [DisallowNull] public string Back { get; set; } = "Back";
    [DisallowNull] public string Next { get; set; } = "Next";
    [DisallowNull] public string Skip { get; set; } = "Skip";
    [DisallowNull] public string Complete { get; set; } = "Complete";
}
