using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Header slot displayed above a message surface.
/// </summary>
public partial class BbMessageHeader : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for the message header.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets header content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the header container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:flex bb:max-w-full bb:min-w-0 bb:items-center bb:px-3 bb:text-xs bb:font-medium bb:text-muted-foreground bb:group-has-data-[variant=ghost]/message:px-0",
        Class);
}
