using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorUI.Components.Item;

/// <summary>
/// A container component that groups related items together.
/// </summary>
/// <remarks>
/// ItemGroup uses semantic HTML with role="list" for accessibility
/// and provides flex column layout for Item children.
/// </remarks>
public partial class ItemGroup : ComponentBase
{
    /// <summary>
    /// Gets or sets additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the group.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the group element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        "flex flex-col gap-0.5",
        Class
    );
}
