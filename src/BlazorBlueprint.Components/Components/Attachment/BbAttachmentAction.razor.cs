using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Action control for an attachment, rendered as a button.
/// </summary>
public partial class BbAttachmentAction : ComponentBase
{
    /// <summary>
    /// Gets or sets the button variant for the action.
    /// </summary>
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Ghost;

    /// <summary>
    /// Gets or sets the button size for the action.
    /// </summary>
    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.IconSmall;

    /// <summary>
    /// Gets or sets additional classes for the action button.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets action content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the underlying button.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
}
