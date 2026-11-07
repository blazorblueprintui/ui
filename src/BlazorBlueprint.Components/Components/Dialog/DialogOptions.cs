namespace BlazorBlueprint.Components;

public abstract class DialogOptions
{
    /// <summary>
    /// The label for the confirm/action button.
    /// When null, the dialog uses a localized default from <see cref="IBbLocalizer"/>
    /// ("ConfirmDialog.Confirm" = "Continue" for confirm dialogs, "PromptDialog.Confirm" = "OK" for prompt dialogs).
    /// </summary>
    public string? ConfirmText { get; set; }

    /// <summary>
    /// The label for the cancel button.
    /// When null, the dialog uses a localized default from <see cref="IBbLocalizer"/>
    /// ("ConfirmDialog.Cancel" / "PromptDialog.Cancel" = "Cancel").
    /// </summary>
    public string? CancelText { get; set; }
}
