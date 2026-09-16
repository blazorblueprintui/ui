using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// A component for displaying visual content (icons/images) in an Item.
/// </summary>
/// <remarks>
/// ItemMedia supports different variants for icons and images with
/// specific sizing and styling options.
/// </remarks>
public partial class BbItemMedia : ComponentBase
{
    /// <summary>
    /// Gets or sets the visual style variant of the media.
    /// </summary>
    [Parameter]
    public ItemMediaVariant Variant { get; set; } = ItemMediaVariant.Default;

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the media container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the media container.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the media element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base media styles
        "bb:flex bb:shrink-0 bb:items-center bb:justify-center",
        // Variant-specific styles
        Variant switch
        {
            ItemMediaVariant.Icon => "bb:size-8 bb:rounded-md bb:border bb:border-border",
            ItemMediaVariant.Image => "bb:size-10 bb:overflow-hidden bb:rounded-lg",
            _ => ""
        },
        Class
    );
}
