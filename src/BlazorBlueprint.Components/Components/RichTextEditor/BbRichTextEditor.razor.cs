using System.Text.Json;
using Ganss.Xss;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Components;

/// <summary>
/// A rich text editor component built on Quill.js that follows the shadcn/ui design system.
/// </summary>
public partial class BbRichTextEditor : ComponentBase, IAsyncDisposable
{
    // === Private Fields ===
    private ElementReference _editorRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<BbRichTextEditor>? _dotNetRef;
    private string _editorId = Guid.NewGuid().ToString("N");
    private bool _jsInitialized;
    private string? _lastKnownValue;
    private bool _pendingValueUpdate;

    // === Format State Tracking ===
    private bool _isBold;
    private bool _isItalic;
    private bool _isUnderline;
    private bool _isStrike;
    private bool _isBulletList;
    private bool _isOrderedList;
    private bool _isBlockquote;
    private bool _isCodeBlock;
    private bool _isCode;
    private bool _isCheckList;
    private string _headerLevel = "";
    private string _align = "";
    private bool _canUndo;
    private bool _canRedo;

    // === Colour Popovers ===
    private bool _textColorOpen;
    private bool _highlightOpen;

    /// <summary>Swatches offered for text colour: greys first, then one saturated tone per hue.</summary>
    private static readonly string[] TextColors =
    [
        "#000000", "#404040", "#737373", "#a3a3a3", "#d4d4d4", "#ffffff", "#78350f",
        "#dc2626", "#ea580c", "#ca8a04", "#16a34a", "#0891b2", "#2563eb", "#7c3aed",
    ];

    /// <summary>Swatches offered for highlight: light tints that keep text readable.</summary>
    private static readonly string[] HighlightColors =
    [
        "#fef08a", "#fde68a", "#fed7aa", "#fecaca", "#fbcfe8", "#ddd6fe", "#c7d2fe",
        "#bfdbfe", "#a5f3fc", "#99f6e4", "#bbf7d0", "#d9f99d", "#e5e5e5", "#f5f5f4",
    ];

    // === Table State ===
    private const int TablePickerSize = 6;
    private bool _isInTable;
    private bool _tableMenuOpen;
    private int _tablePickerRows;
    private int _tablePickerColumns;

    // === Link Dialog State ===
    private bool _linkDialogOpen;
    private string _linkUrl = "";
    private string? _linkUrlError;
    private bool _hasExistingLink;
    private EditorRange? _savedSelection;

    // === ShouldRender Tracking ===
    private bool _parametersChanged;
    private string? _lastValue;
    private bool _lastDisabled;
    private bool _lastReadOnly;
    private bool _lastLinkDialogOpen;
    private bool _lastTableMenuOpen;
    private bool _lastHasImageUploader;
    private bool _formatStateChanged;

    /// <summary>
    /// HTML sanitizer for XSS prevention. Thread-safe for static usage.
    /// </summary>
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        // Quill marks checklist items with data-list="checked|unchecked"; without it a bound
        // Value comes back as plain bullets.
        sanitizer.AllowedAttributes.Add("data-list");

        // Images inserted without an ImageUploader are data URLs, which the default rules drop.
        // Only image data on <img src> is let through; data: stays blocked everywhere else,
        // because on an <a href> it can carry script.
        sanitizer.FilterUrl += (_, e) =>
        {
            if (e.SanitizedUrl == null
                && string.Equals(e.Tag.TagName, "IMG", StringComparison.OrdinalIgnoreCase)
                && e.OriginalUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                e.SanitizedUrl = e.OriginalUrl;
            }
        };

