using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays a short, highlighted piece of text that copies a value
/// to the clipboard when clicked.
/// </summary>
public partial class BbCopyText : ComponentBase, IAsyncDisposable
{
    private readonly string tooltipId = $"bb-copytext-{Guid.NewGuid():N}";
    private IJSObjectReference? clipboardModule;
    private bool isHovered;
    private bool copied;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IBbLocalizer Localizer { get; set; } = default!;

    /// <summary>
    /// Sets the value to be copied to the clipboard when clicked.
    /// </summary>
    [Parameter, EditorRequired]
    public string? Value { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the text.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Callback invoked when the user clicks the component to copy text.
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnCopied { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the container.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string CurrentIconName => copied ? "check" : "copy";

    private string CurrentTooltipText => copied ? Localizer["CopyText.Copied"] : Localizer["CopyText.ClickToCopy"];

    private string TooltipIconCssClass => copied && isHovered ? "h-3 w-3 text-alert-success" : "h-3 w-3 text-primary";

    private string TooltipTextCssClass => copied ? "text-alert-success" : "text-foreground";

    private string? TextCssClass => ClassNames.cn(
        "relative inline-flex gap-1 items-center cursor-pointer text-primary font-semibold",
        Class);

    private string? TooltipCssClass => ClassNames.cn(
        "pointer-events-none absolute bottom-full left-1/2 z-50 mb-2 inline-flex " +
        "-translate-x-1/2 items-center gap-1.5 whitespace-nowrap rounded-md border " +
        "bg-popover px-2.5 py-1 text-xs font-medium shadow-md ",
        isHovered ? "translate-y-0 opacity-100" : "translate-y-1 opacity-0");

    private void HandleMouseEnter()
    {
        isHovered = true;

        if (copied)
        {
            copied = false;
        }
    }

    private void HandleMouseLeave()
    {
        isHovered = false;
    }

    private async Task HandleClickAsync()
    {
        var success = await CopyToClipboardAsync(Value ?? string.Empty);
        if (!success)
        {
            return;
        }

        copied = true;

        if (OnCopied.HasDelegate)
        {
            await OnCopied.InvokeAsync(Value);
        }
    }

    private async Task<bool> CopyToClipboardAsync(string text)
    {
        try
        {
            clipboardModule ??= await JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Components/js/clipboard.js");
            return await clipboardModule.InvokeAsync<bool>("copyToClipboard", text);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (clipboardModule is not null)
        {
            try
            {
                await clipboardModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already gone; nothing to clean up.
            }
        }

        GC.SuppressFinalize(this);
    }
}
