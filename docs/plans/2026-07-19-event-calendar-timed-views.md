---
title: "Timed Day & multi-day views for BbEventCalendar"
date: 2026-07-19
branch: feature/417-event-calendar-timed-views
status: draft
tags: [event-calendar, component, feature]
estimated_tasks: 8
priority: medium
epic: EventCalendar
depends-on: []
tracking-issues: [417]
references:
  - https://github.com/blazorblueprintui/ui/issues/417
  - https://github.com/blazorblueprintui/ui/issues/420
  - https://qcalendar.netlify.app/examples/day/date-type
  - https://qcalendar.netlify.app/examples/day/_3-day
  - https://blazor.radzen.com/scheduler
  - https://mudcalendar.heron.li/calendar#day/week-view-options
---

# Timed Day & multi-day views for `BbEventCalendar`

Tracking issue: [#417](https://github.com/blazorblueprintui/ui/issues/417).
Follow-up (out of scope here): [#420 — resource-grouped columns](https://github.com/blazorblueprintui/ui/issues/420).

## Summary

Add a **timed grid** rendering mode to `BbEventCalendar<TEvent>`: a new
`EventCalendarView.Day` that shows `VisibleDayCount` consecutive days with an hour
axis, timed events positioned by their start/end time, concurrent events packed
side-by-side into columns, and an all-day strip on top. The change is purely
**additive** — Month, Week, and Agenda are untouched.

Today the component has **no time-of-day grid at all**. Its `Week` view is a
per-day list of chips in start order, not a scheduler grid. This plan introduces
the first timed layout in the component.

## Scope

**In scope**

- `EventCalendarView.Day` — a timed grid showing `VisibleDayCount` days.
- N-day via `VisibleDayCount` (no extra enum members — locked decision 1).
- Configurable hour window (`DayStartHour`/`DayEndHour`), slot granularity
  (`SlotDurationMinutes`) and vertical scale (`HourHeight`).
- Overlap / column-packing layout for concurrent timed events.
- All-day / multi-day-spanning strip at the top.
- Prev/next navigation stepping by `VisibleDayCount` days; Today; header title.
- `OnSlotClick(DateTime)` — click an empty slot to create.
- Localization strings, accessibility on positioned event buttons.
- Demo (live example + `CodeExamples` snippet + API-Reference entries) and the
  Components API-surface snapshot update.

**Out of scope**

- Resource-grouped columns in day view → follow-up **#420** (do not start until
  this lands).
- Converting `Week` to a timed grid (locked decision 2 — Week stays a chip list;
  a timed week could be a later opt-in).
- Event drag/resize/move, live-ticking current-time line, auto scroll-to-hour,
  and event virtualization.

## Locked decisions (from maintainer)

1. Multi-day is modeled as a single `EventCalendarView.Day` + a `VisibleDayCount`
   parameter — **not** many enum members.
2. The existing `Week` view stays exactly as-is. This feature is additive.
3. Resource grouping is deferred to a separate issue (**#420**).

Confirmed during planning: `VisibleDayCount` **defaults to 1** (a view named
"Day" shows one day; consumers set 3 for the requester's 3-day case). Plan-doc
lives on a feature branch, not committed to `develop` directly.

## Approach / architecture

### View model & switcher

Add one enum member `Day` to `EventCalendarView`. The built-in view switcher gains
a single **Day** button. N-day is achieved by setting `VisibleDayCount` (e.g. 3);
a consumer wanting a dedicated "3 days" control drives `View` + `VisibleDayCount`
themselves. No `Days`/`ThreeDay` enum members.

### Rendering (no JavaScript)

A fixed-height, vertically-scrolling region composed of a left **hour-axis gutter**
plus `VisibleDayCount` **day columns**:

```
┌──────────────────────────────────────────────┐
│ (gutter)  Mon 14   Tue 15   Wed 16            │  ← day headers (click → OnDateClick)
├──────────────────────────────────────────────┤
│ all-day │  [chip]           [chip]            │  ← all-day strip (ShowAllDaySection)
├─────────┼──────────┬──────────┬───────────────┤
│  08:00  │          │  ┌────┐  │               │
│  09:00  │  ┌────┐  │  │ ev │  │   ┌────┐      │  ← scrollable timed body
│  10:00  │  │ ev │  │  └────┘  │   │ ev │      │
│   …     │  └────┘  │          │   └────┘      │
└─────────┴──────────┴──────────┴───────────────┘
```

- Each day-column body is `position: relative`, height
  `= (DayEndHour − DayStartHour) × HourHeight` px.
- Hour lines are CSS borders (a background layer), drawn every hour;
  optional finer lines at each `SlotDurationMinutes`.
- Each **timed event** is an absolutely-positioned `<button>`:
  - `top = (startMinutesFromDayStart) × pxPerMinute`
  - `height = max(minHeight, durationMinutes × pxPerMinute)`
  - `left` / `width` from the overlap column assignment (below).
  - Events are clamped to the visible window (an event starting before
    `DayStartHour` clips at the top; ending after `DayEndHour` clips at the
    bottom). Events entirely outside the window are dropped from the timed body
    (they still appear in the all-day strip only if they qualify as all-day).
- A background layer of **empty slot buttons** (one per `SlotDurationMinutes`
  per day) sits *beneath* the event buttons and fires `OnSlotClick(day + slotTime)`.
  Event buttons have a higher stacking order so an event click wins over a slot
  click.

`pxPerMinute = HourHeight / 60`.

### Overlap / column-packing algorithm (the core)

Per day column, over that day's timed events sorted by (start, then end desc):

1. Walk events accumulating a **cluster** while the next event starts before the
   running maximum end of the cluster (transitive overlap). When an event starts
   at/after the cluster's max end, close the cluster and start a new one.
2. Within a cluster, greedily assign each event to the **first column** (track)
   whose last-assigned event ends `<=` this event's start; otherwise open a new
   column. `columnCount = number of columns opened in the cluster`.
3. Emit `(columnIndex, columnCount)` per event →
   `left = columnIndex / columnCount`, `width = 1 / columnCount` (as `%`).

Produces a `readonly record struct TimedEventLayout(TEvent Event, double TopPx,
double HeightPx, double LeftPercent, double WidthPercent)` list per day, computed
in the code-behind. Deterministic and O(n log n) per day.

*Optional refinement (noted, not required for v1):* expand an event's width to
absorb trailing free columns it doesn't actually collide with (the Google-Calendar
look). Left out of v1 to keep the algorithm simple; equal-width columns are
correct and readable.

### All-day / multi-day strip (v1)

All-day events (`IsAllDay`) and any event spanning more than one calendar day
render as **per-day chips** in an all-day cell above each visible day column —
reusing the existing chip rendering and the component's existing per-day
expansion of multi-day events. Toggle with `ShowAllDaySection`.

*Future enhancement (noted):* horizontal spanning bars across the day columns
(like the month view). Deferred to keep v1's hard work confined to timed layout.

### Navigation, range, title

- `GetVisibleRange()` → `Day`: `[CurrentDate.Date, CurrentDate.Date.AddDays(VisibleDayCount − 1)]`.
- Prev/next: step by `VisibleDayCount` days in Day view (existing month/week
  stepping unchanged). Today: `CurrentDate = DateTime.Today`.
- `HeaderTitle`: `VisibleDayCount == 1` → full localized date
  (`"Wednesday, 16 July 2026"`); otherwise `"16 – 18 Jul 2026"` style range.
- `RebuildEventLookup` already buckets by day across the visible range — reused
  as-is; the timed body reads each day's list and positions by time.

### Current-time indicator

A static horizontal marker at `top = (now − dayStart) × pxPerMinute` drawn only in
the column matching `DateTime.Today` and only when `now` is within
`[DayStartHour, DayEndHour]`. Controlled by `ShowCurrentTimeIndicator`. It does
not auto-tick in v1 (no timer); live updates are a future enhancement.

### New parameters

| Parameter | Type | Default | Notes |
|---|---|---|---|
| `VisibleDayCount` | `int` | `1` | Days shown in Day view; clamped `>= 1` (sane upper clamp, e.g. 14). |
| `DayStartHour` | `int` | `0` | First hour shown; clamped `0..23`. |
| `DayEndHour` | `int` | `24` | Last hour boundary; clamped `DayStartHour+1..24`. |
| `SlotDurationMinutes` | `int` | `60` | Gridline + slot-click granularity; validated to a divisor of 60 (15/20/30/60). |
| `HourHeight` | `int` | `48` | Pixel height of one hour; clamped to a sensible min. |
| `ShowCurrentTimeIndicator` | `bool` | `true` | Now-line in today's column. |
| `ShowAllDaySection` | `bool` | `true` | Show the all-day strip. |
| `OnSlotClick` | `EventCallback<DateTime>` | — | Fires with the clicked slot's date+time. |

Defaults mean the Day view works with zero configuration (full 0–24 day,
hour slots). A tall 24 h column simply scrolls; consumers narrow the window with
`DayStartHour`/`DayEndHour`.

### Accessibility & localization

- Timed event `<button>`: `aria-label` = `"{title}, {start}–{end}"`; `title`
  attribute for hover.
- Day-column headers are labeled buttons (as Week today) → `OnDateClick`.
- Empty **slot** buttons: `aria-label` = `EventCalendar.CreateEventAt` with the
  time; keep them out of the primary tab sequence (`tabindex="-1"`, pointer
  activation) so they don't flood keyboard navigation, while event chips remain
  fully focusable. (Revisit if a keyboard create-flow is wanted later.)
- New strings in `DefaultBbLocalizer.cs`: `EventCalendar.Day` = "Day",
  `EventCalendar.Now` = "Now", `EventCalendar.CreateEventAt` = "Create event at {0}".
  (`EventCalendar.AllDay` already exists.)

## Plan tasks

1. **`EventCalendarView.cs`** — add `Day` member with xmldoc.
2. **`BbEventCalendar.razor.cs`** — new parameters + validation/clamping;
   `GetVisibleRange`, navigation stepping, and `HeaderTitle` for `Day`; split of
   timed vs all-day events for the visible days; the **overlap column-packing
   algorithm** and `TimedEventLayout`; slot-time / position math helpers;
   `OnSlotClick` handler.
3. **`BbEventCalendar.razor`** — `Day` render branch (day headers, all-day strip,
   scrollable hour grid + gridlines, empty-slot layer, absolutely-positioned
   event buttons, current-time line); add the **Day** button to the view switcher.
4. **`DefaultBbLocalizer.cs`** — add `EventCalendar.Day`, `.Now`, `.CreateEventAt`.
5. **`EventCalendarDemo.razor`** — new "Day & multi-day (timed)" section: a live
   1-day view and a 3-day view (`VisibleDayCount="3"`) with overlapping timed
   sample events, an `OnSlotClick` handler demo; add timed sample events to the
   page's data; update the Views blurb and Notes/Limitations; add API-Reference
   entries for every new parameter.
6. **`CodeExamples/Components/EventCalendar/day-view.txt`** (and
   `multi-day.txt`) — snippet(s) referenced by the demo `CodeBlock`s.
7. **API surface** — run `./scripts/run-tests.sh`, review the `.received.txt`
   diff (new params + `Day` enum member), accept via `./scripts/run-tests.sh
   --accept`.
8. **Build + demo-driven verification (Server and WASM)** — 1-day and 3-day
   render; two overlapping events split into side-by-side columns; a 3-way
   overlap splits into thirds; all-day + multi-day-spanning events land in the
   strip; prev/next steps by N days and Today returns; `OnSlotClick` reports the
   correct date+time for a clicked slot; current-time line shows in today's
   column; `FirstDayOfWeek`/culture and RTL sanity; localized switcher label.

## Verification

No component behavior tests exist for `BbEventCalendar` today (only the
API-surface snapshot), so verification is **demo-driven** in both render modes
plus the accepted snapshot. Drive the new demo section with Playwright/browser:
assert column splitting for overlaps (compare rendered `left`/`width`), the
all-day strip contents, navigation range, and that `OnSlotClick` surfaces the
expected `DateTime`. Full solution must build clean (`TreatWarningsAsErrors`) and
all snapshot tests pass.

## Risks / notes

- **Overlap-packing correctness** is the main risk — cover single, pairwise,
  and 3+ concurrency plus adjacent-but-not-overlapping (shared boundary) in the
  demo checks.
- **Tall default column** (0–24 h): acceptable (scrolls); consumers trim via
  `DayStartHour`/`DayEndHour`. Scroll-to-hour deferred (needs JS).
- **Timezone**: uses `DateTime` values as provided by the accessors, consistent
  with the rest of the component (no conversion).
- **Parameter surface growth**: eight new parameters, but all optional with
  working defaults; documented in the API reference.
