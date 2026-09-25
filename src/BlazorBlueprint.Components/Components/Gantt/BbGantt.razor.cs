using BlazorBlueprint.Primitives.DataGrid;
using BlazorBlueprint.Primitives.Gantt;
using BlazorBlueprint.Primitives.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlueprint.Components;

/// <summary>
/// A plan drawn against a timeline: a task list down the side, a bar for every task across from it,
/// and arrows for what has to happen first.
/// </summary>
/// <typeparam name="TItem">The type of the source items.</typeparam>
/// <remarks>
/// <para>
/// The task list and the timeline are one table, not two panes kept in step. A Gantt is read across
/// the row — this name, that bar — and two scrolling panes can only ever agree about where a row is
/// by measuring each other. Sharing the row means they cannot disagree.
/// </para>
/// <para>
/// Every position is worked out in C# from the column widths and the slot width, so the chart draws
/// the same on the server, in WebAssembly and in a print stylesheet. Nothing is measured in the
/// browser.
/// </para>
/// <para>
/// A task with children is a summary: it takes its dates and its progress from the work underneath
/// rather than carrying its own. A task with no length is a milestone and is drawn as a marker.
/// </para>
/// </remarks>
public partial class BbGantt<TItem> : ComponentBase, IAsyncDisposable
{
    private readonly List<BbGanttColumn<TItem>> columns = [];

    /// <summary>
    /// The widths a drag has overridden, by column key.
    /// </summary>
    /// <remarks>
    /// Kept here rather than on the declarations: a child's parameter belongs to whoever wrote the
    /// markup, so setting it from out here would be undone the next time the parent renders.
    /// </remarks>
    private readonly Dictionary<string, int> widths = new(StringComparer.Ordinal);

    private readonly string chartId = $"bb-gantt-{Guid.NewGuid():N}";

    private HashSet<string> collapsed = new(StringComparer.Ordinal);
    private GanttChart<TItem>? chart;
    private ElementReference rootElement;
    private DotNetObjectReference<BbGantt<TItem>>? selfRef;
    private IJSObjectReference? columnsModule;
    private IJSObjectReference? ganttModule;
    private bool jsReady;
    private bool seeded;
    private bool stale = true;
    private bool scrolled;

    // Set when a rebuild has changed the chart but the DOM still shows the previous one. The
    // JavaScript is wired up on the pass after that, not on the pass that rebuilt.
    private bool redrawPending;
    private string? buildError;
    private string? sortKey;
    private bool sortDescending;

    /// <summary>Gets or sets the tasks, in any order.</summary>
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>Gets or sets the reader for a task's identifier. Must be unique across the set.</summary>
    [Parameter]
    [EditorRequired]
    public Func<TItem, string> IdSelector { get; set; } = default!;

    /// <summary>Gets or sets the reader for a task's name.</summary>
    [Parameter]
    [EditorRequired]
    public Func<TItem, string> TextSelector { get; set; } = default!;

    /// <summary>Gets or sets the reader for a task's start.</summary>
    [Parameter]
    [EditorRequired]
    public Func<TItem, DateTimeOffset> StartSelector { get; set; } = default!;

    /// <summary>Gets or sets the reader for a task's end.</summary>
    [Parameter]
    [EditorRequired]
    public Func<TItem, DateTimeOffset> EndSelector { get; set; } = default!;

    /// <summary>
    /// Gets or sets the reader for the identifier of the task this one sits under. Leave it unset
    /// for a flat plan.
    /// </summary>
    [Parameter]
    public Func<TItem, string?>? ParentIdSelector { get; set; }

    /// <summary>Gets or sets the reader for how far along a task is, from 0 to 1.</summary>
    [Parameter]
    public Func<TItem, double>? ProgressSelector { get; set; }

    /// <summary>
    /// Gets or sets the reader for a bar's colour, as any CSS colour. Null uses the theme's.
    /// </summary>
    [Parameter]
    public Func<TItem, string?>? BarColorSelector { get; set; }

    /// <summary>
    /// Gets or sets the columns of the task list. Leave it unset and the chart shows one column of
    /// task names.
    /// </summary>
    [Parameter]
    public RenderFragment? Columns { get; set; }

    /// <summary>Gets or sets the arrows drawn between tasks.</summary>
    [Parameter]
    public IEnumerable<GanttDependency>? Dependencies { get; set; }

    /// <summary>Gets or sets whether the dependency arrows are drawn.</summary>
    [Parameter]
    public bool ShowDependencies { get; set; } = true;

    /// <summary>Gets or sets how much time one slot of the timeline holds.</summary>
    [Parameter]
    public GanttZoom Zoom { get; set; } = GanttZoom.Day;

