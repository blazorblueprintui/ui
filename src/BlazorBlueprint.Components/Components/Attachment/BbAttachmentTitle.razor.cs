using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Title slot that displays the attachment name.
/// </summary>
public partial class BbAttachmentTitle : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for title text.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets title content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for title element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:block bb:max-w-full bb:min-w-0 bb:truncate bb:font-medium bb:group-data-[state=processing]/attachment:shimmer bb:group-data-[state=uploading]/attachment:shimmer",
        Class
    );
}
