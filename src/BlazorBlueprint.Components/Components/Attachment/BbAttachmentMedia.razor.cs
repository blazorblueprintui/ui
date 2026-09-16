using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Media slot for an attachment card.
/// </summary>
public partial class BbAttachmentMedia : ComponentBase
{
    /// <summary>
    /// Gets or sets media variant.
    /// </summary>
    [Parameter]
    public AttachmentMediaVariant Variant { get; set; } = AttachmentMediaVariant.Icon;

    /// <summary>
    /// Gets or sets custom classes for the media slot.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets media content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the media element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:relative bb:flex bb:aspect-square bb:w-10 bb:shrink-0 bb:items-center bb:justify-center bb:overflow-hidden bb:rounded-lg bb:bg-muted bb:text-foreground bb:group-data-[orientation=vertical]/attachment:w-full bb:group-data-[size=sm]/attachment:w-8 bb:group-data-[size=xs]/attachment:w-7 bb:group-data-[size=xs]/attachment:rounded-md bb:group-data-[state=error]/attachment:bg-destructive/10 bb:group-data-[state=error]/attachment:text-destructive bb:[&_svg]:pointer-events-none bb:[&_svg:not([class*='size-'])]:size-4 bb:group-data-[orientation=vertical]/attachment:[&_svg:not([class*='size-'])]:size-6 bb:group-data-[size=xs]/attachment:[&_svg:not([class*='size-'])]:size-3.5",
        Variant == AttachmentMediaVariant.Icon
            ? "bb:opacity-60 bb:group-data-[state=done]/attachment:opacity-100 bb:group-data-[state=idle]/attachment:opacity-100"
            : "bb:*:[img]:aspect-square bb:*:[img]:w-full bb:*:[img]:object-cover",
        Class
    );
}