    /// <summary>Gets or sets the callback fired when the zoom changes.</summary>
    [Parameter]
    public EventCallback<GanttZoom> ZoomChanged { get; set; }

    /// <summary>Gets or sets whether the toolbar is shown.</summary>
    [Parameter]
    public bool ShowToolbar { get; set; } = true;

    /// <summary>Gets or sets whether the toolbar offers the zoom levels.</summary>
    [Parameter]
    public bool ShowZoomControls { get; set; } = true;

    /// <summary>
    /// Gets or sets how wide one slot is drawn, in pixels. Null takes a width from the zoom.
    /// </summary>
    [Parameter]
    public int? SlotWidth { get; set; }

    /// <summary>Gets or sets an explicit left edge for the timeline.</summary>
    [Parameter]
    public DateTimeOffset? RangeStart { get; set; }

    /// <summary>Gets or sets an explicit right edge for the timeline.</summary>
    [Parameter]
    public DateTimeOffset? RangeEnd { get; set; }

    /// <summary>Gets or sets how far past the tasks the timeline is widened.</summary>
    [Parameter]
    public GanttRangeSnap RangeSnap { get; set; } = GanttRangeSnap.Major;

    /// <summary>Gets or sets the days shaded as non-working. Null shades Saturday and Sunday.</summary>
    [Parameter]
    public IReadOnlyCollection<DayOfWeek>? NonWorkingDays { get; set; }

    /// <summary>Gets or sets whether non-working days are shaded.</summary>
    [Parameter]
    public bool ShowNonWorkingDays { get; set; } = true;

    /// <summary>Gets or sets whether a line marks the current instant.</summary>
    [Parameter]
    public bool ShowToday { get; set; } = true;

    /// <summary>Gets or sets whether the chart scrolls to today the first time it is drawn.</summary>
    [Parameter]
    public bool ScrollToToday { get; set; } = true;

    /// <summary>Gets or sets how tall one row is drawn, in pixels.</summary>
    [Parameter]
    public int RowHeight { get; set; } = 36;

    /// <summary>Gets or sets how tall one tier of the header is drawn, in pixels.</summary>
    [Parameter]
    public int HeaderRowHeight { get; set; } = 30;

    /// <summary>
    /// Gets or sets the identifiers of the tasks whose children are hidden.
    /// </summary>
    /// <remarks>
    /// The closed set is named rather than the open one, unlike <c>BbDataGrid</c>'s
    /// <c>ExpandedNodes</c>: a plan is read open, so an empty set is the state a reader wants first.
    /// </remarks>
    [Parameter]
    public HashSet<string>? CollapsedIds { get; set; }

    /// <summary>Gets or sets the callback fired when a branch is opened or closed.</summary>
    [Parameter]
    public EventCallback<HashSet<string>> CollapsedIdsChanged { get; set; }

    /// <summary>Gets or sets whether every branch starts open. Set it false to start folded up.</summary>
    [Parameter]
    public bool DefaultExpandAll { get; set; } = true;

    /// <summary>Gets or sets whether a summary takes its dates and progress from its children.</summary>
    [Parameter]
    public bool RollUpSummaries { get; set; } = true;

    /// <summary>Gets or sets whether a bar can be dragged along the timeline.</summary>
    [Parameter]
    public bool AllowDrag { get; set; }

    /// <summary>Gets or sets whether a bar's edges can be dragged to change its length.</summary>
    [Parameter]
    public bool AllowResize { get; set; }

    /// <summary>
    /// Gets or sets whether the fill inside a bar can be dragged to set how far along a task is.
    /// </summary>
    [Parameter]
    public bool AllowProgressDrag { get; set; }

    /// <summary>
    /// Gets or sets whether a dependency can be drawn by dragging from one bar to another.
    /// </summary>
    /// <remarks>
    /// The end you drag from and the end you drop on decide the type between them, so all four
    /// come out of the same gesture rather than out of a menu asking which one you meant.
    /// </remarks>
    [Parameter]
    public bool AllowLinking { get; set; }

    /// <summary>
    /// Gets or sets the callback fired when a dependency is drawn. Set <c>Cancel</c> to refuse it.
    /// </summary>
    /// <remarks>
    /// Like a task drag, this asks rather than writes: the arrow appears once the new dependency is
    /// in the collection the chart was given.
    /// </remarks>
    [Parameter]
    public EventCallback<GanttDependencyContext<TItem>> OnDependencyCreate { get; set; }

