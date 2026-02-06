using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace BlazorBlueprint.Components.Item;

/// <summary>
/// A flexible, composable item component for building list items.
/// </summary>
/// <remarks>
/// The Item component provides a structure for list items with support
/// for variants, sizes, and polymorphic rendering via AsChild parameter.
/// </remarks>
public partial class Item : ComponentBase
{
    /// <summary>
    /// Gets or sets the visual style variant of the item.
    /// </summary>
    [Parameter]
    public ItemVariant Variant { get; set; } = ItemVariant.Default;

    /// <summary>
    /// Gets or sets the size of the item.
    /// </summary>
    [Parameter]
    public ItemSize Size { get; set; } = ItemSize.Default;

    /// <summary>
    /// Gets or sets the element type to render as (e.g., "a", "button").
    /// When set, the component renders as that element instead of a div.
    /// </summary>
    [Parameter]
    public string? AsChild { get; set; }

    /// <summary>
    /// Gets or sets the href attribute when rendering as an anchor.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the item.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the item.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the computed CSS classes for the item element.
    /// </summary>
    private string CssClass => ClassNames.cn(
        // Base item styles
        "group relative flex items-center gap-3 rounded-lg",
        // Only add transition-colors if interactive
        IsInteractive ? "transition-colors" : null,
        // Variant-specific styles
        Variant switch
        {
            ItemVariant.Outline => IsInteractive
                ? "border border-border bg-background hover:bg-accent"
                : "border border-border bg-background",
            ItemVariant.Muted => IsInteractive
                ? "bg-muted hover:bg-muted/80"
                : "bg-muted",
            _ => IsInteractive ? "hover:bg-accent" : null
        },
        // Size-specific styles
        Size switch
        {
            ItemSize.Sm => "px-3 py-2 text-sm",
            _ => "px-4 py-3"
        },
        Class
    );

    /// <summary>
    /// Gets whether this item is interactive (clickable/hoverable).
    /// </summary>
    private bool IsInteractive => !string.IsNullOrEmpty(AsChild) ||
                                   (AdditionalAttributes?.ContainsKey("onclick") == true);

    /// <summary>
    /// Gets the element type to render based on AsChild parameter.
    /// </summary>
    private Type GetElementType()
    {
        return AsChild?.ToLower(CultureInfo.InvariantCulture) switch
        {
            "a" => typeof(AnchorElement),
            "button" => typeof(ButtonElement),
            _ => typeof(DivElement)
        };
    }

    /// <summary>
    /// Gets the attributes to pass to the dynamic element.
    /// </summary>
    private Dictionary<string, object> GetElementAttributes()
    {
        var attributes = new Dictionary<string, object>
        {
            ["class"] = CssClass,
            ["data-slot"] = "item",
            ["ChildContent"] = (object?)ChildContent!
        };

        if (!string.IsNullOrEmpty(Href) && AsChild == "a")
        {
            attributes["href"] = Href;
        }

        if (AdditionalAttributes != null)
        {
            foreach (var attr in AdditionalAttributes)
            {
                attributes[attr.Key] = attr.Value;
            }
        }

        return attributes;
    }

    // Helper components for DynamicComponent
    private sealed class DivElement : ComponentBase
    {
        [Parameter] public string? @class { get; set; }
        [Parameter] public string? DataSlot { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? Attributes { get; set; }

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", @class);
            builder.AddAttribute(2, "data-slot", DataSlot);
            builder.AddMultipleAttributes(3, Attributes);
            builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }
    }

    private sealed class AnchorElement : ComponentBase
    {
        [Parameter] public string? @class { get; set; }
        [Parameter] public string? DataSlot { get; set; }
        [Parameter] public string? href { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? Attributes { get; set; }

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "a");
            builder.AddAttribute(1, "class", @class);
            builder.AddAttribute(2, "data-slot", DataSlot);
            if (!string.IsNullOrEmpty(href))
            {
                builder.AddAttribute(3, "href", href);
            }
            builder.AddMultipleAttributes(4, Attributes);
            builder.AddContent(5, ChildContent);
            builder.CloseElement();
        }
    }

    private sealed class ButtonElement : ComponentBase
    {
        [Parameter] public string? @class { get; set; }
        [Parameter] public string? DataSlot { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? Attributes { get; set; }

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "type", "button");
            builder.AddAttribute(2, "class", @class);
            builder.AddAttribute(3, "data-slot", DataSlot);
            builder.AddMultipleAttributes(4, Attributes);
            builder.AddContent(5, ChildContent);
            builder.CloseElement();
        }
    }
}
