using BlazorUI.Components.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorUI.Components.RichTextEditor;

/// <summary>
/// A rich text editor component built on Quill.js that follows the shadcn/ui design system.
/// </summary>
public partial class RichTextEditor : ComponentBase, IAsyncDisposable
{
    // === Private Fields ===
    private ElementReference _editorRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<RichTextEditor>? _dotNetRef;
    private string _editorId = Guid.NewGuid().ToString("N");
    private bool _jsInitialized;
    private string? _lastKnownValue;
    private bool _pendingValueUpdate;

    // === Parameters - Value Binding ===

    /// <summary>
    /// Gets or sets the HTML content of the editor.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the editor content changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the Delta (JSON) representation of the editor content.
    /// </summary>
    [Parameter]
    public string? DeltaValue { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the Delta content changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> DeltaValueChanged { get; set; }

    // === Parameters - Toolbar ===

    /// <summary>
    /// Gets or sets the toolbar preset configuration.
    /// </summary>
    [Parameter]
    public ToolbarPreset Toolbar { get; set; } = ToolbarPreset.Standard;

    /// <summary>
    /// Gets or sets custom toolbar content.
    /// </summary>
    [Parameter]
    public RenderFragment? ToolbarContent { get; set; }

    // === Parameters - Appearance ===

    /// <summary>
    /// Gets or sets the placeholder text displayed when the editor is empty.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Gets or sets the minimum height of the editor.
    /// </summary>
    [Parameter]
    public string MinHeight { get; set; } = "150px";

    /// <summary>
    /// Gets or sets the maximum height of the editor. Content will scroll when exceeded.
    /// </summary>
    [Parameter]
    public string? MaxHeight { get; set; }

    /// <summary>
    /// Gets or sets a fixed height for the editor.
    /// </summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the HTML id attribute for the editor container.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    // === Parameters - State ===

    /// <summary>
    /// Gets or sets whether the editor is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets whether the editor is read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    // === Parameters - Accessibility ===

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

    // === Parameters - Events ===

    /// <summary>
    /// Gets or sets the callback invoked when the editor content changes.
    /// </summary>
    [Parameter]
    public EventCallback<TextChangeEventArgs> OnTextChange { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<SelectionChangeEventArgs> OnSelectionChange { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the editor gains focus.
    /// </summary>
    [Parameter]
    public EventCallback OnFocus { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the editor loses focus.
    /// </summary>
    [Parameter]
    public EventCallback OnBlur { get; set; }

    // === Lifecycle Methods ===

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeJsAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // If Value changed externally, update the editor
        if (_jsInitialized && Value != _lastKnownValue && !_pendingValueUpdate)
        {
            _pendingValueUpdate = true;
            await SetHtmlAsync(Value);
            _lastKnownValue = Value;
            _pendingValueUpdate = false;
        }
    }

    private async Task InitializeJsAsync()
    {
        if (_jsInitialized) return;

        try
        {
            _jsModule = await JS.InvokeAsync<IJSObjectReference>("import",
                "./_content/BlazorUI.Components/js/quill-interop.js");
            _dotNetRef = DotNetObjectReference.Create(this);

            var options = BuildEditorOptions();
            await _jsModule.InvokeVoidAsync("initializeEditor",
                _editorRef, _dotNetRef, _editorId, options);
            _jsInitialized = true;

            // Set initial content
            if (!string.IsNullOrEmpty(Value))
            {
                await _jsModule.InvokeVoidAsync("setHtml", _editorId, Value);
                _lastKnownValue = Value;
            }

            // Apply disabled state
            if (Disabled)
            {
                await _jsModule.InvokeVoidAsync("disable", _editorId);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to initialize RichTextEditor JS: {ex.Message}");
        }
    }

    // === JSInvokable Callbacks ===

    [JSInvokable]
    public async Task OnTextChangeCallback(TextChangeEventArgs args)
    {
        _lastKnownValue = args.Html;
        Value = args.Html;
        await ValueChanged.InvokeAsync(args.Html);

        DeltaValue = args.Delta;
        await DeltaValueChanged.InvokeAsync(args.Delta);

        await OnTextChange.InvokeAsync(args);
    }

    [JSInvokable]
    public async Task OnSelectionChangeCallback(SelectionChangeEventArgs args)
    {
        // Detect focus/blur from selection (null range = lost focus)
        if (args.Range == null && args.OldRange != null)
        {
            await OnBlur.InvokeAsync();
        }
        else if (args.Range != null && args.OldRange == null)
        {
            await OnFocus.InvokeAsync();
        }

        await OnSelectionChange.InvokeAsync(args);
    }

    // === Public API Methods ===

    /// <summary>
    /// Focuses the editor.
    /// </summary>
    public async Task FocusAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            await _jsModule.InvokeVoidAsync("focus", _editorId);
        }
    }

    /// <summary>
    /// Removes focus from the editor.
    /// </summary>
    public async Task BlurAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            await _jsModule.InvokeVoidAsync("blur", _editorId);
        }
    }

