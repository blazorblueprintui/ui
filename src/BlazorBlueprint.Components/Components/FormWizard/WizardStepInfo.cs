namespace BlazorBlueprint.Components;

/// <summary>
/// Internal data class holding metadata for a wizard step.
/// Populated by <see cref="BbWizardStep"/> during registration.
/// </summary>
internal class WizardStepInfo
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsOptional { get; set; }
    public bool Disabled { get; set; }
    public string[]? FieldNames { get; set; }
    public Func<bool>? Validator { get; set; }
}