        return sanitizer;
    }

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

    // === Parameters - Images ===

    /// <summary>
    /// Receives every image the user adds — from the toolbar button, a drop or a paste — and
    /// returns the URL to embed, or <c>null</c> to reject the file. Store the image wherever
    /// you keep uploads and return its address. Without a handler, images are embedded as
    /// data URLs inside the HTML, which is what Quill does on its own; that works for small
    /// pictures but bloats the value and is not what you want in a database.
    /// </summary>
    [Parameter]
    public Func<EditorImageUpload, Task<string?>>? ImageUploader { get; set; }

    /// <summary>
    /// Gets or sets the largest image, in bytes, that is streamed to <see cref="ImageUploader"/>.
    /// Larger files are rejected before any data moves. Defaults to 10 MB.
    /// </summary>
    [Parameter]
    public long MaxImageSize { get; set; } = 10 * 1024 * 1024;

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
    /// Gets or sets additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

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
        _parametersChanged = true;

        var hasImageUploader = ImageUploader != null;
        if (_jsInitialized && _jsModule != null && hasImageUploader != _lastHasImageUploader)
        {
            _lastHasImageUploader = hasImageUploader;
            await _jsModule.InvokeVoidAsync("setImageUploader", _editorId, hasImageUploader);
        }

        // If Value changed externally, update the editor
        if (_jsInitialized && Value != _lastKnownValue && !_pendingValueUpdate)
        {
            _pendingValueUpdate = true;
            try
            {
                await SetHtmlAsync(Value);
                _lastKnownValue = Value;
            }
            finally
            {
                _pendingValueUpdate = false;
            }
        }
    }

    private async Task InitializeJsAsync()
    {
        if (_jsInitialized)
        {
            return;
        }

        try
        {
            _jsModule = await JsModules.GetAsync(JS, "./_content/BlazorBlueprint.Components/js/quill-interop.js");
            _dotNetRef = DotNetObjectReference.Create(this);

            var options = BuildEditorOptions();
            _lastHasImageUploader = ImageUploader != null;
            await _jsModule.InvokeVoidAsync("initializeEditor",
                _editorRef, _dotNetRef, _editorId, options);
            _jsInitialized = true;

            // Set initial content (sanitized to prevent XSS)
            if (!string.IsNullOrEmpty(Value))
            {
                var sanitized = Sanitizer.Sanitize(Value);
                await _jsModule.InvokeVoidAsync("setHtml", _editorId, sanitized);
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
        if (_canUndo != args.CanUndo || _canRedo != args.CanRedo)
        {
            _canUndo = args.CanUndo;
            _canRedo = args.CanRedo;
            _formatStateChanged = true;
            StateHasChanged();
        }

        _lastKnownValue = args.Html;
        Value = args.Html;
        await ValueChanged.InvokeAsync(args.Html);

        DeltaValue = args.Delta;
        await DeltaValueChanged.InvokeAsync(args.Delta);

        await OnTextChange.InvokeAsync(args);
    }

    /// <summary>
    /// Called from JavaScript with each image the user picked, dropped or pasted, when an
    /// <see cref="ImageUploader"/> is set. Returns the URL to embed, or <c>null</c> to skip.
    /// </summary>
    [JSInvokable]
    public async Task<string?> OnImageUploadCallback(IJSStreamReference stream, string fileName, string contentType, long size)
    {
        await using (stream)
        {
            if (ImageUploader == null || size > MaxImageSize)
            {
                return null;
            }

            await using var content = await stream.OpenReadStreamAsync(MaxImageSize);
            return await ImageUploader(new EditorImageUpload(fileName, contentType, size, content));
        }
    }

    [JSInvokable]
    public async Task OnSelectionChangeCallback(SelectionChangeEventArgs args)
    {
        // Update format state from selection
        if (args.Format != null)
        {
            UpdateFormatState(args.Format);
        }

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

    private static bool GetFormatBool(Dictionary<string, object?> format, string key)
    {
        if (!format.TryGetValue(key, out var value) || value == null)
        {
            return false;
        }

        if (value is bool b)
        {
            return b;
        }

        if (value is JsonElement je && je.ValueKind == JsonValueKind.True)
        {
            return true;
        }

        return false;
    }

    private static string GetFormatString(Dictionary<string, object?> format, string key)
    {
        if (!format.TryGetValue(key, out var value) || value == null)
        {
            return "";
        }

        if (value is string s)
        {
            return s;
        }

        if (value is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String)
            {
                return je.GetString() ?? "";
            }
            if (je.ValueKind == JsonValueKind.Number)
            {
                return je.GetInt32().ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        return value.ToString() ?? "";
    }

    // === Toolbar Actions ===

    private async Task ToggleFormatAsync(string format, object? value = null)
    {
        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        // Toggle: if already active, remove; otherwise apply
        var isActive = format switch
        {
            "bold" => _isBold,
            "italic" => _isItalic,
            "underline" => _isUnderline,
            "strike" => _isStrike,
            "code" => _isCode,
            "blockquote" => _isBlockquote,
            "code-block" => _isCodeBlock,
            "list" when value?.ToString() == "bullet" => _isBulletList,
            "list" when value?.ToString() == "ordered" => _isOrderedList,
            "list" when value?.ToString() == "check" => _isCheckList,
            _ => false
        };

        // A checklist is Quill's list format with the value "unchecked" (or "checked");
        // "check" is the toolbar's name for it.
        var applied = value?.ToString() == "check" ? "unchecked" : value;
        var newValue = isActive ? false : (applied ?? true);

        // Use formatAndGetState for all formats to ensure immediate state sync
        var formatState = await _jsModule.InvokeAsync<Dictionary<string, object?>>(
            "formatAndGetState", _editorId, format, newValue);
        UpdateFormatState(formatState);

        // Refocus the editor after toolbar button click
        await _jsModule.InvokeVoidAsync("focus", _editorId);
    }

    private void UpdateFormatState(Dictionary<string, object?> format)
    {
        if (format == null)
        {
            return;
        }

        _isBold = GetFormatBool(format, "bold");
        _isItalic = GetFormatBool(format, "italic");
        _isUnderline = GetFormatBool(format, "underline");
        _isStrike = GetFormatBool(format, "strike");
        _isBlockquote = GetFormatBool(format, "blockquote");
        _isCodeBlock = GetFormatBool(format, "code-block");
        _isCode = GetFormatBool(format, "code");

        var listValue = GetFormatString(format, "list");
        _isBulletList = listValue == "bullet";
        _isOrderedList = listValue == "ordered";
        _isCheckList = listValue is "checked" or "unchecked";

        _headerLevel = GetFormatString(format, "header");
        _align = GetFormatString(format, "align");

        // Quill reports the row id of the cell that holds the caret; any value means "in a table".
        _isInTable = !string.IsNullOrEmpty(GetFormatString(format, "table"));

        // Mark format state as changed for ShouldRender optimization
        _formatStateChanged = true;
        StateHasChanged();
    }

    /// <summary>
    /// Determines whether the component should re-render based on tracked state changes.
    /// This optimization reduces unnecessary render cycles from bidirectional binding.
    /// </summary>
    protected override bool ShouldRender()
    {
        if (_parametersChanged)
        {
            _parametersChanged = false;
            _lastValue = Value;
            _lastDisabled = Disabled;
            _lastReadOnly = ReadOnly;
            _lastLinkDialogOpen = _linkDialogOpen;
            _lastTableMenuOpen = _tableMenuOpen;
            _formatStateChanged = false;
            return true;
        }

        var valueChanged = _lastValue != Value;
        var disabledChanged = _lastDisabled != Disabled;
        var readOnlyChanged = _lastReadOnly != ReadOnly;
        var dialogChanged = _lastLinkDialogOpen != _linkDialogOpen;
        var tableMenuChanged = _lastTableMenuOpen != _tableMenuOpen;

        if (valueChanged || disabledChanged || readOnlyChanged || dialogChanged || tableMenuChanged || _formatStateChanged)
        {
            _lastValue = Value;
            _lastDisabled = Disabled;
            _lastReadOnly = ReadOnly;
            _lastLinkDialogOpen = _linkDialogOpen;
            _lastTableMenuOpen = _tableMenuOpen;
            _formatStateChanged = false;
            return true;
        }

        return false;
    }

    private async Task HandleHeaderChangeAsync(string? value)
    {
        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        _headerLevel = value ?? "";

        if (string.IsNullOrEmpty(value))
        {
            await _jsModule.InvokeVoidAsync("format", _editorId, "header", false);
        }
        else
        {
            await _jsModule.InvokeVoidAsync("format", _editorId, "header", int.Parse(value, System.Globalization.CultureInfo.InvariantCulture));
        }

        // Refocus the editor after dropdown change
        await _jsModule.InvokeVoidAsync("focus", _editorId);
    }

    private async Task InsertLinkAsync()
    {
        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        // Save the current selection before opening dialog
        _savedSelection = await GetSelectionAsync();

        // Check if there's already a link at the selection
        var format = await _jsModule.InvokeAsync<Dictionary<string, object?>>("getFormat", _editorId);
        object? linkValue = null;
        _hasExistingLink = format != null && format.TryGetValue("link", out linkValue) && linkValue != null;

        if (_hasExistingLink)
        {
            if (linkValue is string existingUrl)
            {
                _linkUrl = existingUrl;
            }
            else if (linkValue is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                _linkUrl = je.GetString() ?? "https://";
            }
            else
            {
                _linkUrl = "https://";
            }
        }
        else
        {
            _linkUrl = "https://";
        }

        _linkUrlError = null;
        _linkDialogOpen = true;
    }

    private void CloseLinkDialog()
    {
        _linkDialogOpen = false;
        _linkUrl = "";
        _linkUrlError = null;
        _savedSelection = null;
    }

    private void ValidateLinkUrl()
    {
        if (string.IsNullOrWhiteSpace(_linkUrl) || _linkUrl == "https://")
        {
            _linkUrlError = null;
        }
        else if (!IsValidUrl(_linkUrl))
        {
            _linkUrlError = "Please enter a valid URL";
        }
        else
        {
            _linkUrlError = null;
        }
    }

    private static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url == "https://")
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private async Task ApplyLinkAsync()
    {
        if (_jsModule == null || !_jsInitialized || !IsValidUrl(_linkUrl))
        {
            return;
        }

        // Restore selection before applying link
        if (_savedSelection != null)
        {
            await SetSelectionAsync(_savedSelection.Index, _savedSelection.Length);
        }

        await _jsModule.InvokeVoidAsync("format", _editorId, "link", _linkUrl);

        CloseLinkDialog();
        await _jsModule.InvokeVoidAsync("focus", _editorId);
    }

    private async Task RemoveLinkAsync()
    {
        if (_jsModule == null || !_jsInitialized)
        {
            return;
        }

        // Restore selection before removing link
        if (_savedSelection != null)
        {
            await SetSelectionAsync(_savedSelection.Index, _savedSelection.Length);
        }

        await _jsModule.InvokeVoidAsync("format", _editorId, "link", false);

        CloseLinkDialog();
        await _jsModule.InvokeVoidAsync("focus", _editorId);
    }

    // === Alignment, Colour, Images, History ===

    private string AlignIcon => _align switch
    {
        "center" => "align-center",
        "right" => "align-right",
        "justify" => "align-justify",
        _ => "align-left"
    };

    private async Task SetAlignAsync(string? value)
    {
        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        var state = await _jsModule.InvokeAsync<Dictionary<string, object?>>(
            "formatAndGetState", _editorId, "align", value is null ? false : value);
        UpdateFormatState(state);
        await _jsModule.InvokeVoidAsync("focus", _editorId);
    }

    private void HandleTextColorOpenChanged(bool open)
    {
        _textColorOpen = open;
        _formatStateChanged = true;
    }

    private void HandleHighlightOpenChanged(bool open)
    {
        _highlightOpen = open;
        _formatStateChanged = true;
    }

    private async Task SetColorAsync(string format, string? value)
    {
        _textColorOpen = false;
        _highlightOpen = false;
        _formatStateChanged = true;

        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        var state = await _jsModule.InvokeAsync<Dictionary<string, object?>>(
            "formatAndGetState", _editorId, format, value is null ? false : value);
        UpdateFormatState(state);
        await _jsModule.InvokeVoidAsync("focus", _editorId);
    }

    private async Task PickImageAsync()
    {
        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        await _jsModule.InvokeVoidAsync("pickImage", _editorId);
    }

    private sealed class HistoryState
    {
        public bool CanUndo { get; set; }
        public bool CanRedo { get; set; }
    }

    private async Task HistoryAsync(string action)
    {
        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        var state = await _jsModule.InvokeAsync<HistoryState>(action, _editorId);
        _canUndo = state.CanUndo;
        _canRedo = state.CanRedo;
        _formatStateChanged = true;
        StateHasChanged();
    }

    // === Table Toolbar ===

    private void HandleTableMenuOpenChange(bool open)
    {
        if (!open)
        {
            SetTablePickerHover(0, 0);
        }
    }

    private void SetTablePickerHover(int rows, int columns)
    {
        if (_tablePickerRows == rows && _tablePickerColumns == columns)
        {
            return;
        }

        _tablePickerRows = rows;
        _tablePickerColumns = columns;
        _formatStateChanged = true;
    }

    private string TablePickerLabel => _tablePickerRows == 0
        ? Localizer["RichTextEditor.InsertTable"]
        : Localizer["RichTextEditor.TableSize", _tablePickerRows, _tablePickerColumns];

    private string TablePickerCellClass(int row, int column) => ClassNames.cn(
        "bb:h-5 bb:w-5 bb:rounded-sm bb:border bb:transition-colors bb:focus-visible:outline-none bb:focus-visible:ring-2 bb:focus-visible:ring-ring",
        row <= _tablePickerRows && column <= _tablePickerColumns
            ? "bb:border-primary bb:bg-primary/20"
            : "bb:border-input bb:bg-background"
    );

    private async Task InsertTableFromPickerAsync(int rows, int columns)
    {
        _tableMenuOpen = false;
        SetTablePickerHover(0, 0);
        await InsertTableAsync(rows, columns);
    }

    private async Task RunTableActionAsync(string action)
    {
        _tableMenuOpen = false;
        await TableActionAsync(action);
    }

    private async Task TableActionAsync(string action)
    {
        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        var state = await _jsModule.InvokeAsync<Dictionary<string, object?>>("tableAction", _editorId, action);
        UpdateFormatState(state);
    }

    // === Public API Methods ===

    /// <summary>Undoes the last user change. Programmatic <see cref="Value"/> updates are not undoable.</summary>
    public Task UndoAsync() => HistoryAsync("undo");

    /// <summary>Redoes the last undone change.</summary>
    public Task RedoAsync() => HistoryAsync("redo");

    /// <summary>
    /// Inserts an image by URL at the caret. Use this after storing an image yourself; images
    /// the user adds through the toolbar, a drop or a paste go through <see cref="ImageUploader"/>.
    /// </summary>
    public async Task InsertImageAsync(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (_jsModule != null && _jsInitialized && !Disabled)
        {
            await _jsModule.InvokeVoidAsync("insertImage", _editorId, url);
        }
    }

    /// <summary>
    /// Inserts a table with the given number of rows and columns at the caret.
    /// Uses Quill's built-in table module; cells are plain <c>&lt;td&gt;</c> elements and
    /// the HTML output contains a regular <c>&lt;table&gt;</c>.
    /// </summary>
    /// <param name="rows">Number of rows, at least 1.</param>
    /// <param name="columns">Number of columns, at least 1.</param>
    public async Task InsertTableAsync(int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);

        if (_jsModule == null || !_jsInitialized || Disabled)
        {
            return;
        }

        var state = await _jsModule.InvokeAsync<Dictionary<string, object?>>("insertTable", _editorId, rows, columns);
        UpdateFormatState(state);
    }

    /// <summary>Inserts a row above the one that holds the caret. No-op outside a table.</summary>
    public Task InsertRowAboveAsync() => TableActionAsync("insertRowAbove");

    /// <summary>Inserts a row below the one that holds the caret. No-op outside a table.</summary>
    public Task InsertRowBelowAsync() => TableActionAsync("insertRowBelow");

    /// <summary>Inserts a column to the left of the one that holds the caret. No-op outside a table.</summary>
    public Task InsertColumnLeftAsync() => TableActionAsync("insertColumnLeft");

    /// <summary>Inserts a column to the right of the one that holds the caret. No-op outside a table.</summary>
    public Task InsertColumnRightAsync() => TableActionAsync("insertColumnRight");

    /// <summary>Deletes the row that holds the caret. No-op outside a table.</summary>
    public Task DeleteRowAsync() => TableActionAsync("deleteRow");

    /// <summary>Deletes the column that holds the caret. No-op outside a table.</summary>
    public Task DeleteColumnAsync() => TableActionAsync("deleteColumn");

    /// <summary>Deletes the table that holds the caret. No-op outside a table.</summary>
    public Task DeleteTableAsync() => TableActionAsync("deleteTable");

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
    /// HTML is sanitized to prevent XSS attacks.
    /// </summary>
    public async Task SetHtmlAsync(string? html)
    {
        if (_jsModule != null && _jsInitialized)
        {
            var sanitized = string.IsNullOrEmpty(html) ? "" : Sanitizer.Sanitize(html);
            await _jsModule.InvokeVoidAsync("setHtml", _editorId, sanitized);
        }
    }

    /// <summary>
    /// Gets the Delta (JSON) content of the editor.
    /// Delta is Quill's native document format that preserves all formatting information.
    /// </summary>
    public async Task<string> GetDeltaAsync()
    {
        if (_jsModule != null && _jsInitialized)
        {
            return await _jsModule.InvokeAsync<string>("getContents", _editorId) ?? "{}";
        }
        return "{}";
    }

    /// <summary>
    /// Sets the editor content using a Delta (JSON) object.
    /// This is the preferred method when working with Quill's native format.
    /// </summary>
    public async Task SetDeltaAsync(string? deltaJson)
    {
        if (_jsModule != null && _jsInitialized)
        {
            if (string.IsNullOrEmpty(deltaJson))
            {
                await _jsModule.InvokeVoidAsync("setContents", _editorId, "{\"ops\":[{\"insert\":\"\\n\"}]}");
            }
            else
            {
                await _jsModule.InvokeVoidAsync("setContents", _editorId, deltaJson);
            }
        }
    }

    // === Private Helper Methods ===

    private object BuildEditorOptions() => new
    {
        placeholder = Placeholder ?? "",
        readOnly = Disabled || ReadOnly,
        hasImageUploader = ImageUploader != null
    };

    // === CSS Classes ===

    private string ContainerCssClass => ClassNames.cn(
        "bb:flex bb:flex-col bb:rounded-md bb:border bb:border-input bb:bg-background",
        "bb:focus-within:border-ring",
        ClassNames.when(AriaInvalid == true, "bb:border-destructive"),
        ClassNames.when(Disabled, "bb:opacity-50 bb:cursor-not-allowed"),
        Class
    );

    private static string ToolbarCssClass => ClassNames.cn(
        "bb:flex bb:flex-wrap bb:items-center bb:gap-1 bb:px-3 bb:py-2 bb:border-b bb:border-input bb:bg-muted/40"
    );

    private string EditorCssClass => ClassNames.cn(
        "bb:text-base bb:md:text-sm",
        ClassNames.when(Disabled, "bb:cursor-not-allowed")
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
        GC.SuppressFinalize(this);
        if (_jsModule != null && _jsInitialized)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("disposeEditor", _editorId);
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect in Blazor Server - safe to ignore
            }
            catch (ObjectDisposedException)
            {
                // Module already disposed - safe to ignore
            }
            catch (InvalidOperationException)
            {
                // JS interop not available (prerendering) - safe to ignore
            }
        }
        _dotNetRef?.Dispose();
    }
}