    /// <summary>
    /// Gets the current selection range.
    /// </summary>
    public async Task<EditorRange?> GetSelectionAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            return await _jsModule.InvokeAsync<EditorRange?>("getSelection", _editorId);
        }
        return null;
    }

    /// <summary>
    /// Sets the selection range.
    /// </summary>
    public async Task SetSelectionAsync(int index, int length = 0)
    {
        if (_jsModule != null && _jsInitialized)
        {
            await _jsModule.InvokeVoidAsync("setSelection", _editorId, index, length);
        }
    }

    /// <summary>
    /// Applies formatting to the current selection.
    /// </summary>
    public async Task FormatAsync(string formatName, object? value = null)
    {
        if (_jsModule != null && _jsInitialized)
        {
            await _jsModule.InvokeVoidAsync("format", _editorId, formatName, value ?? true);
        }
    }

    /// <summary>
    /// Gets the plain text content of the editor.
    /// </summary>
    public async Task<string> GetTextAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            return await _jsModule.InvokeAsync<string>("getText", _editorId) ?? "";
        }
        return "";
    }

    /// <summary>
    /// Gets the length of the editor content.
    /// </summary>
    public async Task<int> GetLengthAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            return await _jsModule.InvokeAsync<int>("getLength", _editorId);
        }
        return 0;
    }

    /// <summary>
    /// Gets the HTML content of the editor.
    /// </summary>
    public async Task<string> GetHtmlAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            return await _jsModule.InvokeAsync<string>("getHtml", _editorId) ?? "";
        }
        return "";
    }

    /// <summary>
    /// Sets the HTML content of the editor.
    /// </summary>
    public async Task SetHtmlAsync(string? html)
    {
        if (_jsModule != null && _jsInitialized)
        {
            await _jsModule.InvokeVoidAsync("setHtml", _editorId, html ?? "");
        }
    }

    // === Private Helper Methods ===

    private object BuildEditorOptions() => new
    {
        placeholder = Placeholder ?? "",
        readOnly = Disabled || ReadOnly,
        toolbar = GetToolbarConfig()
    };

    private object? GetToolbarConfig() => Toolbar switch
    {
        ToolbarPreset.None => false,
        ToolbarPreset.Simple => new object[]
        {
            new object[] { "bold", "italic", "underline" },
            new object[] { new { list = "ordered" }, new { list = "bullet" } }
        },
        ToolbarPreset.Standard => new object[]
        {
            new object[] { new { header = new object[] { 1, 2, 3, false } } },
            new object[] { "bold", "italic", "underline", "strike" },
            new object[] { new { list = "ordered" }, new { list = "bullet" } },
            new object[] { "link" },
            new object[] { "clean" }
        },
        ToolbarPreset.Full => new object[]
        {
            new object[] { new { header = new object[] { 1, 2, 3, 4, 5, 6, false } } },
            new object[] { "bold", "italic", "underline", "strike" },
            new object[] { new { color = Array.Empty<string>() }, new { background = Array.Empty<string>() } },
            new object[] { new { align = Array.Empty<string>() } },
            new object[] { new { list = "ordered" }, new { list = "bullet" }, new { indent = "-1" }, new { indent = "+1" } },
            new object[] { "blockquote", "code-block" },
            new object[] { "link", "image" },
            new object[] { "clean" }
        },
        ToolbarPreset.Custom => null, // Use ToolbarContent instead
        _ => null
    };

    // === CSS Classes ===

    private string ContainerCssClass => ClassNames.cn(
        "flex flex-col rounded-md border border-input bg-background",
        "focus-within:border-ring focus-within:ring-[3px] focus-within:ring-ring/50",
        ClassNames.when(AriaInvalid == true, "border-destructive ring-destructive/20"),
        ClassNames.when(Disabled, "opacity-50 cursor-not-allowed"),
        Class
    );

    private string ToolbarCssClass => ClassNames.cn(
        "flex flex-wrap items-center gap-1 px-3 py-2 border-b border-input bg-muted/40"
    );

    private string EditorCssClass => ClassNames.cn(
        "prose prose-sm dark:prose-invert max-w-none",
        "text-base md:text-sm",
        ClassNames.when(Disabled, "cursor-not-allowed")
    );

    private string EditorStyle
    {
        get
        {
            var styles = new List<string>();

            if (!string.IsNullOrEmpty(Height))
            {
                styles.Add($"height: {Height}");
                styles.Add("overflow-y: auto");
            }
            else
            {
                styles.Add($"min-height: {MinHeight}");
                if (!string.IsNullOrEmpty(MaxHeight))
                {
                    styles.Add($"max-height: {MaxHeight}");
                    styles.Add("overflow-y: auto");
                }
            }

            return string.Join("; ", styles);
        }
    }

    // === Dispose ===

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("disposeEditor", _editorId);
                await _jsModule.DisposeAsync();
            }
            catch { }
        }
        _dotNetRef?.Dispose();
    }
}
