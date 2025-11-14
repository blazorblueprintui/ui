using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Item;

/// <summary>
/// A component for displaying visual content (icons/images) in an Item.
/// </summary>
/// <remarks>
/// ItemMedia supports different variants for icons and images with
/// specific sizing and styling options.
/// </remarks>
public partial class ItemMedia : ComponentBase
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
    /// Gets or sets the content to be rendered inside the media container.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the media element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base media styles
        "flex shrink-0 items-center justify-center",
        // Variant-specific styles
        Variant switch
        {
            ItemMediaVariant.Icon => "size-8 rounded-md border border-border",
            ItemMediaVariant.Image => "size-10 overflow-hidden rounded-lg",
            _ => ""
        },
        Class
    );
}
