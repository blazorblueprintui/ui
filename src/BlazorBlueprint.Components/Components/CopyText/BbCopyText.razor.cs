using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// Displays a short, highlighted piece of text that copies a value
/// to the clipboard when clicked.
/// </summary>
public partial class BbCopyText : ComponentBase, IAsyncDisposable
{
    // Outcome codes, mirroring the constants in clipboard.js.
    private const string CopyOutcomeOk = "ok";
    private const string CopyOutcomeRefused = "refused";
    private const string CopyOutcomeNoValue = "noValue";

    private IJSObjectReference? clipboardModule;
    private DotNetObjectReference<BbCopyText>? copyTextRef;
    private string? lastAsyncValue;
    private IJSObjectReference? elementUtilsModule;
    private ElementReference anchorRef;
    private readonly string portalId = $"copytext-portal-{Guid.NewGuid():N}";
    private bool isHovered;
    private bool copied;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IBbLocalizer Localizer { get; set; } = default!;

    /// <summary>
    /// Sets the value to be copied to the clipboard when clicked.
    /// </summary>
    /// <remarks>
    /// Takes precedence over <see cref="ValueFunc"/> when non-empty. Not <c>EditorRequired</c>, because
    /// supplying <see cref="ValueFunc"/> instead is a complete configuration.
    /// </remarks>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets a function producing the value to copy, evaluated at click time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this where the text is not known up front — derived from state that moves, or expensive
    /// enough that computing it for every render would be wasteful. It is only called when the user
    /// actually copies.
    /// </para>
    /// <para>
    /// <see cref="Value"/> wins when it is a non-empty string. The test is emptiness rather than null
    /// so that an unset-but-bound <c>Value</c> — the empty string, which a plain <c>string</c> binding
    /// reaches naturally — falls through to the function instead of silently copying nothing.
    /// </para>
    /// <para>
    /// Use <see cref="ValueFuncAsync"/> when the value needs awaiting. It is a separate parameter
    /// rather than an overload because the write itself has to be arranged differently — see its
    /// remarks.
    /// </para>
    /// </remarks>
    [Parameter]
    public Func<string?>? ValueFunc { get; set; }

    /// <summary>
    /// Gets or sets an asynchronous function producing the value to copy, evaluated at click time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this where the text has to be fetched or computed asynchronously — a server round trip,
    /// say. <see cref="Value"/> wins when non-empty, then <see cref="ValueFunc"/>, then this.
    /// </para>
    /// <para>
    /// <b>Why this is not simply an awaited <c>ValueFunc</c>.</b> A clipboard write requires
    /// transient user activation, and awaiting spends it — so resolving the text first and writing
    /// second gets the write refused, Safari most strictly, while the component still showed its
    /// copied state. The function is instead invoked from JavaScript <i>inside</i> a
    /// <c>ClipboardItem</c>, handing the browser the promise rather than the result, which keeps
    /// the activation alive for as long as the callback takes.
    /// </para>
    /// <para>
    /// On a browser without promise-aware <c>ClipboardItem</c> support the value has to be resolved
    /// before writing, so a slow function can still be refused there. That is reported through
    /// <see cref="OnCopyFailed"/> rather than passing silently.
    /// </para>
    /// </remarks>
    [Parameter]
    public Func<Task<string?>>? ValueFuncAsync { get; set; }

    /// <summary>
    /// Invoked when a copy did not happen, with the reason.
    /// </summary>
    /// <remarks>
    /// A failed copy used to be invisible — the component showed its copied state and the clipboard
    /// stayed empty. Worth handling whenever <see cref="ValueFuncAsync"/> is in use, where a
    /// refusal is much more likely.
    /// </remarks>
    [Parameter]
    public EventCallback<CopyTextFailure> OnCopyFailed { get; set; }

