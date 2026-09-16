using System.Globalization;
using System.Linq.Expressions;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>A time-of-day input with keyboard-editable hour, minute and optional second segments.</summary>
public partial class BbTimeInput : ComponentBase, IAsyncDisposable
{
    private readonly Dictionary<string, string> parts = new() { ["Hour"] = "", ["Minute"] = "", ["Second"] = "" };
    private readonly SegmentedInputValidation validation;
    private ElementReference root;
    private IJSObjectReference? module;
    private TimeSpan? lastValue;
    private bool lastHour12;
    private bool lastShowSeconds;
    private bool initialized;
    private bool disposed;
    private bool open;
    private bool pm;

    public BbTimeInput()
    {
        validation = new(() => { if (!disposed) { _ = InvokeAsync(StateHasChanged); } });
    }

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IBbLocalizer Localizer { get; set; } = default!;
    [CascadingParameter] private EditContext? EditContext { get; set; }

    /// <summary>The selected time within one day. Partial or invalid input produces null.</summary>
    [Parameter] public TimeSpan? Value { get; set; }
    /// <summary>Raised when the selected time changes.</summary>
    [Parameter] public EventCallback<TimeSpan?> ValueChanged { get; set; }
    /// <summary>Identifies the bound form field for validation and form posting.</summary>
    [Parameter] public Expression<Func<TimeSpan?>>? ValueExpression { get; set; }
    /// <summary>Inclusive minimum time of day.</summary>
    [Parameter] public TimeSpan? MinTime { get; set; }
    /// <summary>Inclusive maximum time of day. Overnight bounds are not supported.</summary>
    [Parameter] public TimeSpan? MaxTime { get; set; }
    /// <summary>Controls separators and default 12/24-hour format.</summary>
    [Parameter] public CultureInfo? Culture { get; set; }
    /// <summary>Overrides the culture's time format when set.</summary>
    [Parameter] public TimeFormat? Format { get; set; }
    /// <summary>Shows the seconds segment.</summary>
    [Parameter] public bool ShowSeconds { get; set; }
    /// <summary>Minute increment for spin keys and picker options, from 1 to 59. Typed minutes need not be multiples.</summary>
    [Parameter] public int MinuteStep { get; set; } = 1;
    /// <summary>Whether to display the optional clock picker button.</summary>
    [Parameter] public bool ShowPicker { get; set; } = true;
    /// <summary>Disables editing and form submission of this control.</summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>Prevents editing while keeping the numeric segments focusable.</summary>
    [Parameter] public bool ReadOnly { get; set; }
    /// <summary>Marks every visible segment required.</summary>
    [Parameter] public bool Required { get; set; }
    /// <summary>ID of the hour segment, for an external label.</summary>
    [Parameter] public string? Id { get; set; }
    /// <summary>Name of the hidden form field containing a 24-hour time.</summary>
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
    private bool Hour12 => Format == TimeFormat.Hour12 || (Format == null && EffectiveCulture.DateTimeFormat.ShortTimePattern.Contains('h'));
    private string AmLabel => string.IsNullOrEmpty(EffectiveCulture.DateTimeFormat.AMDesignator) ? "AM" : EffectiveCulture.DateTimeFormat.AMDesignator;
    private string PmLabel => string.IsNullOrEmpty(EffectiveCulture.DateTimeFormat.PMDesignator) ? "PM" : EffectiveCulture.DateTimeFormat.PMDesignator;
    private string[] Segments => ShowSeconds ? ["Hour", "Minute", "Second"] : ["Hour", "Minute"];
    private int Minimum(string segment) => segment == "Hour" && Hour12 ? 1 : 0;
    private int Maximum(string segment) => segment == "Hour" ? Hour12 ? 12 : 23 : 59;
    private IEnumerable<int> PickerValues(string segment)
    {
        var values = Enumerable.Range(Minimum(segment), Maximum(segment) - Minimum(segment) + 1);
        return segment == "Minute" ? values.Where(n => n % MinuteStep == 0 || n == DateTimeSegments.Number(parts[segment])) : values;
    }
    private string CssClass => ClassNames.cn(
        "bb:flex bb:h-10 bb:w-fit bb:items-center bb:gap-1 bb:rounded-md bb:border bb:border-input bb:bg-background bb:px-3 bb:text-base bb:md:text-sm bb:focus-within:ring-2 bb:focus-within:ring-ring bb:aria-[invalid=true]:border-destructive",
        Disabled ? "bb:opacity-50 bb:cursor-not-allowed" : null, Class);

