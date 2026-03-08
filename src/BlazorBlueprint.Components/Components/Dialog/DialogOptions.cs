namespace BlazorBlueprint.Components;

public abstract class DialogOptions
{
    /// <summary>
    /// The label for the confirm/action button. Default: "OK".
    /// </summary>
    public string ConfirmText { get; set; } = "OK";

    /// <summary>
    /// The label for the cancel button. Default: "Cancel".
    /// </summary>
    public string CancelText { get; set; } = "Cancel";
}