    /// <summary>
    /// Gets or sets the content displayed inside the copy text element.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the text.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the copy button.
    /// Defaults to the localized "Click to copy" text.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

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
        // #459: rounded-sm keeps the ring on the text rather than boxing the whole line, since
        // this is inline and usually sits mid-sentence.
        "rounded-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
        Class);

    // Positioning, offset and z-index now come from the floating portal, so only the visual
    // chrome is left here. The opacity/translate pair went with it: the portal mounts the
    // tooltip when it opens rather than keeping a transparent copy in the layout. With no
    // state left to vary on, this is a constant rather than a computed class string.
    private const string TooltipCssClass =
        "pointer-events-none inline-flex items-center gap-1.5 whitespace-nowrap " +
        "rounded-md border bg-popover px-2.5 py-1 text-xs font-medium shadow-md";

    private void HandleMouseEnter()
    {
        ShowTooltip();
    }

    private void HandleMouseLeave()
    {
        isHovered = false;
    }

    /// <summary>
    /// Shows the tooltip on focus, but only when the focus came from the keyboard.
    /// </summary>
    /// <remarks>
    /// Returning to a background browser tab restores focus to whatever held it, which fires
    /// <c>focus</c> again and used to reopen the tooltip on the last-clicked instance. Nothing
    /// then closed it, because the pointer was never over it to leave — reported in #506.
    ///
    /// Same discrimination as <c>BbHoverCardTrigger</c> (#504), through the same helper: a recent
    /// Tab keydown, rather than <c>:focus-visible</c>, which Chrome reports as true for a
    /// programmatic focus move. When the check cannot run the tooltip shows, because a keyboard
    /// user losing it is worse than it lingering after a tab switch.
    /// </remarks>
    private async Task HandleFocusAsync()
    {
        if (!await IsKeyboardFocusAsync())
        {
            return;
        }

        ShowTooltip();
        StateHasChanged();
    }

    private async Task<bool> IsKeyboardFocusAsync()
    {
        try
        {
            elementUtilsModule ??= await JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorBlueprint.Primitives/js/primitives/element-utils.js");
            return await elementUtilsModule.InvokeAsync<bool>("isKeyboardFocus", anchorRef);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            return true;
        }
    }

    private void HandleBlur()
    {
        isHovered = false;
    }

    private void ShowTooltip()
    {
        isHovered = true;

        if (copied)
        {
            copied = false;
        }
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or " ")
        {
            await HandleClickAsync();
        }
    }

    /// <summary>
    /// Resolves the text to copy: <see cref="Value"/> when non-empty, otherwise <see cref="ValueFunc"/>.
    /// </summary>
    private string? ResolveValue() =>
        !string.IsNullOrEmpty(Value) ? Value : ValueFunc?.Invoke();

    private async Task HandleClickAsync()
    {
        // Resolved once per click, not per render — ValueFunc may be expensive, and copying then
        // reporting two different strings through OnCopied would be worse than either.
        var value = ResolveValue();

        // The async path deliberately does NOT resolve here. Awaiting the consumer's task before
        // writing spends the transient user activation and gets the write refused, which is the
        // whole of #466. JS calls back into ResolveAsyncValue from inside the ClipboardItem.
        var outcome = string.IsNullOrEmpty(value) && ValueFuncAsync is not null
            ? await CopyFromAsyncSourceAsync()
            : await CopyToClipboardAsync(value);

        if (outcome != CopyOutcomeOk)
        {
            await ReportFailureAsync(outcome);
            return;
        }

        copied = true;

        if (OnCopied.HasDelegate)
        {
            // The async path resolves in JS, so the text is only known here once it comes back.
            await OnCopied.InvokeAsync(value ?? lastAsyncValue);
        }
    }

    /// <summary>
    /// Called from JS while the clipboard write is already in flight. Not part of the public API.
    /// </summary>
    /// <remarks>
    /// This runs <i>inside</i> the <c>ClipboardItem</c> promise, which is what keeps the user
    /// activation alive across a slow consumer callback.
    /// </remarks>
    [JSInvokable]
    public async Task<string?> ResolveAsyncValue()
    {
        if (ValueFuncAsync is null)
        {
            return null;
        }

        lastAsyncValue = await ValueFuncAsync();
        return lastAsyncValue;
    }

    private async Task<string> CopyToClipboardAsync(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return CopyOutcomeNoValue;
        }

        try
        {
            var module = await GetClipboardModuleAsync();
            return await module.InvokeAsync<string>("copyToClipboard", text);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
        {
            return CopyOutcomeRefused;
        }
    }

    private async Task<string> CopyFromAsyncSourceAsync()
    {
        try
        {
            copyTextRef ??= DotNetObjectReference.Create(this);
            var module = await GetClipboardModuleAsync();
            return await module.InvokeAsync<string>(
                "copyFromAsyncSource", copyTextRef, nameof(ResolveAsyncValue));
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
        {
            return CopyOutcomeRefused;
        }
    }

    private async Task<IJSObjectReference> GetClipboardModuleAsync() =>
        clipboardModule ??= await JS.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazorBlueprint.Components/js/clipboard.js");

    private async Task ReportFailureAsync(string outcome)
    {
        if (!OnCopyFailed.HasDelegate)
        {
            return;
        }

        await OnCopyFailed.InvokeAsync(
            outcome == CopyOutcomeNoValue ? CopyTextFailure.NoValue : CopyTextFailure.Refused);
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
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit already gone; nothing to clean up.
            }
        }

        if (elementUtilsModule is not null)
        {
            try
            {
                await elementUtilsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit already gone; nothing to clean up.
            }
        }

        copyTextRef?.Dispose();
        copyTextRef = null;

        GC.SuppressFinalize(this);
    }
}
