using BlazorBlueprint.Components.Button;
using BlazorBlueprint.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorBlueprint.Components.SplitButton;

public partial class SplitButton : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? DropdownContent { get; set; }

    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Default;

    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Default;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public IconPosition IconPosition { get; set; } = IconPosition.Start;

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? DropdownAriaLabel { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => ClassNames.cn(
        "inline-flex",
        Class
    );

    private static string PrimaryButtonClass => ClassNames.cn(
        "!rounded-r-none border-r-0 focus-visible:z-10"
    );

    private string DropdownButtonClass => ClassNames.cn(
        "!rounded-l-none !px-2 focus-visible:z-10",
        Variant == ButtonVariant.Outline ? "border-l" : "border-l border-l-primary-foreground/20"
    );

    private ButtonSize DropdownButtonSize => Size switch
    {
        ButtonSize.Small => ButtonSize.Small,
        ButtonSize.Large => ButtonSize.Large,
        _ => ButtonSize.Default
    };
}
