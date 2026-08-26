using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays a file or image attachment card with media, metadata, actions, and trigger overlay support.
/// </summary>
public partial class BbAttachment : ComponentBase
{
    /// <summary>
    /// Gets or sets attachment upload state.
    /// </summary>
    [Parameter]
    public AttachmentState State { get; set; } = AttachmentState.Done;

    /// <summary>
    /// Gets or sets attachment size.
    /// </summary>
    [Parameter]
    public AttachmentSize Size { get; set; } = AttachmentSize.Default;

    /// <summary>
    /// Gets or sets layout orientation.
    /// </summary>
    [Parameter]
    public AttachmentOrientation Orientation { get; set; } = AttachmentOrientation.Horizontal;

    /// <summary>
    /// Gets or sets custom classes for the attachment root.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets attachment content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the attachment root.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "group/attachment relative flex w-fit max-w-full min-w-0 shrink-0 flex-wrap rounded-2xl border bg-card text-card-foreground transition-colors focus-within:ring-1 focus-within:ring-ring/30 has-[>a,>button]:hover:bg-muted/50 data-[state=error]:border-destructive/30 data-[state=idle]:border-dashed",
        Orientation == AttachmentOrientation.Vertical ? "w-24 flex-col has-data-[slot=attachment-content]:w-30" : "min-w-40 items-center",
        Size switch
        {
            AttachmentSize.Sm => "gap-2.5 text-xs has-data-[slot=attachment-content]:px-2 has-data-[slot=attachment-content]:py-1.5 has-data-[slot=attachment-media]:p-1.5",
            AttachmentSize.Xs => "gap-1.5 rounded-xl text-xs has-data-[slot=attachment-content]:px-1.5 has-data-[slot=attachment-content]:py-1 has-data-[slot=attachment-media]:p-1",
            _ => "gap-2 text-sm has-data-[slot=attachment-content]:px-2.5 has-data-[slot=attachment-content]:py-2 has-data-[slot=attachment-media]:p-2",
        },
        State switch
        {
            AttachmentState.Uploading => "border-primary/40 bg-primary/5",
            AttachmentState.Processing => "border-primary/40 bg-primary/5",
            AttachmentState.Error => "border-destructive/40 bg-destructive/5",
            _ => null
        },
        Class
    );
}
