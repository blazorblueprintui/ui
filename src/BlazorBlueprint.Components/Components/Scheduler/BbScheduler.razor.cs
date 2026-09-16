using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorBlueprint.Primitives.Services;

namespace BlazorBlueprint.Components;

/// <summary>A resource scheduler with time slots, recurring appointments and an optional event editor.</summary>
public partial class BbScheduler
{
    [Parameter] public IReadOnlyList<SchedulerEvent> Events { get; set; } = [];
    [Parameter] public EventCallback<IReadOnlyList<SchedulerEvent>> EventsChanged { get; set; }
    [Parameter] public IReadOnlyList<SchedulerResource> Resources { get; set; } = [];
    [Parameter] public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Parameter] public EventCallback<DateOnly> DateChanged { get; set; }
    [Parameter] public SchedulerView View { get; set; } = SchedulerView.Week;
    [Parameter] public EventCallback<SchedulerView> ViewChanged { get; set; }
    /// <summary>First day in Week view. The toolbar offers Monday and Sunday; WorkWeek always shows Monday through Friday.</summary>
    [Parameter] public DayOfWeek FirstDayOfWeek { get; set; } = DayOfWeek.Monday;
    /// <summary>Raised when the week-start choice changes. Supports @bind-FirstDayOfWeek.</summary>
    [Parameter] public EventCallback<DayOfWeek> FirstDayOfWeekChanged { get; set; }
    /// <summary>The IANA time zone used for lane dates and time labels. Event zones remain independent.</summary>
    [Parameter] public string TimeZoneId { get; set; } = "UTC";
    /// <summary>Shows per-event time zones. When false, display and editor use TimeZoneId and zone controls are hidden. Stored instants and recurrence zones are preserved.</summary>
    [Parameter] public bool EnableTimeZones { get; set; } = true;
    /// <summary>Allows dragging appointments between time slots, days and resource lanes. Recurring changes apply to one occurrence.</summary>
    [Parameter] public bool AllowDrag { get; set; } = true;
    /// <summary>Allows resizing the top and bottom edges of appointments, snapped to SlotMinutes.</summary>
    [Parameter] public bool AllowResize { get; set; } = true;
    /// <summary>Time-slot size and drag/resize snapping interval, in minutes. Supports 15, 30, 60 and other divisors of 1440 between 5 and 120.</summary>
    [Parameter] public int SlotMinutes { get; set; } = 30;
    [Parameter] public int StartHour { get; set; } = 8;
    [Parameter] public int EndHour { get; set; } = 18;
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment<SchedulerOccurrence>? EventTemplate { get; set; }
    /// <summary>Runs before EventsChanged. Set Cancel, or throw, to retain the editor and original events.</summary>
    [Parameter] public EventCallback<SchedulerChangeContext> OnEventChange { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly string editorId = $"bb-scheduler-{Guid.NewGuid():N}";
    private IReadOnlyList<SchedulerOccurrence> occurrences = [];
    private List<Lane> lanes = [];
    private SchedulerEvent? draft;
    private SchedulerOccurrence? editingOccurrence;
    private SchedulerEditScope editScope = SchedulerEditScope.Occurrence;
    private SchedulerAmbiguousTimeResolution startResolution;
    private SchedulerAmbiguousTimeResolution endResolution;
    private DateTime? localStart;
    private DateTime? localEnd;
    private static readonly string[] TimeZoneIds = [.. NodaTime.DateTimeZoneProviders.Tzdb.Ids
        .Where(id => TimeZoneInfo.TryFindSystemTimeZoneById(id, out _)).Order(StringComparer.Ordinal)];
    private static readonly string[] RecurrenceFrequencies = ["NONE", "DAILY", "WEEKLY", "MONTHLY", "YEARLY", "CUSTOM"];
    private bool limitRecurrence;
    private string recurrenceFrequency = "NONE";
    private int recurrenceInterval = 1;
    private int? recurrenceCount;
    private bool editorOpen;
    private bool deleteConfirmationOpen;
    private bool restoreDeleteFocus;
    private bool saving;
    private string? editorError;
    private string? loadError;
    private ElementReference schedulerElement;
    private IJSObjectReference? interactionModule;
    private DotNetObjectReference<BbScheduler>? dotNetReference;
    private bool disposed;
    private int revision;

    private DayOfWeek EffectiveWeekStart => View == SchedulerView.WorkWeek ? DayOfWeek.Monday : FirstDayOfWeek;
    private DateOnly RangeDate => View == SchedulerView.Day ? Date
        : Date.AddDays(-(((int)Date.DayOfWeek - (int)EffectiveWeekStart + 7) % 7));
    private int DayCount => View switch { SchedulerView.Day => 1, SchedulerView.WorkWeek => 5, _ => 7 };
    private TimeZoneInfo DisplayZone => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    private string EditorTimeZoneId => EnableTimeZones ? draft!.TimeZoneId : TimeZoneId;
    private string DeleteDescription => Localizer[editingOccurrence != null && !string.IsNullOrWhiteSpace(editingOccurrence.Event.RecurrenceRule)
        ? editScope == SchedulerEditScope.Series ? "Scheduler.DeleteSeriesDescription" : "Scheduler.DeleteOccurrenceDescription"
        : "Scheduler.DeleteDescription", editingOccurrence?.Event.Title ?? ""];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (disposed)
        {
            return;
        }
        if (firstRender)
        {
            interactionModule = await JsModules.GetAsync(JSRuntime, JsModules.Versioned("./_content/BlazorBlueprint.Components/js/scheduler.js", typeof(BbScheduler).Assembly));
            if (disposed)
            {
                return;
            }
            dotNetReference = DotNetObjectReference.Create(this);
            await interactionModule.InvokeVoidAsync("initialize", schedulerElement, dotNetReference);
        }
        if (restoreDeleteFocus && interactionModule != null)
        {
            restoreDeleteFocus = false;
            await interactionModule.InvokeVoidAsync("restoreDeleteFocus", editorId);
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        disposed = true;
        if (interactionModule != null)
        {
            try { await interactionModule.InvokeVoidAsync("dispose", schedulerElement); }
            catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException) { }
        }
        dotNetReference?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnParametersSet()
    {
        if (SlotMinutes < 5 || SlotMinutes > 120 || 1440 % SlotMinutes != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(SlotMinutes), "Use a divisor of 1440 between 5 and 120 minutes.");
        }
        if (StartHour < 0 || EndHour > 24 || EndHour <= StartHour)
        {
            throw new ArgumentOutOfRangeException(nameof(StartHour), "Use 0 <= StartHour < EndHour <= 24.");
        }
        if (Resources.Any(r => string.IsNullOrWhiteSpace(r.Id)) || Resources.Select(r => r.Id).Distinct(StringComparer.Ordinal).Count() != Resources.Count)
        {
            throw new ArgumentException("Resource IDs must be unique.", nameof(Resources));
        }
        RefreshSchedule();
    }

    private void RefreshSchedule()
    {
        revision++;
        loadError = null;
        try
        {
            var start = DayBoundary(RangeDate, 0);
            var end = DayBoundary(RangeDate.AddDays(DayCount), 0);
            occurrences = SchedulerEngine.Expand(Events, start, end);
            lanes = [];
            for (var day = 0; day < DayCount; day++)
            {
                var date = RangeDate.AddDays(day);
                var laneStart = DayBoundary(date, StartHour);
                var laneEnd = DayBoundary(date, EndHour);
                if (Resources.Count == 0)
                {
                    lanes.Add(CreateLane(date, null, laneStart, laneEnd));
                }
                else
                {
                    foreach (var resource in Resources)
                    {
                        lanes.Add(CreateLane(date, resource, laneStart, laneEnd));
                    }
                    if (occurrences.Any(o => IsUnassigned(o.Event)))
                    {
                        lanes.Add(CreateLane(date, new SchedulerResource(string.Empty, Localizer["Scheduler.Unassigned"]), laneStart, laneEnd));
                    }
                }
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or FormatException or TimeZoneNotFoundException or InvalidTimeZoneException or Ical.Net.Evaluation.EvaluationException)
        {
            lanes = [];
            loadError = Localizer["Scheduler.InvalidEvents"];
        }
    }

    private DateTimeOffset DayBoundary(DateOnly date, int hour)
    {
        var local = date.ToDateTime(TimeOnly.MinValue).AddHours(hour);
        // A few zones advance at midnight. Start the lane at the first valid wall minute.
        while (DisplayZone.IsInvalidTime(local))
        {
            local = local.AddMinutes(1);
        }
        return SchedulerEngine.ToInstant(local, TimeZoneId);
    }

    private bool IsUnassigned(SchedulerEvent item) => !item.ResourceIds.Any(id => Resources.Any(r => r.Id == id));

    private Lane CreateLane(DateOnly date, SchedulerResource? resource, DateTimeOffset start, DateTimeOffset end)
    {
        var items = occurrences.Where(o => o.Start < end && o.End > start
            && (resource == null || (resource.Id.Length == 0 ? IsUnassigned(o.Event) : o.Event.ResourceIds.Contains(resource.Id))))
            .OrderBy(o => o.Start).ThenByDescending(o => o.End).ToList();
        var placements = new List<Placement>();
        var group = new List<Placement>();
        var columns = new List<DateTimeOffset>();
        var groupEnd = DateTimeOffset.MinValue;
        foreach (var item in items)
        {
            if (item.Start >= groupEnd)
            {
                FinishGroup();
            }
            var column = columns.FindIndex(e => e <= item.Start);
            if (column < 0)
            {
                column = columns.Count;
                columns.Add(item.End);
            }
            else
            {
                columns[column] = item.End;
            }
            group.Add(new Placement(item, column, 1));
            if (item.End > groupEnd)
            {
                groupEnd = item.End;
            }
        }
        FinishGroup();
        return new Lane(date, resource, start, end, placements);

        void FinishGroup()
        {
            placements.AddRange(group.Select(p => p with { Columns = columns.Count }));
            group.Clear();
            columns.Clear();
            groupEnd = DateTimeOffset.MinValue;
        }
    }

    private string PlacementStyle(Lane lane, Placement placement)
    {
        var from = placement.Occurrence.Start > lane.Start ? placement.Occurrence.Start : lane.Start;
        var to = placement.Occurrence.End < lane.End ? placement.Occurrence.End : lane.End;
        // Inset cards within their time/overlap allocation so neighboring events stay separate.
        var top = ((from - lane.Start).TotalMinutes / SlotMinutes * 40) + 2;
        var height = Math.Max(20, (to - from).TotalMinutes / SlotMinutes * 40) - 4;
        var left = 100.0 * placement.Column / placement.Columns;
        var width = 100.0 / placement.Columns;
        return FormattableString.Invariant($"top:{top}px;height:{height}px;left:calc(4.5rem + (100% - 4.5rem) * {left / 100} + 4px);width:calc((100% - 4.5rem) * {width / 100} - 8px);");
    }

    private string TimeLabel(DateTimeOffset time) => TimeZoneInfo.ConvertTime(time, DisplayZone).ToString("t", CultureInfo.CurrentCulture);
    private string OccurrenceLabel(SchedulerOccurrence item) => $"{item.Event.Title}, {TimeZoneInfo.ConvertTime(item.Start, DisplayZone):f} – {TimeZoneInfo.ConvertTime(item.End, DisplayZone):t}";

    private async Task NavigateAsync(int direction)
    {
        // Both week views advance whole weeks, even when only five days are displayed.
        Date = Date.AddDays(direction * (View == SchedulerView.Day ? 1 : 7));
        RefreshSchedule();
        await DateChanged.InvokeAsync(Date);
    }

    private async Task SetViewAsync(SchedulerView view)
    {
        View = view;
        RefreshSchedule();
        await ViewChanged.InvokeAsync(view);
    }

    private string WeekStartLabel(DayOfWeek day) => Localizer["Scheduler.WeekStartDay", CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(day)];

    private async Task SetFirstDayOfWeekAsync(DayOfWeek day)
    {
        if (FirstDayOfWeek == day)
        {
            return;
        }
        FirstDayOfWeek = day;
        RefreshSchedule();
        await FirstDayOfWeekChanged.InvokeAsync(day);
    }

    /// <summary>Opens the editor for a new appointment at an instant.</summary>
    public void CreateEvent(DateTimeOffset start, string? resourceId = null)
    {
        if (ReadOnly || saving)
        {
            return;
        }
        editingOccurrence = null;
        editScope = SchedulerEditScope.Series;
        draft = new SchedulerEvent { Start = start, End = start.AddMinutes(SlotMinutes), TimeZoneId = TimeZoneId,
            ResourceIds = string.IsNullOrEmpty(resourceId) ? [] : [resourceId] };
        OpenEditor();
    }

    /// <summary>Opens an independent editor copy for one occurrence or its series.</summary>
    public void EditEvent(SchedulerOccurrence occurrence, SchedulerEditScope scope = SchedulerEditScope.Occurrence)
    {
        ArgumentNullException.ThrowIfNull(occurrence);
        if (ReadOnly || saving)
        {
            return;
        }
        editingOccurrence = occurrence;
        editScope = scope;
        SetDraftForScope();
        OpenEditor();
    }

    private void SetDraftForScope()
    {
        draft = editingOccurrence!.Event.Clone();
        if (editScope == SchedulerEditScope.Occurrence)
        {
            draft.Start = editingOccurrence.Start;
            draft.End = editingOccurrence.End;
        }
        UpdateLocalTimes();
        ReadRecurrence();
    }

    private void ScopeChanged(SchedulerEditScope value)
    {
        editScope = value;
        SetDraftForScope();
    }

    private void UpdateLocalTimes()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(EditorTimeZoneId);
        localStart = TimeZoneInfo.ConvertTime(draft!.Start, zone).DateTime;
        localEnd = TimeZoneInfo.ConvertTime(draft.End, zone).DateTime;
        startResolution = ResolutionFor(draft.Start, zone);
        endResolution = ResolutionFor(draft.End, zone);
    }

    private string ResolutionLabel(SchedulerAmbiguousTimeResolution value) =>
        Localizer[value == SchedulerAmbiguousTimeResolution.Earlier ? "Scheduler.EarlierOffset" : "Scheduler.LaterOffset"];

    private bool IsAmbiguousTime(DateTime? local)
    {
        if (!local.HasValue || draft == null)
        {
            return false;
        }
        return TimeZoneInfo.FindSystemTimeZoneById(EditorTimeZoneId).IsAmbiguousTime(DateTime.SpecifyKind(local.Value, DateTimeKind.Unspecified));
    }

    private static SchedulerAmbiguousTimeResolution ResolutionFor(DateTimeOffset instant, TimeZoneInfo zone)
    {
        var local = TimeZoneInfo.ConvertTime(instant, zone);
        return zone.IsAmbiguousTime(local.DateTime) && local.Offset == zone.GetAmbiguousTimeOffsets(local.DateTime).Min()
            ? SchedulerAmbiguousTimeResolution.Later : SchedulerAmbiguousTimeResolution.Earlier;
    }

    private void ReadRecurrence()
    {
        recurrenceFrequency = string.IsNullOrWhiteSpace(draft!.RecurrenceRule) ? "NONE" : "CUSTOM";
        recurrenceInterval = 1;
        recurrenceCount = 10;
        limitRecurrence = false;
    }

    private void FrequencyChanged(string? value)
    {
        recurrenceFrequency = value ?? "NONE";
        UpdateRecurrence();
    }

    private void UpdateRecurrence()
    {
        if (recurrenceFrequency == "NONE")
        {
            draft!.RecurrenceRule = null;
        }
        else if (recurrenceFrequency != "CUSTOM")
        {
            draft!.RecurrenceRule = FormattableString.Invariant($"FREQ={recurrenceFrequency};INTERVAL={recurrenceInterval}")
                + (limitRecurrence ? FormattableString.Invariant($";COUNT={recurrenceCount ?? 10}") : "");
        }
    }

    private void OpenEditor()
    {
        UpdateLocalTimes();
        ReadRecurrence();
        editorError = null;
        deleteConfirmationOpen = false;
        editorOpen = true;
        StateHasChanged();
    }

    private void ToggleResource(string resourceId, bool selected)
    {
        if (selected && !draft!.ResourceIds.Contains(resourceId))
        {
            draft.ResourceIds.Add(resourceId);
        }
        else if (!selected)
        {
            draft!.ResourceIds.Remove(resourceId);
        }
    }

    private async Task SaveAsync(bool delete)
    {
        if (draft == null || saving || ReadOnly)
        {
            return;
        }
        editorError = null;
        saving = true;
        StateHasChanged();
        try
        {
            if (!delete)
            {
                if (!localStart.HasValue || !localEnd.HasValue)
                {
                    throw new ArgumentException("Start and end are required.");
                }
                draft.Start = SchedulerEngine.ToInstant(localStart.Value, EditorTimeZoneId, startResolution);
                draft.End = SchedulerEngine.ToInstant(localEnd.Value, EditorTimeZoneId, endResolution);
                SchedulerEngine.Validate(draft);
            }
            var kind = delete ? SchedulerChangeKind.Delete : editingOccurrence == null ? SchedulerChangeKind.Create : SchedulerChangeKind.Update;
            var proposed = SchedulerEngine.ApplyChange(Events, draft, kind, editScope, editingOccurrence?.Start);
            _ = SchedulerEngine.Expand(proposed, DayBoundary(RangeDate, 0), DayBoundary(RangeDate.AddDays(DayCount), 0));
            var context = new SchedulerChangeContext { Kind = kind, Scope = editScope, Event = draft.Clone(),
                OccurrenceStart = editingOccurrence?.Start, Events = proposed };
            await OnEventChange.InvokeAsync(context);
            if (context.Cancel)
            {
                editorError = Localizer["Scheduler.SaveRejected"];
                return;
            }
            await EventsChanged.InvokeAsync(proposed);
            Events = proposed;
            editorOpen = false;
            deleteConfirmationOpen = false;
            RefreshSchedule();
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or TimeZoneNotFoundException or InvalidTimeZoneException or Ical.Net.Evaluation.EvaluationException)
        {
            editorError = Localizer["Scheduler.InvalidEdit"];
        }
        catch (Exception)
        {
            editorError = Localizer["Scheduler.SaveFailed"];
        }
        finally
        {
            saving = false;
        }
    }

    private void SetEditorOpen(bool value)
    {
        if (!saving)
        {
            editorOpen = value;
            if (!value)
            {
                deleteConfirmationOpen = false;
            }
        }
    }

    private void RequestDelete()
    {
        if (editingOccurrence == null || saving || ReadOnly)
        {
            return;
        }
        editorError = null;
        deleteConfirmationOpen = true;
    }

    private void SetDeleteConfirmationOpen(bool value)
    {
        if (!saving)
        {
            restoreDeleteFocus = deleteConfirmationOpen && !value && editorOpen;
            deleteConfirmationOpen = value;
        }
    }

    private Task ConfirmDeleteAsync() => deleteConfirmationOpen ? SaveAsync(true) : Task.CompletedTask;

    /// <summary>Commits a browser drag/resize after checking the visible occurrence, lane and snapping bounds. Applications normally use the pointer UI or EditEvent.</summary>
    [JSInvokable]
    public async Task CommitInteractionAsync(int renderedRevision, string eventId, long occurrenceStart, int sourceLaneIndex,
        int targetLaneIndex, long startMilliseconds, long endMilliseconds, string action)
    {
        if (disposed || ReadOnly || saving || editorOpen || renderedRevision != revision
            || (action == "move" ? !AllowDrag : !AllowResize || action is not ("start" or "end"))
            || sourceLaneIndex < 0 || sourceLaneIndex >= lanes.Count || targetLaneIndex < 0 || targetLaneIndex >= lanes.Count)
        {
            return;
        }
        var source = lanes[sourceLaneIndex];
        var target = lanes[targetLaneIndex];
        var occurrence = source.Placements.Select(p => p.Occurrence).FirstOrDefault(o => o.Event.Id == eventId && o.Start.ToUnixTimeMilliseconds() == occurrenceStart);
        if (occurrence == null)
        {
            return;
        }
        var originalStart = occurrence.Start.ToUnixTimeMilliseconds();
        var originalEnd = occurrence.End.ToUnixTimeMilliseconds();
        var slot = SlotMinutes * 60_000L;
        var laneStart = target.Start.ToUnixTimeMilliseconds();
        var laneEnd = target.End.ToUnixTimeMilliseconds();
        if (startMilliseconds >= endMilliseconds)
        {
            return;
        }
        if (action == "move")
        {
            var earliest = laneStart - Math.Max(0, source.Start.ToUnixTimeMilliseconds() - originalStart);
            if (startMilliseconds < earliest || startMilliseconds >= laneEnd || endMilliseconds <= laneStart
                || endMilliseconds > DateTimeOffset.MaxValue.ToUnixTimeMilliseconds() || startMilliseconds < DateTimeOffset.MinValue.ToUnixTimeMilliseconds()
                || endMilliseconds - startMilliseconds != originalEnd - originalStart
                || (startMilliseconds - laneStart) % slot != 0)
            {
                return;
            }
        }
        else
        {
            if (sourceLaneIndex != targetLaneIndex || endMilliseconds - startMilliseconds < slot)
            {
                return;
            }
            var edge = action == "start" ? startMilliseconds : endMilliseconds;
            if (edge < laneStart || edge > laneEnd || (edge - laneStart) % slot != 0
                || (action == "start" ? endMilliseconds != originalEnd || originalStart < laneStart : startMilliseconds != originalStart || originalEnd > laneEnd))
            {
                return;
            }
        }
        if (originalStart == startMilliseconds && originalEnd == endMilliseconds && sourceLaneIndex == targetLaneIndex)
        {
            return;
        }
        editingOccurrence = occurrence;
        editScope = SchedulerEditScope.Occurrence;
        draft = occurrence.Event.Clone();
        draft.Start = DateTimeOffset.FromUnixTimeMilliseconds(startMilliseconds);
        draft.End = DateTimeOffset.FromUnixTimeMilliseconds(endMilliseconds);
        if (action == "move" && source.Resource?.Id != target.Resource?.Id)
        {
            if (string.IsNullOrEmpty(target.Resource?.Id))
            {
                draft.ResourceIds.Clear();
            }
            else
            {
                if (source.Resource != null)
                {
                    draft.ResourceIds.Remove(source.Resource.Id);
                }
                if (!draft.ResourceIds.Contains(target.Resource.Id))
                {
                    draft.ResourceIds.Add(target.Resource.Id);
                }
            }
        }
        UpdateLocalTimes();
        ReadRecurrence();
        await SaveAsync(false);
        if (editorError != null)
        {
            editorOpen = true;
        }
        StateHasChanged();
    }

    private sealed record Lane(DateOnly Date, SchedulerResource? Resource, DateTimeOffset Start, DateTimeOffset End, List<Placement> Placements);
    private sealed record Placement(SchedulerOccurrence Occurrence, int Column, int Columns);
}
