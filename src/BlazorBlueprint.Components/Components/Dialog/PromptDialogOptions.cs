namespace BlazorBlueprint.Components;

/// <summary>
/// Provides configuration options for a prompt dialog shown via
/// <see cref="DialogService.PromptAsync(string, string?, PromptDialogOptions?)"/>.
/// </summary>
public class PromptDialogOptions : DialogOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PromptDialogOptions"/> class.
    /// </summary>
    /// <remarks>
    /// Sets the default confirmation button text to <c>"OK"</c>.
    /// </remarks>
    public PromptDialogOptions()
    {
        ConfirmText = "OK";
    }

    /// <summary>
    /// Gets or sets the initial value of the input field.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed in the input field.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the input field is required.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the confirm button should be disabled
    /// until a non-empty value is entered.
    /// </remarks>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed length of the input value.
    /// </summary>
    /// <remarks>
    /// When specified, a validation error is shown if the input
    /// exceeds this length and the confirm button is disabled.
    /// </remarks>
    public int? MaxLength { get; set; }
}
