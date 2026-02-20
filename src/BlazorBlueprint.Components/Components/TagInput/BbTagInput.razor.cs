using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// An inline tag/chip input component for entering and managing a list of string tags.
/// </summary>
public partial class BbTagInput : ComponentBase, IAsyncDisposable
{
    // ── Infrastructure ─────────────────────────────────────────────────
    private ElementReference _containerRef;
    private ElementReference _inputRef;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<BbTagInput>? _dotNetRef;
    private readonly string _instanceId = $"taginput-{Guid.NewGuid():N}";
    private bool _jsInitialized;
    private bool _disposed;

    // ── State ──────────────────────────────────────────────────────────
    private string _inputText = string.Empty;
    private List<string> _currentTags = new();
    private bool _suggestionsOpen;
    private int _suggestionIndex = -1;
    private List<string> _filteredSuggestions = new();
    private CancellationTokenSource? _suggestionCts;
    private CancellationTokenSource? _blurCts;

    // ── ShouldRender tracking ──────────────────────────────────────────
    private IReadOnlyList<string>? _lastTags;
    private string _lastInputText = string.Empty;
    private bool _lastSuggestionsOpen;
    private int _lastSuggestionIndex = -1;
    private bool _lastDisabled;

    // ── Remove handler cache ───────────────────────────────────────────
    private readonly Dictionary<string, Func<Task>> _removeHandlerCache = new();

    // ── Form integration ───────────────────────────────────────────────
    private readonly InputValidationBehavior _validation = new();

    [CascadingParameter]
    private EditContext? CascadedEditContext { get; set; }

    [CascadingParameter(Name = "FieldIsInvalid")]
    private bool? FieldIsInvalid { get; set; }

    // ══════════════════════════════════════════════════════════════════
    // Parameters
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets or sets the list of tags. Use <c>@bind-Tags</c> for two-way binding.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Tags { get; set; }

    /// <summary>
    /// Callback invoked when the tag list changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>?> TagsChanged { get; set; }

    /// <summary>
    /// Placeholder text shown when no tags are present.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Add tag...";

    /// <summary>
    /// Maximum number of tags allowed.
    /// </summary>
    [Parameter]
    public int MaxTags { get; set; } = int.MaxValue;

    /// <summary>
    /// Maximum character length for a single tag.
    /// </summary>
    [Parameter]
    public int MaxTagLength { get; set; } = 50;

    /// <summary>
    /// Whether duplicate tag values are allowed (case-insensitive comparison).
    /// </summary>
    [Parameter]
    public bool AllowDuplicates { get; set; }

    /// <summary>
    /// Which keys trigger tag creation. Combine flags with bitwise OR.
    /// </summary>
    [Parameter]
    public TagInputTrigger AddTrigger { get; set; } = TagInputTrigger.Enter | TagInputTrigger.Comma;

    /// <summary>
    /// Optional validation function. Return false to reject a tag.
    /// </summary>
    [Parameter]
    public Func<string, bool>? Validate { get; set; }

    /// <summary>
    /// Callback invoked when a tag is rejected (duplicate, validation failure, limit reached).
    /// </summary>
    [Parameter]
    public EventCallback<string> OnTagRejected { get; set; }

    /// <summary>
    /// Static list of suggestions shown as the user types.
    /// </summary>
    [Parameter]
    public IEnumerable<string>? Suggestions { get; set; }

    /// <summary>
    /// Async function to load suggestions based on the search query.
    /// </summary>
    [Parameter]
    public Func<string, CancellationToken, Task<IEnumerable<string>>>? OnSearchSuggestions { get; set; }

    /// <summary>
    /// Debounce interval in milliseconds for suggestion search.
    /// </summary>
    [Parameter]
    public int SuggestionDebounceMs { get; set; } = 300;

    /// <summary>
    /// Visual variant for rendered tags.
    /// </summary>
    [Parameter]
    public TagInputVariant Variant { get; set; } = TagInputVariant.Default;

    /// <summary>
    /// Whether to show a clear-all button when tags are present.
    /// </summary>
    [Parameter]
    public bool Clearable { get; set; }