    protected override void OnParametersSet()
    {
        if (MinuteStep is < 1 or > 59) { throw new ArgumentOutOfRangeException(nameof(MinuteStep)); }
        if (MinTime < TimeSpan.Zero || MinTime >= TimeSpan.FromDays(1) || MaxTime < TimeSpan.Zero || MaxTime >= TimeSpan.FromDays(1) || MinTime > MaxTime)
        {
            throw new ArgumentException("Time bounds must be within one day and ordered from minimum to maximum.");
        }
        if (Value < TimeSpan.Zero || Value >= TimeSpan.FromDays(1)) { throw new ArgumentOutOfRangeException(nameof(Value)); }
        validation.Update(EditContext, ValueExpression);
        if (!initialized || Value != lastValue || Hour12 != lastHour12 || ShowSeconds != lastShowSeconds)
        {
            initialized = true;
            lastHour12 = Hour12;
            lastShowSeconds = ShowSeconds;
            Synchronize(Value);
        }
        var parsed = DateTimeSegments.Time(parts["Hour"], parts["Minute"], parts["Second"], ShowSeconds, Hour12, pm, MinTime, MaxTime);
        validation.SetError(parsed == null && Segments.Any(p => parts[p].Length > 0) ? Localizer["TimeInput.Invalid"] : null);
        if (Disabled || ReadOnly) { open = false; }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) { return; }
        module = await JsModules.GetAsync(JS, JsModules.Versioned("./_content/BlazorBlueprint.Components/js/segmented-input.js", typeof(BbTimeInput).Assembly));
        if (!disposed) { await module.InvokeVoidAsync("initialize", root); }
    }

    private void Synchronize(TimeSpan? value)
    {
        lastValue = value;
        pm = value?.Hours >= 12;
        parts["Hour"] = value.HasValue ? (Hour12 ? ((value.Value.Hours + 11) % 12) + 1 : value.Value.Hours).ToString("D2", CultureInfo.InvariantCulture) : "";
        parts["Minute"] = value?.Minutes.ToString("D2", CultureInfo.InvariantCulture) ?? "";
        parts["Second"] = value?.Seconds.ToString("D2", CultureInfo.InvariantCulture) ?? "";
    }

    private async Task EditAsync(string segment, string text)
    {
        if (Disabled || ReadOnly || disposed || !parts.ContainsKey(segment)) { return; }
        parts[segment] = text;
        await PublishAsync();
    }

    private async Task SetPeriodAsync(bool value)
    {
        if (Disabled || ReadOnly || disposed) { return; }
        pm = value;
        await PublishAsync();
    }

    private async Task PublishAsync()
    {
        var next = DateTimeSegments.Time(parts["Hour"], parts["Minute"], parts["Second"], ShowSeconds, Hour12, pm, MinTime, MaxTime);
        validation.SetError(next == null && Segments.Any(p => parts[p].Length > 0) ? Localizer["TimeInput.Invalid"] : null);
        var changed = Value != next;
        Value = lastValue = next;
        if (changed) { await ValueChanged.InvokeAsync(next); }
        validation.NotifyFieldChanged();
    }

    /// <summary>Clears every segment and the bound value.</summary>
    public async Task ClearAsync()
    {
        await InvokeAsync(async () =>
        {
            if (Disabled || ReadOnly || disposed) { return; }
            Synchronize(null);
            open = false;
            await PublishAsync();
            StateHasChanged();
        });
    }

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
