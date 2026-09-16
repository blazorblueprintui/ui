using Ganss.Xss;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Components;

/// <summary>
/// A markdown editor component with toolbar and preview functionality.
/// Inspired by GitHub's comment editor design.
/// </summary>
public partial class BbMarkdownEditor : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<BbMarkdownEditor>? _dotNetRef;
    private ElementReference _textareaRef;
    private string _activeTab = "write";
    private bool _shouldPreventKeydown;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the markdown content value.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text displayed when the editor is empty.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets whether the editor is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the editor container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute for the textarea element.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the textarea.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ID of the element that describes the textarea.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the textarea value is invalid.
    /// </summary>
    [Parameter]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    /// Gets or sets the minimum height for the editor textarea and preview areas (e.g., "200px", "10rem").
    /// Defaults to "150px".
    /// </summary>
    [Parameter]
    public string? MinHeight { get; set; }

    /// <summary>
    /// Markdig pipeline for converting markdown to HTML.
    /// </summary>
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoLinks()
        .Build();

    /// <summary>
    /// HTML sanitizer for XSS prevention. Thread-safe for static usage.
    /// </summary>
    private static readonly HtmlSanitizer Sanitizer = new();

    /// <summary>
    /// Gets the rendered HTML from the markdown content.
    /// HTML is sanitized to prevent XSS attacks.
    /// </summary>
    private string RenderedHtml => string.IsNullOrWhiteSpace(Value)
        ? "<p class=\"text-muted-foreground italic\">Nothing to preview</p>"
        : Sanitizer.Sanitize(Markdown.ToHtml(Value, Pipeline));

    /// <summary>
    /// Gets the CSS classes for the editor container.
    /// </summary>
    private string ContainerCssClass => ClassNames.cn(
        "bb:flex bb:flex-col bb:rounded-md bb:border bb:border-input bb:bg-background",
        "bb:focus-within:ring-2 bb:focus-within:ring-ring bb:focus-within:ring-offset-2",
        ClassNames.when(AriaInvalid == true, "bb:border-destructive bb:ring-destructive/20"),
        ClassNames.when(Disabled, "bb:opacity-50 bb:cursor-not-allowed"),
        Class
    );

    /// <summary>
    /// Gets the inline style for min-height override.
    /// </summary>
    private string? MinHeightStyle => MinHeight != null ? $"min-height:{MinHeight}" : null;

    /// <summary>
    /// Gets the CSS classes for the textarea.
    /// </summary>
    private string TextareaCssClass => ClassNames.cn(
        "bb:flex-1 bb:w-full bb:resize-y bb:border-0 bb:bg-transparent",
        MinHeight == null ? "bb:min-h-[150px]" : null,
        "bb:px-3 bb:py-2 bb:text-sm bb:placeholder:text-muted-foreground",
        "bb:focus:outline-none",
        "bb:disabled:cursor-not-allowed"
    );

    /// <summary>
    /// Gets the CSS classes for the preview area.
    /// </summary>
    private string PreviewCssClass => ClassNames.cn(
        "bb:flex-1 bb:w-full bb:px-3 bb:py-2 bb:overflow-auto",
        MinHeight == null ? "bb:min-h-[150px]" : null,
        "prose prose-sm dark:prose-invert bb:max-w-none",
        "bb:[&_h1]:text-2xl bb:[&_h1]:font-bold bb:[&_h1]:mb-2",
        "bb:[&_h2]:text-xl bb:[&_h2]:font-bold bb:[&_h2]:mb-2",
        "bb:[&_h3]:text-lg bb:[&_h3]:font-bold bb:[&_h3]:mb-2",
        "bb:[&_p]:mb-2 bb:[&_ul]:list-disc bb:[&_ul]:ml-4 bb:[&_ol]:list-decimal bb:[&_ol]:ml-4",
        "bb:[&_li]:mb-1 bb:[&_strong]:font-bold bb:[&_em]:italic bb:[&_u]:underline"
    );

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                _module = await JsModules.GetAsync(JSRuntime, "./_content/BlazorBlueprint.Components/js/markdown-editor.js");

                // Initialize list continuation behavior and undo/redo
                await _module.InvokeVoidAsync("initializeListContinuation", _textareaRef, _dotNetRef);
            }
            catch (JSException)
            {
                // JS module not available, continue without JS features
            }
        }
    }

    /// <summary>
    /// Called from JavaScript when content changes via undo/redo.
    /// </summary>
    [JSInvokable]
    public async Task OnContentChanged(string value)
    {
        Value = value;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(value);
        }
    }

    /// <summary>
    /// Handles input changes in the textarea.
    /// </summary>
    private async Task HandleInput(ChangeEventArgs args)
    {
        var newValue = args.Value?.ToString();
        Value = newValue;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(newValue);
        }
    }

    /// <summary>
    /// Handles keyboard shortcuts in the textarea.
    /// </summary>
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        _shouldPreventKeydown = false;

        // Handle Ctrl/Cmd shortcuts
        if (e.CtrlKey || e.MetaKey)
        {
            switch (e.Key.ToLowerInvariant())
            {
                case "b":
                    _shouldPreventKeydown = true;
                    await ApplyBold();
                    break;
                case "i":
                    _shouldPreventKeydown = true;
                    await ApplyItalic();
                    break;
                case "u":
                    _shouldPreventKeydown = true;
                    await ApplyUnderline();
                    break;
            }
        }
        // Note: Enter key for list continuation is handled by JS event listener
        // initialized in OnAfterRenderAsync
    }

    /// <summary>
    /// Applies bold formatting to selected text.
    /// </summary>
    private async Task ApplyBold()
    {
        if (_module == null || Disabled)
        {
            return;
        }

        try
        {
            var newValue = await _module.InvokeAsync<string>(
                "insertFormatting", _textareaRef, "**", "**", "bold text");
            await UpdateValue(newValue);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Applies italic formatting to selected text.
    /// </summary>
    private async Task ApplyItalic()
    {
        if (_module == null || Disabled)
        {
            return;
        }

        try
        {
            var newValue = await _module.InvokeAsync<string>(
                "insertFormatting", _textareaRef, "*", "*", "italic text");
            await UpdateValue(newValue);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Applies underline formatting to selected text.
    /// </summary>
    private async Task ApplyUnderline()
    {
        if (_module == null || Disabled)
        {
            return;
        }

        try
        {
            var newValue = await _module.InvokeAsync<string>(
                "insertFormatting", _textareaRef, "<u>", "</u>", "underlined text");
            await UpdateValue(newValue);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Applies heading formatting to current line.
    /// </summary>
    private async Task ApplyHeading(int level)
    {
        if (_module == null || Disabled)
        {
            return;
        }

        try
        {
            var prefix = level switch
            {
                1 => "# ",
                2 => "## ",
                3 => "### ",
                _ => ""
            };

            if (string.IsNullOrEmpty(prefix))
            {
                // Remove heading (normal paragraph)
                var newValue = await _module.InvokeAsync<string>(
                    "removeLinePrefix", _textareaRef);
                await UpdateValue(newValue);
            }
            else
            {
                var newValue = await _module.InvokeAsync<string>(
                    "insertLinePrefix", _textareaRef, prefix);
                await UpdateValue(newValue);
            }
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Applies bullet list formatting to current line.
    /// </summary>
    private async Task ApplyBulletList()
    {
        if (_module == null || Disabled)
        {
            return;
        }

        try
        {
            var newValue = await _module.InvokeAsync<string>(
                "insertLinePrefix", _textareaRef, "- ");
            await UpdateValue(newValue);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Applies numbered list formatting to current line.
    /// </summary>
    private async Task ApplyNumberedList()
    {
        if (_module == null || Disabled)
        {
            return;
        }

        try
        {
            var newValue = await _module.InvokeAsync<string>(
                "insertLinePrefix", _textareaRef, "1. ");
            await UpdateValue(newValue);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Updates the value and notifies the parent component.
    /// </summary>
    private async Task UpdateValue(string newValue)
    {
        Value = newValue;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(newValue);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Handles tab change.
    /// </summary>
    private void HandleTabChange(string? tab)
    {
        if (tab != null)
        {
            _activeTab = tab;
        }
    }

    /// <summary>
    /// Gets the CSS classes for a tab based on its active state (GitHub style).
    /// </summary>
    private string GetTabClass(string tabValue)
    {
        var isActive = _activeTab == tabValue;
        return ClassNames.cn(
            // Reset default TabsTrigger styles
            "bb:!shadow-none bb:!ring-0 bb:!ring-offset-0",
            // Base styles
            "bb:px-3 bb:py-1 bb:text-sm bb:font-medium bb:transition-colors bb:rounded-sm",
            // Active state - white background, inactive - transparent
            isActive
                ? "bb:bg-background bb:text-foreground bb:shadow-sm"
                : "bb:bg-transparent bb:text-muted-foreground bb:hover:text-foreground"
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            try
            {
                // Clean up the list continuation listener and editor data
                await _module.InvokeVoidAsync("disposeListContinuation", _textareaRef);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
            {
                // Circuit disconnected, ignore
            }
        }

        _dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }
}
