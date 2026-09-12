using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Full-card trigger overlay for an attachment card.
/// </summary>
public partial class BbAttachmentTrigger : ComponentBase
{
    /// <summary>
    /// Gets or sets the element type to render. Defaults to Button, but automatically switches to Anchor when <see cref="Href"/> is provided.
    /// </summary>
    [Parameter]
    public AttachmentTriggerElement AsChild { get; set; } = AttachmentTriggerElement.Button;

    /// <summary>
    /// Gets or sets href when rendering as an anchor.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets custom classes for the trigger element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Captures additional attributes for the trigger element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private AttachmentTriggerElement ResolvedElement =>
        AsChild == AttachmentTriggerElement.Button && !string.IsNullOrEmpty(Href) ? AttachmentTriggerElement.Anchor : AsChild;

    private string CssClass => ClassNames.cn(
        "absolute inset-0 z-10 outline-none",
        // #459: this covers the whole attachment card, so no ring-offset — an offset ring would
        // sit outside the card it belongs to. rounded-[inherit] follows the card's own radius.
        "rounded-[inherit] focus-visible:ring-2 focus-visible:ring-ring",
        Class
    );
}
