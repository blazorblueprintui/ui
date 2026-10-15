using System.Linq.Expressions;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// An always-visible list of options, with no trigger and no popover.
/// </summary>
/// <remarks>
/// <para>
/// Use this where the choices should stay on screen — a settings panel, a transfer list, a filter
/// pane. Where the choices should stay out of the way until asked for, use <c>BbSelect</c> or
/// <c>BbMultiSelect</c>, which are the same list behind a trigger.
/// </para>
/// <para>
/// The list is a single tab stop and moves an active option with the arrow keys, reported through
/// <c>aria-activedescendant</c>. That is what makes it usable without a mouse.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The type of the option values.</typeparam>
public partial class BbListBox<TValue> : ComponentBase, IAsyncDisposable
{
    private readonly string listId = $"bb-listbox-{Guid.NewGuid():N}";
    private List<SelectOption<TValue>> visibleOptions = [];
    private HashSet<TValue> selectedValues = new();
    private string searchQuery = string.Empty;
    private string lastSuppliedSearch = string.Empty;
    private int activeIndex = -1;
    private int anchorIndex = -1;
    private bool hasFocus;
    private ElementReference listElement;
    private IJSObjectReference? keyboardCleanup;
    private bool pendingScroll;

    private string typeahead = string.Empty;
    private DateTime typeaheadAt = DateTime.MinValue;

    private IJSObjectReference? jsModule;
    private bool disposed;

    private EditContext? editContext;
    private FieldIdentifier fieldIdentifier;

    /// <summary>
    /// Typing resets rather than extends after this long, so an old search does not swallow a new
    /// one. Matches the dwell a native list allows.
    /// </summary>
    private static readonly TimeSpan TypeaheadWindow = TimeSpan.FromMilliseconds(800);

    [CascadingParameter] private EditContext? CascadedEditContext { get; set; }

    /// <summary>
    /// Gets or sets the options to choose from.
    /// </summary>
    [Parameter]
    public IEnumerable<SelectOption<TValue>>? Options { get; set; }

    /// <summary>
    /// Gets or sets whether one option can be chosen or many.
    /// </summary>
    [Parameter]
    public ListBoxSelectionMode SelectionMode { get; set; } = ListBoxSelectionMode.Single;

    /// <summary>
    /// Gets or sets the chosen value, when <see cref="SelectionMode"/> is
    /// <see cref="ListBoxSelectionMode.Single"/>.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the chosen value changes. Use with
    /// <c>@bind-Value</c>.
    /// </summary>
    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression identifying the bound value, for validation inside an
    /// <c>EditForm</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<TValue?>>? ValueExpression { get; set; }

    /// <summary>
    /// Gets or sets the chosen values, when <see cref="SelectionMode"/> is
    /// <see cref="ListBoxSelectionMode.Multiple"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<TValue>? Values { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the chosen values change. Use with
    /// <c>@bind-Values</c>.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TValue>?> ValuesChanged { get; set; }

    /// <summary>
    /// Gets or sets an expression identifying the bound values, for validation inside an
    /// <c>EditForm</c>.
    /// </summary>
    [Parameter]
    public Expression<Func<IEnumerable<TValue>?>>? ValuesExpression { get; set; }

    /// <summary>
    /// Gets or sets the label shown above the list.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets whether a search box is shown above the list.
    /// </summary>
    [Parameter]
    public bool ShowSearch { get; set; }

    /// <summary>
    /// Gets or sets the placeholder for the search box.
    /// </summary>
    [Parameter]
    public string? SearchPlaceholder { get; set; }