    /// <summary>
    /// Gets or sets the callback fired when an arrow is clicked, which is how one gets deleted.
    /// </summary>
    /// <remarks>
    /// Only reachable while <see cref="AllowLinking"/> is on, because an arrow nobody can change is
    /// an arrow nobody should be able to click by accident.
    /// </remarks>
    [Parameter]
    public EventCallback<GanttDependency> OnDependencyClick { get; set; }

    /// <summary>
    /// Gets or sets whether a drag lands on whole slots.
    /// </summary>
    /// <remarks>
    /// On by default. A plan agreed in days should not come back with a start at 09:47 because
    /// that is where the pointer was let go.
    /// </remarks>
    [Parameter]
    public bool SnapToSlot { get; set; } = true;

    /// <summary>
    /// Gets or sets the callback fired when a bar is dragged. Set <c>Cancel</c> to refuse it.
    /// </summary>
    /// <remarks>
    /// The chart does not write to the task. Store the new dates and hand back a changed
    /// collection, or the bar snaps straight back.
    /// </remarks>
    [Parameter]
    public EventCallback<GanttChangeContext<TItem>> OnTaskChange { get; set; }

    /// <summary>Gets or sets the callback fired when a task is clicked, in the list or on the bar.</summary>
    [Parameter]
    public EventCallback<TItem> OnTaskClick { get; set; }

    /// <summary>
    /// Gets or sets whether a key to the shapes is shown under the chart.
    /// </summary>
    /// <remarks>
    /// On by default, and it only lists what the chart actually draws: no milestone entry where
    /// there are no milestones, no shading entry where nothing is shaded.
    /// </remarks>
    [Parameter]
    public bool ShowLegend { get; set; } = true;

    /// <summary>
    /// Gets or sets whether hovering a bar shows a card with its dates, length and progress.
    /// </summary>
    /// <remarks>
    /// The card is plain markup shown by CSS on hover, not a floating overlay. A Gantt can hold
    /// hundreds of bars, and on Blazor Server asking the circuit what to show on every pointer-over
    /// is a card that arrives after the pointer has moved on.
    /// </remarks>
    [Parameter]
    public bool ShowTooltip { get; set; } = true;

    /// <summary>Gets or sets what the hover card holds, replacing the default lines.</summary>
    [Parameter]
    public RenderFragment<GanttRow<TItem>>? TooltipTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether a row can be dragged up and down the task list.
    /// </summary>
    /// <remarks>
    /// Dropping between two rows reorders; dropping onto the middle of one re-parents. Both arrive
    /// as <see cref="OnTaskMove"/>, because only the caller knows whether the order it keeps is a
    /// list, a sort field or a column in a database.
    /// </remarks>
    [Parameter]
    public bool AllowRowDrag { get; set; }

    /// <summary>
    /// Gets or sets the callback fired when a row is dropped somewhere else. Set <c>Cancel</c> to
    /// refuse it.
    /// </summary>
    [Parameter]
    public EventCallback<GanttMoveContext<TItem>> OnTaskMove { get; set; }

    /// <summary>Gets or sets what is drawn inside a bar, replacing the default fill.</summary>
    [Parameter]
    public RenderFragment<GanttRow<TItem>>? BarTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether a task's name is written beside its bar.
    /// </summary>
    /// <remarks>
    /// Off by default. The task list is pinned to the side, so the name is already on the row and a
    /// second copy of it is the first thing to get in the way of the bars.
    /// </remarks>
    [Parameter]
    public bool ShowTaskLabels { get; set; }

    /// <summary>Gets or sets whether a column's edge can be dragged to change its width.</summary>
    [Parameter]
    public bool Resizable { get; set; } = true;

    /// <summary>Gets or sets how narrow a dragged column may get, in pixels.</summary>
    [Parameter]
    public int MinColumnWidth { get; set; } = 60;

    /// <summary>Gets or sets the height of the scrolling area, as a CSS length. Null lets it grow.</summary>
    [Parameter]
    public string? Height { get; set; }

    /// <summary>Gets or sets whether a loading indicator replaces the chart.</summary>
    [Parameter]
    public bool Loading { get; set; }

    /// <summary>Gets or sets the message shown when there is nothing to draw.</summary>
    [Parameter]
    public string? EmptyMessage { get; set; }

    /// <summary>Gets or sets the label a screen reader announces for the chart.</summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>Gets or sets extra classes for the root element.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the callback fired after the chart is built.
    /// </summary>
    /// <remarks>
    /// Reading <see cref="Chart"/> through <c>@ref</c> in the parent's markup is always one render
    /// behind, because the parent's markup is written before this component has run. Use this.
    /// </remarks>
    [Parameter]
    public EventCallback<GanttChart<TItem>?> OnBuilt { get; set; }

