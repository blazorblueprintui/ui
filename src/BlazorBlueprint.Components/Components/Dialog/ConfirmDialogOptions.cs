namespace BlazorBlueprint.Components;

/// <summary>
/// Options for customizing a confirm dialog shown via <see cref="DialogService"/>.
/// </summary>
public class ConfirmDialogOptions : DialogOptions
{
    public ConfirmDialogOptions()
    {
        ConfirmText = "Continue";
    }

    /// <summary>
    /// Whether the confirm button should use the destructive variant.
    /// Default: false (uses primary/default variant).
    /// </summary>
    public bool Destructive { get; set; }
}