    /// <summary>
    /// Whether the component is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Optional custom template for rendering each tag. The context parameter is the tag text.
    /// </summary>
    [Parameter]
    public RenderFragment<string>? TagTemplate { get; set; }

    /// <summary>
    /// Expression for the Tags property, used for EditForm validation.
    /// </summary>
    [Parameter]
    public Expression<Func<IReadOnlyList<string>?>>? TagsExpression { get; set; }

    /// <summary>
    /// Accessible label for the tag input container.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    // ══════════════════════════════════════════════════════════════════
    // Computed
    // ══════════════════════════════════════════════════════════════════

    private bool HasSuggestions => Suggestions is not null || OnSearchSuggestions is not null;

    private string? EffectivePlaceholder => _currentTags.Count == 0 ? Placeholder : null;

    private string? ActiveDescendantId =>
        _suggestionsOpen && _suggestionIndex >= 0
            ? $"{_instanceId}-suggestion-{_suggestionIndex}"
            : null;

    private BadgeVariant TagBadgeVariant => Variant switch
    {
        TagInputVariant.Outline => BadgeVariant.Outline,
        TagInputVariant.Secondary => BadgeVariant.Secondary,
        _ => BadgeVariant.Secondary
    };

    private bool IsInvalid => _validation.IsInvalid || FieldIsInvalid == true;