    /// <summary>Gets or sets attributes splatted onto the root element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    /// <summary>
    /// Gets the chart as it was last built, or null where there was nothing to build.
    /// </summary>
    public GanttChart<TItem>? Chart => chart;

    /// <summary>Gets the columns of the task list, in the order they were declared.</summary>
    internal IReadOnlyList<BbGanttColumn<TItem>> DeclaredColumns => columns;

    /// <summary>Gets the column that carries the expander and the indent.</summary>
    internal BbGanttColumn<TItem>? TreeColumn =>
        columns.FirstOrDefault(c => c.IsTree) ?? columns.FirstOrDefault();

    /// <summary>Gets how wide the task list is, in pixels.</summary>
    internal int TreeWidth => columns.Sum(c => c.EffectiveWidth);

    /// <summary>Gets how wide one slot is drawn, in pixels.</summary>
    internal int EffectiveSlotWidth => SlotWidth ?? Zoom switch
    {
        GanttZoom.Hour => 36,
        GanttZoom.Day => 34,
        GanttZoom.Week => 58,
        GanttZoom.Month => 74,
        GanttZoom.Quarter => 82,
        _ => 88,
    };

    /// <summary>Gets whether any gesture on a bar is switched on.</summary>
    private bool Interactive => AllowDrag || AllowResize || AllowProgressDrag || AllowLinking || AllowRowDrag;

    /// <summary>Gets the width a column is drawn at, which a drag on its edge overrides.</summary>
    /// <param name="column">The column.</param>
    /// <returns>The width in pixels.</returns>
    internal int WidthOf(BbGanttColumn<TItem> column) =>
        widths.TryGetValue(column.EffectiveKey, out var width) ? width : column.Width;

    /// <summary>Adds a column declared in this chart's <c>Columns</c>.</summary>
    /// <param name="column">The column.</param>
    internal void Register(BbGanttColumn<TItem> column)
    {
        columns.Add(column);
        Invalidate();
    }

    /// <summary>Takes a column back out.</summary>
    /// <param name="column">The column.</param>
    internal void Unregister(BbGanttColumn<TItem> column)
    {
        columns.Remove(column);
        Invalidate();
    }

    /// <summary>
    /// Marks the chart as needing to be built again.
    /// </summary>
    /// <remarks>
    /// Building is not free, so it happens once before the next render rather than every time a
    /// child reports a changed parameter.
    /// </remarks>
    internal void Invalidate()
    {
        stale = true;
        StateHasChanged();
    }

    /// <summary>Opens or closes a branch.</summary>
    /// <param name="id">The identifier of the task to open or close.</param>
    internal async Task ToggleAsync(string id)
    {
        if (!collapsed.Remove(id))
        {
            collapsed.Add(id);
        }

        await NotifyCollapsedAsync();
    }

    /// <summary>Opens every branch.</summary>
    public async Task ExpandAllAsync()
    {
        collapsed.Clear();
        await NotifyCollapsedAsync();
    }

    /// <summary>Closes every branch.</summary>
    public async Task CollapseAllAsync()
    {
        collapsed = Summaries();
        await NotifyCollapsedAsync();
    }

    /// <summary>Changes how much time one slot holds.</summary>
    /// <param name="zoom">The new zoom.</param>
    public async Task SetZoomAsync(GanttZoom zoom)
    {
        if (Zoom == zoom)
        {
            return;
        }

        Zoom = zoom;
        scrolled = false;
        Invalidate();

        if (ZoomChanged.HasDelegate)
        {
            await ZoomChanged.InvokeAsync(zoom);
        }
    }

    /// <summary>Scrolls the timeline so that the current instant is in view.</summary>
    public async Task GoToTodayAsync() => await ScrollToAsync(DateTimeOffset.Now);

    /// <summary>Scrolls the timeline so that an instant is in view.</summary>
    /// <param name="value">The instant to scroll to.</param>
    public async Task ScrollToAsync(DateTimeOffset value)
    {
        if (chart is null || ganttModule is null)
        {
            return;
        }

        var x = TreeWidth + (chart.Axis.Position(value) * EffectiveSlotWidth);
        await ganttModule.InvokeVoidAsync("scrollTo", rootElement, x, TreeWidth);
    }

