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
        "flex max-w-full min-w-0 items-center self-start px-3 text-left text-xs font-medium text-muted-foreground group-has-data-[variant=ghost]/message:px-0 group-data-[align=end]/message:justify-end group-data-[align=end]/message:self-end group-data-[align=end]/message:text-right",
        Class
    );
}
