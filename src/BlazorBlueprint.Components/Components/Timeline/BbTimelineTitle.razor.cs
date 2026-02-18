using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorBlueprint.Components;

/// <summary>
/// Renders the title heading for a timeline item.
/// </summary>
public partial class BbTimelineTitle : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to render.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures any additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// The HTML element tag to use for the title.
    /// Default is "h3".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "h3";

    private string CssClass => ClassNames.cn(
        "font-semibold leading-none tracking-tight text-foreground",
        Class
    );

    private RenderFragment HeadingFragment => builder =>
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "class", CssClass);
        builder.AddMultipleAttributes(2, AdditionalAttributes);
        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    };
}
