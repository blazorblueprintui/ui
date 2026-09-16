using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Text content slot for <see cref="BbMarker"/>.
/// </summary>
public partial class BbMarkerContent : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for marker text content.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets marker text or rich content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the content element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn("bb:min-w-0 bb:wrap-break-word bb:group-data-[variant=separator]/marker:flex-none bb:group-data-[variant=separator]/marker:text-center bb:*:[a]:underline bb:*:[a]:underline-offset-3 bb:*:[a]:hover:text-foreground", Class);
}