    /// <summary>
    /// Records a column width a drag settled on. Called from JavaScript.
    /// </summary>
    /// <param name="columnId">The key of the column that was dragged.</param>
    /// <param name="dragged">Every managed column's width at the end of the drag.</param>
    /// <remarks>
    /// The drag reports every column, not only the one dragged, so a width that is not a width is
    /// refused here rather than recorded: the column keeps the width it had.
    /// </remarks>
    [JSInvokable]
    public void OnResizeCompleted(string columnId, Dictionary<string, double> dragged)
    {
        ArgumentNullException.ThrowIfNull(dragged);

        foreach (var (key, width) in dragged)
        {
            if (!ColumnWidth.IsUsablePixels(width))
            {
                continue;
            }

            widths[key] = (int)Math.Round(width, MidpointRounding.AwayFromZero);
        }

        // The table has to be re-measured against the new task list width, or the bars stay where
        // the old one put them.
        Invalidate();
    }

    /// <summary>
    /// Reports a finished drag on a bar. Called from JavaScript.
    /// </summary>
    /// <param name="id">The identifier of the task that was dragged.</param>
    /// <param name="action">Which part of the bar was taken hold of.</param>
    /// <param name="slots">How far it moved, in slots.</param>
    [JSInvokable]
    public async Task OnBarDragged(string id, string action, double slots)
    {
        if (chart is null || slots == 0)
        {
            return;
        }

        var row = chart.Rows.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.Ordinal));
        if (row is null)
        {
            return;
        }

        var kind = action switch
        {
            "start" => GanttChangeKind.ResizeStart,
            "end" => GanttChangeKind.ResizeEnd,
            _ => GanttChangeKind.Move,
        };

        var from = row.OffsetSlots;
        var to = row.OffsetSlots + row.LengthSlots;

        switch (kind)
        {
            case GanttChangeKind.Move:
                from += slots;
                to += slots;
                break;
            case GanttChangeKind.ResizeStart:
                from = Math.Min(from + slots, to);
                break;
            default:
                to = Math.Max(to + slots, from);
                break;
        }

        var context = new GanttChangeContext<TItem>
        {
            Kind = kind,
            Item = row.Item,
            Row = row,
            Start = chart.Axis.At(from),
            End = chart.Axis.At(to),
            Progress = row.Progress,
        };

        if (OnTaskChange.HasDelegate)
        {
            await OnTaskChange.InvokeAsync(context);
        }

        // Redraw either way: accepted, the caller's new dates are already in Data; refused, the
        // bar has to go back to where the drag started from.
        Invalidate();
    }

    /// <summary>
    /// Reports a finished drag on a bar's fill. Called from JavaScript.
    /// </summary>
    /// <param name="id">The identifier of the task that was dragged.</param>
    /// <param name="fraction">How far along the drag is asking the task to be, from 0 to 1.</param>
    [JSInvokable]
    public async Task OnProgressDragged(string id, double fraction)
    {
        var row = chart?.Rows.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.Ordinal));
        if (row is null)
        {
            return;
        }

        if (OnTaskChange.HasDelegate)
        {
            await OnTaskChange.InvokeAsync(new GanttChangeContext<TItem>
            {
                Kind = GanttChangeKind.Progress,
                Item = row.Item,
                Row = row,
                Start = row.Start,
                End = row.End,
                Progress = Math.Clamp(fraction, 0, 1),
            });
        }

        Invalidate();
    }

    /// <summary>
    /// Reports a dependency drawn between two bars. Called from JavaScript.
    /// </summary>
    /// <param name="fromId">The identifier of the task the drag started on.</param>
    /// <param name="fromAnchor">Which end of it the drag started from.</param>
    /// <param name="toId">The identifier of the task the drag ended on.</param>
    /// <param name="toAnchor">Which end of it the drag ended on.</param>
    [JSInvokable]
    public async Task OnLinkDrawn(string fromId, string fromAnchor, string toId, string toAnchor)
    {
        var from = chart?.Rows.FirstOrDefault(r => string.Equals(r.Id, fromId, StringComparison.Ordinal));
        var to = chart?.Rows.FirstOrDefault(r => string.Equals(r.Id, toId, StringComparison.Ordinal));

        if (from is null || to is null || ReferenceEquals(from, to))
        {
            return;
        }

        var leaving = string.Equals(fromAnchor, "start", StringComparison.Ordinal);
        var arriving = string.Equals(toAnchor, "start", StringComparison.Ordinal);

        // The two ends the gesture touched are the two ends the name is made of.
        var type = (leaving, arriving) switch
        {
            (true, true) => GanttDependencyType.StartToStart,
            (true, false) => GanttDependencyType.StartToFinish,
            (false, false) => GanttDependencyType.FinishToFinish,
            _ => GanttDependencyType.FinishToStart,
        };

        if (OnDependencyCreate.HasDelegate)
        {
            await OnDependencyCreate.InvokeAsync(new GanttDependencyContext<TItem>
            {
                Dependency = new GanttDependency(from.Id, to.Id, type),
                From = from,
                To = to,
            });
        }

        Invalidate();
    }

    /// <summary>
    /// Reports a row dropped somewhere else in the task list. Called from JavaScript.
    /// </summary>
    /// <param name="id">The identifier of the task that was dragged.</param>
    /// <param name="targetId">The identifier of the task it was dropped on.</param>
    /// <param name="position">Whether it landed before, after or inside the target.</param>
    [JSInvokable]
    public async Task OnRowDropped(string id, string targetId, string position)
    {
        var row = chart?.Rows.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.Ordinal));
        var target = chart?.Rows.FirstOrDefault(r => string.Equals(r.Id, targetId, StringComparison.Ordinal));

        // A task cannot be dropped into its own branch: it would become its own ancestor, and the
        // tree it is drawn from would stop being a tree.
        if (row is null || target is null || ReferenceEquals(row, target) || Descends(target, row))
        {
            Invalidate();
            return;
        }

        var where = position switch
        {
            "before" => GanttDropPosition.Before,
            "inside" => GanttDropPosition.Inside,
            _ => GanttDropPosition.After,
        };

        if (OnTaskMove.HasDelegate)
        {
            await OnTaskMove.InvokeAsync(new GanttMoveContext<TItem>
            {
                Item = row.Item,
                Row = row,
                Target = target,
                Position = where,
                NewParentId = where == GanttDropPosition.Inside ? target.Id : target.ParentId,
            });
        }

        Invalidate();
    }

    /// <summary>Says whether one row sits somewhere under another.</summary>
    /// <param name="row">The row that might be underneath.</param>
    /// <param name="ancestor">The row that might be above it.</param>
    /// <returns><see langword="true"/> when it does.</returns>
    private bool Descends(GanttRow<TItem> row, GanttRow<TItem> ancestor)
    {
        var parent = row.ParentId;
        while (parent is { Length: > 0 })
        {
            if (string.Equals(parent, ancestor.Id, StringComparison.Ordinal))
            {
                return true;
            }

            parent = chart?.Rows.FirstOrDefault(r => string.Equals(r.Id, parent, StringComparison.Ordinal))?.ParentId;
        }

        return false;
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (CollapsedIds is not null && !ReferenceEquals(CollapsedIds, collapsed))
        {
            collapsed = new HashSet<string>(CollapsedIds, StringComparer.Ordinal);
        }

        stale = true;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (stale)
        {
            var previous = chart;
            var previousError = buildError;
            Rebuild();

            // Compared by value, not by reference. Rebuild always makes a new chart, so a
            // reference test is always "changed" — and OnBuilt calls StateHasChanged on whoever
            // handles it, whose re-render sets this component's parameters again, which marks it
            // stale again, which rebuilds again. That circle has nothing to stop it, and it hangs
            // the circuit on Server and the tab on WebAssembly.
            var chartChanged = !SameChart(previous, chart);
            if (chartChanged || !string.Equals(previousError, buildError, StringComparison.Ordinal))
            {
                if (chartChanged && OnBuilt.HasDelegate)
                {
                    await OnBuilt.InvokeAsync(chart);
                }

                // What is on screen is still the chart as it was before this rebuild — on the very
                // first pass, that is the empty message, which has no chart element in it at all.
                // Wiring up JavaScript here hands it something that is not an element. Ask for
                // another render and wire up on the pass after it, when the DOM matches the build.
                redrawPending = true;
                StateHasChanged();
                return;
            }
        }

        // The first mount needs the same full wiring a rebuild does, so it rides the same flag
        // rather than a second one. firstRender is true for exactly one pass and cannot be
        // carried forward; this can.
        if (firstRender)
        {
            redrawPending = true;
        }

        // Cleared only once the setup has actually used it. Consuming it before the call was the
        // bug: a pass that bailed out threw away the very signal the next pass needed, so the
        // column resize handles were drawn and never wired. It only showed up on charts that
        // declare columns, because a column invalidating the chart after it is built is what makes
        // a bail-out pass happen at all.
        if (await SetUpJsAsync(redrawPending))
        {
            redrawPending = false;
        }
    }

    /// <summary>
    /// Wires up the JavaScript for the chart.
    /// </summary>
    /// <param name="redrawn">Whether the chart has been drawn again since the last wiring.</param>
    /// <returns>
    /// <see langword="true"/> when the wiring ran. <see langword="false"/> where it bailed out, so
    /// the caller knows to keep the redraw signal for a pass that can use it.
    /// </returns>
    private async Task<bool> SetUpJsAsync(bool redrawn)
    {
        // Every one of these draws something other than the chart, so the element the JavaScript
        // needs is not in the document. Checking only for a built chart is what put a red banner
        // on the demo page: a chart can exist in C# while the markup is still showing a spinner,
        // an empty message or an error.
        if (chart is null || chart.IsEmpty || Loading || buildError is not null)
        {
            return false;
        }

        // And then the direct test, because everything above only infers that the element exists.
        // A column registering calls Invalidate after the chart is already built, and the pass
        // that follows can reach here before the chart branch has drawn — an interleaving the
        // state check cannot see. An ElementReference no render has assigned has a null Id, and
        // handing one of those to JavaScript is what produced "addEventListener is not a
        // function". Asking the reference itself cannot be raced.
        if (string.IsNullOrEmpty(rootElement.Id))
        {
            return false;
        }

        try
        {
            if (!jsReady)
            {
                selfRef = DotNetObjectReference.Create(this);
                ganttModule = await JsModules.GetAsync(Js, "./_content/BlazorBlueprint.Components/js/gantt.js");
                jsReady = true;
            }

            if (Interactive)
            {
                await ganttModule!.InvokeVoidAsync("initialize", rootElement, selfRef);
            }

            // Only after a redraw: on Blazor Server every call here is a circuit round trip, and
            // the handles a render did not replace are already wired up.
            if (redrawn && Resizable && columns.Count > 0)
            {
                columnsModule ??= await JsModules.GetAsync(Js, "./_content/BlazorBlueprint.Components/js/table-columns.js");
                await columnsModule.InvokeVoidAsync("initColumnResize", rootElement, selfRef, chartId, MinColumnWidth);
                await columnsModule.InvokeVoidAsync("setupResizeHandles", chartId);
            }

            if (ScrollToToday && !scrolled)
            {
                scrolled = true;
                await GoToTodayAsync();
            }

            return true;
        }
        catch (Exception ex) when (ex is JSDisconnectedException or TaskCanceledException or ObjectDisposedException)
        {
            // The circuit went away mid-render; there is nothing left to wire up.
        }
        catch (JSException)
        {
            // A module that failed to load, or a call the browser refused. Catching it is what
            // keeps a wiring problem from becoming a red banner over a chart that has drawn
            // perfectly well: the gestures are degraded, the plan is still readable.
        }

        // Reported as not run, so a later pass tries again rather than leaving the chart with
        // handles nobody wired.
        return false;
    }

    /// <summary>
    /// Gets whether two builds draw the same chart.
    /// </summary>
    /// <param name="left">The chart built last time, which may be null.</param>
    /// <param name="right">The chart just built, which may be null.</param>
    /// <returns><see langword="true"/> when nothing a reader or a handler could notice differs.</returns>
    /// <remarks>
    /// <para>
    /// This walks the rows, which is the same order of work the rebuild that produced them already
    /// did, so it costs one more pass over something already in cache rather than a second build.
    /// It stops at the first difference.
    /// </para>
    /// <para>
    /// Compared exactly rather than through a hash: a hash would be the same cost with a small
    /// chance of calling two different charts the same, and a missed <c>OnBuilt</c> is a silent
    /// wrong answer where an extra one is only wasted work.
    /// </para>
    /// <para>
    /// The source items are deliberately not compared. A change to a field the chart does not draw
    /// is not a change to the chart, and the columns re-render from the caller's own markup anyway.
    /// </para>
    /// </remarks>
    private static bool SameChart(GanttChart<TItem>? left, GanttChart<TItem>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        if (left.TaskCount != right.TaskCount
            || left.Rows.Count != right.Rows.Count
            || left.Links.Count != right.Links.Count)
        {
            return false;
        }

        if (left.Axis.Zoom != right.Axis.Zoom
            || left.Axis.Start != right.Axis.Start
            || left.Axis.End != right.Axis.End
            || !left.Axis.Minor.SequenceEqual(right.Axis.Minor)
            || !left.Axis.Major.SequenceEqual(right.Axis.Major))
        {
            return false;
        }

        for (var i = 0; i < left.Rows.Count; i++)
        {
            if (!SameRow(left.Rows[i], right.Rows[i]))
            {
                return false;
            }
        }

        for (var i = 0; i < left.Links.Count; i++)
        {
            // GanttLink is a record, so this compares every end and position it carries.
            if (left.Links[i] != right.Links[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool SameRow(GanttRow<TItem> left, GanttRow<TItem> right) =>
        string.Equals(left.Id, right.Id, StringComparison.Ordinal)
        && string.Equals(left.Text, right.Text, StringComparison.Ordinal)
        && left.Start == right.Start
        && left.End == right.End
        && left.Progress.Equals(right.Progress)
        && left.Depth == right.Depth
        && left.ChildCount == right.ChildCount
        && left.IsSummary == right.IsSummary
        && left.IsMilestone == right.IsMilestone
        && left.IsExpanded == right.IsExpanded
        && left.OffsetSlots.Equals(right.OffsetSlots)
        && left.LengthSlots.Equals(right.LengthSlots);

    private void Rebuild()
    {
        stale = false;
        buildError = null;

        if (Data is null || IdSelector is null || TextSelector is null || StartSelector is null || EndSelector is null)
        {
            chart = null;
            return;
        }

        if (!seeded && !DefaultExpandAll && CollapsedIds is null)
        {
            // Worked out from the parent identifiers rather than from a built chart, so the first
            // render is already folded up instead of opening and then snapping shut.
            collapsed = Summaries();
            seeded = true;
        }

        try
        {
            chart = GanttBuilder.Build(new GanttSource<TItem>
            {
                Items = Data,
                Id = IdSelector,
                ParentId = ParentIdSelector,
                Text = TextSelector,
                Start = StartSelector,
                End = EndSelector,
                Progress = ProgressSelector,
                Order = SortComparer(),
                Dependencies = ShowDependencies ? Dependencies : null,
                Zoom = Zoom,
                Snap = RangeSnap,
                RangeStart = RangeStart,
                RangeEnd = RangeEnd,
                NonWorkingDays = ShowNonWorkingDays ? NonWorkingDays : [],
                Labels = new GanttLabels
                {
                    WeekFormat = Localizer["Gantt.Week"],
                    QuarterFormat = Localizer["Gantt.Quarter"],
                },
                Collapsed = collapsed,
                RollUpSummaries = RollUpSummaries,
            });
        }
        catch (ArgumentException error)
        {
            // A plan that cannot be drawn is a declaration problem, not a bug. Say so in the markup
            // rather than throwing, which on Blazor Server would take the circuit down.
            chart = null;
            buildError = error.Message;
        }
    }

    private HashSet<string> Summaries()
    {
        var parents = new HashSet<string>(StringComparer.Ordinal);
        if (Data is null || ParentIdSelector is null)
        {
            return parents;
        }

        foreach (var item in Data)
        {
            if (ParentIdSelector(item) is { Length: > 0 } parent)
            {
                parents.Add(parent);
            }
        }

        return parents;
    }

    private Comparer<TItem>? SortComparer()
    {
        if (sortKey is null)
        {
            return null;
        }

        var column = columns.FirstOrDefault(c => string.Equals(c.EffectiveKey, sortKey, StringComparison.Ordinal));
        if (column is null)
        {
            return null;
        }

        var read = column.Value ?? (item => TextSelector(item));
        var inner = column.Comparer ?? Comparer<object?>.Default;
        var sign = sortDescending ? -1 : 1;

        return Comparer<TItem>.Create((a, b) => sign * inner.Compare(read(a), read(b)));
    }

    private async Task SortByAsync(BbGanttColumn<TItem> column)
    {
        if (string.Equals(sortKey, column.EffectiveKey, StringComparison.Ordinal))
        {
            // Ascending, then descending, then back to the order the tasks arrived in.
            if (!sortDescending)
            {
                sortDescending = true;
            }
            else
            {
                sortKey = null;
                sortDescending = false;
            }
        }
        else
        {
            sortKey = column.EffectiveKey;
            sortDescending = false;
        }

        Invalidate();
        await Task.CompletedTask;
    }

    private async Task NotifyCollapsedAsync()
    {
        Invalidate();

        if (CollapsedIdsChanged.HasDelegate)
        {
            await CollapsedIdsChanged.InvokeAsync(new HashSet<string>(collapsed, StringComparer.Ordinal));
        }
    }

    private async Task ClickAsync(GanttRow<TItem> row)
    {
        if (OnTaskClick.HasDelegate)
        {
            await OnTaskClick.InvokeAsync(row.Item);
        }
    }

    private async Task ClickLinkAsync(GanttLink link)
    {
        if (OnDependencyClick.HasDelegate)
        {
            await OnDependencyClick.InvokeAsync(link.Dependency);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the JavaScript handlers this chart set up.</summary>
    /// <returns>A task that completes when the handlers are gone.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        try
        {
            if (ganttModule is not null && jsReady)
            {
                await ganttModule.InvokeVoidAsync("dispose", rootElement);
            }
        }
        catch (JSDisconnectedException)
        {
            // The circuit went away first, which already took the handlers with it.
        }
        catch (ObjectDisposedException)
        {
            // Same again, on a runtime that reports it differently.
        }

        selfRef?.Dispose();
        selfRef = null;
    }
}
