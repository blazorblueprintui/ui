using System.Globalization;
using System.Linq.Expressions;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>A Gregorian date input with individually editable segments in culture-specific order.</summary>
public partial class BbDateInput : ComponentBase, IAsyncDisposable
{
    private readonly Dictionary<string, string> parts = new() { ["Year"] = "", ["Month"] = "", ["Day"] = "" };
    private readonly SegmentedInputValidation validation;
    private ElementReference root;
    private IJSObjectReference? module;
    private DateTime? lastValue;
    private bool initialized;
    private bool disposed;
    private bool open;

    public BbDateInput()
    {
        validation = new(() => { if (!disposed) { _ = InvokeAsync(StateHasChanged); } });
    }

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IBbLocalizer Localizer { get; set; } = default!;
    [CascadingParameter] private EditContext? EditContext { get; set; }

    /// <summary>The selected date, with no time portion. Partial or invalid input produces null.</summary>
    [Parameter] public DateTime? Value { get; set; }
    /// <summary>Raised when the selected date changes.</summary>
    [Parameter] public EventCallback<DateTime?> ValueChanged { get; set; }
    /// <summary>Identifies the bound form field for validation and form posting.</summary>
    [Parameter] public Expression<Func<DateTime?>>? ValueExpression { get; set; }
    /// <summary>Inclusive minimum selectable date.</summary>
    [Parameter] public DateTime? MinDate { get; set; }
    /// <summary>Inclusive maximum selectable date.</summary>
    [Parameter] public DateTime? MaxDate { get; set; }
    /// <summary>Controls segment order and separators. Defaults to the current culture.</summary>
    [Parameter] public CultureInfo? Culture { get; set; }
    /// <summary>Whether to display the optional calendar button.</summary>
    [Parameter] public bool ShowCalendar { get; set; } = true;
    /// <summary>Disables editing and form submission of this control.</summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>Prevents editing while keeping the control focusable.</summary>
    [Parameter] public bool ReadOnly { get; set; }
    /// <summary>Marks every segment required; model validation can enforce a required value.</summary>
    [Parameter] public bool Required { get; set; }
    /// <summary>ID of the first segment, for an external label.</summary>
    [Parameter] public string? Id { get; set; }
    /// <summary>Name of the ISO-formatted hidden form field.</summary>
    [Parameter] public string? Name { get; set; }
    /// <summary>Accessible name of the group.</summary>
    [Parameter] public string? AriaLabel { get; set; }
    /// <summary>ID of help or validation text.</summary>
    [Parameter] public string? AriaDescribedBy { get; set; }
    /// <summary>Additional classes for the input container.</summary>
    [Parameter] public string? Class { get; set; }
    /// <summary>Additional attributes for the input container.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private CultureInfo EffectiveCulture => Culture ?? CultureInfo.CurrentCulture;
    private string FirstSegment => DateTimeSegments.DateOrder(EffectiveCulture)[0];
    private string LastSegment => DateTimeSegments.DateOrder(EffectiveCulture)[^1];
    private int MaxDay => DateTimeSegments.Number(parts["Year"]) is > 0 and <= 9999 and var year
        && DateTimeSegments.Number(parts["Month"]) is > 0 and <= 12 and var month
        ? DateTime.DaysInMonth(year, month) : 31;
    private string CssClass => ClassNames.cn(
        "bb:flex bb:h-10 bb:w-fit bb:items-center bb:gap-1 bb:rounded-md bb:border bb:border-input bb:bg-background bb:px-3 bb:text-base bb:md:text-sm bb:focus-within:ring-2 bb:focus-within:ring-ring bb:aria-[invalid=true]:border-destructive",
        Disabled ? "bb:opacity-50 bb:cursor-not-allowed" : null, Class);

    protected override void OnParametersSet()
    {
        if (MinDate?.Date > MaxDate?.Date) { throw new ArgumentException("MinDate must not exceed MaxDate."); }
        validation.Update(EditContext, ValueExpression);
        if (!initialized || Value != lastValue)
        {
            initialized = true;
            Synchronize(Value);
        }
        var parsed = DateTimeSegments.Date(parts["Year"], parts["Month"], parts["Day"], MinDate, MaxDate);
        validation.SetError(parsed == null && parts.Values.Any(p => p.Length > 0) ? Localizer["DateInput.Invalid"] : null);
        if (Disabled || ReadOnly) { open = false; }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) { return; }
        module = await JsModules.GetAsync(JS, JsModules.Versioned("./_content/BlazorBlueprint.Components/js/segmented-input.js", typeof(BbDateInput).Assembly));
        if (!disposed) { await module.InvokeVoidAsync("initialize", root); }
    }

    private void Synchronize(DateTime? value)
    {
        lastValue = value;
        parts["Year"] = value?.Year.ToString("D4", CultureInfo.InvariantCulture) ?? "";
        parts["Month"] = value?.Month.ToString("D2", CultureInfo.InvariantCulture) ?? "";
        parts["Day"] = value?.Day.ToString("D2", CultureInfo.InvariantCulture) ?? "";
    }

    private async Task EditAsync(string segment, string text)
    {
        if (Disabled || ReadOnly || disposed || !parts.ContainsKey(segment)) { return; }
        parts[segment] = text;
        var next = DateTimeSegments.Date(parts["Year"], parts["Month"], parts["Day"], MinDate, MaxDate);
        validation.SetError(next == null && parts.Values.Any(p => p.Length > 0) ? Localizer["DateInput.Invalid"] : null);
        await PublishAsync(next);
    }

    private async Task SelectAsync(DateTime? value)
    {
        if (Disabled || ReadOnly || disposed) { return; }
        if (value.HasValue && ((MinDate.HasValue && value.Value.Date < MinDate.Value.Date) || (MaxDate.HasValue && value.Value.Date > MaxDate.Value.Date))) { return; }
        open = false;
        Synchronize(value?.Date);
        validation.SetError(null);
        await PublishAsync(value?.Date);
    }

    private async Task PublishAsync(DateTime? value)
    {
        var changed = Value != value;
        Value = lastValue = value;
        if (changed) { await ValueChanged.InvokeAsync(value); }
        validation.NotifyFieldChanged();
    }

    /// <summary>Clears every segment and the bound value.</summary>
    public Task ClearAsync() => InvokeAsync(async () => { await SelectAsync(null); StateHasChanged(); });

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        validation.Dispose();
        if (module != null)
        {
            try { await module.InvokeVoidAsync("dispose", root); }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or TaskCanceledException or ObjectDisposedException) { }
        }
        GC.SuppressFinalize(this);
    }
}
