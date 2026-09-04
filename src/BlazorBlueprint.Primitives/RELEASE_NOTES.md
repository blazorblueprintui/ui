## What's New in v3.16.0

### New Features
- **DataGrid** — `IDataGridColumn` gains `GetSortAndFilterValue` and `GetSortAndFilterExpression`, so a column can sort, filter, search, group and export on a projected value rather than the one it displays. Both have default implementations, so existing column types are unaffected.
- **DataGrid** — new `DataGridSelectionBehavior` with `Toggle` (the default) and `Replace`; `Replace` gives plain-click, Shift+Click range and Ctrl/Cmd+Click semantics. Exposed as `SelectionBehavior` on `BbDataGrid`, `BbDataGridRow` and `DataGridContext`.
- **SelectionState** — new `Anchor`, `ReplaceWith` and `SelectRange`, with `DataGridContext.ApplyRowSelectionInput` and the `RowSelectionModifiers` struct resolving a click into a selection change.
- **DataGrid** — new `DataGridCsvWriter` writes the current rows to CSV in visible column order, using each column's display text. A cell starting `=`, `+`, `-`, `@`, tab or CR is escaped so a spreadsheet does not run it as a formula.
- **DataGrid** — grouping now nests to any depth. `DataGridGroupState` gains `ActiveGroups`, `Depth`, `SetGroups`, `AddGroup`, `RemoveGroup`, `MoveGroup`, `GetLevel`, `IsGroupedBy`, `CollapsedPaths` and `IsHiddenByCollapse`; the single-level members are kept and still work.
- **GroupPath** — new type keying collapsed state by the full ancestor chain, because a raw key is ambiguous once grouping nests.
- **DataGridGroupRow** — new `Path`, `Depth` and `Children`; `Items` now carries every row beneath the group so aggregates roll up.
- **DataGrid** — new `DataGridEditMode` and row editing hooks `IsRowEditing`, `OnCommitEdit` and `OnCancelEdit` on `BbDataGrid` and `BbDataGridRow`, plus `IDataGridColumn.EditTemplate`.
- **DataGridRowSnapshot** — new type restoring a row's original values in place when an edit is cancelled, so anything else holding that row sees the revert.
- **DataGridRenderItem** — new `ForGroupedData(item, depth)` carries the indent depth for a row nested under group headers.
- **DataGridGroupedResult**, **DataGridStateSnapshot** — new `GroupDefinitions` carries the full ordered level list for server-side grouping and state persistence.
- **Filtering** — new `FilterCondition.MatchesValue` and `ToExpressionForSelector`, so a filter can run against any expression instead of only a property resolved by name.
- **AlertDialog**, **AlertDialogPortal**, **TablePagination** — added `AdditionalAttributes`, so unmatched attributes splat onto the rendered element.

### Bug Fixes
- **Popover**, **DropdownMenu**, **HoverCard**, **Dialog**, **Sheet** — the subtree now repaints whenever context open state moves, not only when the consumer agrees. A consumer overriding `ShouldRender` could previously leave the comparison stale, after which `OpenChanged` silently stopped firing for good.
- **HoverCardContent**, **TooltipContent** — `data-state` is bound to the context instead of hardcoded to `open`, so the closed-state exit animation classes can match.
- **FloatingPortal** — the element is held positioned and visible between close and the end of the exit animation, so the animation is seen rather than hidden away immediately. Pointer events stop at once, a reopen cancels the pending hide, and a one-second cap stops a stalled animation stranding an overlay on screen.
- **DataGrid** — a sort key selector declared as `Expression<Func<TData, object>>` has its boxing `Convert` node stripped, so value types order correctly in memory and the sort is still translatable by an `IQueryable` provider.
- **DataGrid** — Enter and Escape now reach Blazor from an input inside a row being edited; the row's keydown interceptor previously blocked every handler in the row.
- **DataGrid** — the focused field is blurred before an Enter commit, so the value just typed is no longer dropped.

### Improvements
- **PortalService** — the missing-host warning now says a host is not registered in this render context, separates a genuinely missing host from a wrong-context one, and names both fixes. It no longer links to an unpublished guide page.
- **FloatingPortal** — the per-portal host timeout is logged only when a host is registered, where it means a slow render rather than a missing host. A page of tooltips produced 24 near-identical warnings and now produces one.