    /// <summary>
    /// Gets or sets what the search box contains. Use with <c>@bind-SearchText</c>.
    /// </summary>
    /// <remarks>
    /// Bindable so that a parent can tell which options are on screen rather than guessing, which
    /// is what <c>BbPickList</c> needs to move only what a search has left visible.
    /// </remarks>
    [Parameter]
    public string? SearchText { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the search box changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> SearchTextChanged { get; set; }

    /// <summary>
    /// Gets or sets whether a select-all checkbox is shown. Only applies in
    /// <see cref="ListBoxSelectionMode.Multiple"/>.
    /// </summary>
    /// <remarks>
    /// It sits above the list rather than inside it: an option that selects the other options
    /// would be announced as one of the choices and would take a place in the arrow-key order.
    /// Select-all covers what the search box has left visible, which is what makes it useful for
    /// narrowing then taking the lot.
    /// </remarks>
    [Parameter]
    public bool ShowSelectAll { get; set; } = true;

    /// <summary>
    /// Gets or sets the label for the select-all checkbox.
    /// </summary>
    [Parameter]
    public string? SelectAllLabel { get; set; }

    /// <summary>
    /// Gets or sets the message shown when there is nothing to choose from.
    /// </summary>
    [Parameter]
    public string? EmptyMessage { get; set; }

    /// <summary>
    /// Gets or sets a predicate marking individual options as unavailable.
    /// </summary>
    [Parameter]
    public Func<SelectOption<TValue>, bool>? OptionDisabled { get; set; }

    /// <summary>
    /// Gets or sets a template for an option's content. The indicator is drawn either way.
    /// </summary>
    [Parameter]
    public RenderFragment<SelectOption<TValue>>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the height of the scrolling area as a CSS length. Default is <c>16rem</c>.
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "16rem";

    /// <summary>
    /// Gets or sets whether the whole list is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets an accessible name for the list, for when there is no <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes for the root element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets additional HTML attributes for the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsMultiple => SelectionMode == ListBoxSelectionMode.Multiple;

    private string EffectiveSearchPlaceholder => SearchPlaceholder ?? Localizer["ListBox.SearchPlaceholder"];

    private string EffectiveSelectAllLabel => SelectAllLabel ?? Localizer["ListBox.SelectAll"];

    private string EffectiveEmptyMessage => EmptyMessage
        ?? (searchQuery.Length > 0 ? Localizer["ListBox.NoMatches"] : Localizer["ListBox.Empty"]);

    private string SelectionSummary => Localizer["ListBox.SelectedCount", selectedValues.Count];

    private string? ActiveDescendantId =>
        hasFocus && activeIndex >= 0 && activeIndex < visibleOptions.Count ? OptionId(activeIndex) : null;

    private string OptionId(int index) => $"{listId}-option-{index}";

    private string RootClass => ClassNames.cn(
        "bb:w-full",
        Disabled ? "bb:opacity-60" : null,
        Class);

    private const string FrameClass =
        "bb:rounded-md bb:border bb:border-input bb:bg-transparent bb:dark:bg-input/30 bb:shadow-xs bb:overflow-hidden";

    private const string CheckboxClass =
        "bb:h-4 bb:w-4 bb:shrink-0 bb:rounded-sm bb:border bb:border-primary bb:flex bb:items-center bb:justify-center " +
        "bb:focus-visible:outline-none bb:disabled:cursor-not-allowed bb:disabled:opacity-50 " +
        "bb:data-[state=checked]:bg-primary bb:data-[state=checked]:text-primary-foreground " +
        "bb:data-[state=indeterminate]:bg-primary bb:data-[state=indeterminate]:text-primary-foreground " +
        "bb:data-[state=unchecked]:bg-background";

    private string IndicatorClass(bool selected) => IsMultiple
        ? ClassNames.cn(
            "bb:h-4 bb:w-4 bb:shrink-0 bb:rounded-sm bb:border bb:border-primary bb:flex bb:items-center bb:justify-center",
            selected ? "bb:bg-primary bb:text-primary-foreground" : "bb:bg-background")
        : "bb:h-4 bb:w-4 bb:shrink-0 bb:flex bb:items-center bb:justify-center";

    private static string OptionClass(bool optionDisabled) => ClassNames.cn(
        "bb:relative bb:flex bb:cursor-pointer bb:select-none bb:items-center bb:gap-2 bb:rounded-sm bb:px-2 bb:py-1.5 bb:text-sm bb:outline-none",
        "bb:data-[active=true]:bg-accent bb:data-[active=true]:text-accent-foreground",
        "bb:data-[selected=true]:font-medium",
        optionDisabled ? "bb:pointer-events-none bb:opacity-50" : "bb:hover:bg-accent/50");

    private enum SelectAllKind
    {
        None,
        Indeterminate,
        All
    }

    private SelectAllKind SelectAllState
    {
        get
        {
            var selectable = visibleOptions.Where(o => !IsOptionDisabled(o)).ToList();
            if (selectable.Count == 0)
            {
                return SelectAllKind.None;
            }

            var chosen = selectable.Count(o => IsSelected(o.Value));
            return chosen == 0 ? SelectAllKind.None
                : chosen == selectable.Count ? SelectAllKind.All
                : SelectAllKind.Indeterminate;
        }
    }

    protected override void OnParametersSet()
    {
        // Adopt the supplied term only when it is a new one. Comparing against searchQuery instead
        // would undo the person's typing on the next render, because their text is exactly what the
        // parameter does not yet hold.
        var supplied = SearchText ?? string.Empty;
        if (supplied != lastSuppliedSearch)
        {
            lastSuppliedSearch = supplied;
            searchQuery = supplied;
        }

        selectedValues = IsMultiple
            ? [.. Values ?? []]
            : Value is null ? [] : [Value];

        RebuildVisibleOptions();

        if (CascadedEditContext is not null)
        {
            if (IsMultiple && ValuesExpression is not null)
            {
                editContext = CascadedEditContext;
                fieldIdentifier = FieldIdentifier.Create(ValuesExpression);
            }
            else if (!IsMultiple && ValueExpression is not null)
            {
                editContext = CascadedEditContext;
                fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            }
        }
    }

    private void RebuildVisibleOptions()
    {
        var all = Options?.Where(o => o is not null) ?? [];
        visibleOptions = [.. all.Where(o => ListBoxSearch.Matches(o.Text, searchQuery))];

        if (activeIndex >= visibleOptions.Count)
        {
            activeIndex = visibleOptions.Count - 1;
        }
    }

    private bool IsSelected(TValue value) => selectedValues.Contains(value);

    private bool IsOptionDisabled(SelectOption<TValue> option) => OptionDisabled?.Invoke(option) ?? false;

    // ── Pointer ────────────────────────────────────────────────────────

    private async Task HandleOptionClickAsync(int index, MouseEventArgs args)
    {
        if (Disabled || index < 0 || index >= visibleOptions.Count)
        {
            return;
        }

        var option = visibleOptions[index];
        if (IsOptionDisabled(option))
        {
            return;
        }

        activeIndex = index;

        if (IsMultiple && args.ShiftKey && anchorIndex >= 0)
        {
            await SelectRangeAsync(anchorIndex, index);
            return;
        }

        anchorIndex = index;

        // Ctrl or Cmd keeps a multi-select additive without needing the checkbox, which is what a
        // list behaves like everywhere else. Without a modifier a plain click still toggles,
        // because the checkboxes say the list is additive.
        await ActivateAsync(option);
    }

    // ── Keyboard ───────────────────────────────────────────────────────

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled)
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                await MoveAsync(1, args.ShiftKey);
                return;
            case "ArrowUp":
                await MoveAsync(-1, args.ShiftKey);
                return;
            case "Home":
                await MoveToAsync(FirstEnabled(0, 1), args.ShiftKey);
                return;
            case "End":
                await MoveToAsync(FirstEnabled(visibleOptions.Count - 1, -1), args.ShiftKey);
                return;
            case "PageDown":
                await MoveAsync(10, args.ShiftKey);
                return;
            case "PageUp":
                await MoveAsync(-10, args.ShiftKey);
                return;
            case " ":
            case "Enter":
                if (activeIndex >= 0 && activeIndex < visibleOptions.Count)
                {
                    anchorIndex = activeIndex;
                    await ActivateAsync(visibleOptions[activeIndex]);
                }
                return;
            case "a" or "A" when IsMultiple && (args.CtrlKey || args.MetaKey):
                await SelectAllAsync();
                return;
        }

        // A single printable character jumps to the next option starting with it.
        if (args.Key.Length == 1 && !args.CtrlKey && !args.MetaKey && !args.AltKey)
        {
            await TypeaheadAsync(args.Key);
            return;
        }
    }

    private async Task HandleSearchInputAsync(ChangeEventArgs args)
    {
        searchQuery = args.Value?.ToString() ?? string.Empty;
        RebuildVisibleOptions();
        // The old active index refers to a list that no longer exists.
        activeIndex = visibleOptions.Count > 0 ? FirstEnabled(0, 1) : -1;
        anchorIndex = -1;
        // lastSuppliedSearch deliberately untouched: it records what the parent last supplied, not
        // what was typed. Setting it here would make the next parameter pass look like a change and
        // wipe the typing on a list box whose SearchText nobody is binding.
        await SearchTextChanged.InvokeAsync(searchQuery);
    }

    private async Task HandleSearchKeyDownAsync(KeyboardEventArgs args)
    {
        if (args.Key is not ("ArrowDown" or "Enter"))
        {
            return;
        }

        // Moving down out of the search box hands the arrows to the list, which is where they mean
        // something. Focus has to actually move: aria-activedescendant on an input the list does
        // not own would point at options the screen reader cannot reach.
        activeIndex = visibleOptions.Count > 0 ? FirstEnabled(0, 1) : -1;
        await FocusListAsync();
    }

    private async Task TypeaheadAsync(string key)
    {
        var now = DateTime.UtcNow;
        var expired = now - typeaheadAt > TypeaheadWindow;

        // Pressing one letter over and over walks through the options starting with it, rather
        // than searching for "ccc" and finding nothing. Anything else spells out a prefix.
        var cycling = !expired && typeahead.Length > 0 && typeahead.All(c => c == key[0]);

        typeahead = expired ? key : typeahead + key;
        typeaheadAt = now;

        var needle = cycling ? key : typeahead;
        var start = activeIndex < 0 ? 0 : activeIndex;
        // A fresh search may match where the caret already is; a cycle must move off it.
        var from = cycling || needle.Length == 1 ? start + 1 : start;

        for (var offset = 0; offset < visibleOptions.Count; offset++)
        {
            var index = (((from + offset) % visibleOptions.Count) + visibleOptions.Count) % visibleOptions.Count;
            var option = visibleOptions[index];
            if (IsOptionDisabled(option) || !option.Text.StartsWith(needle, StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }

            await MoveToAsync(index, extend: false);
            return;
        }
    }

    private async Task MoveAsync(int delta, bool extend)
    {
        if (visibleOptions.Count == 0)
        {
            return;
        }

        var start = activeIndex < 0 ? (delta > 0 ? -1 : visibleOptions.Count) : activeIndex;
        var step = Math.Sign(delta);
        var target = Math.Clamp(start + delta, 0, visibleOptions.Count - 1);

        // Land on something choosable rather than stopping dead on a disabled row.
        target = FirstEnabled(target, step == 0 ? 1 : step);
        await MoveToAsync(target, extend);
    }

    private int FirstEnabled(int from, int step)
    {
        for (var index = Math.Clamp(from, 0, Math.Max(visibleOptions.Count - 1, 0));
             index >= 0 && index < visibleOptions.Count;
             index += step)
        {
            if (!IsOptionDisabled(visibleOptions[index]))
            {
                return index;
            }
        }

        // Nothing in that direction; keep whatever was active rather than jumping somewhere else.
        return activeIndex;
    }

    private async Task MoveToAsync(int index, bool extend)
    {
        if (index < 0 || index >= visibleOptions.Count)
        {
            return;
        }

        activeIndex = index;
        pendingScroll = true;

        if (IsMultiple)
        {
            if (extend)
            {
                if (anchorIndex < 0)
                {
                    anchorIndex = index;
                }
                await SelectRangeAsync(anchorIndex, index);
                return;
            }

            anchorIndex = index;
            return;
        }

        // A single-select list selects as it moves, the way a native one does.
        anchorIndex = index;
        await CommitSingleAsync(visibleOptions[index].Value);
    }

    // ── Selection ──────────────────────────────────────────────────────

    private async Task ActivateAsync(SelectOption<TValue> option)
    {
        if (IsMultiple)
        {
            if (!selectedValues.Add(option.Value))
            {
                selectedValues.Remove(option.Value);
            }
            await CommitMultipleAsync();
            return;
        }

        await CommitSingleAsync(option.Value);
    }

    private async Task SelectRangeAsync(int from, int to)
    {
        var start = Math.Min(from, to);
        var end = Math.Max(from, to);

        for (var index = start; index <= end && index < visibleOptions.Count; index++)
        {
            var option = visibleOptions[index];
            if (!IsOptionDisabled(option))
            {
                selectedValues.Add(option.Value);
            }
        }

        await CommitMultipleAsync();
    }

    private async Task ToggleSelectAllAsync()
    {
        if (SelectAllState == SelectAllKind.All)
        {
            foreach (var option in visibleOptions.Where(o => !IsOptionDisabled(o)))
            {
                selectedValues.Remove(option.Value);
            }
            await CommitMultipleAsync();
            return;
        }

        await SelectAllAsync();
    }

    private async Task SelectAllAsync()
    {
        // Only what the search has left visible. Taking hidden options too would mean the box
        // quietly selecting things the person cannot see.
        foreach (var option in visibleOptions.Where(o => !IsOptionDisabled(o)))
        {
            selectedValues.Add(option.Value);
        }

        await CommitMultipleAsync();
    }

    private async Task CommitSingleAsync(TValue value)
    {
        selectedValues = [value];
        Value = value;
        await ValueChanged.InvokeAsync(value);
        NotifyEditContext();
    }

    private async Task CommitMultipleAsync()
    {
        var values = selectedValues.Count > 0 ? selectedValues.ToList() : null;
        Values = values;
        await ValuesChanged.InvokeAsync(values);
        NotifyEditContext();
    }

    private void NotifyEditContext()
    {
        if (editContext is not null && fieldIdentifier.FieldName is not null)
        {
            editContext.NotifyFieldChanged(fieldIdentifier);
        }
    }

    // ── Focus and scrolling ────────────────────────────────────────────

    private void HandleFocus()
    {
        hasFocus = true;
        if (activeIndex < 0 && visibleOptions.Count > 0)
        {
            // Open on the selection when there is one, so arrowing continues from where the person
            // left off rather than from the top.
            var selected = visibleOptions.FindIndex(o => IsSelected(o.Value));
            activeIndex = selected >= 0 ? selected : FirstEnabled(0, 1);
            pendingScroll = true;
        }
    }

    private void HandleBlur() => hasFocus = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (keyboardCleanup is null && !disposed)
        {
            await SafeJsAsync(async module => keyboardCleanup = await module.InvokeAsync<IJSObjectReference>(
                "keyboardNav.setupListBox", listElement));
        }

        if (!pendingScroll || disposed)
        {
            return;
        }

        pendingScroll = false;
        if (activeIndex < 0 || activeIndex >= visibleOptions.Count)
        {
            return;
        }

        await SafeJsAsync(module => module.InvokeVoidAsync(
            "elementUtils.scrollIntoView", OptionId(activeIndex), "nearest", "instant").AsTask());
    }

    private async Task FocusListAsync() =>
        await SafeJsAsync(module => module.InvokeVoidAsync("elementUtils.focusElement", listId).AsTask());

    private async Task SafeJsAsync(Func<IJSObjectReference, Task> action)
    {
        try
        {
            jsModule ??= await PrimitiveModules.GetAsync(JS);
            if (!disposed)
            {
                await action(jsModule);
            }
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException or InvalidOperationException)
        {
            // Expected during circuit disconnect or prerendering
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        disposed = true;
        if (keyboardCleanup is not null)
        {
            try
            {
                await keyboardCleanup.InvokeVoidAsync("dispose");
                await keyboardCleanup.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException)
            {
                // The browser may have gone away before the component unmounted.
            }
        }
    }
}
