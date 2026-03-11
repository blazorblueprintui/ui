## What's New in v3.5.0

### New Components

- **AlertDialog** — Modal dialog with `role="alertdialog"` that prevents dismiss via Escape or overlay click, ideal for destructive confirmations
- **ContextMenu** — Right-click triggered menu with `role="menu"`, full keyboard navigation via arrow keys, and positioning support
- **Progress** — Determinate and indeterminate progress bar with `role="progressbar"` and `aria-valuenow`/`aria-valuemin`/`aria-valuemax`
- **Separator** — Accessible separator with `role="separator"` or `role="none"` (decorative mode), horizontal and vertical orientation
- **Slider** — Range input with `role="slider"`, keyboard navigation (arrows, Home, End, PageUp, PageDown), and JS-powered drag interaction
- **Toggle** — Accessible toggle button with `aria-pressed`, supporting controlled and uncontrolled state
- **ToggleGroup** — Single or multiple selection toggle group with roving tabindex and keyboard navigation

### New Features

- **Localization** — New `IBbLocalizer` interface with `DefaultBbLocalizer` implementation, enabling `IStringLocalizer` integration for all component chrome strings
- **DataGrid hierarchical tree** — `HierarchyManager` utility for nested, self-referencing, and lazy-loaded tree data with expand/collapse, per-level sorting, child pagination, and filter-aware hierarchy display
- **DataGrid cascading hierarchy selection** — `HierarchySelectionMode.Cascade` for parent-child checkbox cascading with indeterminate state
- **DataGrid hierarchy filter modes** — `HierarchyFilterMode` enum (`ShowMatchedSubtree` / `ShowMatchedOnly`) to control how filtering treats descendants
- **Filtering: finer time units** — Added `Hours`, `Minutes`, and `Seconds` to `InLastPeriod` enum for finer-grained date/time filtering
- **BbPrimitiveBase** — Shared base class with `MergeStyles` and `FilteredAttributes` helpers to reduce duplication across primitives
- **BbTablePagination** — Added string parameters for aria labels to support localization
- **Checkbox** — Added `Required` parameter with `aria-required` support
- **RadioGroup** — Added `Required` parameter with `aria-required` support
- **Popover** — Added configurable `Role` parameter (changed default from `group` to `dialog`)
- **HoverCard trigger** — Added `tabindex="0"` for keyboard focusability

### Bug Fixes

- **DataGrid** — Respect initial `Visible` property on columns so `Visible=false` columns are hidden on first render
- **DataGrid** — Fix column filtering for nullable types; add `DateOnly` and `DateTimeOffset` support in filter expressions
- **DataGrid** — Auto-generate enum filter dropdown options when `FilterOptions` is not explicitly provided
- **DataGrid** — Keep table header visible when filters produce empty results so users can clear filters
- **DataGrid** — Yield keyboard navigation to interactive children (Combobox, MultiSelect) inside cell templates
- **Select** — Fix displaying value instead of text on initial load when using `DisplayTextSelector`
- **Checkbox** — Fix spacebar toggle regression caused by synthetic click double-toggle
- **ContextMenu** — Fix re-open at new position via z-index layering on trigger
- **ContextMenu** — Fix keyboard navigation on re-open by re-initializing keyboard module
- **Switch** — Remove non-standard Enter key handling (Space only per WAI-ARIA)

### Improvements

- **Popover** — Changed `role="group"` to `role="dialog"` for correct ARIA semantics
- **DropdownMenu** — Added `aria-labelledby` pointing to trigger on menu content
- **Tabs** — Filter arrow key navigation by orientation (horizontal: Left/Right, vertical: Up/Down)
- **Select trigger** — Replace blanket `preventDefault` with conditional handling
- **JS interop resilience** — Added `JSException` catch filters across all overlay components for graceful offline/disconnect handling
- **Floating portal** — Silently handle `JSException` in positioning and setup
- **Keyboard shortcuts JS** — Added null check for `event.target`
- **Click-outside JS** — Fixed `setTimeout` race condition in cleanup
- **Select JS** — Added bounds validation in `scrollIntoContainerView`
- **Positioning JS** — Removed unnecessary `!important` on top/left styles
- **Tree keyboard JS** — Improved interactive child detection and error handling
- **Async void elimination** — Replaced `async void` handlers with `async Task` in `BbPopoverContent`, `BbDropdownMenuContent`, `BbSelectContent`, and `BbTreeView`
- **BbSelectContent** — Fixed `DotNetObjectReference` overwrite using `??=` and added disposed guard to `JSInvokable` methods
