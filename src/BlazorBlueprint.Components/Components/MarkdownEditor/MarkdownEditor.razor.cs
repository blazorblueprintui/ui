using BlazorBlueprint.Components.Utilities;
using Ganss.Xss;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components.MarkdownEditor;

/// <summary>
/// A markdown editor component with toolbar and preview functionality.
/// Inspired by GitHub's comment editor design.
/// </summary>
public partial class MarkdownEditor : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<MarkdownEditor>? _dotNetRef;
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
        "flex flex-col rounded-md border border-input bg-background",
        "focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2",
        ClassNames.when(AriaInvalid == true, "border-destructive ring-destructive/20"),
        ClassNames.when(Disabled, "opacity-50 cursor-not-allowed"),
        Class
    );

    /// <summary>
    /// Gets the CSS classes for the textarea.
    /// </summary>
    private static string TextareaCssClass => ClassNames.cn(
        "flex-1 min-h-[150px] w-full resize-y border-0 bg-transparent",
        "px-3 py-2 text-sm placeholder:text-muted-foreground",
        "focus:outline-none",
        "disabled:cursor-not-allowed"
    );

    /// <summary>
    /// Gets the CSS classes for the preview area.
    /// </summary>
    private static string PreviewCssClass => ClassNames.cn(
        "flex-1 min-h-[150px] w-full px-3 py-2 overflow-auto",
        "prose prose-sm dark:prose-invert max-w-none",
        "[&_h1]:text-2xl [&_h1]:font-bold [&_h1]:mb-2",
        "[&_h2]:text-xl [&_h2]:font-bold [&_h2]:mb-2",
        "[&_h3]:text-lg [&_h3]:font-bold [&_h3]:mb-2",
        "[&_p]:mb-2 [&_ul]:list-disc [&_ul]:ml-4 [&_ol]:list-decimal [&_ol]:ml-4",
        "[&_li]:mb-1 [&_strong]:font-bold [&_em]:italic [&_u]:underline"
    );

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/markdown-editor.js");

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
            "!shadow-none !ring-0 !ring-offset-0",
            // Base styles
            "px-3 py-1 text-sm font-medium transition-colors rounded-sm",
            // Active state - white background, inactive - transparent
            isActive
                ? "bg-background text-foreground shadow-sm"
                : "bg-transparent text-muted-foreground hover:text-foreground"
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
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
            catch (JSException)
            {
                // JS error during cleanup, ignore
            }
        }

        _dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }
}
