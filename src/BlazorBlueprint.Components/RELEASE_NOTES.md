## What's New in v3.4.0

### New Components

- **DashboardGrid** — Drag-and-drop, resizable widget grid layout with CSS Grid, collision detection, responsive breakpoints, state persistence, keyboard navigation, ARIA accessibility, and configurable grid guide backgrounds
- **BbDashboardWidget** — Dashboard widget with header, content areas, remove button, and loading skeleton support
- **BbDataGridGroupColumn** — Declarative column component for configuring DataGrid row grouping

### New Features

- **DataGrid** — Row grouping with collapsible group headers, aggregate functions (Sum, Average, Min, Max, Count), and custom header templates via `GroupBy` parameter or `BbDataGridGroupColumn`
- **DataGrid** — Server-side grouping support via `GroupedItemsProvider` delegate
- **DataGrid** — `ExpandAllGroups()` and `CollapseAllGroups()` public methods for toolbar integration
- **DataGrid** — `GroupsCollapsedByDefault` parameter to control initial group state
- **DataGrid** — Pin icon with tooltip on pinned column headers indicating the column is pinned and cannot be moved
- **DataGrid** — Column visibility dropdown now disables pinned columns and prevents hiding the last visible column
- **Tabs** — Responsive mode via `Responsive` parameter on `BbTabsList` that collapses to a `BbSelect` dropdown when tabs overflow their container
- **BbTabsTrigger** — `Label` parameter for display text in the responsive Select fallback
- **DashboardGrid** — `CellWidth`, `MinWidth`, `MaxWidth` parameters for fixed-size grid cells with scroll wrapper
- **DashboardGrid** — Built-in loading skeleton, empty state, and `GetConfig()` method for persistence

### Bug Fixes

- **Slider / RangeSlider** — Fixed style rendering broken by cultures using `,` as decimal separator by using invariant culture for percentage values
- **DataGrid** — Fixed cell border rendering broken by `position: relative` on table rows (Chromium border-collapse bug)
- **DataGrid** — Fixed empty state not rendering when grouping is active
- **DashboardGrid** — Fixed widget position updates not applying during compact mode
- **DashboardGrid** — Fixed stale layout after widget removal and widgets not repositioning until user interaction
- **DashboardGrid** — Fixed resize min-span defaults exceeding widget's declared span
- **DashboardGrid** — Added collision detection in freeform (non-compact) mode
- **Container components** — Fixed parent re-renders not cascading through `BbDataTable`, `BbDataGrid`, `BbDataView`, `BbTable`, and `BbCollapsible` when child components trigger `ValueChanged`

### Improvements

- **DataGrid** — Aggregate values render aligned under their respective column headers with independent `AggregateFormat` parameter
- **DataGrid** — Pagination counts data rows only; group headers don't consume page slots
- **DashboardGrid** — Keyboard accessibility with focus-visible ring, keyboard resize, and compact-on-blur
- **DashboardGrid** — Responsive widget stacking that clamps column spans to fit active breakpoint
- **Primitives** — Bumped BlazorBlueprint.Primitives dependency to 3.4.0
