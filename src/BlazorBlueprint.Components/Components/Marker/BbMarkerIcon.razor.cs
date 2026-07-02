using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Decorative icon slot for <see cref="BbMarker"/>.
/// </summary>
public partial class BbMarkerIcon : ComponentBase
{
    /// <summary>
    /// Gets or sets custom classes for the icon container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets icon content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for the icon container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn("size-4 shrink-0 [&_svg:not([class*='size-'])]:size-4", Class);
}