    // ══════════════════════════════════════════════════════════════════
    // Lifecycle
    // ══════════════════════════════════════════════════════════════════

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!ReferenceEquals(_lastTags, Tags))
        {
            _currentTags = Tags?.ToList() ?? new List<string>();
            _removeHandlerCache.Clear();
            _lastTags = Tags;
        }

        _validation.Update(CascadedEditContext, TagsExpression);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorBlueprint.Components/js/tag-input.js");
                _dotNetRef = DotNetObjectReference.Create(this);
                await _jsModule.InvokeVoidAsync("initialize",
                    _containerRef,
                    _inputRef,
                    _dotNetRef,
                    _instanceId,
                    new { triggers = (int)AddTrigger });
                _jsInitialized = true;
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
            catch (InvalidOperationException)
            {
                // JS interop not available during prerendering
            }
        }
    }

    protected override bool ShouldRender()
    {
        var changed =
            _lastInputText != _inputText ||
            _lastSuggestionsOpen != _suggestionsOpen ||
            _lastSuggestionIndex != _suggestionIndex ||
            _lastDisabled != Disabled;

        if (changed)
        {
            _lastInputText = _inputText;
            _lastSuggestionsOpen = _suggestionsOpen;
            _lastSuggestionIndex = _suggestionIndex;
            _lastDisabled = Disabled;
            return true;
        }

        return true; // Allow re-renders triggered by parent parameter changes
    }

    // ══════════════════════════════════════════════════════════════════
    // JSInvokable callbacks
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Called from JS when a configured trigger key is pressed.
    /// </summary>
    [JSInvokable]
    public async Task JsTriggerAdd()
    {
        if (_disposed || Disabled)
        {
            return;
        }

        // If a suggestion is highlighted, select it instead
        if (_suggestionsOpen && _suggestionIndex >= 0 && _suggestionIndex < _filteredSuggestions.Count)
        {
            await TryAddTag(_filteredSuggestions[_suggestionIndex]);
        }
        else
        {
            await TryAddTag(_inputText);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Called from JS when Backspace is pressed on an empty input.
    /// </summary>
    [JSInvokable]
    public async Task JsBackspace()
    {
        if (_disposed || Disabled || _currentTags.Count == 0)
        {
            return;
        }

        var lastTag = _currentTags[^1];
        _currentTags.RemoveAt(_currentTags.Count - 1);
        _removeHandlerCache.Remove(lastTag);
        await NotifyTagsChanged();
        StateHasChanged();
    }

    /// <summary>
    /// Called from JS when text is pasted containing delimiters.
    /// </summary>
    [JSInvokable]
    public async Task JsPasteText(string text)
    {
        if (_disposed || Disabled || string.IsNullOrEmpty(text))
        {
            return;
        }

        var delimiters = new List<char>();
        if (AddTrigger.HasFlag(TagInputTrigger.Comma))
        {
            delimiters.Add(',');
        }
        if (AddTrigger.HasFlag(TagInputTrigger.Semicolon))
        {
            delimiters.Add(';');
        }
        if (AddTrigger.HasFlag(TagInputTrigger.Space))
        {
            delimiters.Add(' ');
        }

        var parts = delimiters.Count > 0
            ? text.Split(delimiters.ToArray(), StringSplitOptions.RemoveEmptyEntries)
            : new[] { text };

        foreach (var part in parts)
        {
            await TryAddTag(part);
        }

        _inputText = string.Empty;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JS to navigate to the next suggestion.
    /// </summary>
    [JSInvokable]
    public void JsSuggestionNext()
    {
        if (_disposed || !_suggestionsOpen || _filteredSuggestions.Count == 0)
        {
            return;
        }

        _suggestionIndex = (_suggestionIndex + 1) % _filteredSuggestions.Count;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JS to navigate to the previous suggestion.
    /// </summary>
    [JSInvokable]
    public void JsSuggestionPrev()
    {
        if (_disposed || !_suggestionsOpen || _filteredSuggestions.Count == 0)
        {
            return;
        }

        _suggestionIndex = _suggestionIndex <= 0
            ? _filteredSuggestions.Count - 1
            : _suggestionIndex - 1;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JS to close the suggestions dropdown.
    /// </summary>
    [JSInvokable]
    public void JsSuggestionClose()
    {
        if (_disposed)
        {
            return;
        }

        _suggestionsOpen = false;
        _suggestionIndex = -1;
        StateHasChanged();
    }

    // ══════════════════════════════════════════════════════════════════
    // Tag logic
    // ══════════════════════════════════════════════════════════════════

    private async Task TryAddTag(string rawText)
    {
        var text = rawText.Trim();

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (_currentTags.Count >= MaxTags)
        {
            await OnTagRejected.InvokeAsync(text);
            return;
        }

        if (text.Length > MaxTagLength)
        {
            await OnTagRejected.InvokeAsync(text);
            return;
        }

        if (!AllowDuplicates && _currentTags.Contains(text, StringComparer.OrdinalIgnoreCase))
        {
            await OnTagRejected.InvokeAsync(text);
            return;
        }

        if (Validate is not null && !Validate(text))
        {
            await OnTagRejected.InvokeAsync(text);
            return;
        }

        _currentTags.Add(text);
        _inputText = string.Empty;
        _suggestionsOpen = false;
        _suggestionIndex = -1;
        _removeHandlerCache.Clear();
        await NotifyTagsChanged();
    }

    private async Task RemoveTag(string tag)
    {
        _currentTags.Remove(tag);
        _removeHandlerCache.Remove(tag);
        await NotifyTagsChanged();
    }

    private async Task ClearAll()
    {
        _currentTags.Clear();
        _removeHandlerCache.Clear();
        _inputText = string.Empty;
        _suggestionsOpen = false;
        _suggestionIndex = -1;
        await NotifyTagsChanged();
    }

    private async Task NotifyTagsChanged()
    {
        IReadOnlyList<string>? newTags = _currentTags.Count > 0
            ? _currentTags.ToArray()
            : null;

        Tags = newTags;
        _lastTags = newTags;

        await TagsChanged.InvokeAsync(newTags);
        _validation.NotifyFieldChanged();
    }

    private Func<Task> GetRemoveHandler(string tag)
    {
        if (!_removeHandlerCache.TryGetValue(tag, out var handler))
        {
            handler = () => RemoveTag(tag);
            _removeHandlerCache[tag] = handler;
        }

        return handler;
    }

    // ══════════════════════════════════════════════════════════════════
    // Input / focus handlers
    // ══════════════════════════════════════════════════════════════════

    private async Task HandleInput(ChangeEventArgs e)
    {
        _inputText = e.Value?.ToString() ?? string.Empty;

        if (HasSuggestions && !string.IsNullOrWhiteSpace(_inputText))
        {
            await LoadSuggestionsAsync(_inputText);
        }
        else
        {
            _filteredSuggestions.Clear();
            _suggestionsOpen = false;
            _suggestionIndex = -1;
        }
    }

    private void HandleFocus()
    {
        _blurCts?.Cancel();
        _blurCts?.Dispose();
        _blurCts = null;

        if (HasSuggestions && _filteredSuggestions.Count > 0 && !string.IsNullOrWhiteSpace(_inputText))
        {
            _suggestionsOpen = true;
        }
    }

    private async Task HandleBlur()
    {
        // Delay closing to allow mousedown on suggestions to fire first
        _blurCts?.Cancel();
        _blurCts?.Dispose();
        _blurCts = new CancellationTokenSource();
        var token = _blurCts.Token;

        try
        {
            await Task.Delay(150, token);
            if (!token.IsCancellationRequested)
            {
                _suggestionsOpen = false;
                _suggestionIndex = -1;
                StateHasChanged();
            }
        }
        catch (OperationCanceledException)
        {
            // Focus returned before timeout — keep suggestions open
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // Suggestions
    // ══════════════════════════════════════════════════════════════════

    private async Task LoadSuggestionsAsync(string query)
    {
        _suggestionCts?.Cancel();
        _suggestionCts?.Dispose();
        _suggestionCts = new CancellationTokenSource();
        var token = _suggestionCts.Token;

        try
        {
            if (OnSearchSuggestions is not null)
            {
                await Task.Delay(SuggestionDebounceMs, token);
            }

            IEnumerable<string> results;

            if (OnSearchSuggestions is not null)
            {
                results = await OnSearchSuggestions(query, token);
            }
            else if (Suggestions is not null)
            {
                results = Suggestions.Where(s =>
                    s.Contains(query, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            _filteredSuggestions = results
                .Where(s => AllowDuplicates || !_currentTags.Contains(s, StringComparer.OrdinalIgnoreCase))
                .ToList();

            _suggestionsOpen = _filteredSuggestions.Count > 0;
            _suggestionIndex = -1;
            StateHasChanged();
        }
        catch (OperationCanceledException)
        {
            // Expected when query changes rapidly
        }
    }

    private async Task SelectSuggestion(string suggestion)
    {
        _blurCts?.Cancel();
        await TryAddTag(suggestion);
        StateHasChanged();

        // Re-focus the input after selection
        try
        {
            if (_jsModule is not null && _jsInitialized)
            {
                await _jsModule.InvokeVoidAsync("focusInput", _instanceId);
            }
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            // Expected during circuit disconnect
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // CSS
    // ══════════════════════════════════════════════════════════════════

    private string ContainerCssClass => ClassNames.cn(
        "flex flex-wrap items-center gap-1.5 min-h-10 w-full rounded-md",
        "border px-3 py-2 text-sm",
        "transition-colors",
        "focus-within:outline-none focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2",
        Variant switch
        {
            TagInputVariant.Secondary => "border-input bg-secondary/50",
            _ => "border-input bg-background"
        },
        IsInvalid ? "border-destructive focus-within:ring-destructive" : null,
        Disabled ? "opacity-50 cursor-not-allowed" : null,
        Class
    );

    private static string InputCssClass =>
        "flex-1 min-w-[120px] bg-transparent outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed text-sm";

    private static string SuggestionItemCssClass(bool isActive) => ClassNames.cn(
        "flex cursor-pointer select-none items-center rounded-sm px-2 py-1.5 text-sm",
        "hover:bg-accent hover:text-accent-foreground",
        isActive ? "bg-accent text-accent-foreground" : null
    );

    // ══════════════════════════════════════════════════════════════════
    // IAsyncDisposable
    // ══════════════════════════════════════════════════════════════════

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        GC.SuppressFinalize(this);

        _suggestionCts?.Cancel();
        _suggestionCts?.Dispose();
        _suggestionCts = null;

        _blurCts?.Cancel();
        _blurCts?.Dispose();
        _blurCts = null;

        if (_jsModule is not null && _jsInitialized)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("dispose", _instanceId);
                await _jsModule.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
            {
                // Expected during circuit disconnect
            }
            catch (InvalidOperationException)
            {
                // JS interop not available
            }

            _jsModule = null;
        }

        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }
}
