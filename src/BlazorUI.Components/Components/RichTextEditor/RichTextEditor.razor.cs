using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorUI.Components.RichTextEditor;

/// <summary>
/// A WYSIWYG rich text editor component with toolbar and HTML output.
/// Uses contenteditable for true rich text editing where formatting appears directly as user types.
/// </summary>
public partial class RichTextEditor : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<RichTextEditor>? _dotNetRef;
    private ElementReference _editorRef;
    private bool _shouldPreventKeydown;
    private bool _isInitialized;
    private bool _colorPickerOpen;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets or sets the HTML content value.
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
    /// Gets or sets the HTML id attribute for the contenteditable element.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the ARIA label for the editor.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets the ID of the element that describes the editor.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Gets or sets whether the editor value is invalid.
    /// </summary>
    [Parameter]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    /// Gets or sets the minimum height of the editor content area.
    /// </summary>
    [Parameter]
    public string MinHeight { get; set; } = "150px";

    /// <summary>
    /// Gets or sets the maximum height of the editor content area.
    /// When content exceeds this height, a scrollbar appears.
    /// </summary>
    [Parameter]
    public string? MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets whether to show the font size dropdown in the toolbar.
    /// </summary>
    [Parameter]
    public bool ShowFontSize { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the text formatting buttons (Bold, Italic, Underline, Strikethrough).
    /// </summary>
    [Parameter]
    public bool ShowFormatting { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the list buttons (Ordered and Unordered lists).
    /// </summary>
    [Parameter]
    public bool ShowLists { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the text alignment buttons.
    /// </summary>
    [Parameter]
    public bool ShowAlignment { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the color picker.
    /// </summary>
    [Parameter]
    public bool ShowColorPicker { get; set; } = true;

    /// <summary>
    /// Gets or sets a fixed height for the editor content area.
    /// When set, the editor will not auto-expand and will show scrollbar when content overflows.
    /// Takes precedence over MinHeight/MaxHeight when set.
    /// </summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>
    /// Color palette for the font color picker.
    /// </summary>
    private static readonly (string Name, string Value)[] ColorPalette = new[]
    {
        ("Black", "#000000"),
        ("Dark Gray", "#4B5563"),
        ("Gray", "#9CA3AF"),
        ("Light Gray", "#D1D5DB"),
        ("Red", "#EF4444"),
        ("Orange", "#F97316"),
        ("Yellow", "#EAB308"),
        ("Green", "#22C55E"),
        ("Blue", "#3B82F6"),
        ("Indigo", "#6366F1"),
        ("Purple", "#A855F7"),
        ("Pink", "#EC4899")
    };

    /// <summary>
    /// Font size options for the dropdown.
    /// </summary>
    private static readonly (string Name, string Size)[] FontSizes = new[]
    {
        ("Small", "12px"),
        ("Normal", "14px"),
        ("Medium", "18px"),
        ("Large", "24px"),
        ("Extra Large", "32px")
    };

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
    /// Gets the CSS classes for the toolbar.
    /// </summary>
    private string ToolbarCssClass => ClassNames.cn(
        "flex flex-wrap items-center gap-1 px-3 py-2 bg-muted/40"
    );

    /// <summary>
    /// Gets the CSS classes for the contenteditable area.
    /// </summary>
    private string ContentCssClass => ClassNames.cn(
        "w-full px-3 py-2 text-sm",
        // Only use flex-1 when not using fixed height
        ClassNames.when(string.IsNullOrEmpty(Height), "flex-1"),
        "focus:outline-none",
        "overflow-auto",
        "[&:empty]:before:content-[attr(data-placeholder)]",
        "[&:empty]:before:text-muted-foreground",
        "[&:empty]:before:pointer-events-none",
        // Prose-like styling for content
        "[&_ul]:list-disc [&_ul]:ml-4 [&_ol]:list-decimal [&_ol]:ml-4",
        "[&_li]:mb-1",
        ClassNames.when(Disabled, "cursor-not-allowed pointer-events-none")
    );

    /// <summary>
    /// Gets the CSS classes for color swatches.
    /// </summary>
    private string ColorSwatchCssClass => ClassNames.cn(
        "w-6 h-6 rounded border border-input cursor-pointer",
        "hover:scale-110 transition-transform",
        "focus:outline-none focus:ring-2 focus:ring-ring"
    );

    /// <summary>
    /// Gets the inline style for the content area based on height properties.
    /// </summary>
    private string ContentStyle
    {
        get
        {
            if (!string.IsNullOrEmpty(Height))
            {
                // Fixed height mode - no auto-expand
                return $"height: {Height}";
            }

            var styles = new List<string>();
            styles.Add($"min-height: {MinHeight}");

            if (!string.IsNullOrEmpty(MaxHeight))
            {
                styles.Add($"max-height: {MaxHeight}");
            }

            return string.Join("; ", styles);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorUI.Components/js/rich-text-editor.js");

                await _module.InvokeVoidAsync("initialize", _editorRef, _dotNetRef);

                // Set initial content if provided
                if (!string.IsNullOrEmpty(Value))
                {
                    await _module.InvokeVoidAsync("setContent", _editorRef, Value);
                }

                _isInitialized = true;
            }
            catch (JSException)
            {
                // JS module not available, continue without JS features
            }
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Sync external Value changes to the editor
        if (_isInitialized && _module != null)
        {
            try
            {
                var currentContent = await _module.InvokeAsync<string>("getContent", _editorRef);
                if (currentContent != Value)
                {
                    await _module.InvokeVoidAsync("setContent", _editorRef, Value ?? "");
                }
            }
            catch (JSException)
            {
                // Ignore JS errors
            }
        }
    }

    /// <summary>
    /// Called from JavaScript when the content changes.
    /// </summary>
    [JSInvokable]
    public async Task OnContentChanged(string html)
    {
        Value = html;

        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(html);
        }
    }

    /// <summary>
    /// Handles keyboard shortcuts.
    /// Note: Ctrl+Z/Y for undo/redo are handled in JavaScript to prevent browser's native undo race condition.
    /// </summary>
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        _shouldPreventKeydown = false;

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
                // Note: z/y (undo/redo) are handled in JavaScript (rich-text-editor.js)
                // to prevent race condition with browser's native undo
            }
        }
    }

    /// <summary>
    /// Applies bold formatting to selected text.
    /// </summary>
    private async Task ApplyBold()
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("toggleFormat", _editorRef, "strong");
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
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("toggleFormat", _editorRef, "em");
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
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("toggleFormat", _editorRef, "u");
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Applies strikethrough formatting to selected text.
    /// </summary>
    private async Task ApplyStrikethrough()
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("toggleFormat", _editorRef, "s");
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Inserts an unordered (bullet) list.
    /// </summary>
    private async Task InsertUnorderedList()
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("insertList", _editorRef, false);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Inserts an ordered (numbered) list.
    /// </summary>
    private async Task InsertOrderedList()
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("insertList", _editorRef, true);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Sets the font size of selected text.
    /// </summary>
    private async Task SetFontSize(string size)
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("setFontSize", _editorRef, size);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Sets the font color of selected text.
    /// </summary>
    private async Task SetFontColor(string color)
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("setFontColor", _editorRef, color);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Selects a font color and closes the color picker dropdown.
    /// </summary>
    private async Task SelectFontColor(string color)
    {
        _colorPickerOpen = false;
        await SetFontColor(color);
    }

    /// <summary>
    /// Sets the text alignment.
    /// </summary>
    private async Task SetAlignment(string alignment)
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("setAlignment", _editorRef, alignment);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Undoes the last action.
    /// </summary>
    private async Task Undo()
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("undo", _editorRef);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Redoes the last undone action.
    /// </summary>
    private async Task Redo()
    {
        if (_module == null || Disabled) return;

        try
        {
            await _module.InvokeVoidAsync("redo", _editorRef);
        }
        catch (JSException)
        {
            // Ignore JS errors
        }
    }

    /// <summary>
    /// Focuses the editor.
    /// </summary>
    public async Task FocusAsync()
    {
        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("focus", _editorRef);
            }
            catch (JSException)
            {
                // Ignore JS errors
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("dispose", _editorRef);
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
