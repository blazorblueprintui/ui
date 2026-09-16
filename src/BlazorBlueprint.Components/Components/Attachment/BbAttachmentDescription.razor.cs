using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Description slot for attachment metadata such as file size or status.
/// </summary>
public partial class BbAttachmentDescription : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for description text.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets description content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for description element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:mt-0.5 bb:block bb:min-w-0 bb:truncate bb:text-xs bb:text-muted-foreground bb:group-data-[state=error]/attachment:text-destructive/80",
        "bb:w-full",
        Class
    );
}
