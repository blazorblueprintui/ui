using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Footer slot displayed below a message surface.
/// </summary>
public partial class BbMessageFooter : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for the footer.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets footer content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the footer container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:flex bb:max-w-full bb:min-w-0 bb:items-center bb:self-start bb:px-3 bb:text-left bb:text-xs bb:font-medium bb:text-muted-foreground bb:group-has-data-[variant=ghost]/message:px-0 bb:group-data-[align=end]/message:justify-end bb:group-data-[align=end]/message:self-end bb:group-data-[align=end]/message:text-right",
        Class
    );
}
