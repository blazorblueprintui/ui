using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays an inline conversation marker such as status updates, separators, or bordered notes.
/// </summary>
public partial class BbMarker : ComponentBase
{
    /// <summary>
    /// Gets or sets the marker visual variant.
    /// </summary>
    [Parameter]
    public MarkerVariant Variant { get; set; } = MarkerVariant.Default;

    /// <summary>
    /// Gets or sets the element type to render. Defaults to Div, but automatically switches to Anchor when <see cref="Href"/> is provided.
    /// </summary>
    [Parameter]
    public MarkerElement AsChild { get; set; } = MarkerElement.Div;

    /// <summary>
    /// Gets or sets the href when rendering as an anchor.
    /// </summary>
    [Parameter]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the marker root.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets marker content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures unmatched attributes for the rendered element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private MarkerElement ResolvedElement =>
        AsChild == MarkerElement.Div && !string.IsNullOrEmpty(Href) ? MarkerElement.Anchor : AsChild;

    private string CssClass => ClassNames.cn(
        "group/marker relative flex min-h-4 w-full items-center gap-2 text-left text-sm text-muted-foreground [&_svg:not([class*='size-'])]:size-4 [a]:underline [a]:underline-offset-3 [a]:hover:text-foreground",
        Variant switch
        {
            MarkerVariant.Border => "border-b border-border pb-2",
            MarkerVariant.Separator => "w-full items-center justify-center text-xs uppercase tracking-wide",
            _ => null
        },
        Variant == MarkerVariant.Separator
            ? "before:mr-1 before:h-px before:min-w-0 before:flex-1 before:bg-border after:ml-1 after:h-px after:min-w-0 after:flex-1 after:bg-border"
            : null,
        Class
    );
}
