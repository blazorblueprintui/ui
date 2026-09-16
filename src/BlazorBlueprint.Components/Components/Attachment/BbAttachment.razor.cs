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
        "bb:group/attachment bb:relative bb:flex bb:w-fit bb:max-w-full bb:min-w-0 bb:shrink-0 bb:flex-wrap bb:rounded-2xl bb:border bb:bg-card bb:text-card-foreground bb:transition-colors bb:focus-within:ring-1 bb:focus-within:ring-ring/30 bb:has-[>a,>button]:hover:bg-muted/50 bb:data-[state=error]:border-destructive/30 bb:data-[state=idle]:border-dashed",
        Orientation == AttachmentOrientation.Vertical ? "bb:w-24 bb:flex-col bb:has-data-[slot=attachment-content]:w-30" : "bb:min-w-40 bb:items-center",
        Size switch
        {
            AttachmentSize.Sm => "bb:gap-2.5 bb:text-xs bb:has-data-[slot=attachment-content]:px-2 bb:has-data-[slot=attachment-content]:py-1.5 bb:has-data-[slot=attachment-media]:p-1.5",
            AttachmentSize.Xs => "bb:gap-1.5 bb:rounded-xl bb:text-xs bb:has-data-[slot=attachment-content]:px-1.5 bb:has-data-[slot=attachment-content]:py-1 bb:has-data-[slot=attachment-media]:p-1",
            _ => "bb:gap-2 bb:text-sm bb:has-data-[slot=attachment-content]:px-2.5 bb:has-data-[slot=attachment-content]:py-2 bb:has-data-[slot=attachment-media]:p-2",
        },
        State switch
        {
            AttachmentState.Uploading => "bb:border-primary/40 bb:bg-primary/5",
            AttachmentState.Processing => "bb:border-primary/40 bb:bg-primary/5",
            AttachmentState.Error => "bb:border-destructive/40 bb:bg-destructive/5",
            _ => null
        },
        Class
    );
}
