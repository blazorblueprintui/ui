## What's New in v3.4.0

### New Components

- **DashboardGrid** — Headless dashboard grid primitive (`BbDashboardGrid`, `BbDashboardWidget`) with drag, resize, responsive breakpoints (Large/Medium/Small), layout persistence via save/restore snapshots, and compact mode support

### New Features

- **DataGrid** — Row grouping support with `GroupDefinition`, `DataGridGroupState`, and collapsible group headers
- **DataGrid** — Aggregate functions (Count, Sum, Average, Min, Max) on grouped columns via `AggregateFunction` and `AggregateResult`
- **DataGrid** — Server-side grouped data provider via `DataGridGroupedItemsProvider` delegate
- **DataGrid** — `GetRawValue` method on `IDataGridColumn` for unformatted values used in aggregate computation
- **DataGrid** — `Aggregate` and `AggregateFormat` properties on `IDataGridColumn`
- **DataGrid** — `GroupDefinition` and `AggregateColumns` properties on `DataGridRequest` for server-side grouping
- **DataGrid** — Grouping state included in `DataGridStateSnapshot` for persistence

### Bug Fixes

- **Collapsible** — Allow parent re-renders to cascade through correctly by tracking parameter changes in `ShouldRender`
- **DataGrid** — Allow parent re-renders to cascade through correctly by tracking parameter changes in `ShouldRender`
- **Table** — Allow parent re-renders to cascade through correctly by tracking parameter changes in `ShouldRender`
- **DataGrid** — Simplified focus ring classes on data grid rows by removing unnecessary positioning styles
- **DashboardGrid** — Fixed widget position updates during compact mode
