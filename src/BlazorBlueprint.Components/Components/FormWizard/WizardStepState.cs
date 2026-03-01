namespace BlazorBlueprint.Components;

/// <summary>
/// Represents the visual state of a wizard step in the step indicator.
/// </summary>
public enum WizardStepState
{
    /// <summary>
    /// Step has not been visited yet.
    /// </summary>
    Pending,

    /// <summary>
    /// Step is currently displayed and active.
    /// </summary>
    Active,

    /// <summary>
    /// Step has been visited and passed validation.
    /// </summary>
    Completed,

    /// <summary>
    /// Step has been visited but has validation errors.
    /// </summary>
    Invalid,

    /// <summary>
    /// Optional step that was skipped.
    /// </summary>
    Skipped
}
