namespace BlazorBlueprint.Components;

public abstract class DialogOptions
{
    /// <summary>
    /// The label for the confirm/action button. Default: "Continue".
    /// </summary>
    public string ConfirmText { get; set; } = default!;

    /// <summary>
    /// The label for the cancel button. Default: "Cancel".
    /// </summary>
    public string CancelText { get; set; } = "Cancel";
}
