## What's New in v3.16.0

### New Features
- **BbDataGrid** — new `SelectionBehavior` with `DataGridSelectionBehavior.Replace` for file-explorer selection: plain click selects one row, Shift+Click selects a range from the anchor, Ctrl/Cmd+Click adds or removes one. `Toggle` remains the default.
- **BbDataGrid** — new `ShowExport`, `ExportFileName`, `ExportDelimiter`, `ExportScope`, `OnExport`, plus `ExportToCsvAsync()` and `BuildCsv()`, export the searched, filtered and sorted rows across every page to CSV. Cells that a spreadsheet would run as a formula are escaped.
- **BbDataGrid** — grouping now nests to any depth, with collapsed state keyed by group path, aggregates rolling up through every level, and open ancestor headers re-emitted when a page boundary splits a subtree. New `ShowGroupingBreadcrumb` lists the levels with controls to drop one or move it outward.
- **BbDataGrid** — new `EditMode` with `DataGridEditMode.Row` puts a whole row into edit through each column's `EditTemplate`, committing or discarding together. New `BbDataGridEditColumn` renders Edit, Save and Cancel; `EditOnRowClick` and `OnRowCommit` control entry and commit, and a refused commit keeps the row open with the user's input intact.
- **BbDataGridPropertyColumn** — new `SortAndFilterBy` projects the value the column sorts, filters, searches, groups and exports on, so a column can act on a value it does not display. It is an `Expression`, so a server-side `ItemsProvider` over `IQueryable` can still translate it.
- **BbResizablePanelGroup** — new `OnResizeEnd` reports the final panel sizes as percentages in declared order, already clamped to each panel's `MinSize`/`MaxSize`. It fires once per drag, and only when something moved.
- **AdditionalAttributes** — most components now capture unmatched attributes and splat them onto their rendered element, including **BbDock**. This is additive; no existing parameter changed.

### Bug Fixes
- **BbResizablePanel** — a fast drag now pins to `MinSize`/`MaxSize` instead of freezing short of the limit.
- **BbScrollArea** — both scrollbars are positioned out of flow, so the root's `overflow: hidden` no longer clips them away. `ScrollAreaType.Always` is affected as much as `Hover`.
- **ThemeService** — `ThemeOptions.PersistToLocalStorage` now gates reading as well as writing, so a stored theme no longer overrides the configured `Default*` values. Initializing with persistence off also clears any entry an earlier run left behind.
- **BbCalendar** — the year select is wide enough for a four-digit year, so the selected year is no longer truncated. Affects **BbDatePicker** and **BbDateTimePicker**.
- **BbDataGrid** — opening a filter or header menu bumps the state version, so the grid re-renders and the overlay's controlled open value stays correct.
- **BbDataGrid** — Enter and Escape reach the editor inside a row being edited instead of being blocked by the row's keydown interceptor, and committing on Enter no longer drops the value just typed into the focused field.
- **BbSidebar** — unmatched attributes are forwarded to `BbSheetContent`, so they reach a real element in mobile mode instead of vanishing.
- **BbFileUpload** — unmatched attributes continue to land on the inner `<input type="file">`, so `name` and `aria-*` reach the element that holds the files.
- **BbFormFieldDateTimePicker** — the inherited `AdditionalAttributes` are now applied.

### Performance
- **BbResizablePanelGroup** — the drag is driven from JavaScript, which writes `flex-basis` directly and calls into .NET once on pointer release instead of on every `pointermove`. A 40-move drag makes 1 interop call where it made 40. This matters most on Blazor Server, where queued moves compounded the lag across a drag.

### Improvements
- **Localization** — new `DataGrid.*` keys for CSV export, row editing and nested grouping.
- Bumped the `BlazorBlueprint.Primitives` dependency to 3.16.0.
