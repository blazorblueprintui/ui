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
        // #459: no offset — markers repeat down a list and an offset ring hits the rows either side.
        "bb:rounded-sm bb:focus-visible:outline-none bb:focus-visible:ring-2 bb:focus-visible:ring-ring",
        "bb:group/marker bb:relative bb:flex bb:min-h-4 bb:w-full bb:items-center bb:gap-2 bb:text-left bb:text-sm bb:text-muted-foreground bb:[&_svg:not([class*='size-'])]:size-4 bb:[a]:underline bb:[a]:underline-offset-3 bb:[a]:hover:text-foreground",
        Variant switch
        {
            MarkerVariant.Border => "bb:border-b bb:border-border bb:pb-2",
            MarkerVariant.Separator => "bb:w-full bb:items-center bb:justify-center bb:text-xs bb:uppercase bb:tracking-wide",
            _ => null
        },
        Variant == MarkerVariant.Separator
            ? "bb:before:mr-1 bb:before:h-px bb:before:min-w-0 bb:before:flex-1 bb:before:bg-border bb:after:ml-1 bb:after:h-px bb:after:min-w-0 bb:after:flex-1 bb:after:bg-border"
            : null,
        Class
    );
}
