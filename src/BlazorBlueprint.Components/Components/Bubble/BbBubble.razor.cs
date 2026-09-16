using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays framed or unframed conversational bubble content.
/// </summary>
public partial class BbBubble : ComponentBase
{
    /// <summary>
    /// Gets or sets bubble visual variant.
    /// </summary>
    [Parameter]
    public BubbleVariant Variant { get; set; } = BubbleVariant.Default;

    /// <summary>
    /// Gets or sets inline alignment for this bubble row.
    /// </summary>
    [Parameter]
    public BubbleAlign Align { get; set; } = BubbleAlign.Start;

    /// <summary>
    /// Gets or sets custom classes for bubble root.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets bubble content and optional reactions.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Captures additional attributes for bubble root.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "bb:group/bubble bb:relative bb:flex bb:w-fit bb:max-w-[80%] bb:min-w-0 bb:flex-col bb:gap-1 bb:group-data-[align=end]/message:self-end bb:data-[align=end]:self-end bb:data-[variant=ghost]:max-w-full",
        Variant switch
        {
            BubbleVariant.Secondary => "bb:*:data-[slot=bubble-content]:bg-secondary bb:*:data-[slot=bubble-content]:text-secondary-foreground bb:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-[color-mix(in_oklch,var(--secondary),var(--foreground)_5%)]",
            BubbleVariant.Muted => "bb:*:data-[slot=bubble-content]:bg-muted bb:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-[color-mix(in_oklch,var(--muted),var(--foreground)_5%)]",
            BubbleVariant.Tinted => "bb:*:data-[slot=bubble-content]:bg-[oklch(from_var(--primary)_0.93_calc(c*0.4)_h)] bb:*:data-[slot=bubble-content]:text-primary-foreground bb:dark:*:data-[slot=bubble-content]:bg-[oklch(from_var(--primary)_0.3_calc(c*0.4)_h)] bb:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-[oklch(from_var(--primary)_0.88_calc(c*0.5)_h)] bb:dark:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-[oklch(from_var(--primary)_0.35_calc(c*0.5)_h)]",
            BubbleVariant.Outline => "bb:*:data-[slot=bubble-content]:border-border bb:*:data-[slot=bubble-content]:bg-background bb:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-muted bb:[&>[data-slot=bubble-content]:is(button,a):hover]:text-foreground bb:dark:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-input/30",
            BubbleVariant.Ghost => "bb:border-none bb:*:data-[slot=bubble-content]:rounded-none bb:*:data-[slot=bubble-content]:bg-transparent bb:*:data-[slot=bubble-content]:p-0 bb:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-muted bb:[&>[data-slot=bubble-content]:is(button,a):hover]:text-foreground bb:dark:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-muted/50",
            BubbleVariant.Destructive => "bb:*:data-[slot=bubble-content]:bg-destructive/10 bb:*:data-[slot=bubble-content]:text-destructive bb:dark:*:data-[slot=bubble-content]:bg-destructive/20 bb:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-destructive/20 bb:dark:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-destructive/30",
            _ => "bb:*:data-[slot=bubble-content]:bg-primary bb:*:data-[slot=bubble-content]:text-primary-foreground bb:[&>[data-slot=bubble-content]:is(button,a):hover]:bg-primary/80"
        },
        Align == BubbleAlign.End ? "bb:items-end" : "bb:items-start",
        Class
    );
}
