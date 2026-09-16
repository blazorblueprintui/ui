using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays a single conversation row with optional avatar, header, content, and footer slots.
/// </summary>
public partial class BbMessage : ComponentBase
{
    /// <summary>
    /// Gets or sets message alignment within the transcript.
    /// </summary>
    [Parameter]
    public MessageAlign Align { get; set; } = MessageAlign.Start;

    /// <summary>
    /// Gets or sets custom classes for the message row.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets message row content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the message row.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:group/message bb:relative bb:flex bb:w-full bb:min-w-0 bb:gap-2 bb:text-sm bb:data-[align=end]:flex-row-reverse",
        Align == MessageAlign.End ? "bb:justify-end" : "bb:justify-start",
        Class
    );
}
